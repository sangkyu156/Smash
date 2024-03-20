using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// Unity 이벤트를 바인딩하고 재생할 때 트리거하는 피드백
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("이 피드백을 사용하면 모든 유형의 Unity 이벤트를 이 feebdack의 Play, Stop, 초기화 및 재설정 메서드에 바인딩할 수 있습니다.")]
	[FeedbackPath("Events/Events")]
	public class MMFeedbackEvents : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.EventsColor; } }
		#endif

		[Header("Events")]
		/// the events to trigger when the feedback is played
		[Tooltip("피드백이 재생될 때 트리거되는 이벤트")]
		public UnityEvent PlayEvents;
		/// the events to trigger when the feedback is stopped
		[Tooltip("피드백이 중지될 때 트리거되는 이벤트")]
		public UnityEvent StopEvents;
		/// the events to trigger when the feedback is initialized
		[Tooltip("피드백이 초기화될 때 트리거할 이벤트")]
		public UnityEvent InitializationEvents;
		/// the events to trigger when the feedback is reset
		[Tooltip("피드백이 재설정될 때 트리거되는 이벤트")]
		public UnityEvent ResetEvents;

		/// <summary>
		/// On init, triggers the init events
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(GameObject owner)
		{
			base.CustomInitialization(owner);
			if (Active && (InitializationEvents != null))
			{
				InitializationEvents.Invoke();
			}
		}

		/// <summary>
		/// On Play, triggers the play events
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || (PlayEvents == null))
			{
				return;
			}
			PlayEvents.Invoke();    
		}

		/// <summary>
		/// On Stop, triggers the stop events
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || (StopEvents == null))
			{
				return;
			}
			StopEvents.Invoke();
		}

		/// <summary>
		/// On reset, triggers the reset events
		/// </summary>
		protected override void CustomReset()
		{
			if (!Active || !FeedbackTypeAuthorized || (ResetEvents == null))
			{
				return;
			}
			base.CustomReset();
			ResetEvents.Invoke();
		}
	}
}