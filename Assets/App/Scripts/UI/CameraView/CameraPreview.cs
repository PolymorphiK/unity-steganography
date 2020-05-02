using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(RawImage))]
public class CameraPreview : MonoBehaviour {
	[SerializeField]
	private bool isFrontFacing = true;
	[SerializeField]
	private ResolutionType resolution = ResolutionType.Normal;
	[SerializeField]
	private int refresh = 30;

	private RawImage rawImage;
	private WebCamTexture webCamTexture;

	public bool IsFrontFacing {
		get {
			return this.isFrontFacing;
		}
		set {
			this.isFrontFacing = value;

			this.Setup();
		}
	}

	public WebCamTexture Texture {
		get {
			return this.webCamTexture;
		}
	}

	public bool IsRendering {
		get {
			return this.webCamTexture == null ? false : this.webCamTexture.isPlaying;
		}
	}

	public ResolutionType Resolution {
		get {
			return this.resolution;
		}
		set {
			this.resolution = value;

			this.Setup();
		}
	}

	public RawImage Preview {
		get {
			if(this.rawImage == null) this.rawImage = this.GetComponent<RawImage>();

			return this.rawImage;
		}
	}

	public int Width {
		get; private set;
	}

	public int Height {
		get; private set;
	}

	protected virtual void Start() {
		if(Application.HasUserAuthorization(UserAuthorization.WebCam)) this.Setup();
	}

	public virtual void Setup() {
		//if(Application.HasUserAuthorization(UserAuthorization.WebCam)) {
		//	if(this.webCamTexture) this.webCamTexture.Stop();
		//}
		if(this.webCamTexture) {
			this.webCamTexture.Stop();
		}

		this.Preview.enabled = false;
		this.Preview.transform.rotation = Quaternion.identity;
		this.Preview.transform.localScale = Vector3.one;
		//this.Preview.SetNativeSize();
		//this.Preview.raycastTarget = WebCamTexture.devices == null || WebCamTexture.devices.Length == 0 ? true : false;

		if(WebCamTexture.devices == null || WebCamTexture.devices.Length == 0) {
			Debug.LogError("No devices detected.", this);
		}

		for(int i = 0; i < WebCamTexture.devices.Length; ++i) {
			var device = WebCamTexture.devices[i];

			if(device.isFrontFacing == this.IsFrontFacing) {
				Debug.Log("Device: " + device.name);

				if(device.availableResolutions == null) {
					Debug.LogWarning("No Available Resolutions", this);

					this.webCamTexture = new WebCamTexture(device.name);

					this.StartCoroutine(this._StartCamera());

					break;
					//continue;
				}

				//foreach(var res in resolutions) {
				//	var format = string.Format("Size ({0}, {1}) Refresh {2}", res.width, res.height, res.refreshRate);

				//	Debug.Log(format);
				//}

				var resolutionIndex = 0;

				switch(this.Resolution) {
					case ResolutionType.Highest:
						resolutionIndex = 0;
						break;
					case ResolutionType.High:
						resolutionIndex = Mathf.RoundToInt(device.availableResolutions.Length * 0.25F) % device.availableResolutions.Length;
						break;
					case ResolutionType.Normal:
						resolutionIndex = device.availableResolutions.Length / 2;
						break;
					case ResolutionType.Low:
						resolutionIndex = Mathf.RoundToInt(device.availableResolutions.Length * 0.75F) % device.availableResolutions.Length;
						break;
					case ResolutionType.Lowest:
						resolutionIndex = device.availableResolutions.Length - 1;
						break;
				}

				var resolution = new Vector2(device.availableResolutions[resolutionIndex].width, device.availableResolutions[resolutionIndex].height);

				//Debug.Log(string.Format("Loading with Resolution ({0}, {1})", resolution.width, resolution.height), this);

				//var resolution = new Vector2(
				//	device.availableResolutions[device.availableResolutions.Length / 2].width,
				//	device.availableResolutions[device.availableResolutions.Length / 2].height);

				//switch(this.Resolution) {
				//	case ResolutionType.Highest:
				//		//resolution = resolution * 1.0F;
				//		break;
				//	case ResolutionType.High:
				//		resolution = resolution * 0.8F;
				//		break;
				//	case ResolutionType.Normal:
				//		resolution = resolution * 0.6F;
				//		break;
				//	case ResolutionType.Low:
				//		resolution = resolution * 0.5F;
				//		break;
				//	case ResolutionType.Lowest:
				//		resolution = resolution * 0.4F;
				//		break;
				//}

				this.webCamTexture = new WebCamTexture(device.name, Mathf.FloorToInt(resolution.x), Mathf.FloorToInt(resolution.y));
				//this.webCamTexture = new WebCamTexture(device.name);
#if !UNITY_EDITOR_OSX // this seems to crash on OSX when creating a WebCamTexture...
				this.StartCoroutine(this._StartCamera());
#endif

				break;
			}
		}

		//Debug.Log("Device found: " + didFindDevice, this);
	}

