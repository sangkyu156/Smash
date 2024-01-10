using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using static Codice.Client.BaseCommands.Import.Commit;
using TMPro;

namespace MoreMountains.InventoryEngine
{
    /// <summary>
    /// 이 클래스는 인벤토리의 아이템 표시를 처리하고 아이템으로 할 수 있는 다양한 작업(장착, 사용 등)을 실행합니다.
    /// </summary>
    public class InventorySlot : Button
	{
        /// 아이템이 이동되는 동안 슬롯의 배경으로 사용되는 스프라이트
        public Sprite MovedSprite;
        /// 이 슬롯이 속한 인벤토리 표시
        public InventoryDisplay ParentInventoryDisplay;
        /// 슬롯의 인덱스(인벤토리 배열에서의 위치)
        public int Index;
        /// 이 슬롯이 현재 활성화되어 있고 상호 작용할 수 있는지 여부
        public bool SlotEnabled=true;
        
        public Image TargetImage;
		public CanvasGroup TargetCanvasGroup;
		public RectTransform TargetRectTransform;
		public RectTransform IconRectTransform;
		public Image IconImage;
		public TextMeshProUGUI QuantityText;
        public Image CountImage;
				
		protected const float _disabledAlpha = 0.5f;
		protected const float _enabledAlpha = 1.0f;

		protected override void Awake()
		{
			base.Awake();
			TargetImage = this.gameObject.GetComponent<Image>();
			TargetCanvasGroup = this.gameObject.GetComponent<CanvasGroup>();
			TargetRectTransform = this.gameObject.GetComponent<RectTransform>();
            //CountImage = this.gameObject.GetComponentInChildren<Image>();
        }

        /// <summary>
        /// 시작 시 해당 슬롯의 클릭 이벤트를 듣기 시작합니다.
        /// </summary>
        protected override void Start()
		{
			base.Start();
			this.onClick.AddListener(SlotClicked);
		}

        /// <summary>
        /// 이 슬롯에 항목이 있으면 내부에 해당 아이콘을 그립니다.
        /// </summary>
        /// <param name="item">Item.</param>
        /// <param name="index">Index.</param>
        public virtual void DrawIcon(InventoryItem item, int index)
		{
			if (ParentInventoryDisplay != null)
			{				
				if (!InventoryItem.IsNull(item))
				{
					SetIcon(item.Icon);
					SetQuantity(item.Quantity);
                }
				else
				{
					DisableIconAndQuantity();
				}
			}
		}

		public virtual void SetIcon(Sprite newSprite)
		{
			IconImage.gameObject.SetActive(true);
			IconImage.sprite = newSprite;
		}

		public virtual void SetQuantity(int quantity)
		{
			if (quantity > 1)
			{
				QuantityText.gameObject.SetActive(true);
				QuantityText.text = quantity.ToString();
                CountImage.enabled = true;
            }
			else
			{
				QuantityText.gameObject.SetActive(false);
                CountImage.enabled = false;
            }
		}

		public virtual void DisableIconAndQuantity()
		{
			IconImage.gameObject.SetActive(false);
		}

        /// <summary>
        /// 해당 슬롯이 선택되면(마우스 오버 또는 터치를 통해) 다른 클래스가 작동할 이벤트가 트리거됩니다.
        /// </summary>
        /// <param name="eventData">Event data.</param>
        public override void OnSelect(BaseEventData eventData)
		{
			base.OnSelect(eventData);
			if (ParentInventoryDisplay != null)
			{
				InventoryItem item = ParentInventoryDisplay.TargetInventory.Content[Index];
				MMInventoryEvent.Trigger(MMInventoryEventType.Select, this, ParentInventoryDisplay.TargetInventoryName, item, 0, Index, ParentInventoryDisplay.PlayerID);
			}
		}

