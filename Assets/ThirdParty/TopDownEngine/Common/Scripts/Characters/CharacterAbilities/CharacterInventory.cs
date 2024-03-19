using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using MoreMountains.InventoryEngine;
using System.Collections.Generic;

namespace MoreMountains.TopDownEngine
{
	[System.Serializable]
	public struct AutoPickItem
	{
		public InventoryItem Item;
		public int Quantity;
	}

    /// <summary>
    /// 이 구성 요소를 캐릭터에 추가하면 인벤토리를 제어할 수 있습니다.
    /// Animator parameters : none
    /// </summary>
    [MMHiddenProperties("AbilityStopFeedbacks")]
	[AddComponentMenu("TopDown Engine/Character/Abilities/Character Inventory")] 
	public class CharacterInventory : CharacterAbility, MMEventListener<MMInventoryEvent>
	{
		public enum WeaponRotationModes { Normal, AddEmptySlot, AddInitialWeapon }
        
		[Header("Inventories")]
        /// InventoryEngine에 관한 한 이 플레이어의 고유 ID입니다. 이는 해당 플레이어에 대한 모든 Inventory 및 InventoryEngine UI 구성 요소의 PlayerID와 일치해야 합니다. 여기서 멀티플레이어를 사용하지 않으려면 Player1을 그대로 두세요.
        [Tooltip("InventoryEngine에 관한 한 이 플레이어의 고유 ID입니다. 이는 해당 플레이어에 대한 모든 Inventory 및 InventoryEngine UI 구성 요소의 PlayerID와 일치해야 합니다. 여기서 멀티플레이어를 사용하지 않으려면 Player1을 그대로 두세요.")]
		public string PlayerID = "Player1";
        /// 이 캐릭터의 주요 인벤토리 이름
        [Tooltip("이 캐릭터의 주요 인벤토리 이름")]
		public string MainInventoryName;
        /// 이 캐릭터가 무기를 보관하는 인벤토리의 이름
        [Tooltip("이 캐릭터가 무기를 보관하는 인벤토리의 이름")]
		public string WeaponInventoryName;
        /// 이 캐릭터의 단축바 인벤토리 이름
        [Tooltip("이 캐릭터의 단축바 인벤토리 이름")]
		public string HotbarInventoryName;
        /// 인벤토리에 전달할 변환은 인벤토리에 전달되어 드롭에 대한 참조로 사용됩니다. 비워두면 this.transform이 사용됩니다.
        [Tooltip("인벤토리에 전달할 변환은 인벤토리에 전달되어 드롭에 대한 참조로 사용됩니다. 비워두면 this.transform이 사용됩니다.")]
		public Transform InventoryTransform;

		[Header("Weapon Rotation")]
        /// 무기 회전 모드: Normal은 모든 무기를 순환하고, AddEmptySlot은 빈 손으로 돌아가고, AddOriginalWeapon은 원래 무기로 다시 순환합니다.
        [Tooltip("무기 회전 모드: Normal은 모든 무기를 순환하고, AddEmptySlot은 빈 손으로 돌아가고, AddOriginalWeapon은 원래 무기로 다시 순환합니다.")]
		public WeaponRotationModes WeaponRotationMode = WeaponRotationModes.Normal;

		[Header("Auto Pick")]
        /// 시작 시 이 캐릭터의 인벤토리에 자동으로 추가할 아이템 목록
        [Tooltip("시작 시 이 캐릭터의 인벤토리에 자동으로 추가할 아이템 목록")]
		public AutoPickItem[] AutoPickItems;
        /// 이것이 사실이라면 자동 선택 항목은 기본 인벤토리가 비어 있는 경우에만 추가됩니다.
        [Tooltip("이것이 사실이라면 자동 선택 항목은 기본 인벤토리가 비어 있는 경우에만 추가됩니다.")]
		public bool AutoPickOnlyIfMainInventoryIsEmpty;
		
