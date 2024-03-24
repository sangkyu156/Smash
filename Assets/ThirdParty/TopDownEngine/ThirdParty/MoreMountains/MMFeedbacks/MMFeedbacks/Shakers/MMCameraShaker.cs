using UnityEngine;
using System;
using MoreMountains.Tools;

namespace MoreMountains.Feedbacks
{
	[Serializable]
    /// <summary>
    /// Camera shake properties(속성)
    /// </summary>
    public struct MMCameraShakeProperties
	{
		public float Duration;
		public float Amplitude;
		public float Frequency;
		public float AmplitudeX;
		public float AmplitudeY;
		public float AmplitudeZ;

		public MMCameraShakeProperties(float duration, float amplitude, float frequency, float amplitudeX = 0f, float amplitudeY = 0f, float amplitudeZ = 0f)
		{
			Duration = duration;
			Amplitude = amplitude;
			Frequency = frequency;
			AmplitudeX = amplitudeX;
			AmplitudeY = amplitudeY;
			AmplitudeZ = amplitudeZ;
		}
	}

	public enum MMCameraZoomModes { For, Set, Reset }

	public struct MMCameraZoomEvent
	{
		static private event Delegate OnEvent;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
		static public void Register(Delegate callback) { OnEvent += callback; }
		static public void Unregister(Delegate callback) { OnEvent -= callback; }

		public delegate void Delegate(MMCameraZoomModes mode, float newFieldOfView, float transitionDuration, float duration, MMChannelData channelData, bool useUnscaledTime = false, bool stop = false, bool relative = false, bool restore = false, MMTweenType tweenType = null);

		static public void Trigger(MMCameraZoomModes mode, float newFieldOfView, float transitionDuration, float duration, MMChannelData channelData, bool useUnscaledTime = false, bool stop = false, bool relative = false, bool restore = false, MMTweenType tweenType = null)
		{
			OnEvent?.Invoke(mode, newFieldOfView, transitionDuration, duration, channelData, useUnscaledTime, stop, relative, restore, tweenType);
		}
	}

	public struct MMCameraShakeEvent
	{
		static private event Delegate OnEvent;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
		static public void Register(Delegate callback) { OnEvent += callback; }
		static public void Unregister(Delegate callback) { OnEvent -= callback; }

		public delegate void Delegate(float duration, float amplitude, float frequency, float amplitudeX, float amplitudeY, float amplitudeZ, bool infinite = false, MMChannelData channelData = null, bool useUnscaledTime = false);

		static public void Trigger(float duration, float amplitude, float frequency, float amplitudeX, float amplitudeY, float amplitudeZ, bool infinite = false, MMChannelData channelData = null, bool useUnscaledTime = false)
		{
			OnEvent?.Invoke(duration, amplitude, frequency, amplitudeX, amplitudeY, amplitudeZ, infinite, channelData, useUnscaledTime);
		}
	}

	public struct MMCameraShakeStopEvent
	{
		static private event Delegate OnEvent;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
		static public void Register(Delegate callback) { OnEvent += callback; }
		static public void Unregister(Delegate callback) { OnEvent -= callback; }

		public delegate void Delegate(MMChannelData channelData);

		static public void Trigger(MMChannelData channelData)
		{
			OnEvent?.Invoke(channelData);
		}
	}

	[RequireComponent(typeof(MMWiggle))]
	[AddComponentMenu("More Mountains/Feedbacks/Shakers/Camera/MMCameraShaker")]
    /// <summary>
    /// 카메라에 추가할 클래스입니다. MMCameraShakeEvents를 수신하고 이에 따라 카메라를 흔들게 됩니다.
    /// </summary>
    public class MMCameraShaker : MonoBehaviour
	{
		/// whether to listen on a channel defined by an int or by a MMChannel scriptable object. Ints are simple to setup but can get messy and make it harder to remember what int corresponds to what.
		/// MMChannel scriptable objects require you to create them in advance, but come with a readable name and are more scalable
		[Tooltip("int 또는 MMChannel 스크립트 가능 개체로 정의된 채널을 수신할지 여부입니다. Int는 설정이 간단하지만 지저분해질 수 있으며 int가 무엇에 해당하는지 기억하기 어렵게 만들 수 있습니다. " +
"MMChannel 스크립트 가능 개체를 미리 생성해야 하지만 읽기 쉬운 이름이 제공되고 확장성이 더 뛰어납니다.")]
		public MMChannelModes ChannelMode = MMChannelModes.Int;
		/// the channel to listen to - has to match the one on the feedback
		[Tooltip("들을 채널 - 피드백에 있는 채널과 일치해야 합니다.")]
		[MMEnumCondition("ChannelMode", (int)MMChannelModes.Int)]
		public int Channel = 0;
		/// the MMChannel definition asset to use to listen for events. The feedbacks targeting this shaker will have to reference that same MMChannel definition to receive events - to create a MMChannel,
		/// right click anywhere in your project (usually in a Data folder) and go MoreMountains > MMChannel, then name it with some unique name
		[Tooltip("이벤트를 수신하는 데 사용할 MMChannel 정의 자산입니다. 이 셰이커를 대상으로 하는 피드백은 이벤트를 수신하기 위해 동일한 MMChannel 정의를 참조해야 합니다. MMChannel을 생성하려면 " +
"프로젝트(일반적으로 Data 폴더)의 아무 곳이나 마우스 오른쪽 버튼으로 클릭하고 MoreMountains > MMChannel로 이동한 다음 고유한 이름으로 이름을 지정합니다.")]
		[MMEnumCondition("ChannelMode", (int)MMChannelModes.MMChannel)]
		public MMChannel MMChannelDefinition = null;
		/// a cooldown, in seconds, after a shake, during which no other shake can start
		[Tooltip("흔들기 후 다른 흔들림이 시작될 수 없는 쿨다운(초)")]
		public float CooldownBetweenShakes = 0f;
	    
