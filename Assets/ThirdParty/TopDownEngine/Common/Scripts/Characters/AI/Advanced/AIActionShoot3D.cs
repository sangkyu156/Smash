using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 현재 장착된 무기를 사용하여 사격하는 액션. 무기가 자동 모드인 경우 상태를 종료할 때까지 사격하고 세미 오토 모드에서는 한 번만 사격합니다. 
	/// 선택적으로 캐릭터가 목표물을 향해(좌/우) 조준할 수 있습니다(무기에 WeaponAim 구성 요소가 있는 경우).
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Actions/AIActionShoot3D")]
	//[RequireComponent(typeof(CharacterOrientation3D))]
	//[RequireComponent(typeof(CharacterHandleWeapon))]
	public class AIActionShoot3D : AIAction
	{
		public enum AimOrigins { Transform, SpawnPosition }
        
		[Header("Binding")]
        /// 이 AI 작업이 조종해야 하는 CharacterHandleWeapon 능력입니다. 공백으로 두면 시스템은 가장 먼저 찾은 항목을 가져옵니다.
        [Tooltip("이 AI 작업이 조종해야 하는 CharacterHandleWeapon 능력입니다. 공백으로 두면 시스템은 가장 먼저 찾은 항목을 가져옵니다.")]
		public CharacterHandleWeapon TargetHandleWeaponAbility;
        
		[Header("Behaviour")]
        /// true인 경우 캐릭터는 총을 쏠 때 대상을 조준합니다.
        [Tooltip("true인 경우 캐릭터는 총을 쏠 때 대상을 조준합니다.")]
		public bool AimAtTarget = true;
        /// 목표 원점으로 간주하는 지점
        [Tooltip("목표 원점으로 간주하는 지점")]
		public AimOrigins AimOrigin = AimOrigins.Transform;
        /// 조준에 적용할 오프셋(머리/몸통 등을 자동으로 조준하는 데 유용함)
        [Tooltip("조준에 적용할 오프셋(머리/몸통 등을 자동으로 조준하는 데 유용함)")]
		public Vector3 ShootOffset;
        /// 이것이 true로 설정되면 수직 조준이 수평을 유지하도록 잠깁니다.
        [Tooltip("이것이 true로 설정되면 수직 조준이 수평을 유지하도록 잠깁니다.")]
		public bool LockVerticalAim = false;

		protected CharacterOrientation3D _orientation3D;
		protected Character _character;
		protected WeaponAim _weaponAim;
		protected ProjectileWeapon _projectileWeapon;
		protected Vector3 _weaponAimDirection;
		protected int _numberOfShoots = 0;
		protected bool _shooting = false;
		protected Weapon _targetWeapon;

        /// <summary>
        /// 초기화 시 CharacterHandleWeapon 기능을 가져옵니다.
        /// </summary>
        public override void Initialization()
		{
			if(!ShouldInitialize) return;
			base.Initialization();
			_character = GetComponentInParent<Character>();
			_orientation3D = _character?.FindAbility<CharacterOrientation3D>();
			if (TargetHandleWeaponAbility == null)
			{
				TargetHandleWeaponAbility = _character?.FindAbility<CharacterHandleWeapon>();
			}
		}

        /// <summary>
        /// PerformAction에서는 필요한 경우 정면을 바라보고 조준한 다음 촬영합니다.
        /// </summary>
        public override void PerformAction()
		{
			MakeChangesToTheWeapon();
			TestAimAtTarget();
			Shoot();
		}

        /// <summary>
        /// AI 스크립트와 잘 작동하도록 무기를 변경합니다.
        /// </summary>
        protected virtual void MakeChangesToTheWeapon()
		{
			if (TargetHandleWeaponAbility.CurrentWeapon != null)
			{
				TargetHandleWeaponAbility.CurrentWeapon.TimeBetweenUsesReleaseInterruption = true;
			}
		}

        /// <summary>
        /// 필요한 경우 현재 목표를 설정합니다.
        /// </summary>
        protected virtual void Update()
		{
			if (TargetHandleWeaponAbility.CurrentWeapon != null)
			{
				if (_weaponAim != null)
				{
					if (_shooting)
					{
						if (LockVerticalAim)
						{
							_weaponAimDirection.y = 0;
						}

						if (AimAtTarget)
						{
							_weaponAim.SetCurrentAim(_weaponAimDirection);    
						}
					}
				}
			}
		}

        /// <summary>
        /// 필요한 경우 목표물을 겨냥합니다.
        /// </summary>
        protected virtual void TestAimAtTarget()
		{
			if (!AimAtTarget || (_brain.Target == null))
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
					if (_projectileWeapon != null)
					{
						if (AimOrigin == AimOrigins.Transform)
						{
							_weaponAimDirection = _brain.Target.position + ShootOffset - _character.transform.position;   
						}
						else if (AimOrigin == AimOrigins.SpawnPosition)
						{
							_projectileWeapon.DetermineSpawnPosition();
							_weaponAimDirection = _brain.Target.position + ShootOffset - _projectileWeapon.SpawnPosition;    
						}
					}
					else
					{
						_weaponAimDirection = _brain.Target.position + ShootOffset - _character.transform.position;
					}                    
				}                
			}
		}

        /// <summary>
        /// 무기를 활성화합니다
        /// </summary>
        protected virtual void Shoot()
		{
			if (_numberOfShoots < 1)
			{
				_targetWeapon = TargetHandleWeaponAbility.CurrentWeapon;
				TargetHandleWeaponAbility.ShootStart();
				_numberOfShoots++;
			}

			if ((_targetWeapon == null) || (TargetHandleWeaponAbility.CurrentWeapon != _targetWeapon))
			{
				OnEnterState();
			}
		}

        /// <summary>
        /// 상태에 들어가면 사격 카운터를 재설정하고 무기를 잡습니다.
        /// </summary>
        public override void OnEnterState()
		{
			base.OnEnterState();
			_numberOfShoots = 0;
			_shooting = true;
			_weaponAim = TargetHandleWeaponAbility.CurrentWeapon.gameObject.MMGetComponentNoAlloc<WeaponAim>();
			_projectileWeapon = TargetHandleWeaponAbility.CurrentWeapon.gameObject.MMGetComponentNoAlloc<ProjectileWeapon>();
        }

        /// <summary>
        /// 상태를 종료할 때 더 이상 총격을 가하지 않는지 확인합니다.
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