﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 이 피드백을 사용하면 왜곡 필터의 왜곡 레벨을 제어할 수 있습니다. 필터에는 MMAudioFilterDistortionShaker가 필요합니다.
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackPath("Audio/Audio Filter Distortion")]
	[FeedbackHelp("이 피드백을 사용하면 왜곡 필터의 왜곡 레벨을 제어할 수 있습니다. 필터에는 MMAudioFilterDistortionShaker가 필요합니다.")]
	public class MMF_AudioFilterDistortion : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.SoundsColor; } }
		public override string RequiredTargetText => RequiredChannelText;
		#endif
		/// returns the duration of the feedback
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(Duration); } set { Duration = value; } }
		public override bool HasChannel => true;
		public override bool HasRandomness => true;

		[MMFInspectorGroup("Distortion Filter", true, 28)]
		/// the duration of the shake, in seconds
		[Tooltip("흔들림의 지속 시간(초)")]
		public float Duration = 2f;
		/// whether or not to reset shaker values after shake
		[Tooltip("whether or not to reset shaker values after shake")]
		public bool ResetShakerValuesAfterShake = true;
		/// whether or not to reset the target's values after shake
		[Tooltip("whether or not to reset the target's values after shake")]
		public bool ResetTargetValuesAfterShake = true;
		/// whether or not to add to the initial value
		[Tooltip("초기값에 추가할지 여부")]
		public bool RelativeDistortion = false;
		/// the curve used to animate the intensity value on
		[Tooltip("강도 값을 애니메이션하는 데 사용되는 곡선")]
		public AnimationCurve ShakeDistortion = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("the value to remap the curve's 0 to")]
		[Range(0f, 1f)]
		public float RemapDistortionZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("the value to remap the curve's 1 to")]
		[Range(0f, 1f)]
		public float RemapDistortionOne = 1f;

		/// <summary>
		/// Triggers the corresponding coroutine
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
            
			float intensityMultiplier = ComputeIntensity(feedbacksIntensity, position);
			float remapZero = 0f;
			float remapOne = 0f;
            
			if (!Timing.ConstantIntensity)
			{
				remapZero = RemapDistortionZero * intensityMultiplier;
				remapOne = RemapDistortionOne * intensityMultiplier;
			}
            
			MMAudioFilterDistortionShakeEvent.Trigger(ShakeDistortion, FeedbackDuration, remapZero, remapOne, RelativeDistortion,
				intensityMultiplier, ChannelData, ResetShakerValuesAfterShake, ResetTargetValuesAfterShake, NormalPlayDirection, ComputedTimescaleMode);
		}
        
		/// <summary>
		/// On stop we stop our transition
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
		{
			base.CustomStopFeedback(position, feedbacksIntensity);
            
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			MMAudioFilterDistortionShakeEvent.Trigger(ShakeDistortion, FeedbackDuration, 0f,0f, stop:true);
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
			MMAudioFilterDistortionShakeEvent.Trigger(ShakeDistortion, FeedbackDuration, 0f,0f, restore:true);
		}
	}
}