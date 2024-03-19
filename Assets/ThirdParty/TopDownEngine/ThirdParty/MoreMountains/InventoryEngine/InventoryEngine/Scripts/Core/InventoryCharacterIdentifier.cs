using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.InventoryEngine
{
    /// <summary>
    /// 멀티플레이어 컨텍스트에서 작업할 때 아이템을 선택할 수 있는 캐릭터에 이 클래스를 추가하면 ItemPicker가 자동으로 아이템을 올바른 PlayerID로 보냅니다.
    /// </summary>
    public class InventoryCharacterIdentifier : MonoBehaviour
	{
		/// the unique ID of the player
		public string PlayerID = "Player1";
	}    
}