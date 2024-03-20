using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 이 피드백을 사용하면 대상 MMFeedback 또는 특정 범위 내에서 지정된 채널의 모든 MMFeedback을 트리거할 수 있습니다. MMFeedbacksShaker가 필요합니다.
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("이 피드백을 사용하면 대상 MMFeedback 또는 특정 범위 내에서 지정된 채널의 모든 MMFeedback을 트리거할 수 있습니다. MMFeedbacksShaker가 필요합니다.")]
	[FeedbackPath("Feedbacks/Feedbacks Player")]
	public class MMF_Feedbacks : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.FeedbacksColor; } }
		public override string RequiredTargetText => RequiredChannelText;
		#endif
		/// the duration of this feedback is the duration of our target feedback
		public override float FeedbackDuration 
		{
			get
			{
				if (TargetFeedbacks == Owner)
				{
					return 0f;
				}
				if ((Mode == Modes.PlayTargetFeedbacks) && (TargetFeedbacks != null))
				{
					return TargetFeedbacks.TotalDuration;
				}
				else
				{
					return 0f;    
				}
			} 
		}
		public override bool HasChannel => true;
        
		public enum Modes { PlayFeedbacksInArea, PlayTargetFeedbacks }
        
		[MMFInspectorGroup("Feedbacks", true, 79)]
        
		/// the selected mode for this feedback
		[Tooltip("이 피드백을 위해 선택된 모드")]
		public Modes Mode = Modes.PlayFeedbacksInArea;
        
		/// a specific MMFeedbacks / MMF_Player to play
		[MMFEnumCondition("Mode", (int)Modes.PlayTargetFeedbacks)]
		[Tooltip("재생할 특정 MMFeedbacks / MMF_Player")]
		public MMFeedbacks TargetFeedbacks;
        
		/// whether or not to use a range
		[MMFEnumCondition("Mode", (int)Modes.PlayFeedbacksInArea)]
		[Tooltip("범위를 사용할지 여부")]
		public bool OnlyTriggerPlayersInRange = false;
		/// the range of the event, in units
		[MMFEnumCondition("Mode", (int)Modes.PlayFeedbacksInArea)]
		[Tooltip("이벤트 범위(단위)")]
		public float EventRange = 100f;
		/// the transform to use to broadcast the event as origin point
		[MMFEnumCondition("Mode", (int)Modes.PlayFeedbacksInArea)]
		[Tooltip("이벤트를 원점으로 방송하는 데 사용할 변환")]
		public Transform EventOriginTransform;

		/// <summary>
		/// On init we turn the light off if needed
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);
            
			if (EventOriginTransform == null)
			{
				EventOriginTransform = owner.transform;
			}
		}

		/// <summary>
		/// On Play we trigger our target feedback or trigger a feedback shake event to shake feedbacks in the area
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (TargetFeedbacks == Owner)
			{
				return;
			}
			
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			if (Mode == Modes.PlayFeedbacksInArea)
			{
				MMFeedbacksShakeEvent.Trigger(ChannelData, OnlyTriggerPlayersInRange, EventRange, EventOriginTransform.position);    
			}
			else if (Mode == Modes.PlayTargetFeedbacks)
			{
				TargetFeedbacks?.PlayFeedbacks(position, feedbacksIntensity);
			}
		}
	}
}