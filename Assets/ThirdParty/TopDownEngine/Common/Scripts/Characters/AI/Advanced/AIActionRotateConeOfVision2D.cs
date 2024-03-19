using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 AIAction은 AI의 ConeOfVision2D를 AI의 움직임이나 무기 조준 방향으로 회전시킵니다.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Actions/AIActionRotateConeOfVision2D")]
	public class AIActionRotateConeOfVision2D : AIAction
	{
		/// the possible directions we can aim the cone at
		public enum Modes { Movement, WeaponAim }
		
		[Header("Bindings")]
        /// Vision 2D의 원뿔이 회전합니다.
        [Tooltip("Vision 2D의 원뿔이 회전합니다.")]
		public MMConeOfVision2D TargetConeOfVision2D;
        
		[Header("Aim")]
        /// AI의 이동 방향을 겨냥할지 무기 조준 방향을 겨냥할지
        [Tooltip("AI의 이동 방향을 겨냥할지 무기 조준 방향을 겨냥할지")]
		public Modes Mode = Modes.Movement;

		[Header("Interpolation")]
        /// 회전을 보간할지 여부
        [Tooltip("회전을 보간할지 여부")]
		public bool Interpolate = false;
        /// the회전을 보간하는 속도
        [Tooltip("회전을 보간하는 속도")]
		[MMCondition("Interpolate", true)] 
		public float InterpolateRate = 5f;
        
		protected CharacterHandleWeapon _characterHandleWeapon;
		protected WeaponAim _weaponAim;
		protected TopDownController _controller;
		protected Vector3 _newAim;
		protected float _angle;
		protected Vector3 _eulerAngles = Vector3.zero;
        
		/// <summary>
		/// On init we grab our components
		/// </summary>
		public override void Initialization()
		{
			if(!ShouldInitialize) return;
			base.Initialization();
			_characterHandleWeapon = this.gameObject.GetComponentInParent<Character>()?.FindAbility<CharacterHandleWeapon>();
			_controller = this.gameObject.GetComponentInParent<TopDownController>();
			if (TargetConeOfVision2D == null)
			{
				TargetConeOfVision2D = this.gameObject.GetComponent<MMConeOfVision2D>();	
			}
		}

		public override void PerformAction()
		{
			AimCone();
		}

		/// <summary>
		/// Aims the cone at either movement or weapon aim if possible
		/// </summary>
		protected virtual void AimCone()
		{
			if (TargetConeOfVision2D == null)
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
		/// Rotates the cone, interpolating the rotation if needed
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

			_angle = MMMaths.AngleBetween(this.transform.right, _newAim);
			_eulerAngles.y = -_angle;
            
			TargetConeOfVision2D.SetDirectionAndAngles(_newAim, _eulerAngles);
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