        /// <summary>
        /// 해당 슬롯을 클릭하면 다른 클래스가 조치를 취할 수 있는 이벤트가 트리거됩니다.
        /// </summary>
        public virtual void SlotClicked () 
		{
			if (ParentInventoryDisplay != null)
			{
				InventoryItem item = ParentInventoryDisplay.TargetInventory.Content[Index];
				if (ParentInventoryDisplay.InEquipSelection)
				{
					MMInventoryEvent.Trigger(MMInventoryEventType.EquipRequest, this, ParentInventoryDisplay.TargetInventoryName, ParentInventoryDisplay.TargetInventory.Content[Index], 0, Index, ParentInventoryDisplay.PlayerID);
				}
				MMInventoryEvent.Trigger(MMInventoryEventType.Click, this, ParentInventoryDisplay.TargetInventoryName, item, 0, Index, ParentInventoryDisplay.PlayerID);
				// if we're currently moving an object
				if (InventoryDisplay.CurrentlyBeingMovedItemIndex != -1)
				{
					Move();
				}
			}
		}

        /// <summary>
        /// 이동을 위해 이 슬롯의 항목을 선택하거나 현재 선택된 항목을 해당 슬롯으로 이동합니다. 또한 가능하면 두 개체를 모두 교체합니다.
        /// </summary>
        public virtual void Move()
		{
			if (!SlotEnabled) { return; }

            // 아직 객체를 움직이고 있지 않은 경우
            if (InventoryDisplay.CurrentlyBeingMovedItemIndex == -1)
			{
                // 우리가 있는 슬롯이 비어 있으면 아무것도 하지 않습니다.
                if (InventoryItem.IsNull(ParentInventoryDisplay.TargetInventory.Content[Index]))
				{
					MMInventoryEvent.Trigger(MMInventoryEventType.Error, this, ParentInventoryDisplay.TargetInventoryName, null, 0, Index, ParentInventoryDisplay.PlayerID);
					return;
				}
				if (ParentInventoryDisplay.TargetInventory.Content[Index].CanMoveObject)
				{
                    // 배경 이미지를 변경합니다
                    TargetImage.sprite = ParentInventoryDisplay.MovedSlotImage;
					InventoryDisplay.CurrentlyBeingMovedFromInventoryDisplay = ParentInventoryDisplay;
					InventoryDisplay.CurrentlyBeingMovedItemIndex = Index;
				}
			}
            // 물체를 움직이고 있다면
            else
            {
				bool moveSuccessful = false;
                // 객체를 새 슬롯으로 이동합니다.
                if (ParentInventoryDisplay == InventoryDisplay.CurrentlyBeingMovedFromInventoryDisplay)
				{
					if (!ParentInventoryDisplay.TargetInventory.MoveItem(InventoryDisplay.CurrentlyBeingMovedItemIndex, Index))
					{
                        // 이동할 수 없는 경우(예: 비어 있지 않은 대상 슬롯) 소리를 재생합니다.
                        MMInventoryEvent.Trigger(MMInventoryEventType.Error, this, ParentInventoryDisplay.TargetInventoryName, null, 0, Index, ParentInventoryDisplay.PlayerID);
						moveSuccessful = false;
					}
					else
					{
						moveSuccessful = true;
					}
				}
				else
				{
					if (!ParentInventoryDisplay.AllowMovingObjectsToThisInventory)
					{
						moveSuccessful = false;
					}
					else
					{
						if (!InventoryDisplay.CurrentlyBeingMovedFromInventoryDisplay.TargetInventory.MoveItemToInventory(InventoryDisplay.CurrentlyBeingMovedItemIndex, ParentInventoryDisplay.TargetInventory, Index))
						{
                            // 이동할 수 없는 경우(예: 비어 있지 않은 대상 슬롯) 소리를 재생합니다.
                            MMInventoryEvent.Trigger(MMInventoryEventType.Error, this, ParentInventoryDisplay.TargetInventoryName, null, 0, Index, ParentInventoryDisplay.PlayerID);
							moveSuccessful = false;
						}
						else
						{
							moveSuccessful = true;
						}
					}
				}

				if (moveSuccessful)
				{
                    // 이동할 수 있으면 현재BeingMoved 포인터를 재설정합니다.
                    InventoryDisplay.CurrentlyBeingMovedItemIndex = -1;
					InventoryDisplay.CurrentlyBeingMovedFromInventoryDisplay = null;
					MMInventoryEvent.Trigger(MMInventoryEventType.Move, this, ParentInventoryDisplay.TargetInventoryName, ParentInventoryDisplay.TargetInventory.Content[Index], 0, Index, ParentInventoryDisplay.PlayerID);
				}
			}
		}

