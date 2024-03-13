using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;

namespace MoreMountains.InventoryEngine
{
    /// <summary>
    /// 이 구성 요소를 개체에 추가하면 선택하여 인벤토리에 추가할 수 있습니다.
    /// </summary>
    public class ItemPicker : MonoBehaviour 
	{
		[Header("Item to pick")]
        /// 골라야 할 아이템
        [MMInformation("이 구성 요소를 Trigger box Collider 2D에 추가하면 선택 가능하게 되고 지정된 항목이 대상 인벤토리에 추가됩니다. 이전에 생성된 항목을 아래 슬롯으로 드래그하기만 하면 됩니다. 항목을 만드는 방법에 대한 자세한 내용은 설명서를 참조하세요. 여기서는 개체를 선택할 때 해당 항목 중 몇 개를 선택해야 하는지 지정할 수도 있습니다.", MMInformationAttribute.InformationType.Info,false)]
		public InventoryItem Item;
		
		[Header("Pick Quantity")]
        /// 피킹 시 재고에 추가되어야 하는 해당 품목의 초기 수량
        [Tooltip("피킹 시 재고에 추가되어야 하는 해당 품목의 초기 수량")]
		public int Quantity = 1;
        /// 피킹 시 재고에 추가되어야 하는 해당 품목의 현재 수량
        [MMReadOnly]
		[Tooltip("피킹 시 재고에 추가되어야 하는 해당 품목의 현재 수량")]
		public int RemainingQuantity = 1;
		
		[Header("Conditions")]
        /// true로 설정하면 인벤토리가 가득 차 있어도 캐릭터가 이 아이템을 선택할 수 있습니다.
        [Tooltip("true로 설정하면 인벤토리가 가득 차 있어도 캐릭터가 이 아이템을 선택할 수 있습니다.")]
		public bool PickableIfInventoryIsFull = false;
        /// 이것을 true로 설정하면 객체를 선택할 때 객체가 비활성화됩니다.
        [Tooltip("이것을 true로 설정하면 객체를 선택할 때 객체가 비활성화됩니다.")]
		public bool DisableObjectWhenDepleted = false;
        /// 이것이 사실이라면 이 객체는 플레이어 태그가 있는 충돌체에 의해서만 선택될 수 있습니다.
        [Tooltip("이것이 사실이라면 이 객체는 플레이어 태그가 있는 충돌체에 의해서만 선택될 수 있습니다.")]
		public bool RequirePlayerTag = true;

        protected int _pickedQuantity = 0;
		protected Inventory _targetInventory;
        protected Inventory _quickSlots;

        /// <summary>
        /// 시작 시 항목 선택기를 초기화합니다.
        /// </summary>
        protected virtual void Start()
		{
			Initialization ();
        }

        /// <summary>
        /// Init에서는 목표 인벤토리를 찾습니다.
        /// </summary>
        protected virtual void Initialization()
		{
			FindTargetInventory (Item.TargetInventoryName);
			FindTargetQuickSlots("QuickSlots");

            ResetQuantity();
		}

        /// <summary>
        /// 남은 수량을 초기 수량으로 재설정합니다.
        /// </summary>
        public virtual void ResetQuantity()
		{
			RemainingQuantity = Quantity;
		}

        /// <summary>
        /// 무언가가 피커와 충돌할 때 트리거됩니다.
        /// </summary>
        /// <param name="collider">Other.</param>
        public virtual void OnTriggerEnter(Collider collider)
		{
            // 선택기와 충돌하는 것이 CharacterBehavior가 아닌 경우 아무것도 하지 않고 종료합니다.
            if (RequirePlayerTag && (!collider.CompareTag("Player")))
			{
				return;
			}

			string playerID = "Player1";
			InventoryCharacterIdentifier identifier = collider.GetComponent<InventoryCharacterIdentifier>();
			if (identifier != null)
			{
				playerID = identifier.PlayerID;
			}

			//먹은아이템이 퀵슬롯에 있고 & 최대치가 아닌 상태로 등록되어있으면 몇개로 등록되어 있는지 확인하고 그 숫자를 A로 저장, 퀵슬롯 인덱스를 B로 저장하고 
			//인벤토리에서 같은 아이템ID에 A만큼 저장되어있는 인덱트를 찾아서 거기에다가 먹은 아이템 추가, 퀵슬롯인덱스B에 먹은아이템 추가 하면 끝

			//먹은 아이템이 퀵슬롯에 최대치가 아닌 상태로 등록되어 있는지 아닌지 확인, true이면 퀵슬롯에 먹은 아이템이 최대치가 아닌상태로 등록되어있다는거
			if (_quickSlots.QuickSlotCheckForItem(Item.ItemID, Item.MaximumStack))
			{
                DetermineMaxQuantity(); //최대수량 체크
                                        //인벤토리에 퀵슬롯에서 발견한 먹은아이템과 같은아이템,같은개수있는곳에 먹은 아이템 추가
                Debug.Log("1");
                _targetInventory.FindSameItemAndAddItem(Item.ItemID, _quickSlots.GetQuickSlotTargetQuantity(), Item);
                Debug.Log("2");
                //퀵슬롯에 먹은아이템과 같은아이템이면서 최대치로 등록안된 슬롯에 먹은 아이템 추가
                _quickSlots.AddItemAt(Item, 1, _quickSlots.GetQuickSlotTargetIndex());
                Debug.Log("3");
                //아이템 비활성화
                DisableObjectIfNeeded();
                Debug.Log("4");
            }
			else
                Pick(Item.TargetInventoryName, playerID);
        }

