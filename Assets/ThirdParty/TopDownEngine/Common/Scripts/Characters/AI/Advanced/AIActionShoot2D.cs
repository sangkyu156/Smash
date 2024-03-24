using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 현재 장착된 무기를 사용하여 사격하는 Action입니다. 무기가 자동 모드인 경우 해당 상태를 종료할 때까지 발사되고 반자동 모드에서는 한 번만 발사됩니다.
	/// 선택적으로 캐릭터가 대상을 향하게 하고(왼쪽/오른쪽) 이를 조준할 수 있습니다(무기에 WeaponAim 구성 요소가 있는 경우).
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Actions/AIActionShoot2D")]
	//[RequireComponent(typeof(CharacterOrientation2D))]
	//[RequireComponent(typeof(CharacterHandleWeapon))]
	public class AIActionShoot2D : AIAction
	{
		public enum AimOrigins { Transform, SpawnPoint }
        
		[Header("Binding")]
        /// 이 AI 작업이 조종해야 하는 CharacterHandleWeapon 능력입니다. 공백으로 두면 시스템은 가장 먼저 찾은 항목을 가져옵니다.
        [Tooltip("이 AI 작업이 조종해야 하는 CharacterHandleWeapon 능력입니다. 공백으로 두면 시스템은 가장 먼저 찾은 항목을 가져옵니다.")]
		public CharacterHandleWeapon TargetHandleWeaponAbility;

		[Header("Behaviour")]
        /// 대상을 향한 조준 방향을 계산할 때 고려할 원점
        [Tooltip("대상을 향한 조준 방향을 계산할 때 고려할 원점")]
		public AimOrigins AimOrigin = AimOrigins.Transform;
        /// true인 경우 캐릭터는 사격할 때 대상(왼쪽/오른쪽)을 향하게 됩니다
        [Tooltip("iftrue, 캐릭터는 사격할 때 대상(왼쪽/오른쪽)을 향하게 됩니다.")]
		public bool FaceTarget = true;
        /// true인 경우 캐릭터는 총을 쏠 때 대상을 조준합니다.
        [Tooltip("true인 경우 캐릭터는 총을 쏠 때 대상을 조준합니다.")]
		public bool AimAtTarget = false;
        /// 이 상태에서만 조준을 수행할지 여부
        [Tooltip("이 상태에서만 조준을 수행할지 여부")]
		[MMCondition("AimAtTarget")]
		public bool OnlyAimWhenInState = false;

		protected CharacterOrientation2D _orientation2D;
		protected Character _character;
		protected WeaponAim _weaponAim;
		protected ProjectileWeapon _projectileWeapon;
		protected Vector3 _weaponAimDirection;
		protected int _numberOfShoots = 0;
		protected bool _shooting = false;

		/// <summary>
		/// On init we grab our CharacterHandleWeapon ability
		/// </summary>
		public override void Initialization()
		{
			if(!ShouldInitialize) return;
			base.Initialization();
			_character = GetComponentInParent<Character>();
			_orientation2D = _character?.FindAbility<CharacterOrientation2D>();
			if (TargetHandleWeaponAbility == null)
			{
				TargetHandleWeaponAbility = _character?.FindAbility<CharacterHandleWeapon>();
			}
		}

		/// <summary>
		/// On PerformAction we face and aim if needed, and we shoot
		/// </summary>
		public override void PerformAction()
		{
			MakeChangesToTheWeapon();
			TestFaceTarget();
			TestAimAtTarget();
			Shoot();
		}

		/// <summary>
		/// Sets the current aim if needed
		/// </summary>
		protected virtual void Update()
		{
			if (OnlyAimWhenInState && !_shooting)
			{
				return;
			}
			
			if (TargetHandleWeaponAbility.CurrentWeapon != null)
			{
				if (_weaponAim != null)
				{
					if (_shooting)
					{
						_weaponAim.SetCurrentAim(_weaponAimDirection);
					}
					else
					{
						if (_orientation2D != null)
						{
							if (_orientation2D.IsFacingRight)
							{
								_weaponAim.SetCurrentAim(Vector3.right);
							}
							else
							{
								_weaponAim.SetCurrentAim(Vector3.left);
							}
						}                        
					}
				}
			}
		}

		/// <summary>
		/// Makes changes to the weapon to ensure it works ok with AI scripts
		/// </summary>
		protected virtual void MakeChangesToTheWeapon()
		{
			if (TargetHandleWeaponAbility.CurrentWeapon != null)
			{
				TargetHandleWeaponAbility.CurrentWeapon.TimeBetweenUsesReleaseInterruption = true;
			}
		}

		/// <summary>
		/// Faces the target if required
		/// </summary>
		protected virtual void TestFaceTarget()
		{
			if (!FaceTarget)
			{
				return;
			}

			if (this.transform.position.x > _brain.Target.position.x)
			{
				_orientation2D.FaceDirection(-1);
			}
			else
			{
				_orientation2D.FaceDirection(1);
			}            
		}

		/// <summary>
		/// Aims at the target if required
		/// </summary>
		protected virtual void TestAimAtTarget()
		{
			if (!AimAtTarget)
			{
				return;
			}

			if (TargetHandleWeaponAbility.CurrentWeapon != null)
			{
				if (_weaponAim == null)
				{
					_weaponAim = TargetHandleWeaponAbility.CurrentWeapon.gameObject.MMGetComponentNoAlloc<WeaponAim>();
				}

				if (_weaponAim != null)
				{
					if ((AimOrigin == AimOrigins.SpawnPoint) && (_projectileWeapon != null))
					{
						_projectileWeapon.DetermineSpawnPosition();
						_weaponAimDirection = _brain.Target.position - _projectileWeapon.SpawnPosition;
					}
					else
					{
						_weaponAimDirection = _brain.Target.position - _character.transform.position;
					}                    
				}                
			}
		}

		/// <summary>
		/// Activates the weapon
		/// </summary>
		protected virtual void Shoot()
		{
			if (_numberOfShoots < 1)
			{
				TargetHandleWeaponAbility.ShootStart();
				_numberOfShoots++;
			}
		}

		/// <summary>
		/// When entering the state we reset our shoot counter and grab our weapon
		/// </summary>
		public override void OnEnterState()
		{
			base.OnEnterState();
			_numberOfShoots = 0;
			_shooting = true;
			if (TargetHandleWeaponAbility.CurrentWeapon != null)
			{
				_weaponAim = TargetHandleWeaponAbility.CurrentWeapon.gameObject.MMGetComponentNoAlloc<WeaponAim>();
				_projectileWeapon = TargetHandleWeaponAbility.CurrentWeapon.gameObject.MMGetComponentNoAlloc<ProjectileWeapon>();	
			}
		}

		/// <summary>
		/// When exiting the state we make sure we're not shooting anymore
		/// </summary>
		public override void OnExitState()
		{
			base.OnExitState();
			if (TargetHandleWeaponAbility != null)
			{
				TargetHandleWeaponAbility.ForceStop();    
			}
			_shooting = false;
		}
	}
}