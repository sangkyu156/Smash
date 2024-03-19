using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using MoreMountains.InventoryEngine;
using System.Collections.Generic;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 구성 요소를 장면의 개체에 추가하여 상자처럼 작동하도록 합니다. 열려면 키로 작동하는 영역이 필요하고, 내용을 채우려면 항목 선택기가 필요합니다.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Items/InventoryEngineChest")]
	public class InventoryEngineChest : TopDownMonoBehaviour 
	{
		protected Animator _animator;
		protected ItemPicker[] _itemPickerList;

		/// <summary>
		/// On start we grab our animator and list of item pickers
		/// </summary>
		protected virtual void Start()
		{
			_animator = GetComponent<Animator> ();
			_itemPickerList = GetComponents<ItemPicker> ();
		}

		/// <summary>
		/// A public method to open the chest, usually called by the associated key operated zone
		/// </summary>
		public virtual void OpenChest()
		{
			TriggerOpeningAnimation ();
			PickChestContents ();
		}

		/// <summary>
		/// Triggers the opening animation.
		/// </summary>
		protected virtual void TriggerOpeningAnimation()
		{
			if (_animator == null)
			{
				return;
			}
			_animator.SetTrigger ("OpenChest");
		}

        /// <summary>
        /// 연결된 선택기에 있는 모든 항목을 플레이어의 인벤토리에 넣습니다.
        /// </summary>
        protected virtual void PickChestContents()
		{
			if (_itemPickerList.Length == 0)
			{
				return;
			}
			foreach (ItemPicker picker in _itemPickerList)
			{
				picker.Pick ();
			}
		}
			
	}
}