		protected MMWiggle _wiggle;
		protected float _shakeStartedTimestamp = -Single.MaxValue;

		/// <summary>
		/// On Awake, grabs the MMShaker component
		/// </summary>
		protected virtual void Awake()
		{
			_wiggle = GetComponent<MMWiggle>();
		}

		/// <summary>
		/// Shakes the camera for Duration seconds, by the desired amplitude and frequency
		/// </summary>
		/// <param name="duration">Duration.</param>
		/// <param name="amplitude">Amplitude.</param>
		/// <param name="frequency">Frequency.</param>
		public virtual void ShakeCamera(float duration, float amplitude, float frequency, float amplitudeX, float amplitudeY, float amplitudeZ, bool useUnscaledTime)
		{
			if (Time.unscaledTime - _shakeStartedTimestamp < CooldownBetweenShakes)
			{
				return;
			}
			
			if ((amplitudeX != 0f) || (amplitudeY != 0f) || (amplitudeZ != 0f))
			{
				_wiggle.PositionWiggleProperties.AmplitudeMin.x = -amplitudeX;
				_wiggle.PositionWiggleProperties.AmplitudeMin.y = -amplitudeY;
				_wiggle.PositionWiggleProperties.AmplitudeMin.z = -amplitudeZ;
                
				_wiggle.PositionWiggleProperties.AmplitudeMax.x = amplitudeX;
				_wiggle.PositionWiggleProperties.AmplitudeMax.y = amplitudeY;
				_wiggle.PositionWiggleProperties.AmplitudeMax.z = amplitudeZ;
			}
			else
			{
				_wiggle.PositionWiggleProperties.AmplitudeMin = Vector3.one * -amplitude;
				_wiggle.PositionWiggleProperties.AmplitudeMax = Vector3.one * amplitude;
			}

			_shakeStartedTimestamp = Time.time;
			_wiggle.PositionWiggleProperties.UseUnscaledTime = useUnscaledTime;
			_wiggle.PositionWiggleProperties.FrequencyMin = frequency;
			_wiggle.PositionWiggleProperties.FrequencyMax = frequency;
			_wiggle.PositionWiggleProperties.NoiseFrequencyMin = frequency * Vector3.one;
			_wiggle.PositionWiggleProperties.NoiseFrequencyMax = frequency * Vector3.one;
			_wiggle.WigglePosition(duration);
		}

		/// <summary>
		/// When a MMCameraShakeEvent is caught, shakes the camera
		/// </summary>
		/// <param name="shakeEvent">Shake event.</param>
		public virtual void OnCameraShakeEvent(float duration, float amplitude, float frequency, float amplitudeX, float amplitudeY, float amplitudeZ, bool infinite, MMChannelData channelData, bool useUnscaledTime)
		{
			if (!MMChannel.Match(channelData, ChannelMode, Channel, MMChannelDefinition))
			{
				return;
			}
			this.ShakeCamera (duration, amplitude, frequency, amplitudeX, amplitudeY, amplitudeZ, useUnscaledTime);
		}

		/// <summary>
		/// On enable, starts listening for events
		/// </summary>
		protected virtual void OnEnable()
		{
			MMCameraShakeEvent.Register(OnCameraShakeEvent);
		}

		/// <summary>
		/// On disable, stops listening to events
		/// </summary>
		protected virtual void OnDisable()
		{
			MMCameraShakeEvent.Unregister(OnCameraShakeEvent);
		}

	}
}