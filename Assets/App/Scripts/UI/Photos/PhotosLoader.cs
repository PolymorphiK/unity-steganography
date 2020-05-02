using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class PhotosLoader : MonoBehaviour {
	public RectTransform content;
	public PhotoView template;

	private PhotosView.PhotoResult result;
	private List<string> cache = new List<string>();

	public bool IsLoading {
		get;
		private set;
	}

	public virtual void LoadPhotos(PhotosView.PhotoResult result) {
		if(this.IsLoading) return;

		this.IsLoading = true;
		this.gameObject.SetActive(true);

		this.result = result;

		uThread.RunCoroutine(this._GetPhotos(), () => {
			this.IsLoading = false;
		});
	}

	IEnumerator _GetPhotos() {
		var endpoints = new List<string>(this.result.images);

		for(int i = 0; i < this.cache.Count; ++i) {
			endpoints.Remove(this.cache[i]);
		}

		var photoViews = new List<PhotoView>();

		for(int i = 0; i < endpoints.Count; ++i) {
			var clone = GameObject.Instantiate(this.template.gameObject);
			clone.transform.SetParent(this.content);
			clone.transform.localScale = Vector3.one;
			clone.transform.localRotation = Quaternion.identity;

			var photoView = clone.GetComponent<PhotoView>();

			photoViews.Add(photoView);

			clone.SetActive(true);

			this.cache.Add(endpoints[i]);
		}

		int batch = 4;
		UnityWebRequestAsyncOperation[] operations = new UnityWebRequestAsyncOperation[batch];

		int iterations = endpoints.Count / batch;

		for(int i = 0; i < iterations; ++i) {
			for(int j = 0; j < batch; ++j) {
				int index = i * batch + j;

				var url = this.result.url + "/" + endpoints[index];

				var request = UnityWebRequest.Get(url);
				request.downloadHandler = new DownloadHandlerTexture();

				operations[j] = request.SendWebRequest();

				int func_j = j;

				uThread.RunCoroutine(this.Yield(operations[func_j]),
					() => {
						Debug.Log(index);
						var operation = operations[func_j];

						if(operation.webRequest.isHttpError || operation.webRequest.isNetworkError) {
							Debug.LogError(operation.webRequest.error);
						} else {
							photoViews[index].Texture2D = (operation.webRequest.downloadHandler as DownloadHandlerTexture).texture;
						}
					});
			}

			yield return new WaitUntil(() => {
				bool isDone = true;

				for(int k = 0; k < operations.Length; ++k) {
					int index = i * batch + k;

					photoViews[index].Progress = operations[k].progress;

					isDone &= operations[k].isDone && operations[k].progress >= 1.0F;
				}

				return isDone;
			});

			yield return new WaitForSecondsRealtime(0.25F);
		}

		int remaining = endpoints.Count % batch;

		if(remaining > 0) {
			for(int i = 0; i < remaining; ++i) {
				int index = iterations * batch + i;

				var url = this.result.url + "/" + endpoints[index];

				var request = UnityWebRequest.Get(url);
				request.downloadHandler = new DownloadHandlerTexture();

				operations[i] = request.SendWebRequest();

				int func_i = i;

				uThread.RunCoroutine(this.Yield(operations[func_i]), () => {
					photoViews[index].Texture2D = (operations[func_i].webRequest.downloadHandler as DownloadHandlerTexture).texture;
				});
			}

			yield return new WaitUntil(() => {
				bool isDone = true;

				for(int k = 0; k < remaining; ++k) {
					int index = iterations * batch + k;

					photoViews[index].Progress = operations[k].progress;

					isDone &= operations[k].isDone;
				}

				return isDone;
			});
		}
	}

	IEnumerator Yield(UnityWebRequestAsyncOperation operation) {
		yield return operation;
	}
}
