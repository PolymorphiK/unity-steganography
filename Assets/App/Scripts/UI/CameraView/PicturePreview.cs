using UnityEngine;
using UnityEngine.UI;

public class PicturePreview : MonoBehaviour {
	[SerializeField]
	private Image preview;

	public void Preview(Texture2D texture) {
		var sprite = Sprite.Create(texture, new Rect(Vector2.zero, new Vector2(texture.width, texture.height)), Vector2.one * 0.5F);

		this.preview.sprite = sprite;

		this.gameObject.SetActive(true);
	}
}