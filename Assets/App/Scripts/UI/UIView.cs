using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public abstract class UIView : MonoBehaviour {
	[SerializeField]
	private string title;

	protected virtual void OnEnable() {
		this.MatchParent();
	}

	public string Title { get { return this.title; } set { this.title = value; } }

	protected virtual void MatchParent() {
		var rectTransform = this.GetComponent<RectTransform>();
		rectTransform.localScale = Vector3.one;
		rectTransform.anchorMin = Vector2.zero;
		rectTransform.anchorMax = Vector2.one;
		rectTransform.sizeDelta = Vector2.zero;
		rectTransform.anchoredPosition = Vector2.zero;
	}
}