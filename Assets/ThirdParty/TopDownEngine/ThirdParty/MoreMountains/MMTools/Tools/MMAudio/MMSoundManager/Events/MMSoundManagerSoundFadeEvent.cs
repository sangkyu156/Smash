using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 이 이벤트를 통해 일시중지할 수 있습니다.
    ///
    /// Example : MMSoundManagerSoundFadeEvent.Trigger(33, 2f, 0.3f, new MMTweenType(MMTween.MMTweenCurve.EaseInElastic));
    /// 탄성 곡선에서 2초에 걸쳐 ID가 33인 사운드를 볼륨 0.3 방향으로 페이드합니다.
    /// </summary>
    public struct MMSoundManagerSoundFadeEvent
	{
		public enum Modes { PlayFade, StopFade }

		/// whether we are fading a sound, or stopping an existing fade
		public Modes Mode;
		/// the ID of the sound to fade
		public int SoundID;
		/// the duration of the fade (in seconds)
		public float FadeDuration;
		/// the volume towards which to fade this sound
		public float FinalVolume;
		/// the tween over which to fade this sound
		public MMTweenType FadeTween;
		
		
        
		public MMSoundManagerSoundFadeEvent(Modes mode, int soundID, float fadeDuration, float finalVolume, MMTweenType fadeTween)
		{
			Mode = mode;
			SoundID = soundID;
			FadeDuration = fadeDuration;
			FinalVolume = finalVolume;
			FadeTween = fadeTween;
		}

		static MMSoundManagerSoundFadeEvent e;
		public static void Trigger(Modes mode, int soundID, float fadeDuration, float finalVolume, MMTweenType fadeTween)
		{
			e.Mode = mode;
			e.SoundID = soundID;
			e.FadeDuration = fadeDuration;
			e.FinalVolume = finalVolume;
			e.FadeTween = fadeTween;
			MMEventManager.TriggerEvent(e);
		}
	}
}