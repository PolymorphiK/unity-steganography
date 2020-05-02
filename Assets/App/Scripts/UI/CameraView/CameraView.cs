using UnityEngine;

public class CameraView : UIView {
	public ImageEncoder imageEncoder;

	protected override void OnEnable() {
		base.OnEnable();

		this.imageEncoder.onEncodeEvent.AddListener(this.ImageEncoder_OnEncodeEvent);
	}

	protected virtual void OnDisable() {
		this.imageEncoder.onEncodeEvent.RemoveListener(this.ImageEncoder_OnEncodeEvent);
	}

	protected virtual void ImageEncoder_OnEncodeEvent(Texture2D stegImage) {
		NetworkView.PostImage(stegImage);
	}
}