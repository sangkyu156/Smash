using UnityEngine;

namespace MoreMountains.Tools
{
    /// <summary>
    /// MMDebugMenu에서 체크박스 이벤트를 브로드캐스트하는 데 사용되는 이벤트
    /// </summary>
    public struct MMDebugMenuCheckboxEvent
	{
		static private event Delegate OnEvent;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
		static public void Register(Delegate callback) { OnEvent += callback; }
		static public void Unregister(Delegate callback) { OnEvent -= callback; }

		public enum EventModes { FromCheckbox, SetCheckbox }
		public delegate void Delegate(string checkboxEventName, bool value, EventModes eventMode = EventModes.FromCheckbox);
		static public void Trigger(string checkboxEventName, bool value, EventModes eventMode = EventModes.FromCheckbox)
		{
			OnEvent?.Invoke(checkboxEventName, value, eventMode);
		}
	}
}