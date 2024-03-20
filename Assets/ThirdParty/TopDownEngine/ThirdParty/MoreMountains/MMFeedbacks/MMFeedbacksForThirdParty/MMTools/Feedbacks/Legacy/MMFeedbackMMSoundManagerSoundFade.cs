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
    /// 이 피드백을 사용하면 MMSoundManager를 통해 특정 사운드에 페이드를 트리거할 수 있습니다. 이 작업을 수행하려면 장면에 MMSoundManager가 필요합니다.
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackPath("Audio/MMSoundManager Sound Fade")]
	[FeedbackHelp("이 피드백을 사용하면 MMSoundManager를 통해 특정 사운드에 페이드를 트리거할 수 있습니다. 이 작업을 수행하려면 장면에 MMSoundManager가 필요합니다.")]
	public class MMFeedbackMMSoundManagerSoundFade : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.SoundsColor; } }
		#endif

		[Header("MMSoundManager Sound Fade")]
		/// the ID of the sound you want to fade. Has to match the ID you specified when playing the sound initially
		[Tooltip("페이드하려는 사운드의 ID입니다. 처음 사운드를 재생할 때 지정한 ID와 일치해야 합니다.")]
		public int SoundID = 0;
		/// the duration of the fade, in seconds
		[Tooltip("페이드 지속 시간(초)")]
		public float FadeDuration = 1f;
		/// the volume towards which to fade
		[Tooltip("페이드되는 볼륨")]
		[Range(MMSoundManagerSettings._minimalVolume,MMSoundManagerSettings._maxVolume)]
		public float FinalVolume = MMSoundManagerSettings._minimalVolume;
		/// the tween to apply over the fade
		[Tooltip("페이드 위에 적용할 트윈")]
		public MMTweenType FadeTween = new MMTweenType(MMTween.MMTweenCurve.EaseInOutQuartic);
        
		protected AudioSource _targetAudioSource;
        
		/// <summary>
		/// On play, we start our fade via a fade event
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
            
			MMSoundManagerSoundFadeEvent.Trigger(MMSoundManagerSoundFadeEvent.Modes.PlayFade, SoundID, FadeDuration, FinalVolume, FadeTween);
		}
	}
}