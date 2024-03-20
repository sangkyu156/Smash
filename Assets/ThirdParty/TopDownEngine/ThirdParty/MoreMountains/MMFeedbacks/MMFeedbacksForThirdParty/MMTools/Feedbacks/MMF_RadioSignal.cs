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
	public class MMF_RadioSignal : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
        
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.GameObjectColor; } }
		public override bool EvaluateRequiresSetup() { return (TargetSignal == null); }
		public override string RequiredTargetText { get { return TargetSignal != null ? TargetSignal.name : "";  } }
		public override string RequiresSetupText { get { return "This feedback requires that a TargetSignal be set to be able to work properly. You can set one below."; } }
		#endif
        
		/// the duration of this feedback is 0
		public override float FeedbackDuration { get { return 0f; } }
		public override bool HasRandomness => true;
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => TargetSignal = FindAutomatedTarget<MMRadioSignal>();

		[MMFInspectorGroup("Radio Signal", true, 72)]
		/// The target MMRadioSignal to trigger
		[Tooltip("트리거할 대상 MMRadioSignal")]
		public MMRadioSignal TargetSignal;
		/// the timescale to operate on
		[Tooltip("작업할 기간")]
		public MMRadioSignal.TimeScales TimeScale = MMRadioSignal.TimeScales.Unscaled;
		/// the duration of the shake, in seconds
		[Tooltip("흔들림의 지속 시간(초)")]
		public float Duration = 1f;
		/// a global multiplier to apply to the end result of the combination
		[Tooltip("조합의 최종 결과에 적용할 전역 승수")]
		public float GlobalMultiplier = 1f;
        

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
					float intensityMultiplier = ComputeIntensity(feedbacksIntensity, position);
                    
					TargetSignal.Duration = Duration;
					TargetSignal.GlobalMultiplier = GlobalMultiplier * intensityMultiplier;
					TargetSignal.TimeScale = TimeScale;
					TargetSignal.StartShaking();
				}
			}
		}

		/// <summary>
		/// On Stop, stops the target signal
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
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
		
		/// <summary>
		/// On restore, we put our object back at its initial position
		/// </summary>
		protected override void CustomRestoreInitialValues()
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			
			if (TargetSignal != null)
			{
				TargetSignal.Stop();
				TargetSignal.ApplyLevel(0f);
			}
		}
	}
}