        /// <summary>
        /// 이 슬롯에 있는 아이템 1개를 소비하여 소리를 발생시키고 사용 중인 이 아이템에 대해 정의된 모든 동작을 실행합니다.
        /// </summary>
        public virtual void Use()
		{
			if (!SlotEnabled) { return; }
			MMInventoryEvent.Trigger(MMInventoryEventType.UseRequest, this, ParentInventoryDisplay.TargetInventoryName, ParentInventoryDisplay.TargetInventory.Content[Index], 0, Index, ParentInventoryDisplay.PlayerID);
		}

        /// <summary>
        /// 가능하다면 이 아이템을 장착하세요.
        /// </summary>
        public virtual void Equip()
		{
			if (!SlotEnabled) { return; }
			MMInventoryEvent.Trigger(MMInventoryEventType.EquipRequest, this, ParentInventoryDisplay.TargetInventoryName, ParentInventoryDisplay.TargetInventory.Content[Index], 0, Index, ParentInventoryDisplay.PlayerID);
		}

        /// <summary>
        /// 가능하다면 이 아이템을 장착 해제하세요.
        /// </summary>
        public virtual void UnEquip()
		{
			if (!SlotEnabled) { return; }
			MMInventoryEvent.Trigger(MMInventoryEventType.UnEquipRequest, this, ParentInventoryDisplay.TargetInventoryName, ParentInventoryDisplay.TargetInventory.Content[Index], 0, Index, ParentInventoryDisplay.PlayerID);
		}

        /// <summary>
        /// 이 아이템을 떨어뜨립니다.
        /// </summary>
        public virtual void Drop()
		{
			if (!SlotEnabled) { return; }
			if (InventoryItem.IsNull(ParentInventoryDisplay.TargetInventory.Content[Index]))
			{
				MMInventoryEvent.Trigger(MMInventoryEventType.Error, this, ParentInventoryDisplay.TargetInventoryName, null, 0, Index, ParentInventoryDisplay.PlayerID);
				return;
			}
			if (!ParentInventoryDisplay.TargetInventory.Content[Index].Droppable)
			{
				return;
			}
			if (ParentInventoryDisplay.TargetInventory.Content[Index].Drop(ParentInventoryDisplay.PlayerID))
			{
				InventoryDisplay.CurrentlyBeingMovedItemIndex = -1;
				InventoryDisplay.CurrentlyBeingMovedFromInventoryDisplay = null;
				MMInventoryEvent.Trigger(MMInventoryEventType.Drop, this, ParentInventoryDisplay.TargetInventoryName, ParentInventoryDisplay.TargetInventory.Content[Index], 0, Index, ParentInventoryDisplay.PlayerID);
			}            
		}

		//아이템을 구매합니다.
		public virtual void Buy()
		{
			Debug.Log($"{ParentInventoryDisplay.TargetInventoryName} 에서 구매를 합니다.");
            

        }

		//아이템을 판매 합니다.
		public virtual void Sell()
		{
            Debug.Log($"{ParentInventoryDisplay.TargetInventoryName} 에서 판매를 합니다.");
        }

        /// <summary>
        /// 슬롯을 비활성화합니다.
        /// </summary>
        public virtual void DisableSlot()
		{
			this.interactable = false;
			SlotEnabled = false;
			TargetCanvasGroup.alpha = _disabledAlpha;
		}

        /// <summary>
        /// 슬롯을 활성화합니다.
        /// </summary>
        public virtual void EnableSlot()
		{
			this.interactable = true;
			SlotEnabled = true;
			TargetCanvasGroup.alpha = _enabledAlpha;
		}

        /// <summary>
        /// 이 슬롯의 아이템을 장착할 수 있으면 true를 반환하고, 그렇지 않으면 false를 반환합니다.
        /// </summary>
        public virtual bool Equippable()
		{
			if (InventoryItem.IsNull(ParentInventoryDisplay.TargetInventory.Content[Index]))
			{
				return false;
			}
			if (!ParentInventoryDisplay.TargetInventory.Content[Index].IsEquippable)
			{
				return false;
			}
			else
			{
				return true;
			}
		}