		[Header("Auto Equip")]
        /// 시작 시 자동으로 장착되는 무기
        [Tooltip("시작 시 자동으로 장착되는 무기")]
		public InventoryWeapon AutoEquipWeaponOnStart;
        /// 이것이 사실이라면 자동 장착은 메인 인벤토리가 비어 있는 경우에만 발생합니다.
        [Tooltip("이것이 사실이라면 자동 장착은 메인 인벤토리가 비어 있는 경우에만 발생합니다.")]
		public bool AutoEquipOnlyIfMainInventoryIsEmpty;
        /// 대상 핸들 무기 능력 - 비어 있는 경우 가장 먼저 찾은 것을 선택합니다.
        [Tooltip("대상 핸들 무기 능력 - 비어 있는 경우 가장 먼저 찾은 것을 선택합니다.")]
		public CharacterHandleWeapon CharacterHandleWeapon;

		public Inventory MainInventory { get; set; }
		public Inventory WeaponInventory { get; set; }
		public Inventory HotbarInventory { get; set; }
		public List<string> AvailableWeaponsIDs => _availableWeaponsIDs;

		protected List<int> _availableWeapons;
		protected List<string> _availableWeaponsIDs;
		protected string _nextWeaponID;
		protected bool _nextFrameWeapon = false;
		protected string _nextFrameWeaponName;
		protected const string _emptySlotWeaponName = "_EmptySlotWeaponName";
		protected const string _initialSlotWeaponName = "_InitialSlotWeaponName";
		protected bool _initialized = false;

		/// <summary>
		/// On init we setup our ability
		/// </summary>
		protected override void Initialization () 
		{
			base.Initialization();
			Setup ();
		}

		/// <summary>
		/// Grabs all inventories, and fills weapon lists
		/// </summary>
		protected virtual void Setup()
		{
			if (InventoryTransform == null)
			{
				InventoryTransform = this.transform;
			}
			GrabInventories ();
			if (CharacterHandleWeapon == null)
			{
				CharacterHandleWeapon = _character?.FindAbility<CharacterHandleWeapon> ();	
			}
			FillAvailableWeaponsLists ();

			if (_initialized)
			{
				return;
			}

			bool mainInventoryEmpty = true;
			if (MainInventory != null)
			{
				mainInventoryEmpty = MainInventory.NumberOfFilledSlots == 0;
			}
			bool canAutoPick = !(AutoPickOnlyIfMainInventoryIsEmpty && !mainInventoryEmpty);
			bool canAutoEquip = !(AutoEquipOnlyIfMainInventoryIsEmpty && !mainInventoryEmpty);
			
			// we auto pick items if needed
			if ((AutoPickItems.Length > 0) && !_initialized && canAutoPick)
			{
				foreach (AutoPickItem item in AutoPickItems)
				{
					MMInventoryEvent.Trigger(MMInventoryEventType.Pick, null, item.Item.TargetInventoryName, item.Item, item.Quantity, 0, PlayerID);
				}
			}

			// we auto equip a weapon if needed
			if ((AutoEquipWeaponOnStart != null) && !_initialized && canAutoEquip)
			{
				MMInventoryEvent.Trigger(MMInventoryEventType.Pick, null, AutoEquipWeaponOnStart.TargetInventoryName, AutoEquipWeaponOnStart, 1, 0, PlayerID);
				EquipWeapon(AutoEquipWeaponOnStart.ItemID);
			}
			_initialized = true;
		}

		public override void ProcessAbility()
		{
			base.ProcessAbility();
            
			if (_nextFrameWeapon)
			{
				EquipWeapon(_nextFrameWeaponName);
				_nextFrameWeapon = false;
			}
		}

		/// <summary>
		/// Grabs any inventory it can find that matches the names set in the inspector
		/// </summary>
		protected virtual void GrabInventories()
		{
			Inventory[] inventories = FindObjectsOfType<Inventory>();
			foreach (Inventory inventory in inventories)
			{
				if (inventory.PlayerID != PlayerID)
				{
					continue;
				}
				if ((MainInventory == null) && (inventory.name == MainInventoryName))
				{
					MainInventory = inventory;
				}
				if ((WeaponInventory == null) && (inventory.name == WeaponInventoryName))
				{
					WeaponInventory = inventory;
				}
				if ((HotbarInventory == null) && (inventory.name == HotbarInventoryName))
				{
					HotbarInventory = inventory;
				}
			}
			if (MainInventory != null) { MainInventory.SetOwner (this.gameObject); MainInventory.TargetTransform = InventoryTransform;}
			if (WeaponInventory != null) { WeaponInventory.SetOwner (this.gameObject); WeaponInventory.TargetTransform = InventoryTransform;}
			if (HotbarInventory != null) { HotbarInventory.SetOwner (this.gameObject); HotbarInventory.TargetTransform = InventoryTransform;}
		}

