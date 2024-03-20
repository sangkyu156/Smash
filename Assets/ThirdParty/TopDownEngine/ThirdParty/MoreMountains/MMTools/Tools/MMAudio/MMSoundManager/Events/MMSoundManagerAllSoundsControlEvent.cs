using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
	public enum MMSoundManagerAllSoundsControlEventTypes
	{
		Pause, Play, Stop, Free, FreeAllButPersistent, FreeAllLooping
	}

    /// <summary>
    /// 이 이벤트를 사용하면 MMSoundManager를 통해 재생되는 모든 사운드를 한 번에 일시 중지/재생/중지/해제할 수 있습니다.
    ///
    /// Example : MMSoundManagerAllSoundsControlEvent.Trigger(MMSoundManagerAllSoundsControlEventTypes.Stop);
    /// 모든 소리 재생을 한 번에 중지합니다.
    /// </summary>
    public struct MMSoundManagerAllSoundsControlEvent
	{
		public MMSoundManagerAllSoundsControlEventTypes EventType;
        
		public MMSoundManagerAllSoundsControlEvent(MMSoundManagerAllSoundsControlEventTypes eventType)
		{
			EventType = eventType;
		}

		static MMSoundManagerAllSoundsControlEvent e;
		public static void Trigger(MMSoundManagerAllSoundsControlEventTypes eventType)
		{
			e.EventType = eventType;
			MMEventManager.TriggerEvent(e);
		}
	}
}