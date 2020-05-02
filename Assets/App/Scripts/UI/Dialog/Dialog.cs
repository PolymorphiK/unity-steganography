using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class Dialog : MonoBehaviour {
	private static Dialog instance;

	static Dialog Instance {
		get {
			if(Dialog.instance == null) {
				Dialog.instance = Object.FindObjectOfType<Dialog>();

				if(Dialog.instance == null) {
					var asset = Resources.Load<Dialog>("UIViews/Dialog");

					if(asset == null) {
						throw new System.Exception("Could not find Dialog component!");
					}

					asset.gameObject.SetActive(false);

					var clone = GameObject.Instantiate(asset.gameObject);

					Dialog.instance = clone.GetComponent<Dialog>();

					clone.GetComponent<Canvas>().sortingOrder = 3000;

					clone.SetActive(true);

					asset.gameObject.SetActive(true);
				}
			}

			return Dialog.instance;
		}
	}

	public Text message;

	public static void Show(string message) {
		if(Instance == null) return;

		Instance.message.text = message;

		Instance.gameObject.SetActive(true);
	}
}