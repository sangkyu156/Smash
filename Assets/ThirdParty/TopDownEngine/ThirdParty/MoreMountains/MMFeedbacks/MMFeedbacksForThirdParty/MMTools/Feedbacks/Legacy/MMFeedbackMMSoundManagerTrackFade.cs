using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using MoreMountains.Tools;
using UnityEngine.Audio;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 이 피드백을 사용하면 특정 트랙의 모든 사운드를 한 번에 페이드할 수 있습니다. 이 작업을 수행하려면 장면에 MMSoundManager가 필요합니다.
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackPath("Audio/MMSoundManager Track Fade")]
	[FeedbackHelp("이 피드백을 사용하면 특정 트랙의 모든 사운드를 한 번에 페이드할 수 있습니다. 이 작업을 수행하려면 장면에 MMSoundManager가 필요합니다.")]
	public class MMFeedbackMMSoundManagerTrackFade : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.SoundsColor; } }
		#endif

		/// the duration of this feedback is the duration of the fade
		public override float FeedbackDuration { get { return FadeDuration; } }
        
		[Header("MMSoundManager Track Fade")]
		/// the track to fade the volume on
		[Tooltip("볼륨을 페이드할 트랙")]
		public MMSoundManager.MMSoundManagerTracks Track;
		/// the duration of the fade, in seconds
		[Tooltip("페이드 지속 시간(초)")]
		public float FadeDuration = 1f;
		/// the volume to reach at the end of the fade
		[Tooltip("페이드가 끝날 때 도달할 볼륨")]
		[Range(MMSoundManagerSettings._minimalVolume,MMSoundManagerSettings._maxVolume)]
		public float FinalVolume = MMSoundManagerSettings._minimalVolume;
		/// the tween to operate the fade on
		[Tooltip("페이드를 작동할 트윈")]
		public MMTweenType FadeTween = new MMTweenType(MMTween.MMTweenCurve.EaseInOutQuartic);
        
		/// <summary>
		/// On Play, triggers a fade event, meant to be caught by the MMSoundManager
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
            
			MMSoundManagerTrackFadeEvent.Trigger(MMSoundManagerTrackFadeEvent.Modes.PlayFade, Track, FadeDuration, FinalVolume, FadeTween);
		}
	}
}