using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;



namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// AI의 움직임이나 조준 방향으로 객체를 조준하는 데 사용되는 AIACtion입니다.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Actions/AIActionAimObject")]
	public class AIActionAimObject : AIAction
	{
		/// the possible directions we can aim the target object at
		public enum Modes { Movement, WeaponAim }
		/// the axis to aim at the movement or weapon aim direction
		public enum PossibleAxis { Right, Forward }
        
		[Header("Aim Object")]
        /// 목표로 삼는 대상
        [Tooltip("목표로 삼는 대상")]
		public GameObject GameObjectToAim;
        /// AI의 이동 방향을 겨냥할지 무기 조준 방향을 겨냥할지
        [Tooltip("AI의 이동 방향을 겨냥할지 무기 조준 방향을 겨냥할지")]
		public Modes Mode = Modes.Movement;
        /// 순간에 조준할 축 또는 무기 조준 방향(보통 2D에서는 오른쪽, 3D에서는 앞으로)
        [Tooltip("순간에 조준할 축 또는 무기 조준 방향(보통 2D에서는 오른쪽, 3D에서는 앞으로)")]
		public PossibleAxis Axis = PossibleAxis.Right;

		[Header("Interpolation")]
        /// 회전을 보간할지 여부
        [Tooltip("회전을 보간할지 여부")]
		public bool Interpolate = false;
        /// 회전을 보간하는 속도
        [Tooltip("회전을 보간하는 속도")]
		[MMCondition("Interpolate", true)] 
		public float InterpolateRate = 5f;
        
		protected CharacterHandleWeapon _characterHandleWeapon;
		protected WeaponAim _weaponAim;
		protected TopDownController _controller;
		protected Vector3 _newAim;
        
		/// <summary>
		/// On init we grab our components
		/// </summary>
		public override void Initialization()
		{
			if(!ShouldInitialize) return;
			base.Initialization();
			_characterHandleWeapon = this.gameObject.GetComponentInParent<Character>()?.FindAbility<CharacterHandleWeapon>();
			_controller = this.gameObject.GetComponentInParent<TopDownController>();
		}

		public override void PerformAction()
		{
			AimObject();
		}

		/// <summary>
		/// Aims the object at either movement or weapon aim if possible
		/// </summary>
		protected virtual void AimObject()
		{
			if (GameObjectToAim == null)
			{
				return;
			}
            
			switch (Mode )
			{
				case Modes.Movement:
					AimAt(_controller.CurrentDirection.normalized);
					break;
				case Modes.WeaponAim:
					if (_weaponAim == null)
					{
						GrabWeaponAim();
					}
					else
					{
						AimAt(_weaponAim.CurrentAim.normalized);    
					}
					break;
			}
		}

		/// <summary>
		/// Rotates the target object, interpolating the rotation if needed
		/// </summary>
		/// <param name="direction"></param>
		protected virtual void AimAt(Vector3 direction)
		{
			if (Interpolate)
			{
				_newAim = MMMaths.Lerp(_newAim, direction, InterpolateRate, Time.deltaTime);
			}
			else
			{
				_newAim = direction;
			}
            
			switch (Axis)
			{
				case PossibleAxis.Forward:
					GameObjectToAim.transform.forward = _newAim;
					break;
				case PossibleAxis.Right:
					GameObjectToAim.transform.right = _newAim;
					break;
			}
		}
        
		/// <summary>
		/// Caches the weapon aim comp
		/// </summary>
		protected virtual void GrabWeaponAim()
		{
			if ((_characterHandleWeapon != null) && (_characterHandleWeapon.CurrentWeapon != null))
			{
				_weaponAim = _characterHandleWeapon.CurrentWeapon.gameObject.MMGetComponentNoAlloc<WeaponAim>();
			}            
		}
        
		/// <summary>
		/// On entry we grab the weapon aim and cache it
		/// </summary>
		public override void OnEnterState()
		{
			base.OnEnterState();
			GrabWeaponAim();
		}
	}
}