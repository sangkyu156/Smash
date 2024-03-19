using UnityEngine;

namespace MoreMountains.Tools
{
    /// <summary>
    /// MMDebugMenu에서 버튼 이벤트를 브로드캐스트하는 데 사용되는 이벤트
    /// </summary>
    public struct MMDebugMenuButtonEvent
	{
		public enum EventModes { FromButton, SetButton }

		static private event Delegate OnEvent;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
		static public void Register(Delegate callback) { OnEvent += callback; }
		static public void Unregister(Delegate callback) { OnEvent -= callback; }

		public delegate void Delegate(string buttonEventName, bool active = true, EventModes eventMode = EventModes.FromButton);
		static public void Trigger(string buttonEventName, bool active = true, EventModes eventMode = EventModes.FromButton)
		{
			OnEvent?.Invoke(buttonEventName, active, eventMode);
		}
	}
}