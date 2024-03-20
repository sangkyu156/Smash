using System;
using System.Collections;
using MoreMountains.Tools;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 이 셰이커를 사용하면 변환의 스케일을 한 번 또는 영구적으로 이동하여 지정된 기간 동안 지정된 범위 내에서 스케일을 흔들 수 있습니다.
    /// 선택적 노이즈 및 감쇠를 사용하여 무작위 여부에 관계없이 방향을 따라 흔들림을 적용할 수 있습니다.
    /// </summary>
    public class MMScaleShaker : MMShaker
	{
		public enum Modes { Transform, RectTransform }

		[MMInspectorGroup("Target", true, 41)]
		/// whether this shaker should target Transforms or RectTransforms
		[Tooltip("이 셰이커가 Transforms 또는 RectTransforms를 대상으로 해야 하는지 여부")]
		public Modes Mode = Modes.Transform;
		/// the transform to shake the scale of. If left blank, this component will target the transform it's put on.
		[Tooltip("규모를 흔들기 위한 변환입니다. 비워 두면 이 구성 요소는 적용된 변환을 대상으로 합니다.")]
		[MMEnumCondition("Mode", (int)Modes.Transform)]
		public Transform TargetTransform;
		/// the rect transform to shake the scale of. If left blank, this component will target the transform it's put on.
		[Tooltip("스케일을 흔들기 위한 ret 변환입니다. 비워 두면 이 구성 요소는 적용된 변환을 대상으로 합니다.")]
		[MMEnumCondition("Mode", (int)Modes.RectTransform)]
		public RectTransform TargetRectTransform;
        
		[MMInspectorGroup("Shake Settings", true, 42)]
		/// the speed at which the transform should shake
		[Tooltip("변환이 흔들리는 속도")]
		public float ShakeSpeed = 20f;
		/// the maximum distance from its initial scale the transform will move to during the shake
		[Tooltip("흔들림 중에 변환이 이동할 초기 배율로부터의 최대 거리")]
		public float ShakeRange = 0.5f;
        
		[MMInspectorGroup("Direction", true, 43)]
		/// the direction along which to shake the transform's scale
		[Tooltip("변환의 스케일을 흔들는 방향")]
		public Vector3 ShakeMainDirection = Vector3.up;
		/// if this is true, instead of using ShakeMainDirection as the direction of the shake, a random vector3 will be generated, randomized between ShakeMainDirection and ShakeAltDirection
		[Tooltip("이것이 사실이라면 ShakeMainDirection을 흔들기 방향으로 사용하는 대신 무작위 벡터3가 생성되어 ShakeMainDirection과 ShakeAltDirection 사이에서 무작위로 생성됩니다.")]
		public bool RandomizeDirection = false;
		/// when in RandomizeDirection mode, a vector against which to randomize the main direction
		[Tooltip("RandomizeDirection 모드에 있을 때 주 방향을 무작위화할 벡터")]
		[MMCondition("RandomizeDirection", true)]
		public Vector3 ShakeAltDirection = Vector3.up;
		/// if this is true, a new direction will be randomized every time a shake happens
		[Tooltip("이것이 사실이라면 흔들릴 때마다 새로운 방향이 무작위로 지정됩니다.")]
		public bool RandomizeDirectionOnPlay = false;

		[MMInspectorGroup("Directional Noise", true, 47)]
		/// whether or not to add noise to the main direction
		[Tooltip("주 방향에 노이즈를 추가할지 여부")]
		public bool AddDirectionalNoise = true;
		/// when adding directional noise, noise strength will be randomized between this value and DirectionalNoiseStrengthMax
		[Tooltip("방향성 노이즈를 추가할 때 노이즈 강도는 이 값과 DirectionalNoiseStrengthMax 사이에서 무작위로 지정됩니다.")]
		[MMCondition("AddDirectionalNoise", true)]
		public Vector3 DirectionalNoiseStrengthMin = new Vector3(0.25f, 0.25f, 0.25f);
		/// when adding directional noise, noise strength will be randomized between this value and DirectionalNoiseStrengthMin
		[Tooltip("방향성 노이즈를 추가할 때 노이즈 강도는 이 값과 DirectionalNoiseStrengthMin 사이에서 무작위로 지정됩니다.")]
		[MMCondition("AddDirectionalNoise", true)]
		public Vector3 DirectionalNoiseStrengthMax = new Vector3(0.25f, 0.25f, 0.25f);
        
		[MMInspectorGroup("Randomness", true, 44)]
		/// a unique seed you can use to get different outcomes when shaking more than one transform at once
		[Tooltip("한 번에 둘 이상의 변환을 흔들 때 다른 결과를 얻는 데 사용할 수 있는 고유한 시드")]
		public Vector3 RandomnessSeed;
		/// whether or not to generate a unique seed automatically on every shake
		[Tooltip("흔들릴 때마다 고유한 시드를 자동으로 생성할지 여부")]
		public bool RandomizeSeedOnShake = true;

		[MMInspectorGroup("One Time", true, 45)]
		/// whether or not to use attenuation, which will impact the amplitude of the shake, along the defined curve
		[Tooltip("정의된 곡선을 따라 흔들림의 진폭에 영향을 미치는 감쇠를 사용할지 여부")]
		public bool UseAttenuation = true;
		/// the animation curve used to define attenuation, impacting the amplitude of the shake
		[Tooltip("감쇠를 정의하는 데 사용되는 애니메이션 곡선으로 흔들림의 진폭에 영향을 미칩니다.")]
		[MMCondition("UseAttenuation", true)]
		public AnimationCurve AttenuationCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));

		[MMInspectorGroup("Test", true, 46)]
		[MMInspectorButton("StartShaking")] 
		public bool StartShakingButton;

		public virtual float Randomness => RandomnessSeed.x + RandomnessSeed.y + RandomnessSeed.z;

		protected float _attenuation = 1f;
		protected float _oscillation;
		protected Vector3 _initialScale;
		protected Vector3 _workDirection;
		protected Vector3 _noiseVector;
		protected Vector3 _newScale;
		protected Vector3 _randomNoiseStrength;
		protected Vector3 _noNoise = Vector3.zero;
		protected Vector3 _randomizedDirection;

		/// <summary>
		/// On init we initialize our values
		/// </summary>
		protected override void Initialization()
		{
			base.Initialization();
			if (TargetTransform == null) { TargetTransform = this.transform; }
			if (TargetRectTransform == null) { TargetRectTransform = this.GetComponent<RectTransform>(); }
			GrabInitialScale();
		}

		protected virtual void GrabInitialScale()
		{
			switch (Mode)
			{
				case Modes.Transform:
					_initialScale = TargetTransform.localScale;
					break;
				case Modes.RectTransform:
					_initialScale = TargetRectTransform.localScale;
					break;
			}
		}
               
		/// <summary>
		/// When that shaker gets added, we initialize its shake duration
		/// </summary>
		protected virtual void Reset()
		{
			ShakeDuration = 0.5f;
		}

		protected override void ShakeStarts()
		{
			GrabInitialScale();
			if (RandomizeSeedOnShake)
			{
				RandomnessSeed = Random.insideUnitSphere;
			}

			if (RandomizeDirectionOnPlay)
			{
				ShakeMainDirection = Random.insideUnitSphere;
				ShakeAltDirection = Random.insideUnitSphere;
			}
           
			_randomizedDirection = RandomizeDirection ? MMMaths.RandomVector3(ShakeMainDirection, ShakeAltDirection) : ShakeMainDirection;
		}
        
		protected override void Shake()
		{
			_oscillation = Mathf.Sin(ShakeSpeed * (Randomness + _journey));
			float remappedTime = MMFeedbacksHelpers.Remap(_journey, 0f, ShakeDuration, 0f, 1f);
           
			_attenuation = ComputeAttenuation(remappedTime);
			_workDirection = ShakeMainDirection + ComputeNoise(_journey);
			_workDirection.Normalize();
			_newScale = ComputeNewScale();
			ApplyNewScale(_newScale);
		}
        
		protected override void ShakeComplete()
		{
			base.ShakeComplete();
			_attenuation = 0f;
			_newScale = ComputeNewScale();
			if (TargetTransform != null)
			{
				ApplyNewScale(_newScale);	
			}
		}

		protected virtual void ApplyNewScale(Vector3 newScale)
		{
			switch (Mode)
			{
				case Modes.Transform:
					TargetTransform.localScale = newScale;
					break;
				case Modes.RectTransform:
					TargetRectTransform.localScale = newScale;
					break;
			}
		}

		protected virtual Vector3 ComputeNewScale()
		{
			return _initialScale + _workDirection * _oscillation * ShakeRange * _attenuation;
		}

		protected virtual float ComputeAttenuation(float remappedTime)
		{
			return (UseAttenuation && !PermanentShake) ? AttenuationCurve.Evaluate(remappedTime) : 1f;
		}
        
		protected virtual Vector3 ComputeNoise(float time)
		{
			if (!AddDirectionalNoise)
			{
				return _noNoise;
			}

			_randomNoiseStrength = MMMaths.RandomVector3(DirectionalNoiseStrengthMin, DirectionalNoiseStrengthMax); 
	        
			_noiseVector.x = _randomNoiseStrength.x * (Mathf.PerlinNoise(RandomnessSeed.x, time) - 0.5f) ;
			_noiseVector.y = _randomNoiseStrength.y * (Mathf.PerlinNoise(RandomnessSeed.y, time) - 0.5f);
			_noiseVector.z = _randomNoiseStrength.z * (Mathf.PerlinNoise(RandomnessSeed.z, time) - 0.5f);
	        
			return _noiseVector;
		}
        
		protected float _originalDuration;
		protected float _originalShakeSpeed;
		protected float _originalShakeRange;
		protected Vector3 _originalShakeMainDirection;
		protected bool _originalRandomizeDirection;
		protected Vector3 _originalShakeAltDirection;
		protected bool _originalRandomizeDirectionOnPlay;
		protected bool _originalAddDirectionalNoise;
		protected Vector3 _originalDirectionalNoiseStrengthMin;
		protected Vector3 _originalDirectionalNoiseStrengthMax;
		protected Vector3 _originalRandomnessSeed;
		protected bool _originalRandomizeSeedOnShake;
		protected bool _originalUseAttenuation;
		protected AnimationCurve _originalAttenuationCurve;

		public virtual void OnMMScaleShakeEvent(float duration, float shakeSpeed, float shakeRange, Vector3 shakeMainDirection, bool randomizeDirection, Vector3 shakeAltDirection, bool randomizeDirectionOnPlay, bool addDirectionalNoise, 
			Vector3 directionalNoiseStrengthMin, Vector3 directionalNoiseStrengthMax, Vector3 randomnessSeed, bool randomizeSeedOnShake, bool useAttenuation, AnimationCurve attenuationCurve,
			bool useRange = false, float rangeDistance = 0f, bool useRangeFalloff = false, AnimationCurve rangeFalloff = null, Vector2 remapRangeFalloff = default(Vector2), Vector3 rangePosition = default(Vector3),
			float feedbacksIntensity = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, 
			bool resetTargetValuesAfterShake = true, bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false)
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
				_originalDuration = ShakeDuration;
				_originalShakeSpeed = ShakeSpeed;
				_originalShakeRange = ShakeRange;
				_originalShakeMainDirection = ShakeMainDirection;
				_originalRandomizeDirection = RandomizeDirection;
				_originalShakeAltDirection = ShakeAltDirection;
				_originalRandomizeDirectionOnPlay = RandomizeDirectionOnPlay;
				_originalAddDirectionalNoise = AddDirectionalNoise;
				_originalDirectionalNoiseStrengthMin = DirectionalNoiseStrengthMin;
				_originalDirectionalNoiseStrengthMax = DirectionalNoiseStrengthMax;
				_originalRandomnessSeed = RandomnessSeed;
				_originalRandomizeSeedOnShake = RandomizeSeedOnShake;
				_originalUseAttenuation = UseAttenuation;
				_originalAttenuationCurve = AttenuationCurve;
			}

			if (!OnlyUseShakerValues)
			{
				TimescaleMode = timescaleMode;
				ShakeDuration = duration;
				ShakeSpeed = shakeSpeed;
				ShakeRange = shakeRange * feedbacksIntensity * ComputeRangeIntensity(useRange, rangeDistance,
					useRangeFalloff, rangeFalloff, remapRangeFalloff, rangePosition);
				ShakeMainDirection = shakeMainDirection;
				RandomizeDirection = randomizeDirection;
				ShakeAltDirection = shakeAltDirection;
				RandomizeDirectionOnPlay = randomizeDirectionOnPlay;
				AddDirectionalNoise = addDirectionalNoise;
				DirectionalNoiseStrengthMin = directionalNoiseStrengthMin;
				DirectionalNoiseStrengthMax = directionalNoiseStrengthMax;
				RandomnessSeed = randomnessSeed;
				RandomizeSeedOnShake = randomizeSeedOnShake;
				UseAttenuation = useAttenuation;
				AttenuationCurve = attenuationCurve;
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
			switch (Mode)
			{
				case Modes.Transform:
					TargetTransform.localScale = _initialScale;
					break;
				case Modes.RectTransform:
					TargetRectTransform.localScale = _initialScale;
					break;
			}
		}

		/// <summary>
		/// Resets the shaker's values
		/// </summary>
		protected override void ResetShakerValues()
		{
			base.ResetShakerValues();
			ShakeDuration = _originalDuration;
			ShakeSpeed = _originalShakeSpeed;
			ShakeRange = _originalShakeRange;
			ShakeMainDirection = _originalShakeMainDirection;
			RandomizeDirection = _originalRandomizeDirection;
			ShakeAltDirection = _originalShakeAltDirection;
			RandomizeDirectionOnPlay = _originalRandomizeDirectionOnPlay;
			AddDirectionalNoise = _originalAddDirectionalNoise;
			DirectionalNoiseStrengthMin = _originalDirectionalNoiseStrengthMin;
			DirectionalNoiseStrengthMax = _originalDirectionalNoiseStrengthMax;
			RandomnessSeed = _originalRandomnessSeed;
			RandomizeSeedOnShake = _originalRandomizeSeedOnShake;
			UseAttenuation = _originalUseAttenuation;
			AttenuationCurve = _originalAttenuationCurve;
		}

		/// <summary>
		/// Starts listening for events
		/// </summary>
		public override void StartListening()
		{
			base.StartListening();
			MMScaleShakeEvent.Register(OnMMScaleShakeEvent);
		}

		/// <summary>
		/// Stops listening for events
		/// </summary>
		public override void StopListening()
		{
			base.StopListening();
			MMScaleShakeEvent.Unregister(OnMMScaleShakeEvent);
		}
	}
	
	public struct MMScaleShakeEvent
	{
		static private event Delegate OnEvent;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
		static public void Register(Delegate callback) { OnEvent += callback; }
		static public void Unregister(Delegate callback) { OnEvent -= callback; }

		public delegate void Delegate(float duration, float shakeSpeed, float shakeRange, Vector3 shakeMainDirection, bool randomizeDirection, Vector3 shakeAltDirection, bool randomizeDirectionOnPlay, bool addDirectionalNoise, 
			Vector3 directionalNoiseStrengthMin, Vector3 directionalNoiseStrengthMax, Vector3 randomnessSeed, bool randomizeSeedOnShake, bool useAttenuation, AnimationCurve attenuationCurve,
			bool useRange = false, float rangeDistance = 0f, bool useRangeFalloff = false, AnimationCurve rangeFalloff = null, Vector2 remapRangeFalloff = default(Vector2), Vector3 rangePosition = default(Vector3),
			float feedbacksIntensity = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, 
			bool resetTargetValuesAfterShake = true, bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false);

		static public void Trigger(float duration, float shakeSpeed, float shakeRange, Vector3 shakeMainDirection, bool randomizeDirection, Vector3 shakeAltDirection, bool randomizeDirectionOnPlay, bool addDirectionalNoise, 
			Vector3 directionalNoiseStrengthMin, Vector3 directionalNoiseStrengthMax, Vector3 randomnessSeed, bool randomizeSeedOnShake, bool useAttenuation, AnimationCurve attenuationCurve,
			bool useRange = false, float rangeDistance = 0f, bool useRangeFalloff = false, AnimationCurve rangeFalloff = null, Vector2 remapRangeFalloff = default(Vector2), Vector3 rangePosition = default(Vector3),
			float feedbacksIntensity = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, 
			bool resetTargetValuesAfterShake = true, bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false)
		{
			OnEvent?.Invoke( duration, shakeSpeed,  shakeRange,  shakeMainDirection,  randomizeDirection,  shakeAltDirection,  randomizeDirectionOnPlay,  addDirectionalNoise, 
				directionalNoiseStrengthMin,  directionalNoiseStrengthMax,  randomnessSeed,  randomizeSeedOnShake,  useAttenuation,  attenuationCurve,
				useRange, rangeDistance, useRangeFalloff, rangeFalloff, remapRangeFalloff, rangePosition,
				feedbacksIntensity, channelData, resetShakerValuesAfterShake, resetTargetValuesAfterShake, forwardDirection, timescaleMode, stop, restore);
		}
	}
}