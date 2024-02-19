﻿using MoreMountains.Tools;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Codice.Client.BaseCommands.Import.Commit;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
	using UnityEngine.InputSystem;
#endif

namespace MoreMountains.InventoryEngine
{
    /// <summary>
    /// Example of how you can call an inventory from your game. 
    /// I suggest having your Input and GUI manager classes handle that though.
    /// </summary>
    public class InventoryInputManager : MonoBehaviour, MMEventListener<MMInventoryEvent>
    {
        [Header("Targets")]
        [MMInformation("여기에 인벤토리 컨테이너(인벤토리를 열거나 닫을 때 켜거나 끄려는 CanvasGroup), 기본 InventoryDisplay 및 열릴 때 InventoryDisplay 아래에 표시될 오버레이를 바인딩합니다.", MMInformationAttribute.InformationType.Info, false)]
        /// 인벤토리 열기/닫기 버튼을 누를 때 표시하거나 숨기려는 모든 요소가 포함된 CanvasGroup
        [Tooltip("인벤토리 열기/닫기 버튼을 누를 때 표시하거나 숨기려는 모든 요소가 포함된 CanvasGroup")]
        public CanvasGroup TargetInventoryContainer;
        // 내가만든 변수
        // 플레이어가 NPC근처가서 'E'키 누르면 나오는 상점
        public CanvasGroup NPC_InventoryContainer;
        // 플레이어가 NPC근처가서 'E'키 루느면 나오는 플레이어 인벤토리
        public CanvasGroup Player_InventoryContainer;
        // 퀵슬롯 인벤토리
        public InventoryDisplay QuickSlotsDisplay;
        /// 주요 재고 표시
        [Tooltip("The main inventory display")]
        public InventoryDisplay TargetInventoryDisplay;
        /// 인벤토리 열기/닫기 시 그 아래에 사용될 페이더입니다.
        [Tooltip("인벤토리 열기/닫기 시 그 아래에 사용될 페이더입니다.")]
        public CanvasGroup Overlay;

        [Header("Overlay")]
        /// 활성화 시 오버레이의 불투명도
        [Tooltip("활성화 시 오버레이의 불투명도")]
        public float OverlayActiveOpacity = 0.85f;
        /// 비활성 상태일 때 오버레이의 불투명도
        [Tooltip("비활성 상태일 때 오버레이의 불투명도")]
        public float OverlayInactiveOpacity = 0f;

        [Header("Start Behaviour")]
        [MMInformation("HideContainerOnStart를 true로 설정하면 이 필드 바로 위에 정의된 TargetInventoryContainer가 장면 뷰에 표시된 상태로 두더라도 시작 시 자동으로 숨겨집니다. 설정에 유용합니다.", MMInformationAttribute.InformationType.Info, false)]
        /// 이것이 사실이라면 시작 시 인벤토리 컨테이너가 자동으로 숨겨집니다.
        [Tooltip("이것이 사실이라면 시작 시 인벤토리 컨테이너가 자동으로 숨겨집니다.")]
        public bool HideContainerOnStart = true;

        [Header("Permissions")]
        [MMInformation("여기에서 인벤토리가 열려 있을 때만 입력을 포착하도록 할지 여부를 결정할 수 있습니다.", MMInformationAttribute.InformationType.Info, false)]
        /// 이것이 사실이라면 시작 시 인벤토리 컨테이너가 자동으로 숨겨집니다.
        [Tooltip("이것이 사실이라면 시작 시 인벤토리 컨테이너가 자동으로 숨겨집니다.")]
        public bool InputOnlyWhenOpen = true;

        //퀵슬롯 인벤토리
        public Inventory QuickSlots;
        int currentlySelectedSlot;

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
		
			[Header("Input System Key Mapping")] 

			/// the key used to open/close the inventory
			public InputActionProperty ToggleInventoryKey = new InputActionProperty(
				new InputAction(
					name: "IM_Toggle",
					type: InputActionType.Button, 
					binding: "Keyboard/I", 
					interactions: "Press(behavior=2)"));
			
			/// the key used to open the inventory
			public InputActionProperty OpenInventoryKey = new InputActionProperty(
				new InputAction(
					name: "IM_Open",
					type: InputActionType.Button, 
					interactions: "Press(behavior=2)"));
			
			/// the key used to open the inventory
			public InputActionProperty CloseInventoryKey = new InputActionProperty(
				new InputAction(
					name: "IM_Close",
					type: InputActionType.Button, 
					interactions: "Press(behavior=2)"));

