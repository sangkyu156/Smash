using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 클래스를 건강 클래스가 있는 캐릭터나 개체에 추가하면 해당 건강이 여기 설정에 따라 자동으로 채워집니다.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/Health/Health Auto Refill")]
	public class HealthAutoRefill : TopDownMonoBehaviour
	{
        /// 가능한 리필 모드 :
        /// - linear : 초당 특정 속도로 지속적인 체력 재충전
        /// - bursts : 주기적인 건강 폭발
        public enum RefillModes { Linear, Bursts }

		[Header("Mode")]
		/// the selected refill mode 
		[Tooltip("선택한 리필 모드")]
		public RefillModes RefillMode;

		[Header("Cooldown")]
		/// how much time, in seconds, should pass before the refill kicks in
		[Tooltip("리필이 시작되기 전에 얼마나 많은 시간(초)이 지나야 합니까?")]
		public float CooldownAfterHit = 1f;
        
		[Header("Refill Settings")]
		/// if this is true, health will refill itself when not at full health
		[Tooltip("이것이 사실이라면 체력이 완전히 회복되지 않았을 때 체력이 저절로 회복됩니다.")]
		public bool RefillHealth = true;
		/// the amount of health per second to restore when in linear mode
		[MMEnumCondition("RefillMode", (int)RefillModes.Linear)]
		[Tooltip("선형 모드에서 복원할 초당 체력 양")]
		public float HealthPerSecond;
		/// the amount of health to restore per burst when in burst mode
		[MMEnumCondition("RefillMode", (int)RefillModes.Bursts)]
		[Tooltip("버스트 모드에서 버스트당 회복할 체력의 양")]
		public float HealthPerBurst = 5;
		/// the duration between two health bursts, in seconds
		[MMEnumCondition("RefillMode", (int)RefillModes.Bursts)]
		[Tooltip("두 번의 체력 폭발 사이의 지속 시간(초)")]
		public float DurationBetweenBursts = 2f;

		protected Health _health;
		protected float _lastHitTime = 0f;
		protected float _healthToGive = 0f;
		protected float _lastBurstTimestamp;

		/// <summary>
		/// On Awake we do our init
		/// </summary>
		protected virtual void Awake()
		{
			Initialization();
		}

		/// <summary>
		/// On init we grab our Health component
		/// </summary>
		protected virtual void Initialization()
		{
			_health = this.gameObject.GetComponent<Health>();
		}

		/// <summary>
		/// On Update we refill
		/// </summary>
		protected virtual void Update()
		{
			ProcessRefillHealth();
		}  

		/// <summary>
		/// Tests if a refill is needed and processes it
		/// </summary>
		protected virtual void ProcessRefillHealth()
		{
			if (!RefillHealth)
			{
				return;
			}

			if (Time.time - _lastHitTime < CooldownAfterHit)
			{
				return;
			}

			if (_health.CurrentHealth < _health.MaximumHealth)
			{
				switch (RefillMode)
				{
					case RefillModes.Bursts:
						if (Time.time - _lastBurstTimestamp > DurationBetweenBursts)
						{
							_health.ReceiveHealth(HealthPerBurst, this.gameObject);
							_lastBurstTimestamp = Time.time;
						}
						break;

					case RefillModes.Linear:
						_healthToGive += HealthPerSecond * Time.deltaTime;
						if (_healthToGive > 1f)
						{
							float givenHealth = _healthToGive;
							_healthToGive -= givenHealth;
							_health.ReceiveHealth(givenHealth, this.gameObject);
						}
						break;
				}
			}
		}

		/// <summary>
		/// On hit we store our time
		/// </summary>
		public virtual void OnHit()
		{
			_lastHitTime = Time.time;
		}
        
		/// <summary>
		/// On enable we start listening for hits
		/// </summary>
		protected virtual void OnEnable()
		{
			_health.OnHit += OnHit;
		}

		/// <summary>
		/// On disable we stop listening for hits
		/// </summary>
		protected virtual void OnDisable()
		{
			_health.OnHit -= OnHit;
		}
	}
}