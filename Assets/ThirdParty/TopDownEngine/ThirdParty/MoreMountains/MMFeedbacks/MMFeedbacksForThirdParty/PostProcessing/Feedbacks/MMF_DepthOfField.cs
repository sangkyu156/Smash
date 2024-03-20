using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;

namespace MoreMountains.FeedbacksForThirdParty
{
    /// <summary>
    /// 이 피드백을 사용하면 시간이 지남에 따라 피사계 심도 초점 거리, 조리개 및 초점 거리를 제어할 수 있습니다.
    /// 장면에 PostProcessVolume이 있는 객체가 있어야 합니다.
    /// 심도가 활성화되고 MMDepthOfFieldShaker 구성 요소가 있는 경우.
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("이 피드백을 사용하면 시간이 지남에 따라 피사계 심도 초점 거리, 조리개 및 초점 거리를 제어할 수 있습니다. " +
"장면에 PostProcessVolume이 있는 객체가 있어야 합니다." +
"Depth of Field가 활성화되어 있고 MMDepthOfFieldShaker 구성 요소가 있습니다.")]
	#if MM_POSTPROCESSING
	[FeedbackPath("PostProcess/Depth Of Field")]
	#endif
	public class MMF_DepthOfField : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.PostProcessColor; } }
		public override string RequiredTargetText => RequiredChannelText;
		#endif

		/// the duration of this feedback is the duration of the shake
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(ShakeDuration); } set { ShakeDuration = value;  } }
		public override bool HasChannel => true;
		public override bool HasRandomness => true;

		[MMFInspectorGroup("Depth Of Field", true, 51)]
		/// the duration of the shake, in seconds
		[Tooltip("the duration of the shake, in seconds")]
		public float ShakeDuration = 2f;
		/// whether or not to add to the initial values
		[Tooltip("초기값에 추가할지 여부")]
		public bool RelativeValues = true;
		/// whether or not to reset shaker values after shake
		[Tooltip("흔들기 후 셰이커 값을 재설정할지 여부")]
		public bool ResetShakerValuesAfterShake = true;
		/// whether or not to reset the target's values after shake
		[Tooltip("흔들기 후 대상의 값을 재설정할지 여부")]
		public bool ResetTargetValuesAfterShake = true;

		[MMFInspectorGroup("Focus Distance", true, 52)]
		/// the curve used to animate the focus distance value on
		[Tooltip("초점 거리 값을 애니메이션하는 데 사용되는 곡선")]
		public AnimationCurve ShakeFocusDistance = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("the value to remap the curve's 0 to")]
		public float RemapFocusDistanceZero = 4f;
		/// the value to remap the curve's 1 to
		[Tooltip("the value to remap the curve's 1 to")]
		public float RemapFocusDistanceOne = 50f;

		[MMFInspectorGroup("Aperture", true, 53)]
		/// the curve used to animate the aperture value on
		[Tooltip("조리개 값에 애니메이션을 적용하는 데 사용되는 곡선")]
		public AnimationCurve ShakeAperture = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("the value to remap the curve's 0 to")]
		[Range(0.1f, 32f)]
		public float RemapApertureZero = 0.6f;
		/// the value to remap the curve's 1 to
		[Tooltip("the value to remap the curve's 1 to")]
		[Range(0.1f, 32f)]
		public float RemapApertureOne = 0.2f;

		[MMFInspectorGroup("Focal Length", true, 54)]
		/// the curve used to animate the focal length value on
		[Tooltip("초점 거리 값을 애니메이션화하는 데 사용되는 곡선")]
		public AnimationCurve ShakeFocalLength = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("the value to remap the curve's 0 to")]
		[Range(0f, 300f)]
		public float RemapFocalLengthZero = 27.5f;
		/// the value to remap the curve's 1 to
		[Tooltip("the value to remap the curve's 1 to")]
		[Range(0f, 300f)]
		public float RemapFocalLengthOne = 27.5f;

		/// <summary>
		/// Triggers a DoF shake
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
			MMDepthOfFieldShakeEvent.Trigger(ShakeFocusDistance, FeedbackDuration, RemapFocusDistanceZero, RemapFocusDistanceOne,
				ShakeAperture, RemapApertureZero, RemapApertureOne,
				ShakeFocalLength, RemapFocalLengthZero, RemapFocalLengthOne,
				RelativeValues, intensityMultiplier, ChannelData, ResetShakerValuesAfterShake, ResetTargetValuesAfterShake, NormalPlayDirection, ComputedTimescaleMode);
            
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
            
			MMDepthOfFieldShakeEvent.Trigger(ShakeFocusDistance, FeedbackDuration, RemapFocusDistanceZero, RemapFocusDistanceOne,
				ShakeAperture, RemapApertureZero, RemapApertureOne,
				ShakeFocalLength, RemapFocalLengthZero, RemapFocalLengthOne,
				RelativeValues, stop:true);
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
            
			MMDepthOfFieldShakeEvent.Trigger(ShakeFocusDistance, FeedbackDuration, RemapFocusDistanceZero, RemapFocusDistanceOne,
				ShakeAperture, RemapApertureZero, RemapApertureOne,
				ShakeFocalLength, RemapFocalLengthZero, RemapFocalLengthOne,
				RelativeValues, restore:true);
		}
	}
}