			/// the alt key used to open/close the inventory
			public InputActionProperty CancelKey = new InputActionProperty(
				new InputAction(
					name: "IM_Cancel", 
					type: InputActionType.Button, 
					binding: "Keyboard/escape", 
					interactions: "Press(behavior=2)"));
			
			/// the key used to move an item
			public InputActionProperty MoveKey = new InputActionProperty(
				new InputAction(
					name: "IM_Move", 
					type: InputActionType.Button, 
					binding: "Keyboard/insert", 
					interactions: "Press(behavior=2)"));
			
			/// the key used to equip an item
			public InputActionProperty EquipKey = new InputActionProperty(
				new InputAction(
					name: "IM_Equip", 
					type: InputActionType.Button, 
					binding: "Keyboard/home",
					interactions: "Press(behavior=2)"));
			
			/// the key used to use an item
			public InputActionProperty UseKey = new InputActionProperty(
				new InputAction(
					name: "IM_Use", 
					type: InputActionType.Button, 
					binding: "Keyboard/end",
					interactions: "Press(behavior=2)"));
			
			/// the key used to equip or use an item
			public InputActionProperty EquipOrUseKey = new InputActionProperty(
				new InputAction(
					name: "IM_EquipOrUse", 
					type: InputActionType.Button, 
					binding: "Keyboard/space", 
					interactions: "Press(behavior=2)"));
			
			/// the key used to drop an item
			public InputActionProperty DropKey = new InputActionProperty(
				new InputAction(
					name: "IM_Drop", 
					type: InputActionType.Button,
					binding: "Keyboard/delete",		
					interactions: "Press(behavior=2)"));
			
			/// the key used to go to the next inventory
			public InputActionProperty NextInvKey = new InputActionProperty(
				new InputAction(
					name: "IM_NextInv", 
					type: InputActionType.Button, 
					binding: "Keyboard/pageDown", 
					interactions: "Press(behavior=2)"));
			
			/// the key used to go to the previous inventory
			public InputActionProperty PrevInvKey = new InputActionProperty(
				new InputAction(
					name: "IM_PrevInv", 
					type: InputActionType.Button, 
					binding: "Keyboard/pageUp", 
					interactions: "Press(behavior=2)"));
#else
        [Header("Key Mapping")]
        [MMInformation("여기서는 원하는 다양한 키 바인딩을 설정해야 합니다. 기본적으로 일부가 있지만 자유롭게 변경할 수 있습니다.", MMInformationAttribute.InformationType.Info, false)]
        /// 인벤토리를 열고 닫는 데 사용되는 키
        public KeyCode ToggleInventoryKey = KeyCode.I;
        /// 인벤토리를 열고 닫는 데 사용되는 Alt 키
        public KeyCode ToggleInventoryAltKey = KeyCode.Joystick1Button6;
        /// 인벤토리를 여는 데 사용되는 키
        public KeyCode OpenInventoryKey;
        /// the key used to close the inventory
        public KeyCode CloseInventoryKey;
        /// the alt key used to open/close the inventory
        public KeyCode CancelKey = KeyCode.Escape;
        /// the alt key used to open/close the inventory
        public KeyCode CancelKeyAlt = KeyCode.Joystick1Button7;
        /// the key used to move an item
        public string MoveKey = "insert";
        /// the alt key used to move an item
        public string MoveAltKey = "joystick button 2";
        /// the key used to equip an item
        public string EquipKey = "home";
        /// the alt key used to equip an item
        public string EquipAltKey = "home";
        /// the key used to use an item
        public string UseKey = "end";
        /// the alt key used to use an item
        public string UseAltKey = "end";
        /// the key used to equip or use an item
        public string EquipOrUseKey = "space";
        /// the alt key used to equip or use an item
        public string EquipOrUseAltKey = "joystick button 0";
        /// the key used to drop an item
        public string DropKey = "delete";
        /// the alt key used to drop an item
        public string DropAltKey = "joystick button 1";
        /// the key used to go to the next inventory
        public string NextInvKey = "page down";
        /// the alt key used to go to the next inventory
        public string NextInvAltKey = "joystick button 4";
        /// the key used to go to the previous inventory
        public string PrevInvKey = "page up";
        /// the alt key used to go to the previous inventory
        public string PrevInvAltKey = "joystick button 5";
#endif

