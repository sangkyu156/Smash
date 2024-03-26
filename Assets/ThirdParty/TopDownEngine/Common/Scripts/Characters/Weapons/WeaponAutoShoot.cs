using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// WeaponAutoAim(2D 또는 3D)이 있는 무기에 이 구성 요소를 추가하면 선택적인 지연 후 자동으로 대상을 향해 발사됩니다.
    /// 자동 촬영을 방지/중지하려면 이 구성요소를 비활성화하고 다시 활성화하여 자동 촬영을 재개하세요.
    /// </summary>
    public class WeaponAutoShoot : TopDownMonoBehaviour
	{
		[Header("Auto Shoot")]
		/// the delay (in seconds) between acquiring a target and starting shooting at it
		[Tooltip("표적 획득과 사격 시작 사이의 지연(초)")]
		public float DelayBeforeShootAfterAcquiringTarget = 0.1f;
		/// if this is true, the weapon will only auto shoot if its owner is idle 
		[Tooltip("이것이 사실이라면 무기는 소유자가 유휴 상태일 때만 자동으로 발사됩니다.")]
		public bool OnlyAutoShootIfOwnerIsIdle = false;
		
		protected WeaponAutoAim _weaponAutoAim;
		protected Weapon _weapon;
		protected bool _hasWeaponAndAutoAim;
		protected float _targetAcquiredAt;
		protected Transform _lastTarget;
        
		/// <summary>
		/// On Awake we initialize our component
		/// </summary>
		protected virtual void Start()
		{
			Initialization();
		}
        
		/// <summary>
		/// Grabs auto aim and weapon
		/// </summary>
		protected virtual void Initialization()
		{
			_weaponAutoAim = this.gameObject.GetComponent<WeaponAutoAim>();
			_weapon = this.gameObject.GetComponent<Weapon>();
			if (_weaponAutoAim == null)
			{
				Debug.LogWarning(this.name + " : the WeaponAutoShoot on this object requires that you add either a WeaponAutoAim2D or WeaponAutoAim3D component to your weapon.");
				return;
			}
			_hasWeaponAndAutoAim = (_weapon != null) && (_weaponAutoAim != null);
		}

		/// <summary>
		/// A public method you can use to update the cached Weapon
		/// </summary>
		/// <param name="newWeapon"></param>
		public virtual void SetCurrentWeapon(Weapon newWeapon)
		{
			_weapon = newWeapon;
		}

        /// <summary>
        /// 업데이트 시 자동 Shoot을 처리합니다.
        /// </summary>
        protected virtual void LateUpdate()
		{
			HandleAutoShoot();
		}

		/// <summary>
		/// Returns true if this weapon can autoshoot, false otherwise
		/// </summary>
		/// <returns></returns>
		public virtual bool CanAutoShoot()
		{
			if (!_hasWeaponAndAutoAim)
			{
				return false;
			}

			if (OnlyAutoShootIfOwnerIsIdle)
			{
				if (_weapon.Owner.MovementState.CurrentState != CharacterStates.MovementStates.Idle)
				{
					return false;
				}
			}

			return true;
		}

        /// <summary>
        /// 충분한 시간 동안 목표물이 있는지 확인하고 필요한 경우 Shoot합니다.
        /// </summary>
        protected virtual void HandleAutoShoot()
		{
			if (!CanAutoShoot())
			{
				return;
			}

			if (_weaponAutoAim.Target != null)
			{
				if (_lastTarget != _weaponAutoAim.Target)
				{
					_targetAcquiredAt = Time.time;
				}

				if (Time.time - _targetAcquiredAt >= DelayBeforeShootAfterAcquiringTarget)
				{
					_weapon.WeaponInputStart();    
				}
				_lastTarget = _weaponAutoAim.Target;
			}
		}
	}    
}