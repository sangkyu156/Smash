using UnityEngine;
using MoreMountains.Tools;
using System;

namespace MoreMountains.InventoryEngine
{
	[Serializable]
	public class InventoryItemDisplayProperties
	{
		public bool DisplayEquipUseButton = true;
		public bool DisplayMoveButton = true;
		public bool DisplayDropButton = true;
		public bool DisplayEquipButton = true;
		public bool DisplayUseButton = true;
		public bool DisplayUnequipButton = true;
	}
	
	[Serializable]
    /// <summary>
    /// 확장될 예정인 인벤토리 아이템의 기본 클래스입니다.
    /// 기본 속성을 처리하고 드롭 생성을 수행합니다.
    /// </summary>
    public class InventoryItem : ScriptableObject 
	{
		[Header("ID and Target")]
        /// 항목의 (고유) ID
        [Tooltip("항목의 (고유) ID")]
		public string ItemID;
        /// 이 아이템이 저장될 인벤토리 이름
        [Tooltip("이 아이템이 저장될 인벤토리 이름")]
		public string TargetInventoryName = "MainInventory";
        /// 이것이 true인 경우 항목은 인벤토리에 공간이 있는 어디에도 추가되지 않고 대신 지정된 TargetIndex 슬롯에 추가됩니다.
        [Tooltip("이것이 true인 경우 항목은 인벤토리에 공간이 있는 어디에도 추가되지 않고 대신 지정된 TargetIndex 슬롯에 추가됩니다.")]
		public bool ForceSlotIndex = false;
        /// ForceSlotIndex가 true인 경우 대상 인벤토리에 항목이 추가될 인덱스입니다.
        [Tooltip("ForceSlotIndex가 true인 경우 대상 인벤토리에 항목이 추가될 인덱스입니다.")]
		[MMCondition("ForceSlotIndex", true)]
		public int TargetIndex = 0;

		[Header("Permissions")]
        /// 이 항목을 "사용"할 수 있는지 여부(Use 메서드를 통해) - 중요합니다. 이는 이 개체의 초기 상태일 뿐이며, IsUsable은 그 이후 언제든지 사용됩니다.
        [Tooltip("이 항목을 \"사용\"할 수 있는지 여부(Use 메서드를 통해) - 중요합니다. 이는 이 개체의 초기 상태일 뿐이며, IsUsable은 그 이후 언제든지 사용됩니다.")]
		public bool Usable = false;

        /// 이것이 true라면 설치류인 아이템이다.
        public bool isInstallable = false;

        /// 이것이 사실이라면 해당 객체에 대해 Use를 호출하면 해당 객체의 한 단위가 소비됩니다.
        [Tooltip("이것이 사실이라면 해당 객체에 대해 Use를 호출하면 해당 객체의 한 단위가 소비됩니다.")]
		[MMCondition("Usable", true)] 
		public bool Consumable = true;

        /// 이 아이템이 소모품인 경우, 한 번 사용 시 소모되는 개수(보통 1개)를 결정합니다.
        [Tooltip("이 아이템이 소모품인 경우, 한 번 사용 시 소모되는 개수(보통 1개)를 결정합니다.")]
		[MMCondition("Consumable", true)] 
		public int ConsumeQuantity = 1;
        /// 이 아이템을 장착할 수 있는지 여부 - 중요합니다. 이는 이 개체의 초기 상태일 뿐이며, IsEquippable은 그 이후 언제든지 사용됩니다.
        [Tooltip("이 아이템을 장착할 수 있는지 여부 - 중요합니다. 이는 이 개체의 초기 상태일 뿐이며, IsEquippable은 그 이후 언제든지 사용됩니다.")]
		public bool Equippable = false;
        /// 대상 인벤토리가 가득 찬 경우 이 아이템을 장착할 수 있는지 여부
        [Tooltip("대상 인벤토리가 가득 찬 경우 이 아이템을 장착할 수 있는지 여부")]
		[MMCondition("Equippable", true)]
		public bool EquippableIfInventoryIsFull = true;
        /// 이것이 사실이라면 이 아이템은 장착 시 원래 인벤토리에서 제거되고 EquipmentInventory로 이동됩니다.
        [Tooltip("이것이 사실이라면 이 아이템은 장착 시 원래 인벤토리에서 제거되고 EquipmentInventory로 이동됩니다.")]
		[MMCondition("Equippable", true)]
		public bool MoveWhenEquipped = true;

        /// 이것이 사실이라면 이 아이템은 드랍될 수 있습니다
        [Tooltip("이것이 사실이라면 이 아이템은 드랍될 수 있습니다")]
		public bool Droppable = true;
        /// 이것이 사실이라면 객체를 이동할 수 있습니다.
        [Tooltip("이것이 사실이라면 객체를 이동할 수 있습니다.")]
		public bool CanMoveObject=true;
        /// 이것이 사실이라면 객체는 다른 객체로 교체될 수 있습니다.
        [Tooltip("이것이 사실이라면 객체는 다른 객체로 교체될 수 있습니다.")]
		public bool CanSwapObject=true;
        /// 해당 항목이 선택될 때 인벤토리 작업 버튼을 표시할지 여부를 정의하는 속성 집합
        [Tooltip("해당 항목이 선택될 때 인벤토리 작업 버튼을 표시할지 여부를 정의하는 속성 집합")]
		public InventoryItemDisplayProperties DisplayProperties;

