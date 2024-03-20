using UnityEngine;
using MoreMountains.Feedbacks;
#if MM_POSTPROCESSING
using UnityEngine.Rendering.PostProcessing;
#endif 

namespace MoreMountains.FeedbacksForThirdParty
{
    /// <summary>
    /// 전역 PP 볼륨이 시작 값과 끝 값 사이에서 큐에 가중치를 자동으로 혼합하도록 하려면 이 클래스를 사용하십시오.
    /// </summary>
    [AddComponentMenu("More Mountains/Feedbacks/Shakers/PostProcessing/MMGlobalPostProcessingVolumeAutoBlend")]
	#if MM_POSTPROCESSING
	[RequireComponent(typeof(PostProcessVolume))]
	#endif
	public class MMGlobalPostProcessingVolumeAutoBlend : MonoBehaviour
	{
		/// the possible timescales this blend can operate on
		public enum TimeScales { Scaled, Unscaled }
		/// the possible blend trigger modes 
		public enum BlendTriggerModes { OnEnable, Script }

		[Header("Blend")]
		/// the trigger mode for this MMGlobalPostProcessingVolumeAutoBlend
		/// Start : will play automatically on enable
		[Tooltip("이 MMGlobalPostProcessingVolumeAutoBlend의 트리거 모드")]
		public BlendTriggerModes BlendTriggerMode = BlendTriggerModes.OnEnable;
		/// the duration of the blend (in seconds)
		[Tooltip("혼합 기간(초)")]
		public float BlendDuration = 1f;
		/// the curve to use to blend
		[Tooltip("블렌드에 사용할 곡선")]
		public AnimationCurve Curve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1f));

		[Header("Weight")]
		/// the weight at the start of the blend
		[Tooltip("블렌드 시작 시의 무게")]
		[Range(0f, 1f)]
		public float InitialWeight = 0f;
		/// the desired weight at the end of the blend
		[Tooltip("블렌드 종료 시 원하는 무게")]
		[Range(0f, 1f)]
		public float FinalWeight = 1f;

		[Header("Behaviour")]
		/// the timescale to operate on
		[Tooltip("작업할 기간\r\n")]
		public TimeScales TimeScale = TimeScales.Unscaled;
		/// whether or not the associated volume should be disabled at 0
		[Tooltip("관련 볼륨을 0에서 비활성화해야 하는지 여부")]
		public bool DisableVolumeOnZeroWeight = true;
		/// whether or not this blender should disable itself at 0
		[Tooltip("이 블렌더가 0에서 비활성화되어야 하는지 여부")]
		public bool DisableSelfAfterEnd = true;
		/// whether or not this blender can be interrupted
		[Tooltip("이 블렌더를 중단할 수 있는지 여부")]
		public bool Interruptable = true;
		/// whether or not this blender should pick the current value as its starting point
		[Tooltip("이 블렌더가 현재 값을 시작점으로 선택해야 하는지 여부")]
		public bool StartFromCurrentValue = true;
		/// reset to initial value on end 
		[Tooltip("종료 시 초기값으로 재설정 ")]
		public bool ResetToInitialValueOnEnd = false;

		[Header("Tests")]
		/// test blend button
		[Tooltip("테스트 혼합 버튼")]
		[MMFInspectorButton("Blend")]
		public bool TestBlend;
		/// test blend back button
		[Tooltip("테스트 블렌드 뒤로 버튼")]
		[MMFInspectorButton("BlendBack")]
		public bool TestBlendBackwards;

		/// <summary>
		/// Returns the correct timescale based on the chosen settings
		/// </summary>
		/// <returns></returns>
		protected float GetTime()
		{
			return (TimeScale == TimeScales.Unscaled) ? Time.unscaledTime : Time.time;
		}

		protected float _initial;
		protected float _destination;
		protected float _startTime;
		protected bool _blending = false;
		#if MM_POSTPROCESSING
		protected PostProcessVolume _volume;
		
		/// <summary>
		/// On Awake we store our volume
		/// </summary>
		protected virtual void Awake()
		{
			#if MM_POSTPROCESSING
			_volume = this.gameObject.GetComponent<PostProcessVolume>();
			_volume.weight = InitialWeight;
			#endif
		}

		/// <summary>
		/// On start we start blending if needed
		/// </summary>
		protected virtual void OnEnable()
		{
			if ((BlendTriggerMode == BlendTriggerModes.OnEnable) && !_blending)
			{
				Blend();
			}
		}

		/// <summary>
		/// Blends the volume's weight from the initial value to the final one
		/// </summary>
		public virtual void Blend()
		{
			if (_blending && !Interruptable)
			{
				return;
			}
			_initial = StartFromCurrentValue ? _volume.weight : InitialWeight;
			_destination = FinalWeight;
			StartBlending();
		}

		/// <summary>
		/// Blends the volume's weight from the final value to the initial one
		/// </summary>
		public virtual void BlendBack()
		{
			if (_blending && !Interruptable)
			{
				return;
			}
			_initial = StartFromCurrentValue ? _volume.weight : FinalWeight;
			_destination = InitialWeight;
			StartBlending();
		}

		/// <summary>
		/// Internal method used to start blending
		/// </summary>
		protected virtual void StartBlending()
		{
			_startTime = GetTime();
			_blending = true;
			this.enabled = true;
			if (DisableVolumeOnZeroWeight)
			{
				_volume.enabled = true;
			}
		}

		/// <summary>
		/// Stops any blending that may be in progress
		/// </summary>
		public virtual void StopBlending()
		{
			_blending = false;
		}

		/// <summary>
		/// On update, processes the blend if needed
		/// </summary>
		protected virtual void Update()
		{
			if (!_blending)
			{
				return;
			}

			float timeElapsed = (GetTime() - _startTime);
			if (timeElapsed < BlendDuration)
			{                
				float remapped = MMFeedbacksHelpers.Remap(timeElapsed, 0f, BlendDuration, 0f, 1f);
				_volume.weight = Mathf.LerpUnclamped(_initial, _destination, Curve.Evaluate(remapped));
			}
			else
			{
				// after end is reached
				_volume.weight = ResetToInitialValueOnEnd ? _initial : _destination;
				_blending = false;
				if (DisableVolumeOnZeroWeight && (_volume.weight == 0f))
				{
					_volume.enabled = false;
				}
				if (DisableSelfAfterEnd)
				{
					this.enabled = false;
				}
			}            
		}
	
		public virtual void RestoreInitialValues()
		{
			_volume.weight = _initial;
		}
		
		#endif
	}
}