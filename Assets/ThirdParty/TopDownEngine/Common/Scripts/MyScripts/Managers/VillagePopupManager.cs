using MoreMountains.InventoryEngine;
using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VillagePopupManager : MonoBehaviour
{
    public GameObject BuyPopup;
    public ItemStore itemStore;
    public InventoryDetails inventoryDetails;
    public PlayerDataManager playerData;
    public Inventory inventory;
    int curQuantity;
    InventoryItem curItem;

    private void Start()
    {
        OffBuyPopup();
    }

    public void OnBuyPopup()
    { BuyPopup.SetActive(true); }

    public void OffBuyPopup()
    { BuyPopup.SetActive(false); }

    //���� �������� ������ ���� ��������
    public void GetCurrentItemInformation()
    {
        curItem = inventoryDetails.SetCurrentItemInformation();
        curQuantity = itemStore.quantity;
    }

    //����� OK ��ư ������ ����Ǵ� �޼ҵ� ��������
    public void BuyItem()
    {
        GetCurrentItemInformation();
        playerData.BuyItem(curItem, curQuantity);
        itemStore.SetPlayerGold();
        //�κ��丮�� ������ �־��ָ��
        curItem.TargetInventoryName = "RogueMainInventory";
        inventory.AddItem(curItem, curQuantity);
        inventory.SaveInventory();

        OffBuyPopup();
    }

    public void SellItem()
    {
        GetCurrentItemInformation();
        //���� ������ � �Ǹ��Ҳ��� �˾ƿͼ� �׸�ŭ �÷��̾� ���� �߰��ϰ� ������ �����ϸ��
    }
}
