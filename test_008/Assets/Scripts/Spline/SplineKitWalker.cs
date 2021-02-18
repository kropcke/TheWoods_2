using UnityEngine;

public class SplineKitWalker : MonoBehaviour {

	public SplineKitSpline spline;

	public float duration;

	public bool lookForward;

	public SplineKitWalkerMode mode;

	private float progress;
	private bool goingForward = true;

	private void Update() {
		if (goingForward) {
			progress += Time.deltaTime / duration;
			if (progress > 1f) {
				if (mode == SplineKitWalkerMode.Once) {
					progress = 1f;
				} else if (mode == SplineKitWalkerMode.Loop) {
					progress -= 1f;
				} else {
					progress = 2f - progress;
					goingForward = false;
				}
			}
		} else {
			progress -= Time.deltaTime / duration;
			if (progress < 0f) {
				progress = -progress;
				goingForward = true;
			}
		}

		Vector3 position = spline.GetPoint(progress);
		transform.localPosition = position;
		if (lookForward) {
			transform.LookAt(position + spline.GetDirection(progress));
		}
	}
}