        [Header("Close Bindings")]
        /// 이 인벤토리가 열릴 때 강제 종료되어야 하는 다른 인벤토리 목록
        public List<string> CloseList;

        public enum ManageButtonsModes { Interactable, SetActive }

        [Header("Buttons")]
        /// 이것이 사실이라면, InputManager는 현재 선택된 슬롯을 기반으로 인벤토리 제어 버튼의 상호작용 가능한 상태를 변경합니다.
        public bool ManageButtons = false;
        /// 버튼을 활성화하기 위해 선택한 모드(상호작용 가능은 버튼의 상호작용 가능 상태를 변경하고, SetActive는 버튼 게임 개체를 활성화/비활성화합니다)
        [MMCondition("ManageButtons", true)]
        public ManageButtonsModes ManageButtonsMode = ManageButtonsModes.SetActive;
        /// the button used to equip or use an item
        [MMCondition("ManageButtons", true)]
        public Button EquipUseButton;
        /// the button used to move an item
        [MMCondition("ManageButtons", true)]
        public Button MoveButton;
        /// the button used to drop an item
        [MMCondition("ManageButtons", true)]
        public Button DropButton;
        /// the button used to equip an item
        [MMCondition("ManageButtons", true)]
        public Button EquipButton;
        /// the button used to use an item
        [MMCondition("ManageButtons", true)]
        public Button UseButton;
        /// the button used to unequip an item
        [MMCondition("ManageButtons", true)]
        public Button UnEquipButton;

        /// 현재 선택중인 슬롯을 반환합니다.
        public InventorySlot CurrentlySelectedInventorySlot { get; set; }

        [Header("State")]
        /// 이것이 사실이면 관련 인벤토리가 열려 있고, 그렇지 않으면 닫혀 있습니다.
        [MMReadOnly]
        public bool InventoryIsOpen;
        public bool NPCInventoryIsOpen; //내가만든 변수
        public bool ButtonPromptIsOpen; //내가만든 변수

        protected CanvasGroup _canvasGroup;
        protected GameObject _currentSelection;
        protected InventorySlot _currentInventorySlot;
        protected List<InventoryHotbar> _targetInventoryHotbars;
        protected InventoryDisplay _currentInventoryDisplay;
        private bool _isEquipUseButtonNotNull;
        private bool _isEquipButtonNotNull;
        private bool _isUseButtonNotNull;
        private bool _isUnEquipButtonNotNull;
        private bool _isMoveButtonNotNull;
        private bool _isDropButtonNotNull;

        protected bool _toggleInventoryKeyPressed;
        protected bool _toggleNPCInventoryKeyPressed;
        protected bool _openInventoryKeyPressed;
        protected bool _closeInventoryKeyPressed;
        protected bool _cancelKeyPressed;
        protected bool _prevInvKeyPressed;
        protected bool _nextInvKeyPressed;
        protected bool _moveKeyPressed;
        protected bool _equipOrUseKeyPressed;
        protected bool _equipKeyPressed;
        protected bool _useKeyPressed;
        protected bool _dropKeyPressed;
        protected bool _buyKeyPressed;
        protected bool _sellKeyPressed;
        protected bool _hotbarInputPressed = false;
        protected bool _quickSlot1;
        protected bool _quickSlot2;
        protected bool _quickSlot3;
        protected bool _quickSlot4;
        protected bool _quickSlot5;
        protected bool _quickSlot6;


        /// <summary>
        /// 처음에는 참고 자료를 수집하고 핫바 목록을 준비합니다.
        /// </summary>
        protected virtual void Start()
        {
            _isDropButtonNotNull = DropButton != null;
            _isMoveButtonNotNull = MoveButton != null;
            _isUnEquipButtonNotNull = UnEquipButton != null;
            _isUseButtonNotNull = UseButton != null;
            _isEquipButtonNotNull = EquipButton != null;
            _isEquipUseButtonNotNull = EquipUseButton != null;
            _currentInventoryDisplay = TargetInventoryDisplay;
            InventoryIsOpen = false;
            NPCInventoryIsOpen = false;
            ButtonPromptIsOpen = false;
            _targetInventoryHotbars = new List<InventoryHotbar>();
            _canvasGroup = GetComponent<CanvasGroup>();
            foreach (InventoryHotbar go in FindObjectsOfType(typeof(InventoryHotbar)) as InventoryHotbar[])
            {
                _targetInventoryHotbars.Add(go);
            }
            if (HideContainerOnStart)
            {
                if (TargetInventoryContainer != null) { TargetInventoryContainer.alpha = 0; }
                if (NPC_InventoryContainer != null) { NPC_InventoryContainer.alpha = 0; }
                if (Player_InventoryContainer != null) { Player_InventoryContainer.alpha = 0; }
                if (Overlay != null) { Overlay.alpha = OverlayInactiveOpacity; }
                EventSystem.current.sendNavigationEvents = false;
                if (_canvasGroup != null)
                {
                    _canvasGroup.blocksRaycasts = false;
                }
            }
        }

