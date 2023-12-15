using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using MoreMountains.InventoryEngine;
using MoreMountains.Feedbacks;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 일반적으로 항목을 선택할 때 발생하는 이벤트로, 청취자에게 어떤 항목이 선택되었는지 알려줍니다.
    /// </summary>
    public struct PickableItemEvent
	{
		public GameObject Picker;
		public PickableItem PickedItem;

        /// <summary>
        /// <see cref="MoreMountains.TopDownEngine.PickableItemEvent"/> 구조체의 새 인스턴스를 초기화합니다.
        /// </summary>
        /// <param name="pickedItem">Picked item.</param>
        public PickableItemEvent(PickableItem pickedItem, GameObject picker) 
		{
			Picker = picker;
			PickedItem = pickedItem;
		}
		static PickableItemEvent e;
		public static void Trigger(PickableItem pickedItem, GameObject picker)
		{
			e.Picker = picker;
			e.PickedItem = pickedItem;
			MMEventManager.TriggerEvent(e);
		}
	}

	/// <summary>
	/// Coin manager
	/// </summary>
	public class PickableItem : TopDownMonoBehaviour
	{
		[Header("Pickable Item")]
        /// 개체를 선택할 때 재생할 피드백
        [Tooltip("개체를 선택할 때 재생할 피드백")]
		public MMFeedbacks PickedMMFeedbacks;
        /// 이것이 사실이라면 선택 시 선택기의 충돌체가 비활성화됩니다.
        [Tooltip("이것이 사실이라면 선택 시 선택기의 충돌체가 비활성화됩니다.")]
		public bool DisableColliderOnPick = false;
        /// true로 설정하면 객체를 선택할 때 객체가 비활성화됩니다.
        [Tooltip("true로 설정하면 객체를 선택할 때 객체가 비활성화됩니다.")]
		public bool DisableObjectOnPick = true;
        /// 객체를 비활성화하기까지의 기간(초), 0인 경우 즉시
        [MMCondition("DisableObjectOnPick", true)]
		[Tooltip("객체를 비활성화하기까지의 기간(초), 0인 경우 즉시")]
		public float DisableDelay = 0f;
        /// true로 설정하면 객체를 선택할 때 객체가 비활성화됩니다.
        [Tooltip("true로 설정하면 객체를 선택할 때 객체가 비활성화됩니다.")]
		public bool DisableModelOnPick = false;
        /// true로 설정하면 대상 객체를 선택할 때 비활성화됩니다.
        [Tooltip("true로 설정하면 대상 객체를 선택할 때 비활성화됩니다.")]
		public bool DisableTargetObjectOnPick = false;
        /// 비활성화TargetObjectOnPick이 true인 경우 선택 시 비활성화할 개체입니다.
        [Tooltip("비활성화TargetObjectOnPick이 true인 경우 선택 시 비활성화할 개체입니다.")]
		[MMCondition("DisableTargetObjectOnPick", true)]
		public GameObject TargetObjectToDisable;
        /// 비활성화TargetObjectOnPick이 true인 경우 대상을 비활성화하기 전의 시간(초)
        [Tooltip("비활성화TargetObjectOnPick이 true인 경우 대상을 비활성화하기 전의 시간(초)")]
		[MMCondition("DisableTargetObjectOnPick", true)]
		public float TargetObjectDisableDelay = 1f;
        /// 이 선택기의 시각적 표현
        [MMCondition("DisableModelOnPick", true)]
		[Tooltip("이 선택기의 시각적 표현")]
		public GameObject Model;

		[Header("Pick Conditions")]
        /// 이것이 사실이라면 이 선택 가능한 항목은 캐릭터 구성 요소가 있는 객체에서만 선택할 수 있습니다. 
        [Tooltip("이것이 사실이라면 이 선택 가능한 항목은 캐릭터 구성 요소가 있는 객체에서만 선택할 수 있습니다.")]
		public bool RequireCharacterComponent = true;
        /// 이것이 사실이라면 이 선택 가능한 항목은 플레이어 유형의 캐릭터 구성 요소가 있는 객체에서만 선택할 수 있습니다.
        [Tooltip("이것이 사실이라면 이 선택 가능한 항목은 플레이어 유형의 캐릭터 구성 요소가 있는 객체에서만 선택할 수 있습니다.")]
		public bool RequirePlayerType = true;

		protected Collider _collider;
		protected Collider2D _collider2D;
		protected GameObject _collidingObject;
		protected Character _character = null;
		protected bool _pickable = false;
		protected ItemPicker _itemPicker = null;
		protected WaitForSeconds _disableDelay;

		protected virtual void Start()
		{
			_disableDelay = new WaitForSeconds(DisableDelay);
			_collider = gameObject.GetComponent<Collider>();
			_collider2D = gameObject.GetComponent<Collider2D>();
			_itemPicker = gameObject.GetComponent<ItemPicker> ();
			PickedMMFeedbacks?.Initialization(this.gameObject);
		}

        /// <summary>
        /// 무언가가 동전과 충돌할 때 트리거됩니다.
        /// </summary>
        /// <param name="collider">Other.</param>
        public virtual void OnTriggerEnter (Collider collider) 
		{
			_collidingObject = collider.gameObject;
			PickItem (collider.gameObject);
		}

        /// <summary>
        /// 무언가가 동전과 충돌할 때 트리거됩니다.
        /// </summary>
        /// <param name="collider">Other.</param>
        public virtual void OnTriggerEnter2D (Collider2D collider) 
		{
			_collidingObject = collider.gameObject;
			PickItem (collider.gameObject);
		}

        /// <summary>
        /// 항목을 선택할 수 있는지 확인하고, 그렇다면 효과를 트리거하고 개체를 비활성화합니다.
        /// </summary>
        public virtual void PickItem(GameObject picker)
		{
			if (CheckIfPickable ())
			{
				Effects ();
				PickableItemEvent.Trigger(this, picker);
				Pick (picker);
				if (DisableColliderOnPick)
				{
					if (_collider != null)
					{
						_collider.enabled = false;
					}
					if (_collider2D != null)
					{
						_collider2D.enabled = false;
					}
				}
				if (DisableModelOnPick && (Model != null))
				{
					Model.gameObject.SetActive(false);
				}
				
				if (DisableObjectOnPick)
				{
                    // 게임오브젝트를 비활성화합니다
                    if (DisableDelay == 0f)
					{
						this.gameObject.SetActive(false);
					}
					else
					{
						StartCoroutine(DisablePickerCoroutine());
					}
				}
				
				if (DisableTargetObjectOnPick && (TargetObjectToDisable != null))
				{
					if (TargetObjectDisableDelay == 0f)
					{
						TargetObjectToDisable.SetActive(false);
					}
					else
					{
						StartCoroutine(DisableTargetObjectCoroutine());
					}
				}			
			} 
		}

		protected virtual IEnumerator DisableTargetObjectCoroutine()
		{
			yield return MMCoroutine.WaitFor(TargetObjectDisableDelay);
			TargetObjectToDisable.SetActive(false);
		}

		protected virtual IEnumerator DisablePickerCoroutine()
		{
			yield return _disableDelay;
			this.gameObject.SetActive(false);
		}

        /// <summary>
        /// 개체를 선택할 수 있는지 확인합니다.
        /// </summary>
        /// <returns><c>true</c>, if if pickable was checked, <c>false</c> otherwise.</returns>
        protected virtual bool CheckIfPickable()
		{
            // 동전과 충돌하는 것이 캐릭터 동작이 아닌 경우 아무것도 하지 않고 종료합니다.
            _character = _collidingObject.GetComponent<Character>();
			if (RequireCharacterComponent)
			{
				if (_character == null)
				{
					return false;
				}
				
				if (RequirePlayerType && (_character.CharacterType != Character.CharacterTypes.Player))
				{
					return false;
				}
			}
			if (_itemPicker != null)
			{
				if  (!_itemPicker.Pickable())
				{
					return false;	
				}
			}

			return true;
		}

        /// <summary>
        /// 다양한 선택 효과를 트리거합니다.
        /// </summary>
        protected virtual void Effects()
		{
			PickedMMFeedbacks?.PlayFeedbacks();
		}

        /// <summary>
        /// 객체를 선택할 때 어떤 일이 발생하는지 설명하려면 이를 재정의하세요.
        /// </summary>
        protected virtual void Pick(GameObject picker)
		{
			
		}
	}
}