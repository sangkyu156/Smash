using System;
using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 발사체 무기와 함께 사용되는 발사체 클래스
    /// </summary>
    [AddComponentMenu("TopDown Engine/Weapons/Projectile")]
	public class Projectile : MMPoolableObject  
	{
		public enum MovementVectors { Forward, Right, Up}
		
		[Header("Movement")]
		/// if true, the projectile will rotate at initialization towards its rotation
		[Tooltip("true인 경우 발사체는 초기화 시 회전 방향으로 회전합니다.")]
		public bool FaceDirection = true;
		/// if true, the projectile will rotate towards movement
		[Tooltip("true인 경우 발사체가 움직임을 향해 회전합니다.")]
		public bool FaceMovement = false;
		/// if FaceMovement is true, the projectile's vector specified below will be aligned to the movement vector, usually you'll want to go with Forward in 3D, Right in 2D
		[Tooltip("FaceMovement가 true인 경우 아래에 지정된 발사체의 벡터는 이동 벡터에 정렬됩니다. 일반적으로 3D에서는 앞으로, 2D에서는 오른쪽으로 가고 싶을 것입니다.")]
		[MMCondition("FaceMovement", true)]
		public MovementVectors MovementVector = MovementVectors.Forward;

		/// the speed of the object (relative to the level's speed)
		[Tooltip("객체의 속도(레벨 속도에 상대적)")]
		public float Speed = 0;
		/// the acceleration of the object over time. Starts accelerating on enable.
		[Tooltip("시간에 따른 물체의 가속도. 활성화되면 가속이 시작됩니다.")]
		public float Acceleration = 0;
		/// the current direction of the object
		[Tooltip("물체의 현재 방향")]
		public Vector3 Direction = Vector3.left;
		/// if set to true, the spawner can change the direction of the object. If not the one set in its inspector will be used.
		[Tooltip("true로 설정하면 생성자가 객체의 방향을 변경할 수 있습니다. 그렇지 않은 경우 해당 검사기에 설정된 것이 사용됩니다.")]
		public bool DirectionCanBeChangedBySpawner = true;
		/// the flip factor to apply if and when the projectile is mirrored
		[Tooltip("발사체가 미러링되는 경우 적용할 반전 계수")]
		public Vector3 FlipValue = new Vector3(-1,1,1);
		/// set this to true if your projectile's model (or sprite) is facing right, false otherwise
		[Tooltip("발사체의 모델(또는 스프라이트)이 오른쪽을 향하고 있으면 true로 설정하고, 그렇지 않으면 false로 설정합니다.")]
		public bool ProjectileIsFacingRight = true;

		[Header("Spawn")]
		[MMInformation("여기서 이 개체가 손상을 입거나 손상을 입히지 않는 초기 지연(초)을 정의할 수 있습니다. 이 지연은 개체가 활성화되면 시작됩니다. 발사체가 소유자(로켓 등)에 피해를 입힐지 여부도 정의할 수 있습니다.", MoreMountains.Tools.MMInformationAttribute.InformationType.Info,false)]
		/// the initial delay during which the projectile can't be destroyed
		[Tooltip("발사체가 파괴될 수 없는 초기 지연 시간")]
		public float InitialInvulnerabilityDuration=0f;
		/// should the projectile damage its owner?
		[Tooltip("발사체가 소유자에게 피해를 입혀야 합니까?")]
		public bool DamageOwner = false;

		/// Returns the associated damage on touch zone
		public DamageOnTouch TargetDamageOnTouch { get { return _damageOnTouch; } }
		public Weapon SourceWeapon { get { return _weapon; } }

		protected Weapon _weapon;
		protected GameObject _owner;
		protected Vector3 _movement;
		protected float _initialSpeed;
		protected SpriteRenderer _spriteRenderer;
		protected DamageOnTouch _damageOnTouch;
		protected WaitForSeconds _initialInvulnerabilityDurationWFS;
		protected Collider _collider;
		protected Collider2D _collider2D;
		protected Rigidbody _rigidBody;
		protected Rigidbody2D _rigidBody2D;
		protected bool _facingRightInitially;
		protected bool _initialFlipX;
		protected Vector3 _initialLocalScale;
		protected bool _shouldMove = true;
		protected Health _health;
		protected bool _spawnerIsFacingRight;

		/// <summary>
		/// On awake, we store the initial speed of the object 
		/// </summary>
		protected virtual void Awake ()
		{
			_facingRightInitially = ProjectileIsFacingRight;
			_initialSpeed = Speed;
			_health = GetComponent<Health> ();
			_collider = GetComponent<Collider> ();
			_collider2D = GetComponent<Collider2D>();
			_spriteRenderer = GetComponent<SpriteRenderer> ();
			_damageOnTouch = GetComponent<DamageOnTouch>();
			_rigidBody = GetComponent<Rigidbody> ();
			_rigidBody2D = GetComponent<Rigidbody2D> ();
			_initialInvulnerabilityDurationWFS = new WaitForSeconds (InitialInvulnerabilityDuration);
			if (_spriteRenderer != null) {	_initialFlipX = _spriteRenderer.flipX ;		}
			_initialLocalScale = transform.localScale;
		}

		/// <summary>
		/// Handles the projectile's initial invincibility
		/// </summary>
		/// <returns>The invulnerability.</returns>
		protected virtual IEnumerator InitialInvulnerability()
		{
			if (_damageOnTouch == null) { yield break; }
			if (_weapon == null) { yield break; }

			_damageOnTouch.ClearIgnoreList();
			_damageOnTouch.IgnoreGameObject(_weapon.Owner.gameObject);
			yield return _initialInvulnerabilityDurationWFS;
			if (DamageOwner)
			{
				_damageOnTouch.StopIgnoringObject(_weapon.Owner.gameObject);
			}
		}

		/// <summary>
		/// Initializes the projectile
		/// </summary>
		protected virtual void Initialization()
		{
			Speed = _initialSpeed;
			ProjectileIsFacingRight = _facingRightInitially;
			if (_spriteRenderer != null) {	_spriteRenderer.flipX = _initialFlipX;	}
			transform.localScale = _initialLocalScale;	
			_shouldMove = true;
			_damageOnTouch?.InitializeFeedbacks();

			if (_collider != null)
			{
				_collider.enabled = true;
			}
			if (_collider2D != null)
			{
				_collider2D.enabled = true;
			}
		}

		/// <summary>
		/// On update(), we move the object based on the level's speed and the object's speed, and apply acceleration
		/// </summary>
		protected virtual void FixedUpdate ()
		{
			base.Update ();
			if (_shouldMove)
			{
				Movement();
			}
		}

		/// <summary>
		/// Handles the projectile's movement, every frame
		/// </summary>
		public virtual void Movement()
		{
			_movement = Direction * (Speed / 10) * Time.deltaTime;
			//transform.Translate(_movement,Space.World);
			if (_rigidBody != null)
			{
				_rigidBody.MovePosition (this.transform.position + _movement);
			}
			if (_rigidBody2D != null)
			{
				_rigidBody2D.MovePosition(this.transform.position + _movement);
			}
			// We apply the acceleration to increase the speed
			Speed += Acceleration * Time.deltaTime;
		}

		/// <summary>
		/// Sets the projectile's direction.
		/// </summary>
		/// <param name="newDirection">New direction.</param>
		/// <param name="newRotation">New rotation.</param>
		/// <param name="spawnerIsFacingRight">If set to <c>true</c> spawner is facing right.</param>
		public virtual void SetDirection(Vector3 newDirection, Quaternion newRotation, bool spawnerIsFacingRight = true)
		{
			_spawnerIsFacingRight = spawnerIsFacingRight;

			if (DirectionCanBeChangedBySpawner)
			{
				Direction = newDirection;
			}
			if (ProjectileIsFacingRight != spawnerIsFacingRight)
			{
				Flip ();
			}
			if (FaceDirection)
			{
				transform.rotation = newRotation;
			}

			if (_damageOnTouch != null)
			{
				_damageOnTouch.SetKnockbackScriptDirection(newDirection);
			}

			if (FaceMovement)
			{
				switch (MovementVector)
				{
					case MovementVectors.Forward:
						transform.forward = newDirection;
						break;
					case MovementVectors.Right:
						transform.right = newDirection;
						break;
					case MovementVectors.Up:
						transform.up = newDirection;
						break;
				}
			}
		}

		/// <summary>
		/// Flip the projectile
		/// </summary>
		protected virtual void Flip()
		{
			if (_spriteRenderer != null)
			{
				_spriteRenderer.flipX = !_spriteRenderer.flipX;
			}	
			else
			{
				this.transform.localScale = Vector3.Scale(this.transform.localScale,FlipValue) ;
			}
		}
        
		/// <summary>
		/// Flip the projectile
		/// </summary>
		protected virtual void Flip(bool state)
		{
			if (_spriteRenderer != null)
			{
				_spriteRenderer.flipX = state;
			}
			else
			{
				this.transform.localScale = Vector3.Scale(this.transform.localScale, FlipValue);
			}
		}

		/// <summary>
		/// Sets the projectile's parent weapon.
		/// </summary>
		/// <param name="newWeapon">New weapon.</param>
		public virtual void SetWeapon(Weapon newWeapon)
		{
			_weapon = newWeapon;
		}

		/// <summary>
		/// Sets the damage caused by the projectile's DamageOnTouch to the specified value
		/// </summary>
		/// <param name="newDamage"></param>
		public virtual void SetDamage(float minDamage, float maxDamage)
		{
			if (_damageOnTouch != null)
			{
				_damageOnTouch.MinDamageCaused = minDamage;
				_damageOnTouch.MaxDamageCaused = maxDamage;
			}
		}
        
		/// <summary>
		/// Sets the projectile's owner.
		/// </summary>
		/// <param name="newOwner">New owner.</param>
		public virtual void SetOwner(GameObject newOwner)
		{
			_owner = newOwner;
			DamageOnTouch damageOnTouch = this.gameObject.MMGetComponentNoAlloc<DamageOnTouch>();
			if (damageOnTouch != null)
			{
				damageOnTouch.Owner = newOwner;
				if (!DamageOwner)
				{
					damageOnTouch.ClearIgnoreList();
					damageOnTouch.IgnoreGameObject(newOwner);
				}
			}
		}

		/// <summary>
		/// Returns the current Owner of the projectile
		/// </summary>
		/// <returns></returns>
		public virtual GameObject GetOwner()
		{
			return _owner;
		}

		/// <summary>
		/// On death, disables colliders and prevents movement
		/// </summary>
		public virtual void StopAt()
		{
			if (_collider != null)
			{
				_collider.enabled = false;
			}
			if (_collider2D != null)
			{
				_collider2D.enabled = false;
			}
			
			_shouldMove = false;
		}

		/// <summary>
		/// On death, we stop our projectile
		/// </summary>
		protected virtual void OnDeath()
		{
			StopAt ();
		}

		/// <summary>
		/// On enable, we trigger a short invulnerability
		/// </summary>
		protected override void OnEnable ()
		{
			base.OnEnable ();

			Initialization();
			if (InitialInvulnerabilityDuration>0)
			{
				StartCoroutine(InitialInvulnerability());
			}

			if (_health != null)
			{
				_health.OnDeath += OnDeath;
			}
		}

		/// <summary>
		/// On disable, we plug our OnDeath method to the health component
		/// </summary>
		protected override void OnDisable()
		{
			base.OnDisable ();
			if (_health != null)
			{
				_health.OnDeath -= OnDeath;
			}			
		}
	}	
}