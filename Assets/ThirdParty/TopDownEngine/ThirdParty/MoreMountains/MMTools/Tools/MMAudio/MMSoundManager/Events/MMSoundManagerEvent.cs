using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
	public enum MMSoundManagerEventTypes
	{
		SaveSettings,
		LoadSettings,
		ResetSettings,
		SettingsLoaded
	}

    /// <summary>
    /// 이 이벤트를 사용하면 MMSoundManager 설정에서 저장/로드/재설정을 트리거할 수 있습니다.
    ///
    /// Example : MMSoundManagerEvent.Trigger(MMSoundManagerEventTypes.SaveSettings);
    /// 설정을 저장합니다.
    /// </summary>
    public struct MMSoundManagerEvent
	{
		public MMSoundManagerEventTypes EventType;
        
		public MMSoundManagerEvent(MMSoundManagerEventTypes eventType)
		{
			EventType = eventType;
		}

		static MMSoundManagerEvent e;
		public static void Trigger(MMSoundManagerEventTypes eventType)
		{
			e.EventType = eventType;
			MMEventManager.TriggerEvent(e);
		}
	}
}