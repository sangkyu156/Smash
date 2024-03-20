using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 이 피드백은 모든 이전 피드백이 실행될 때까지 "보류"하거나 대기한 다음 지정된 기간 동안 MMFeedbacks 시퀀스의 실행을 일시 중지합니다.
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("이 피드백은 이전 피드백이 모두 실행될 때까지 '유지'하거나 대기한 다음 지정된 기간 동안 MMFeedbacks 시퀀스의 실행을 일시 중지합니다.")]
	[FeedbackPath("Pause/Holding Pause")]
	public class MMF_HoldingPause : MMF_Pause
	{
		/// sets the color of this feedback in the inspector
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.HoldingPauseColor; } }
		#endif
		public override bool HoldingPause { get { return true; } }
                
		/// the duration of this feedback is the duration of the pause
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(PauseDuration); } set { PauseDuration = value; } }
        
		/// <summary>
		/// On custom play we just play our pause
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (Active)
			{
				Owner.StartCoroutine(PlayPause());
			}
		}
	}
}