		/// <summary>
		/// On handle input, we watch for the switch weapon button, and switch weapon if needed
		/// </summary>
		protected override void HandleInput()
		{
			if (!AbilityAuthorized
			    || (_condition.CurrentState != CharacterStates.CharacterConditions.Normal))
			{
				return;
			}
			if (_inputManager.SwitchWeaponButton.State.CurrentState == MMInput.ButtonStates.ButtonDown)
			{
				SwitchWeapon ();
			}
		}

		/// <summary>
		/// Fills the weapon list. The weapon list will be used to determine what weapon we can switch to
		/// </summary>
		protected virtual void FillAvailableWeaponsLists()
		{
			_availableWeaponsIDs = new List<string> ();
			if ((CharacterHandleWeapon == null) || (WeaponInventory == null))
			{
				return;
			}
			_availableWeapons = MainInventory.InventoryContains (ItemClasses.Weapon);
			foreach (int index in _availableWeapons)
			{
				_availableWeaponsIDs.Add (MainInventory.Content [index].ItemID);
			}
			if (!InventoryItem.IsNull(WeaponInventory.Content[0]))
			{
				_availableWeaponsIDs.Add (WeaponInventory.Content [0].ItemID);
			}

			_availableWeaponsIDs.Sort ();
		}

		/// <summary>
		/// Determines the name of the next weapon in line
		/// </summary>
		protected virtual void DetermineNextWeaponName ()
		{
			if (InventoryItem.IsNull(WeaponInventory.Content[0]))
			{
				_nextWeaponID = _availableWeaponsIDs [0];
				return;
			}

			if ((_nextWeaponID == _emptySlotWeaponName) || (_nextWeaponID == _initialSlotWeaponName))
			{
				_nextWeaponID = _availableWeaponsIDs[0];
				return;
			}

			for (int i = 0; i < _availableWeaponsIDs.Count; i++)
			{
				if (_availableWeaponsIDs[i] == WeaponInventory.Content[0].ItemID)
				{
					if (i == _availableWeaponsIDs.Count - 1)
					{
						switch (WeaponRotationMode)
						{
							case WeaponRotationModes.AddEmptySlot:
								_nextWeaponID = _emptySlotWeaponName;
								return;
							case WeaponRotationModes.AddInitialWeapon:
								_nextWeaponID = _initialSlotWeaponName;
								return;
						}

						_nextWeaponID = _availableWeaponsIDs [0];
					}
					else
					{
						_nextWeaponID = _availableWeaponsIDs [i+1];
					}
				}
			}
		}

		/// <summary>
		/// Equips the weapon with the name passed in parameters
		/// </summary>
		/// <param name="weaponID"></param>
		public virtual void EquipWeapon(string weaponID)
		{
			if ((weaponID == _emptySlotWeaponName) && (CharacterHandleWeapon != null))
			{
				MMInventoryEvent.Trigger(MMInventoryEventType.UnEquipRequest, null, WeaponInventoryName, WeaponInventory.Content[0], 0, 0, PlayerID);
				CharacterHandleWeapon.ChangeWeapon(null, _emptySlotWeaponName, false);
				MMInventoryEvent.Trigger(MMInventoryEventType.Redraw, null, WeaponInventory.name, null, 0, 0, PlayerID);
				return;
			}

			if ((weaponID == _initialSlotWeaponName) && (CharacterHandleWeapon != null))
			{
				MMInventoryEvent.Trigger(MMInventoryEventType.UnEquipRequest, null, WeaponInventoryName, WeaponInventory.Content[0], 0, 0, PlayerID);
				CharacterHandleWeapon.ChangeWeapon(CharacterHandleWeapon.InitialWeapon, _initialSlotWeaponName, false);
				MMInventoryEvent.Trigger(MMInventoryEventType.Redraw, null, WeaponInventory.name, null, 0, 0, PlayerID);
				return;
			}
			
			for (int i = 0; i < MainInventory.Content.Length ; i++)
			{
				if (InventoryItem.IsNull(MainInventory.Content[i]))
				{
					continue;
				}
				if (MainInventory.Content[i].ItemID == weaponID)
				{
					MMInventoryEvent.Trigger(MMInventoryEventType.EquipRequest, null, MainInventory.name, MainInventory.Content[i], 0, i, PlayerID);
					break;
				}
			}
		}

