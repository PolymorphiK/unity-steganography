using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class PhotoView : MonoBehaviour {
	[SerializeField]
	private Button button;
	[SerializeField]
	private RawImage rawImage;
	[SerializeField]
	private Spinner spinner;
	[SerializeField]
	private Image progress;
	[SerializeField]
	private Text percent;

	private Texture2D texture;

	protected virtual void Start() {
		this.spinner.gameObject.SetActive(true);
		this.RawImage.gameObject.SetActive(false);
		this.Progress = 0.0F;
		this.progress.gameObject.SetActive(true);
	}

	public Button Button {
		get {
			if(this.button == null) this.button = this.GetComponent<Button>();

			return this.button;
		}
	}

	public RawImage RawImage {
		get {
			if(this.rawImage == null) this.rawImage = this.GetComponentInChildren<RawImage>();

			return this.rawImage;
		}
	}

	public float Progress {
		get {
			return this.progress.fillAmount;
		} set {
			this.progress.fillAmount = value;

			if(value > 0.0F) {
				this.spinner.gameObject.SetActive(false);
			}

			this.percent.text = (value * 100.0F).ToString("0");
		}
	}

	public Texture2D Texture2D {
		get {
			return this.texture;
		} set {
			this.texture = value;

			this.RawImage.texture = this.texture;

			var aspectRatio = this.RawImage.GetComponent<AspectRatioFitter>();

			aspectRatio.aspectRatio = (float)this.texture.width / this.texture.height;
			aspectRatio.aspectMode = AspectRatioFitter.AspectMode.FitInParent;

			//this.spinner.gameObject.SetActive(false);
			this.RawImage.gameObject.SetActive(true);
			this.progress.gameObject.SetActive(false);
			this.percent.gameObject.SetActive(false);
		}
	}

	public virtual void OnView() {
		var pixels = this.Texture2D.GetPixels32();

		LSB.Decode(pixels,
			(code, data) => {
				if(code == LSB.Code.Error) {
					Debug.LogError("Could not decode message!");
					return;
				}

				var message = System.Text.Encoding.UTF8.GetString(data);

				int index = -1;

				for(int i = 0; i < message.Length; ++i) {
					if(message[i] == ']') {
						if((i + 1) < message.Length) {
							if(message[i + 1] == '|' && message[i + 2] == '[') {
								index = i;
								break;
							}
						} else {
							index = -1;
						}
					}
				}

				if (index == -1) return;

				message = message.Substring(0, index);

				Dialog.Show(message);
		});
	}
}
