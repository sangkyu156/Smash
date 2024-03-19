using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 능력을 사용하면 캐릭터의 이동 방향이나 조준 방향으로 개체의 방향을 지정할 수 있습니다.
    /// 해당 객체는 원하는 무엇이든 될 수 있습니다(스프라이트, 모델, 선 등).
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/Abilities/Character Direction Marker")]
	public class CharacterDirectionMarker : CharacterAbility
	{
		/// the possible modes rotation can be based on
		public enum Modes { MovementDirection, AimDirection, None }
        
		[Header("Direction Marker")]
        /// 회전할 객체
        [Tooltip("회전할 객체")]
		public Transform DirectionMarker;
        /// 마커 능력을 참조하는 데 사용되는 고유 ID
        [Tooltip("마커 능력을 참조하는 데 사용되는 고유 ID")]
		public int DirectionMarkerID;
        /// 방향을 선택하기 위해 선택한 모드
        [Tooltip("방향을 선택하기 위해 선택한 모드")]
		public Modes Mode = Modes.MovementDirection;
        
		[Header("Position")]
        /// 회전 중심으로 적용할 오프셋
        [Tooltip("회전 중심으로 적용할 오프셋")]
		public Vector3 RotationCenterOffset = Vector3.zero;
        /// 조준할 때 위쪽으로 간주하는 축
        [Tooltip("조준할 때 위쪽으로 간주하는 축")]
		public Vector3 UpVector = Vector3.up;
        /// 조준할 때 전방으로 간주할 축
        [Tooltip("조준할 때 전방으로 간주할 축")]
		public Vector3 ForwardVector = Vector3.forward;
        /// 이것이 사실이라면 마커는 X축에서 회전할 수 없습니다.
        [Tooltip("이것이 사실이라면 마커는 X축에서 회전할 수 없습니다.")]
		public bool PreventXRotation = false;
        /// 이것이 사실이라면 마커는 Y축에서 회전할 수 없습니다.
        [Tooltip("이것이 사실이라면 마커는 Y축에서 회전할 수 없습니다.")]
		public bool PreventYRotation = false;
        /// 이것이 사실이라면 마커는 Z축에서 회전할 수 없습니다.
        [Tooltip("이것이 사실이라면 마커는 Z축에서 회전할 수 없습니다.")]
		public bool PreventZRotation = false;

		[Header("Offset along magnitude")]
        /// 방향의 크기에 따라 위치를 오프셋할지 여부(예를 들어 더 빠르게 이동하면 마커가 캐릭터에서 더 멀리 이동할 수 있음)
        [Tooltip("방향의 크기에 따라 위치를 오프셋할지 여부(예를 들어 더 빠르게 이동하면 마커가 캐릭터에서 더 멀리 이동할 수 있음)")]
		public bool OffsetAlongMagnitude = false;
        /// 속도 크기의 최소 경계
        [Tooltip("속도 크기의 최소 경계")]
		[MMCondition("OffsetAlongMagnitude", true)]
		public float MinimumVelocity = 0f;
        /// 속도 크기의 최대 경계
        [Tooltip("속도 크기의 최대 경계")]
		[MMCondition("OffsetAlongMagnitude", true)]
		public float MaximumVelocity = 7f;
        /// 가장 낮은 속도에 있을 때 마커를 배치할 거리
        [Tooltip("가장 낮은 속도에 있을 때 마커를 배치할 거리")]
		[MMCondition("OffsetAlongMagnitude", true)]
		public float OffsetRemapMin = 0f;
        /// 최고 속도에 있을 때 마커를 배치할 거리
        [Tooltip("최고 속도에 있을 때 마커를 배치할 거리")]
		[MMCondition("OffsetAlongMagnitude", true)]
		public float OffsetRemapMax = 1f;
        
		[Header("Auto Disable")]
        /// 이동 크기가 특정 임계값 미만일 때 마커를 비활성화할지 여부
        [Tooltip("이동 크기가 특정 임계값 미만일 때 마커를 비활성화할지 여부")]
		public bool DisableBelowThreshold = false;
        /// 마커를 비활성화하는 임계값
        [Tooltip("마커를 비활성화하는 임계값")]
		[MMCondition("DisableBelowThreshold", true)]
		public float DisableThreshold = 0.1f;
        
		[Header("Interpolation")]
        /// 회전을 보간할지 여부
        [Tooltip("회전을 보간할지 여부")]
		public bool Interpolate = false;
        /// 회전을 보간하는 속도
        [Tooltip("회전을 보간하는 속도")]
		[MMCondition("Interpolate", true)] 
		public float InterpolateRate = 5f;
        
		[Header("Interpolation")] 
		protected CharacterHandleWeapon _characterHandleWeapon;
		protected WeaponAim _weaponAim;
		protected Vector3 _direction;
		protected Quaternion _newRotation;
		protected Vector3 _newPosition;
		protected Vector3 _newRotationVector;
        
		/// <summary>
		/// On init we store our CharacterHandleWeapon
		/// </summary>
		protected override void Initialization()
		{
			base.Initialization();
			_characterHandleWeapon = _character?.FindAbility<CharacterHandleWeapon>();
		}
        
		/// <summary>
		/// On Process, we aim our object
		/// </summary>
		public override void ProcessAbility()
		{
			base.ProcessAbility();
			AimMarker();
		}

		/// <summary>
		/// Rotates the object to match the selected direction
		/// </summary>
		protected virtual void AimMarker()
		{
			if (!AbilityAuthorized)
			{
				return;
			}
            
			if (DirectionMarker == null)
			{
				return;
			}
            
			switch (Mode )
			{
				case Modes.MovementDirection:
					AimAt(_controller.CurrentDirection.normalized);
					ApplyOffset(_controller.Velocity.magnitude);
					break;
				case Modes.AimDirection:
					if (_weaponAim == null)
					{
						GrabWeaponAim();
					}
					else
					{
						AimAt(_weaponAim.CurrentAim.normalized);    
						ApplyOffset(_weaponAim.CurrentAim.magnitude);
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
				_direction = MMMaths.Lerp(_direction, direction, InterpolateRate, Time.deltaTime);
			}
			else
			{
				_direction = direction;
			}

			if (_direction == Vector3.zero)
			{
				return;
			}
            
			_newRotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(_direction, UpVector), InterpolateRate * Time.time);

			_newRotationVector.x = PreventXRotation ? 0f : _newRotation.eulerAngles.x;
			_newRotationVector.y = PreventYRotation ? 0f : _newRotation.eulerAngles.y;
			_newRotationVector.z = PreventZRotation ? 0f : _newRotation.eulerAngles.z;
			_newRotation.eulerAngles = _newRotationVector;
            
			DirectionMarker.transform.rotation = _newRotation;
		}

		/// <summary>
		/// Applies an offset if needed
		/// </summary>
		/// <param name="rawValue"></param>
		protected virtual void ApplyOffset(float rawValue)
		{
			_newPosition = RotationCenterOffset; 

			if (OffsetAlongMagnitude)
			{
				float remappedValue = MMMaths.Remap(rawValue, MinimumVelocity, MaximumVelocity, OffsetRemapMin, OffsetRemapMax);

				_newPosition += ForwardVector * remappedValue; 
				_newPosition = _newRotation * _newPosition;
			}

			if (Interpolate)
			{
				_newPosition = MMMaths.Lerp(DirectionMarker.transform.localPosition, _newPosition, InterpolateRate, Time.deltaTime);
			}

			DirectionMarker.transform.localPosition = _newPosition;

			if (DisableBelowThreshold)
			{
				DirectionMarker.gameObject.SetActive(rawValue > DisableThreshold);    
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
	}    
}