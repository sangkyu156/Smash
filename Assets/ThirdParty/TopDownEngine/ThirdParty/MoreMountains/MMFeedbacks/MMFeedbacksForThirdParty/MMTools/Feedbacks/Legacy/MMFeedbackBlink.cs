using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 이 피드백은 MMBlink 개체를 트리거하여 무언가를 깜박일 수 있게 합니다.
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("이 피드백을 사용하면 MMBlink 개체에서 깜박임을 트리거할 수 있습니다.")]
	[FeedbackPath("Renderer/MMBlink")]
	public class MMFeedbackBlink : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.RendererColor; } }
		#endif

		/// the possible modes for this feedback, that correspond to MMBlink's public methods
		public enum BlinkModes { Toggle, Start, Stop }
        
		[Header("Blink")]
		/// the target object to blink
		[Tooltip("깜박이는 대상 객체")]
		public MMBlink TargetBlink;
		/// the selected mode for this feedback
		[Tooltip("이 피드백을 위해 선택된 모드")]
		public BlinkModes BlinkMode = BlinkModes.Toggle;

		/// <summary>
		/// On Custom play, we trigger our MMBlink object
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || (TargetBlink == null))
			{
				return;
			}
			TargetBlink.TimescaleMode = Timing.TimescaleMode;
			switch (BlinkMode)
			{
				case BlinkModes.Toggle:
					TargetBlink.ToggleBlinking();
					break;
				case BlinkModes.Start:
					TargetBlink.StartBlinking();
					break;
				case BlinkModes.Stop:
					TargetBlink.StopBlinking();
					break;
			}
		}
	}
}