        /// <summary>
        /// 무언가가 피커와 충돌할 때 트리거됩니다.
        /// </summary>
        /// <param name="collider">Other.</param>
        public virtual void OnTriggerEnter2D (Collider2D collider) 
		{
            // 선택기와 충돌하는 것이 CharacterBehavior가 아닌 경우 아무것도 하지 않고 종료합니다.
            if (RequirePlayerTag && (!collider.CompareTag("Player")))
			{
				return;
			}

			string playerID = "Player1";
			InventoryCharacterIdentifier identifier = collider.GetComponent<InventoryCharacterIdentifier>();
			if (identifier != null)
			{
				playerID = identifier.PlayerID;
			}

			Pick(Item.TargetInventoryName, playerID);
		}

        /// <summary>
        /// 이 항목을 선택하여 대상 인벤토리에 추가합니다.
        /// </summary>
        public virtual void Pick()
		{
			Pick(Item.TargetInventoryName);
		}

        /// <summary>
        /// 이 아이템을 선택하고 매개변수로 지정된 대상 인벤토리에 추가합니다.
        /// </summary>
        /// <param name="targetInventoryName">Target inventory name.</param>
        public virtual void Pick(string targetInventoryName, string playerID = "Player1")
		{
			FindTargetInventory(targetInventoryName, playerID);// 넣을 인벤 찾기
			if (_targetInventory == null)
			{
				return;
			}

			if (!Pickable()) //인벤에 넣을수 있는지 확인
			{
				PickFail ();
				return;
			}

			DetermineMaxQuantity (); //최대수량 체크
			if (!Application.isPlaying)
			{
				if (!Item.ForceSlotIndex)
				{
					_targetInventory.AddItem(Item, 1);	
				}
				else
				{
					_targetInventory.AddItemAt(Item, 1, Item.TargetIndex);
				}
			}
			else
			{
				MMInventoryEvent.Trigger(MMInventoryEventType.Pick, null, Item.TargetInventoryName, Item, _pickedQuantity, 0, playerID);
			}
			if (Item.Pick(playerID))
			{
				RemainingQuantity = RemainingQuantity - _pickedQuantity;
				PickSuccess();
				DisableObjectIfNeeded();
			}
		}

        /// <summary>
        /// 개체가 성공적으로 선택되면 어떤 일이 발생하는지 설명합니다.
        /// </summary>
        protected virtual void PickSuccess()
		{
			
		}

        /// <summary>
        /// 개체를 선택하지 못한 경우(보통 인벤토리가 가득 찼을 때) 어떻게 되는지 설명합니다.
        /// </summary>
        protected virtual void PickFail()
		{

		}

        /// <summary>
        /// 필요한 경우 개체를 비활성화합니다.
        /// </summary>
        protected virtual void DisableObjectIfNeeded()
		{
			Debug.Log($"DisableObjectWhenDepleted = {DisableObjectWhenDepleted}, RemainingQuantity = {RemainingQuantity}");
            // 게임오브젝트를 비활성화합니다
            if (DisableObjectWhenDepleted/* && RemainingQuantity <= 0*/)
			{
				gameObject.SetActive(false);	
			}
		}

        /// <summary>
        /// 이 항목에서 선택할 수 있는 항목의 최대 수량을 결정합니다.
        /// </summary>
        protected virtual void DetermineMaxQuantity()
		{
			_pickedQuantity = _targetInventory.NumberOfStackableSlots (Item.ItemID, Item.MaximumStack);
			if (RemainingQuantity < _pickedQuantity)
			{
				_pickedQuantity = RemainingQuantity;
			}
		}

        /// <summary>
        /// 이 항목을 선택할 수 있으면 true를 반환하고, 그렇지 않으면 false를 반환합니다.
        /// </summary>
        public virtual bool Pickable()
		{
			if (!PickableIfInventoryIsFull && _targetInventory.NumberOfFreeSlots == 0)
			{
                // 우리는 그것을 보관할 수 있는 장소가 없는지 확인합니다
                int spaceAvailable = 0;
				List<int> list = _targetInventory.InventoryContains(Item.ItemID);
				if (list.Count > 0)
				{
					foreach (int index in list)
					{
						spaceAvailable += (Item.MaximumStack - _targetInventory.Content[index].Quantity);
					}
				}

				if (Item.Quantity <= spaceAvailable)
				{
					return true;
				}
				else
				{
					return false;	
				}
			}

			return true;
		}

        /// <summary>
        /// 이름을 기준으로 대상 인벤토리를 찾습니다.
        /// </summary>
        /// <param name="targetInventoryName">Target inventory name.</param>
        public virtual void FindTargetInventory(string targetInventoryName, string playerID = "Player1")
		{
			_targetInventory = null;
			if (targetInventoryName == null)
			{
				return;
			}
			_targetInventory = Inventory.FindInventory(targetInventoryName, playerID);
		}

        //FindQuickSlots
        /// <summary>
        /// 이름을 기준으로 퀵슬롯을 찾습니다.
        /// </summary>
        /// <param name="targetInventoryName">Target inventory name.</param>
        public virtual void FindTargetQuickSlots(string targetQuickSlotsName, string playerID = "PlayerQuickSlots")
        {
            _quickSlots = null;
            if (targetQuickSlotsName == null)
            {
                return;
            }
            _quickSlots = Inventory.FindQuickSlots(targetQuickSlotsName, playerID);
        }
    }
}