        /// 이 객체를 사용할 수 있는지 여부
        public virtual bool IsUsable {  get { return Usable;  } }
        /// 이 객체를 사용할 수 있는지 여부
        public virtual bool IsEquippable { get { return Equippable; } }
        //  이 객체를 설치할 수 있는지 여부
        public virtual bool IsInstallable { get { return isInstallable; } }


        [HideInInspector]
        /// 이 아이템의 기본 수량
        public int Quantity = 1;
        // 이 아이템의 가격
        public int price = 1;

        [Header("Basic info")]
        /// 항목 이름 - 세부정보 패널에 표시됩니다.
        [Tooltip("항목 이름 - 세부정보 패널에 표시됩니다.")]
		public string ItemName;
        public Define.Grade grade = Define.Grade.Normal;
        
        /// 세부정보 패널에 표시할 항목의 간단한 설명
        [TextArea]
		[Tooltip("세부정보 패널에 표시할 항목의 간단한 설명")]
		public string ShortDescription;
		[TextArea]
        /// 세부정보 패널에 표시할 항목의 긴 설명
        [Tooltip("세부정보 패널에 표시할 항목의 긴 설명")]
		public string Description;

		[Header("Image")]
        /// 인벤토리 슬롯에 표시될 아이콘
        [Tooltip("인벤토리 슬롯에 표시될 아이콘")]
		public Sprite Icon;

		[Header("Prefab Drop")]
        /// 항목을 떨어뜨렸을 때 인스턴스화할 프리팹
        [Tooltip("항목을 떨어뜨렸을 때 인스턴스화할 프리팹")]
		public GameObject Prefab;
        /// 이것이 사실인 경우, 삭제 시 객체의 수량은 강제로 PrefabDropQuantity로 설정됩니다.
        [Tooltip("이것이 사실인 경우, 삭제 시 객체의 수량은 강제로 PrefabDropQuantity로 설정됩니다.")]
		public bool ForcePrefabDropQuantity = false;
        /// ForcePrefabDropQuantity가 true인 경우 생성된 항목에 강제할 수량입니다.
        [Tooltip("ForcePrefabDropQuantity가 true인 경우 생성된 항목에 강제할 수량입니다.")]
		[MMCondition("ForcePrefabDropQuantity", true)]
		public int PrefabDropQuantity = 1;
        /// 객체를 떨어뜨렸을 때 생성되어야 하는 최소 거리
        [Tooltip("객체를 떨어뜨렸을 때 생성되어야 하는 최소 거리")]
		public MMSpawnAroundProperties DropProperties;

		[Header("Inventory Properties")]
        /// 이 개체를 쌓을 수 있는 경우(단일 인벤토리 슬롯에 여러 인스턴스) 여기에서 해당 스택의 최대 크기를 지정할 수 있습니다.
        [Tooltip("이 개체를 쌓을 수 있는 경우(단일 인벤토리 슬롯에 여러 인스턴스) 여기에서 해당 스택의 최대 크기를 지정할 수 있습니다.")]
		public int MaximumStack = 1;
        /// 아이템의 클래스
        [Tooltip("아이템의 클래스")]
		public ItemClasses ItemClass;

		[Header("Equippable")]
        /// 이 아이템을 장착할 수 있는 경우 여기에서 대상 인벤토리 이름(예: ArmorInventory)을 설정할 수 있습니다. 물론 장면에 이름이 일치하는 인벤토리가 필요합니다.
        [Tooltip("이 아이템을 장착할 수 있는 경우 여기에서 대상 인벤토리 이름(예: ArmorInventory)을 설정할 수 있습니다. 물론 장면에 이름이 일치하는 인벤토리가 필요합니다.")]
		public string TargetEquipmentInventoryName;
        /// 아이템을 장착했을 때 재생되어야 하는 소리(선택 사항)
        [Tooltip("아이템을 장착했을 때 재생되어야 하는 소리(선택 사항)")]
		public AudioClip EquippedSound;

		[Header("Usable")]
        /// 이 항목을 사용할 수 있는 경우 여기에서 사용할 때 재생할 소리를 설정할 수 있습니다. 그렇지 않은 경우 기본 소리가 재생됩니다.
        [Tooltip("이 항목을 사용할 수 있는 경우 여기에서 사용할 때 재생할 소리를 설정할 수 있습니다. 그렇지 않은 경우 기본 소리가 재생됩니다.")]
		public AudioClip UsedSound;

