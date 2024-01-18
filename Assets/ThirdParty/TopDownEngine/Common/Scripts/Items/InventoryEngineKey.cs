using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System;
using MoreMountains.InventoryEngine;

namespace MoreMountains.TopDownEngine
{	
	[CreateAssetMenu(fileName = "InventoryEngineKey", menuName = "MoreMountains/TopDownEngine/InventoryEngineKey", order = 1)]
	[Serializable]
    /// <summary>
    /// 선택 가능한 핵심 아이템
    /// </summary>
    public class InventoryEngineKey : InventoryItem 
	{
        /// <summary>
        /// 아이템이 사용되면 캐릭터의 건강 구성요소를 확보하려고 시도하고, 존재하는 경우 건강 보너스 양을 추가합니다.
        /// </summary>
        public override bool Use(string playerID)
		{
			base.Use(playerID);
			return true;
		}
	}
}