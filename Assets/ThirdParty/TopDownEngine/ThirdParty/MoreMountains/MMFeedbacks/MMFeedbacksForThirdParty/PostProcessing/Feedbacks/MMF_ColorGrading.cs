using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;

namespace MoreMountains.FeedbacksForThirdParty
{
    /// <summary>
    /// 이 피드백을 사용하면 시간 경과에 따른 노출 후 색상 그레이딩, 색조 변화, 채도 및 대비를 제어할 수 있습니다.
    /// 장면에 PostProcessVolume이 있는 객체가 있어야 합니다.
    /// 컬러 그레이딩이 활성화되어 있고 MMColorGradingShaker 구성 요소가 있습니다.
    /// </summary>
    [AddComponentMenu("")]
	#if MM_POSTPROCESSING
	[FeedbackPath("PostProcess/Color Grading")]
	#endif
	[FeedbackHelp("이 피드백을 사용하면 시간 경과에 따른 노출 후 색상 그레이딩, 색조 변화, 채도 및 대비를 제어할 수 있습니다. " +
"장면에 PostProcessVolume이 있는 객체가 있어야 합니다." +
"컬러 그레이딩이 활성화되어 있고 MMColorGradingShaker 구성 요소가 있습니다.")]
	public class MMF_ColorGrading : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.PostProcessColor; } }
		public override string RequiredTargetText => RequiredChannelText;
		#endif

		/// the duration of this feedback is the duration of the shake
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(ShakeDuration); }  set { ShakeDuration = value;  } }
		public override bool HasChannel => true;
		public override bool HasRandomness => true;

		[MMFInspectorGroup("Color Grading", true, 46)]
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

		[MMFInspectorGroup("Post Exposure", true, 47)]
		/// the curve used to animate the focus distance value on
		[Tooltip("초점 거리 값을 애니메이션하는 데 사용되는 곡선")]
		public AnimationCurve ShakePostExposure = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("the value to remap the curve's 0 to")]
		public float RemapPostExposureZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("the value to remap the curve's 1 to")]
		public float RemapPostExposureOne = 1f;

		[MMFInspectorGroup("Hue Shift", true, 48)]
		/// the curve used to animate the aperture value on
		[Tooltip("조리개 값에 애니메이션을 적용하는 데 사용되는 곡선")]
		public AnimationCurve ShakeHueShift = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("the value to remap the curve's 0 to")]
		[Range(-180f, 180f)]
		public float RemapHueShiftZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("the value to remap the curve's 1 to")]
		[Range(-180f, 180f)]
		public float RemapHueShiftOne = 180f;

		[MMFInspectorGroup("Saturation", true, 49)]
		/// the curve used to animate the focal length value on
		[Tooltip("초점 거리 값을 애니메이션화하는 데 사용되는 곡선")]
		public AnimationCurve ShakeSaturation = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("the value to remap the curve's 0 to")]
		[Range(-100f, 100f)]
		public float RemapSaturationZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("the value to remap the curve's 1 to")]
		[Range(-100f, 100f)]
		public float RemapSaturationOne = 100f;

		[MMFInspectorGroup("Contrast", true, 50)]
		/// the curve used to animate the focal length value on
		[Tooltip("초점 거리 값을 애니메이션화하는 데 사용되는 곡선")]
		public AnimationCurve ShakeContrast = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("the value to remap the curve's 0 to")]
		[Range(-100f, 100f)]
		public float RemapContrastZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("the value to remap the curve's 1 to")]
		[Range(-100f, 100f)]
		public float RemapContrastOne = 100f;

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
            
			float intensityMultiplier = ComputeIntensity(feedbacksIntensity, position);
			MMColorGradingShakeEvent.Trigger(ShakePostExposure, RemapPostExposureZero, RemapPostExposureOne, 
				ShakeHueShift, RemapHueShiftZero, RemapHueShiftOne, 
				ShakeSaturation, RemapSaturationZero, RemapSaturationOne, 
				ShakeContrast, RemapContrastZero, RemapContrastOne, 
				FeedbackDuration,                     
				RelativeIntensity, intensityMultiplier, ChannelData, ResetShakerValuesAfterShake, ResetTargetValuesAfterShake, NormalPlayDirection, ComputedTimescaleMode);
            
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

		/// <summary>
		/// On restore, we put our object back at its initial position
		/// </summary>
		protected override void CustomRestoreInitialValues()
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
            
			MMColorGradingShakeEvent.Trigger(ShakePostExposure, RemapPostExposureZero, RemapPostExposureOne, 
				ShakeHueShift, RemapHueShiftZero, RemapHueShiftOne, 
				ShakeSaturation, RemapSaturationZero, RemapSaturationOne, 
				ShakeContrast, RemapContrastZero, RemapContrastOne, 
				FeedbackDuration,                     
				restore:true);
		}
	}
}