        /// <summary>
        /// 매 프레임마다 인벤토리, 단축바에 대한 입력을 확인하고 현재 선택을 확인합니다.
        /// </summary>
        protected virtual void Update()
        {
            HandleInventoryInput();
            HandleHotbarsInput();
            CheckCurrentlySelectedSlot();
            HandleButtons();
        }

        /// <summary>
        /// 매 프레임마다 현재 선택된 객체가 무엇인지 확인하고 저장합니다.
        /// </summary>
        protected virtual void CheckCurrentlySelectedSlot()
        {
            _currentSelection = EventSystem.current.currentSelectedGameObject;
            if (_currentSelection == null)
            {
                return;
            }
            _currentInventorySlot = _currentSelection.gameObject.MMGetComponentNoAlloc<InventorySlot>();
            if (_currentInventorySlot != null)
            {
                CurrentlySelectedInventorySlot = _currentInventorySlot;
            }
        }

        /// <summary>
        /// ManageButtons가 true로 설정된 경우 현재 선택된 슬롯을 기반으로 인벤토리 제어를 상호 작용 가능하게 설정하거나 해제합니다.
        /// </summary>
        protected virtual void HandleButtons()
        {
            if (!ManageButtons)
            {
                return;
            }

            if (CurrentlySelectedInventorySlot != null)
            {
                if (_isUseButtonNotNull)
                {
                    SetButtonState(UseButton, CurrentlySelectedInventorySlot.Usable() && CurrentlySelectedInventorySlot.UseButtonShouldShow());
                }

                if (_isEquipButtonNotNull)
                {
                    SetButtonState(EquipButton, CurrentlySelectedInventorySlot.Equippable() && CurrentlySelectedInventorySlot.EquipButtonShouldShow());
                }

                if (_isEquipUseButtonNotNull)
                {
                    SetButtonState(EquipUseButton, (CurrentlySelectedInventorySlot.Usable() ||
                                                    CurrentlySelectedInventorySlot.Equippable()) && CurrentlySelectedInventorySlot.EquipUseButtonShouldShow());
                }

                if (_isUnEquipButtonNotNull)
                {
                    SetButtonState(UnEquipButton, CurrentlySelectedInventorySlot.Unequippable() && CurrentlySelectedInventorySlot.UnequipButtonShouldShow());
                }

                if (_isMoveButtonNotNull)
                {
                    SetButtonState(MoveButton, CurrentlySelectedInventorySlot.Movable() && CurrentlySelectedInventorySlot.MoveButtonShouldShow());
                }

                if (_isDropButtonNotNull)
                {
                    SetButtonState(DropButton, CurrentlySelectedInventorySlot.Droppable() && CurrentlySelectedInventorySlot.DropButtonShouldShow());
                }
            }
            else
            {
                SetButtonState(UseButton, false);
                SetButtonState(EquipButton, false);
                SetButtonState(EquipUseButton, false);
                SetButtonState(DropButton, false);
                SetButtonState(MoveButton, false);
                SetButtonState(UnEquipButton, false);
            }
        }

        /// <summary>
        /// An internal method used to turn a button on or off
        /// </summary>
        /// <param name="targetButton"></param>
        /// <param name="state"></param>
        protected virtual void SetButtonState(Button targetButton, bool state)
        {
            if (ManageButtonsMode == ManageButtonsModes.Interactable)
            {
                targetButton.interactable = state;
            }
            else
            {
                targetButton.gameObject.SetActive(state);
            }
        }

        /// <summary>
        /// 현재 상태에 따라 인벤토리 패널을 열거나 닫습니다.
        /// </summary>
        public virtual void ToggleInventory()
        {
            if (InventoryIsOpen)
            {
                CloseInventory();
            }
            else
            {
                OpenInventory();
            }
        }

