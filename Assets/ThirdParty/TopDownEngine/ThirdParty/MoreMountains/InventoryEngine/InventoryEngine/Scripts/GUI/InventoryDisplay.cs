using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.EventSystems;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MoreMountains.InventoryEngine
{	
	[SelectionBase]
    /// <summary>
    /// 사용자가 인벤토리와 상호작용할 수 있도록 인벤토리의 시각적 표현을 처리하는 구성요소
    /// </summary>
    public class InventoryDisplay : MonoBehaviour, MMEventListener<MMInventoryEvent>
	{
		[Header("Binding")]
        /// 표시할 인벤토리 이름
        [MMInformation("InventoryDisplay는 Inventory에 포함된 데이터의 시각화를 처리하는 구성 요소입니다. 표시하려는 인벤토리의 이름을 지정하여 시작하세요.", MMInformationAttribute.InformationType.Info,false)]
		public string TargetInventoryName = "MainInventory";
		public string PlayerID = "Player1";

		protected Inventory _targetInventory = null;

        /// <summary>
        /// 이름에 따라 대상 인벤토리를 확보합니다.
        /// </summary>
        /// <value>대상 인벤토리</value>
        public Inventory TargetInventory 
		{ 
			get 
			{ 
				if (TargetInventoryName==null)
				{
					return null;
				}
				if (_targetInventory == null)
				{
					foreach (Inventory inventory in UnityEngine.Object.FindObjectsOfType<Inventory>())
					{
						if ((inventory.name == TargetInventoryName) && (inventory.PlayerID == PlayerID))
						{
							_targetInventory = inventory;
						}
					}	
				}
				return _targetInventory;
			}
		}
		
		
		public struct ItemQuantity
		{
			public string ItemID;
			public int Quantity;

			public ItemQuantity(string itemID, int quantity)
			{
				ItemID = itemID;
				Quantity = quantity;
			}
		}

		[Header("Inventory Size")]
        /// 표시할 행 수
        [MMInformation("InventoryDisplay는 각각 하나의 항목이 포함된 슬롯에 인벤토리 데이터를 표시하고 그리드에 표시합니다. 여기에서 원하는 슬롯의 행과 열 수를 설정할 수 있습니다. 설정에 만족하면 이 검사기 하단에 있는 '자동 설정' 버튼을 눌러 변경 사항을 확인할 수 있습니다.", MMInformationAttribute.InformationType.Info,false)]
		public int NumberOfRows = 3;
        /// 표시할 열 수
        public int NumberOfColumns = 2;

        /// 이 인벤토리의 총 슬롯 수
        public int InventorySize { get { return NumberOfRows * NumberOfColumns; } set {} }		

		[Header("Equipment")]
		[MMInformation("장비 인벤토리의 내용이 표시되면 여기에 선택 인벤토리를 바인딩해야 합니다. 선택 인벤토리는 장비에 대한 아이템을 선택하는 인벤토리입니다. 일반적으로 선택 인벤토리가 주 인벤토리입니다. 다시 말하지만, 이것이 장비 인벤토리인 경우 승인하려는 항목 클래스를 지정할 수 있습니다.", MMInformationAttribute.InformationType.Info,false)]
		public InventoryDisplay TargetChoiceInventory;
		public ItemClasses ItemClass;

		[Header("Behaviour")]
        /// 이것이 사실이라면 객체가 포함되어 있지 않더라도 슬롯을 그릴 것입니다. 그렇지 않으면 우리는 그것들을 그리지 않습니다
        [MMInformation("이것이 사실이라면 객체가 포함되어 있지 않더라도 슬롯을 그릴 것입니다. 그렇지 않으면 우리는 그것들을 그리지 않습니다.", MMInformationAttribute.InformationType.Info,false)]
		public bool DrawEmptySlots=true;
        /// 이것이 사실이라면 플레이어는 이동 버튼을 사용하여 다른 인벤토리의 개체를 이 인벤토리로 이동할 수 있습니다.
        [MMInformation("이것이 사실이라면 플레이어는 이동 버튼을 사용하여 다른 인벤토리의 개체를 이 인벤토리로 이동할 수 있습니다.", MMInformationAttribute.InformationType.Info,false)]
		public bool AllowMovingObjectsToThisInventory = false;

		[Header("Inventory Padding")]
		[MMInformation("여기서 인벤토리 패널의 테두리와 슬롯 사이의 패딩을 정의할 수 있습니다.", MMInformationAttribute.InformationType.Info,false)]
        /// 인벤토리 패널 상단과 첫 번째 슬롯 사이의 내부 여백
        public int PaddingTop = 20;
        /// 인벤토리 패널 오른쪽과 마지막 슬롯 사이의 내부 여백
        public int PaddingRight = 20;
        /// 인벤토리 패널 하단과 마지막 슬롯 사이의 내부 여백
        public int PaddingBottom = 20;
        /// 인벤토리 패널 왼쪽과 첫 번째 슬롯 사이의 내부 여백
        public int PaddingLeft = 20;

		[Header("Slots")]
		[MMInformation(
            "이 인벤토리 하단에 있는 자동 설정 버튼을 누르면 InventoryDisplay가 인벤토리의 콘텐츠를 표시할 준비가 된 슬롯으로 채워집니다. 여기에서 슬롯의 크기, 여백을 정의하고 슬롯이 비어 있거나 채워졌을 때 사용할 이미지를 정의할 수 있습니다.",
			MMInformationAttribute.InformationType.Info, false)]
        /// 슬롯으로 사용할 게임 개체입니다. 비워두면 런타임 시 자동으로 생성됩니다.
        public InventorySlot SlotPrefab;
        /// 슬롯의 수평 및 수직 크기
        public Vector2 SlotSize = new Vector2(50,50);
        /// 각 슬롯의 아이콘 크기
        public Vector2 IconSize = new Vector2(30,30);
        /// 슬롯 행과 열 사이에 적용할 수평 및 수직 여백
        public Vector2 SlotMargin = new Vector2(5,5);
        /// 슬롯이 비어있을 때 각 슬롯의 배경으로 설정할 이미지입니다.
        public Sprite EmptySlotImage;
        /// 슬롯이 비어 있지 않을 때 각 슬롯의 배경으로 설정할 이미지입니다.
        public Sprite FilledSlotImage;
        /// 슬롯 하이라이트 시 각 슬롯의 배경으로 설정할 이미지입니다.
        public Sprite HighlightedSlotImage;
        /// 슬롯을 눌렀을 때 각 슬롯의 배경으로 설정할 이미지입니다.
        public Sprite PressedSlotImage;
        /// 슬롯 비활성화 시 각 슬롯의 배경으로 설정할 이미지
        public Sprite DisabledSlotImage;
        /// 슬롯에 있는 아이템이 이동할 때 각 슬롯의 배경으로 설정할 이미지입니다.
        public Sprite MovedSlotImage;
        /// 이미지 유형(슬라이스, 일반, 타일...)
        public Image.Type SlotImageType;
		
		[Header("Navigation")]
		[MMInformation("여기에서 내장 내비게이션 시스템(플레이어가 키보드 화살표나 조이스틱을 사용하여 슬롯에서 슬롯으로 이동할 수 있음)을 사용할지 여부와 장면이 시작될 때 이 인벤토리 표시 패널에 초점을 맞출지 여부를 결정할 수 있습니다. . 일반적으로 주요 인벤토리에 집중하기를 원할 것입니다.", MMInformationAttribute.InformationType.Info,false)]
        /// true인 경우 엔진은 키보드나 게임패드를 사용하여 다양한 슬롯을 탐색하기 위한 바인딩을 자동으로 생성합니다.
        public bool EnableNavigation = true;
        /// 이것이 사실이라면 이 인벤토리 표시는 시작 시 초점을 맞춥니다.
        public bool GetFocusOnStart = false;

		[Header("Title Text")]
		[MMInformation("여기에서 인벤토리 표시 패널 옆에 제목을 표시할지 여부를 결정할 수 있습니다. 이를 위해 제목, 글꼴, 글꼴 크기, 색상 등을 지정할 수 있습니다.", MMInformationAttribute.InformationType.Info,false)]
        /// true인 경우 패널 제목이 표시됩니다.
        public bool DisplayTitle=true;
        /// 표시될 인벤토리의 제목
        public string Title;
        /// 수량을 표시하는 데 사용되는 글꼴
        public Font TitleFont;
        /// 사용할 글꼴 크기
        public int TitleFontSize=20;
        /// 수량을 표시할 색상
        public Color TitleColor = Color.black;
        /// 패딩(슬롯 가장자리까지의 거리)
        public Vector3 TitleOffset=Vector3.zero;
        /// 수량을 표시해야 하는 위치
        public TextAnchor TitleAlignment = TextAnchor.LowerRight;

		[Header("Quantity Text")]
		[MMInformation("인벤토리에 쌓인 항목(동전이나 물약과 같이 단일 슬롯에 특정 종류의 항목이 두 개 이상)으로 포함된 경우 항목 아이콘 옆에 수량을 표시하고 싶을 것입니다. 이를 위해 여기서 사용할 글꼴, 색상, 해당 수량 텍스트의 위치를 ​​지정할 수 있습니다.", MMInformationAttribute.InformationType.Info,false)]
        /// 수량을 표시하는 데 사용되는 글꼴
        public Font QtyFont;
        /// 사용할 글꼴 크기
        public int QtyFontSize=12;
        /// 수량을 표시할 색상
        public Color QtyColor = Color.black;
        /// 패딩(슬롯 가장자리까지의 거리)
        public float QtyPadding=10f;
        /// 수량을 표시해야 하는 위치
        public TextAnchor QtyAlignment = TextAnchor.LowerRight;

		[Header("Extra Inventory Navigation")]
		[MMInformation("InventoryInputManager에는 한 인벤토리 패널에서 다음 패널로 이동할 수 있는 컨트롤이 제공됩니다. 여기에서는 이전 또는 다음 인벤토리 버튼을 누를 때 플레이어가 이 패널에서 이동해야 하는 인벤토리를 정의할 수 있습니다.", MMInformationAttribute.InformationType.Info,false)]
		public InventoryDisplay PreviousInventory;
		public InventoryDisplay NextInventory;

        /// 행과 열에 인벤토리를 표시하는 데 사용되는 그리드 레이아웃
        public GridLayoutGroup InventoryGrid { get; protected set; }
        /// 인벤토리 이름을 표시하는 데 사용되는 게임오브젝트
        public InventoryDisplayTitle InventoryTitle { get; protected set; }
        /// 메인 패널
        public RectTransform InventoryRectTransform { get { return GetComponent<RectTransform>(); }}
        /// 내부 슬롯 목록
        public List<InventorySlot> SlotContainer { get; protected set; }
        /// 작업 후 포커스가 반환되어야 하는 인벤토리
        public InventoryDisplay ReturnInventory { get; protected set; }
        /// 이 재고 표시가 열려 있는지 여부
        public bool IsOpen { get; protected set; }
		
		public bool InEquipSelection { get; set; }

        /// 현재 이동 중인 항목

        public static InventoryDisplay CurrentlyBeingMovedFromInventoryDisplay;
		public static int CurrentlyBeingMovedItemIndex = -1;

		protected List<ItemQuantity> _contentLastUpdate;	
		protected List<int> _comparison;	
		protected SpriteState _spriteState = new SpriteState();
		protected InventorySlot _currentlySelectedSlot;
		protected InventorySlot _slotPrefab = null;

		//테스트

        /// <summary>
        /// 재고 디스플레이 생성 및 설정(보통 검사관의 전용 버튼을 통해 호출됨)
        /// </summary>
        public virtual void SetupInventoryDisplay()
		{
			if (TargetInventoryName == "")
			{
				Debug.LogError("The " + this.name + " Inventory Display doesn't have a TargetInventoryName set. You need to set one from its inspector, matching an Inventory's name.");
				return;
			}

			if (TargetInventory == null)
			{
				Debug.LogError("The " + this.name + " Inventory Display couldn't find a TargetInventory. You either need to create an inventory with a matching inventory name (" + TargetInventoryName + "), or set that TargetInventoryName to one that exists.");
				return;
			}

            // 사운드 플레이어 구성요소도 있는 경우 해당 구성요소도 설정합니다.
            if (this.gameObject.MMGetComponentNoAlloc<InventorySoundPlayer>() != null)
			{
				this.gameObject.MMGetComponentNoAlloc<InventorySoundPlayer> ().SetupInventorySoundPlayer ();
			}

			InitializeSprites();
			AddGridLayoutGroup();
			DrawInventoryTitle();
			ResizeInventoryDisplay ();
			DrawInventoryContent();
		}

        /// <summary>
        /// Awake에서는 인벤토리 내용을 추적하는 데 사용되는 다양한 목록을 초기화합니다.
        /// </summary>
        protected virtual void Awake()
		{
			_contentLastUpdate = new List<ItemQuantity>();		
			SlotContainer = new List<InventorySlot>() ;		
			_comparison = new List<int>();
			if (!TargetInventory.Persistent)
			{
				RedrawInventoryDisplay (); 	
			}
		}

		/// <summary>
		/// 필요할 때 인벤토리 표시 내용을 다시 그립니다(보통 대상 인벤토리가 변경된 후).
		/// </summary>
		protected virtual void RedrawInventoryDisplay()
		{
			InitializeSprites();
			AddGridLayoutGroup();
			DrawInventoryContent();		
			FillLastUpdateContent();	
		}

        /// <summary>
        /// 스프라이트를 초기화합니다.
        /// </summary>
        protected virtual void InitializeSprites()
		{
            // 다양한 버튼 상태를 지정하기 위해 spriteState를 만듭니다.
            _spriteState.disabledSprite = DisabledSlotImage;
			_spriteState.selectedSprite = HighlightedSlotImage;
			_spriteState.highlightedSprite = HighlightedSlotImage;
			_spriteState.pressedSprite = PressedSlotImage;
		}

        /// <summary>
        /// 인벤토리 제목 하위 개체를 추가하고 설정합니다.
        /// </summary>
        protected virtual void DrawInventoryTitle()
		{
			if (!DisplayTitle)
			{
				return;
			}
			if (GetComponentInChildren<InventoryDisplayTitle>() != null)
			{
				if (!Application.isPlaying)
				{
					foreach (InventoryDisplayTitle title in GetComponentsInChildren<InventoryDisplayTitle>())
					{
						DestroyImmediate(title.gameObject);
					}
				}
				else
				{
					foreach (InventoryDisplayTitle title in GetComponentsInChildren<InventoryDisplayTitle>())
					{
						Destroy(title.gameObject);
					}
				}
			}
			GameObject inventoryTitle = new GameObject();
			InventoryTitle = inventoryTitle.AddComponent<InventoryDisplayTitle>();
			inventoryTitle.name="InventoryTitle";
			inventoryTitle.GetComponent<RectTransform>().SetParent(this.transform);
			inventoryTitle.GetComponent<RectTransform>().sizeDelta = GetComponent<RectTransform>().sizeDelta;
			inventoryTitle.GetComponent<RectTransform>().localPosition = TitleOffset;
			inventoryTitle.GetComponent<RectTransform>().localScale = Vector3.one;
			InventoryTitle.text = Title;
			InventoryTitle.color = TitleColor;
			InventoryTitle.font = TitleFont;
			InventoryTitle.fontSize = TitleFontSize;
			InventoryTitle.alignment = TitleAlignment;
			InventoryTitle.raycastTarget = false;
		}

        /// <summary>
        /// 그리드 레이아웃 그룹이 아직 없는 경우 추가합니다.
        /// </summary>
        protected virtual void AddGridLayoutGroup()
		{
			if (GetComponentInChildren<InventoryDisplayGrid>() == null)
			{
				GameObject inventoryGrid=new GameObject("InventoryDisplayGrid");
				inventoryGrid.transform.parent=this.transform;
				inventoryGrid.transform.position=transform.position;
				inventoryGrid.transform.localScale=Vector3.one;
				inventoryGrid.AddComponent<InventoryDisplayGrid>();
				InventoryGrid = inventoryGrid.AddComponent<GridLayoutGroup>();
			}
			if (InventoryGrid == null)
			{
				InventoryGrid = GetComponentInChildren<GridLayoutGroup>();
			}
			InventoryGrid.padding.top = PaddingTop;
			InventoryGrid.padding.right = PaddingRight;
			InventoryGrid.padding.bottom = PaddingBottom;
			InventoryGrid.padding.left = PaddingLeft;
			InventoryGrid.cellSize = SlotSize;
			InventoryGrid.spacing = SlotMargin;
		}

        /// <summary>
        /// 행/열 수, 패딩 및 여백을 고려하여 인벤토리 패널의 크기를 조정합니다.
        /// </summary>
        protected virtual void ResizeInventoryDisplay()
		{

			float newWidth = PaddingLeft + SlotSize.x * NumberOfColumns + SlotMargin.x * (NumberOfColumns-1) + PaddingRight;
			float newHeight = PaddingTop + SlotSize.y * NumberOfRows + SlotMargin.y * (NumberOfRows-1) + PaddingBottom;

			TargetInventory.ResizeArray(NumberOfRows * NumberOfColumns);	

			Vector2 newSize= new Vector2(newWidth,newHeight);
			InventoryRectTransform.sizeDelta = newSize;
			InventoryGrid.GetComponent<RectTransform>().sizeDelta = newSize;
		}

        /// <summary>
        /// 인벤토리의 내용(슬롯 및 아이콘)을 그립니다.
        /// </summary>
        protected virtual void DrawInventoryContent ()             
		{            
			if (SlotContainer != null)
			{
				SlotContainer.Clear();
			}
			else
			{
				SlotContainer = new List<InventorySlot>();
			}
            // 스프라이트를 초기화합니다
            if (EmptySlotImage==null)
			{
				InitializeSprites();
			}
            // 기존 슬롯을 모두 제거합니다.
            foreach (InventorySlot slot in transform.GetComponentsInChildren<InventorySlot>())
			{	 			
				if (!Application.isPlaying)
				{
					DestroyImmediate (slot.gameObject);
				}
				else
				{
					Destroy(slot.gameObject);
				}				
			}
            // 각 슬롯에 대해 슬롯과 해당 콘텐츠를 생성합니다.
            for (int i = 0; i < TargetInventory.Content.Length; i ++) 
			{    
				DrawSlot(i);
			}

			if (_slotPrefab != null)
			{
				if (Application.isPlaying)
				{
					Destroy(_slotPrefab.gameObject);
					_slotPrefab = null;
				}
				else
				{
					DestroyImmediate(_slotPrefab.gameObject);
					_slotPrefab = null;
				}	
			}

			if (EnableNavigation)
			{
				SetupSlotNavigation();
			}
		}

        /// <summary>
        /// 콘텐츠가 변경된 경우 인벤토리 패널을 다시 그립니다.
        /// </summary>
        protected virtual void ContentHasChanged()
		{
			if (!(Application.isPlaying))
			{
				AddGridLayoutGroup();
				DrawInventoryContent();
				#if UNITY_EDITOR
				EditorUtility.SetDirty(gameObject);
				#endif
			}
			else
			{
				if (!DrawEmptySlots)
				{
					DrawInventoryContent();
				}
				UpdateInventoryContent();
			}
		}

        /// <summary>
        /// 업데이트의 마지막 내용을 채웁니다.
        /// </summary>
        protected virtual void FillLastUpdateContent()		
		{		
			_contentLastUpdate.Clear();		
			_comparison.Clear();
			for (int i = 0; i < TargetInventory.Content.Length; i ++) 		
			{  		
				if (!InventoryItem.IsNull(TargetInventory.Content[i]))
				{
					_contentLastUpdate.Add(new ItemQuantity(TargetInventory.Content[i].ItemID, TargetInventory.Content[i].Quantity));
				}
				else
				{
					_contentLastUpdate.Add(new ItemQuantity(null,0));	
				}	
			}	
		}

        /// <summary>
        /// 인벤토리의 내용(슬롯 및 아이콘)을 그립니다.
        /// </summary>
        protected virtual void UpdateInventoryContent ()             
		{      
			if (_contentLastUpdate == null || _contentLastUpdate.Count == 0)
			{
				FillLastUpdateContent();
			}

            // 현재 콘텐츠와 저장소에 있는 콘텐츠를 비교하여 변경 사항을 찾습니다.
            for (int i = 0; i < TargetInventory.Content.Length; i ++) 
			{
				if ((TargetInventory.Content[i] == null) && (_contentLastUpdate[i].ItemID != null))
				{
					_comparison.Add(i);
				}
				if ((TargetInventory.Content[i] != null) && (_contentLastUpdate[i].ItemID == null))
				{
					_comparison.Add(i);
				}
				if ((TargetInventory.Content[i] != null) && (_contentLastUpdate[i].ItemID != null))
				{
					if ((TargetInventory.Content[i].ItemID != _contentLastUpdate[i].ItemID) || (TargetInventory.Content[i].Quantity != _contentLastUpdate[i].Quantity))
					{
						_comparison.Add(i);
					}
				}
			}
			if (_comparison.Count>0)
			{
				foreach (int comparison in _comparison)
				{
					UpdateSlot(comparison);
				}
			} 	    
			FillLastUpdateContent();
		}

        /// <summary>
        /// 슬롯의 내용과 모양을 업데이트합니다.
        /// </summary>
        /// <param name="i">The index.</param>
        protected virtual void UpdateSlot(int i)
		{
			
			if (SlotContainer.Count < i)
			{
				Debug.LogWarning ("인벤토리 표시가 제대로 초기화되지 않은 것 같습니다. Load 이벤트를 트리거하지 않는 경우 해당 검사기에서 인벤토리를 비지속성으로 표시할 수 있습니다. 그렇지 않으면 저장된 인벤토리를 재설정하고 비운 후 다시 시도할 수 있습니다.");
			}

			if (SlotContainer.Count <= i)
			{
				return;
			}
			
			if (SlotContainer[i] == null)
			{
				return;
			}
            // 슬롯의 bg 이미지를 업데이트합니다.
            if (!InventoryItem.IsNull(TargetInventory.Content[i]))
			{
				SlotContainer[i].TargetImage.sprite = FilledSlotImage;   
			}
			else
			{
				SlotContainer[i].TargetImage.sprite = EmptySlotImage; 
			}
			if (!InventoryItem.IsNull(TargetInventory.Content[i]))
			{
                // 아이콘을 다시 그립니다
                SlotContainer[i].DrawIcon(TargetInventory.Content[i],i);
			}
			else
			{
				SlotContainer[i].DrawIcon(null,i);
			}
		}

        /// <summary>
        /// 모든 슬롯 생성에 사용할 슬롯 프리팹을 생성합니다.
        /// </summary>
        protected virtual void InitializeSlotPrefab()
		{
			if (SlotPrefab != null)
			{
				_slotPrefab = Instantiate(SlotPrefab);
			}
			else
			{
				GameObject newSlot = new GameObject();
				newSlot.AddComponent<RectTransform>();

				newSlot.AddComponent<Image> ();
				newSlot.MMGetComponentNoAlloc<Image> ().raycastTarget = true;

				_slotPrefab = newSlot.AddComponent<InventorySlot> ();
				_slotPrefab.transition = Selectable.Transition.SpriteSwap;

				Navigation explicitNavigation = new Navigation ();
				explicitNavigation.mode = Navigation.Mode.Explicit;
				_slotPrefab.GetComponent<InventorySlot> ().navigation = explicitNavigation;

				_slotPrefab.interactable = true;

				newSlot.AddComponent<CanvasGroup> ();
				newSlot.MMGetComponentNoAlloc<CanvasGroup> ().alpha = 1;
				newSlot.MMGetComponentNoAlloc<CanvasGroup> ().interactable = true;
				newSlot.MMGetComponentNoAlloc<CanvasGroup> ().blocksRaycasts = true;
				newSlot.MMGetComponentNoAlloc<CanvasGroup> ().ignoreParentGroups = false;
				
				// we add the icon
				GameObject itemIcon = new GameObject("Slot Icon", typeof(RectTransform));
				itemIcon.transform.SetParent(newSlot.transform);
				UnityEngine.UI.Image itemIconImage = itemIcon.AddComponent<Image>();
				_slotPrefab.IconImage = itemIconImage;
				RectTransform itemRectTransform = itemIcon.GetComponent<RectTransform>();
				itemRectTransform.localPosition = Vector3.zero;
				itemRectTransform.localScale = Vector3.one;
				MMGUI.SetSize(itemRectTransform, IconSize);

				// we add the quantity placeholder
				GameObject textObject = new GameObject("Slot Quantity", typeof(RectTransform));
				textObject.transform.SetParent(itemIcon.transform);
				Text textComponent = textObject.AddComponent<Text>();
				_slotPrefab.QuantityText = textComponent;
				textComponent.font = QtyFont;
				textComponent.fontSize = QtyFontSize;
				textComponent.color = QtyColor;
				textComponent.alignment = QtyAlignment;
				RectTransform textObjectRectTransform = textObject.GetComponent<RectTransform>();
				textObjectRectTransform.localPosition = Vector3.zero;
				textObjectRectTransform.localScale = Vector3.one;
				MMGUI.SetSize(textObjectRectTransform, (SlotSize - Vector2.one * QtyPadding)); 

				_slotPrefab.name = "SlotPrefab";
			}
		}

        /// <summary>
        /// 슬롯과 해당 내용(아이콘, 수량...)을 그립니다.
        /// </summary>
        /// <param name="i">The index.</param>
        protected virtual void DrawSlot(int i)
		{
			if (!DrawEmptySlots)
			{
				if (InventoryItem.IsNull(TargetInventory.Content[i]))
				{
					return;
				}
			}

			if ((_slotPrefab == null) || (!_slotPrefab.isActiveAndEnabled))
			{
				InitializeSlotPrefab ();
			}

			InventorySlot theSlot = Instantiate(_slotPrefab);

			theSlot.transform.SetParent(InventoryGrid.transform);
			theSlot.TargetRectTransform.localScale = Vector3.one;
			theSlot.transform.position = transform.position;
			theSlot.name = "Slot "+i;

			// we add the background image
			if (!InventoryItem.IsNull(TargetInventory.Content[i]))
			{
				theSlot.TargetImage.sprite = FilledSlotImage;   
			}
			else
			{
				theSlot.TargetImage.sprite = EmptySlotImage;      	
			}
			theSlot.TargetImage.type = SlotImageType; 
			theSlot.spriteState=_spriteState;
			theSlot.MovedSprite=MovedSlotImage;
			theSlot.ParentInventoryDisplay = this;
			theSlot.Index=i;

			SlotContainer.Add(theSlot);	

			theSlot.gameObject.SetActive(true)	;

			theSlot.DrawIcon(TargetInventory.Content[i],i);
		}

        /// <summary>
        /// 사용자가 왼쪽/오른쪽/위/아래 화살표를 사용하여 이동할 수 있도록 Unity의 GUI 내장 시스템을 사용하여 슬롯 탐색을 설정합니다.
        /// </summary>
        protected virtual void SetupSlotNavigation()
		{
			if (!EnableNavigation)
			{
				return;
			}

			for (int i=0; i<SlotContainer.Count;i++)
			{
				if (SlotContainer[i]==null)
				{
					return;
				}
				Navigation navigation = SlotContainer[i].navigation;
                // 우리는 올라갈 때 어디로 갈지 정한다
                if (i - NumberOfColumns >= 0) 
				{
					navigation.selectOnUp = SlotContainer[i-NumberOfColumns];
				}
				else
				{
					navigation.selectOnUp=null;
				}
                // 내려갈 때 어디로 갈지 우리가 정해요
                if (i+NumberOfColumns < SlotContainer.Count) 
				{
					navigation.selectOnDown = SlotContainer[i+NumberOfColumns];
				}
				else
				{
					navigation.selectOnDown=null;
				}
                // 우리는 왼쪽으로 갈 때 어디로 갈지 결정합니다
                if ((i%NumberOfColumns != 0) && (i>0))
				{
					navigation.selectOnLeft = SlotContainer[i-1];
				}
				else
				{
					navigation.selectOnLeft=null;
				}
                // 우리는 오른쪽으로 갈 때 어디로 가야할지 결정합니다
                if (((i+1)%NumberOfColumns != 0)  && (i<SlotContainer.Count - 1))
				{
					navigation.selectOnRight = SlotContainer[i+1];
				}
				else
				{
					navigation.selectOnRight=null;
				}
				SlotContainer[i].navigation = navigation;
			}
		}

        /// <summary>		
        /// 인벤토리의 첫 번째 항목에 초점을 설정합니다.		
        /// </summary>		
        public virtual void Focus()		
		{
            if (!EnableNavigation)
			{
				return;
			}
			
			if (SlotContainer.Count > 0)
			{
				SlotContainer[0].Select();
			}		

			if (EventSystem.current.currentSelectedGameObject == null)
			{
				InventorySlot newSlot = transform.GetComponentInChildren<InventorySlot>();
				if (newSlot != null)
				{
					EventSystem.current.SetSelectedGameObject (newSlot.gameObject);	
				}
			}			
		}

        /// <summary>
        /// 현재 선택된 인벤토리 슬롯을 반환합니다.
        /// </summary>
        /// <returns>The selected inventory slot.</returns>
        public virtual InventorySlot CurrentlySelectedInventorySlot()
		{
			return _currentlySelectedSlot;
		}

        /// <summary>
        /// 현재 선택된 슬롯을 설정합니다.
        /// </summary>
        /// <param name="slot">Slot.</param>
        public virtual void SetCurrentlySelectedSlot(InventorySlot slot)
		{
			_currentlySelectedSlot = slot;
		}

        /// <summary>
        /// 매개변수에 전달된 int 방향에 따라 이전(-1) 또는 다음(1) 인벤토리로 이동합니다.
        /// </summary>
        /// <param name="direction">Direction.</param>
        public virtual InventoryDisplay GoToInventory(int direction)
		{
			if (direction==-1)
			{
				if (PreviousInventory==null)
				{
					return null;
				}
				PreviousInventory.Focus();
				return PreviousInventory;
			}
			else
			{
				if (NextInventory==null)
				{
					return null;
				}
				NextInventory.Focus();	
				return NextInventory;			
			}
		}

        /// <summary>
        /// 반품 재고 표시를 설정합니다.
        /// </summary>
        /// <param name="inventoryDisplay">Inventory display.</param>
        public virtual void SetReturnInventory(InventoryDisplay inventoryDisplay)
		{
			ReturnInventory = inventoryDisplay;
		}

        /// <summary>
        /// 가능하다면 현재 반품 인벤토리 포커스로 포커스를 되돌립니다. (보통 아이템 장착 후)
        /// </summary>
        public virtual void ReturnInventoryFocus()
		{
			if (ReturnInventory == null)
			{
				return;
			}
			else
			{
				InEquipSelection = false;
				ResetDisabledStates();
				ReturnInventory.Focus();
				ReturnInventory = null;
			}
		}

        /// <summary>
        /// 특정 클래스의 슬롯을 제외한 인벤토리 표시의 모든 슬롯을 비활성화합니다.
        /// </summary>
        /// <param name="itemClass">Item class.</param>
        public virtual void DisableAllBut(ItemClasses itemClass)
		{
			for (int i=0; i < SlotContainer.Count;i++)
			{
				if (InventoryItem.IsNull(TargetInventory.Content[i]))
				{
					continue;
				}
				if (TargetInventory.Content[i].ItemClass!=itemClass)
				{
					SlotContainer[i].DisableSlot();
				}
			}
		}

        /// <summary>
        /// 모든 슬롯을 다시 활성화합니다(일반적으로 일부 슬롯을 비활성화한 후).
        /// </summary>
        public virtual void ResetDisabledStates()
		{
			for (int i=0; i<SlotContainer.Count;i++)
			{
				SlotContainer[i].EnableSlot();
			}
		}

        /// <summary>
        /// MMInventoryEvents를 포착하고 이에 대한 조치를 취합니다.
        /// </summary>
        /// <param name="inventoryEvent">Inventory event.</param>
        public virtual void OnMMEvent(MMInventoryEvent inventoryEvent)
		{
            // 이 이벤트가 재고 표시와 관련이 없으면 아무것도 하지 않고 종료됩니다.
            if (inventoryEvent.TargetInventoryName != this.TargetInventoryName)
			{
				return;
			}

			if (inventoryEvent.PlayerID != this.PlayerID)
			{
				return;
			}

			switch (inventoryEvent.InventoryEventType)
			{
				case MMInventoryEventType.Select:
					SetCurrentlySelectedSlot (inventoryEvent.Slot);
					break;

				case MMInventoryEventType.Click:
					ReturnInventoryFocus ();
					SetCurrentlySelectedSlot (inventoryEvent.Slot);
					break;

				case MMInventoryEventType.Move:
					this.ReturnInventoryFocus();
					UpdateSlot(inventoryEvent.Index);
					break;

				case MMInventoryEventType.ItemUsed:
					this.ReturnInventoryFocus();
					break;
				
				case MMInventoryEventType.EquipRequest:
					if (this.TargetInventory.InventoryType == Inventory.InventoryTypes.Equipment)
					{
                        // 목표 재고가 설정되어 있지 않으면 아무것도 하지 않고 종료합니다.
                        if (TargetChoiceInventory == null)
						{
							Debug.LogWarning ("InventoryEngine Warning : " + this + " has no choice inventory associated to it.");
							return;
						}
                        // 올바른 유형과 일치하지 않는 모든 슬롯을 비활성화합니다.
                        TargetChoiceInventory.DisableAllBut (this.ItemClass);
                        // 목표 인벤토리에 초점을 맞췄습니다.
                        TargetChoiceInventory.Focus ();
						TargetChoiceInventory.InEquipSelection = true;
                        // 반환 초점 인벤토리를 설정했습니다.
                        TargetChoiceInventory.SetReturnInventory (this);
					}
					break;
				
				case MMInventoryEventType.ItemEquipped:
					ReturnInventoryFocus();
					break;

				case MMInventoryEventType.Drop:
					this.ReturnInventoryFocus ();
					break;

				case MMInventoryEventType.ItemUnEquipped:
					this.ReturnInventoryFocus ();
					break;

				case MMInventoryEventType.InventoryOpens:
					Focus();
					InventoryDisplay.CurrentlyBeingMovedItemIndex = -1;
					IsOpen = true;
					EventSystem.current.sendNavigationEvents = true;
					break;

				case MMInventoryEventType.InventoryCloses:
					InventoryDisplay.CurrentlyBeingMovedItemIndex = -1;
					EventSystem.current.sendNavigationEvents = false;
					IsOpen = false;
					SetCurrentlySelectedSlot (inventoryEvent.Slot);
					break;

				case MMInventoryEventType.ContentChanged:
					ContentHasChanged ();
					break;

				case MMInventoryEventType.Redraw:
					RedrawInventoryDisplay ();
					break;

				case MMInventoryEventType.InventoryLoaded:
					RedrawInventoryDisplay ();
					if (GetFocusOnStart)
					{
						Focus();
					}
					break;
			}
		}

        /// <summary>
        /// 활성화하면 MMInventoryEvents 수신을 시작합니다.
        /// </summary>
        protected virtual void OnEnable()
		{
			this.MMEventStartListening<MMInventoryEvent>();
		}

        /// <summary>
        /// 비활성화되면 MMInventoryEvents 수신이 중지됩니다.
        /// </summary>
        protected virtual void OnDisable()
		{
			this.MMEventStopListening<MMInventoryEvent>();
		}
	}
}