	protected virtual IEnumerator _StartCamera() {
		var asyncOperation = Application.RequestUserAuthorization(UserAuthorization.WebCam);

		yield return asyncOperation;

		var requestTime = Time.realtimeSinceStartup;

		yield return new WaitUntil(() => {
			return Application.HasUserAuthorization(UserAuthorization.WebCam) || Time.realtimeSinceStartup - requestTime >= 10.0F;
		});

		yield return new WaitForSecondsRealtime(1.0F);

		if(Application.HasUserAuthorization(UserAuthorization.WebCam)) {
			var scale = Vector3.one;

			this.Preview.texture = this.webCamTexture;

			this.webCamTexture.Play();

			this.Preview.enabled = false;

			yield return new WaitUntil(() => {
				return this.webCamTexture.width > 32 || this.webCamTexture.requestedWidth > 32;
			});

			this.Preview.enabled = true;

			var cam_width = this.webCamTexture.width <= 32 ? this.webCamTexture.requestedWidth : this.webCamTexture.width;
			var cam_height = this.webCamTexture.height <= 32 ? this.webCamTexture.requestedHeight : this.webCamTexture.height;

			var size = new Vector2(cam_width, cam_height);

			if(this.IsFrontFacing) {
				this.Preview.transform.localRotation = Quaternion.Euler(0.0F, 0.0F,  -this.webCamTexture.videoRotationAngle);
			} else {
				this.Preview.transform.localRotation = Quaternion.Euler(0.0F, 0.0F, -this.webCamTexture.videoRotationAngle);
			}

			var compass = this.Preview.transform.localRotation * Vector2.right;
			compass.x = Mathf.Abs(Mathf.Round(compass.x));
			compass.y = Mathf.Abs(Mathf.Round(compass.y));
			var parent = this.transform.parent.GetComponent<RectTransform>();

			size = Quaternion.Euler(0.0F, 0.0F, this.webCamTexture.videoRotationAngle) * size;

			size.x = Mathf.Abs(size.x);
			size.y = Mathf.Abs(size.y);

			this.Width = (int)size.x;
			this.Height = (int)size.y;

			var pSize = Quaternion.Euler(0.0F, 0.0F, this.webCamTexture.videoRotationAngle) * parent.rect.size;
			pSize.x = Mathf.Abs(pSize.x);
			pSize.y = Mathf.Abs(pSize.y);

			var ratio = size.x / size.y;

			var aspectRatioFitter = this.GetComponent<AspectRatioFitter>();

			if(aspectRatioFitter) aspectRatioFitter.aspectRatio = ratio;

			// The large side is the height....
			if(compass.y > 0.0F) {
				var height = pSize.x * ratio;

				size.x = pSize.x;
				size.y = height;

				if(this.IsFrontFacing) {
					if(this.webCamTexture.videoVerticallyMirrored == false) {
						scale.y = -scale.y;
					}
				}
			} else {
				// the large side is the width...
				var height = pSize.x / ratio;

				size.x = pSize.x;
				size.y = height;

				if(this.IsFrontFacing) {
					if(this.webCamTexture.videoVerticallyMirrored == false) {
						scale.x = -scale.x;
					}
				}
			}

			this.Preview.rectTransform.sizeDelta = size;
			this.Preview.rectTransform.localScale = scale;

			this.Preview.texture = this.webCamTexture;

			this.webCamTexture.Play();

			Debug.Log("Camera Preview Complete.", this);
		} else {
			Debug.LogWarning("Application was not granted permission for Camera", this);
			this.webCamTexture = null;

			this.Preview.raycastTarget = true;
		}
	}

	public virtual void Play() {
		if(Application.HasUserAuthorization(UserAuthorization.WebCam) == false) return;

		if(this.webCamTexture && this.webCamTexture.isPlaying == false) this.webCamTexture.Play();

		this.Preview.texture = this.webCamTexture;

		this.gameObject.SetActive(true);
	}

	public virtual void Pause() {
		if(Application.HasUserAuthorization(UserAuthorization.WebCam) == false) return;

		if(this.webCamTexture && this.webCamTexture.isPlaying) this.webCamTexture.Pause();

		this.Preview.texture = null;
	}

	public virtual void Stop() {
		if(Application.HasUserAuthorization(UserAuthorization.WebCam) == false) return;

		if(this.webCamTexture && this.webCamTexture.isPlaying) this.webCamTexture.Stop();

		this.Preview.texture = null;
	}

	public virtual void Swap() {
		this.IsFrontFacing = !this.IsFrontFacing;
	}

	protected virtual void OnValidate() {
#if UNITY_EDITOR
		//this.Preview.SetNativeSize();
#endif
	}

	[System.Serializable]
	public enum ResolutionType {
		Highest = 0,
		High,
		Normal,
		Low,
		Lowest
	}
}
