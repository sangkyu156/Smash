using System.Collections;
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
	[FeedbackPath("PostProcess/Lens Distortion URP")]
	[FeedbackHelp("이 피드백을 통해 시간 경과에 따른 URP 렌즈 왜곡 강도를 제어할 수 있습니다. " +
"장면에 볼륨이 있는 객체가 있어야 합니다." +
"렌즈 왜곡이 활성화되고 MMLensDistortionShaker_URP 구성 요소가 포함되어 있습니다.")]
	public class MMFeedbackLensDistortion_URP : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.PostProcessColor; } }
		#endif

		[Header("Lens Distortion")]
		/// the channel to emit on
		[Tooltip("the channel to emit on")]
		public int Channel = 0;
		/// the duration of the shake in seconds
		[Tooltip("흔들리는 시간(초)")]
		public float Duration = 0.8f;
		/// whether or not to reset shaker values after shake
		[Tooltip("흔들기 후 셰이커 값을 재설정할지 여부")]
		public bool ResetShakerValuesAfterShake = true;
		/// whether or not to reset the target's values after shake
		[Tooltip("흔들기 후 대상의 값을 재설정할지 여부")]
		public bool ResetTargetValuesAfterShake = true;

		[Header("Intensity")]
		/// whether or not to add to the initial intensity value
		[Tooltip("초기 강도 값에 추가할지 여부")]
		public bool RelativeIntensity = false;
		/// the curve to animate the intensity on
		[Tooltip("강도에 애니메이션을 적용하는 곡선")]
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
		[Range(-100f, 100f)]
		public float RemapIntensityZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("the value to remap the curve's 1 to")]
		[Range(-100f, 100f)]
		public float RemapIntensityOne = 0.5f;

		/// the duration of this feedback is the duration of the shake
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(Duration); } set { Duration = value; } }

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
            
			float intensityMultiplier = Timing.ConstantIntensity ? 1f : feedbacksIntensity;
			MMLensDistortionShakeEvent_URP.Trigger(Intensity, FeedbackDuration, RemapIntensityZero, RemapIntensityOne, RelativeIntensity, intensityMultiplier,
				ChannelData(Channel), ResetShakerValuesAfterShake, ResetTargetValuesAfterShake, NormalPlayDirection, Timing.TimescaleMode);
            
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
            
			MMLensDistortionShakeEvent_URP.Trigger(Intensity, FeedbackDuration, RemapIntensityZero, RemapIntensityOne, RelativeIntensity, stop:true, channelData:ChannelData(Channel));
            
		}
	}
}