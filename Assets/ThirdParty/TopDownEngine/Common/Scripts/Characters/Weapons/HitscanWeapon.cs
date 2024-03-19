using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Random = System.Random;

namespace MoreMountains.TopDownEngine
{
	public class HitscanWeapon : Weapon
	{
		/// the possible modes this weapon laser sight can run on, 3D by default
		public enum Modes { TwoD, ThreeD }

		[MMInspectorGroup("Hitscan Spawn", true, 23)]
		/// the offset position at which the projectile will spawn
		[Tooltip("발사체가 생성되는 오프셋 위치")]
		public Vector3 ProjectileSpawnOffset = Vector3.zero;
		/// the spread (in degrees) to apply randomly (or not) on each angle when spawning a projectile
		[Tooltip("발사체를 생성할 때 각 각도에 무작위로 적용할(또는 적용하지 않을) 확산(도)")]
		public Vector3 Spread = Vector3.zero;
		/// whether or not the weapon should rotate to align with the spread angle
		[Tooltip("무기가 확산 각도에 맞춰 회전해야 하는지 여부")]
		public bool RotateWeaponOnSpread = false;
		/// whether or not the spread should be random (if not it'll be equally distributed)
		[Tooltip("스프레드가 무작위로 이루어져야 하는지 여부(그렇지 않은 경우 균등하게 분산됨)")]
		public bool RandomSpread = true;
		/// the projectile's spawn position
		[MMReadOnly]
		[Tooltip("발사체의 스폰 위치")]
		public Vector3 SpawnPosition = Vector3.zero;

		[MMInspectorGroup("Hitscan", true, 24)]
		/// whether this hitscan should work in 2D or 3D
		[Tooltip("이 히트스캔이 2D에서 작동해야 하는지 아니면 3D에서 작동해야 하는지 여부")]
		public Modes Mode = Modes.ThreeD;
		/// the layer(s) on which to hitscan ray should collide
		[Tooltip("히트스캔 광선이 충돌해야 하는 레이어")]
		public LayerMask HitscanTargetLayers;
		/// the maximum distance of this weapon, after that bullets will be considered lost
		[Tooltip("이 무기의 최대 거리. 그 이후에는 총알이 손실된 것으로 간주됩니다.")]
		public float HitscanMaxDistance = 100f;
		/// the min amount of damage to apply to a damageable (something with a Health component) every time there's a hit
		[FormerlySerializedAs("DamageCaused")] 
		[Tooltip("적중이 발생할 때마다 손상 가능한 항목(건강 구성 요소가 있는 항목)에 적용할 최소 손상량입니다.")]
		public float MinDamageCaused = 5;
		/// the maximum amount of damage to apply to a damageable (something with a Health component) every time there's a hit 
		[Tooltip("적중이 발생할 때마다 손상 가능한 항목(건강 구성 요소가 있는 항목)에 적용할 최대 손상량입니다.")]
		public float MaxDamageCaused = 5;
		/// the duration of the invincibility after a hit (to prevent insta death in the case of rapid fire)
		[Tooltip("명중 후 무적 시간 (속사시 즉사 방지)")]
		public float DamageCausedInvincibilityDuration = 0.2f;
		/// a list of typed damage definitions that will be applied on top of the base damage
		[Tooltip("기본 손상 위에 적용될 입력된 손상 정의 목록")]
		public List<TypedDamage> TypedDamages;
		
		[MMInspectorGroup("Hit Damageable", true, 25)]
		/// a MMFeedbacks to move to the position of the hit and to play when hitting something with a Health component
		[Tooltip("히트 위치로 이동하고 Health 구성 요소로 무언가를 히트할 때 재생하는 MMFeedbacks")]
		public MMFeedbacks HitDamageable;
		/// a particle system to move to the position of the hit and to play when hitting something with a Health component
		[Tooltip("적중 위치로 이동하고 Health 구성 요소로 무언가를 칠 때 재생되는 입자 시스템")]
		public ParticleSystem DamageableImpactParticles;
        
