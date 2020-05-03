using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ImageEncoder : MonoBehaviour {
	public Button encode;
	public OnEncodeEvent onEncodeEvent = new OnEncodeEvent();

	private string message = string.Empty;
	private Texture2D picture = null;

	protected virtual void OnEnable() {
		this.OnMessageChanged(this.message);
	}

	public virtual void OnMessageChanged(string text) {
		this.message = text;

		this.encode.interactable = string.IsNullOrEmpty(this.message) == false && this.picture;
	}

	public virtual void OnPictureCaptured(Texture2D picture) {
		this.picture = picture;

		this.encode.interactable = string.IsNullOrEmpty(this.message) == false && this.picture;
	}

	public virtual void Encode() {
		var pixels = this.picture.GetPixels32();

		var message = this.message + "]|[";

		LSB.Encode(pixels, System.Text.Encoding.UTF8.GetBytes(message),
			(code) => {
				if(code == LSB.Code.Success) {
					var result = new Texture2D(this.picture.width, this.picture.height);

					result.SetPixels32(pixels);

					this.onEncodeEvent.Invoke(result);
				}
			});
	}

	[System.Serializable]
	public class OnEncodeEvent : UnityEvent<Texture2D> { }
}