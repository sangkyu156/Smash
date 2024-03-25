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
        /// 목표 물체를 겨냥할 수 있는 가능한 방향
        public enum Modes { Movement, WeaponAim }
        /// 이동을 목표로 하는 축 또는 무기 조준 방향
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
        /// 초기화 시 구성 요소를 가져옵니다.
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
        /// 가능하다면 물체를 움직임이나 무기 조준으로 조준합니다.
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
        /// 필요한 경우 회전을 보간하여 대상 객체를 회전합니다.
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
        /// 무기 조준 구성 요소를 캐시합니다.
        /// </summary>
        protected virtual void GrabWeaponAim()
		{
			if ((_characterHandleWeapon != null) && (_characterHandleWeapon.CurrentWeapon != null))
			{
				_weaponAim = _characterHandleWeapon.CurrentWeapon.gameObject.MMGetComponentNoAlloc<WeaponAim>();
			}            
		}

        /// <summary>
        /// 입장 시 무기 조준을 잡고 캐시합니다.
        /// </summary>
        public override void OnEnterState()
		{
			base.OnEnterState();
			GrabWeaponAim();
		}
	}
}