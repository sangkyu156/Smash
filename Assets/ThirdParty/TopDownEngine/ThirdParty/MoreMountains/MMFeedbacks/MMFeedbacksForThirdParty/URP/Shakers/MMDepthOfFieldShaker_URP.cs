using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
#if MM_URP
using UnityEngine.Rendering.Universal;
#endif
using MoreMountains.Feedbacks;
using MoreMountains.Tools;

namespace MoreMountains.FeedbacksForThirdParty
{
    /// <summary>
    /// 이 클래스를 URP 심도 사후 처리를 사용하여 카메라에 추가하면 이벤트를 가져와 해당 값을 "흔들" 수 있습니다.
    /// </summary>
#if MM_URP
    [RequireComponent(typeof(Volume))]
	#endif
	[AddComponentMenu("More Mountains/Feedbacks/Shakers/PostProcessing/MMDepthOfFieldShaker_URP")]
	public class MMDepthOfFieldShaker_URP : MMShaker
	{
		/// whether or not to add to the initial value
		public bool RelativeValues = true;

		[MMInspectorGroup("Focus Distance", true, 51)]
		/// the curve used to animate the focus distance value on
		[Tooltip("초점 거리 값을 애니메이션하는 데 사용되는 곡선")]
		public AnimationCurve ShakeFocusDistance = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("곡선의 0을 다시 매핑할 값")]
		public float RemapFocusDistanceZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("곡선의 1을 다시 매핑할 값")]
		public float RemapFocusDistanceOne = 3f;

		[MMInspectorGroup("Aperture", true, 52)]
		/// the curve used to animate the aperture value on
		[Tooltip("조리개 값에 애니메이션을 적용하는 데 사용되는 곡선")]
		public AnimationCurve ShakeAperture = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Range(0.1f, 32f)]
		[Tooltip("곡선의 0을 다시 매핑할 값")]
		public float RemapApertureZero = 0f;
		/// the value to remap the curve's 1 to
		[Range(0.1f, 32f)]
		[Tooltip("곡선의 1을 다시 매핑할 값")]
		public float RemapApertureOne = 0f;

		[MMInspectorGroup("Focal Length", true, 53)]
		/// the curve used to animate the focal length value on
		[Tooltip("초점 거리 값을 애니메이션화하는 데 사용되는 곡선")]
		public AnimationCurve ShakeFocalLength = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("곡선의 0을 다시 매핑할 값")]
		[Range(0f, 300f)]
		public float RemapFocalLengthZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("곡선의 1을 다시 매핑할 값")]
		[Range(0f, 300f)]
		public float RemapFocalLengthOne = 0f;

		#if MM_URP
		protected Volume _volume;
		protected DepthOfField _depthOfField;
		protected float _initialFocusDistance;
		protected float _initialAperture;
		protected float _initialFocalLength;
		protected float _originalShakeDuration;
		protected bool _originalRelativeValues;
		protected AnimationCurve _originalShakeFocusDistance;
		protected float _originalRemapFocusDistanceZero;
		protected float _originalRemapFocusDistanceOne;
		protected AnimationCurve _originalShakeAperture;
		protected float _originalRemapApertureZero;
		protected float _originalRemapApertureOne;
		protected AnimationCurve _originalShakeFocalLength;
		protected float _originalRemapFocalLengthZero;
		protected float _originalRemapFocalLengthOne;

		/// <summary>
		/// On init we initialize our values
		/// </summary>
		protected override void Initialization()
		{
			base.Initialization();
			_volume = this.gameObject.GetComponent<Volume>();
			_volume.profile.TryGet(out _depthOfField);
		}

		/// <summary>
		/// Shakes values over time
		/// </summary>
		protected override void Shake()
		{
			float newFocusDistance = ShakeFloat(ShakeFocusDistance, RemapFocusDistanceZero, RemapFocusDistanceOne, RelativeValues, _initialFocusDistance);
			_depthOfField.focusDistance.Override(newFocusDistance);
			float newAperture = ShakeFloat(ShakeAperture, RemapApertureZero, RemapApertureOne, RelativeValues, _initialAperture);
			_depthOfField.aperture.Override(newAperture);
			float newFocalLength = ShakeFloat(ShakeFocalLength, RemapFocalLengthZero, RemapFocalLengthOne, RelativeValues, _initialFocalLength);
			_depthOfField.focalLength.Override(newFocalLength);
		}

		/// <summary>
		/// When that shaker gets added, we initialize its shake duration
		/// </summary>
		protected virtual void Reset()
		{
			ShakeDuration = 2f;
		}

		/// <summary>
		/// Collects initial values on the target
		/// </summary>
		protected override void GrabInitialValues()
		{
			_initialFocusDistance = _depthOfField.focusDistance.value;
			_initialAperture = _depthOfField.aperture.value;
			_initialFocalLength = _depthOfField.focalLength.value;
		}

		/// <summary>
		/// When we get the appropriate event, we trigger a shake
		/// </summary>
		/// <param name="intensity"></param>
		/// <param name="duration"></param>
		/// <param name="amplitude"></param>
		/// <param name="relativeIntensity"></param>
		/// <param name="attenuation"></param>
		/// <param name="channel"></param>
		public virtual void OnDepthOfFieldShakeEvent(AnimationCurve focusDistance, float duration, float remapFocusDistanceMin, float remapFocusDistanceMax,
			AnimationCurve aperture, float remapApertureMin, float remapApertureMax,
			AnimationCurve focalLength, float remapFocalLengthMin, float remapFocalLengthMax,
			bool relativeValues = false,
			float attenuation = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, 
			bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false)
		{
			if (!CheckEventAllowed(channelData) || (!Interruptible && Shaking))
			{
				return;
			}
	            
			if (stop)
			{
				Stop();
				return;
			}

			if (restore)
			{
				ResetTargetValues();
				return;
			}

			_resetShakerValuesAfterShake = resetShakerValuesAfterShake;
			_resetTargetValuesAfterShake = resetTargetValuesAfterShake;

			if (resetShakerValuesAfterShake)
			{
				_originalShakeDuration = ShakeDuration;
				_originalRelativeValues = RelativeValues;
				_originalShakeFocusDistance = ShakeFocusDistance;
				_originalRemapFocusDistanceZero = RemapFocusDistanceZero;
				_originalRemapFocusDistanceOne = RemapFocusDistanceOne;
				_originalShakeAperture = ShakeAperture;
				_originalRemapApertureZero = RemapApertureZero;
				_originalRemapApertureOne = RemapApertureOne;
				_originalShakeFocalLength = ShakeFocalLength;
				_originalRemapFocalLengthZero = RemapFocalLengthZero;
				_originalRemapFocalLengthOne = RemapFocalLengthOne;
			}

			if (!OnlyUseShakerValues)
			{
				TimescaleMode = timescaleMode;
				ShakeDuration = duration;
				RelativeValues = relativeValues;
				ShakeFocusDistance = focusDistance;
				RemapFocusDistanceZero = remapFocusDistanceMin;
				RemapFocusDistanceOne = remapFocusDistanceMax;
				ShakeAperture = aperture;
				RemapApertureZero = remapApertureMin;
				RemapApertureOne = remapApertureMax;
				ShakeFocalLength = focalLength;
				RemapFocalLengthZero = remapFocalLengthMin;
				RemapFocalLengthOne = remapFocalLengthMax;
				ForwardDirection = forwardDirection;
			}

			Play();
		}

		/// <summary>
		/// Resets the target's values
		/// </summary>
		protected override void ResetTargetValues()
		{
			base.ResetTargetValues();
			_depthOfField.focusDistance.Override(_initialFocusDistance);
			_depthOfField.aperture.Override(_initialAperture);
			_depthOfField.focalLength.Override(_initialFocalLength);
		}

		/// <summary>
		/// Resets the shaker's values
		/// </summary>
		protected override void ResetShakerValues()
		{
			base.ResetShakerValues();
			ShakeDuration = _originalShakeDuration;
			RelativeValues = _originalRelativeValues;
			ShakeFocusDistance = _originalShakeFocusDistance;
			RemapFocusDistanceZero = _originalRemapFocusDistanceZero;
			RemapFocusDistanceOne = _originalRemapFocusDistanceOne;
			ShakeAperture = _originalShakeAperture;
			RemapApertureZero = _originalRemapApertureZero;
			RemapApertureOne = _originalRemapApertureOne;
			ShakeFocalLength = _originalShakeFocalLength;
			RemapFocalLengthZero = _originalRemapFocalLengthZero;
			RemapFocalLengthOne = _originalRemapFocalLengthOne;
		}

		/// <summary>
		/// Starts listening for events
		/// </summary>
		public override void StartListening()
		{
			base.StartListening();
			MMDepthOfFieldShakeEvent_URP.Register(OnDepthOfFieldShakeEvent);
		}

		/// <summary>
		/// Stops listening for events
		/// </summary>
		public override void StopListening()
		{
			base.StopListening();
			MMDepthOfFieldShakeEvent_URP.Unregister(OnDepthOfFieldShakeEvent);
		}
		#endif
	}

	/// <summary>
	/// An event used to trigger vignette shakes
	/// </summary>
	public struct MMDepthOfFieldShakeEvent_URP
	{
		static private event Delegate OnEvent;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
		static public void Register(Delegate callback) { OnEvent += callback; }
		static public void Unregister(Delegate callback) { OnEvent -= callback; }

		public delegate void Delegate(AnimationCurve focusDistance, float duration, float remapFocusDistanceMin, float remapFocusDistanceMax,
			AnimationCurve aperture, float remapApertureMin, float remapApertureMax,
			AnimationCurve focalLength, float remapFocalLengthMin, float remapFocalLengthMax,
			bool relativeValues = false,
			float attenuation = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, 
			bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false);

		static public void Trigger(AnimationCurve focusDistance, float duration, float remapFocusDistanceMin, float remapFocusDistanceMax,
			AnimationCurve aperture, float remapApertureMin, float remapApertureMax,
			AnimationCurve focalLength, float remapFocalLengthMin, float remapFocalLengthMax,
			bool relativeValues = false,
			float attenuation = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, 
			bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false)
		{
			OnEvent?.Invoke(focusDistance, duration, remapFocusDistanceMin, remapFocusDistanceMax,
				aperture, remapApertureMin, remapApertureMax,
				focalLength, remapFocalLengthMin, remapFocalLengthMax, relativeValues,
				attenuation, channelData, resetShakerValuesAfterShake, resetTargetValuesAfterShake, forwardDirection, timescaleMode, stop, restore);
		}
	}
}