		[MMInspectorGroup("Hit Non Damageable", true, 26)]
		/// a MMFeedbacks to move to the position of the hit and to play when hitting something without a Health component
		[Tooltip("히트 위치로 이동하고 Health 구성 요소 없이 무언가를 히트할 때 재생하는 MMFeedbacks")]
		public MMFeedbacks HitNonDamageable;
		/// a particle system to move to the position of the hit and to play when hitting something without a Health component
		[Tooltip("히트 위치로 이동하고 Health 구성 요소 없이 무언가를 히트할 때 재생되는 파티클 시스템")]
		public ParticleSystem NonDamageableImpactParticles;

		protected Vector3 _flippedProjectileSpawnOffset;
		protected Vector3 _randomSpreadDirection;
		protected bool _initialized = false;
		protected Transform _projectileSpawnTransform;
		public RaycastHit _hit { get; protected set; }
		public RaycastHit2D _hit2D { get; protected set; }
		public Vector3 _origin { get; protected set; }
		protected Vector3 _destination;
		protected Vector3 _direction;
		protected GameObject _hitObject = null;
		protected Vector3 _hitPoint;
		protected Health _health;
		protected Vector3 _damageDirection;

		[MMInspectorButton("TestShoot")]
		/// a button to test the shoot method
		public bool TestShootButton;

		/// <summary>
		/// A test method that triggers the weapon
		/// </summary>
		protected virtual void TestShoot()
		{
			if (WeaponState.CurrentState == WeaponStates.WeaponIdle)
			{
				WeaponInputStart();
			}
			else
			{
				WeaponInputStop();
			}
		}

		/// <summary>
		/// Initialize this weapon
		/// </summary>
		public override void Initialization()
		{
			base.Initialization();
			_weaponAim = GetComponent<WeaponAim>();

			if (!_initialized)
			{
				if (FlipWeaponOnCharacterFlip)
				{
					_flippedProjectileSpawnOffset = ProjectileSpawnOffset;
					_flippedProjectileSpawnOffset.y = -_flippedProjectileSpawnOffset.y;
				}
				_initialized = true;
			}
		}

		/// <summary>
		/// Called everytime the weapon is used
		/// </summary>
		public override void WeaponUse()
		{
			base.WeaponUse();

			DetermineSpawnPosition();
			DetermineDirection();
			SpawnProjectile(SpawnPosition, true);
			HandleDamage();
		}

		/// <summary>
		/// Determines the direction of the ray we have to cast
		/// </summary>
		protected virtual void DetermineDirection()
		{
			if (RandomSpread)
			{
				_randomSpreadDirection.x = UnityEngine.Random.Range(-Spread.x, Spread.x);
				_randomSpreadDirection.y = UnityEngine.Random.Range(-Spread.y, Spread.y);
				_randomSpreadDirection.z = UnityEngine.Random.Range(-Spread.z, Spread.z);
			}
			else
			{
                
				_randomSpreadDirection = Vector3.zero;
			}
            
			Quaternion spread = Quaternion.Euler(_randomSpreadDirection);
            
			if (Owner.CharacterDimension == Character.CharacterDimensions.Type3D)
			{
				_randomSpreadDirection = spread * transform.forward;
			}
			else
			{
				_randomSpreadDirection = spread * transform.right * (Flipped ? -1 : 1);
			}
            
			if (RotateWeaponOnSpread)
			{
				this.transform.rotation = this.transform.rotation * spread;
			}
		}

