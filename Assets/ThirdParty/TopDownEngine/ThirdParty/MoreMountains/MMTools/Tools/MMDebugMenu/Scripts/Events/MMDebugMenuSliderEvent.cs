using UnityEngine;

namespace MoreMountains.Tools
{
    /// <summary>
    /// MMDebugMenu에서 슬라이더 이벤트를 브로드캐스트하는 데 사용되는 이벤트
    /// </summary>
    public struct MMDebugMenuSliderEvent
	{
		static private event Delegate OnEvent;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
		static public void Register(Delegate callback) { OnEvent += callback; }
		static public void Unregister(Delegate callback) { OnEvent -= callback; }

		public enum EventModes { FromSlider, SetSlider }
		public delegate void Delegate(string sliderEventName, float value, EventModes eventMode = EventModes.FromSlider);
		static public void Trigger(string sliderEventName, float value, EventModes eventMode = EventModes.FromSlider)
		{
			OnEvent?.Invoke(sliderEventName, value, eventMode);
		}
	}
}