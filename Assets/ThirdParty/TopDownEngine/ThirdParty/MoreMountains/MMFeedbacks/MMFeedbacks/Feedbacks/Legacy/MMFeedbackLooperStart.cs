using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 이 피드백은 일시 중지 역할을 할 수도 있지만 루프의 시작점 역할도 할 수 있습니다. 이 아래에 FeedbackLooper를 추가하고 몇 가지 피드백 후에 MMFeedback이 두 가지 사이에서 반복됩니다.
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("이 피드백은 일시 중지 역할을 할 수도 있지만 루프의 시작점 역할도 할 수 있습니다. 이 아래에 FeedbackLooper를 추가하고 몇 가지 피드백 후에 MMFeedback이 두 가지 사이에서 반복됩니다.")]
	[FeedbackPath("Loop/Looper Start")]
	public class MMFeedbackLooperStart : MMFeedbackPause
	{
		/// sets the color of this feedback in the inspector
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.LooperStartColor; } }
		#endif
		public override bool LooperStart { get { return true; } }

		/// the duration of this feedback is the duration of the pause
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(PauseDuration); } set { PauseDuration = value; } }

		/// <summary>
		/// Overrides the default value
		/// </summary>
		protected virtual void Reset()
		{
			PauseDuration = 0;
		}

		/// <summary>
		/// On play we run our pause
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
            
			StartCoroutine(PlayPause());
		}

	}
}