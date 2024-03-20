using System;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
	public class MMShaker : MMMonoBehaviour
	{
		[MMInspectorGroup("Shaker Settings", true, 3)]
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
		/// the duration of the shake, in seconds
		[Tooltip("흔들림의 지속 시간(초)")]
		public float ShakeDuration = 0.2f;
		/// if this is true this shaker will play on awake
		[Tooltip("이것이 사실이라면 이 셰이커는 깨어 있을 것입니다")]
		public bool PlayOnAwake = false;
		/// if this is true, the shaker will shake permanently as long as its game object is active
		[Tooltip("이것이 사실이라면 게임 개체가 활성화되어 있는 동안 셰이커는 영구적으로 흔들릴 것입니다.")]
		public bool PermanentShake = false;
		/// if this is true, a new shake can happen while shaking
		[Tooltip("이것이 사실이라면 흔들면서 새로운 흔들림이 일어날 수 있습니다")]
		public bool Interruptible = true;
		/// if this is true, this shaker will always reset target values, regardless of how it was called
		[Tooltip("이것이 사실이라면 이 셰이커는 호출 방법에 관계없이 항상 목표 값을 재설정합니다.")]
		public bool AlwaysResetTargetValuesAfterShake = false;
		/// if this is true, this shaker will ignore any value passed in an event that triggered it, and will instead use the values set on its inspector
		[Tooltip("이것이 사실인 경우 이 셰이커는 이를 트리거한 이벤트에 전달된 모든 값을 무시하고 대신 해당 검사기에 설정된 값을 사용합니다.")]
		public bool OnlyUseShakerValues = false;
		/// a cooldown, in seconds, after a shake, during which no other shake can start
		[Tooltip("흔들기 후 다른 흔들림이 시작될 수 없는 쿨다운(초)")]
		public float CooldownBetweenShakes = 0f;
		/// whether or not this shaker is shaking right now
		[Tooltip("지금 이 셰이커가 흔들리고 있는지 아닌지")]
		[MMFReadOnly]
		public bool Shaking = false;
        
		[HideInInspector] 
		public bool ForwardDirection = true;

		[HideInInspector] 
		public TimescaleModes TimescaleMode = TimescaleModes.Scaled;

		public virtual float GetTime() { return (TimescaleMode == TimescaleModes.Scaled) ? Time.time : Time.unscaledTime; }
		public virtual float GetDeltaTime() { return (TimescaleMode == TimescaleModes.Scaled) ? Time.deltaTime : Time.unscaledDeltaTime; }
		public virtual MMChannelData ChannelData => new MMChannelData(ChannelMode, Channel, MMChannelDefinition);
        
		public bool ListeningToEvents => _listeningToEvents;

		[HideInInspector]
		internal bool _listeningToEvents = false;
		protected float _shakeStartedTimestamp = -Single.MaxValue;
		protected float _remappedTimeSinceStart;
		protected bool _resetShakerValuesAfterShake;
		protected bool _resetTargetValuesAfterShake;
		protected float _journey;
        
		/// <summary>
		/// On Awake we grab our volume and profile
		/// </summary>
		protected virtual void Awake()
		{
			Initialization();
			// in case someone else trigger StartListening before Awake
			if (!_listeningToEvents)
			{
				StartListening();
			}
			Shaking = PlayOnAwake;
			this.enabled = PlayOnAwake;
		}

		/// <summary>
		/// Override this method to initialize your shaker
		/// </summary>
		protected virtual void Initialization()
		{
		}

		/// <summary>
		/// Call this externally if you need to force a new initialization
		/// </summary>
		public virtual void ForceInitialization()
		{
			Initialization();
		}

		/// <summary>
		/// Starts shaking the values
		/// </summary>
		public virtual void StartShaking()
		{
			_journey = ForwardDirection ? 0f : ShakeDuration;

			if (GetTime() - _shakeStartedTimestamp < CooldownBetweenShakes)
			{
				return;
			}
            
			if (Shaking)
			{
				return;
			}
			else
			{
				this.enabled = true;
				_shakeStartedTimestamp = GetTime();
				Shaking = true;
				GrabInitialValues();
				ShakeStarts();
			}
		}

		/// <summary>
		/// Describes what happens when a shake starts
		/// </summary>
		protected virtual void ShakeStarts()
		{

		}

		/// <summary>
		/// A method designed to collect initial values
		/// </summary>
		protected virtual void GrabInitialValues()
		{

		}

		/// <summary>
		/// On Update, we shake our values if needed, or reset if our shake has ended
		/// </summary>
		protected virtual void Update()
		{
			if (Shaking || PermanentShake)
			{
				Shake();
				_journey += ForwardDirection ? GetDeltaTime() : -GetDeltaTime();
			}

			if (Shaking && !PermanentShake && ((_journey < 0) || (_journey > ShakeDuration)))
			{
				Shaking = false;
				ShakeComplete();
			}
		}

		/// <summary>
		/// Override this method to implement shake over time
		/// </summary>
		protected virtual void Shake()
		{

		}

		/// <summary>
		/// A method used to "shake" a flot over time along a curve
		/// </summary>
		/// <param name="curve"></param>
		/// <param name="remapMin"></param>
		/// <param name="remapMax"></param>
		/// <param name="relativeIntensity"></param>
		/// <param name="initialValue"></param>
		/// <returns></returns>
		protected virtual float ShakeFloat(AnimationCurve curve, float remapMin, float remapMax, bool relativeIntensity, float initialValue)
		{
			float newValue = 0f;
            
			float remappedTime = MMFeedbacksHelpers.Remap(_journey, 0f, ShakeDuration, 0f, 1f);
            
			float curveValue = curve.Evaluate(remappedTime);
			newValue = MMFeedbacksHelpers.Remap(curveValue, 0f, 1f, remapMin, remapMax);
			if (relativeIntensity)
			{
				newValue += initialValue;
			}
			return newValue;
		}

		/// <summary>
		/// Resets the values on the target
		/// </summary>
		protected virtual void ResetTargetValues()
		{

		}

		/// <summary>
		/// Resets the values on the shaker
		/// </summary>
		protected virtual void ResetShakerValues()
		{

		}

		/// <summary>
		/// Describes what happens when the shake is complete
		/// </summary>
		protected virtual void ShakeComplete()
		{
			if (_resetTargetValuesAfterShake || AlwaysResetTargetValuesAfterShake)
			{
				ResetTargetValues();
			}   
			if (_resetShakerValuesAfterShake)
			{
				ResetShakerValues();
			}            
			this.enabled = false;
		}

		/// <summary>
		/// On enable we start shaking if needed
		/// </summary>
		protected virtual void OnEnable()
		{
			StartShaking();
		}
             
		/// <summary>
		/// On destroy we stop listening for events
		/// </summary>
		protected virtual void OnDestroy()
		{
			StopListening();
		}

		/// <summary>
		/// On disable we complete our shake if it was in progress
		/// </summary>
		protected virtual void OnDisable()
		{
			if (Shaking)
			{
				ShakeComplete();
			}
		}

		/// <summary>
		/// Starts this shaker
		/// </summary>
		public virtual void Play()
		{
			if (GetTime() - _shakeStartedTimestamp < CooldownBetweenShakes)
			{
				return;
			}
			this.enabled = true;
		}

		/// <summary>
		/// Stops this shaker
		/// </summary>
		public virtual void Stop()
		{
			Shaking = false;
			ShakeComplete();
		}
        
		/// <summary>
		/// Starts listening for events
		/// </summary>
		public virtual void StartListening()
		{
			_listeningToEvents = true;
		}

		/// <summary>
		/// Stops listening for events
		/// </summary>
		public virtual void StopListening()
		{
			_listeningToEvents = false;
		}

		/// <summary>
		/// Returns true if this shaker should listen to events, false otherwise
		/// </summary>
		/// <param name="channel"></param>
		/// <returns></returns>
		protected virtual bool CheckEventAllowed(MMChannelData channelData, bool useRange = false, float range = 0f, Vector3 eventOriginPosition = default(Vector3))
		{
			if (!MMChannel.Match(channelData, ChannelMode, Channel, MMChannelDefinition))
			{
				return false;
			}
			if (!this.gameObject.activeInHierarchy)
			{
				return false;
			}
			else
			{
				if (useRange)
				{
					if (Vector3.Distance(this.transform.position, eventOriginPosition) > range)
					{
						return false;
					}
				}

				return true;
			}
		}
		
		public virtual float ComputeRangeIntensity(bool useRange, float rangeDistance, bool useRangeFalloff, AnimationCurve rangeFalloff, Vector2 remapRangeFalloff, Vector3 rangePosition)
		{
			if (!useRange)
			{
				return 1f;
			}

			float distanceToCenter = Vector3.Distance(rangePosition, this.transform.position);

			if (distanceToCenter > rangeDistance)
			{
				return 0f;
			}

			if (!useRangeFalloff)
			{
				return 1f;
			}

			float normalizedDistance = MMMaths.Remap(distanceToCenter, 0f, rangeDistance, 0f, 1f);
			float curveValue = rangeFalloff.Evaluate(normalizedDistance);
			float newIntensity = MMMaths.Remap(curveValue, 0f, 1f, remapRangeFalloff.x, remapRangeFalloff.y);
			return newIntensity;
		}
	}
}