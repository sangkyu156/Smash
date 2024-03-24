using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System;
using MoreMountains.InventoryEngine;

namespace MoreMountains.TopDownEngine
{	
	[CreateAssetMenu(fileName = "InventoryEngineHealth", menuName = "MoreMountains/TopDownEngine/InventoryEngineHealth", order = 1)]
	[Serializable]
    /// <summary>
    /// 선택 가능한 건강 아이템
    /// </summary>
    public class InventoryEngineHealth : InventoryItem 
	{
		[Header("Health")]
		[MMInformation("여기에서 이 아이템을 사용할 때 얻는 체력의 양을 지정해야 합니다.", MMInformationAttribute.InformationType.Info,false)]
		/// the amount of health to add to the player when the item is used
		[Tooltip("아이템이 사용될 때 플레이어에게 추가되는 체력의 양")]
		public float HealthBonus;

		/// <summary>
		/// When the item is used, we try to grab our character's Health component, and if it exists, we add our health bonus amount of health
		/// </summary>
		public override bool Use(string playerID)
		{
			base.Use(playerID);

			if (TargetInventory(playerID).Owner == null)
			{
				return false;
			}

			Health characterHealth = TargetInventory(playerID).Owner.GetComponent<Health>();
			if (characterHealth != null)
			{
				characterHealth.ReceiveHealth(HealthBonus,TargetInventory(playerID).gameObject);
				return true;
			}
			else
			{
				return false;
			}
		}

	}
}