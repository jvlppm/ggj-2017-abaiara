using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Threading.Tasks;

namespace UI
{
	public class AnimatedMessage : MonoBehaviour {
		public Text Label;
		public Transform Panel;

		float baseScale = 1;

		public static AnimatedMessage instance;

		void Awake() {
			instance = this;
			//baseScale = Panel.localScale.y;
			Panel.localScale = new Vector3(1, 0, 1);

			var baseColor = Label.color;
			Label.color = new Color(baseColor.r, baseColor.g, baseColor.b, 0);
		}

		public Task ShowMessageAsync(string text, float duration = 2) {
			return this.RunTask(ShowMessageAnimatedCR(text, duration));
		}

		IEnumerator ShowMessageAnimatedCR(string text, float duration) {
			Label.text = text;
			var baseColor = Label.color;

			yield return Task.WhenAll(
				Task.Factory.StartNew(CoroutineAnimations.Interpolate(0.5f, v => {
					Panel.localScale = new Vector3(1, v * baseScale, 1); 
				})),
				Task.Factory.StartNew(CoroutineAnimations.Interpolate(1, v => {
					Label.transform.localPosition = new Vector3(500 * (1 - v), 0, 0);
					Label.color = new Color(baseColor.r, baseColor.g, baseColor.b, 0.2f + v * 0.8f);
				}))
			);

			yield return new WaitForSeconds(duration);

			yield return Task.WhenAll(
				Task.Factory.StartNew(CoroutineAnimations.Interpolate(0.5f, v => {
					Panel.localScale = new Vector3(1, (1 - v) * baseScale, 1);
					Label.color = new Color(baseColor.r, baseColor.g, baseColor.b, Mathf.Clamp(1 - v * 2, 0, 1)); 
				})),
				Task.Factory.StartNew(CoroutineAnimations.Interpolate(1, v => {
					Label.transform.localPosition = new Vector3(-500 * v, 0, 0);
				}))
			);

			Label.transform.Translate(0, 500, 0);
		}
	}
}
