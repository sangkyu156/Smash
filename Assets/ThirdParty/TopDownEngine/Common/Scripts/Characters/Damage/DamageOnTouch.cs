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
    /// �� ���� ��Ҹ� ��ü�� �߰��ϸ� ��ü�� �浹�ϴ� ��ü�� �ջ��� �����ϴ�.
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

        /// �˹��� �߰��� �� �ִ� ���: noKnockback, �ƹ� �۾��� �������� �ʰ� ���� �����ϰų� ���� �߰����� �ʽ��ϴ�.
        public enum KnockbackStyles
		{
			NoKnockback,
			AddForce
		}

        /// ������ �˹� ����
        public enum KnockbackDirections
		{
			BasedOnOwnerPosition,//������ ��ġ ����
            BasedOnSpeed,//�ӵ� ����
            BasedOnDirection,//���� ���
            BasedOnScriptDirection//��ũ��Ʈ ���⿡ ����
        }

        /// �ջ� ������ �����ϴ� ������ ���
        public enum DamageDirections
		{
			BasedOnOwnerPosition,//������ ��ġ ����
            BasedOnVelocity,//�ӵ� ����
            BasedOnScriptDirection//��ũ��Ʈ ���⿡ ����
        }

		public const TriggerAndCollisionMask AllowedTriggerCallbacks = TriggerAndCollisionMask.OnTriggerEnter
		                                                                  | TriggerAndCollisionMask.OnTriggerStay
		                                                                  | TriggerAndCollisionMask.OnTriggerEnter2D
		                                                                  | TriggerAndCollisionMask.OnTriggerStay2D;

		[MMInspectorGroup("Targets", true, 3)]
		[MMInformation(
            "�� ���� ��Ҵ� ��ü�� �浹�ϴ� ��ü�� �ջ��� �������� ����ϴ�. ���⿡�� ������ ������ �޴� ���̾�(ǥ�� ���� ��� �÷��̾� ����), �� ���ط�, ���� �� ���ظ� �޴� ��ü�� ������ ���� ���� ������ �� �ֽ��ϴ�. ���� �� ������ ���ӵǴ� �ð�(��)�� ������ ���� �ֽ��ϴ�.",
			MMInformationAttribute.InformationType.Info, false)]
        /// �� ��ü�� ���� �ջ�� ���̾�
        [Tooltip("�� ��ü�� ���� �ջ�� ���̾�")]
		public LayerMask TargetLayerMask;
        /// DamageOnTouch ������ ������
        [MMReadOnly] [Tooltip("DamageOnTouch ������ ������")]
		public GameObject Owner;

        /// �⺻������ �Է� �� ����(2D �� 3D ���) �� ���ظ� �����ؾ� �ϴ� Ʈ���Ÿ� ���������� �� �ʵ带 ����ϸ� �ʿ��� ��� Ʈ���Ÿ� ������ �� �ֽ��ϴ�.
        [Tooltip(
            "�⺻������ �Է� �� ����(2D �� 3D ���) �� �ջ��� �����ؾ� �ϴ� Ʈ���Ÿ� ���������� �� �ʵ带 ����ϸ� �ʿ��� ��� Ʈ���Ÿ� ������ �� �ֽ��ϴ�.")]
		public TriggerAndCollisionMask TriggerFilter = AllowedTriggerCallbacks;

		[MMInspectorGroup("������ ��ġ", true, 8)]
        /// �÷��̾��� ü�¿��� ������ �ּ� ü���Դϴ�.
        [FormerlySerializedAs("������ ��ġ")]
		[Tooltip("�÷��̾��� ü�¿��� ������ �ּ� ü���Դϴ�.")]
		public float MinDamageCaused = 10f;
        /// �÷��̾��� ü�¿��� ������ �ִ� ü��
        [Tooltip("�÷��̾��� ü�¿��� ������ �ִ� ü��")]
		public float MaxDamageCaused = 10f;
        /// �⺻ �ջ� ���� ����� ���� �ջ� ���� ���
        [Tooltip("�⺻ �ջ� ���� ����� ���� �ջ� ���� ���")]
		public List<TypedDamage> TypedDamages;
        /// �ǰ� ���� ����� ���޵Ǵ� ���� ������ �����ϴ� ����� �Ϲ������� ���� ����(�߻�ü) �̵��� �ӵ��� ����ϰ� ���� ���⿡ ������ ��ġ�� ����մϴ�.
        [Tooltip("�ǰ� ���� ����� ���޵Ǵ� ���� ������ �����ϴ� ����� �Ϲ������� ���� ����(�߻�ü) �̵��� �ӵ��� ����ϰ� ���� ���⿡ ������ ��ġ�� ����մϴ�.")]
		public DamageDirections DamageDirectionMode = DamageDirections.BasedOnVelocity;
		
		[Header("Knockback")]
        /// ���ظ� ���� �� ������ �˹� ����
        [Tooltip("���ظ� ���� �� ������ �˹� ����")]
		public KnockbackStyles DamageCausedKnockbackType = KnockbackStyles.AddForce;
        /// �˹��� �����ϴ� ����
        [Tooltip("�˹��� �����ϴ� ����")]
		public KnockbackDirections DamageCausedKnockbackDirection = KnockbackDirections.BasedOnOwnerPosition;
        /// �ջ�� ��ü�� ���ϴ� ��
        [Tooltip("�ջ�� ��ü�� ���ϴ� ��")]
		public Vector3 DamageCausedKnockbackForce = new Vector3(10, 10, 10);
		
		[Header("����")]
        /// ���� �� ���� �������� ���� �ð�(��)
        [Tooltip("���� �� ���� �������� ���� �ð�(��)")]
		public float InvincibilityDuration = 0.5f;

		[Header("�ð��� ������ ���� �ջ�")]
        /// ��ġ ������ �ջ��� �ð��� ������ ���� �ջ��� �������� ����
        [Tooltip("��ġ ������ �ջ��� �ð��� ������ ���� �ջ��� �������� ����")]
		public bool RepeatDamageOverTime = false;
        /// �ð� ����� ���� ���� ��忡���� ���ذ� �� ���̳� �ݺ��Ǿ�� �մϱ�?
        [Tooltip("�ð� ����� ���� ���� ��忡���� ���ذ� �� ���̳� �ݺ��Ǿ�� �մϱ�?")] 
		[MMCondition("�ݺ����ؽð� �ʰ�", true)]
		public int AmountOfRepeats = 3;
        /// �ð� ����� ���� �ջ� ����� ��� �� �ջ� ������ ���� �ð�(��)
        [Tooltip("�ð� ����� ���� �ջ� ����� ��� �� �ջ� ������ ���� �ð�(��)")]
		[MMCondition("�ð��� ������ ���� ���ظ� �ݺ�", true)]
		public float DurationBetweenRepeats = 1f;
        /// �ð� ����� ���� �ջ� ��忡 �ִ� ��� �ߴ��� �� �ִ��� ����(Health:InterruptDamageOverTime �޼��带 ȣ���Ͽ�)
        [Tooltip("�ð� ����� ���� �ջ� ��忡 �ִ� ��� �ߴ��� �� �ִ��� ����(Health:InterruptDamageOverTime �޼��带 ȣ���Ͽ�)")] 
		[MMCondition("�ð��� ������ ���� ���ظ� �ݺ�", true)]
		public bool DamageOverTimeInterruptible = true;
        /// �ð� ����� ���� ���� ����� ��� �ݺ��Ǵ� ���� ����
        [Tooltip("�ð� ����� ���� ���� ����� ��� �ݺ��Ǵ� ���� ����")] 
		[MMCondition("�ð��� ������ ���� ���ظ� �ݺ�", true)]
		public DamageType RepeatedDamageType;
		
		[MMInspectorGroup("���� ����", true, 69)]
		[MMInformation("�浹�� ��� �Ϳ� ���ظ� ���� �Ŀ��� �� ��ü�� ������ ��ó�� �Ե��� �� �� �ֽ��ϴ�. " +
                       "���� ��� �Ѿ��� ���� �ε��� �� �����մϴ�. ���⿡�� ���𰡿� ���� ������ �󸶳� ���� ���ظ� ������ ������ �� �ֽ��ϴ�. " +
                       "�Ǵ� �ջ�� �� �ְų� �ջ���� �ʴ� ���� ĥ ���� �����մϴ�. �� ��ü�� �����ϰ� ����Ϸ��� Health ���� ��ҵ� �ʿ��մϴ�.",
			MMInformationAttribute.InformationType.Info, false)]
        /// �츮�� �浹�ϴ� ���� �ջ� �������� ���ο� ������� �Ź� �޴� �ջ��� ��
        [Tooltip("�츮�� �浹�ϴ� ���� �ջ� �������� ���ο� ������� �Ź� �޴� �ջ��� ��")]
		public float DamageTakenEveryTime = 0;
        /// �ջ� ������ ��ü�� �浹���� �� �޴� ���ط�
        [Tooltip("�ջ� ������ ��ü�� �浹���� �� �޴� ���ط�")]
		public float DamageTakenDamageable = 0;
        /// �ջ���� �ʴ� ��ü�� �浹���� �� �޴� ���ط�
        [Tooltip("�ջ���� �ʴ� ��ü�� �浹���� �� �޴� ���ط�")]
		public float DamageTakenNonDamageable = 0;
        /// ���ظ� ���� �� ������ �˹� ����
        [Tooltip("���ظ� ���� �� ������ �˹� ����")]
		public KnockbackStyles DamageTakenKnockbackType = KnockbackStyles.NoKnockback;
        /// �ջ�� ��ü�� ���ϴ� ��
        [Tooltip("�ջ�� ��ü�� ���ϴ� ��")]
		public Vector3 DamageTakenKnockbackForce = Vector3.zero;
        /// ���� �� ���� �������� ���� �ð�(��)
        [Tooltip("���� �� ���� �������� ���� �ð�(��)")]
		public float DamageTakenInvincibilityDuration = 0.5f;

		[MMInspectorGroup("Feedbacks", true, 18)]
        /// Damageable�� ĥ �� ����� �ǵ��
        [Tooltip("Damageable�� ĥ �� ����� �ǵ��")]
		public MMFeedbacks HitDamageableFeedback;
        /// �ջ���� �ʴ� ������ ������ �� ����Ǵ� �ǵ��
        [Tooltip("�ջ���� �ʴ� ������ ������ �� ����Ǵ� �ǵ��")]
		public MMFeedbacks HitNonDamageableFeedback;
        /// �����̵� ĥ �� ����Ǵ� �ǵ��
        [Tooltip("�����̵� ĥ �� ����Ǵ� �ǵ��")]
		public MMFeedbacks HitAnythingFeedback;

        /// Damageable�� ĥ �� Ʈ���ŵǴ� �̺�Ʈ
        public UnityEvent<Health> HitDamageableEvent;
        /// �ջ���� �ʴ� ������ ������ �� Ʈ���ŵǴ� �̺�Ʈ
        public UnityEvent<GameObject> HitNonDamageableEvent;
        /// �����̵� �ε��� �� Ʈ���ŵǴ� �̺�Ʈ
        public UnityEvent<GameObject> HitAnythingEvent;

        // ����		
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
        /// Awake������ ��ġ ������ �ջ��� �ʱ�ȭ�մϴ�.
        /// </summary>
        protected virtual void Awake()
		{
			Initialization();

			if (gameObject.tag == "Player")
			{
				MinDamageCaused = PlayerDataManager.GetPower(); //3Ÿ �������� �� �ذ����� �� ���ϰ� ������ ���Ŀ� ����
				MaxDamageCaused = PlayerDataManager.GetPower();
			}
        }

        /// <summary>
        /// OnEnable ���� �ð��� ���� Ÿ�ӽ������� �����մϴ�.
        /// </summary>
        protected virtual void OnEnable()
		{
			_startTime = Time.time;
			_lastPosition = transform.position;
			_lastDamagePosition = transform.position;
		}

        /// <summary>
        /// ���� ���, �ǵ��, �浹ü �� ���� ��� �������⸦ �ʱ�ȭ�մϴ�.
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
        /// ���� ��Ҹ� �����մϴ�.
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
        /// �浹ü�� �ʱ�ȭ�ϰ� �ʿ��� ��� Ʈ���ŷ� �����մϴ�.
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
        /// �ʿ��� ��� _ignoredGameObjects ����� �ʱ�ȭ�մϴ�.
        /// </summary>
        protected virtual void InitializeIgnoreList()
		{
			if (_ignoredGameObjects == null) _ignoredGameObjects = new List<GameObject>();
		}

        /// <summary>
        /// �ǵ�� �ʱ�ȭ
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
        /// ��Ȱ��ȭ�ϸ� ���� ����� �������ϴ�.
        /// </summary>
        protected virtual void OnDisable()
		{
			ClearIgnoreList();
		}

        /// <summary>
        /// ���� �� �˻���� ����ȭ�Ǿ����� Ȯ���մϴ�.
        /// </summary>
        protected virtual void OnValidate()
		{
			TriggerFilter &= AllowedTriggerCallbacks;
		}

        #endregion

        #region Gizmos

        /// <summary>
        /// ����� ���� �� ������ �ʱ�ȭ�մϴ�.
        /// </summary>
        protected virtual void InitalizeGizmos()
		{
			_gizmosColor = Color.red;
			_gizmosColor.a = 0.25f;
		}

        /// <summary>
        /// ����� ũ�⸦ (��)������ �� �ִ� ���� �޼���
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
        /// ����� �������� ������ �� �ִ� ���� �޼ҵ�
        /// </summary>
        /// <param name="newOffset"></param>
        public virtual void SetGizmoOffset(Vector3 newOffset)
		{
			_gizmoOffset = newOffset;
		}

        /// <summary>
        /// �ջ� ���� ������ ť�곪 ���� �׸��ϴ�.
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
        /// �˹��� ��ũ��Ʈ ���� ����� ��� �˹� ������ ������ �� �ֽ��ϴ�.
        /// </summary>
        /// <param name="newDirection"></param>
        public virtual void SetKnockbackScriptDirection(Vector3 newDirection)
		{
			_knockbackScriptDirection = newDirection;
		}

        /// <summary>
        /// �ջ� ������ ��ũ��Ʈ ����� ��� �ջ� ������ ������ �� �ֽ��ϴ�.
        /// </summary>
        /// <param name="newDirection"></param>
        public virtual void SetDamageScriptDirection(Vector3 newDirection)
		{
			_damageDirection = newDirection;
		}

        /// <summary>
        /// �Ű������� ������ ���ӿ�����Ʈ�� ���� ��Ͽ� �߰��մϴ�.
        /// </summary>
        /// <param name="newIgnoredGameObject">New ignored game object.</param>
        public virtual void IgnoreGameObject(GameObject newIgnoredGameObject)
		{
			InitializeIgnoreList();
			_ignoredGameObjects.Add(newIgnoredGameObject);
		}

        /// <summary>
        /// ���� ��Ͽ��� �Ű������� ������ ��ü�� �����մϴ�.
        /// </summary>
        /// <param name="ignoredGameObject">Ignored game object.</param>
        public virtual void StopIgnoringObject(GameObject ignoredGameObject)
		{
			if (_ignoredGameObjects != null) _ignoredGameObjects.Remove(ignoredGameObject);
		}

        /// <summary>
        /// ���� ����� ����ϴ�.
        /// </summary>
        public virtual void ClearIgnoreList()
		{
			InitializeIgnoreList();
			_ignoredGameObjects.Clear();
		}

        #endregion

        #region Loop

        /// <summary>
        /// ������ ������Ʈ �߿� ��ü�� ��ġ�� �ӵ��� �����մϴ�.
        /// </summary>
        protected virtual void Update()
		{
			ComputeVelocity();
		}

        /// <summary>
        /// ���� ������Ʈ������ ��ġ�� �����մϴ�.
        /// </summary>
        protected void LateUpdate()
		{
			_positionLastFrame = transform.position;
		}

        /// <summary>
        /// ��ü�� ������ ��ġ�� �������� �ӵ��� ����մϴ�.
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
        ///Health Damage �޼��忡 ������ ���� ������ �����մϴ�.
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
        /// �÷��̾���� �浹�� �߻��ϸ� �÷��̾�� ���ظ� �ְ� �о���ϴ�.
        /// </summary>
        /// <param name="collider">what's colliding with the object.</param>
        public virtual void OnTriggerStay2D(Collider2D collider)
		{
			if (0 == (TriggerFilter & TriggerAndCollisionMask.OnTriggerStay2D)) return;
			Colliding(collider.gameObject);
		}

        /// <summary>
        /// 2D �Է� Ʈ���ſ��� �浹�ϴ� ������ ȣ���մϴ�.
        /// </summary>
        /// <param name="collider"></param>S
        public virtual void OnTriggerEnter2D(Collider2D collider)
		{
			if (0 == (TriggerFilter & TriggerAndCollisionMask.OnTriggerEnter2D)) return;
			Colliding(collider.gameObject);
		}

        /// <summary>
        /// Ʈ���� �����̿��� �浹�ϴ� ��������Ʈ�� ȣ���մϴ�.
        /// </summary>
        /// <param name="collider"></param>
        public virtual void OnTriggerStay(Collider collider)
		{
			if (0 == (TriggerFilter & TriggerAndCollisionMask.OnTriggerStay)) return;
			Colliding(collider.gameObject);
		}

        /// <summary>
        /// Ʈ���� �Է� �� �浹�ϴ� ��������Ʈ�� ȣ���մϴ�.
        /// </summary>
        /// <param name="collider"></param>
        public virtual void OnTriggerEnter(Collider collider)
		{
            if (0 == (TriggerFilter & TriggerAndCollisionMask.OnTriggerEnter)) return;
            Colliding(collider.gameObject);
		}

        #endregion

        /// <summary>
        /// �浹 �� ������ ���ظ� �����ϴ�.
        /// </summary>
        /// <param name="collider"></param>
        protected virtual void Colliding(GameObject collider)
		{
			if (!EvaluateAvailability(collider))
			{
                return;
			}

            // ĳ�� �缳��
            _colliderTopDownController = null;
			_colliderHealth = collider.gameObject.MMGetComponentNoAlloc<Health>();

            // �츮�� �浹�ϴ� ���� �ջ�� �� �ִ� ���
            if (_colliderHealth != null)
			{
                if (_colliderHealth.CurrentHealth > 0)
				{
                    OnCollideWithDamageable(_colliderHealth);
				}
			}
            else // �츮�� �浹�ϴ� ���� �ջ�� �� ���ٸ�
            {
				OnCollideWithNonDamageable();
				HitNonDamageableEvent?.Invoke(collider);
			}

			OnAnyCollision(collider);
			HitAnythingEvent?.Invoke(collider);
			HitAnythingFeedback?.PlayFeedbacks(transform.position);
		}

        /// <summary>
        /// �� �����ӿ� ���ظ� ���ؾ� �ϴ��� ���θ� Ȯ���մϴ�.
        /// </summary>
        /// <param name="collider"></param>
        /// <returns></returns>
        protected virtual bool EvaluateAvailability(GameObject collider)
		{
            // Ȱ������ ������ �ƹ��͵� ���� �ʽ��ϴ�
            if (!isActiveAndEnabled) { return false; }

            //�浹�ϴ� ��ü�� ���� ����� �Ϻ��� ��� �ƹ� �۾��� �������� �ʰ� �����մϴ�.
            if (_ignoredGameObjects.Contains(collider)) { return false; }

            // �츮�� �浹�ϴ� ���� ��� ���̾��� �Ϻΰ� �ƴ� ��� �ƹ��͵� ���� �ʰ� �����մϴ�.
            if (!MMLayers.LayerInLayerMask(collider.layer, TargetLayerMask)) { return false; }

            // ù ��° �����ӿ����� ���ظ� ������ �ʽ��ϴ�.
            if (Time.time == 0f) { return false; }

			return true;
		}

        /// <summary>
        /// �ջ�� �� �ִ� ��ü�� �浹�� �� � ���� �Ͼ���� �����մϴ�.
        /// </summary>
        /// <param name="health">Health.</param>
        protected virtual void OnCollideWithDamageable(Health health)
		{
			_collidingHealth = health;

            if (health.CanTakeDamageThisFrame())
			{
                // �浹�ϴ� ���� TopDownController�� ��� �о�� ���� �����մϴ�.
                _colliderTopDownController = health.gameObject.MMGetComponentNoAlloc<TopDownController>();

				HitDamageableFeedback?.PlayFeedbacks(this.transform.position);
				HitDamageableEvent?.Invoke(_colliderHealth);

                // �츮�� �浹�� �Ϳ� ���ظ� �����ϴ�.
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

            // �츮�� �ڱ� ���ظ� ������
            if (DamageTakenEveryTime + DamageTakenDamageable > 0 && !_colliderHealth.PreventTakeSelfDamage)
			{
				SelfDamage(DamageTakenEveryTime + DamageTakenDamageable);
			}
		}

        #region Knockback

        /// <summary>
        /// �ʿ��� ��� �˹��� �����մϴ�.
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
        /// �˹��� �������� ���θ� �����մϴ�.
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
        /// 2D ���ؽ�Ʈ�� �ִ� ��� �˹��� �����մϴ�.
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
        /// 3D ȯ�濡 �ִ� ��� �˹��� �����մϴ�.
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
        /// �ջ���� �ʴ� ��ü�� �浹�� �� � ���� �Ͼ���� �����մϴ�.
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
        /// �����̵� �浹�� �� � ���� �߻��� �� �ִ��� �����մϴ�.
        /// </summary>
        protected virtual void OnAnyCollision(GameObject other)
		{

		}

        /// <summary>
        /// �ڽſ��� ���ظ� �����ϴ�.
        /// </summary>
        /// <param name="damage">Damage.</param>
        protected virtual void SelfDamage(float damage)
		{
			if (_health != null)
			{
				_damageDirection = Vector3.up;
				_health.Damage(damage, gameObject, 0f, DamageTakenInvincibilityDuration, _damageDirection);
			}

            // �浹�ϴ� ���� TopDownController�� ��� �о�� ���� �����մϴ�.
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