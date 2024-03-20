using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// 이 피드백을 사용하면 시간이 지남에 따라 대상 텍스트의 내용을 제어할 수 있습니다.
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("이 피드백을 사용하면 시간이 지남에 따라 대상 텍스트의 내용을 제어할 수 있습니다.")]
	[FeedbackPath("UI/Text")]
	public class MMFeedbackText : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		public enum ColorModes { Instant, Gradient, Interpolate }

		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.UIColor; } }
		#endif

		[Header("Target")]
		/// the Text component to control
		[Tooltip("제어할 텍스트 구성요소")]
		public Text TargetText;

        
		/// the new text to replace the old one with
		[Tooltip("이전 텍스트를 대체할 새 텍스트")]
		[TextArea]
		public string NewText = "Hello World";

		/// <summary>
		/// On play we change the text of our target TMPText
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			if (TargetText == null)
			{
				return;
			}

			TargetText.text = NewText;
		}
	}
}