        /// <summary>
        /// 이 슬롯의 아이템을 사용할 수 있으면 true를 반환하고, 그렇지 않으면 false를 반환합니다.
        /// </summary>
        public virtual bool Usable()
		{
			if (InventoryItem.IsNull(ParentInventoryDisplay.TargetInventory.Content[Index]))
			{
				return false;
			}
			if (!ParentInventoryDisplay.TargetInventory.Content[Index].IsUsable)
			{
				return false;
			}
			else
			{
				return true;
			}
		}

        /// <summary>
        /// 이 슬롯에 아이템을 떨어뜨릴 수 있으면 true를 반환하고, 그렇지 않으면 false를 반환합니다.
        /// </summary>
        public virtual bool Movable()
		{
			if (InventoryItem.IsNull(ParentInventoryDisplay.TargetInventory.Content[Index]))
			{
				return false;
			}
			if (!ParentInventoryDisplay.TargetInventory.Content[Index].CanMoveObject)
			{
				return false;
			}
			else
			{
				return true;
			}
		}

        /// <summary>
        /// 이 슬롯에 아이템을 떨어뜨릴 수 있으면 true를 반환하고, 그렇지 않으면 false를 반환합니다.
        /// </summary>
        public virtual bool Droppable()
		{
			if (InventoryItem.IsNull(ParentInventoryDisplay.TargetInventory.Content[Index]))
			{
				return false;
			}
			if (!ParentInventoryDisplay.TargetInventory.Content[Index].Droppable)
			{
				return false;
			}
			else
			{
                return true;
            }
        }

        // 선택중인 아이템을 반환합니다.
		public virtual InventoryItem CurrentlySelectedItem()
		{
			if (InventoryItem.IsNull(ParentInventoryDisplay.TargetInventory.Content[Index]))
			{
				return ParentInventoryDisplay.TargetInventory.Content[Index];
			}
			else
				return null;
        }

        /// <summary>
        /// 이 슬롯에 아이템을 떨어뜨릴 수 있으면 true를 반환하고, 그렇지 않으면 false를 반환합니다.
        /// </summary>
        public virtual bool Unequippable()
		{
			if (InventoryItem.IsNull(ParentInventoryDisplay.TargetInventory.Content[Index]))
			{
				return false;
			}
			if (ParentInventoryDisplay.TargetInventory.InventoryType != Inventory.InventoryTypes.Equipment)
			{
				return false;
			}
			else
			{
				return true;
			}
		}

		public virtual bool EquipUseButtonShouldShow()
		{
			if (InventoryItem.IsNull(ParentInventoryDisplay.TargetInventory.Content[Index])) { return false; }
			return ParentInventoryDisplay.TargetInventory.Content[Index].DisplayProperties.DisplayEquipUseButton;
		}

		public virtual bool MoveButtonShouldShow()
		{
			if (InventoryItem.IsNull(ParentInventoryDisplay.TargetInventory.Content[Index])) { return false; }
			return ParentInventoryDisplay.TargetInventory.Content[Index].DisplayProperties.DisplayMoveButton;
		}

		public virtual bool DropButtonShouldShow()
		{
			if (InventoryItem.IsNull(ParentInventoryDisplay.TargetInventory.Content[Index])) { return false; }
			return ParentInventoryDisplay.TargetInventory.Content[Index].DisplayProperties.DisplayDropButton;
		}

		public virtual bool EquipButtonShouldShow()
		{
			if (InventoryItem.IsNull(ParentInventoryDisplay.TargetInventory.Content[Index])) { return false; }
			return ParentInventoryDisplay.TargetInventory.Content[Index].DisplayProperties.DisplayEquipButton;
		}

		public virtual bool UseButtonShouldShow()
		{
			if (InventoryItem.IsNull(ParentInventoryDisplay.TargetInventory.Content[Index])) { return false; }
			return ParentInventoryDisplay.TargetInventory.Content[Index].DisplayProperties.DisplayUseButton;
		}

		public virtual bool UnequipButtonShouldShow()
		{
			if (InventoryItem.IsNull(ParentInventoryDisplay.TargetInventory.Content[Index])) { return false; }
			return ParentInventoryDisplay.TargetInventory.Content[Index].DisplayProperties.DisplayUnequipButton;
		}
		
	}
}