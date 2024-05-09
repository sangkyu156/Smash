using MoreMountains.InventoryEngine;
using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.PostProcessing.SubpixelMorphologicalAntialiasing;

public class VillagePopupManager : MonoBehaviour, MMEventListener<MMInventoryEvent>, MMEventListener<MMGameEvent>
{
    public GameObject buyPopup;
    public GameObject sellPopup;
    public GameObject skillBuyPopup;
    public GameObject skillResetPopup;
    public SkillDetails skillDetails;
    public ItemStore itemStore;
    public Inventory inventory;
    public InventoryDisplay inventoryDisplay;
    public InventoryDetails inventoryDetails;
    int curQuantity;
    string newItemName;
    InventoryItem curItem;
    InventorySlot curSolt;
    public InventoryItem[] InventoryItems = new InventoryItem[50];

    private void Awake()
    {
        InventoryItems = Resources.LoadAll<InventoryItem>($"Prefabs/Items");
    }

    private void Start()
    {
        OffBuyPopup();
        OffSellPopup();
        OffSkillBuyPopup();
        OffSkillResetPopup();
    }

    public void OnBuyPopup()
    { buyPopup.SetActive(true); }

    public void OffBuyPopup()
    { buyPopup.SetActive(false); }

    public void OnSellPopup()
    { sellPopup.SetActive(true); }

    public void OffSellPopup()
    { sellPopup.SetActive(false); }
    public void OnSkillBuyPopup()
    {
        if (skillDetails.curSkill == null) 
            return;
        skillBuyPopup.SetActive(true); 
    }
    public void OffSkillBuyPopup()
    { skillBuyPopup.SetActive(false); }
    public void OnSkillResetPopup()
    {
        if (skillDetails.curSkill == null) 
            return; 
        skillResetPopup.SetActive(true); 
    }
    public void OffSkillResetPopup()
    { skillResetPopup.SetActive(false); }

    //����� OK ��ư ������ ����Ǵ� �޼ҵ� ��������
    public void BuyItem()
    {
        if (DataManager.Instance.datas.CurPlayerGold < curItem.price * curQuantity)
            return;

        curQuantity = itemStore.quantity;
        DataManager.Instance.datas.CurPlayerGold -= (curItem.price * curQuantity);
        itemStore.SetPlayerGold();
        newItemName = curItem.ItemID.Replace("_npc", "");
        //�������� ������ ã�Ƽ� �κ��� �־��ֱ�
        foreach (InventoryItem item in InventoryItems)
        {
            if(item.ItemID == newItemName)
            {
                inventory.AddItem(item, curQuantity);
            }
        }
        inventory.SaveInventory();
        DataManager.Instance.DataSave();
        inventoryDetails.DisplayDetails(null);

        OffBuyPopup();
    }

    public void SellItem()
    {
        curQuantity = itemStore.quantity;
        DataManager.Instance.datas.CurPlayerGold += (int)Mathf.Round((curItem.price * curQuantity) * 0.8f);
        itemStore.SetPlayerGold();
        inventory.RemoveItem(curSolt.Index, curQuantity);
        inventory.SaveInventory();
        DataManager.Instance.DataSave();
        inventoryDisplay.RedrawInventoryDisplay();
        inventoryDisplay.Focus();
        inventoryDetails.DisplayDetails(null);

        OffSellPopup();
    }

    public void BuySkill()
    {
        if (skillDetails.curSkill == null) return;
        skillDetails.SkillBuyButton();
        OffSkillBuyPopup();
    }

    public void OnMMEvent(MMInventoryEvent inventoryEvent)
    {
        switch (inventoryEvent.InventoryEventType)
        {
            case MMInventoryEventType.Pick:
                break;
            case MMInventoryEventType.Select:
                break;
            case MMInventoryEventType.Click:
                curSolt = inventoryEvent.Slot;
                curItem = curSolt.CurrentlySelectedItem();
                break;
            case MMInventoryEventType.Move:
                break;
            case MMInventoryEventType.UseRequest:
                break;
            case MMInventoryEventType.ItemUsed:
                break;
            case MMInventoryEventType.EquipRequest:
                break;
            case MMInventoryEventType.ItemEquipped:
                break;
            case MMInventoryEventType.UnEquipRequest:
                break;
            case MMInventoryEventType.ItemUnEquipped:
                break;
            case MMInventoryEventType.Drop:
                break;
            case MMInventoryEventType.Destroy:
                break;
            case MMInventoryEventType.Error:
                break;
            case MMInventoryEventType.Redraw:
                break;
            case MMInventoryEventType.ContentChanged:
                break;
            case MMInventoryEventType.InventoryOpens:
                break;
            case MMInventoryEventType.InventoryCloseRequest:
                break;
            case MMInventoryEventType.InventoryCloses:
                break;
            case MMInventoryEventType.InventoryLoaded:
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Ȱ��ȭ�ϸ� MMInventoryEvents ������ �����մϴ�.
    /// </summary>
    protected virtual void OnEnable()
    {
        this.MMEventStartListening<MMInventoryEvent>();
        this.MMEventStartListening<MMGameEvent>();
    }

    /// <summary>
    /// ��Ȱ��ȭ�Ǹ� MMInventoryEvents ������ �����˴ϴ�.
    /// </summary>
    protected virtual void OnDisable()
    {
        this.MMEventStopListening<MMInventoryEvent>();
        this.MMEventStopListening<MMGameEvent>();
    }

    public void OnMMEvent(MMGameEvent gameEvent)
    {
        //inventory.ResetSavedInventory();//�κ��丮 ���� �����

        //inventory.SaveInventory();//�κ��丮 ���� �����
        //inventory.LoadSavedInventory();//�κ��丮 �ҷ�����

        if (gameEvent.EventName == "StartItemAdd")
        {
            Debug.Log("�ʱ� ������ ä�����");
            foreach (InventoryItem item in InventoryItems)
            {
                if (item.ItemID == "RockNormal")
                {
                    inventory.AddItem(item, 5);
                    break;
                }
            }

            foreach (InventoryItem item in InventoryItems)
            {
                if (item.ItemID == "HealRockNormal")
                {
                    inventory.AddItem(item, 2);
                    break;
                }
            }

            foreach (InventoryItem item in InventoryItems)
            {
                if (item.ItemID == "IceRockNormal")
                {
                    inventory.AddItem(item, 2);
                    break;
                }
            }

            //�׽�Ʈ
            foreach (InventoryItem item in InventoryItems)
            {
                if (item.ItemID == "DragonflyPet")
                {
                    inventory.AddItem(item, 5);
                    break;
                }
            }

            inventory.SaveInventory();
        }
    }
}