        //NPC 인벤토리 패널을 열거나 닫는다
        public virtual void NPCToggleInventory()
        {
            if (NPCInventoryIsOpen)
            {
                CloseNPCInventory();
            }
            else
            {
                OpenNPCInventory();
            }
        }

        /// <summary>
        /// 인벤토리 패널을 엽니다.
        /// </summary>
        public virtual void OpenInventory()
        {
            if (CloseList.Count > 0)
            {
                foreach (string playerID in CloseList)
                {
                    MMInventoryEvent.Trigger(MMInventoryEventType.InventoryCloseRequest, null, "", null, 0, 0, playerID);
                }
            }

            if (_canvasGroup != null)
            {
                _canvasGroup.blocksRaycasts = true;
            }

            // 우리는 인벤토리를 엽니다
            MMInventoryEvent.Trigger(MMInventoryEventType.InventoryOpens, null, TargetInventoryDisplay.TargetInventoryName, TargetInventoryDisplay.TargetInventory.Content[0], 0, 0, TargetInventoryDisplay.PlayerID);
            MMGameEvent.Trigger("inventoryOpens");
            InventoryIsOpen = true;

            StartCoroutine(MMFade.FadeCanvasGroup(TargetInventoryContainer, 0.2f, 1f));
            StartCoroutine(MMFade.FadeCanvasGroup(Overlay, 0.2f, OverlayActiveOpacity));

            //다른 인벤토리 슬롯창 클릭 안되도록
            NPC_InventoryContainer.GetComponent<CanvasGroup>().blocksRaycasts = false;
            Player_InventoryContainer.GetComponent<CanvasGroup>().blocksRaycasts = false;

            //첫번째 슬롯 가리키기
            TargetInventoryContainer.GetComponentInChildren<InventoryDisplay>().Focus();
        }

        // 상점 패널을 엽니다.
        public virtual void OpenNPCInventory()
        {
            if (CloseList.Count > 0)
            {
                foreach (string playerID in CloseList)
                {
                    MMInventoryEvent.Trigger(MMInventoryEventType.InventoryCloseRequest, null, "", null, 0, 0, playerID); //다른 팝업창 다닫기
                }
            }

            if (_canvasGroup != null)
            {
                _canvasGroup.blocksRaycasts = true; //클릭시 반응 하게 만들기
            }

            MMInventoryEvent.Trigger(MMInventoryEventType.InventoryOpens, null, TargetInventoryDisplay.TargetInventoryName, TargetInventoryDisplay.TargetInventory.Content[0], 0, 0, TargetInventoryDisplay.PlayerID);
            MMGameEvent.Trigger("inventoryOpens");
            NPCInventoryIsOpen = true;

            StartCoroutine(MMFade.FadeCanvasGroup(NPC_InventoryContainer, 0.2f, 1f));
            StartCoroutine(MMFade.FadeCanvasGroup(Player_InventoryContainer, 0.2f, 1f));
            StartCoroutine(MMFade.FadeCanvasGroup(Overlay, 0.2f, OverlayActiveOpacity));

            //현재 플레이어가 가지고 있는돈 업데이트 하기


            //다른 인벤토리 슬롯창 클릭 안되도록
            TargetInventoryContainer.GetComponent<CanvasGroup>().blocksRaycasts = false;

            //첫번째 슬롯 가리키기
            NPC_InventoryContainer.GetComponentInChildren<InventoryDisplay>().Focus();

            //아이템 정보창 안보이게 하기
            for (int i = 0; i < NPC_InventoryContainer.transform.childCount; i++)
            {
                if (NPC_InventoryContainer.transform.GetChild(i).GetComponent<InventoryDetails>() != null)
                {
                    NPC_InventoryContainer.transform.GetChild(i).GetComponent<InventoryDetails>().DisplayDetailsHidden();
                }
            }
        }

        /// <summary>
        /// Closes the inventory panel
        /// </summary>
        public virtual void CloseInventory()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.blocksRaycasts = false;
            }
            // we close our inventory
            MMInventoryEvent.Trigger(MMInventoryEventType.InventoryCloses, null, TargetInventoryDisplay.TargetInventoryName, null, 0, 0, TargetInventoryDisplay.PlayerID);
            MMGameEvent.Trigger("inventoryCloses");
            InventoryIsOpen = false;

            StartCoroutine(MMFade.FadeCanvasGroup(TargetInventoryContainer, 0.2f, 0f));
            StartCoroutine(MMFade.FadeCanvasGroup(Overlay, 0.2f, OverlayInactiveOpacity));

