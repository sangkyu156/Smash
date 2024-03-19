using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;
using UnityEngine.Serialization;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 무기를 사용할 때 "상처 구역"을 활성화하는 기본 근접 무기 클래스
    /// </summary>
    [AddComponentMenu("TopDown Engine/Weapons/Melee Weapon")]
	public class MeleeWeapon : Weapon
	{
		/// the possible shapes for the melee weapon's damage area
		public enum MeleeDamageAreaShapes { Rectangle, Circle, Box, Sphere }
		public enum MeleeDamageAreaModes { Generated, Existing }

		[MMInspectorGroup("Damage Area", true, 22)]
		/// the possible modes to handle the damage area. In Generated, the MeleeWeapon will create it, in Existing, you can bind an existing damage area - usually nested under the weapon
		[Tooltip("피해 지역을 처리하기 위한 가능한 모드. 생성에서는 MeleeWeapon이 생성하고, 기존에서는 기존 손상 영역을 바인딩할 수 있습니다. 일반적으로 무기 아래에 중첩됩니다.")]
		public MeleeDamageAreaModes MeleeDamageAreaMode = MeleeDamageAreaModes.Generated;
		/// the shape of the damage area (rectangle or circle)
		[Tooltip("손상 부위의 모양(직사각형 또는 원형)")]
		[MMEnumCondition("MeleeDamageAreaMode", (int)MeleeDamageAreaModes.Generated)]
		public MeleeDamageAreaShapes DamageAreaShape = MeleeDamageAreaShapes.Rectangle;
		/// the offset to apply to the damage area (from the weapon's attachment position
		[Tooltip("손상 영역에 적용할 오프셋(무기 부착 위치에서)")]
		[MMEnumCondition("MeleeDamageAreaMode", (int)MeleeDamageAreaModes.Generated)]
		public Vector3 AreaOffset = new Vector3(1, 0);
		/// the size of the damage area
		[Tooltip("손상 부위의 크기")]
		[MMEnumCondition("MeleeDamageAreaMode", (int)MeleeDamageAreaModes.Generated)]
		public Vector3 AreaSize = new Vector3(1, 1);
		/// the trigger filters this melee weapon should apply damage on (by default, it'll apply damage on everything, but you can change this to only apply when targets enter the area, for example)
		[Tooltip("트리거는 이 근접 무기가 피해를 적용해야 한다고 필터링합니다(기본적으로 모든 것에 피해를 적용하지만 예를 들어 대상이 해당 지역에 들어갈 때만 적용되도록 변경할 수 있습니다).")]
		[MMEnumCondition("MeleeDamageAreaMode", (int)MeleeDamageAreaModes.Generated)]
		public DamageOnTouch.TriggerAndCollisionMask TriggerFilter = DamageOnTouch.AllowedTriggerCallbacks;
		/// the feedback to play when hitting a Damageable
		[Tooltip("Damageable을 칠 때 재생할 피드백")]
		[MMEnumCondition("MeleeDamageAreaMode", (int)MeleeDamageAreaModes.Generated)]
		public MMFeedbacks HitDamageableFeedback;
		/// the feedback to play when hitting a non Damageable
		[Tooltip("손상되지 않는 공격을 가했을 때 재생되는 피드백")]
		[MMEnumCondition("MeleeDamageAreaMode", (int)MeleeDamageAreaModes.Generated)]
		public MMFeedbacks HitNonDamageableFeedback;
		/// an existing damage area to activate/handle as the weapon is used
		[Tooltip("무기를 사용할 때 활성화/처리할 기존 손상 영역")]
		[MMEnumCondition("MeleeDamageAreaMode", (int)MeleeDamageAreaModes.Existing)]
		public DamageOnTouch ExistingDamageArea;

		[MMInspectorGroup("Damage Area Timing", true, 23)]
        
		/// the initial delay to apply before triggering the damage area
		[Tooltip("피해 영역을 트리거하기 전에 적용할 초기 지연")]
		public float InitialDelay = 0f;
		/// the duration during which the damage area is active
		[Tooltip("손상 영역이 활성화되는 기간")]
		public float ActiveDuration = 1f;

		[MMInspectorGroup("Damage Caused", true, 24)]

		/// the layers that will be damaged by this object
		[Tooltip("이 개체에 의해 손상될 레이어")]
		public LayerMask TargetLayerMask;
		/// The min amount of health to remove from the player's health
		[FormerlySerializedAs("DamageCaused")] 
		[Tooltip("플레이어의 체력에서 제거할 최소 체력입니다.")]
		public float MinDamageCaused = 10f;
		/// The max amount of health to remove from the player's health
		[FormerlySerializedAs("DamageCaused")] 
		[Tooltip("플레이어의 체력에서 제거할 최대 체력")]
		public float MaxDamageCaused = 10f;
		/// the kind of knockback to apply
		[Tooltip("적용할 넉백 종류")]
		public DamageOnTouch.KnockbackStyles Knockback;
		/// The force to apply to the object that gets damaged
		[Tooltip("손상된 물체에 가하는 힘")]
		public Vector3 KnockbackForce = new Vector3(10, 2, 0);
		/// The direction in which to apply the knockback 
		[Tooltip("넉백을 적용할 방향")]
		public DamageOnTouch.KnockbackDirections KnockbackDirection = DamageOnTouch.KnockbackDirections.BasedOnOwnerPosition;
		/// The duration of the invincibility frames after the hit (in seconds)
		[Tooltip("적중 후 무적 프레임의 지속 시간(초)")]
		public float InvincibilityDuration = 0.5f;
		/// if this is true, the owner can be damaged by its own weapon's damage area (usually false)
		[Tooltip("이것이 사실이라면, 소유자는 자신의 무기의 피해 범위에 따라 피해를 입을 수 있습니다(보통 거짓).")]
		public bool CanDamageOwner = false;

		protected Collider _damageAreaCollider;
		protected Collider2D _damageAreaCollider2D;
		protected bool _attackInProgress = false;
		protected Color _gizmosColor;
		protected Vector3 _gizmoSize;
		protected CircleCollider2D _circleCollider2D;
		protected BoxCollider2D _boxCollider2D;
		protected BoxCollider _boxCollider;
		protected SphereCollider _sphereCollider;
		protected Vector3 _gizmoOffset;
		protected DamageOnTouch _damageOnTouch;
		protected GameObject _damageArea;
		GameObject findObject;

        private void Awake()
        {

        }

        protected override void Start()
        {
            findObject = transform.parent.gameObject.transform.parent.gameObject;
            animator = findObject.GetComponentInChildren<Animator>();
        }

        /// <summary>
        /// Initialization
        /// </summary>
        public override void Initialization()
		{
			base.Initialization();

			if (_damageArea == null)
			{
				CreateDamageArea();
				DisableDamageArea();
			}
			if (Owner != null)
			{
				_damageOnTouch.Owner = Owner.gameObject;
			}            
		}

		/// <summary>
		/// Creates the damage area.
		/// </summary>
		protected virtual void CreateDamageArea()
		{
			if ((MeleeDamageAreaMode == MeleeDamageAreaModes.Existing) && (ExistingDamageArea != null))
			{
				_damageArea = ExistingDamageArea.gameObject;
				_damageAreaCollider = _damageArea.gameObject.GetComponent<Collider>();
				_damageAreaCollider2D = _damageArea.gameObject.GetComponent<Collider2D>();
				_damageOnTouch = ExistingDamageArea;
				return;
			}
			
			_damageArea = new GameObject();
			_damageArea.name = this.name + "DamageArea";
			_damageArea.transform.position = this.transform.position;
			_damageArea.transform.rotation = this.transform.rotation;
			_damageArea.transform.SetParent(this.transform);
			_damageArea.transform.localScale = Vector3.one;
			_damageArea.layer = this.gameObject.layer;
            
			if (DamageAreaShape == MeleeDamageAreaShapes.Rectangle)
			{
				_boxCollider2D = _damageArea.AddComponent<BoxCollider2D>();
				_boxCollider2D.offset = AreaOffset;
				_boxCollider2D.size = AreaSize;
				_damageAreaCollider2D = _boxCollider2D;
				_damageAreaCollider2D.isTrigger = true;
			}
			if (DamageAreaShape == MeleeDamageAreaShapes.Circle)
			{
				_circleCollider2D = _damageArea.AddComponent<CircleCollider2D>();
				_circleCollider2D.transform.position = this.transform.position;
				_circleCollider2D.offset = AreaOffset;
				_circleCollider2D.radius = AreaSize.x / 2;
				_damageAreaCollider2D = _circleCollider2D;
				_damageAreaCollider2D.isTrigger = true;
			}

			if ((DamageAreaShape == MeleeDamageAreaShapes.Rectangle) || (DamageAreaShape == MeleeDamageAreaShapes.Circle))
			{
				Rigidbody2D rigidBody = _damageArea.AddComponent<Rigidbody2D>();
				rigidBody.isKinematic = true;
				rigidBody.sleepMode = RigidbodySleepMode2D.NeverSleep;
			}            

			if (DamageAreaShape == MeleeDamageAreaShapes.Box)
			{
				_boxCollider = _damageArea.AddComponent<BoxCollider>();
				_boxCollider.center = AreaOffset;
				_boxCollider.size = AreaSize;
				_damageAreaCollider = _boxCollider;
				_damageAreaCollider.isTrigger = true;
			}
			if (DamageAreaShape == MeleeDamageAreaShapes.Sphere)
			{
				_sphereCollider = _damageArea.AddComponent<SphereCollider>();
				_sphereCollider.transform.position = this.transform.position + this.transform.rotation * AreaOffset;
				_sphereCollider.radius = AreaSize.x / 2;
				_damageAreaCollider = _sphereCollider;
				_damageAreaCollider.isTrigger = true;
			}

			if ((DamageAreaShape == MeleeDamageAreaShapes.Box) || (DamageAreaShape == MeleeDamageAreaShapes.Sphere))
			{
				Rigidbody rigidBody = _damageArea.AddComponent<Rigidbody>();
				rigidBody.isKinematic = true;

				rigidBody.gameObject.AddComponent<MMRagdollerIgnore>();
			}

			_damageOnTouch = _damageArea.AddComponent<DamageOnTouch>();
			_damageOnTouch.SetGizmoSize(AreaSize);
			_damageOnTouch.SetGizmoOffset(AreaOffset);
			_damageOnTouch.TargetLayerMask = TargetLayerMask;
            if (gameObject.tag == "Player")
			{
                _damageOnTouch.MinDamageCaused = PlayerDataManager.GetPower();
                _damageOnTouch.MaxDamageCaused = PlayerDataManager.GetPower();
            }
			else
			{
                _damageOnTouch.MinDamageCaused = MinDamageCaused;
                _damageOnTouch.MaxDamageCaused = MaxDamageCaused;
            }
			_damageOnTouch.DamageDirectionMode = DamageOnTouch.DamageDirections.BasedOnOwnerPosition;
			_damageOnTouch.DamageCausedKnockbackType = Knockback;
			_damageOnTouch.DamageCausedKnockbackForce = KnockbackForce;
			_damageOnTouch.DamageCausedKnockbackDirection = KnockbackDirection;
			_damageOnTouch.InvincibilityDuration = InvincibilityDuration;
			_damageOnTouch.HitDamageableFeedback = HitDamageableFeedback;
			_damageOnTouch.HitNonDamageableFeedback = HitNonDamageableFeedback;
			_damageOnTouch.TriggerFilter = TriggerFilter;
            
			if (!CanDamageOwner && (Owner != null))
			{
				_damageOnTouch.IgnoreGameObject(Owner.gameObject);    
			}
		}



        /// <summary>
        /// 무기가 사용되면 공격 루틴이 시작됩니다.
        /// </summary>
        public override void WeaponUse()
		{
            base.WeaponUse();
			StartCoroutine(MeleeWeaponAttack());
        }

        /// <summary>
        /// 공격을 유발하여 피해 영역을 켰다가 끕니다.
        /// </summary>
        /// <returns>무기 공격.</returns>
        protected virtual IEnumerator MeleeWeaponAttack()
		{
			if (_attackInProgress) { yield break; }

            _attackInProgress = true;
			yield return new WaitForSeconds(InitialDelay);
			EnableDamageArea();
			yield return new WaitForSeconds(ActiveDuration);
			DisableDamageArea();
			_attackInProgress = false;
        }

        /// <summary>
        /// 피해 지역을 활성화합니다.
        /// </summary>
        protected virtual void EnableDamageArea()
		{
            //현제 실행되고 있는 에니메이션 이름이 "Attack03_1"이 아닐때만 피해 지역 활성화 (3타 때문)
            if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Attack03_1"))
			{
                if (_damageAreaCollider2D != null)
                {
                    _damageAreaCollider2D.enabled = true;
                }
                if (_damageAreaCollider != null)
                {
                    _damageAreaCollider.enabled = true;
                }
            }
		}


		/// <summary>
		/// Disables the damage area.
		/// </summary>
		protected virtual void DisableDamageArea()
		{
			if (_damageAreaCollider2D != null)
			{
				_damageAreaCollider2D.enabled = false;
			}
			if (_damageAreaCollider != null)
			{
				_damageAreaCollider.enabled = false;
			}
		}

        /// <summary>
        /// 선택하면 여러 기즈모를 그립니다.
        /// </summary>
        protected virtual void OnDrawGizmosSelected()
		{
			if (!Application.isPlaying)
			{
				DrawGizmos();
			}            
		}

		protected virtual void DrawGizmos()
		{
			if (MeleeDamageAreaMode == MeleeDamageAreaModes.Existing)
			{
				return;
			}
			
			if (DamageAreaShape == MeleeDamageAreaShapes.Box)
			{
				Gizmos.DrawWireCube(this.transform.position + AreaOffset, AreaSize);
			}

			if (DamageAreaShape == MeleeDamageAreaShapes.Circle)
			{
				Gizmos.DrawWireSphere(this.transform.position + AreaOffset, AreaSize.x / 2);
			}

			if (DamageAreaShape == MeleeDamageAreaShapes.Rectangle)
			{
				MMDebug.DrawGizmoRectangle(this.transform.position + AreaOffset, AreaSize, Color.red);
			}

			if (DamageAreaShape == MeleeDamageAreaShapes.Sphere)
			{
				Gizmos.DrawWireSphere(this.transform.position + AreaOffset, AreaSize.x / 2);
			}
		}

        /// <summary>
        /// 비활성화 시 플래그를 false로 설정합니다.
        /// </summary>
        protected virtual void OnDisable()
		{
			_attackInProgress = false;
		}
	}
}