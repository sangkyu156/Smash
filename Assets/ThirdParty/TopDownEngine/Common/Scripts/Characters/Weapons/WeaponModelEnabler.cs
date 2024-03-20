using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;
using System;

namespace MoreMountains.TopDownEngine
{
	[Serializable]
	public struct WeaponModelBindings
	{
		public GameObject WeaponModel;
		public int WeaponAnimationID;
	}

    /// <summary>
    /// 이 클래스는 무기가 실제 무기 객체와 분리된 경우 무기의 시각적 표현을 활성화/비활성화하는 일을 담당합니다.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Weapons/Weapon Model Enabler")]
	public class WeaponModelEnabler : TopDownMonoBehaviour
	{
		/// a list of model bindings. A binding is made of a gameobject, already present on the character, that will act as the visual representation of the weapon, and a name, that has to match the WeaponAnimationID of the actual Weapon
		[Tooltip("모델 바인딩 목록. 바인딩은 무기의 시각적 표현 역할을 할 캐릭터에 이미 존재하는 게임 객체와 실제 무기의 WeaponAnimationID와 일치해야 하는 이름으로 구성됩니다.")]
		public WeaponModelBindings[] Bindings;

		public CharacterHandleWeapon HandleWeapon;

		/// <summary>
		/// On Awake we grab our CharacterHandleWeapon component
		/// </summary>
		protected virtual void Awake()
		{
			if (HandleWeapon == null)
			{
				HandleWeapon = this.gameObject.GetComponent<CharacterHandleWeapon>();	
			}
		}

		/// <summary>
		/// On Update, we enable/disable bound gameobjects based on their name
		/// </summary>
		protected virtual void Update()
		{
			if (Bindings.Length <= 0)
			{
				return;
			}

			if (HandleWeapon == null)
			{
				return;
			}

			if (HandleWeapon.CurrentWeapon == null)
			{
				foreach (WeaponModelBindings binding in Bindings)
				{
					binding.WeaponModel.SetActive(false);
				}
				return;
			}

			foreach (WeaponModelBindings binding in Bindings)
			{
				if (binding.WeaponAnimationID == HandleWeapon.CurrentWeapon.WeaponAnimationID)
				{
					binding.WeaponModel.SetActive(true);
				}
				else
				{
					binding.WeaponModel.SetActive(false);
				}
			}
		}			
	}
}