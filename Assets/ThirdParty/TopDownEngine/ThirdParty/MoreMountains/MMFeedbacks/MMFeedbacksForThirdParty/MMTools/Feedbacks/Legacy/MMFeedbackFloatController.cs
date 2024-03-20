﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 이 피드백은 대상 FloatController에서 일회성 재생을 트리거합니다.
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("이 피드백을 사용하면 대상 FloatController에서 일회성 재생을 트리거할 수 있습니다.")]
	[FeedbackPath("GameObject/FloatController")]
	public class MMFeedbackFloatController : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// the different possible modes 
		public enum Modes { OneTime, ToDestination }
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.GameObjectColor; } }
		#endif

		[Header("Float Controller")]
		/// the mode this controller is in
		[Tooltip("이 컨트롤러의 모드")]
		public Modes Mode = Modes.OneTime;
		/// the float controller to trigger a one time play on
		[Tooltip("일회성 재생을 트리거하는 플로트 컨트롤러")]
		public FloatController TargetFloatController;
		/// whether this should revert to original at the end
		[Tooltip("마지막에 원본으로 되돌릴지 여부")]
		public bool RevertToInitialValueAfterEnd = false;
		/// the duration of the One Time shake
		[Tooltip("One Time Shake의 지속 시간")]
		[MMFEnumCondition("Mode", (int)Modes.OneTime)]
		public float OneTimeDuration = 1f;
		/// the amplitude of the One Time shake (this will be multiplied by the curve's height)
		[Tooltip("일회성 흔들림의 진폭(이에 곡선 높이를 곱함)")]
		[MMFEnumCondition("Mode", (int)Modes.OneTime)]
		public float OneTimeAmplitude = 1f;
		/// the low value to remap the normalized curve value to 
		[Tooltip("정규화된 곡선 값을 다시 매핑할 낮은 값")]
		[MMFEnumCondition("Mode", (int)Modes.OneTime)]
		public float OneTimeRemapMin = 0f;
		/// the high value to remap the normalized curve value to 
		[Tooltip("정규화된 곡선 값을 다시 매핑할 높은 값")]
		[MMFEnumCondition("Mode", (int)Modes.OneTime)]
		public float OneTimeRemapMax = 1f;
		/// the curve to apply to the one time shake
		[Tooltip("일회성 흔들기에 적용할 곡선")]
		[MMFEnumCondition("Mode", (int)Modes.OneTime)]
		public AnimationCurve OneTimeCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to move this float controller to
		[Tooltip("이 float 컨트롤러를 이동할 값")]
		[MMFEnumCondition("Mode", (int)Modes.ToDestination)]
		public float ToDestinationValue = 1f;
		/// the duration over which to move the value
		[Tooltip("값을 이동하는 기간")]
		[MMFEnumCondition("Mode", (int)Modes.ToDestination)]
		public float ToDestinationDuration = 1f;
		/// the curve over which to move the value in ToDestination mode
		[Tooltip("ToDestination 모드에서 값을 이동할 곡선")]
		[MMFEnumCondition("Mode", (int)Modes.ToDestination)]
		public AnimationCurve ToDestinationCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));

		/// the duration of this feedback is the duration of the one time hit
		public override float FeedbackDuration
		{
			get { return (Mode == Modes.OneTime) ? ApplyTimeMultiplier(OneTimeDuration) : ApplyTimeMultiplier(ToDestinationDuration); } 
			set { OneTimeDuration = value; ToDestinationDuration = value; }
		}

		protected float _oneTimeDurationStorage;
		protected float _oneTimeAmplitudeStorage;
		protected float _oneTimeRemapMinStorage;
		protected float _oneTimeRemapMaxStorage;
		protected AnimationCurve _oneTimeCurveStorage;
		protected float _toDestinationValueStorage;
		protected float _toDestinationDurationStorage;
		protected AnimationCurve _toDestinationCurveStorage;
		protected bool _revertToInitialValueAfterEndStorage;

		/// <summary>
		/// On init we grab our initial values on the target float controller
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(GameObject owner)
		{
			if (Active && (TargetFloatController != null))
			{
				_oneTimeDurationStorage = TargetFloatController.OneTimeDuration;
				_oneTimeAmplitudeStorage = TargetFloatController.OneTimeAmplitude;
				_oneTimeCurveStorage = TargetFloatController.OneTimeCurve;
				_oneTimeRemapMinStorage = TargetFloatController.OneTimeRemapMin;
				_oneTimeRemapMaxStorage = TargetFloatController.OneTimeRemapMax;
				_toDestinationCurveStorage = TargetFloatController.ToDestinationCurve;
				_toDestinationDurationStorage = TargetFloatController.ToDestinationDuration;
				_toDestinationValueStorage = TargetFloatController.ToDestinationValue;
				_revertToInitialValueAfterEndStorage = TargetFloatController.RevertToInitialValueAfterEnd;
			}
		}

		/// <summary>
		/// On play we trigger a one time or ToDestination play on our target float controller
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || (TargetFloatController == null))
			{
				return;
			}
            
			float intensityMultiplier = Timing.ConstantIntensity ? 1f : feedbacksIntensity;
			TargetFloatController.RevertToInitialValueAfterEnd = RevertToInitialValueAfterEnd;

			if (Mode == Modes.OneTime)
			{
				TargetFloatController.OneTimeDuration = FeedbackDuration;
				TargetFloatController.OneTimeAmplitude = OneTimeAmplitude;
				TargetFloatController.OneTimeCurve = OneTimeCurve;
				if (NormalPlayDirection)
				{
					TargetFloatController.OneTimeRemapMin = OneTimeRemapMin * intensityMultiplier;
					TargetFloatController.OneTimeRemapMax = OneTimeRemapMax * intensityMultiplier;
				}
				else
				{
					TargetFloatController.OneTimeRemapMin = OneTimeRemapMax * intensityMultiplier;
					TargetFloatController.OneTimeRemapMax = OneTimeRemapMin * intensityMultiplier;   
				}
				TargetFloatController.OneTime();
			}
			if (Mode == Modes.ToDestination)
			{
				TargetFloatController.ToDestinationCurve = ToDestinationCurve;
				TargetFloatController.ToDestinationDuration = FeedbackDuration;
				TargetFloatController.ToDestinationValue = ToDestinationValue;
				TargetFloatController.ToDestination();
			}
		}

		/// <summary>
		/// On reset we reset our values on the target controller with the ones stored initially
		/// </summary>
		protected override void CustomReset()
		{
			base.CustomReset();
			if (Active && FeedbackTypeAuthorized && (TargetFloatController != null))
			{
				TargetFloatController.OneTimeDuration = _oneTimeDurationStorage;
				TargetFloatController.OneTimeAmplitude = _oneTimeAmplitudeStorage;
				TargetFloatController.OneTimeCurve = _oneTimeCurveStorage;
				TargetFloatController.OneTimeRemapMin = _oneTimeRemapMinStorage;
				TargetFloatController.OneTimeRemapMax = _oneTimeRemapMaxStorage;
				TargetFloatController.ToDestinationCurve = _toDestinationCurveStorage;
				TargetFloatController.ToDestinationDuration = _toDestinationDurationStorage;
				TargetFloatController.ToDestinationValue = _toDestinationValueStorage;
				TargetFloatController.RevertToInitialValueAfterEnd = _revertToInitialValueAfterEndStorage;
			}
		}


		/// <summary>
		/// On stop, we interrupt movement if it was active
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
            
			if (TargetFloatController != null)
			{
				TargetFloatController.Stop();
			}
		}
	}
}