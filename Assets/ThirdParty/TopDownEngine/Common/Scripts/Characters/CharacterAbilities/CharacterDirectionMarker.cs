using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// �� �ɷ��� ����ϸ� ĳ������ �̵� �����̳� ���� �������� ��ü�� ������ ������ �� �ֽ��ϴ�.
    /// �ش� ��ü�� ���ϴ� �����̵� �� �� �ֽ��ϴ�(��������Ʈ, ��, �� ��).
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/Abilities/Character Direction Marker")]
	public class CharacterDirectionMarker : CharacterAbility
	{
		/// the possible modes rotation can be based on
		public enum Modes { MovementDirection, AimDirection, None }
        
		[Header("Direction Marker")]
        /// ȸ���� ��ü
        [Tooltip("ȸ���� ��ü")]
		public Transform DirectionMarker;
        /// ��Ŀ �ɷ��� �����ϴ� �� ���Ǵ� ���� ID
        [Tooltip("��Ŀ �ɷ��� �����ϴ� �� ���Ǵ� ���� ID")]
		public int DirectionMarkerID;
        /// ������ �����ϱ� ���� ������ ���
        [Tooltip("������ �����ϱ� ���� ������ ���")]
		public Modes Mode = Modes.MovementDirection;
        
		[Header("Position")]
        /// ȸ�� �߽����� ������ ������
        [Tooltip("ȸ�� �߽����� ������ ������")]
		public Vector3 RotationCenterOffset = Vector3.zero;
        /// ������ �� �������� �����ϴ� ��
        [Tooltip("������ �� �������� �����ϴ� ��")]
		public Vector3 UpVector = Vector3.up;
        /// ������ �� �������� ������ ��
        [Tooltip("������ �� �������� ������ ��")]
		public Vector3 ForwardVector = Vector3.forward;
        /// �̰��� ����̶�� ��Ŀ�� X�࿡�� ȸ���� �� �����ϴ�.
        [Tooltip("�̰��� ����̶�� ��Ŀ�� X�࿡�� ȸ���� �� �����ϴ�.")]
		public bool PreventXRotation = false;
        /// �̰��� ����̶�� ��Ŀ�� Y�࿡�� ȸ���� �� �����ϴ�.
        [Tooltip("�̰��� ����̶�� ��Ŀ�� Y�࿡�� ȸ���� �� �����ϴ�.")]
		public bool PreventYRotation = false;
        /// �̰��� ����̶�� ��Ŀ�� Z�࿡�� ȸ���� �� �����ϴ�.
        [Tooltip("�̰��� ����̶�� ��Ŀ�� Z�࿡�� ȸ���� �� �����ϴ�.")]
		public bool PreventZRotation = false;

		[Header("Offset along magnitude")]
        /// ������ ũ�⿡ ���� ��ġ�� ���������� ����(���� ��� �� ������ �̵��ϸ� ��Ŀ�� ĳ���Ϳ��� �� �ָ� �̵��� �� ����)
        [Tooltip("������ ũ�⿡ ���� ��ġ�� ���������� ����(���� ��� �� ������ �̵��ϸ� ��Ŀ�� ĳ���Ϳ��� �� �ָ� �̵��� �� ����)")]
		public bool OffsetAlongMagnitude = false;
        /// �ӵ� ũ���� �ּ� ���
        [Tooltip("�ӵ� ũ���� �ּ� ���")]
		[MMCondition("OffsetAlongMagnitude", true)]
		public float MinimumVelocity = 0f;
        /// �ӵ� ũ���� �ִ� ���
        [Tooltip("�ӵ� ũ���� �ִ� ���")]
		[MMCondition("OffsetAlongMagnitude", true)]
		public float MaximumVelocity = 7f;
        /// ���� ���� �ӵ��� ���� �� ��Ŀ�� ��ġ�� �Ÿ�
        [Tooltip("���� ���� �ӵ��� ���� �� ��Ŀ�� ��ġ�� �Ÿ�")]
		[MMCondition("OffsetAlongMagnitude", true)]
		public float OffsetRemapMin = 0f;
        /// �ְ� �ӵ��� ���� �� ��Ŀ�� ��ġ�� �Ÿ�
        [Tooltip("�ְ� �ӵ��� ���� �� ��Ŀ�� ��ġ�� �Ÿ�")]
		[MMCondition("OffsetAlongMagnitude", true)]
		public float OffsetRemapMax = 1f;
        
		[Header("Auto Disable")]
        /// �̵� ũ�Ⱑ Ư�� �Ӱ谪 �̸��� �� ��Ŀ�� ��Ȱ��ȭ���� ����
        [Tooltip("�̵� ũ�Ⱑ Ư�� �Ӱ谪 �̸��� �� ��Ŀ�� ��Ȱ��ȭ���� ����")]
		public bool DisableBelowThreshold = false;
        /// ��Ŀ�� ��Ȱ��ȭ�ϴ� �Ӱ谪
        [Tooltip("��Ŀ�� ��Ȱ��ȭ�ϴ� �Ӱ谪")]
		[MMCondition("DisableBelowThreshold", true)]
		public float DisableThreshold = 0.1f;
        
		[Header("Interpolation")]
        /// ȸ���� �������� ����
        [Tooltip("ȸ���� �������� ����")]
		public bool Interpolate = false;
        /// ȸ���� �����ϴ� �ӵ�
        [Tooltip("ȸ���� �����ϴ� �ӵ�")]
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