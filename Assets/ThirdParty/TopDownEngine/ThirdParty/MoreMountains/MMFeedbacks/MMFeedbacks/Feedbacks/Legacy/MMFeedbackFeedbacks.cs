using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 피드백의 다양한 단계에서 객체를 활성화 또는 비활성화합니다.
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("이 피드백을 사용하면 특정 범위 내의 지정된 채널에서 MMFeedback을 트리거할 수 있습니다. MMFeedbacksShaker가 필요합니다.")]
	[FeedbackPath("GameObject/MMFeedbacks")]
	public class MMFeedbackFeedbacks : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// the duration of this feedback is 0
		public override float FeedbackDuration { get { return 0f; } }
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.GameObjectColor; } }
		#endif

		[Header("MMFeedbacks")]
		/// the channel to broadcast on
		[Tooltip("the channel to broadcast on")]
		public int Channel = 0;
		/// whether or not to use a range
		[Tooltip("범위를 사용할지 여부")]
		public bool UseRange = false;
		/// the range of the event, in units
		[Tooltip("이벤트 범위(단위)")]
		public float EventRange = 100f;
		/// the transform to use to broadcast the event as origin point
		[Tooltip("이벤트를 원점으로 방송하는 데 사용할 변환")]
		public Transform EventOriginTransform;

		/// <summary>
		/// On init we turn the light off if needed
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(GameObject owner)
		{
			base.CustomInitialization(owner);
            
			if (EventOriginTransform == null)
			{
				EventOriginTransform = this.transform;
			}
		}

		/// <summary>
		/// On Play we trigger our feedback shake event
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			MMFeedbacksShakeEvent.Trigger(ChannelData(Channel), UseRange, EventRange, EventOriginTransform.position);
		}
	}
}