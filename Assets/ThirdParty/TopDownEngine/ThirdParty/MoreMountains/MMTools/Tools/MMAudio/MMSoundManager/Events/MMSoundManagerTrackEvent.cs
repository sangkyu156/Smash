using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
	public enum MMSoundManagerTrackEventTypes
	{
		MuteTrack,
		UnmuteTrack,
		SetVolumeTrack,
		PlayTrack,
		PauseTrack,
		StopTrack,
		FreeTrack
	}

    /// <summary>
    /// 이 피드백을 통해 선택한 트랙의 음소거, 음소거 해제, 재생, 일시중지, 중지, 해제 또는 볼륨 설정을 수행할 수 있습니다.
    ///
    /// Example :  MMSoundManagerTrackEvent.Trigger(MMSoundManagerTrackEventTypes.PauseTrack,MMSoundManager.MMSoundManagerTracks.UI);
    /// 전체 UI 트랙을 일시중지합니다.
    /// </summary>
    public struct MMSoundManagerTrackEvent
	{
		/// the order to pass to the track
		public MMSoundManagerTrackEventTypes TrackEventType;
		/// the track to pass the order to
		public MMSoundManager.MMSoundManagerTracks Track;
		/// if in SetVolume mode, the volume to which to set the track to
		public float Volume;
        
		public MMSoundManagerTrackEvent(MMSoundManagerTrackEventTypes trackEventType, MMSoundManager.MMSoundManagerTracks track = MMSoundManager.MMSoundManagerTracks.Master, float volume = 1f)
		{
			TrackEventType = trackEventType;
			Track = track;
			Volume = volume;
		}

		static MMSoundManagerTrackEvent e;
		public static void Trigger(MMSoundManagerTrackEventTypes trackEventType, MMSoundManager.MMSoundManagerTracks track = MMSoundManager.MMSoundManagerTracks.Master, float volume = 1f)
		{
			e.TrackEventType = trackEventType;
			e.Track = track;
			e.Volume = volume;
			MMEventManager.TriggerEvent(e);
		}
	}
}