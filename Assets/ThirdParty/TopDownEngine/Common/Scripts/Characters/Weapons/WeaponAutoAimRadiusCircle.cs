using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 클래스는 자동 조준 무기가 있는 경우 해당 무기의 반경과 일치하는 원을 자동으로 그립니다.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
	[AddComponentMenu("TopDown Engine/Weapons/Weapon Auto Aim Radius Circle")]
	public class WeaponAutoAimRadiusCircle : MMLineRendererCircle
	{
		[Header("Weapon Radius")]
		public CharacterHandleWeapon TargetHandleWeaponAbility;

		/// <summary>
		/// On initialization, hooks itself to weapon changes
		/// </summary>
		protected override void Initialization()
		{
			base.Initialization();
			_line = gameObject.GetComponent<LineRenderer>();
			_line.enabled = false;
            
			if (TargetHandleWeaponAbility != null)
			{
				TargetHandleWeaponAbility.OnWeaponChange += OnWeaponChange;
			}
		}
        
		/// <summary>
		/// When the weapon changes, if it has auto aim, draws a circle around it
		/// </summary>
		void OnWeaponChange()
		{
			if (TargetHandleWeaponAbility.CurrentWeapon == null)
			{
				return;
			}
			WeaponAutoAim autoAim = TargetHandleWeaponAbility.CurrentWeapon.GetComponent<WeaponAutoAim>();
			_line.enabled = (autoAim != null);
            
			if (autoAim != null)
			{
				HorizontalRadius = autoAim.ScanRadius;
				VerticalRadius = autoAim.ScanRadius;
			}
			DrawCircle();
		}

		/// <summary>
		/// On disables we unhook from our delegate
		/// </summary>
		void OnDisable()
		{
			if (TargetHandleWeaponAbility != null)
			{
				TargetHandleWeaponAbility.OnWeaponChange -= OnWeaponChange;
			}
		}
	}
}