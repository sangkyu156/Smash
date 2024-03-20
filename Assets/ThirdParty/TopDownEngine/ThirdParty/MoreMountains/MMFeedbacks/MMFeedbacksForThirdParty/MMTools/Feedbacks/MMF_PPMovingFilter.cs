using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 이 피드백은 MMPostProcessingMovableFilter 개체에 의해 포착되는 사후 처리 이동 필터 이벤트를 트리거합니다.
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("이 피드백은 MMPostProcessingMovableFilter 개체에 의해 포착되는 사후 처리 이동 필터 이벤트를 트리거합니다.")]
	[FeedbackPath("PostProcess/PPMovingFilter")]
	public class MMF_PPMovingFilter : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
        
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.PostProcessColor; } }
		public override string RequiredTargetText => RequiredChannelText;
		#endif
        
		/// the duration of this feedback is the duration of the transition
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(TransitionDuration); } set { TransitionDuration = value;  } }
		public override bool HasChannel => true;

		/// the possible modes for this feedback 
		public enum Modes { Toggle, On, Off }

		[MMFInspectorGroup("PostProcessing Profile Moving Filter", true, 54)]
		/// the selected mode for this feedback 
		[Tooltip("이 피드백을 위해 선택된 모드")]
		public Modes Mode = Modes.Toggle;
		/// the duration of the transition
		[Tooltip("전환 기간")]
		public float TransitionDuration = 1f;
		/// the curve to move along to
		[Tooltip("따라 이동하는 곡선")]
		public MMTweenType Curve = new MMTweenType(MMTween.MMTweenCurve.EaseInCubic);

		protected bool _active = false;
		protected bool _toggle = false;

		/// <summary>
		/// On custom play, we trigger a MMPostProcessingMovingFilterEvent with the selected parameters
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
            
			_active = (Mode == Modes.On);
			_toggle = (Mode == Modes.Toggle);

			MMPostProcessingMovingFilterEvent.Trigger(Curve, _active, _toggle, FeedbackDuration, Channel);
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
            
			MMPostProcessingMovingFilterEvent.Trigger(Curve, _active, _toggle, FeedbackDuration, stop:true);
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

			MMPostProcessingMovingFilterEvent.Trigger(Curve, _active, _toggle, FeedbackDuration, restore:true);
		}
	}
}