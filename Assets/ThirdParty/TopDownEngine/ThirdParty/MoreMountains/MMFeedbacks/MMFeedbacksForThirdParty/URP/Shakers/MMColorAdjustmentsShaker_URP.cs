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
    /// URP 색상 조정 사후 처리를 사용하여 이 클래스를 카메라에 추가하면 이벤트를 가져와 해당 값을 "흔들" 수 있습니다.
    /// </summary>
#if MM_URP
    [RequireComponent(typeof(Volume))]
	#endif
	[AddComponentMenu("More Mountains/Feedbacks/Shakers/PostProcessing/MMColorAdjustmentsShaker_URP")]
	public class MMColorAdjustmentsShaker_URP : MMShaker
	{
		/// whether or not to add to the initial value
		public bool RelativeValues = true;

		[MMInspectorGroup("Post Exposure", true, 43)]
		/// the curve used to animate the focus distance value on
		[Tooltip("초점 거리 값을 애니메이션하는 데 사용되는 곡선")]
		public AnimationCurve ShakePostExposure = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("곡선의 0을 다시 매핑할 값")]
		public float RemapPostExposureZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("곡선의 1을 다시 매핑할 값")]
		public float RemapPostExposureOne = 1f;

		[MMInspectorGroup("Hue Shift", true, 44)]
		/// the curve used to animate the aperture value on
		[Tooltip("조리개 값에 애니메이션을 적용하는 데 사용되는 곡선")]
		public AnimationCurve ShakeHueShift = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Range(-180f, 180f)]
		[Tooltip("곡선의 0을 다시 매핑할 값")]
		public float RemapHueShiftZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("곡선의 1을 다시 매핑할 값")]
		[Range(-180f, 180f)]
		public float RemapHueShiftOne = 180f;

		[MMInspectorGroup("Saturation", true, 45)]
		/// the curve used to animate the focal length value on
		[Tooltip("초점 거리 값을 애니메이션화하는 데 사용되는 곡선")]
		public AnimationCurve ShakeSaturation = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("곡선의 0을 다시 매핑할 값")]
		[Range(-100f, 100f)]
		public float RemapSaturationZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("곡선의 1을 다시 매핑할 값")]
		[Range(-100f, 100f)]
		public float RemapSaturationOne = 100f;

		[MMInspectorGroup("Contrast", true, 47)]
		/// the curve used to animate the focal length value on
		[Tooltip("초점 거리 값을 애니메이션화하는 데 사용되는 곡선")]
		public AnimationCurve ShakeContrast = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("곡선의 0을 다시 매핑할 값")]
		[Range(-100f, 100f)]
		public float RemapContrastZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("곡선의 1을 다시 매핑할 값")]
		[Range(-100f, 100f)]
		public float RemapContrastOne = 100f;
        
		public enum ColorFilterModes { None, Gradient, Interpolate }
 
		[MMInspectorGroup("Color Filter", true, 48)]
		/// the color filter mode to work with (none, over a gradient, or interpolate to a destination color
		[Tooltip("작업할 색상 필터 모드(없음, 그라디언트 또는 대상 색상으로 보간)")]
		public ColorFilterModes ColorFilterMode = ColorFilterModes.None;
		/// the gradient over which to modify the color filter
		[Tooltip("색상 필터를 수정할 그라데이션")]
		[MMFEnumCondition("ColorFilterMode", (int)ColorFilterModes.Gradient)]
		[GradientUsage(true)]
		public Gradient ColorFilterGradient;
		/// the destination color to match when in Interpolate mode
		[Tooltip("보간 모드에 있을 때 일치시킬 대상 색상")]
		[MMFEnumCondition("ColorFilterMode", (int) ColorFilterModes.Interpolate)]
		public Color ColorFilterDestination = Color.yellow;
		/// the curve over which to interpolate the color filter
		[Tooltip("컬러 필터를 보간할 곡선")]
		[MMFEnumCondition("ColorFilterMode", (int) ColorFilterModes.Interpolate)]
		public AnimationCurve ColorFilterCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
        
		#if MM_URP
		protected Volume _volume;
		protected ColorAdjustments _colorAdjustments;
		protected float _initialPostExposure;
		protected float _initialHueShift;
		protected float _initialSaturation;
		protected float _initialContrast;
		protected Color _initialColorFilterColor;
		protected float _originalShakeDuration;
		protected bool _originalRelativeValues;
		protected AnimationCurve _originalShakePostExposure;
		protected float _originalRemapPostExposureZero;
		protected float _originalRemapPostExposureOne;
		protected AnimationCurve _originalShakeHueShift;
		protected float _originalRemapHueShiftZero;
		protected float _originalRemapHueShiftOne;
		protected AnimationCurve _originalShakeSaturation;
		protected float _originalRemapSaturationZero;
		protected float _originalRemapSaturationOne;
		protected AnimationCurve _originalShakeContrast;
		protected float _originalRemapContrastZero;
		protected float _originalRemapContrastOne;
		protected ColorFilterModes _originalColorFilterMode;
		protected Gradient _originalColorFilterGradient;
		protected Color _originalColorFilterDestination;
		protected AnimationCurve _originalColorFilterCurve;  

		/// <summary>
		/// On init we initialize our values
		/// </summary>
		protected override void Initialization()
		{
			base.Initialization();
			_volume = this.gameObject.GetComponent<Volume>();
			_volume.profile.TryGet(out _colorAdjustments);
		}

		/// <summary>
		/// When that shaker gets added, we initialize its shake duration
		/// </summary>
		protected virtual void Reset()
		{
			ShakeDuration = 0.8f;
		}

		/// <summary>
		/// Shakes values over time
		/// </summary>
		protected override void Shake()
		{
			float newPostExposure = ShakeFloat(ShakePostExposure, RemapPostExposureZero, RemapPostExposureOne, RelativeValues, _initialPostExposure);
			_colorAdjustments.postExposure.Override(newPostExposure);
			float newHueShift = ShakeFloat(ShakeHueShift, RemapHueShiftZero, RemapHueShiftOne, RelativeValues, _initialHueShift);
			_colorAdjustments.hueShift.Override(newHueShift);
			float newSaturation = ShakeFloat(ShakeSaturation, RemapSaturationZero, RemapSaturationOne, RelativeValues, _initialSaturation);
			_colorAdjustments.saturation.Override(newSaturation);
			float newContrast = ShakeFloat(ShakeContrast, RemapContrastZero, RemapContrastOne, RelativeValues, _initialContrast);
			_colorAdjustments.contrast.Override(newContrast);

			_remappedTimeSinceStart = MMFeedbacksHelpers.Remap(Time.time - _shakeStartedTimestamp, 0f, ShakeDuration, 0f, 1f);
	            
			if (ColorFilterMode == ColorFilterModes.Gradient)
			{
				_colorAdjustments.colorFilter.Override(ColorFilterGradient.Evaluate(_remappedTimeSinceStart));    
			}
			else if (ColorFilterMode == ColorFilterModes.Interpolate)
			{
				float factor = ColorFilterCurve.Evaluate(_remappedTimeSinceStart);
				_colorAdjustments.colorFilter.Override(Color.LerpUnclamped(_initialColorFilterColor, ColorFilterDestination, factor));
			}
		}

		/// <summary>
		/// Collects initial values on the target
		/// </summary>
		protected override void GrabInitialValues()
		{
			_initialPostExposure = _colorAdjustments.postExposure.value;
			_initialHueShift = _colorAdjustments.hueShift.value;
			_initialSaturation = _colorAdjustments.saturation.value;
			_initialContrast = _colorAdjustments.contrast.value;
			_initialColorFilterColor = _colorAdjustments.colorFilter.value;
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
		public virtual void OnMMColorGradingShakeEvent(AnimationCurve shakePostExposure, float remapPostExposureZero, float remapPostExposureOne,
			AnimationCurve shakeHueShift, float remapHueShiftZero, float remapHueShiftOne,
			AnimationCurve shakeSaturation, float remapSaturationZero, float remapSaturationOne,
			AnimationCurve shakeContrast, float remapContrastZero, float remapContrastOne,
			ColorFilterModes colorFilterMode, Gradient colorFilterGradient, Color colorFilterDestination,AnimationCurve colorFilterCurve,  
			float duration, bool relativeValues = false,
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
				_originalShakePostExposure = ShakePostExposure;
				_originalRemapPostExposureZero = RemapPostExposureZero;
				_originalRemapPostExposureOne = RemapPostExposureOne;
				_originalShakeHueShift = ShakeHueShift;
				_originalRemapHueShiftZero = RemapHueShiftZero;
				_originalRemapHueShiftOne = RemapHueShiftOne;
				_originalShakeSaturation = ShakeSaturation;
				_originalRemapSaturationZero = RemapSaturationZero;
				_originalRemapSaturationOne = RemapSaturationOne;
				_originalShakeContrast = ShakeContrast;
				_originalRemapContrastZero = RemapContrastZero;
				_originalRemapContrastOne = RemapContrastOne;
				_originalColorFilterMode = ColorFilterMode;
				_originalColorFilterGradient = ColorFilterGradient;
				_originalColorFilterDestination = ColorFilterDestination;
				_originalColorFilterCurve = ColorFilterCurve;
			}

			if (!OnlyUseShakerValues)
			{
				TimescaleMode = timescaleMode;
				ShakeDuration = duration;
				RelativeValues = relativeValues;
				ShakePostExposure = shakePostExposure;
				RemapPostExposureZero = remapPostExposureZero;
				RemapPostExposureOne = remapPostExposureOne;
				ShakeHueShift = shakeHueShift;
				RemapHueShiftZero = remapHueShiftZero;
				RemapHueShiftOne = remapHueShiftOne;
				ShakeSaturation = shakeSaturation;
				RemapSaturationZero = remapSaturationZero;
				RemapSaturationOne = remapSaturationOne;
				ShakeContrast = shakeContrast;
				RemapContrastZero = remapContrastZero;
				RemapContrastOne = remapContrastOne;
				ColorFilterMode = colorFilterMode;
				ColorFilterGradient = colorFilterGradient;
				ColorFilterDestination = colorFilterDestination;
				ColorFilterCurve = colorFilterCurve;
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
			_colorAdjustments.postExposure.Override(_initialPostExposure);
			_colorAdjustments.hueShift.Override(_initialHueShift);
			_colorAdjustments.saturation.Override(_initialSaturation);
			_colorAdjustments.contrast.Override(_initialContrast);
			_colorAdjustments.colorFilter.Override(_initialColorFilterColor);
		}

		/// <summary>
		/// Resets the shaker's values
		/// </summary>
		protected override void ResetShakerValues()
		{
			base.ResetShakerValues();
			ShakeDuration = _originalShakeDuration;
			RelativeValues = _originalRelativeValues;
			ShakePostExposure = _originalShakePostExposure;
			RemapPostExposureZero = _originalRemapPostExposureZero;
			RemapPostExposureOne = _originalRemapPostExposureOne;
			ShakeHueShift = _originalShakeHueShift;
			RemapHueShiftZero = _originalRemapHueShiftZero;
			RemapHueShiftOne = _originalRemapHueShiftOne;
			ShakeSaturation = _originalShakeSaturation;
			RemapSaturationZero = _originalRemapSaturationZero;
			RemapSaturationOne = _originalRemapSaturationOne;
			ShakeContrast = _originalShakeContrast;
			RemapContrastZero = _originalRemapContrastZero;
			RemapContrastOne = _originalRemapContrastOne;
			ColorFilterMode = _originalColorFilterMode;
			ColorFilterGradient = _originalColorFilterGradient;
			ColorFilterDestination = _originalColorFilterDestination;
			ColorFilterCurve = _originalColorFilterCurve;
		}

		/// <summary>
		/// Starts listening for events
		/// </summary>
		public override void StartListening()
		{
			base.StartListening();
			MMColorAdjustmentsShakeEvent_URP.Register(OnMMColorGradingShakeEvent);
		}

		/// <summary>
		/// Stops listening for events
		/// </summary>
		public override void StopListening()
		{
			base.StopListening();
			MMColorAdjustmentsShakeEvent_URP.Unregister(OnMMColorGradingShakeEvent);
		}
		#endif
	}

	/// <summary>
	/// An event used to trigger vignette shakes
	/// </summary>
	public struct MMColorAdjustmentsShakeEvent_URP
	{
		static private event Delegate OnEvent;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
		static public void Register(Delegate callback) { OnEvent += callback; }
		static public void Unregister(Delegate callback) { OnEvent -= callback; }

		public delegate void Delegate(AnimationCurve shakePostExposure, float remapPostExposureZero, float remapPostExposureOne,
			AnimationCurve shakeHueShift, float remapHueShiftZero, float remapHueShiftOne,
			AnimationCurve shakeSaturation, float remapSaturationZero, float remapSaturationOne,
			AnimationCurve shakeContrast, float remapContrastZero, float remapContrastOne,
			MMColorAdjustmentsShaker_URP.ColorFilterModes colorFilterMode, Gradient colorFilterGradient, Color colorFilterDestination,AnimationCurve colorFilterCurve,  
			float duration, bool relativeValues = false,
			float attenuation = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, 
			bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false);

		static public void Trigger(AnimationCurve shakePostExposure, float remapPostExposureZero, float remapPostExposureOne,
			AnimationCurve shakeHueShift, float remapHueShiftZero, float remapHueShiftOne,
			AnimationCurve shakeSaturation, float remapSaturationZero, float remapSaturationOne,
			AnimationCurve shakeContrast, float remapContrastZero, float remapContrastOne,
			MMColorAdjustmentsShaker_URP.ColorFilterModes colorFilterMode, Gradient colorFilterGradient, Color colorFilterDestination,AnimationCurve colorFilterCurve,  
			float duration, bool relativeValues = false,
			float attenuation = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, bool resetTargetValuesAfterShake = true, 
			bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false)
		{
			OnEvent?.Invoke(shakePostExposure, remapPostExposureZero, remapPostExposureOne,
				shakeHueShift, remapHueShiftZero, remapHueShiftOne,
				shakeSaturation, remapSaturationZero, remapSaturationOne,
				shakeContrast, remapContrastZero, remapContrastOne,
				colorFilterMode, colorFilterGradient, colorFilterDestination, colorFilterCurve,
				duration, relativeValues, attenuation, channelData, resetShakerValuesAfterShake, resetTargetValuesAfterShake, forwardDirection, timescaleMode, stop, restore);
		}
	}
}