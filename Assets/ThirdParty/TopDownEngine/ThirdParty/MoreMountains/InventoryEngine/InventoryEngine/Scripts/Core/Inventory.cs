using MoreMountains.Tools;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace MoreMountains.InventoryEngine
{
    [Serializable]
    /// <summary>
    /// 인벤토리 기본 클래스.
    /// 아이템 저장, 콘텐츠 저장 및 로드, 아이템 추가, 아이템 제거, 장착 등을 처리합니다.
    /// </summary>
    public class Inventory : MonoBehaviour, MMEventListener<MMInventoryEvent>, MMEventListener<MMGameEvent>
    {
        public static List<Inventory> RegisteredInventories;

        /// 가능한 다양한 인벤토리 유형은 기본이며, 장비는 특별한 동작을 갖습니다(장착된 무기/방어구 등을 넣는 슬롯에 사용).
        public enum InventoryTypes { Main, Equipment }

        [Header("Player ID")]
        /// 이 인벤토리의 소유자를 식별하는 데 사용되는 고유 ID
        [Tooltip("이 인벤토리의 소유자를 식별하는 데 사용되는 고유 ID")]
        public string PlayerID = "Player1";

        /// 이 인벤토리에 있는 인벤토리 항목의 전체 목록
        [Tooltip("이는 인벤토리 내용을 실시간으로 보여줍니다. 검사기를 통해 이 목록을 수정하지 마십시오. 제어 목적으로만 표시됩니다.")]
        [MMReadOnly]
        public InventoryItem[] Content;

        [Header("Inventory Type")]
        /// 이 인벤토리가 주 인벤토리인지 장비 인벤토리인지 여부
        [Tooltip("여기에서 인벤토리 유형을 정의할 수 있습니다. 메인은 'regular' 재고입니다. 장비 인벤토리는 특정 아이템 클래스에 귀속되며 전용 옵션을 갖습니다.")]
        public InventoryTypes InventoryType = InventoryTypes.Main;

        [Header("Target Transform")]
        [Tooltip("TargetTransform은 인벤토리에서 떨어진 개체가 생성되는 장면의 모든 변환입니다.")]
        /// 객체를 떨어뜨렸을 때 생성될 변환
        public Transform TargetTransform;

        [Header("Persistence(고집)")]
        [Tooltip("여기서 이 인벤토리가 로드 및 저장 이벤트에 응답해야 하는지 여부를 정의할 수 있습니다. 인벤토리를 디스크에 저장하지 않으려면 false로 설정하세요. 또한 시작 시 재설정하여 이 레벨이 시작될 때 항상 비어 있도록 할 수도 있습니다.")]
        /// 이 인벤토리가 저장되고 로드되는지 여부
        public bool Persistent = true;
        /// 시작 시 이 인벤토리를 재설정해야 하는지 여부
        public bool ResetThisInventorySaveOnStart = false;

        [Header("Debug")]
        /// true인 경우 검사기에서 인벤토리의 내용을 그립니다.
        [Tooltip("인벤토리 구성 요소는 인벤토리의 데이터베이스 및 컨트롤러 부분과 같습니다. 화면에는 아무것도 표시되지 않으므로 InventoryDisplay도 필요합니다. 여기에서 검사기에서 디버그 콘텐츠를 출력할지 여부를 결정할 수 있습니다(디버깅에 유용함).")]
        public bool DrawContentInInspector = false;

        /// 인벤토리 소유자(여러 캐릭터가 있는 게임의 경우)
        public GameObject Owner { get; set; }

        /// 이 인벤토리의 여유 슬롯 수
        public int NumberOfFreeSlots => Content.Length - NumberOfFilledSlots;

        /// 인벤토리가 가득 차 있는지 여부(남은 여유 슬롯이 없음)
        public bool IsFull => NumberOfFreeSlots <= 0;

        /// 채워진 슬롯 수
        public int NumberOfFilledSlots
        {
            get
            {
                int numberOfFilledSlots = 0;
                for (int i = 0; i < Content.Length; i++)
                {
                    if (!InventoryItem.IsNull(Content[i]))
                    {
                        numberOfFilledSlots++;
                    }
                }
                return numberOfFilledSlots;
            }
        }

        public int NumberOfStackableSlots(string searchedItemID, int maxStackSize)
        {
            int numberOfStackableSlots = 0;
            int i = 0;

            while (i < Content.Length)
            {
                if (InventoryItem.IsNull(Content[i]))
                {
                    numberOfStackableSlots += maxStackSize;
                }
                else
                {
                    if (Content[i].ItemID == searchedItemID)
                    {
                        numberOfStackableSlots += maxStackSize - Content[i].Quantity;
                    }
                }
                i++;
            }

            return numberOfStackableSlots;
        }

        public static string _resourceItemPath = "Prefabs/Items/";
        public static string _saveFolderName = "InventoryEngine/";
        public static string _saveFileExtension = ".inventory";

        /// <summary>
        /// 검색된 이름 및 플레이어 ID와 일치하는 인벤토리를 반환합니다(발견된 경우).
        /// </summary>
        /// <param name="inventoryName"></param>
        /// <param name="playerID"></param>
        /// <returns></returns>
        public static Inventory FindInventory(string inventoryName, string playerID)
        {
            if (inventoryName == null)
            {
                return null;
            }

            foreach (Inventory inventory in RegisteredInventories)
            {
                if ((inventory.name == inventoryName) && (inventory.PlayerID == playerID))
                {
                    return inventory;
                }
            }
            return null;
        }

        //테스트
        public InventoryItem StoreItem;

        //내가 만든 변수
        public TextMeshProUGUI playerGold;
        public GameObject ParentPreviewObjects;
        InventoryItem ItemsToInclude;
        public InventoryItem[] InventoryItems = new InventoryItem[50];
        //현재 설치중인지 확인하는 변수
        public bool isInstalling = false;
        //메인 인벤토리
        public Inventory mainInventory;


        /// <summary>
        /// Awake에서는 이 인벤토리를 등록합니다.
        /// </summary>
        protected virtual void Awake()
        {
            ItemsToInclude = (InventoryItem)ScriptableObject.CreateInstance("InventoryItem");
            if (PlayerID == "NPC1")
            {
                InventoryItems = Resources.LoadAll<InventoryItem>($"Prefabs/Items");
            }
            RegisterInventory();
        }

        protected virtual void Start()
        {
            //파일 있는지 체크하고(해당 컴퓨터에서 처음 실행하는지 확인하고) 파일이 있으면 로드하고 없으면 초기값 넣어서 저장하고 불러온다
            if (PlayerID == "Player1")
                IsStartSetup();

            if (PlayerID == "NPC1")
                StoreItemSeting();
        }

        /// <summary>
        /// 나중에 다른 스크립트에서 액세스할 수 있도록 이 인벤토리를 등록합니다.
        /// </summary>
        protected virtual void RegisterInventory()
        {
            if (RegisteredInventories == null)
            {
                RegisteredInventories = new List<Inventory>();
            }
            if (RegisteredInventories.Count > 0)
            {
                for (int i = RegisteredInventories.Count - 1; i >= 0; i--)
                {
                    if (RegisteredInventories[i] == null)
                    {
                        RegisteredInventories.RemoveAt(i);
                    }
                }
            }
            RegisteredInventories.Add(this);
        }

        /// <summary>
        /// 예를 들어 아이템의 효과를 적용하는 데 유용한 이 인벤토리의 소유자를 설정합니다.
        /// </summary>
        /// <param name="newOwner">New owner.</param>
        public virtual void SetOwner(GameObject newOwner)
        {
            Owner = newOwner;
        }

        /// <summary>
        /// 지정된 유형의 항목을 추가하려고 시도합니다. 이는 이름 기반이라는 점에 유의하세요.
        /// </summary>
        /// <returns><c>true</c>, 항목이 추가된 경우, <c>false</c> 추가할 수 없는 경우(아이템 없음, 인벤토리 가득 참)</returns>
        /// <param name="itemToAdd">Item to add.</param>
        public virtual bool AddItem(InventoryItem itemToAdd, int quantity)
        {
            // 추가할 항목이 null이면 아무것도 하지 않고 종료합니다.
            if (itemToAdd == null)
            {
                Debug.LogWarning(this.name + " : 인벤토리에 추가하려는 항목이 null입니다.");
                return false;
            }

            List<int> list = InventoryContains(itemToAdd.ItemID);
            // 이미 인벤토리에 이와 같은 항목이 하나 이상 있고 쌓을 수 있는 경우
            if (list.Count > 0 && itemToAdd.MaximumStack > 1)
            {
                // 추가하려는 항목과 일치하는 항목을 저장합니다.
                for (int i = 0; i < list.Count; i++)
                {
                    // 인벤토리에 이런 종류의 항목 중 하나에 아직 공간이 있으면 추가합니다.
                    if (Content[list[i]].Quantity < itemToAdd.MaximumStack)
                    {
                        // 우리는 품목의 수량을 늘립니다.
                        Content[list[i]].Quantity += quantity;
                        // 최대 스택을 초과하는 경우
                        if (Content[list[i]].Quantity > Content[list[i]].MaximumStack)
                        {
                            InventoryItem restToAdd = itemToAdd;
                            int restToAddQuantity = Content[list[i]].Quantity - Content[list[i]].MaximumStack;
                            // 수량을 고정하고 나머지를 새 항목으로 추가합니다.
                            Content[list[i]].Quantity = Content[list[i]].MaximumStack;
                            AddItem(restToAdd, restToAddQuantity);
                        }
                        MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, this.name, null, 0, 0, PlayerID);
                        return true;
                    }
                }
            }
            // 인벤토리의 최대 크기에 도달하면 항목을 추가하지 않습니다.
            if (NumberOfFilledSlots >= Content.Length)
            {
                return false;
            }

            while (quantity > 0)
            {
                if (quantity > itemToAdd.MaximumStack)
                {
                    AddItem(itemToAdd, itemToAdd.MaximumStack);
                    quantity -= itemToAdd.MaximumStack;
                }
                else
                {
                    AddItemToArray(itemToAdd, quantity);
                    quantity = 0;
                }
            }
            // 아직 여기에 있다면 사용 가능한 첫 번째 슬롯에 항목을 추가합니다.
            MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, this.name, null, 0, 0, PlayerID);
            return true;
        }

        /// <summary>
        /// 지정된 항목의 지정된 수량을 선택한 대상 색인의 인벤토리에 추가합니다.
        /// </summary>
        /// <param name="itemToAdd"></param>
        /// <param name="quantity"></param>
        /// <param name="destinationIndex"></param>
        /// <returns></returns>
        public virtual bool AddItemAt(InventoryItem itemToAdd, int quantity, int destinationIndex)
        {
            int tempQuantity = quantity;

            if (!InventoryItem.IsNull(Content[destinationIndex]))
            {
                if ((Content[destinationIndex].ItemID != itemToAdd.ItemID) || (Content[destinationIndex].MaximumStack <= 1))
                {
                    return false;
                }
                else
                {
                    tempQuantity += Content[destinationIndex].Quantity;
                }
            }

            if (tempQuantity > itemToAdd.MaximumStack)
            {
                tempQuantity = itemToAdd.MaximumStack;
            }

            Content[destinationIndex] = itemToAdd.Copy();
            Content[destinationIndex].Quantity = tempQuantity;

            // 아직 여기에 있다면 사용 가능한 첫 번째 슬롯에 항목을 추가합니다.
            MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, this.name, null, 0, 0, PlayerID);
            return true;
        }

        /// <summary>
        /// 첫 번째 매개변수 슬롯의 항목을 두 번째 슬롯으로 이동하려고 합니다.
        /// </summary>
        /// <returns><c>true</c>, 항목이 이동된 경우, <c>false</c> 아닐경우.</returns>
        /// <param name="startIndex">Start index.</param>
        /// <param name="endIndex">End index.</param>
        public virtual bool MoveItem(int startIndex, int endIndex)
        {
            bool swap = false;
            // 이동하려는 항목이 null이면 이는 빈 슬롯을 이동하려고 한다는 의미입니다.
            if (InventoryItem.IsNull(Content[startIndex]))
            {
                Debug.LogWarning("InventoryEngine : 빈 슬롯을 이동하려고 합니다.");
                return false;
            }
            // 두 객체가 모두 교체 가능하면 교체하겠습니다.
            if (Content[startIndex].CanSwapObject)
            {
                if (!InventoryItem.IsNull(Content[endIndex]))
                {
                    if (Content[endIndex].CanSwapObject)
                    {
                        swap = true;
                    }
                }
            }
            // 대상 슬롯이 비어 있는 경우
            if (InventoryItem.IsNull(Content[endIndex]))
            {
                // 우리는 목적지에 항목의 복사본을 만듭니다
                Content[endIndex] = Content[startIndex].Copy();
                // 우리는 원본을 제거합니다
                RemoveItemFromArray(startIndex);
                // 콘텐츠가 변경되었으며 GUI가 첨부된 경우 인벤토리를 다시 그려야 할 수도 있다고 언급했습니다.
                MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, this.name, null, 0, 0, PlayerID);
                return true;
            }
            else
            {
                // 객체를 교환할 수 있으면 시도해 보겠습니다. 그렇지 않으면 대상 슬롯이 null이 아니므로 false를 반환합니다.
                if (swap)
                {
                    // 우리는 물건을 교환한다
                    InventoryItem tempItem = Content[endIndex].Copy();
                    Content[endIndex] = Content[startIndex].Copy();
                    Content[startIndex] = tempItem;
                    MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, this.name, null, 0, 0, PlayerID);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// 이 방법을 사용하면 startIndex의 항목을 선택한 targetInventory(선택 사항인 endIndex)로 이동할 수 있습니다.
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="targetInventory"></param>
        /// <param name="endIndex"></param>
        /// <returns></returns>
        public virtual bool MoveItemToInventory(int startIndex, Inventory targetInventory, int endIndex = -1)
        {
            // 이동하려는 항목이 null이면 이는 빈 슬롯을 이동하려고 한다는 의미입니다.
            if (InventoryItem.IsNull(Content[startIndex]))
            {
                Debug.LogWarning("InventoryEngine : 빈 슬롯을 이동하려고 합니다.");
                return false;
            }

            // 목적지가 비어 있지 않으면 우리도 떠납니다.
            if ((endIndex >= 0) && (!InventoryItem.IsNull(targetInventory.Content[endIndex])))
            {
                Debug.LogWarning("InventoryEngine : 대상 슬롯이 비어 있지 않아 이동할 수 없습니다.");
                return false;
            }

            InventoryItem itemToMove = Content[startIndex].Copy();

            // 대상 인덱스를 지정했다면 이를 사용하고, 그렇지 않으면 정상적으로 추가합니다.
            if (endIndex >= 0)
            {
                //퀵 슬롯에 똑같은 아이템,똑같은 수량이 이미 존재하면 삭제후 움긴 자리에 다시 생성
                if (SceneManager.GetActiveScene().name != "Village")
                {
                    for (int i = 0; i < targetInventory.Content.Length; i++)
                    {
                        if (targetInventory.Content[i] == null)
                            continue;

                        if (targetInventory.Content[i].ItemID == itemToMove.ItemID && targetInventory.Content[i].Quantity == itemToMove.Quantity)
                        {
                            targetInventory.GetComponent<Inventory>().RemoveItem(i, targetInventory.Content[i].Quantity);
                        }
                    }
                }

                targetInventory.AddItemAt(itemToMove, itemToMove.Quantity, endIndex);
            }
            else
            {
                targetInventory.AddItem(itemToMove, itemToMove.Quantity);
            }

            // 원래 제거해야하는데 퀵슬롯에 가는거라 제거안함 나중에 퀵슬롯 말고 다른곳으로 움길일이 있으면 다시 수정해야함.
            RemoveItem(startIndex, itemToMove.Quantity);
            AddItemAt(itemToMove, itemToMove.Quantity, startIndex);//퀵슬롯으로 이동후 스프라이트 안바뀌는 버그때문에 지우고 그자리에 다시 생성함

            return true;
        }

        /// <summary>
        /// 인벤토리에서 지정된 항목을 제거합니다.
        /// </summary>
        /// <returns>항목이 제거된 경우<c>true</c>, 아닐경우<c>false</c></returns>
        /// <param name="itemToRemove">Item to remove.</param>
        public virtual bool RemoveItem(int i, int quantity)
        {
            if (i < 0 || i >= Content.Length)
            {
                Debug.LogWarning("InventoryEngine : 잘못된 색인에서 항목을 제거하려고 합니다.");
                return false;
            }
            if (InventoryItem.IsNull(Content[i]))
            {
                Debug.LogWarning("InventoryEngine : 빈 슬롯에서 제거하려고 합니다.");
                return false;
            }

            quantity = Mathf.Max(0, quantity);

            Content[i].Quantity -= quantity;
            if (Content[i].Quantity <= 0)// 사용후 아이템 수량이 0이하면 아이템을 삭제시킨다.
            {
                bool suppressionSuccessful = RemoveItemFromArray(i);
                MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, this.name, null, 0, 0, PlayerID);
                return suppressionSuccessful;
            }
            else
            {
                MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, this.name, null, 0, 0, PlayerID);
                return true;
            }
        }

        /// <summary>
        /// 지정된 itemID와 일치하는 항목의 지정된 수량을 제거합니다.
        /// </summary>
        /// <param name="itemID"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        public virtual bool RemoveItemByID(string itemID, int quantity)
        {
            if (quantity < 1)
            {
                Debug.LogWarning("InventoryEngine : 재고에서 잘못된 수량(" + quantity + ")을 제거하려고 합니다.");
                return false;
            }

            if (itemID == null || itemID == "")
            {
                Debug.LogWarning("InventoryEngine : 항목을 제거하려고 하는데 항목 ID가 지정되지 않았습니다.");
                return false;
            }

            int quantityLeftToRemove = quantity;


            List<int> list = InventoryContains(itemID);
            foreach (int index in list)
            {
                int quantityAtIndex = Content[index].Quantity;
                RemoveItem(index, quantityLeftToRemove);
                quantityLeftToRemove -= quantityAtIndex;
                if (quantityLeftToRemove <= 0)
                {
                    return true;
                }
            }

            return false;
        }

        //지정한 아이템ID와 지정한 아이템수량이 같은 아이템을 찾아 item.ConsumeQuantity만큼 제거합니다. 제거했는데 아이템 남은수량이 0이라면 아이템을 삭제합니다.
        public virtual bool RemoveItemByIDandQuantity(string itemID,int itemQuantity, int RemoveQuantity)
        {
            if (RemoveQuantity < 1)
            {
                Debug.LogWarning("InventoryEngine : 재고에서 잘못된 수량(" + RemoveQuantity + ")을 제거하려고 합니다.");
                return false;
            }

            if (itemID == null || itemID == "")
            {
                Debug.LogWarning("InventoryEngine : 항목을 제거하려고 하는데 항목 ID가 지정되지 않았습니다.");
                return false;
            }

            int quantityLeftToRemove = RemoveQuantity;

            List<int> list = InventoryContains(itemID);
            for (int i = 0; i < list.Count; i++)
            {
                int quantityAtIndex = Content[list[i]].Quantity;
                Debug.Log($"인벤토리에 들어있는 [{list[i]}]번째 아이템 수량 = [{Content[list[i]].Quantity}]");
                if (Content[list[i]].Quantity == itemQuantity)
                {
                    RemoveItem(list[i], quantityLeftToRemove);
                    Debug.Log($"인벤토리에 들어있는 [{list[i]}]번째 아이템 삭제");
                    quantityAtIndex -= quantityAtIndex; //가지고 있는 아이템수량에서 소모하고 싶은 수량을 뺏는데 1이상이면 트루
                    if(quantityAtIndex > 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 인덱스 i에 저장된 항목을 파괴합니다.
        /// </summary>
        /// <returns><c>true</c>, 아이템이 파괴된 경우, <c>false</c> 그렇지 않을 경우.</returns>
        /// <param name="i">The index.</param>
        public virtual bool DestroyItem(int i)
        {
            Content[i] = null;

            MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, this.name, null, 0, 0, PlayerID);
            return true;
        }

        /// <summary>
        /// 현재 인벤토리 상태를 비웁니다.
        /// </summary>
        public virtual void EmptyInventory()
        {
            Content = new InventoryItem[Content.Length];

            MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, this.name, null, 0, 0, PlayerID);
        }

        /// <summary>
        /// 콘텐츠 배열에 항목을 추가합니다.
        /// </summary>
        /// <returns><c>true</c>, 배열에 항목이 추가된 경우, <c>false</c> 아닐경우.</returns>
        /// <param name="itemToAdd">Item to add.</param>
        /// <param name="quantity">Quantity.</param>
        protected virtual bool AddItemToArray(InventoryItem itemToAdd, int quantity)
        {
            if (NumberOfFreeSlots == 0)
            {
                return false;
            }
            int i = 0;
            while (i < Content.Length)
            {
                if (InventoryItem.IsNull(Content[i]))
                {
                    Content[i] = itemToAdd.Copy();
                    Content[i].Quantity = quantity;
                    return true;
                }
                i++;
            }
            return false;
        }

        /// <summary>
        /// 배열에서 인덱스 i에 있는 항목을 제거합니다.
        /// </summary>
        /// <returns>배열의 항목이 제거된 경우<c>true</c> 아닐경우<c>false</c> </returns>
        /// <param name="i">The index.</param>
        protected virtual bool RemoveItemFromArray(int i)
        {
            if (i < Content.Length)
            {
                //Content[i].ItemID = null;
                Content[i] = null;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 배열의 크기를 지정된 새 크기로 조정합니다.
        /// </summary>
        /// <param name="newSize">New size.</param>
        public virtual void ResizeArray(int newSize)
        {
            InventoryItem[] temp = new InventoryItem[newSize];
            for (int i = 0; i < Mathf.Min(newSize, Content.Length); i++)
            {
                temp[i] = Content[i];
            }
            Content = temp;
        }

        /// <summary>
        /// 지정된 이름과 일치하는 항목의 총 수량을 반환합니다.
        /// </summary>
        /// <returns>The quantity.</returns>
        /// <param name="searchedItem">Searched item.</param>
        public virtual int GetQuantity(string searchedItemID)
        {
            List<int> list = InventoryContains(searchedItemID);
            int total = 0;
            foreach (int i in list)
            {
                total += Content[i].Quantity;
            }
            return total;
        }

        /// <summary>
        /// 지정된 이름과 일치하는 인벤토리의 Index들을 반환합니다.
        /// </summary>
        /// <returns>검색 기준과 일치하는 항목 목록입니다.</returns>
        /// <param name="searchedType">검색된 유형입니다.</param>
        public virtual List<int> InventoryContains(string searchedItemID)
        {
            List<int> list = new List<int>();

            for (int i = 0; i < Content.Length; i++)
            {
                if (!InventoryItem.IsNull(Content[i]))
                {
                    if (Content[i].ItemID == searchedItemID)
                    {
                        list.Add(i);
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// 지정된 클래스와 일치하는 인벤토리의 모든 항목 목록을 반환합니다.
        /// </summary>
        /// <returns>검색 기준과 일치하는 항목 목록입니다.</returns>
        /// <param name="searchedType">The searched type.</param>
        public virtual List<int> InventoryContains(MoreMountains.InventoryEngine.ItemClasses searchedClass)
        {
            List<int> list = new List<int>();

            for (int i = 0; i < Content.Length; i++)
            {
                if (InventoryItem.IsNull(Content[i]))
                {
                    continue;
                }
                if (Content[i].ItemClass == searchedClass)
                {
                    list.Add(i);
                }
            }
            return list;
        }

        /// <summary>
        /// 인벤토리를 파일에 저장합니다.
        /// </summary>
        public virtual void SaveInventory()
        {
            SerializedInventory serializedInventory = new SerializedInventory();
            FillSerializedInventory(serializedInventory);
            MMSaveLoadManager.Save(serializedInventory, DetermineSaveName());
        }

        /// <summary>
        /// 파일이 있는 경우 인벤토리 로드를 시도합니다.
        /// </summary>
        public virtual void LoadSavedInventory()
        {
            SerializedInventory serializedInventory = (SerializedInventory)MMSaveLoadManager.Load(typeof(SerializedInventory), DetermineSaveName());
            ExtractSerializedInventory(serializedInventory);
            MMInventoryEvent.Trigger(MMInventoryEventType.InventoryLoaded, null, this.name, null, 0, 0, PlayerID);
        }

        /// <summary>
        /// 보관을 위해 일련번호가 지정된 인벤토리를 채웁니다.
        /// </summary>
        /// <param name="serializedInventory">직렬화된 인벤토리.</param>
        protected virtual void FillSerializedInventory(SerializedInventory serializedInventory)
        {
            serializedInventory.InventoryType = InventoryType;
            serializedInventory.DrawContentInInspector = DrawContentInInspector;
            serializedInventory.ContentType = new string[Content.Length];
            serializedInventory.ContentQuantity = new int[Content.Length];
            for (int i = 0; i < Content.Length; i++)
            {
                if (!InventoryItem.IsNull(Content[i]))
                {
                    serializedInventory.ContentType[i] = Content[i].ItemID;
                    serializedInventory.ContentQuantity[i] = Content[i].Quantity;
                }
                else
                {
                    serializedInventory.ContentType[i] = null;
                    serializedInventory.ContentQuantity[i] = 0;
                }
            }
        }

        protected InventoryItem _loadedInventoryItem;

        /// <summary>
        /// 파일 콘텐츠에서 직렬화된 인벤토리를 추출합니다.
        /// </summary>
        /// <param name="serializedInventory">Serialized inventory.</param>
        protected virtual void ExtractSerializedInventory(SerializedInventory serializedInventory)
        {
            if (serializedInventory == null)
            {
                return;
            }

            InventoryType = serializedInventory.InventoryType;
            DrawContentInInspector = serializedInventory.DrawContentInInspector;
            Content = new InventoryItem[serializedInventory.ContentType.Length];
            for (int i = 0; i < serializedInventory.ContentType.Length; i++)
            {
                if ((serializedInventory.ContentType[i] != null) && (serializedInventory.ContentType[i] != ""))
                {
                    _loadedInventoryItem = Resources.Load<InventoryItem>(_resourceItemPath + serializedInventory.ContentType[i]);
                    if (_loadedInventoryItem == null)
                    {
                        Debug.LogError("InventoryEngine : Couldn't find any inventory item to load at " + _resourceItemPath
                            + " named " + serializedInventory.ContentType[i] + ". Make sure all your items definitions names (the name of the InventoryItem scriptable " +
                            "objects) are exactly the same as their ItemID string in their inspector. " +
                            "Once that's done, also make sure you reset all saved inventories as the mismatched names and IDs may have " +
                            "corrupted them.");
                    }
                    else
                    {
                        Content[i] = _loadedInventoryItem.Copy();
                        Content[i].Quantity = serializedInventory.ContentQuantity[i];
                    }
                }
                else
                {
                    Content[i] = null;
                }
            }
        }

        protected virtual string DetermineSaveName()
        {
            return gameObject.name + "_" + PlayerID + _saveFileExtension;
        }

        /// <summary>
        /// 모든 저장 파일을 파괴합니다.
        /// </summary>
        public virtual void ResetSavedInventory()
        {
            MMSaveLoadManager.DeleteSave(DetermineSaveName(), _saveFolderName);
            Debug.LogFormat("Inventory save file deleted");
        }

        /// <summary>
        /// 매개변수에 전달된 항목의 사용 및 잠재적 소비를 트리거합니다. 항목의 슬롯(선택 사항)과 색인을 지정할 수도 있습니다.
        /// </summary>
        /// <param name="item">Item.</param>
        /// <param name="slot">Slot.</param>
        /// <param name="index">Index.</param>
        public virtual bool UseItem(InventoryItem item, int index, InventorySlot slot = null)
        {
            if (InventoryItem.IsNull(item))
            {
                MMInventoryEvent.Trigger(MMInventoryEventType.Error, slot, this.name, null, 0, index, PlayerID);
                return false;
            }
            if (!item.IsUsable)
            {
                return false;
            }

            if (item.Use(PlayerID))
            {
                // 수량에서 ConsumeQuantity만큼 제거
                MMInventoryEvent.Trigger(MMInventoryEventType.ItemUsed, slot, this.name, item.Copy(), 0, index, PlayerID);
                if (item.Consumable)
                {
                    //현재 아이템 사용하는 방법은 퀵슬롯에 등록 해서만 사용이 가능하게 끔 만들었음
                    Debug.Log($"퀵슬롯에 들어있는 [{index}]번째 아이템 수량 = [{Content[index].Quantity}]");
                    mainInventory.RemoveItemByIDandQuantity(item.ItemID, Content[index].Quantity, item.ConsumeQuantity);

                    RemoveItem(index, item.ConsumeQuantity);
                    Debug.Log($"퀵슬롯에 들어있는 [{index}]번째 아이템 삭제");
                }
            }
            return true;
        }

        /// <summary>
        /// 이름에 지정된 대로 항목의 사용을 트리거합니다. 중복되는 경우 항목을 어느 슬롯에서 가져갈지 특별히 신경쓰지 않는 경우 이전 서명보다 이 서명을 선호합니다.
        /// </summary>
        /// <param name="itemName"></param>
        /// <returns></returns>
        public virtual bool UseItem(string itemName)
        {
            List<int> list = InventoryContains(itemName);
            if (list.Count > 0)
            {
                UseItem(Content[list[list.Count - 1]], list[list.Count - 1], null);
                return true;
            }
            else
            {
                return false;
            }
        }

        //아이템 설치모드에 들어갑니다.
        public virtual bool InstallationItem(InventoryItem item, int index, InventorySlot slot = null)
        {
            if (InventoryItem.IsNull(item))
            {
                MMInventoryEvent.Trigger(MMInventoryEventType.Error, slot, this.name, null, 0, index, PlayerID);
                return false;
            }
            if (!item.IsInstallable)
            {
                return false;
            }

            //아이템 설치 모드 실행
            if (item.Installation(PlayerID))
            {
                //설치류 아이템을 사용했기 때문에 설치중(isInstalling)을 true로 바꿔준다.
                if (item.isInstallable)
                    isInstalling = true;
            }

            return true;
        }

        //아이템 설치모드에서 취소 하여 원래 모드로 돌아가기
        public virtual bool CancleInstallationItem(InventoryItem item, int index, InventorySlot slot = null)
        {
            if (InventoryItem.IsNull(item))
            {
                MMInventoryEvent.Trigger(MMInventoryEventType.Error, slot, this.name, null, 0, index, PlayerID);
                return false;
            }
            if (!item.IsInstallable)
            {
                return false;
            }

            //아이템 설치 모드 실행
            if (item.InstallationCancel(PlayerID))
            {
                if (item.isInstallable)
                    isInstalling = false;
            }

            return true;
        }


        /// <summary>
        /// 지정된 슬롯에 아이템을 장착합니다.
        /// </summary>
        /// <param name="item">Item.</param>
        /// <param name="index">Index.</param>
        /// <param name="slot">Slot.</param>
        public virtual void EquipItem(InventoryItem item, int index, InventorySlot slot = null)
        {
            if (InventoryType == Inventory.InventoryTypes.Main)
            {
                InventoryItem oldItem = null;
                if (InventoryItem.IsNull(item))
                {
                    MMInventoryEvent.Trigger(MMInventoryEventType.Error, slot, this.name, null, 0, index, PlayerID);
                    return;
                }
                // 객체가 장착 가능하지 않으면 아무것도 하지 않고 종료합니다.
                if (!item.IsEquippable)
                {
                    return;
                }
                // 대상 장비 인벤토리가 설정되어 있지 않으면 아무것도 하지 않고 종료됩니다.
                if (item.TargetEquipmentInventory(PlayerID) == null)
                {
                    Debug.LogWarning("InventoryEngine Warning : " + Content[index].ItemName + "'s target equipment inventory couldn't be found.");
                    return;
                }
                // 객체를 이동할 수 없으면 오류 소리를 내고 종료합니다.
                if (!item.CanMoveObject)
                {
                    MMInventoryEvent.Trigger(MMInventoryEventType.Error, slot, this.name, null, 0, index, PlayerID);
                    return;
                }
                // 인벤토리가 가득 차면 개체를 장착할 수 없고, 실제로 장착되어 있으면 아무것도 하지 않고 종료합니다.
                if (!item.EquippableIfInventoryIsFull)
                {
                    if (item.TargetEquipmentInventory(PlayerID).IsFull)
                    {
                        return;
                    }
                }
                // 아이템의 장비 메소드를 호출하세요
                if (!item.Equip(PlayerID))
                {
                    return;
                }
                // 모노슬롯 인벤토리라면 교체 준비를 합니다
                if (item.TargetEquipmentInventory(PlayerID).Content.Length == 1)
                {
                    if (!InventoryItem.IsNull(item.TargetEquipmentInventory(PlayerID).Content[0]))
                    {
                        if (
                            (item.CanSwapObject)
                            && (item.TargetEquipmentInventory(PlayerID).Content[0].CanMoveObject)
                            && (item.TargetEquipmentInventory(PlayerID).Content[0].CanSwapObject)
                        )
                        {
                            // 장비 인벤토리에 아이템을 저장합니다
                            oldItem = item.TargetEquipmentInventory(PlayerID).Content[0].Copy();
                            item.TargetEquipmentInventory(PlayerID).EmptyInventory();
                        }
                    }
                }
                // 대상 장비 인벤토리에 하나를 추가합니다.
                item.TargetEquipmentInventory(PlayerID).AddItem(item.Copy(), item.Quantity);
                // 수량에서 1개 제거
                if (item.MoveWhenEquipped)
                {
                    RemoveItem(index, item.Quantity);
                }
                if (oldItem != null)
                {
                    oldItem.Swap(PlayerID);
                    if (oldItem.ForceSlotIndex)
                    {
                        AddItemAt(oldItem, oldItem.Quantity, oldItem.TargetIndex);
                    }
                    else
                    {
                        AddItem(oldItem, oldItem.Quantity);
                    }
                }
                MMInventoryEvent.Trigger(MMInventoryEventType.ItemEquipped, slot, this.name, item, item.Quantity, index, PlayerID);
            }
        }

        /// <summary>
        /// 아이템을 떨어뜨려 인벤토리에서 제거하고 잠재적으로 캐릭터 근처 땅에 아이템을 생성합니다.
        /// </summary>
        /// <param name="item">Item.</param>
        /// <param name="index">Index.</param>
        /// <param name="slot">Slot.</param>
        public virtual void DropItem(InventoryItem item, int index, InventorySlot slot = null)
        {
            if (InventoryItem.IsNull(item))
            {
                MMInventoryEvent.Trigger(MMInventoryEventType.Error, slot, this.name, null, 0, index, PlayerID);
                return;
            }
            item.SpawnPrefab(PlayerID);

            if (this.name == item.TargetEquipmentInventoryName)
            {
                if (item.UnEquip(PlayerID))
                {
                    DestroyItem(index);
                }
            }
            else
            {
                DestroyItem(index);
            }

        }

        public virtual void DestroyItem(InventoryItem item, int index, InventorySlot slot = null)
        {
            if (InventoryItem.IsNull(item))
            {
                MMInventoryEvent.Trigger(MMInventoryEventType.Error, slot, this.name, null, 0, index, PlayerID);
                return;
            }
            DestroyItem(index);
        }

        public virtual void UnEquipItem(InventoryItem item, int index, InventorySlot slot = null)
        {
            // 이 슬롯에 항목이 없으면 오류가 발생합니다.
            if (InventoryItem.IsNull(item))
            {
                MMInventoryEvent.Trigger(MMInventoryEventType.Error, slot, this.name, null, 0, index, PlayerID);
                return;
            }
            // 장비 인벤토리에 없으면 오류가 발생합니다.
            if (InventoryType != InventoryTypes.Equipment)
            {
                MMInventoryEvent.Trigger(MMInventoryEventType.Error, slot, this.name, null, 0, index, PlayerID);
                return;
            }
            // 아이템의 장착 해제 효과를 발동시킵니다.
            if (!item.UnEquip(PlayerID))
            {
                return;
            }
            MMInventoryEvent.Trigger(MMInventoryEventType.ItemUnEquipped, slot, this.name, item, item.Quantity, index, PlayerID);

            // 대상 인벤토리가 있으면 해당 항목을 다시 해당 인벤토리에 추가하려고 합니다.
            if (item.TargetInventory(PlayerID) != null)
            {
                bool itemAdded = false;
                if (item.ForceSlotIndex)
                {
                    itemAdded = item.TargetInventory(PlayerID).AddItemAt(item, item.Quantity, item.TargetIndex);
                    if (!itemAdded)
                    {
                        itemAdded = item.TargetInventory(PlayerID).AddItem(item, item.Quantity);
                    }
                }
                else
                {
                    itemAdded = item.TargetInventory(PlayerID).AddItem(item, item.Quantity);
                }

                // 항목을 추가했다면
                if (itemAdded)
                {
                    DestroyItem(index);
                }
                else
                {
                    // 할 수 없으면(예를 들어 재고가 가득 찬 경우) 땅에 떨어뜨립니다.
                    MMInventoryEvent.Trigger(MMInventoryEventType.Drop, slot, this.name, item, item.Quantity, index, PlayerID);
                }
            }
        }

        /// <summary>
        /// 처음 시작하는지 확인하고 결과에 따라 인벤토리에 아이템 넣기
        /// </summary>
		public virtual void IsStartSetup()
        {
            string path = MMSaveLoadManager.DetermineSavePath();

            //파일이 있는지 먼저 체크
            if (Directory.Exists(path))
            {
                //폴더가 존재함
                LoadSavedInventory();
                Debug.Log($"세이브 파일이 이미 존재하여 세이브 되어있는 인벤토리를 불러왔습니다. (경로 = {path})");
            }
            else
            {
                //폴더가 존재하지 않음
                SaveInventory();
                LoadSavedInventory();
                Debug.Log($"세이브 파일이 존재하지 않아 초기값으로 저장후 인벤토리를 불러왔습니다. (경로 = {path})");
            }

            //Debug.Log($"세이브 파일이 이미 존재하여 세이브 되어있는 인벤토리를 불러왔습니다. (경로 = {path})");
            //Debug.Log($"세이브 파일이 존재하지 않아 초기값으로 저장후 인벤토리를 불러왔습니다. (경로 = {path})");
        }

        /// <summary>
        /// Store에 판매하는 아이템 세팅
        /// </summary>
		public void StoreItemSeting()
        {
            for (int i = 0; i < InventoryItems.Length; i++)
            {
                if (InventoryItems[i].ItemID == "Apple_npc")
                {
                    ItemsToInclude = InventoryItems[i];
                    AddItem(ItemsToInclude, 1);
                }
                else if (InventoryItems[i].ItemID == "ArmorBlue_npc")
                {
                    ItemsToInclude = InventoryItems[i];
                    AddItem(ItemsToInclude, 1);
                }
                else if(InventoryItems[i].ItemID == "Axe_npc")
                {
                    ItemsToInclude = InventoryItems[i];
                    AddItem(ItemsToInclude, 1);
                }
                else if(InventoryItems[i].ItemID == "StoneBarrier_npc")
                {
                    ItemsToInclude = InventoryItems[i];
                    AddItem(ItemsToInclude, 1);
                }
            }
        }

        /// <summary>
        /// 인벤토리 이벤트를 포착하고 이에 대한 조치를 취합니다.
        /// </summary>
        /// <param name="inventoryEvent">Inventory event.</param>
        public virtual void OnMMEvent(MMInventoryEvent inventoryEvent)
        {
            // 이 이벤트가 재고 표시와 관련이 없으면 아무것도 하지 않고 종료됩니다.
            if (inventoryEvent.TargetInventoryName != this.name)
            {
                return;
            }
            if (inventoryEvent.PlayerID != PlayerID)
            {
                return;
            }
            switch (inventoryEvent.InventoryEventType)
            {
                case MMInventoryEventType.Pick:
                    if (inventoryEvent.EventItem.ForceSlotIndex)
                    {
                        AddItemAt(inventoryEvent.EventItem, inventoryEvent.Quantity, inventoryEvent.EventItem.TargetIndex);
                    }
                    else
                    {
                        AddItem(inventoryEvent.EventItem, inventoryEvent.Quantity);
                    }
                    break;

                case MMInventoryEventType.UseRequest:
                    UseItem(inventoryEvent.EventItem, inventoryEvent.Index, inventoryEvent.Slot);
                    break;

                case MMInventoryEventType.Installed:
                    InstallationItem(inventoryEvent.EventItem, inventoryEvent.Index, inventoryEvent.Slot);
                    break;

                case MMInventoryEventType.CancelInstallation:
                    CancleInstallationItem(inventoryEvent.EventItem, inventoryEvent.Index, inventoryEvent.Slot);
                    break;


                case MMInventoryEventType.EquipRequest:
                    EquipItem(inventoryEvent.EventItem, inventoryEvent.Index, inventoryEvent.Slot);
                    break;

                case MMInventoryEventType.UnEquipRequest:
                    UnEquipItem(inventoryEvent.EventItem, inventoryEvent.Index, inventoryEvent.Slot);
                    break;

                case MMInventoryEventType.Destroy:
                    DestroyItem(inventoryEvent.EventItem, inventoryEvent.Index, inventoryEvent.Slot);
                    break;

                case MMInventoryEventType.Drop:
                    DropItem(inventoryEvent.EventItem, inventoryEvent.Index, inventoryEvent.Slot);
                    break;
            }
        }

        /// <summary>
        /// MMGameEvent를 포착하면 이름을 기반으로 작업을 수행합니다.
        /// </summary>
        /// <param name="gameEvent">Game event.</param>
        public virtual void OnMMEvent(MMGameEvent gameEvent)
        {
            if ((gameEvent.EventName == "Save") && Persistent)
            {
                SaveInventory();
            }
            if ((gameEvent.EventName == "Load") && Persistent)
            {
                if (ResetThisInventorySaveOnStart)
                {
                    ResetSavedInventory();
                }
                LoadSavedInventory();
            }
        }

        /// <summary>
        /// 활성화되면 MMGameEvents 수신을 시작합니다. 이를 확장하여 다른 유형의 이벤트를 수신할 수도 있습니다.
        /// </summary>
        protected virtual void OnEnable()
        {
            this.MMEventStartListening<MMGameEvent>();
            this.MMEventStartListening<MMInventoryEvent>();
        }

        /// <summary>
        /// 비활성화하면 MMGameEvents 수신이 중지됩니다. 다른 유형의 이벤트 수신을 중지하기 위해 이를 확장할 수 있습니다.
        /// </summary>
        protected virtual void OnDisable()
        {
            this.MMEventStopListening<MMGameEvent>();
            this.MMEventStopListening<MMInventoryEvent>();
        }
    }
}