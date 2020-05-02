using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StegEncoding : MonoBehaviour {
	public Texture2D master;
	public Texture2D target;
	public string message = "Hello, World!";
	[Range(0.0F, 1.0F)]
	public float progress = 0.0F;

	public void Sample() {
		var pixels = this.master.GetPixels32();

		long total_bits = pixels.Length * 3;

		Debug.Log("Pixels: " + pixels.Length);
		Debug.Log("Bits: " + total_bits);
		Debug.Log("Mb: " + total_bits / 1E6F);
		Debug.Log("Bytes: " + total_bits / 8);
		Debug.Log("KB: " + (total_bits / 8 / 1024));
		Debug.Log("MB: " + (total_bits / 8 / 1024 / 1024));

		//this.target = new Texture2D(master.width, master.height);
		//this.target.SetPixels32(master.GetPixels32());
		//this.target.Apply();
	}

	public void Encode() {
		var pixels = this.master.GetPixels32();
		var bytes = System.Text.Encoding.UTF8.GetBytes(this.message);

		LSB.Encode(pixels, bytes,
			(result) => {
				Debug.Log(result);

				if(result == LSB.Code.Success) {
					this.target = new Texture2D(this.master.width, this.master.height);
					this.target.SetPixels32(pixels);
					this.target.Apply();
				}
			},
			(progress) => {
				this.progress = progress;
			});
	}

	public void Decode() {
		if(this.target == null) return;

		var pixels = this.target.GetPixels32();

		LSB.Decode(pixels,
			(code, data) => {
				if(code == LSB.Code.Error) {
					Debug.LogError("Error decoding...");
					return;
				}

				var message = System.Text.Encoding.UTF8.GetString(data);

				Debug.Log("Messsage: " + message);
			});
	}
}