		[Header("Sounds")]
        /// 항목을 이동할 때 재생할 소리(선택 사항)
        [Tooltip("항목을 이동할 때 재생할 소리(선택 사항)")]
		public AudioClip MovedSound;
        /// 항목을 떨어뜨렸을 때 재생되어야 하는 소리(선택 사항)
        [Tooltip("항목을 떨어뜨렸을 때 재생되어야 하는 소리(선택 사항)")]
		public AudioClip DroppedSound;
        /// false로 설정하면 기본 사운드가 사용되지 않고 사운드가 재생되지 않습니다.
        [Tooltip("false로 설정하면 기본 사운드가 사용되지 않고 사운드가 재생되지 않습니다.")]
		public bool UseDefaultSoundsIfNull = true;

		protected Inventory _targetInventory = null;
		protected Inventory _targetEquipmentInventory = null;

        /// <summary>
        /// 대상 인벤토리를 가져옵니다.
        /// </summary>
        /// <value>대상 인벤토리입니다.</value>
        public virtual Inventory TargetInventory(string playerID)
		{ 
			if (TargetInventoryName == null)
			{
				return null;
			}
			_targetInventory = Inventory.FindInventory(TargetInventoryName, playerID);
			return _targetInventory;
		}

        /// <summary>
        /// 대상 장비 인벤토리를 가져옵니다.
        /// </summary>
        /// <value>대상 장비 인벤토리입니다.</value>
        public virtual Inventory TargetEquipmentInventory(string playerID)
		{ 
			if (TargetEquipmentInventoryName == null)
			{
				return null;
			}
			_targetEquipmentInventory = Inventory.FindInventory(TargetEquipmentInventoryName, playerID);
			return _targetEquipmentInventory;
		}

        /// <summary>
        /// 항목이 null인지 여부를 결정합니다.
        /// </summary>
        /// <returns><c>true</c> null인 경우 지정된 항목입니다. 그렇지 않으면, <c>false</c>.</returns>
        /// <param name="item">Item.</param>
        public static bool IsNull(InventoryItem item)
		{
			if (item==null)
			{
				return true;
			}
			if (item.ItemID==null)
			{
				return true;
			}
			if (item.ItemID=="")
			{
				return true;
			}
			return false;
		}

        /// <summary>
        /// 항목을 새 항목으로 복사합니다.
        /// </summary>
        public virtual InventoryItem Copy()
		{
			string name = this.name;
			InventoryItem clone = UnityEngine.Object.Instantiate(this) as InventoryItem;
			clone.name = name;
			return clone;
		}

        /// <summary>
        /// 연관된 프리팹을 생성합니다.
        /// </summary>
        public virtual void SpawnPrefab(string playerID)
		{
			if (TargetInventory(playerID) != null)
			{
                // 이 슬롯에 항목에 대한 프리팹 세트가 있는 경우 지정된 오프셋에서 인스턴스화합니다.
                if (Prefab!=null && TargetInventory(playerID).TargetTransform!=null)
				{
					GameObject droppedObject=(GameObject)Instantiate(Prefab);
					if (droppedObject.GetComponent<ItemPicker>()!=null)
					{
						if (ForcePrefabDropQuantity)
						{
							droppedObject.GetComponent<ItemPicker>().Quantity = PrefabDropQuantity;
							droppedObject.GetComponent<ItemPicker>().RemainingQuantity = PrefabDropQuantity;	
						}
					}

					MMSpawnAround.ApplySpawnAroundProperties(droppedObject, DropProperties,
						TargetInventory(playerID).TargetTransform.position);
				}
			}
		}

        /// <summary>
        /// 개체를 선택하면 어떤 일이 발생합니까? 이를 재정의하여 자신만의 동작을 추가하세요.
        /// </summary>
        public virtual bool Pick(string playerID) { return true; }

        /// <summary>
        /// 객체가 사용될 때 일어나는 일 - 이를 재정의하여 자신만의 동작을 추가하세요.
        /// </summary>
        public virtual bool Use(string playerID) { return true; }

        /// <summary>
        /// 객체가 장착되면 어떤 일이 발생합니까? 이를 재정의하여 자신만의 동작을 추가하세요.
        /// </summary>
        public virtual bool Equip(string playerID) { return true; }

        /// <summary>
        /// 개체가 장착되지 않은 경우(드롭 시 호출됨) 발생하는 상황 - 이를 재정의하여 고유한 동작을 추가합니다.
        /// </summary>
        public virtual bool UnEquip(string playerID) { return true; }

        /// <summary>
        /// 객체가 다른 객체로 교체되면 어떻게 되나요?
        /// </summary>
        public virtual void Swap(string playerID) {}

        /// <summary>
        /// 개체를 떨어뜨리면 어떻게 됩니까? 이를 재정의하여 자신만의 동작을 추가하세요.
        /// </summary>
        public virtual bool Drop(string playerID) { return true; }

        //아이템을 설치할때
        public virtual bool Installation(string playerID) { return true; }

        //설치 취소할때
        public virtual bool InstallationCancel(string playerID) { return true; }
    }
}