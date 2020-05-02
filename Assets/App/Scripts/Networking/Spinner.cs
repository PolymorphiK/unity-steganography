using UnityEngine;

public class Spinner : MonoBehaviour {
	public float degrees = 45.0F;
	public float delay = 0.1F;

	public float next = 0.0F;

	protected virtual void OnEnable() {
		this.transform.localRotation = Quaternion.identity;
	}

	protected virtual void Update() {
		if(Time.realtimeSinceStartup > next) {
			this.next = Time.realtimeSinceStartup + this.delay;

			this.transform.localRotation *= Quaternion.Euler(0.0F, 0.0F, this.degrees);
		}
	}
}