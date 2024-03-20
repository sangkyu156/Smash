using UnityEngine;
using UnityEngine.Events;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 이 컴포넌트를 객체에 추가하면 지정된 이름의 이벤트가 트리거될 때 UnityEvents를 쉽게 트리거할 수 있습니다.
    /// </summary>
    public class MMGameEventListener : MonoBehaviour, MMEventListener<MMGameEvent>
	{
		[Header("MMGameEvent")] 
		/// the name of the event you want to listen for
		[Tooltip("듣고 싶은 이벤트 이름")]
		public string EventName = "Load";
		/// a UnityEvent hook you can use to call methods when the specified event gets triggered
		[Tooltip("지정된 이벤트가 트리거될 때 메소드를 호출하는 데 사용할 수 있는 UnityEvent 후크")]
		public UnityEvent OnMMGameEvent;
		
		/// <summary>
		/// When a MMGameEvent happens, we trigger our UnityEvent if necessary
		/// </summary>
		/// <param name="gameEvent"></param>
		public void OnMMEvent(MMGameEvent gameEvent)
		{
			if (gameEvent.EventName == EventName)
			{
				OnMMGameEvent?.Invoke();
			}
		}

		/// <summary>
		/// On enable, we start listening for MMGameEvents. You may want to extend that to listen to other types of events.
		/// </summary>
		protected virtual void OnEnable()
		{
			this.MMEventStartListening<MMGameEvent>();
		}

		/// <summary>
		/// On disable, we stop listening for MMGameEvents. You may want to extend that to stop listening to other types of events.
		/// </summary>
		protected virtual void OnDisable()
		{
			this.MMEventStopListening<MMGameEvent>();
		}
	}	
}