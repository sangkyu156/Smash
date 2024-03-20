using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;

namespace MoreMountains.FeedbacksForThirdParty
{
    /// <summary>
    /// 이 피드백을 통해 Global PostProcessing Volume AutoBlend 구성 요소를 시험해 볼 수 있습니다. GPPVAB 구성 요소는 PostProcessing Volume에 배치되며 필요에 따라 시간 경과에 따라 가중치를 제어하고 혼합할 수 있습니다.
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("이 피드백을 통해 Global PostProcessing Volume AutoBlend 구성 요소를 시험해 볼 수 있습니다. " +
"GPPVAB 구성 요소는 PostProcessing Volume에 배치되며 필요에 따라 시간 경과에 따라 무게를 제어하고 혼합할 수 있습니다.")]
	[FeedbackPath("PostProcess/Global PP Volume Auto Blend")]
	public class MMFeedbackGlobalPPVolumeAutoBlend : MMFeedback
	{
        /// 이 유형의 모든 피드백을 한 번에 비활성화하는 데 사용되는 정적 부울입니다.
        public static bool FeedbackTypeAuthorized = true;
        /// 이 피드백에 가능한 모드는 다음과 같습니다.
        /// - default : 블렌더에서 Blend() 및 BlendBack()을 트리거할 수 있습니다.
        /// - override : 블렌더에서 새로운 초기, 최종, 지속 시간 및 곡선 값을 지정하고 Blend()를 트리거할 수 있습니다.
        public enum Modes { Default, Override }
        /// 기본 모드에서 가능한 작업
        public enum Actions { Blend, BlendBack }

		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.PostProcessColor; } }
		#endif
		/// defines the duration of the feedback
		public override float FeedbackDuration
		{
			get
			{
				if (Mode == Modes.Override)
				{
					return BlendDuration;
				}
				else
				{
					if (TargetAutoBlend == null)
					{
						return 0.1f;
					}
					else
					{
						return TargetAutoBlend.BlendDuration;
					}
				}
			}
		}
               
		[Header("PostProcess Volume Blend")]
		/// the target auto blend to pilot with this feedback
		[Tooltip("이 피드백으로 시험할 대상 자동 블렌드")]
		public MMGlobalPostProcessingVolumeAutoBlend TargetAutoBlend;
		/// the chosen mode
		[Tooltip("선택한 모드")]
		public Modes Mode = Modes.Default;
		/// the chosen action when in default mode
		[Tooltip("기본 모드에서 선택한 동작")]
		[MMFEnumCondition("Mode", (int)Modes.Default)]
		public Actions BlendAction = Actions.Blend;
		/// the duration of the blend, in seconds when in override mode
		[Tooltip("오버라이드 모드에 있을 때 블렌드 지속 시간(초)")]
		[MMFEnumCondition("Mode", (int)Modes.Override)]
		public float BlendDuration = 1f;
		/// the curve to apply to the blend
		[Tooltip("블렌드에 적용할 곡선")]
		[MMFEnumCondition("Mode", (int)Modes.Override)]
		public AnimationCurve BlendCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1f));
		/// the weight to blend from
		[Tooltip("블렌딩할 무게")]
		[MMFEnumCondition("Mode", (int)Modes.Override)]
		public float InitialWeight = 0f;
		/// the weight to blend to
		[Tooltip("블렌딩할 무게")]
		[MMFEnumCondition("Mode", (int)Modes.Override)]
		public float FinalWeight = 1f;
		/// whether or not to reset to the initial value at the end of the shake
		[Tooltip("흔들기 종료 시 초기값으로 재설정할지 여부")]
		[MMFEnumCondition("Mode", (int)Modes.Override)]
		public bool ResetToInitialValueOnEnd = true;

		/// <summary>
		/// On custom play, triggers a blend on the target blender, overriding its settings if needed
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
        
			#if MM_POSTPROCESSING
            
			if (TargetAutoBlend == null)
			{
				Debug.LogWarning(this.name + " : this MMFeedbackGlobalPPVolumeAutoBlend needs a TargetAutoBlend, please set one in its inspector.");
				return;
			}
			if (Mode == Modes.Default)
			{
				if (!NormalPlayDirection)
				{
					if (BlendAction == Actions.Blend)
					{
						TargetAutoBlend.BlendBack();
						return;
					}
					if (BlendAction == Actions.BlendBack)
					{
						TargetAutoBlend.Blend();
						return;
					}
				}
				else
				{
					if (BlendAction == Actions.Blend)
					{
						TargetAutoBlend.Blend();
						return;
					}
					if (BlendAction == Actions.BlendBack)
					{
						TargetAutoBlend.BlendBack();
						return;
					}    
				}
			}
			else
			{
				TargetAutoBlend.BlendDuration = BlendDuration;
				TargetAutoBlend.Curve = BlendCurve;
				if (!NormalPlayDirection)
				{
					TargetAutoBlend.InitialWeight = FinalWeight;
					TargetAutoBlend.FinalWeight = InitialWeight;   
				}
				else
				{
					TargetAutoBlend.InitialWeight = InitialWeight;
					TargetAutoBlend.FinalWeight = FinalWeight;    
				}
				TargetAutoBlend.ResetToInitialValueOnEnd = ResetToInitialValueOnEnd;
				TargetAutoBlend.Blend();
			}
			#endif
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
        
			#if MM_POSTPROCESSING
            
			base.CustomStopFeedback(position, feedbacksIntensity);
            
			if (TargetAutoBlend != null)
			{
				TargetAutoBlend.StopBlending();
			}
			#endif
		}
	}
}