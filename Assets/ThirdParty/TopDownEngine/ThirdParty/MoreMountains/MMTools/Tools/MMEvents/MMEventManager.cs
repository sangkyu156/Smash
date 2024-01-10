//#define EVENTROUTER_THROWEXCEPTIONS 
#if EVENTROUTER_THROWEXCEPTIONS
//#define EVENTROUTER_REQUIRELISTENER // Uncomment this if you want listeners to be required for sending events.
#endif

using System;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    /// <summary>
    /// MMGameEvents는 게임 전반에 걸쳐 일반 게임 이벤트(게임 시작, 게임 종료, 사망 등)에 사용됩니다.
    /// </summary>
    public struct MMGameEvent
    {
        public string EventName;
        public MMGameEvent(string newName)
        {
            EventName = newName;
        }
        static MMGameEvent e;
        public static void Trigger(string newName)
        {
            e.EventName = newName;
            MMEventManager.TriggerEvent(e);
        }
    }

    /// <summary>
    /// 이 클래스는 이벤트 관리를 처리하고, 게임 전반에 걸쳐 이벤트를 브로드캐스팅하고, 
    /// 한 클래스(또는 여러 클래스)에 어떤 일이 발생했음을 알리는 데 사용할 수 있습니다.
    /// 이벤트는 구조체이므로 원하는 모든 종류의 이벤트를 정의할 수 있습니다.
    /// 이 관리자는 기본적으로 문자열로만 구성된 MMGameEvents와 함께 제공되지만 원하는 경우 더 복잡한 문자열로 작업할 수도 있습니다.
    /// 
    /// 어디서든 새 이벤트를 트리거하려면 YOUR_EVENT.Trigger(YOUR_PARAMETERS)를 수행하세요.
    /// 따라서 MMGameEvent.Trigger("Save"); 예를 들어 Save MMGameEvent를 트리거합니다.
    /// 
    /// MMEventManager.TriggerEvent(YOUR_EVENT)를 호출할 수도 있습니다.
    /// 예: MMEventManager.TriggerEvent(new MMGameEvent("GameStart")); GameStart라는 MMGameEvent를 모든 리스너에게 브로드캐스팅합니다.
    /// 
    /// 어떤 수업에서든 이벤트 듣기를 시작하려면 다음 3가지 작업을 수행해야 합니다.
    ///
    /// 1 - 클래스가 해당 종류의 이벤트에 대해 MMEventListener 인터페이스를 구현한다고 알려줍니다.
    /// 예: public class GUIManager : Singleton<GUIManager>, MMEventListener<MMGameEvent>
    /// 이 중 하나 이상을 가질 수 있습니다(이벤트 유형당 하나씩).
    /// 
    /// 2 - 활성화 및 비활성화에서 각각 이벤트 수신을 시작하고 중지합니다.
    /// void OnEnable()
    /// {
    /// 	this.MMEventStartListening<MMGameEvent>();
    /// }
    /// void OnDisable()
    /// {
    /// 	this.MMEventStopListening<MMGameEvent>();
    /// }
    /// 
    /// 3 - 해당 이벤트에 대한 MMEventListener 인터페이스를 구현합니다.
    /// 예시 ↓
    /// public void OnMMEvent(MMGameEvent gameEvent)
    /// {
    /// 	if (gameEvent.EventName == "GameOver")
    ///		{
    ///			// DO SOMETHING
    ///		}
    /// } 
    /// 게임의 어느 곳에서나 발생하는 MMGameEvent 유형의 모든 이벤트를 포착하고 이름이 GameOver인 경우 작업을 수행합니다.
    /// </summary>
    [ExecuteAlways]
    public static class MMEventManager
    {
        private static Dictionary<Type, List<MMEventListenerBase>> _subscribersList;

        static MMEventManager()
        {
            _subscribersList = new Dictionary<Type, List<MMEventListenerBase>>();
        }

        /// <summary>
        /// 특정 이벤트에 새로운 구독자를 추가합니다.
        /// </summary>
        /// <param name="listener">listener.</param>
        /// <typeparam name="MMEvent">The event type.</typeparam>
        public static void AddListener<MMEvent>(MMEventListener<MMEvent> listener) where MMEvent : struct
        {
            Type eventType = typeof(MMEvent);

            if (!_subscribersList.ContainsKey(eventType))
            {
                _subscribersList[eventType] = new List<MMEventListenerBase>();
            }

            if (!SubscriptionExists(eventType, listener))
            {
                _subscribersList[eventType].Add(listener);
            }
        }

        /// <summary>
        /// 특정 이벤트에서 구독자를 제거합니다.
        /// </summary>
        /// <param name="listener">listener.</param>
        /// <typeparam name="MMEvent">The event type.</typeparam>
        public static void RemoveListener<MMEvent>(MMEventListener<MMEvent> listener) where MMEvent : struct
        {
            Type eventType = typeof(MMEvent);

            if (!_subscribersList.ContainsKey(eventType))
            {
#if EVENTROUTER_THROWEXCEPTIONS
					throw new ArgumentException( string.Format( "Removing listener \"{0}\", but the event type \"{1}\" isn't registered.", listener, eventType.ToString() ) );
#else
                return;
#endif
            }

            List<MMEventListenerBase> subscriberList = _subscribersList[eventType];

#if EVENTROUTER_THROWEXCEPTIONS
	            bool listenerFound = false;
#endif

            for (int i = subscriberList.Count - 1; i >= 0; i--)
            {
                if (subscriberList[i] == listener)
                {
                    subscriberList.Remove(subscriberList[i]);
#if EVENTROUTER_THROWEXCEPTIONS
					    listenerFound = true;
#endif

                    if (subscriberList.Count == 0)
                    {
                        _subscribersList.Remove(eventType);
                    }

                    return;
                }
            }

#if EVENTROUTER_THROWEXCEPTIONS
		        if( !listenerFound )
		        {
					throw new ArgumentException( string.Format( "Removing listener, but the supplied receiver isn't subscribed to event type \"{0}\".", eventType.ToString() ) );
		        }
#endif
        }

        /// <summary>
        /// 이벤트를 트리거합니다. 이를 구독하는 모든 인스턴스가 이를 수신합니다.(그리고 잠재적으로 이에 따라 조치를 취할 것입니다).
        /// </summary>
        /// <param name="newEvent">The event to trigger.</param>
        /// <typeparam name="MMEvent">The 1st type parameter.</typeparam>
        public static void TriggerEvent<MMEvent>(MMEvent newEvent) where MMEvent : struct
        {
            List<MMEventListenerBase> list;
            if (!_subscribersList.TryGetValue(typeof(MMEvent), out list))
#if EVENTROUTER_REQUIRELISTENER
			            throw new ArgumentException( string.Format( "Attempting to send event of type \"{0}\", but no listener for this type has been found. Make sure this.Subscribe<{0}>(EventRouter) has been called, or that all listeners to this event haven't been unsubscribed.", typeof( MMEvent ).ToString() ) );
#else
                return;
#endif

            for (int i = list.Count - 1; i >= 0; i--)
            {
                (list[i] as MMEventListener<MMEvent>).OnMMEvent(newEvent);
            }
        }

        /// <summary>
        /// 특정 유형의 이벤트에 대한 구독자가 있는지 확인합니다.
        /// </summary>
        /// <returns>구독 중인 존재가 있으면 true이고, 그렇지 않으면 false입니다.</returns>
        /// <param name="type">Type.</param>
        /// <param name="receiver">Receiver.</param>
        private static bool SubscriptionExists(Type type, MMEventListenerBase receiver)
        {
            List<MMEventListenerBase> receivers;

            if (!_subscribersList.TryGetValue(type, out receivers)) return false;

            bool exists = false;

            for (int i = receivers.Count - 1; i >= 0; i--)
            {
                if (receivers[i] == receiver)
                {
                    exists = true;
                    break;
                }
            }

            return exists;
        }
    }

    /// <summary>
    /// 모든 클래스가 이벤트 수신을 시작하거나 중지할 수 있도록 하는 정적 클래스
    /// </summary>
    public static class EventRegister
    {
        public delegate void Delegate<T>(T eventType);

        public static void MMEventStartListening<EventType>(this MMEventListener<EventType> caller) where EventType : struct
        {
            MMEventManager.AddListener<EventType>(caller);
        }

        public static void MMEventStopListening<EventType>(this MMEventListener<EventType> caller) where EventType : struct
        {
            MMEventManager.RemoveListener<EventType>(caller);
        }
    }

    /// <summary>
    /// 이벤트 리스너 기본 인터페이스
    /// </summary>
    public interface MMEventListenerBase { };

    /// <summary>
    /// 듣고 싶은 각 이벤트 유형에 대해 구현해야 하는 공개 인터페이스입니다.
    /// </summary>
    public interface MMEventListener<T> : MMEventListenerBase
    {
        void OnMMEvent(T eventType);
    }

    public class MMEventListenerWrapper<TOwner, TTarget, TEvent> : MMEventListener<TEvent>, IDisposable
        where TEvent : struct
    {
        private Action<TTarget> _callback;

        private TOwner _owner;
        public MMEventListenerWrapper(TOwner owner, Action<TTarget> callback)
        {
            _owner = owner;
            _callback = callback;
            RegisterCallbacks(true);
        }

        public void Dispose()
        {
            RegisterCallbacks(false);
            _callback = null;
        }

        protected virtual TTarget OnEvent(TEvent eventType) => default;
        public void OnMMEvent(TEvent eventType)
        {
            var item = OnEvent(eventType);
            _callback?.Invoke(item);
        }

        private void RegisterCallbacks(bool b)
        {
            if (b)
            {
                this.MMEventStartListening<TEvent>();
            }
            else
            {
                this.MMEventStopListening<TEvent>();
            }
        }
    }
}