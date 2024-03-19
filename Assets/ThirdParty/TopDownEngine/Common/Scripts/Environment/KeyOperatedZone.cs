using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using UnityEngine.Events;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 구성 요소를 충돌기 2D에 추가하면 다음과 같은 경우 작업을 수행하도록 할 수 있습니다.
    /// 지정된 키를 갖춘 캐릭터가 입력됩니다.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Environment/Key Operated Zone")]
	public class KeyOperatedZone : ButtonActivated 
	{
		[Header("Key")]

		/// whether this zone actually requires a key
		[Tooltip("이 영역에 실제로 키가 필요한지 여부")]
		public bool RequiresKey = true;
		/// the key ID, that will be checked against the existence (or not) of a key of the same name in the player's inventory
		[Tooltip("플레이어의 인벤토리에 같은 이름의 키가 있는지 여부를 확인하는 키 ID입니다")]
		public string KeyID;
		/// the method that should be triggered when the key is used
		[Tooltip("키가 사용될 때 트리거되어야 하는 메소드")]
		public UnityEvent KeyAction;
        
		protected GameObject _collidingObject;
		protected List<int> _keyList;

		/// <summary>
		/// On Start we initialize our object
		/// </summary>
		protected virtual void Start()
		{
			_keyList = new List<int> ();
		}

		/// <summary>
		/// On enter we store our colliding object
		/// </summary>
		/// <param name="collider">Something colliding with the water.</param>
		protected override void OnTriggerEnter2D(Collider2D collider)
		{
			_collidingObject = collider.gameObject;
			base.OnTriggerEnter2D (collider);
		}

		protected override void OnTriggerEnter(Collider collider)
		{
			_collidingObject = collider.gameObject;
			base.OnTriggerEnter(collider);
		}

		/// <summary>
		/// When the button is pressed, we check if we have a key in our inventory
		/// </summary>
		public override void TriggerButtonAction()
		{
			if (!CheckNumberOfUses())
			{
				PromptError();
				return;
			}

			if (_collidingObject == null) { return; }

			if (RequiresKey)
			{
				CharacterInventory characterInventory = _collidingObject.MMGetComponentNoAlloc<Character>()?.FindAbility<CharacterInventory> ();
				if (characterInventory == null)
				{
					PromptError();
					return;
				}	

				_keyList.Clear ();
				_keyList = characterInventory.MainInventory.InventoryContains (KeyID);
				if (_keyList.Count == 0)
				{
					PromptError();
					return;
				}
				else
				{
					base.TriggerButtonAction ();
					characterInventory.MainInventory.UseItem(KeyID);
				}
			}

			TriggerKeyAction ();
			ActivateZone ();
		}

		/// <summary>
		/// Calls the method associated to the key action
		/// </summary>
		protected virtual void TriggerKeyAction()
		{
			if (KeyAction != null)
			{
				KeyAction.Invoke ();
			}
		}
	}
}