            //다른 인벤토리 슬롯창 클릭 안되도록 했던거 원래대로 돌리기
            NPC_InventoryContainer.GetComponent<CanvasGroup>().blocksRaycasts = true;
            Player_InventoryContainer.GetComponent<CanvasGroup>().blocksRaycasts = true;

            Time.timeScale = 1f;
        }

        //상점 패널을 닫습니다.
        public virtual void CloseNPCInventory()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.blocksRaycasts = false;
            }
            // we close our inventory
            MMInventoryEvent.Trigger(MMInventoryEventType.InventoryCloses, null, TargetInventoryDisplay.TargetInventoryName, null, 0, 0, TargetInventoryDisplay.PlayerID);
            MMGameEvent.Trigger("inventoryCloses");
            NPCInventoryIsOpen = false;

            StartCoroutine(MMFade.FadeCanvasGroup(NPC_InventoryContainer, 0.2f, 0f));
            StartCoroutine(MMFade.FadeCanvasGroup(Player_InventoryContainer, 0.2f, 0f));
            StartCoroutine(MMFade.FadeCanvasGroup(Overlay, 0.2f, OverlayInactiveOpacity));

            //다른 인벤토리 슬롯창 클릭 안되도록 했던거 원래대로 돌리기
            TargetInventoryContainer.GetComponent<CanvasGroup>().blocksRaycasts = true;

            Time.timeScale = 1f;
        }

        //퀵슬롯을 눌렀을때 호출되는 함수
        //public virtual void CheckCurrentlySelectedQuickSlot(KeyCode key)
        //{
        //    if (SceneManager.GetActiveScene().name != "Village" || SceneManager.GetActiveScene().name != "LevelSelect")
        //    {
        //        QuickSlotsDisplay.SelectQuickSlot(key);
        //    }
        //}

        /// <summary>
        /// 재고 관련 입력을 처리하고 이에 따라 조치를 취합니다.
		/// </summary>
		protected virtual void HandleInventoryInput()
        {
            // 현재 재고 표시가 없으면 아무것도 하지 않고 종료합니다.
            if (_currentInventoryDisplay == null)
            {
                return;
            }

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
				_toggleInventoryKeyPressed = ToggleInventoryKey.action.WasPressedThisFrame();
				_openInventoryKeyPressed = OpenInventoryKey.action.WasPressedThisFrame();
				_closeInventoryKeyPressed = CloseInventoryKey.action.WasPressedThisFrame();
				_cancelKeyPressed = CancelKey.action.WasPressedThisFrame();
				_prevInvKeyPressed = PrevInvKey.action.WasPressedThisFrame();
				_nextInvKeyPressed = NextInvKey.action.WasPressedThisFrame();
				_moveKeyPressed = MoveKey.action.WasPressedThisFrame();
				_equipOrUseKeyPressed = EquipOrUseKey.action.WasPressedThisFrame();
				_equipKeyPressed = EquipKey.action.WasPressedThisFrame();
				_useKeyPressed = UseKey.action.WasPressedThisFrame();
				_dropKeyPressed = DropKey.action.WasPressedThisFrame();
#else
            _toggleInventoryKeyPressed = Input.GetKeyDown(ToggleInventoryKey) || Input.GetKeyDown(ToggleInventoryAltKey);
            _toggleNPCInventoryKeyPressed = Input.GetKeyDown(KeyCode.E);
            _openInventoryKeyPressed = Input.GetKeyDown(OpenInventoryKey);
            _closeInventoryKeyPressed = Input.GetKeyDown(CloseInventoryKey);
            _cancelKeyPressed = (Input.GetKeyDown(CancelKey)) || (Input.GetKeyDown(CancelKeyAlt));
            _prevInvKeyPressed = Input.GetKeyDown(PrevInvKey) || Input.GetKeyDown(PrevInvAltKey);
            _nextInvKeyPressed = Input.GetKeyDown(NextInvKey) || Input.GetKeyDown(NextInvAltKey);
            _moveKeyPressed = (Input.GetKeyDown(MoveKey) || Input.GetKeyDown(MoveAltKey));
            _equipOrUseKeyPressed = Input.GetKeyDown(EquipOrUseKey) || Input.GetKeyDown(EquipOrUseAltKey);
            _equipKeyPressed = Input.GetKeyDown(EquipKey) || Input.GetKeyDown(EquipAltKey);
            _useKeyPressed = Input.GetKeyDown(UseKey) || Input.GetKeyDown(UseAltKey);
            _dropKeyPressed = Input.GetKeyDown(DropKey) || Input.GetKeyDown(DropAltKey);
            _quickSlot1 = Input.GetKeyDown(KeyCode.Alpha1);
            _quickSlot2 = Input.GetKeyDown(KeyCode.Alpha2);
            _quickSlot3 = Input.GetKeyDown(KeyCode.Alpha3);
            _quickSlot4 = Input.GetKeyDown(KeyCode.Alpha4);
            _quickSlot5 = Input.GetKeyDown(KeyCode.Alpha5);
            _quickSlot6 = Input.GetKeyDown(KeyCode.Alpha6);
#endif
            if (QuickSlots != null)
            {
                if (_cancelKeyPressed && QuickSlots.isInstalling)
                {
                    //여기에 설치중인 아이템 다시 돌려놔야함
                    Debug.Log("설치 취소");
                    QuickSlotsDisplay.CancleQuickSlotItem(currentlySelectedSlot);
                }

                if(Input.GetMouseButtonDown(0) && QuickSlots.isInstalling)
                {
                    //설치 아이템 사용
                    QuickSlotsDisplay.UseQuickSlotItem(currentlySelectedSlot);
                }

                //설치중이라면 'esc'버튼 빼고 전부 입력안되게
                if (QuickSlots.isInstalling)
                    return;
            }

            //아이템 설치중
            if (_quickSlot1 && Overlay.alpha == 0)
            {
                QuickSlotsDisplay.InstallationQuickSlotItem(KeyCode.Alpha1);
                currentlySelectedSlot = 0;
            }

            if (_quickSlot2 && Overlay.alpha == 0)
            {
                QuickSlotsDisplay.InstallationQuickSlotItem(KeyCode.Alpha2);
                currentlySelectedSlot = 1;
            }

            if (_quickSlot3 && Overlay.alpha == 0)
            {
                QuickSlotsDisplay.InstallationQuickSlotItem(KeyCode.Alpha3);
                currentlySelectedSlot = 2;
            }

            if (_quickSlot4 && Overlay.alpha == 0)
            {
                QuickSlotsDisplay.InstallationQuickSlotItem(KeyCode.Alpha4);
                currentlySelectedSlot = 3;
            }

            if (_quickSlot5 && Overlay.alpha == 0)
            {
                QuickSlotsDisplay.InstallationQuickSlotItem(KeyCode.Alpha5);
                currentlySelectedSlot = 4;
            }

            if (_quickSlot6 && Overlay.alpha == 0)
            {
                QuickSlotsDisplay.InstallationQuickSlotItem(KeyCode.Alpha6);
                currentlySelectedSlot = 5;
            }

            // 사용자가 '인벤토리 전환' 키를 누르면
            if (_toggleInventoryKeyPressed && Player_InventoryContainer.alpha == 0)
            {
                ToggleInventory();
            }

            if (_toggleNPCInventoryKeyPressed && ButtonPromptIsOpen && TargetInventoryContainer.alpha == 0)
            {
                NPCToggleInventory();
            }

            if (_openInventoryKeyPressed)
            {
                OpenInventory();
            }

            if (_closeInventoryKeyPressed)
            {
                CloseInventory();
            }

            if (_cancelKeyPressed)
            {
                if (InventoryIsOpen)
                {
                    CloseInventory();
                }
                if (NPCInventoryIsOpen)
                {
                    CloseNPCInventory();
                }
            }

            // 열릴 때만 입력을 승인했고 인벤토리가 현재 닫혀 있는 경우 아무 작업도 하지 않고 종료합니다.
            if (InputOnlyWhenOpen && !InventoryIsOpen)
            {
                return;
            }

            // previous inventory panel
            if (_prevInvKeyPressed)
            {
                if (_currentInventoryDisplay.GoToInventory(-1) != null)
                {
                    _currentInventoryDisplay = _currentInventoryDisplay.GoToInventory(-1);
                }
            }

            // next inventory panel
            if (_nextInvKeyPressed)
            {
                if (_currentInventoryDisplay.GoToInventory(1) != null)
                {
                    _currentInventoryDisplay = _currentInventoryDisplay.GoToInventory(1);
                }
            }

            // move
            if (_moveKeyPressed)
            {
                if (CurrentlySelectedInventorySlot != null)
                {
                    CurrentlySelectedInventorySlot.Move();
                }
            }

            // equip or use
            if (_equipOrUseKeyPressed)
            {
                EquipOrUse();
            }

            // equip
            if (_equipKeyPressed)
            {
                if (CurrentlySelectedInventorySlot != null)
                {
                    CurrentlySelectedInventorySlot.Equip();
                }
            }

            // use
            if (_useKeyPressed)
            {
                if (CurrentlySelectedInventorySlot != null)
                {
                    CurrentlySelectedInventorySlot.Use();
                }
            }

            // drop
            if (_dropKeyPressed)
            {
                if (CurrentlySelectedInventorySlot != null)
                {
                    CurrentlySelectedInventorySlot.Drop();
                }
            }
        }

        /// <summary>
        /// Checks for hotbar input and acts on it
        /// </summary>
        protected virtual void HandleHotbarsInput()
        {
            if (!InventoryIsOpen)
            {
                foreach (InventoryHotbar hotbar in _targetInventoryHotbars)
                {
                    if (hotbar != null)
                    {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
						_hotbarInputPressed = hotbar.HotbarInputAction.action.WasPressedThisFrame();
#else
                        _hotbarInputPressed = Input.GetKeyDown(hotbar.HotbarKey) || Input.GetKeyDown(hotbar.HotbarAltKey);
#endif

                        if (_hotbarInputPressed)
                        {
                            hotbar.Action();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 장착/사용 버튼을 누르면 두 가지 방법 중 어떤 방법을 호출할지 결정합니다.
        /// </summary>
        public virtual void EquipOrUse()
        {
            if (CurrentlySelectedInventorySlot.Equippable())
            {
                CurrentlySelectedInventorySlot.Equip();
            }
            if (CurrentlySelectedInventorySlot.Usable())
            {
                CurrentlySelectedInventorySlot.Use();
            }
        }

        public virtual void Equip()
        {
            CurrentlySelectedInventorySlot.Equip();
        }

        public virtual void Use()
        {
            CurrentlySelectedInventorySlot.Use();
        }

        public virtual void UnEquip()
        {
            CurrentlySelectedInventorySlot.UnEquip();
        }

        /// <summary>
        /// Triggers the selected slot's move method
        /// </summary>
        public virtual void Move()
        {
            CurrentlySelectedInventorySlot.Move();
        }

        /// <summary>
        /// Triggers the selected slot's drop method
        /// </summary>
        public virtual void Drop()
        {
            CurrentlySelectedInventorySlot.Drop();
        }

        /// <summary>
        /// Catches MMInventoryEvents and acts on them
        /// </summary>
        /// <param name="inventoryEvent">Inventory event.</param>
        public virtual void OnMMEvent(MMInventoryEvent inventoryEvent)
        {
            if (inventoryEvent.PlayerID != TargetInventoryDisplay.PlayerID)
            {
                return;
            }

            if (inventoryEvent.InventoryEventType == MMInventoryEventType.InventoryCloseRequest)
            {
                CloseInventory();
            }
        }

        /// <summary>
        /// On Enable, we start listening for MMInventoryEvents
        /// </summary>
        protected virtual void OnEnable()
        {
            this.MMEventStartListening<MMInventoryEvent>();
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
				ToggleInventoryKey.action.Enable();
				OpenInventoryKey.action.Enable();
				CloseInventoryKey.action.Enable();
				CancelKey.action.Enable();
				MoveKey.action.Enable();
				EquipKey.action.Enable();
				UseKey.action.Enable();
				EquipOrUseKey.action.Enable();
				DropKey.action.Enable();
				NextInvKey.action.Enable();
				PrevInvKey.action.Enable();
#endif
        }

        /// <summary>
        /// On Disable, we stop listening for MMInventoryEvents
        /// </summary>
        protected virtual void OnDisable()
        {
            this.MMEventStopListening<MMInventoryEvent>();
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
				ToggleInventoryKey.action.Disable();
				OpenInventoryKey.action.Disable();
				CloseInventoryKey.action.Disable();
				CancelKey.action.Disable();
				MoveKey.action.Disable();
				EquipKey.action.Disable();
				UseKey.action.Disable();
				EquipOrUseKey.action.Disable();
				DropKey.action.Disable();
				NextInvKey.action.Disable();
				PrevInvKey.action.Disable();
#endif
        }
    }
}