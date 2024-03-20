using UnityEngine;
#if MM_TEXTMESHPRO
using TMPro;
#endif

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 이 피드백을 사용하면 대상 TMP 텍스트 구성 요소의 텍스트를 변경할 수 있습니다.
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("이 피드백을 사용하면 대상 TMP 텍스트 구성 요소의 텍스트를 변경할 수 있습니다.")]
	[FeedbackPath("TextMesh Pro/TMP Text")]
	public class MMFeedbackTMPText : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.TMPColor; } }
		#endif

		#if MM_TEXTMESHPRO
		[Header("TextMesh Pro")]
		/// the target TMP_Text component we want to change the text on
		[Tooltip("텍스트를 변경하려는 대상 TMP_Text 구성 요소")]
		public TMP_Text TargetTMPText;
		#endif
		
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
			
			#if MM_TEXTMESHPRO
			if (TargetTMPText == null)
			{
				return;
			}
			TargetTMPText.text = NewText;
			#endif
		}
	}
}