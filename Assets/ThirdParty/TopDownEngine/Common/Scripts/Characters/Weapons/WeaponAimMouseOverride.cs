using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 구성 요소를 WeaponAim에 추가하면 마우스가 활성화되면 무기 조준 제어 모드를 마우스로 자동 전환하는 작업이 자동으로 처리됩니다.
    /// 그런 다음 게임패드 축 중 하나를 다시 터치하면 조준 제어가 해당 축으로 다시 전환됩니다.
    /// WeaponAim 제어 모드는 초기에 게임패드 제어 모드로 설정되어야 합니다.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Weapons/Weapon Aim Mouse Override")]
	public class WeaponAimMouseOverride : MonoBehaviour
	{
		[Header("Behavior")]
		[MMInformation("이 구성 요소를 WeaponAim에 추가하면 마우스가 활성화되면 무기 조준 제어 모드를 마우스로 자동 전환하는 작업이 자동으로 처리됩니다. " +
"그런 다음 게임패드 축 중 하나를 다시 터치하면 조준 제어가 다시 해당 축으로 전환됩니다." +
"WeaponAim 제어 모드는 처음에 게임패드 제어 모드로 설정되어야 합니다.", 
						MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]
		
		/// if this is true, mouse position will be evaluated, and if it differs from the one last frame, we'll switch to mouse control mode
		[Tooltip("이것이 사실이라면 마우스 위치가 평가되고, 마지막 프레임과 다르면 마우스 제어 모드로 전환됩니다.")]
		public bool CheckMouse = true;
		/// if this is true, the primary axis will be evaluated, and if it differs from the one last frame, we'll switch back to the initial control mode
		[Tooltip("이것이 사실이라면 기본 축이 평가되고, 마지막 프레임과 다르면 초기 제어 모드로 다시 전환됩니다.")]
		public bool CheckPrimaryAxis = true;
		/// if this is true, the secondary axis will be evaluated, and if it differs from the one last frame, we'll switch back to the initial control mode
		[Tooltip("이것이 사실이라면 보조 축이 평가되고 마지막 프레임과 다를 경우 초기 제어 모드로 다시 전환됩니다.")]
		public bool CheckSecondaryAxis = true;
		
		protected WeaponAim _weaponAim;
		protected Vector2 _primaryAxisInput;
		protected Vector2 _primaryAxisInputLastFrame;
		protected Vector2 _secondaryAxisInput;
		protected Vector2 _secondaryAxisInputLastFrame;
		protected Vector2 _mouseInput;
		protected Vector2 _mouseInputLastFrame;
		protected WeaponAim.AimControls _initialAimControl;

		/// <summary>
		/// On Awake we store our WeaponAim component and grab our initial aim control mode 
		/// </summary>
		protected virtual void Awake()
		{
			_weaponAim = this.gameObject.GetComponent<WeaponAim>();
			GetInitialAimControl();
		}

		/// <summary>
		/// Sets the current aim control mode as the initial one, that the component will switch back to when going back from mouse mode
		/// </summary>
		public virtual void GetInitialAimControl()
		{
			_initialAimControl = _weaponAim.AimControl;
			if (_weaponAim.AimControl == WeaponAim.AimControls.Mouse)
			{
				Debug.LogWarning(this.gameObject + " : this component requires that you set its associated WeaponAim to a control mode other than Mouse.");
			}
		}

		/// <summary>
		/// On update, checks mouse and axis, and stores last frame's data
		/// </summary>
		protected virtual void Update()
		{
			CheckMouseInput();
			CheckAxisInput();
			StoreLastFrameData();
		}

		/// <summary>
		/// We store our current input data to be able to compare against it next frame
		/// </summary>
		protected virtual void StoreLastFrameData()
		{
			_mouseInputLastFrame = _mouseInput;
			_primaryAxisInputLastFrame = _primaryAxisInput;
			_secondaryAxisInputLastFrame = _secondaryAxisInput;
		}

		/// <summary>
		/// Checks if mouse input has changed, switches to mouse control if that's the case
		/// </summary>
		protected virtual void CheckMouseInput()
		{
			if (!CheckMouse)
			{
				return;
			}

			_mouseInput = _weaponAim.TargetWeapon.Owner.LinkedInputManager.MousePosition;
			if (_mouseInput != _mouseInputLastFrame)
			{
				SwitchToMouse();
			}
		}

		/// <summary>
		/// Checks if axis input has changed, switches back to initial control mode if that's the case
		/// </summary>
		protected virtual void CheckAxisInput()
		{
			if (CheckPrimaryAxis)
			{
				_primaryAxisInput = _weaponAim.TargetWeapon.Owner.LinkedInputManager.PrimaryMovement;
				if (_primaryAxisInput != _primaryAxisInputLastFrame)
				{
					SwitchToInitialControlMode();
				}
			}

			if (CheckSecondaryAxis)
			{
				_secondaryAxisInput = _weaponAim.TargetWeapon.Owner.LinkedInputManager.SecondaryMovement;
				if (_secondaryAxisInput != _secondaryAxisInputLastFrame)
				{
					SwitchToInitialControlMode();
				}
			}
		}
		
		/// <summary>
		/// Changes aim control mode to mouse
		/// </summary>
		public virtual void SwitchToMouse()
		{
			_weaponAim.AimControl = WeaponAim.AimControls.Mouse;
		}

		/// <summary>
		/// Changes aim control mouse to the initial mode
		/// </summary>
		public virtual void SwitchToInitialControlMode()
		{
			_weaponAim.AimControl = _initialAimControl;
		}
	}	
}

