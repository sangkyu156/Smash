using System;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.InventoryEngine
{
    /// <summary>
    /// 이 클래스는 플레이어 캐릭터가 그 위에 있을 때 대상 인벤토리를 표시하고 캐릭터가 영역을 나갈 때 닫히는 저장 상자의 간단한 예를 보여줍니다.
    /// </summary>
    public class InventoryDemoStorageBox : MonoBehaviour
	{
		public CanvasGroup TargetCanvasGroup;

		public virtual void OpenStorage(string playerID)
		{
			TargetCanvasGroup.alpha = 1;
			Debug.Log("인벤 열림");
		}

		public virtual void CloseStorage(string playerID)
		{
			TargetCanvasGroup.alpha = 0;
            Debug.Log("인벤 닫힘");
        }
		
		public virtual void OnTriggerEnter(Collider collider)
		{
			OnEnter(collider.gameObject);
		}

		public virtual void OnTriggerEnter2D (Collider2D collider) 
		{
			OnEnter(collider.gameObject);
		}

		protected virtual void OnEnter(GameObject collider)
		{
			// if what's colliding with the picker ain't a Player, we do nothing and exit
			if (!collider.CompareTag("Player"))
			{
				return;
			}

			string playerID = "Player1";
			InventoryCharacterIdentifier identifier = collider.GetComponent<InventoryCharacterIdentifier>();
			if (identifier != null)
			{
				playerID = identifier.PlayerID;
			}

			OpenStorage(playerID);
		}

		public void OnTriggerExit(Collider collider)
		{
			OnExit(collider.gameObject);
		}

		public virtual void OnTriggerExit2D (Collider2D collider) 
		{
			OnExit(collider.gameObject);
		}
		
		protected virtual void OnExit(GameObject collider)
		{
			// if what's colliding with the picker ain't a Player, we do nothing and exit
			if (!collider.CompareTag("Player"))
			{
				return;
			}

			string playerID = "Player1";
			InventoryCharacterIdentifier identifier = collider.GetComponent<InventoryCharacterIdentifier>();
			if (identifier != null)
			{
				playerID = identifier.PlayerID;
			}

			CloseStorage(playerID);
		}
	}	
}