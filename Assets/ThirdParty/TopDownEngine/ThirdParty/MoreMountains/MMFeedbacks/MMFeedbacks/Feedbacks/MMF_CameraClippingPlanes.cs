using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 이 피드백을 사용하면 시간이 지남에 따라 카메라의 클리핑 평면을 제어할 수 있습니다. 카메라에 MMCameraClippingPlanesShaker가 필요합니다.
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackPath("Camera/Clipping Planes")]
	[FeedbackHelp("이 피드백을 사용하면 시간이 지남에 따라 카메라의 클리핑 평면을 제어할 수 있습니다. 카메라에 MMCameraClippingPlanesShaker가 필요합니다.")]
	public class MMF_CameraClippingPlanes : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.CameraColor; } }
		public override string RequiredTargetText => RequiredChannelText;
		#endif
		/// returns the duration of the feedback
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(Duration); } set { Duration = value; } }
		public override bool HasChannel => true;
		public override bool HasRandomness => true;

		[MMFInspectorGroup("Clipping Planes", true, 52)]
		/// the duration of the shake, in seconds
		[Tooltip("the duration of the shake, in seconds")]
		public float Duration = 2f;
		/// whether or not to reset shaker values after shake
		[Tooltip("whether or not to reset shaker values after shake")]
		public bool ResetShakerValuesAfterShake = true;
		/// whether or not to reset the target's values after shake
		[Tooltip("whether or not to reset the target's values after shake")]
		public bool ResetTargetValuesAfterShake = true;
		/// whether or not to add to the initial value
		[Tooltip("초기값에 추가할지 여부")]
		public bool RelativeClippingPlanes = false;

		[MMFInspectorGroup("Near Plane", true, 53)]
		/// the curve used to animate the intensity value on
		[Tooltip("강도 값을 애니메이션하는 데 사용되는 곡선")]
		public AnimationCurve ShakeNear = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to        
		[Tooltip("the value to remap the curve's 0 to")]
		public float RemapNearZero = 0.01f;
		/// the value to remap the curve's 1 to        
		[Tooltip("the value to remap the curve's 1 to")]
		public float RemapNearOne = 6.25f;

		[MMFInspectorGroup("Far Plane", true, 54)]
		/// the curve used to animate the intensity value on
		[Tooltip("강도 값을 애니메이션하는 데 사용되는 곡선")]
		public AnimationCurve ShakeFar = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to        
		[Tooltip("the value to remap the curve's 0 to")]
		public float RemapFarZero = 1000f;
		/// the value to remap the curve's 1 to        
		[Tooltip("the value to remap the curve's 1 to")]
		public float RemapFarOne = 5000f;

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

			feedbacksIntensity = ComputeIntensity(feedbacksIntensity, position);
			
			MMCameraClippingPlanesShakeEvent.Trigger(ShakeNear, FeedbackDuration, RemapNearZero, RemapNearOne, 
				ShakeFar, RemapFarZero, RemapFarOne,
				RelativeClippingPlanes,
				feedbacksIntensity, ChannelData, ResetShakerValuesAfterShake, ResetTargetValuesAfterShake, NormalPlayDirection, ComputedTimescaleMode);
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
			MMCameraClippingPlanesShakeEvent.Trigger(ShakeNear, FeedbackDuration, RemapNearZero, RemapNearOne, 
				ShakeFar, RemapFarZero, RemapFarOne, stop: true);
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
			MMCameraClippingPlanesShakeEvent.Trigger(ShakeNear, FeedbackDuration, RemapNearZero, RemapNearOne, 
				ShakeFar, RemapFarZero, RemapFarOne, restore: true);
		}
	}
}