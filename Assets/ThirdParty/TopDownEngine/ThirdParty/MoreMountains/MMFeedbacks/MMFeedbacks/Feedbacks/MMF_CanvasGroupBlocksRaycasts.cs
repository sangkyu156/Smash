using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 이 피드백을 통해 플레이 시 대상 CanvasGroup의 BlocksRaycast 매개변수를 켜거나 끌 수 있습니다.
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("이 피드백을 통해 플레이 시 대상 CanvasGroup의 BlocksRaycast 매개변수를 켜거나 끌 수 있습니다.")]
	[FeedbackPath("UI/CanvasGroup BlocksRaycasts")]
	public class MMF_CanvasGroupBlocksRaycasts : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.UIColor; } }
		public override bool EvaluateRequiresSetup() { return (TargetCanvasGroup == null); }
		public override string RequiredTargetText { get { return TargetCanvasGroup != null ? TargetCanvasGroup.name : "";  } }
		public override string RequiresSetupText { get { return "This feedback requires that a TargetCanvasGroup be set to be able to work properly. You can set one below."; } }
		#endif
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => TargetCanvasGroup = FindAutomatedTarget<CanvasGroup>();
        
		[MMFInspectorGroup("Block Raycasts", true, 54, true)]
		/// the target canvas group we want to control the BlocksRaycasts parameter on 
		[Tooltip("BlocksRaycasts 매개변수를 제어하려는 대상 캔버스 그룹")]
		public CanvasGroup TargetCanvasGroup;
		/// if this is true, on play, the target canvas group will block raycasts, if false it won't
		[Tooltip("이것이 true이면 플레이 시 대상 캔버스 그룹이 레이캐스트를 차단하고, false이면 차단하지 않습니다.")]
		public bool ShouldBlockRaycasts = true;

		protected bool _initialState;
        
		/// <summary>
		/// On play we turn raycast block on or off
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			if (TargetCanvasGroup == null)
			{
				return;
			}

			_initialState = TargetCanvasGroup.blocksRaycasts;
			TargetCanvasGroup.blocksRaycasts = ShouldBlockRaycasts;
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
			TargetCanvasGroup.blocksRaycasts = _initialState;
		}
	}
}