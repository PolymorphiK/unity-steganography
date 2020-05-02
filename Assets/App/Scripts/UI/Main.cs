using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Main : MonoBehaviour {
	public string startView = "Photos";
	public RectTransform content;
	public Text header;
	private UIView current = null;

	private Dictionary<string, UIView> screens = new Dictionary<string, UIView>();

	protected virtual void Start() {
		this.Open(this.startView);
	}

	// this is not the ideal way, but for the sake
	// of simplicity let's go with it.
	public void Open(string target) {
		if(this.screens.ContainsKey(target)) {
			if(this.current) {
				this.current.gameObject.SetActive(false);
			}

			this.current = this.screens[target];

			this.current.gameObject.SetActive(true);

			this.header.text = this.current.Title;

			return;
		}

		var asset = Resources.Load<UIView>("UIViews/" + target);

		if(asset == null) {
			Debug.LogWarning("Could not open menu with Id " + target);
			return;
		}

		asset.gameObject.SetActive(false);

		if(this.current) {
			this.current.gameObject.SetActive(false);
		}

		var clone = Object.Instantiate(asset.gameObject);
		clone.transform.SetParent(this.content);
		clone.transform.localScale = Vector3.one;
		this.current = clone.GetComponent<UIView>();

		this.header.text = this.current.Title;

		this.screens.Add(target, this.current);

		clone.gameObject.SetActive(true);

		asset.gameObject.SetActive(true);
	}
}