using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 이 피드백은 플레이 시 TimeScale 이벤트를 전송하여 시간 척도를 변경합니다.
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("이 피드백은 장면에 MMTimeManager 개체가 있는 경우 지정된 설정에 따라 시간 단위를 수정하는 데 사용되는 MMTimeScaleEvent를 트리거합니다. 이러한 설정은 새로운 시간 척도(0.5는 정상보다 2배 느리고, 2는 2배 더 빠름), 시간 척도 수정 기간, 일반 시간 척도와 변경된 시간 척도 사이를 전환하는 선택적 속도입니다.")]
	[FeedbackPath("Time/Timescale Modifier")]
	public class MMF_TimescaleModifier : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
        /// <summary>
        /// 이 피드백에 가능한 모드는 다음과 같습니다.
        /// - shake : 특정 기간 동안 시간 척도를 변경합니다.
        /// - 변경: 시간 척도를 새로운 값으로 영원히 설정합니다(다시 변경할 때까지).
        /// - 재설정: 시간 척도를 이전 값으로 재설정합니다.
        /// </summary>
        public enum Modes { Shake, Change, Reset }

		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.TimeColor; } }
		public override string RequiredTargetText { get { return Mode.ToString() + " x" + TimeScale ;  } }
		public override bool HasCustomInspectors { get { return true; } }
		#endif

		[MMFInspectorGroup("Timescale Modifier", true, 63)]
		/// the selected mode
		[Tooltip("이 피드백에 가능한 모드는 다음과 같습니다. : shake : 특정 기간 동안 시간 척도를 변경합니다." +
                 "- change : 시간 척도를 새로운 값으로 영원히 설정합니다(다시 변경할 때까지)." +
                 "- reset : 시간 척도를 이전 값으로 재설정합니다.")]
		public Modes Mode = Modes.Shake;

		/// the new timescale to apply
		[Tooltip("the new timescale to apply")]
		public float TimeScale = 0.5f;
		/// the duration of the timescale modification
		[Tooltip("기간 수정 기간")]
		[MMFEnumCondition("Mode", (int)Modes.Shake)]
		public float TimeScaleDuration = 1f;
		/// whether to reset the timescale on Stop or not
		[Tooltip("중지 시 시간 척도를 재설정할지 여부")]
		public bool ResetTimescaleOnStop = false;
		
		[MMFInspectorGroup("Interpolation", true, 63)]
		/// whether or not we should lerp the timescale
		[Tooltip("시간 척도를 조정해야 할지 말지")]
		public bool TimeScaleLerp = false;
		/// whether to lerp over a set duration, or at a certain speed
		[Tooltip("설정된 기간 동안 또는 특정 속도로 러프할지 여부")]
		public MMTimeScaleLerpModes TimescaleLerpMode = MMTimeScaleLerpModes.Speed;
		/// in Speed mode, the speed at which to lerp the timescale
		[Tooltip("속도 모드에서 시간 척도를 조정하는 속도")]
		[MMFEnumCondition("TimescaleLerpMode", (int)MMTimeScaleLerpModes.Speed)]
		public float TimeScaleLerpSpeed = 1f;
		/// in Duration mode, the curve to use to lerp the timescale
		[Tooltip("기간 모드에서 시간 척도를 조정하는 데 사용할 곡선")]
		[MMFEnumCondition("TimescaleLerpMode", (int)MMTimeScaleLerpModes.Duration)]
		public MMTweenType TimescaleLerpCurve = new MMTweenType( new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1))); 
		/// in Duration mode, the duration of the timescale interpolation, in unscaled time seconds
		[Tooltip("Duration 모드에서 시간 척도 보간 기간(스케일이 적용되지 않은 시간 초 단위)")]
		[MMFEnumCondition("TimescaleLerpMode", (int)MMTimeScaleLerpModes.Duration)]
		public float TimescaleLerpDuration = 1f;
		/// whether or not we should lerp the timescale as it goes back to normal afterwards
		[Tooltip("나중에 정상으로 돌아오면서 시간 척도를 조정해야 할지 여부")]
		[MMFEnumCondition("TimescaleLerpMode", (int)MMTimeScaleLerpModes.Duration)]
		public bool TimeScaleLerpOnReset = false;
		/// in Duration mode, the curve to use to lerp the timescale
		[Tooltip("기간 모드에서 시간 척도를 조정하는 데 사용할 곡선")]
		[MMFEnumCondition("TimescaleLerpMode", (int)MMTimeScaleLerpModes.Duration)]
		public MMTweenType TimescaleLerpCurveOnReset = new MMTweenType( new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1)));
		/// in Duration mode, the duration of the timescale interpolation, in unscaled time seconds
		[Tooltip("Duration 모드에서 시간 척도 보간 기간(스케일이 적용되지 않은 시간 초 단위)")]
		[MMFEnumCondition("TimescaleLerpMode", (int)MMTimeScaleLerpModes.Duration)]
		public float TimescaleLerpDurationOnReset = 1f;

		/// the duration of this feedback is the duration of the time modification
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(TimeScaleDuration); } set { TimeScaleDuration = value; } }

		/// <summary>
		/// On Play, triggers a time scale event
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			switch (Mode)
			{
				case Modes.Shake:
					MMTimeScaleEvent.Trigger(MMTimeScaleMethods.For, TimeScale, FeedbackDuration, TimeScaleLerp, TimeScaleLerpSpeed, false, TimescaleLerpMode, TimescaleLerpCurve, TimescaleLerpDuration, TimeScaleLerpOnReset, TimescaleLerpCurveOnReset, TimescaleLerpDurationOnReset);
					break;
				case Modes.Change:
					MMTimeScaleEvent.Trigger(MMTimeScaleMethods.For, TimeScale, 0f, TimeScaleLerp, TimeScaleLerpSpeed, true, TimescaleLerpMode, TimescaleLerpCurve, TimescaleLerpDuration, TimeScaleLerpOnReset, TimescaleLerpCurveOnReset, TimescaleLerpDurationOnReset);
					break;
				case Modes.Reset:
					MMTimeScaleEvent.Trigger(MMTimeScaleMethods.Reset, TimeScale, 0f, false, 0f, true);
					break;
			}     
		}

		/// <summary>
		/// On stop, we reset timescale if needed
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || !ResetTimescaleOnStop)
			{
				return;
			}
			MMTimeScaleEvent.Trigger(MMTimeScaleMethods.Reset, TimeScale, 0f, false, 0f, true);
		}
		
		/// <summary>
		/// On restore, we restore our initial state
		/// </summary>
		protected override void CustomRestoreInitialValues()
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			MMTimeScaleEvent.Trigger(MMTimeScaleMethods.Reset, TimeScale, 0f, false, 0f, true);
		}
	}
}