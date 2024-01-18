using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System;

namespace MoreMountains.InventoryEngine
{	
	[CreateAssetMenu(fileName = "BombItem", menuName = "MoreMountains/InventoryEngine/BombItem", order = 2)]
	[Serializable]
    /// <summary>
    /// 폭탄 아이템의 데모 클래스
    /// </summary>
    public class BombItem : InventoryItem 
	{
        /// <summary>
        /// 폭탄이 사용되면 작동했음을 보여주기 위해 디버그 메시지를 표시합니다. 실제 게임에서는 아마도 폭탄이 생성될 것입니다.
        /// </summary>
        public override bool Use(string playerID)
		{
			base.Use(playerID);
			Debug.LogFormat("bomb explosion");
			return true;
		}
	}
}