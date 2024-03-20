using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 이 피드백을 통해 지정된 시간 동안 대상 AudioMixer 스냅샷으로 전환할 수 있습니다.
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("이 피드백을 통해 지정된 시간 동안 대상 AudioMixer 스냅샷으로 전환할 수 있습니다.")]
	[FeedbackPath("Audio/AudioMixer Snapshot Transition")]
	public class MMF_AudioMixerSnapshotTransition : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.UIColor; } }
		public override bool EvaluateRequiresSetup() { return ((TargetSnapshot == null) || (OriginalSnapshot == null)); }
		public override string RequiredTargetText { get { return ((TargetSnapshot != null) && (OriginalSnapshot != null)) ? TargetSnapshot.name + " to "+ OriginalSnapshot.name : "";  } }
		public override string RequiresSetupText { get { return "This feedback requires that both a OriginalSnapshot and TargetSnapshot be set to be able to work properly. You can set these below."; } }
		#endif
        
		[MMFInspectorGroup("AudioMixer Snapshot", true, 44)]
		/// the target audio mixer snapshot we want to transition to 
		[Tooltip("전환하려는 대상 오디오 믹서 스냅샷")]
		public AudioMixerSnapshot TargetSnapshot;
		/// the audio mixer snapshot we want to transition from, optional, only needed if you plan to play this feedback in reverse 
		[Tooltip("전환하려는 오디오 믹서 스냅샷(선택 사항)은 이 피드백을 역방향으로 재생하려는 경우에만 필요합니다.")]
		public AudioMixerSnapshot OriginalSnapshot;
		/// the duration, in seconds, over which to transition to the selected snapshot
		[Tooltip("선택한 스냅샷으로 전환하는 기간(초)")]
		public float TransitionDuration = 1f;
        
		/// <summary>
		/// On play we transition to the selected snapshot
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			if (TargetSnapshot == null)
			{
				return;
			}

			if (!NormalPlayDirection)
			{
				if (OriginalSnapshot != null)
				{
					OriginalSnapshot.TransitionTo(TransitionDuration);     
				}
				else
				{
					TargetSnapshot.TransitionTo(TransitionDuration);
				}
			}
			else
			{
				TargetSnapshot.TransitionTo(TransitionDuration);     
			}
		}
	}
}