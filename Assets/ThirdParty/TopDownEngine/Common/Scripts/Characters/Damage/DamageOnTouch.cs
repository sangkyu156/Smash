using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;
using System;
using MoreMountains.Feedbacks;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 구성 요소를 개체에 추가하면 개체와 충돌하는 개체에 손상을 입힙니다.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/Damage/DamageOnTouch")]
	public class DamageOnTouch : MMMonoBehaviour
	{
		[Flags]
		public enum TriggerAndCollisionMask
		{
			IgnoreAll = 0,
			OnTriggerEnter = 1 << 0,
			OnTriggerStay = 1 << 1,
			OnTriggerEnter2D = 1 << 6,
			OnTriggerStay2D = 1 << 7,

			All_3D = OnTriggerEnter | OnTriggerStay,
			All_2D = OnTriggerEnter2D | OnTriggerStay2D,
			All = All_3D | All_2D
		}

        /// 넉백을 추가할 수 있는 방법: noKnockback, 아무 작업도 수행하지 않고 힘을 설정하거나 힘을 추가하지 않습니다.
        public enum KnockbackStyles
		{
			NoKnockback,
			AddForce
		}

        /// 가능한 넉백 방향
        public enum KnockbackDirections
		{
			BasedOnOwnerPosition,//소유자 위치 기준
            BasedOnSpeed,//속도 기준
            BasedOnDirection,//방향 기반
            BasedOnScriptDirection//스크립트 방향에 따라
        }

        /// 손상 방향을 결정하는 가능한 방법
        public enum DamageDirections
		{
			BasedOnOwnerPosition,//소유자 위치 기준
            BasedOnVelocity,//속도 기준
            BasedOnScriptDirection//스크립트 방향에 따라
        }

		public const TriggerAndCollisionMask AllowedTriggerCallbacks = TriggerAndCollisionMask.OnTriggerEnter
		                                                                  | TriggerAndCollisionMask.OnTriggerStay
		                                                                  | TriggerAndCollisionMask.OnTriggerEnter2D
		                                                                  | TriggerAndCollisionMask.OnTriggerStay2D;

		[MMInspectorGroup("Targets", true, 3)]
		[MMInformation(
            "이 구성 요소는 개체가 충돌하는 개체에 손상을 입히도록 만듭니다. 여기에서 피해의 영향을 받는 레이어(표준 적의 경우 플레이어 선택), 줄 피해량, 적중 시 피해를 받는 개체에 적용할 힘의 양을 정의할 수 있습니다. 적중 후 무적이 지속되는 시간(초)을 지정할 수도 있습니다.",
			MMInformationAttribute.InformationType.Info, false)]
        /// 이 개체에 의해 손상될 레이어
        [Tooltip("이 개체에 의해 손상될 레이어")]
		public LayerMask TargetLayerMask;
        /// DamageOnTouch 구역의 소유자
        [MMReadOnly] [Tooltip("DamageOnTouch 구역의 소유자")]
		public GameObject Owner;

        /// 기본적으로 입력 및 유지(2D 및 3D 모두) 시 피해를 적용해야 하는 트리거를 정의하지만 이 필드를 사용하면 필요한 경우 트리거를 제외할 수 있습니다.
        [Tooltip(
            "기본적으로 입력 및 유지(2D 및 3D 모두) 시 손상을 적용해야 하는 트리거를 정의하지만 이 필드를 사용하면 필요한 경우 트리거를 제외할 수 있습니다.")]
		public TriggerAndCollisionMask TriggerFilter = AllowedTriggerCallbacks;

		[MMInspectorGroup("데미지 수치", true, 8)]
        /// 플레이어의 체력에서 제거할 최소 체력입니다.
        [FormerlySerializedAs("데미지 수치")]
		[Tooltip("플레이어의 체력에서 제거할 최소 체력입니다.")]
		public float MinDamageCaused = 10f;
        /// 플레이어의 체력에서 제거할 최대 체력
        [Tooltip("플레이어의 체력에서 제거할 최대 체력")]
		public float MaxDamageCaused = 10f;
        /// 기본 손상 위에 적용될 유형 손상 정의 목록
        [Tooltip("기본 손상 위에 적용될 유형 손상 정의 목록")]
		public List<TypedDamage> TypedDamages;
        /// 건강 피해 방법에 전달되는 피해 방향을 결정하는 방법은 일반적으로 피해 영역(발사체) 이동에 속도를 사용하고 근접 무기에 소유자 위치를 사용합니다.
        [Tooltip("건강 피해 방법에 전달되는 피해 방향을 결정하는 방법은 일반적으로 피해 영역(발사체) 이동에 속도를 사용하고 근접 무기에 소유자 위치를 사용합니다.")]
		public DamageDirections DamageDirectionMode = DamageDirections.BasedOnVelocity;
		
		[Header("Knockback")]
        /// 피해를 입힐 때 적용할 넉백 유형
        [Tooltip("피해를 입힐 때 적용할 넉백 유형")]
		public KnockbackStyles DamageCausedKnockbackType = KnockbackStyles.AddForce;
        /// 넉백을 적용하는 방향
        [Tooltip("넉백을 적용하는 방향")]
		public KnockbackDirections DamageCausedKnockbackDirection = KnockbackDirections.BasedOnOwnerPosition;
        /// 손상된 물체에 가하는 힘
        [Tooltip("손상된 물체에 가하는 힘")]
		public Vector3 DamageCausedKnockbackForce = new Vector3(10, 10, 10);
		
		[Header("무적")]
        /// 명중 후 무적 프레임의 지속 시간(초)
        [Tooltip("명중 후 무적 프레임의 지속 시간(초)")]
		public float InvincibilityDuration = 0.5f;

		[Header("시간이 지남에 따른 손상")]
        /// 터치 영역의 손상이 시간이 지남에 따라 손상을 적용할지 여부
        [Tooltip("터치 영역의 손상이 시간이 지남에 따라 손상을 적용할지 여부")]
		public bool RepeatDamageOverTime = false;
        /// 시간 경과에 따른 피해 모드에서는 피해가 몇 번이나 반복되어야 합니까?
        [Tooltip("시간 경과에 따른 피해 모드에서는 피해가 몇 번이나 반복되어야 합니까?")] 
		[MMCondition("반복피해시간 초과", true)]
		public int AmountOfRepeats = 3;
        /// 시간 경과에 따른 손상 모드인 경우 두 손상 사이의 지속 시간(초)
        [Tooltip("시간 경과에 따른 손상 모드인 경우 두 손상 사이의 지속 시간(초)")]
		[MMCondition("시간이 지남에 따라 피해를 반복", true)]
		public float DurationBetweenRepeats = 1f;
        /// 시간 경과에 따른 손상 모드에 있는 경우 중단할 수 있는지 여부(Health:InterruptDamageOverTime 메서드를 호출하여)
        [Tooltip("시간 경과에 따른 손상 모드에 있는 경우 중단할 수 있는지 여부(Health:InterruptDamageOverTime 메서드를 호출하여)")] 
		[MMCondition("시간이 지남에 따라 피해를 반복", true)]
		public bool DamageOverTimeInterruptible = true;
        /// 시간 경과에 따른 피해 모드인 경우 반복되는 피해 유형
        [Tooltip("시간 경과에 따른 피해 모드인 경우 반복되는 피해 유형")] 
		[MMCondition("시간이 지남에 따라 피해를 반복", true)]
		public DamageType RepeatedDamageType;
		
		[MMInspectorGroup("받은 피해", true, 69)]
		[MMInformation("충돌한 모든 것에 피해를 가한 후에는 이 개체가 스스로 상처를 입도록 할 수 있습니다. " +
                       "예를 들어 총알은 벽에 부딪힌 후 폭발합니다. 여기에서 무언가에 맞을 때마다 얼마나 많은 피해를 입을지 정의할 수 있습니다. " +
                       "또는 손상될 수 있거나 손상되지 않는 것을 칠 때만 가능합니다. 이 개체를 유용하게 사용하려면 Health 구성 요소도 필요합니다.",
			MMInformationAttribute.InformationType.Info, false)]
        /// 우리가 충돌하는 것이 손상 가능한지 여부에 관계없이 매번 받는 손상의 양
        [Tooltip("우리가 충돌하는 것이 손상 가능한지 여부에 관계없이 매번 받는 손상의 양")]
		public float DamageTakenEveryTime = 0;
        /// 손상 가능한 물체와 충돌했을 때 받는 피해량
        [Tooltip("손상 가능한 물체와 충돌했을 때 받는 피해량")]
		public float DamageTakenDamageable = 0;
        /// 손상되지 않는 물체와 충돌했을 때 받는 피해량
        [Tooltip("손상되지 않는 물체와 충돌했을 때 받는 피해량")]
		public float DamageTakenNonDamageable = 0;
        /// 피해를 입을 때 적용할 넉백 유형
        [Tooltip("피해를 입을 때 적용할 넉백 유형")]
		public KnockbackStyles DamageTakenKnockbackType = KnockbackStyles.NoKnockback;
        /// 손상된 물체에 가하는 힘
        [Tooltip("손상된 물체에 가하는 힘")]
		public Vector3 DamageTakenKnockbackForce = Vector3.zero;
        /// 적중 후 무적 프레임의 지속 시간(초)
        [Tooltip("적중 후 무적 프레임의 지속 시간(초)")]
		public float DamageTakenInvincibilityDuration = 0.5f;

		[MMInspectorGroup("Feedbacks", true, 18)]
        /// Damageable을 칠 때 재생할 피드백
        [Tooltip("Damageable을 칠 때 재생할 피드백")]
		public MMFeedbacks HitDamageableFeedback;
        /// 손상되지 않는 공격을 가했을 때 재생되는 피드백
        [Tooltip("손상되지 않는 공격을 가했을 때 재생되는 피드백")]
		public MMFeedbacks HitNonDamageableFeedback;
        /// 무엇이든 칠 때 재생되는 피드백
        [Tooltip("무엇이든 칠 때 재생되는 피드백")]
		public MMFeedbacks HitAnythingFeedback;

        /// Damageable을 칠 때 트리거되는 이벤트
        public UnityEvent<Health> HitDamageableEvent;
        /// 손상되지 않는 공격을 가했을 때 트리거되는 이벤트
        public UnityEvent<GameObject> HitNonDamageableEvent;
        /// 무엇이든 부딪힐 때 트리거되는 이벤트
        public UnityEvent<GameObject> HitAnythingEvent;

        // 저장		
        protected Vector3 _lastPosition, _lastDamagePosition, _velocity, _knockbackForce, _damageDirection;
		protected float _startTime = 0f;
		protected Health _colliderHealth;
		protected TopDownController _topDownController;
		protected TopDownController _colliderTopDownController;
		protected Health _health;
		protected List<GameObject> _ignoredGameObjects;
		protected Vector3 _knockbackForceApplied;
		protected CircleCollider2D _circleCollider2D;
		protected BoxCollider2D _boxCollider2D;
		protected SphereCollider _sphereCollider;
		protected BoxCollider _boxCollider;
		protected Color _gizmosColor;
		protected Vector3 _gizmoSize;
		protected Vector3 _gizmoOffset;
		protected Transform _gizmoTransform;
		protected bool _twoD = false;
		protected bool _initializedFeedbacks = false;
		protected Vector3 _positionLastFrame;
		protected Vector3 _knockbackScriptDirection;
		protected Vector3 _relativePosition;
		protected Vector3 _damageScriptDirection;
		protected Health _collidingHealth;

        #region Initialization

        /// <summary>
        /// Awake에서는 터치 영역의 손상을 초기화합니다.
        /// </summary>
        protected virtual void Awake()
		{
			Initialization();

			if (gameObject.tag == "Player")
			{
				MinDamageCaused = PlayerDataManager.GetPower(); //3타 데미지를 더 쌔게할지 더 약하게 할지는 추후에 결정
				MaxDamageCaused = PlayerDataManager.GetPower();
			}
        }

        /// <summary>
        /// OnEnable 시작 시간을 현재 타임스탬프로 설정합니다.
        /// </summary>
        protected virtual void OnEnable()
		{
			_startTime = Time.time;
			_lastPosition = transform.position;
			_lastDamagePosition = transform.position;
		}

        /// <summary>
        /// 무시 목록, 피드백, 충돌체 및 구성 요소 가져오기를 초기화합니다.
        /// </summary>
        public virtual void Initialization()
		{
			InitializeIgnoreList();
			GrabComponents();
			InitalizeGizmos();
			InitializeColliders();
			InitializeFeedbacks();
		}

        /// <summary>
        /// 구성 요소를 저장합니다.
        /// </summary>
        protected virtual void GrabComponents()
		{
			_health = GetComponent<Health>();
			_topDownController = GetComponent<TopDownController>();
			_boxCollider = GetComponent<BoxCollider>();
			_sphereCollider = GetComponent<SphereCollider>();
			_boxCollider2D = GetComponent<BoxCollider2D>();
			_circleCollider2D = GetComponent<CircleCollider2D>();
			_lastDamagePosition = transform.position;
		}

        /// <summary>
        /// 충돌체를 초기화하고 필요한 경우 트리거로 설정합니다.
        /// </summary>
        protected virtual void InitializeColliders()
		{
			_twoD = _boxCollider2D != null || _circleCollider2D != null;
			if (_boxCollider2D != null)
			{
				SetGizmoOffset(_boxCollider2D.offset);
				_boxCollider2D.isTrigger = true;
			}

			if (_boxCollider != null)
			{
				SetGizmoOffset(_boxCollider.center);
				_boxCollider.isTrigger = true;
			}

			if (_sphereCollider != null)
			{
				SetGizmoOffset(_sphereCollider.center);
				_sphereCollider.isTrigger = true;
			}

			if (_circleCollider2D != null)
			{
				SetGizmoOffset(_circleCollider2D.offset);
				_circleCollider2D.isTrigger = true;
			}
		}

        /// <summary>
        /// 필요한 경우 _ignoredGameObjects 목록을 초기화합니다.
        /// </summary>
        protected virtual void InitializeIgnoreList()
		{
			if (_ignoredGameObjects == null) _ignoredGameObjects = new List<GameObject>();
		}

        /// <summary>
        /// 피드백 초기화
        /// </summary>
        public virtual void InitializeFeedbacks()
		{
			if (_initializedFeedbacks) return;

			HitDamageableFeedback?.Initialization(this.gameObject);
			HitNonDamageableFeedback?.Initialization(this.gameObject);
			HitAnythingFeedback?.Initialization(this.gameObject);
			_initializedFeedbacks = true;
		}

        /// <summary>
        /// 비활성화하면 무시 목록이 지워집니다.
        /// </summary>
        protected virtual void OnDisable()
		{
			ClearIgnoreList();
		}

        /// <summary>
        /// 검증 시 검사관이 동기화되었는지 확인합니다.
        /// </summary>
        protected virtual void OnValidate()
		{
			TriggerFilter &= AllowedTriggerCallbacks;
		}

        #endregion

        #region Gizmos

        /// <summary>
        /// 기즈모 색상 및 설정을 초기화합니다.
        /// </summary>
        protected virtual void InitalizeGizmos()
		{
			_gizmosColor = Color.red;
			_gizmosColor.a = 0.25f;
		}

        /// <summary>
        /// 기즈모 크기를 (재)정의할 수 있는 공개 메서드
        /// </summary>
        /// <param name="newGizmoSize"></param>
        public virtual void SetGizmoSize(Vector3 newGizmoSize)
		{
			_boxCollider2D = GetComponent<BoxCollider2D>();
			_boxCollider = GetComponent<BoxCollider>();
			_sphereCollider = GetComponent<SphereCollider>();
			_circleCollider2D = GetComponent<CircleCollider2D>();
			_gizmoSize = newGizmoSize;
		}

        /// <summary>
        /// 기즈모 오프셋을 지정할 수 있는 공개 메소드
        /// </summary>
        /// <param name="newOffset"></param>
        public virtual void SetGizmoOffset(Vector3 newOffset)
		{
			_gizmoOffset = newOffset;
		}

        /// <summary>
        /// 손상 영역 주위에 큐브나 구를 그립니다.
        /// </summary>
        protected virtual void OnDrawGizmos()
		{
			Gizmos.color = _gizmosColor;

			if (_boxCollider2D != null)
			{
				if (_boxCollider2D.enabled)
				{
					MMDebug.DrawGizmoCube(transform, _gizmoOffset, _boxCollider2D.size, false);
				}
				else
				{
					MMDebug.DrawGizmoCube(transform, _gizmoOffset, _boxCollider2D.size, true);
				}
			}

			if (_circleCollider2D != null)
			{
				Matrix4x4 rotationMatrix = transform.localToWorldMatrix;
				Gizmos.matrix = rotationMatrix;
				if (_circleCollider2D.enabled)
				{
					Gizmos.DrawSphere( (Vector2)_gizmoOffset, _circleCollider2D.radius);
				}
				else
				{
					Gizmos.DrawWireSphere((Vector2)_gizmoOffset, _circleCollider2D.radius);
				}
			}

			if (_boxCollider != null)
			{
				if (_boxCollider.enabled)
					MMDebug.DrawGizmoCube(transform,
						_gizmoOffset,
						_boxCollider.size,
						false);
				else
					MMDebug.DrawGizmoCube(transform,
						_gizmoOffset,
						_boxCollider.size,
						true);
			}

			if (_sphereCollider != null)
			{
				if (_sphereCollider.enabled)
					Gizmos.DrawSphere(transform.position, _sphereCollider.radius);
				else
					Gizmos.DrawWireSphere(transform.position, _sphereCollider.radius);
			}
		}

        #endregion

        #region PublicAPIs

        /// <summary>
        /// 넉백이 스크립트 방향 모드인 경우 넉백 방향을 지정할 수 있습니다.
        /// </summary>
        /// <param name="newDirection"></param>
        public virtual void SetKnockbackScriptDirection(Vector3 newDirection)
		{
			_knockbackScriptDirection = newDirection;
		}

        /// <summary>
        /// 손상 방향이 스크립트 모드인 경우 손상 방향을 지정할 수 있습니다.
        /// </summary>
        /// <param name="newDirection"></param>
        public virtual void SetDamageScriptDirection(Vector3 newDirection)
		{
			_damageDirection = newDirection;
		}

        /// <summary>
        /// 매개변수에 설정된 게임오브젝트를 무시 목록에 추가합니다.
        /// </summary>
        /// <param name="newIgnoredGameObject">New ignored game object.</param>
        public virtual void IgnoreGameObject(GameObject newIgnoredGameObject)
		{
			InitializeIgnoreList();
			_ignoredGameObjects.Add(newIgnoredGameObject);
		}

        /// <summary>
        /// 무시 목록에서 매개변수에 설정된 개체를 제거합니다.
        /// </summary>
        /// <param name="ignoredGameObject">Ignored game object.</param>
        public virtual void StopIgnoringObject(GameObject ignoredGameObject)
		{
			if (_ignoredGameObjects != null) _ignoredGameObjects.Remove(ignoredGameObject);
		}

        /// <summary>
        /// 무시 목록을 지웁니다.
        /// </summary>
        public virtual void ClearIgnoreList()
		{
			InitializeIgnoreList();
			_ignoredGameObjects.Clear();
		}

        #endregion

        #region Loop

        /// <summary>
        /// 마지막 업데이트 중에 객체의 위치와 속도를 저장합니다.
        /// </summary>
        protected virtual void Update()
		{
			ComputeVelocity();
		}

        /// <summary>
        /// 늦은 업데이트에서는 위치를 저장합니다.
        /// </summary>
        protected void LateUpdate()
		{
			_positionLastFrame = transform.position;
		}

        /// <summary>
        /// 물체의 마지막 위치를 기준으로 속도를 계산합니다.
        /// </summary>
        protected virtual void ComputeVelocity()
		{
			if (Time.deltaTime != 0f)
			{
				_velocity = (_lastPosition - (Vector3)transform.position) / Time.deltaTime;

				if (Vector3.Distance(_lastDamagePosition, transform.position) > 0.5f)
				{
					_lastDamagePosition = transform.position;
				}

				_lastPosition = transform.position;
			}
		}

        /// <summary>
        ///Health Damage 메서드에 전달할 피해 방향을 결정합니다.
        /// </summary>
        protected virtual void DetermineDamageDirection()
		{
			switch (DamageDirectionMode)
			{
				case DamageDirections.BasedOnOwnerPosition:
					if (Owner == null)
					{
						Owner = gameObject;
					}
					if (_twoD)
					{
						_damageDirection = _collidingHealth.transform.position - Owner.transform.position;
						_damageDirection.z = 0;
					}
					else
					{
						_damageDirection = _collidingHealth.transform.position - Owner.transform.position;
					}
					break;
				case DamageDirections.BasedOnVelocity:
					_damageDirection = transform.position - _lastDamagePosition;
					break;
				case DamageDirections.BasedOnScriptDirection:
					_damageDirection = _damageScriptDirection;
					break;
			}

			_damageDirection = _damageDirection.normalized;
		}

        #endregion

        #region CollisionDetection

        /// <summary>
        /// 플레이어와의 충돌이 발생하면 플레이어에게 피해를 주고 밀어냅니다.
        /// </summary>
        /// <param name="collider">what's colliding with the object.</param>
        public virtual void OnTriggerStay2D(Collider2D collider)
		{
			if (0 == (TriggerFilter & TriggerAndCollisionMask.OnTriggerStay2D)) return;
			Colliding(collider.gameObject);
		}

        /// <summary>
        /// 2D 입력 트리거에서 충돌하는 끝점을 호출합니다.
        /// </summary>
        /// <param name="collider"></param>S
        public virtual void OnTriggerEnter2D(Collider2D collider)
		{
			if (0 == (TriggerFilter & TriggerAndCollisionMask.OnTriggerEnter2D)) return;
			Colliding(collider.gameObject);
		}

        /// <summary>
        /// 트리거 스테이에서 충돌하는 엔드포인트를 호출합니다.
        /// </summary>
        /// <param name="collider"></param>
        public virtual void OnTriggerStay(Collider collider)
		{
			if (0 == (TriggerFilter & TriggerAndCollisionMask.OnTriggerStay)) return;
			Colliding(collider.gameObject);
		}

        /// <summary>
        /// 트리거 입력 시 충돌하는 엔드포인트를 호출합니다.
        /// </summary>
        /// <param name="collider"></param>
        public virtual void OnTriggerEnter(Collider collider)
		{
            if (0 == (TriggerFilter & TriggerAndCollisionMask.OnTriggerEnter)) return;
            Colliding(collider.gameObject);
		}

        #endregion

        /// <summary>
        /// 충돌 시 적절한 피해를 입힙니다.
        /// </summary>
        /// <param name="collider"></param>
        protected virtual void Colliding(GameObject collider)
		{
			if (!EvaluateAvailability(collider))
			{
                return;
			}

            // 캐시 재설정
            _colliderTopDownController = null;
			_colliderHealth = collider.gameObject.MMGetComponentNoAlloc<Health>();

            // 우리가 충돌하는 것이 손상될 수 있는 경우
            if (_colliderHealth != null)
			{
                if (_colliderHealth.CurrentHealth > 0)
				{
                    OnCollideWithDamageable(_colliderHealth);
				}
			}
            else // 우리가 충돌하는 것이 손상될 수 없다면
            {
				OnCollideWithNonDamageable();
				HitNonDamageableEvent?.Invoke(collider);
			}

			OnAnyCollision(collider);
			HitAnythingEvent?.Invoke(collider);
			HitAnythingFeedback?.PlayFeedbacks(transform.position);
		}

        /// <summary>
        /// 이 프레임에 피해를 가해야 하는지 여부를 확인합니다.
        /// </summary>
        /// <param name="collider"></param>
        /// <returns></returns>
        protected virtual bool EvaluateAvailability(GameObject collider)
		{
            // 활동하지 않으면 아무것도 하지 않습니다
            if (!isActiveAndEnabled) { return false; }

            //충돌하는 객체가 무시 목록의 일부인 경우 아무 작업도 수행하지 않고 종료합니다.
            if (_ignoredGameObjects.Contains(collider)) { return false; }

            // 우리가 충돌하는 것이 대상 레이어의 일부가 아닌 경우 아무것도 하지 않고 종료합니다.
            if (!MMLayers.LayerInLayerMask(collider.layer, TargetLayerMask)) { return false; }

            // 첫 번째 프레임에서는 피해를 입히지 않습니다.
            if (Time.time == 0f) { return false; }

			return true;
		}

        /// <summary>
        /// 손상될 수 있는 물체와 충돌할 때 어떤 일이 일어나는지 설명합니다.
        /// </summary>
        /// <param name="health">Health.</param>
        protected virtual void OnCollideWithDamageable(Health health)
		{
			_collidingHealth = health;

            if (health.CanTakeDamageThisFrame())
			{
                // 충돌하는 것이 TopDownController인 경우 밀어내기 힘을 적용합니다.
                _colliderTopDownController = health.gameObject.MMGetComponentNoAlloc<TopDownController>();

				HitDamageableFeedback?.PlayFeedbacks(this.transform.position);
				HitDamageableEvent?.Invoke(_colliderHealth);

                // 우리는 충돌한 것에 피해를 입힙니다.
                float randomDamage =
					UnityEngine.Random.Range(MinDamageCaused, Mathf.Max(MaxDamageCaused, MinDamageCaused));

				ApplyKnockback(randomDamage, TypedDamages);

				DetermineDamageDirection();

				if (RepeatDamageOverTime)
				{
					_colliderHealth.DamageOverTime(randomDamage, gameObject, InvincibilityDuration,
						InvincibilityDuration, _damageDirection, TypedDamages, AmountOfRepeats, DurationBetweenRepeats,
						DamageOverTimeInterruptible, RepeatedDamageType);
				}
				else
				{
                    _colliderHealth.Damage(randomDamage, gameObject, InvincibilityDuration, InvincibilityDuration,
						_damageDirection, TypedDamages);
				}
			}

            // 우리는 자기 피해를 입힌다
            if (DamageTakenEveryTime + DamageTakenDamageable > 0 && !_colliderHealth.PreventTakeSelfDamage)
			{
				SelfDamage(DamageTakenEveryTime + DamageTakenDamageable);
			}
		}

        #region Knockback

        /// <summary>
        /// 필요한 경우 넉백을 적용합니다.
        /// </summary>
        protected virtual void ApplyKnockback(float damage, List<TypedDamage> typedDamages)
		{
			if (ShouldApplyKnockback(damage, typedDamages))
			{
				_knockbackForce = DamageCausedKnockbackForce * _colliderHealth.KnockbackForceMultiplier;
				_knockbackForce = _colliderHealth.ComputeKnockbackForce(_knockbackForce, typedDamages);

				if (_twoD) // if we're in 2D
				{
					ApplyKnockback2D();
				}
				else // if we're in 3D
				{
					ApplyKnockback3D();
				}
				
				if (DamageCausedKnockbackType == KnockbackStyles.AddForce)
				{
					_colliderTopDownController.Impact(_knockbackForce.normalized, _knockbackForce.magnitude);
				}
			}
		}

        /// <summary>
        /// 넉백을 적용할지 여부를 결정합니다.
        /// </summary>
        /// <returns></returns>
        protected virtual bool ShouldApplyKnockback(float damage, List<TypedDamage> typedDamages)
		{
			if (_colliderHealth.ImmuneToKnockbackIfZeroDamage)
			{
				if (_colliderHealth.ComputeDamageOutput(damage, typedDamages, false) == 0)
				{
					return false;
				}
			}
			
			return (_colliderTopDownController != null)
			       && (DamageCausedKnockbackForce != Vector3.zero)
			       && !_colliderHealth.Invulnerable
			       && _colliderHealth.CanGetKnockback(typedDamages);
		}

        /// <summary>
        /// 2D 컨텍스트에 있는 경우 넉백을 적용합니다.
        /// </summary>
        protected virtual void ApplyKnockback2D()
		{
			switch (DamageCausedKnockbackDirection)
			{
				case KnockbackDirections.BasedOnSpeed:
					var totalVelocity = _colliderTopDownController.Speed + _velocity;
					_knockbackForce = Vector3.RotateTowards(_knockbackForce,
						totalVelocity.normalized, 10f, 0f);
					break;
				case KnockbackDirections.BasedOnOwnerPosition:
					if (Owner == null)
					{
						Owner = gameObject;
					}
					_relativePosition = _colliderTopDownController.transform.position - Owner.transform.position;
					_knockbackForce = Vector3.RotateTowards(_knockbackForce, _relativePosition.normalized, 10f, 0f);
					break;
				case KnockbackDirections.BasedOnDirection:
					var direction = transform.position - _positionLastFrame;
					_knockbackForce = direction * _knockbackForce.magnitude;
					break;
				case KnockbackDirections.BasedOnScriptDirection:
					_knockbackForce = _knockbackScriptDirection * _knockbackForce.magnitude;
					break;
			}
		}

        /// <summary>
        /// 3D 환경에 있는 경우 넉백을 적용합니다.
        /// </summary>
        protected virtual void ApplyKnockback3D()
		{
			switch (DamageCausedKnockbackDirection)
			{
				case KnockbackDirections.BasedOnSpeed:
					var totalVelocity = _colliderTopDownController.Speed + _velocity;
					_knockbackForce = _knockbackForce * totalVelocity.magnitude;
					break;
				case KnockbackDirections.BasedOnOwnerPosition:
					if (Owner == null)
					{
						Owner = gameObject;
					}
					_relativePosition = _colliderTopDownController.transform.position - Owner.transform.position;
					_knockbackForce = Quaternion.LookRotation(_relativePosition) * _knockbackForce;
					break;
				case KnockbackDirections.BasedOnDirection:
					var direction = transform.position - _positionLastFrame;
					_knockbackForce = direction * _knockbackForce.magnitude;
					break;
				case KnockbackDirections.BasedOnScriptDirection:
					_knockbackForce = _knockbackScriptDirection * _knockbackForce.magnitude;
					break;
			}
		}

        #endregion


        /// <summary>
        /// 손상되지 않는 물체와 충돌할 때 어떤 일이 일어나는지 설명합니다.
        /// </summary>
        protected virtual void OnCollideWithNonDamageable()
		{
			float selfDamage = DamageTakenEveryTime + DamageTakenNonDamageable; 
			if (selfDamage > 0)
			{
				SelfDamage(selfDamage);
			}
			HitNonDamageableFeedback?.PlayFeedbacks(transform.position);
		}

        /// <summary>
        /// 무엇이든 충돌할 때 어떤 일이 발생할 수 있는지 설명합니다.
        /// </summary>
        protected virtual void OnAnyCollision(GameObject other)
		{

		}

        /// <summary>
        /// 자신에게 피해를 입힙니다.
        /// </summary>
        /// <param name="damage">Damage.</param>
        protected virtual void SelfDamage(float damage)
		{
			if (_health != null)
			{
				_damageDirection = Vector3.up;
				_health.Damage(damage, gameObject, 0f, DamageTakenInvincibilityDuration, _damageDirection);
			}

            // 충돌하는 것이 TopDownController인 경우 밀어내기 힘을 적용합니다.
            if ((_topDownController != null) && (_colliderTopDownController != null))
			{
				Vector3 totalVelocity = _colliderTopDownController.Speed + _velocity;
				Vector3 knockbackForce =
					Vector3.RotateTowards(DamageTakenKnockbackForce, totalVelocity.normalized, 10f, 0f);

				if (DamageTakenKnockbackType == KnockbackStyles.AddForce)
				{
					_topDownController.AddForce(knockbackForce);
				}
			}
		}
	}
}