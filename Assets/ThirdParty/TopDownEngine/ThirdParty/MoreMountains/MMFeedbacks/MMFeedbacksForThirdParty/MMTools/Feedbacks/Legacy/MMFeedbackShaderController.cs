using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 이 피드백을 통해 대상 ShaderController의 값을 제어하고 런타임 시 셰이더 기반 재질의 동작과 측면을 수정할 수 있습니다.
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("이 피드백을 사용하면 대상 ShaderController에서 일회성 재생을 트리거할 수 있습니다.")]
	[FeedbackPath("Renderer/ShaderController")]
	public class MMFeedbackShaderController : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// the different possible modes 
		public enum Modes { OneTime, ToDestination }
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.RendererColor; } }
		#endif

		[Header("Float Controller")]
		/// the mode this controller is in
		[Tooltip("이 컨트롤러의 모드")]
		public Modes Mode = Modes.OneTime;
		/// the float controller to trigger a one time play on
		[Tooltip("일회성 재생을 트리거하는 플로트 컨트롤러")]
		public ShaderController TargetShaderController;
		/// an optional list of float controllers to trigger a one time play on
		[Tooltip("일회성 재생을 트리거하는 플로트 컨트롤러의 선택적 목록")]
		public List<ShaderController> TargetShaderControllerList;
		/// whether this should revert to original at the end
		[Tooltip("마지막에 원본으로 되돌릴지 여부")]
		public bool RevertToInitialValueAfterEnd = false;
		/// the duration of the One Time shake
		[Tooltip("One Time Shake의 지속 시간")]
		[MMFEnumCondition("Mode", (int)Modes.OneTime)]
		public float OneTimeDuration = 1f;
		/// the amplitude of the One Time shake (this will be multiplied by the curve's height)
		[Tooltip("일회성 흔들림의 진폭(이에 곡선 높이를 곱함)")]
		[MMFEnumCondition("Mode", (int)Modes.OneTime)]
		public float OneTimeAmplitude = 1f;
		/// the low value to remap the normalized curve value to 
		[Tooltip("정규화된 곡선 값을 다시 매핑할 낮은 값")]
		[MMFEnumCondition("Mode", (int)Modes.OneTime)]
		public float OneTimeRemapMin = 0f;
		/// the high value to remap the normalized curve value to 
		[Tooltip("정규화된 곡선 값을 다시 매핑할 높은 값")]
		[MMFEnumCondition("Mode", (int)Modes.OneTime)]
		public float OneTimeRemapMax = 1f;
		/// the curve to apply to the one time shake
		[Tooltip("일회성 흔들기에 적용할 곡선")]
		[MMFEnumCondition("Mode", (int)Modes.OneTime)]
		public AnimationCurve OneTimeCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));

		/// the new value towards which to move the current value
		[Tooltip("현재 값을 이동할 새 값")]
		[MMFEnumCondition("Mode", (int)Modes.ToDestination)]
		public float ToDestinationValue = 1f;
		/// the duration over which to interpolate the target value
		[Tooltip("목표 값을 보간하는 기간")]
		[MMFEnumCondition("Mode", (int)Modes.ToDestination)]
		public float ToDestinationDuration = 1f;
		/// the color to aim for (when targetting a Color property
		[Tooltip("목표로 삼을 색상(Color 속성을 대상으로 하는 경우)")]
		[MMFEnumCondition("Mode", (int)Modes.ToDestination)]
		public Color ToDestinationColor = Color.red;
		/// the curve over which to interpolate the value
		[Tooltip("값을 보간할 곡선")]
		[MMFEnumCondition("Mode", (int)Modes.ToDestination)]
		public AnimationCurve ToDestinationCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));

		/// the duration of this feedback is the duration of the one time hit
		public override float FeedbackDuration
		{
			get { return (Mode == Modes.OneTime) ? ApplyTimeMultiplier(OneTimeDuration) : ApplyTimeMultiplier(ToDestinationDuration); } 
			set { OneTimeDuration = value; ToDestinationDuration = value; }
		}

		protected float _oneTimeDurationStorage;
		protected float _oneTimeAmplitudeStorage;
		protected float _oneTimeRemapMinStorage;
		protected float _oneTimeRemapMaxStorage;
		protected AnimationCurve _oneTimeCurveStorage;
		protected float _toDestinationValueStorage;
		protected float _toDestinationDurationStorage;
		protected AnimationCurve _toDestinationCurveStorage;
		protected bool _revertToInitialValueAfterEndStorage;

		/// <summary>
		/// On init we grab our initial controller values
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(GameObject owner)
		{
			if (Active && (TargetShaderController != null))
			{
				_oneTimeDurationStorage = TargetShaderController.OneTimeDuration;
				_oneTimeAmplitudeStorage = TargetShaderController.OneTimeAmplitude;
				_oneTimeCurveStorage = TargetShaderController.OneTimeCurve;
				_oneTimeRemapMinStorage = TargetShaderController.OneTimeRemapMin;
				_oneTimeRemapMaxStorage = TargetShaderController.OneTimeRemapMax;
				_toDestinationCurveStorage = TargetShaderController.ToDestinationCurve;
				_toDestinationDurationStorage = TargetShaderController.ToDestinationDuration;
				_toDestinationValueStorage = TargetShaderController.ToDestinationValue;
				_revertToInitialValueAfterEndStorage = TargetShaderController.RevertToInitialValueAfterEnd;
			}
		}

		/// <summary>
		/// On play we trigger a OneTime or ToDestination play on our shader controller
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || (TargetShaderController == null))
			{
				return;
			}
            
			float intensityMultiplier = Timing.ConstantIntensity ? 1f : feedbacksIntensity;
            
			PerformPlay(TargetShaderController, intensityMultiplier);     

			foreach (ShaderController shaderController in TargetShaderControllerList)
			{
				PerformPlay(shaderController, intensityMultiplier);     
			}    
		}

		protected virtual void PerformPlay(ShaderController shaderController, float intensityMultiplier)
		{
			shaderController.RevertToInitialValueAfterEnd = RevertToInitialValueAfterEnd;
			if (Mode == Modes.OneTime)
			{
				shaderController.OneTimeDuration = FeedbackDuration;
				shaderController.OneTimeAmplitude = OneTimeAmplitude;
				shaderController.OneTimeCurve = OneTimeCurve;
				if (NormalPlayDirection)
				{
					shaderController.OneTimeRemapMin = OneTimeRemapMin * intensityMultiplier;
					shaderController.OneTimeRemapMax = OneTimeRemapMax * intensityMultiplier;    
				}
				else
				{
					shaderController.OneTimeRemapMin = OneTimeRemapMax * intensityMultiplier;
					shaderController.OneTimeRemapMax = OneTimeRemapMin * intensityMultiplier;
				}
				shaderController.OneTime();
			}
			if (Mode == Modes.ToDestination)
			{
				shaderController.ToColor = ToDestinationColor;
				shaderController.ToDestinationCurve = ToDestinationCurve;
				shaderController.ToDestinationDuration = FeedbackDuration;
				shaderController.ToDestinationValue = ToDestinationValue;
				shaderController.ToDestination();
			}   
		}
        
		/// <summary>
		/// Stops this feedback
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
            
			if (TargetShaderController != null)
			{
				TargetShaderController.Stop();
			}

			foreach (ShaderController shaderController in TargetShaderControllerList)
			{
				shaderController.Stop();
			}
		}

		/// <summary>
		/// On reset we restore our initial values
		/// </summary>
		protected override void CustomReset()
		{
			base.CustomReset();
			if (Active && FeedbackTypeAuthorized && (TargetShaderController != null))
			{
				PerformReset(TargetShaderController);
			}

			foreach (ShaderController shaderController in TargetShaderControllerList)
			{
				PerformReset(shaderController);
			}
		}

		protected virtual void PerformReset(ShaderController shaderController)
		{
			shaderController.OneTimeDuration = _oneTimeDurationStorage;
			shaderController.OneTimeAmplitude = _oneTimeAmplitudeStorage;
			shaderController.OneTimeCurve = _oneTimeCurveStorage;
			shaderController.OneTimeRemapMin = _oneTimeRemapMinStorage;
			shaderController.OneTimeRemapMax = _oneTimeRemapMaxStorage;
			shaderController.ToDestinationCurve = _toDestinationCurveStorage;
			shaderController.ToDestinationDuration = _toDestinationDurationStorage;
			shaderController.ToDestinationValue = _toDestinationValueStorage;
			shaderController.RevertToInitialValueAfterEnd = _revertToInitialValueAfterEndStorage;
		}

	}
}