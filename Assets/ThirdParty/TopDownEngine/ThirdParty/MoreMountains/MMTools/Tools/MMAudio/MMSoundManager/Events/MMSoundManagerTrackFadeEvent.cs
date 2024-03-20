using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 이 이벤트를 사용하면 MMSoundManager가 전체 트랙의 사운드 볼륨을 지정된 FinalVolume 방향으로 페이드하도록 명령할 수 있습니다.
    ///
    /// Example : MMSoundManagerTrackFadeEvent.Trigger(MMSoundManager.MMSoundManagerTracks.Music, 2f, 0.5f, new MMTweenType(MMTween.MMTweenCurve.EaseInCubic));
    /// 큐빅 트윈의 이즈를 사용하여 음악 트랙의 볼륨을 2초에 걸쳐 0.5 방향으로 페이드합니다.
    /// </summary>
    public struct MMSoundManagerTrackFadeEvent
	{
		public enum Modes { PlayFade, StopFade }

		/// whether we are fading a sound, or stopping an existing fade
		public Modes Mode;
		/// the track to fade the volume of
		public MMSoundManager.MMSoundManagerTracks Track;
		/// the duration of the fade, in seconds
		public float FadeDuration;
		/// the final volume to fade towards
		public float FinalVolume;
		/// the tween to use when fading
		public MMTweenType FadeTween;
        
		public MMSoundManagerTrackFadeEvent(Modes mode, MMSoundManager.MMSoundManagerTracks track, float fadeDuration, float finalVolume, MMTweenType fadeTween)
		{
			Mode = mode;
			Track = track;
			FadeDuration = fadeDuration;
			FinalVolume = finalVolume;
			FadeTween = fadeTween;
		}

		static MMSoundManagerTrackFadeEvent e;
		public static void Trigger(Modes mode, MMSoundManager.MMSoundManagerTracks track, float fadeDuration, float finalVolume, MMTweenType fadeTween)
		{
			e.Mode = mode;
			e.Track = track;
			e.FadeDuration = fadeDuration;
			e.FinalVolume = finalVolume;
			e.FadeTween = fadeTween;
			MMEventManager.TriggerEvent(e);
		}
	}
}