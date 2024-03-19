using UnityEngine;
using MoreMountains.Tools;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

namespace MoreMountains.InventoryEngine
{
    /// <summary>
    /// 사용 및 장비에 대한 단축키를 허용하는 전용 키가 연결된 특별한 종류의 인벤토리 디스플레이
    /// </summary>
    public class InventoryHotbar : InventoryDisplay 
	{
        /// 단축바에 있는 개체에 대해 수행할 수 있는 가능한 작업
        public enum HotbarPossibleAction { Use, Equip }
		[Header("Hotbar")]

		[MMInformation("여기서는 핫바의 동작을 활성화하기 위해 핫바가 수신할 키를 정의할 수 있습니다.", MMInformationAttribute.InformationType.Info,false)]
		#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
		
		public InputActionProperty HotbarInputAction = new InputActionProperty(
			new InputAction(
				name: "IM_Demo_LeftKey",
				type: InputActionType.Button, 
				binding: "", 
				interactions: "Press(behavior=2)"));
		
		#else
		/// the key associated to the hotbar, that will trigger the action when pressed
		public string HotbarKey;
		/// the alt key associated to the hotbar
		public string HotbarAltKey;	
		#endif
		/// the action associated to the key or alt key press
		public HotbarPossibleAction ActionOnKey	;

		/// <summary>
		/// Executed when the key or alt key gets pressed, triggers the specified action
		/// </summary>
		public virtual void Action()
		{
			for (int i = TargetInventory.Content.Length-1 ; i>=0 ; i--)
			{
				if (!InventoryItem.IsNull(TargetInventory.Content[i]))
				{
					if ((ActionOnKey == HotbarPossibleAction.Equip) && (SlotContainer[i] != null))
					{
						SlotContainer[i].Equip();
					}
					if ((ActionOnKey == HotbarPossibleAction.Use) && (SlotContainer[i] != null))
					{
						SlotContainer[i].Use();
					}
					return;
				}
			}
		}
		
		/// <summary>
		/// On Enable, we start listening for MMInventoryEvents
		/// </summary>
		protected override void OnEnable()
		{
			base.OnEnable();
			#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
			HotbarInputAction.action.Enable();
			#endif
		}

		/// <summary>
		/// On Disable, we stop listening for MMInventoryEvents
		/// </summary>
		protected override void OnDisable()
		{
			base.OnDisable();
			#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
			HotbarInputAction.action.Disable();
			#endif
			
		}
	}
}