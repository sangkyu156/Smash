using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;

namespace MoreMountains.FeedbacksForThirdParty
{
    /// <summary>
    /// 이 피드백을 사용하면 시간 경과에 따른 노출 후 색상 그레이딩, 색조 변화, 채도 및 대비를 제어할 수 있습니다.
    /// 이를 위해서는 장면에 컬러 그레이딩이 활성화된 PostProcessVolume과 MMColorGradingShaker 구성 요소가 있는 객체가 있어야 합니다.
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackPath("PostProcess/Color Grading")]
	[FeedbackHelp("이 피드백을 사용하면 시간 경과에 따른 노출 후 색상 그레이딩, 색조 변화, 채도 및 대비를 제어할 수 있습니다. " +
                  "이를 위해서는 장면에 컬러 그레이딩이 활성화된 PostProcessVolume과 MMColorGradingShaker 구성 요소가 있는 객체가 있어야 합니다.")]
	public class MMFeedbackColorGrading : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.PostProcessColor; } }
		#endif

		[Header("Color Grading")]
		/// the channel to emit on
		[Tooltip("the channel to emit on")]
		public int Channel = 0;
		/// the duration of the shake, in seconds
		[Tooltip("흔들림의 지속 시간(초)")]
		public float ShakeDuration = 1f;
		/// whether or not to add to the initial intensity
		[Tooltip("초기 강도에 추가할지 여부")]
		public bool RelativeIntensity = true;
		/// whether or not to reset shaker values after shake
		[Tooltip("흔들기 후 셰이커 값을 재설정할지 여부")]
		public bool ResetShakerValuesAfterShake = true;
		/// whether or not to reset the target's values after shake
		[Tooltip("흔들기 후 대상의 값을 재설정할지 여부")]
		public bool ResetTargetValuesAfterShake = true;

		[Header("Post Exposure")]
		/// the curve used to animate the focus distance value on
		[Tooltip("the curve used to animate the focus distance value on")]
		public AnimationCurve ShakePostExposure = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("the value to remap the curve's 0 to")]
		public float RemapPostExposureZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("the value to remap the curve's 1 to")]
		public float RemapPostExposureOne = 1f;

		[Header("Hue Shift")]
		/// the curve used to animate the aperture value on
		[Tooltip("the curve used to animate the aperture value on")]
		public AnimationCurve ShakeHueShift = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("the value to remap the curve's 0 to")]
		[Range(-180f, 180f)]
		public float RemapHueShiftZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("the value to remap the curve's 1 to")]
		[Range(-180f, 180f)]
		public float RemapHueShiftOne = 180f;

		[Header("Saturation")]
		/// the curve used to animate the focal length value on
		[Tooltip("the curve used to animate the focal length value on")]
		public AnimationCurve ShakeSaturation = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("the value to remap the curve's 0 to")]
		[Range(-100f, 100f)]
		public float RemapSaturationZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("the value to remap the curve's 1 to")]
		[Range(-100f, 100f)]
		public float RemapSaturationOne = 100f;

		[Header("Contrast")]
		/// the curve used to animate the focal length value on
		[Tooltip("the curve used to animate the focal length value on")]
		public AnimationCurve ShakeContrast = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("the value to remap the curve's 0 to")]
		[Range(-100f, 100f)]
		public float RemapContrastZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("the value to remap the curve's 1 to")]
		[Range(-100f, 100f)]
		public float RemapContrastOne = 100f;

		/// the duration of this feedback is the duration of the shake
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(ShakeDuration); }  set { ShakeDuration = value;  } }

		/// <summary>
		/// Triggers a color grading shake
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
            
			float intensityMultiplier = Timing.ConstantIntensity ? 1f : feedbacksIntensity;
			MMColorGradingShakeEvent.Trigger(ShakePostExposure, RemapPostExposureZero, RemapPostExposureOne, 
				ShakeHueShift, RemapHueShiftZero, RemapHueShiftOne, 
				ShakeSaturation, RemapSaturationZero, RemapSaturationOne, 
				ShakeContrast, RemapContrastZero, RemapContrastOne, 
				FeedbackDuration,                     
				RelativeIntensity, intensityMultiplier, ChannelData(Channel), ResetShakerValuesAfterShake, ResetTargetValuesAfterShake, NormalPlayDirection, Timing.TimescaleMode);
            
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
            
			MMColorGradingShakeEvent.Trigger(ShakePostExposure, RemapPostExposureZero, RemapPostExposureOne, 
				ShakeHueShift, RemapHueShiftZero, RemapHueShiftOne, 
				ShakeSaturation, RemapSaturationZero, RemapSaturationOne, 
				ShakeContrast, RemapContrastZero, RemapContrastOne, 
				FeedbackDuration,                     
				stop:true);
            
		}
	}
}