		/// <summary>
		/// Spawns a new object and positions/resizes it
		/// </summary>
		public virtual void SpawnProjectile(Vector3 spawnPosition, bool triggerObjectActivation = true)
		{
			_hitObject = null;

			// we cast a ray in the direction
			if (Mode == Modes.ThreeD)
			{
				// if 3D
				_origin = SpawnPosition;
				_hit = MMDebug.Raycast3D(_origin, _randomSpreadDirection, HitscanMaxDistance, HitscanTargetLayers, Color.red, true);
                
				// if we've hit something, our destination is the raycast hit
				if (_hit.transform != null)
				{
					_hitObject = _hit.collider.gameObject;
					_hitPoint = _hit.point;

				}
				// otherwise we just draw our laser in front of our weapon 
				else
				{
					_hitObject = null;
				}
			}
			else
			{
				// if 2D

				//_direction = this.Flipped ? Vector3.left : Vector3.right;

				// we cast a ray in front of the weapon to detect an obstacle
				_origin = SpawnPosition;
				_hit2D = MMDebug.RayCast(_origin, _randomSpreadDirection, HitscanMaxDistance, HitscanTargetLayers, Color.red, true);
				if (_hit2D)
				{
					_hitObject = _hit2D.collider.gameObject;
					_hitPoint = _hit2D.point;
				}
				// otherwise we just draw our laser in front of our weapon 
				else
				{
					_hitObject = null;
				}
			}      
		}

        /// <summary>
        /// 손상 및 관련 피드백 처리
        /// </summary>
        protected virtual void HandleDamage()
		{
			if (_hitObject == null)
			{
				return;
			}

			_health = _hitObject.MMGetComponentNoAlloc<Health>();

			if (_health == null)
			{
				// hit non damageable
				if (HitNonDamageable != null)
				{
					HitNonDamageable.transform.position = _hitPoint;
					HitNonDamageable.transform.LookAt(this.transform);
					HitNonDamageable.PlayFeedbacks();
				}

				if (NonDamageableImpactParticles != null)
				{
					NonDamageableImpactParticles.transform.position = _hitPoint;
					NonDamageableImpactParticles.transform.LookAt(this.transform);
					NonDamageableImpactParticles.Play();
				}
			}
			else
			{
				// hit damageable
				_damageDirection = (_hitObject.transform.position - this.transform.position).normalized;
                
				float randomDamage = UnityEngine.Random.Range(MinDamageCaused, Mathf.Max(MaxDamageCaused, MinDamageCaused));
				_health.Damage(randomDamage, this.gameObject, DamageCausedInvincibilityDuration, DamageCausedInvincibilityDuration, _damageDirection, TypedDamages);

				if (HitDamageable != null)
				{
					HitDamageable.transform.position = _hitPoint;
					HitDamageable.transform.LookAt(this.transform);
					HitDamageable.PlayFeedbacks();
				}
                
				if (DamageableImpactParticles != null)
				{
					DamageableImpactParticles.transform.position = _hitPoint;
					DamageableImpactParticles.transform.LookAt(this.transform);
					DamageableImpactParticles.Play();
				}
			}

		}

		/// <summary>
		/// Determines the spawn position based on the spawn offset and whether or not the weapon is flipped
		/// </summary>
		public virtual void DetermineSpawnPosition()
		{
			if (Flipped)
			{
				if (FlipWeaponOnCharacterFlip)
				{
					SpawnPosition = this.transform.position - this.transform.rotation * _flippedProjectileSpawnOffset;
				}
				else
				{
					SpawnPosition = this.transform.position - this.transform.rotation * ProjectileSpawnOffset;
				}
			}
			else
			{
				SpawnPosition = this.transform.position + this.transform.rotation * ProjectileSpawnOffset;
			}

			if (WeaponUseTransform != null)
			{
				SpawnPosition = WeaponUseTransform.position;
			}
		}

		/// <summary>
		/// When the weapon is selected, draws a circle at the spawn's position
		/// </summary>
		protected virtual void OnDrawGizmosSelected()
		{
			DetermineSpawnPosition();

			Gizmos.color = Color.white;
			Gizmos.DrawWireSphere(SpawnPosition, 0.2f);
		}

	}
}