		/// <summary>
		/// Switches to the next weapon in line
		/// </summary>
		protected virtual void SwitchWeapon()
		{
			// if there's no character handle weapon component, we can't switch weapon, we do nothing and exit
			if ((CharacterHandleWeapon == null) || (WeaponInventory == null))
			{
				return;
			}

			FillAvailableWeaponsLists ();

			// if we only have 0 or 1 weapon, there's nothing to switch, we do nothing and exit
			if (_availableWeaponsIDs.Count <= 0)
			{
				return;
			}

			DetermineNextWeaponName ();
			EquipWeapon (_nextWeaponID);
			PlayAbilityStartFeedbacks();
			PlayAbilityStartSfx();
		}

		/// <summary>
		/// Watches for InventoryLoaded events
		/// When an inventory gets loaded, if it's our WeaponInventory, we check if there's already a weapon equipped, and if yes, we equip it
		/// </summary>
		/// <param name="inventoryEvent">Inventory event.</param>
		public virtual void OnMMEvent(MMInventoryEvent inventoryEvent)
		{
			if (inventoryEvent.InventoryEventType == MMInventoryEventType.InventoryLoaded)
			{
				if (inventoryEvent.TargetInventoryName == WeaponInventoryName)
				{
					this.Setup ();
					if (WeaponInventory != null)
					{
						if (!InventoryItem.IsNull (WeaponInventory.Content [0]))
						{
							CharacterHandleWeapon.Setup ();
							WeaponInventory.Content [0].Equip (PlayerID);
						}
					}
				}
			}
			if (inventoryEvent.InventoryEventType == MMInventoryEventType.Pick)
			{
				bool isSubclass = (inventoryEvent.EventItem.GetType().IsSubclassOf(typeof(InventoryWeapon)));
				bool isClass = (inventoryEvent.EventItem.GetType() == typeof(InventoryWeapon));
				if (isClass || isSubclass)
				{
					InventoryWeapon inventoryWeapon = (InventoryWeapon)inventoryEvent.EventItem;
					switch (inventoryWeapon.AutoEquipMode)
					{
						case InventoryWeapon.AutoEquipModes.NoAutoEquip:
							// we do nothing
							break;

						case InventoryWeapon.AutoEquipModes.AutoEquip:
							_nextFrameWeapon = true;
							_nextFrameWeaponName = inventoryEvent.EventItem.ItemID;
							break;

						case InventoryWeapon.AutoEquipModes.AutoEquipIfEmptyHanded:
							if (CharacterHandleWeapon.CurrentWeapon == null)
							{
								_nextFrameWeapon = true;
								_nextFrameWeaponName = inventoryEvent.EventItem.ItemID;
							}
							break;
					}
				}
			}
		}

		protected override void OnDeath()
		{
			base.OnDeath();
			if (WeaponInventory != null)
			{
				MMInventoryEvent.Trigger(MMInventoryEventType.UnEquipRequest, null, WeaponInventoryName, WeaponInventory.Content[0], 0, 0, PlayerID);
			}            
		}

		/// <summary>
		/// On enable, we start listening for MMGameEvents. You may want to extend that to listen to other types of events.
		/// </summary>
		protected override void OnEnable()
		{
			base.OnEnable();
			this.MMEventStartListening<MMInventoryEvent>();
		}

		/// <summary>
		/// On disable, we stop listening for MMGameEvents. You may want to extend that to stop listening to other types of events.
		/// </summary>
		protected override void OnDisable()
		{
			base.OnDisable ();
			this.MMEventStopListening<MMInventoryEvent>();
		}
	}
}