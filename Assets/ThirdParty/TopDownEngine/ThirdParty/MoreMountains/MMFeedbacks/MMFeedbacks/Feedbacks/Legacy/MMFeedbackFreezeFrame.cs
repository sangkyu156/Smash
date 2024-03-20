using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 이 피드백은 플레이 시 고정 프레임 이벤트를 트리거하여 지정된 기간 동안 게임을 일시 중지합니다(일반적으로 짧지만 반드시 그런 것은 아님).
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("이 피드백은 지정된 기간(초) 동안 시간 척도를 동결합니다. 저는 보통 0.01초나 0.02초를 사용하지만 원하는 대로 자유롭게 조정할 수 있습니다. 작동하려면 장면에 MMTimeManager가 필요합니다.")]
	[FeedbackPath("Time/Freeze Frame")]
	public class MMFeedbackFreezeFrame : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.TimeColor; } }
		#endif

		[Header("Freeze Frame")]
		/// the duration of the freeze frame
		[Tooltip("정지 프레임의 지속 시간")]
		public float FreezeFrameDuration = 0.02f;
		/// the minimum value the timescale should be at for this freeze frame to happen. This can be useful to avoid triggering freeze frames when the timescale is already frozen. 
		[Tooltip("이 정지 프레임이 발생하는 데 필요한 시간 척도의 최소값입니다. 이는 시간 척도가 이미 고정되어 있을 때 고정 프레임이 트리거되는 것을 방지하는 데 유용할 수 있습니다.")]
		public float MinimumTimescaleThreshold = 0.1f;

		/// the duration of this feedback is the duration of the freeze frame
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(FreezeFrameDuration); } set { FreezeFrameDuration = value; } }

		/// <summary>
		/// On Play we trigger a freeze frame event
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
            
			if (Time.timeScale < MinimumTimescaleThreshold)
			{
				return;
			}
            
			MMFreezeFrameEvent.Trigger(FeedbackDuration);
		}
	}
}