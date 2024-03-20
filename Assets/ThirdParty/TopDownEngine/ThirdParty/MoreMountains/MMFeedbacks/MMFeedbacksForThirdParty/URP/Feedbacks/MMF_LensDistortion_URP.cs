﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;

namespace MoreMountains.FeedbacksForThirdParty
{
    /// <summary>
    /// 이 피드백을 통해 시간 경과에 따른 URP 렌즈 왜곡 강도를 제어할 수 있습니다.
    /// 장면에 볼륨이 있는 객체가 있어야 합니다.
    /// 렌즈 왜곡이 활성화되고 MMLensDistortionShaker_URP 구성 요소가 있는 경우.
    /// </summary>
    [AddComponentMenu("")]
	#if MM_URP
	[FeedbackPath("PostProcess/Lens Distortion URP")]
	#endif
	[FeedbackHelp("이 피드백을 통해 시간 경과에 따른 URP 렌즈 왜곡 강도를 제어할 수 있습니다. " +
"장면에 볼륨이 있는 객체가 있어야 합니다." +
"렌즈 왜곡이 활성화되고 MMLensDistortionShaker_URP 구성 요소가 포함되어 있습니다.")]
	public class MMF_LensDistortion_URP : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.PostProcessColor; } }
		#endif

		/// the duration of this feedback is the duration of the shake
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(Duration); } set { Duration = value; } }
		public override bool HasChannel => true;
		public override bool HasRandomness => true;

		[MMFInspectorGroup("Lens Distortion", true, 22)]
		/// the duration of the shake in seconds
		[Tooltip("the duration of the shake in seconds")]
		public float Duration = 0.8f;
		/// whether or not to reset shaker values after shake
		[Tooltip("whether or not to reset shaker values after shake")]
		public bool ResetShakerValuesAfterShake = true;
		/// whether or not to reset the target's values after shake
		[Tooltip("whether or not to reset the target's values after shake")]
		public bool ResetTargetValuesAfterShake = true;

		[MMFInspectorGroup("Intensity", true, 23)]
		/// whether or not to add to the initial intensity value
		[Tooltip("whether or not to add to the initial intensity value")]
		public bool RelativeIntensity = false;
		/// the curve to animate the intensity on
		[Tooltip("the curve to animate the intensity on")]
		public AnimationCurve Intensity = new AnimationCurve(new Keyframe(0, 0),
			new Keyframe(0.2f, 1),
			new Keyframe(0.25f, -1),
			new Keyframe(0.35f, 0.7f),
			new Keyframe(0.4f, -0.7f),
			new Keyframe(0.6f, 0.3f),
			new Keyframe(0.65f, -0.3f),
			new Keyframe(0.8f, 0.1f),
			new Keyframe(0.85f, -0.1f),
			new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("the value to remap the curve's 0 to")]
		[Range(-1f, 1f)]
		public float RemapIntensityZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("the value to remap the curve's 1 to")]
		[Range(-1f, 1f)]
		public float RemapIntensityOne = 0.5f;

		/// <summary>
		/// Triggers a lens distortion shake
		/// </summary>
		/// <param name="position"></param>
		/// <param name="attenuation"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
            
			float intensityMultiplier = ComputeIntensity(feedbacksIntensity, position);
			MMLensDistortionShakeEvent_URP.Trigger(Intensity, FeedbackDuration, RemapIntensityZero, RemapIntensityOne, RelativeIntensity, intensityMultiplier,
				ChannelData, ResetShakerValuesAfterShake, ResetTargetValuesAfterShake, NormalPlayDirection, ComputedTimescaleMode);
            
		}
        
		/// <summary>
		/// On stop we stop our transition
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			base.CustomStopFeedback(position, feedbacksIntensity);
			MMLensDistortionShakeEvent_URP.Trigger(Intensity, FeedbackDuration, RemapIntensityZero, RemapIntensityOne, RelativeIntensity, stop:true, channelData: ChannelData);
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
			
			MMLensDistortionShakeEvent_URP.Trigger(Intensity, FeedbackDuration, RemapIntensityZero, RemapIntensityOne, RelativeIntensity, restore:true, channelData: ChannelData);
		}
	}
}