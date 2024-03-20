using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 이 피드백을 사용하면 대상 MMRadioSignal(일반적으로 MMRadioBroadcaster에서 MMRadioReceivers에서 들을 수 있는 값을 내보내는 데 사용됨)에서 재생을 트리거할 수 있습니다. 이 피드백에서 기간, 시간 척도 및 승수를 지정할 수도 있습니다.
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("이 피드백을 사용하면 대상 MMRadioSignal(일반적으로 MMRadioBroadcaster에서 MMRadioReceivers에서 들을 수 있는 값을 내보내는 데 사용됨)에서 재생을 트리거할 수 있습니다. 이 피드백에서 기간, 시간 척도 및 승수를 지정할 수도 있습니다.")]
	[FeedbackPath("GameObject/MMRadioSignal")]
	public class MMFeedbackRadioSignal : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		
		/// the duration of this feedback is 0
		public override float FeedbackDuration { get { return 0f; } }

		/// The target MMRadioSignal to trigger
		[Tooltip("트리거할 대상 MMRadioSignal")]
		public MMRadioSignal TargetSignal;
		/// the timescale to operate on
		[Tooltip("작업할 기간")]
		public MMRadioSignal.TimeScales TimeScale = MMRadioSignal.TimeScales.Unscaled;
        
		/// the duration of the shake, in seconds
		[Tooltip("흔들림 지속 시간(초)")]
		public float Duration = 1f;
		/// a global multiplier to apply to the end result of the combination
		[Tooltip("조합의 최종 결과에 적용할 전역 승수")]
		public float GlobalMultiplier = 1f;

		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.GameObjectColor; } }
		#endif
        

		/// <summary>
		/// On Play we set the values on our target signal and make it start shaking its level
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (Active && FeedbackTypeAuthorized)
			{
				if (TargetSignal != null)
				{
					float intensityMultiplier = Timing.ConstantIntensity ? 1f : feedbacksIntensity;
                    
					TargetSignal.Duration = Duration;
					TargetSignal.GlobalMultiplier = GlobalMultiplier * intensityMultiplier;
					TargetSignal.TimeScale = TimeScale;
					TargetSignal.StartShaking();
				}
			}
		}

		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
		{
			base.CustomStopFeedback(position, feedbacksIntensity);
			if (Active)
			{
				if (TargetSignal != null)
				{
					TargetSignal.Stop();
				}
			}
		}
	}
}