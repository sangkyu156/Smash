using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System;
using MoreMountains.InventoryEngine;

namespace MoreMountains.TopDownEngine
{	
	[CreateAssetMenu(fileName = "InventoryWeapon", menuName = "MoreMountains/TopDownEngine/InventoryWeapon", order = 2)]
	[Serializable]
    /// <summary>
    /// TopDown 엔진의 무기 아이템
    /// </summary>
    public class InventoryWeapon : InventoryItem 
	{
		/// the possible auto equip modes
		public enum AutoEquipModes { NoAutoEquip, AutoEquip, AutoEquipIfEmptyHanded }
        
		[Header("Weapon")]
		[MMInformation("여기서 해당 아이템을 선택할 때 장착하고 싶은 무기를 바인딩해야 합니다.", MMInformationAttribute.InformationType.Info,false)]
		/// the weapon to equip
		[Tooltip("장착할 무기")]
		public Weapon EquippableWeapon;
		/// how to equip this weapon when picked : not equip it, automatically equip it, or only equip it if no weapon is currently equipped
		[Tooltip("이 무기를 선택했을 때 장착하는 방법: 장착하지 않음, 자동으로 장착, 현재 무기가 장착되어 있지 않은 경우에만 장착")]
		public AutoEquipModes AutoEquipMode = AutoEquipModes.NoAutoEquip;
		/// the ID of the CharacterHandleWeapon you want this weapon to be equipped to
		[Tooltip("이 무기에 장착하려는 CharacterHandleWeapon의 ID")]
		public int HandleWeaponID = 1;

		/// <summary>
		/// When we grab the weapon, we equip it
		/// </summary>
		public override bool Equip(string playerID)
		{
			EquipWeapon (EquippableWeapon, playerID);
			return true;
		}

		/// <summary>
		/// When dropping or unequipping a weapon, we remove it
		/// </summary>
		public override bool UnEquip(string playerID)
		{
			// if this is a currently equipped weapon, we unequip it
			if (this.TargetEquipmentInventory(playerID) == null)
			{
				return false;
			}

			if (this.TargetEquipmentInventory(playerID).InventoryContains(this.ItemID).Count > 0)
			{
				EquipWeapon(null, playerID);
			}

			return true;
		}

		/// <summary>
		/// Grabs the CharacterHandleWeapon component and sets the weapon
		/// </summary>
		/// <param name="newWeapon">New weapon.</param>
		protected virtual void EquipWeapon(Weapon newWeapon, string playerID)
		{
			if (EquippableWeapon == null)
			{
				return;
			}
			if (TargetInventory(playerID).Owner == null)
			{
				return;
			}

			Character character = TargetInventory(playerID).Owner.GetComponentInParent<Character>();

			if (character == null)
			{
				return;
			}

			// we equip the weapon to the chosen CharacterHandleWeapon
			CharacterHandleWeapon targetHandleWeapon = null;
			CharacterHandleWeapon[] handleWeapons = character.GetComponentsInChildren<CharacterHandleWeapon>();
			foreach (CharacterHandleWeapon handleWeapon in handleWeapons)
			{
				if (handleWeapon.HandleWeaponID == HandleWeaponID)
				{
					targetHandleWeapon = handleWeapon;
				}
			}
			
			if (targetHandleWeapon != null)
			{
				targetHandleWeapon.ChangeWeapon(newWeapon, this.ItemID);
			}
		}
	}
}