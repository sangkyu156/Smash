using System;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 이 피드백을 사용하면 참조를 보유할 수 있으며 다른 피드백에서 해당 목표를 자동으로 설정하는 데 사용할 수 있습니다.
    /// 플레이할 때 아무 것도 하지 않습니다.
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("이 피드백을 사용하면 참조를 보유할 수 있으며 다른 피드백에서 해당 목표를 자동으로 설정하는 데 사용할 수 있습니다. 플레이할 때 아무 것도 하지 않습니다.")]
	[FeedbackPath("Feedbacks/MMF Reference Holder")]
	public class MMF_ReferenceHolder : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.FeedbacksColor; } }
		public override string RequiredTargetText => GameObjectReference != null ? GameObjectReference.name : "";  
		#endif
		/// the duration of this feedback is 0
		public override float FeedbackDuration => 0f;
		public override bool DisplayFullHeaderColor => true;

		[MMFInspectorGroup("References", true, 37, true)]
		/// the game object to set as the target (or on which to look for a specific component as a target) of all feedbacks that may look at this reference holder for a target
		[Tooltip("타겟에 대한 이 참조 홀더를 볼 수 있는 모든 피드백 중 타겟으로 설정할(또는 특정 구성 요소를 타겟으로 찾는) 게임 객체")] 
		public GameObject GameObjectReference;
		/// whether or not to force this reference holder on all compatible feedbacks in the MMF Player's list
		[Tooltip("MMF 플레이어 목록의 모든 호환 가능한 피드백에 대해 이 참조 보유자를 강제할지 여부")] 
		public bool ForceReferenceOnAll = false;
		
		/// <summary>
		/// On init we force our reference on all feedbacks if needed
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);
			if (ForceReferenceOnAll)
			{
				for (int index = 0; index < Owner.FeedbacksList.Count; index++)
				{
					if (Owner.FeedbacksList[index].HasAutomatedTargetAcquisition)
					{
						Owner.FeedbacksList[index].SetIndexInFeedbacksList(index);
						Owner.FeedbacksList[index].ForcedReferenceHolder = this;
						Owner.FeedbacksList[index].ForceAutomateTargetAcquisition();
					}
				}
			}
		}

		/// <summary>
		/// On Play we do nothing
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			return;
		}
	}
}