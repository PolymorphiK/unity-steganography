using UnityEngine;
using UnityEngine.Events;

public class ImageCapture : MonoBehaviour {
	public CameraPreview preview;
	public OnPictureEvent onPictureEvent = new OnPictureEvent();

	private Texture2D picture;

	public virtual void TakePicture() {
		int width = this.preview.Texture.width;
		int height = this.preview.Texture.height;
		Color32[] pixels = this.preview.Texture.GetPixels32();
		int iterations = this.preview.Texture.videoRotationAngle / 90;

		this.picture = null;

		if(iterations > 0) {
			this.picture = this.RotateTexture(pixels, width, height, iterations > 0, Mathf.Abs(iterations));
		} else {
			this.picture = new Texture2D(width, height);

			this.picture.SetPixels32(pixels);
		}

		this.picture.Apply();

		this.preview.Pause();
		this.preview.gameObject.SetActive(false);

		this.onPictureEvent.Invoke(this.picture);
	}

	public virtual void Retake() {
		//this.ResetState();
		this.preview.gameObject.SetActive(true);
		this.preview.Play();
	}

	public virtual void ChangeCameraMode() {
		this.preview.IsFrontFacing = !this.preview.IsFrontFacing;
	}

	// https://answers.unity.com/questions/951835/rotate-the-contents-of-a-texture.html
	Texture2D RotateTexture(Color32[] pixels, int width, int height, bool clockwise, int iterations) {
		Color32[] original = pixels;
		Color32[] rotated = new Color32[original.Length];
		int w = height;
		int h = width;

		int iRotated, iOriginal;

		for(int iteration = 0; iteration < iterations; ++iteration) {
			int t = w;
			w = h;
			h = t;

			for(int j = 0; j < h; ++j) {
				for(int i = 0; i < w; ++i) {
					iRotated = (i + 1) * h - j - 1;
					iOriginal = clockwise ? original.Length - 1 - (j * w + i) : j * w + i;
					rotated[iRotated] = original[iOriginal];
				}
			}

			for(int i = 0; i < original.Length; ++i) {
				original[i] = rotated[i];
			}
		}

		Texture2D rotatedTexture = new Texture2D(h, w);
		rotatedTexture.SetPixels32(rotated);
		return rotatedTexture;
	}

	[System.Serializable]
	public class OnPictureEvent : UnityEvent<Texture2D> { }
}