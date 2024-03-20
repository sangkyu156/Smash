using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 직접적으로 또는 이벤트를 통해 MMRadioReceiver에 레벨을 브로드캐스트하는 데 사용되는 클래스
    /// 모든 클래스의 거의 모든 값을 읽을 수 있습니다.
    /// </summary>
    [MMRequiresConstantRepaint]
	public class MMRadioBroadcaster : MMMonoBehaviour
	{
		[Header("Source")]
		/// the emitter to read the level on
		public MMPropertyEmitter Emitter;

		[Header("Destinations")]
		/// a list of receivers hardwired to this broadcaster, that will receive the level at runtime
		public MMRadioReceiver[] Receivers;

		[Header("Channel Broadcasting")]
		/// whether or not this broadcaster should use events to broadcast its level on the specified channel
		public bool BroadcastOnChannel = true;
		/// the channel to broadcast on, has to match the Channel on the target receivers
		[MMCondition("BroadcastOnChannel", true)]
		public int Channel = 0;
		/// whether to broadcast all the time, or only when the value changes (lighter on performance, but won't "lock" the value)
		[MMCondition("BroadcastOnChannel", true)]
		public bool OnlyBroadcastOnValueChange = true;
        
		/// a delegate to handle value changes
		public delegate void OnValueChangeDelegate();
		/// what to do on value change
		public OnValueChangeDelegate OnValueChange;

		protected float _levelLastFrame = 0f;

		/// <summary>
		/// On Awake we initialize our emitter
		/// </summary>
		protected virtual void Awake()
		{
			Emitter.Initialization(this.gameObject);
		}

		/// <summary>
		/// On Update we process our broadcast
		/// </summary>
		protected virtual void Update()
		{
			ProcessBroadcast();
		}

		/// <summary>
		/// Broadcasts the value if needed
		/// </summary>
		protected virtual void ProcessBroadcast()
		{
			if (Emitter == null)
			{
				return;
			}

			float level = Emitter.GetLevel();

			if (level != _levelLastFrame)
			{
				// we trigger a value change event
				OnValueChange?.Invoke();

				// for each of our receivers, we set the level manually
				foreach (MMRadioReceiver receiver in Receivers)
				{
					receiver?.SetLevel(level);
				}

				// we broadcast an event
				if (BroadcastOnChannel)
				{
					MMRadioLevelEvent.Trigger(Channel, level);
				}
			}           

			_levelLastFrame = level;
		}
	}

	/// <summary>
	/// A struct event used to broadcast the level to channels
	/// </summary>
	public struct MMRadioLevelEvent
	{
		static private event Delegate OnEvent;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
		static public void Register(Delegate callback) { OnEvent += callback; }
		static public void Unregister(Delegate callback) { OnEvent -= callback; }

		public delegate void Delegate(int channel, float level);
		static public void Trigger(int channel, float level)
		{
			OnEvent?.Invoke(channel, level);
		}
	}
}