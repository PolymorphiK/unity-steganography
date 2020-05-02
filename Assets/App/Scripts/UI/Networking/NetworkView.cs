using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

[RequireComponent(typeof(Canvas))]
public class NetworkView : MonoBehaviour {
	static NetworkView instance = null;

	static NetworkView Instance {
		get {
			if(NetworkView.instance == null) {
				NetworkView.instance = Object.FindObjectOfType<NetworkView>();

				if(NetworkView.instance == null) {
					var asset = Resources.Load<NetworkView>("UIViews/NetworkView");

					if(asset == null) throw new System.IO.FileLoadException("Could not load NetworkView from Resources folder!");

					asset.gameObject.SetActive(false);

					var clone = GameObject.Instantiate(asset.gameObject);

					DontDestroyOnLoad(clone);

					NetworkView.instance = clone.GetComponent<NetworkView>();

					asset.gameObject.SetActive(true);
				}
			}

			return NetworkView.instance;
		}
	}

	public Slider progress;
	public Text output;

	private AsyncOperation operation = null;

	Queue<Request> networkRequests = new Queue<Request>();

	public static void PostImage(Texture2D texture) {
		var png = texture.EncodeToPNG();

		var base64 = System.Convert.ToBase64String(png);
		var type = "png";

		var data = new Dictionary<string, string>();
		data.Add("image", base64);
		data.Add("type", type);

		var json = JsonConvert.SerializeObject(data);

		var request = WebRequest.CreateApiPostRequest(Endpoints.Endpoint, Endpoints.Post.Images, json);

		Instance.networkRequests.Enqueue(new Request {
			request = request,
			callback = (response) => {
				Instance.output.text = response;
			},
			name = "Posting Image..."
		});

		Instance.gameObject.SetActive(true);
	}

	private void OnEnable() {
		this.StartCoroutine(this._RunRequest());

		this.GetComponent<Canvas>().sortingOrder = 32767;
	}

	protected IEnumerator _RunRequest() {
		while(this.networkRequests.Count > 0) {
			this.operation = null;
			var request = this.networkRequests.Dequeue();

			this.output.text = request.name;

			var webRequest = request.request;

			var asyncOperation = webRequest.SendWebRequest();

			this.operation = asyncOperation;

			yield return asyncOperation;

			if(webRequest.isNetworkError || webRequest.isHttpError) {
				request.callback?.Invoke(webRequest.error);

				continue;
			}

			request.callback?.Invoke(webRequest.downloadHandler.text);

			yield return new WaitForSecondsRealtime(2.0F);
		}

		this.gameObject.SetActive(false);
	}

	protected virtual void Update() {
		this.progress.value = this.operation == null ? 0.0F : this.operation.progress;
	}

	class Request {
		public UnityWebRequest request;
		public System.Action<string> callback;
		public string name;
	}

	public interface IResult {
		int code { get; }
		string error { get; }
		string message { get; }
	}

	public interface IResult<T> : IResult {
		T payload { get; }
	}
}