using UnityEngine;
using System.Collections;
using Newtonsoft.Json;
using System.Collections.Generic;

public class PhotosView : UIView {
	[SerializeField]
	private RectTransform loading;
	[SerializeField]
	private RectTransform noPhotos;
	[SerializeField]
	private PhotosLoader photos;

	private bool isFetchingPhotoList = false;

	protected virtual void FetchPhotoList() {
		if(this.isFetchingPhotoList || this.photos.IsLoading) return;

		uThread.RunCoroutine(this._FetchPhotoList(), () => {
			this.isFetchingPhotoList = false;
		});
	}

	protected IEnumerator _FetchPhotoList() {
		// sanity...
		if(this.isFetchingPhotoList) yield break;

		this.loading.gameObject.SetActive(true);
		this.noPhotos.gameObject.SetActive(false);
		this.photos.gameObject.SetActive(false);
		

		this.isFetchingPhotoList = true;

		var request = WebRequest.CreateApiGetRequest(Endpoints.Endpoint, Endpoints.Get.Images);

		yield return request.SendWebRequest();

		this.loading.gameObject.SetActive(false);

		if(request.isNetworkError || request.isHttpError) {
			this.noPhotos.gameObject.SetActive(true);
			yield break;
		}

		var results = JsonConvert.DeserializeObject<PhotoResult>(request.downloadHandler.text);

		if(results == null || results.images == null || results.images.Length == 0) {
			this.noPhotos.gameObject.SetActive(true);

			yield break;
		}

		this.photos.LoadPhotos(results);
	}

	public virtual void Refresh() {
		this.FetchPhotoList();
	}

	[System.Serializable]
	public class PhotoResult {
		public string url;
		public string[] images;
	}
}