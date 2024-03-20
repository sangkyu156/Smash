using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
	public enum MMSoundManagerSoundControlEventTypes
	{
		Pause,
		Resume,
		Stop,
		Free
	}

    /// <summary>
    /// MMSoundManager에서 특정 사운드를 제어하는 ​​데 사용되는 이벤트입니다.
    /// ID로 검색할 수도 있고, 오디오 소스가 있는 경우 직접 전달할 수도 있습니다.
    ///
    /// Example : MMSoundManagerSoundControlEvent.Trigger(MMSoundManagerSoundControlEventTypes.Stop, 33);
    /// ID가 33인 사운드의 재생이 중지됩니다.
    /// </summary>
    public struct MMSoundManagerSoundControlEvent
	{
		/// the ID of the sound to control (has to match the one used to play it)
		public int SoundID;
		/// the control mode
		public MMSoundManagerSoundControlEventTypes MMSoundManagerSoundControlEventType;
		/// the audiosource to control (if specified)
		public AudioSource TargetSource;
        
		public MMSoundManagerSoundControlEvent(MMSoundManagerSoundControlEventTypes eventType, int soundID, AudioSource source = null)
		{
			SoundID = soundID;
			TargetSource = source;
			MMSoundManagerSoundControlEventType = eventType;
		}

		static MMSoundManagerSoundControlEvent e;
		public static void Trigger(MMSoundManagerSoundControlEventTypes eventType, int soundID, AudioSource source = null)
		{
			e.SoundID = soundID;
			e.TargetSource = source;
			e.MMSoundManagerSoundControlEventType = eventType;
			MMEventManager.TriggerEvent(e);
		}
	}
}