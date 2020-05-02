using UnityEngine.Networking;
using Newtonsoft.Json;

public static class WebRequest : System.Object {
	public static UnityWebRequest CreateApiGetRequest(string url, int port, string action, object body = null) {
        string endpoint = string.Format("{0}:{1}", url, port);

        return WebRequest.CreateApiGetRequest(endpoint, action, body);
	}

    public static UnityWebRequest CreateApiGetRequest(string endpoint, string actionUrl, object body = null) {
        return CreateApiRequest(endpoint + actionUrl, UnityWebRequest.kHttpVerbGET, body);
    }

	public static UnityWebRequest CreateApiPostRequest(string url, int port, string action, object body = null) {
        string endpoint = string.Format("{0}:{1}", url, port);

        return WebRequest.CreateApiPostRequest(endpoint, action, body);
	}

    public static UnityWebRequest CreateApiPostRequest(string endpoint, string actionUrl, object body = null) {
        return CreateApiRequest(endpoint + actionUrl, UnityWebRequest.kHttpVerbPOST, body);
    }

    static UnityWebRequest CreateApiRequest(string url, string method, object body) {
        string bodyString = null;

        if(body is string) {
            bodyString = (string)body;
        } else if(body != null) {
            bodyString = JsonConvert.SerializeObject(body);
        }

        var request = new UnityWebRequest();

        request.url = url;
        request.method = method;
        request.downloadHandler = new DownloadHandlerBuffer();
        request.uploadHandler = new UploadHandlerRaw(
			string.IsNullOrEmpty(bodyString) ? null : System.Text.Encoding.UTF8.GetBytes(bodyString));
        request.SetRequestHeader("Accept", "application/json");
        request.SetRequestHeader("Content-Type", "application/json");
        request.timeout = 60;

        return request;
    }
}