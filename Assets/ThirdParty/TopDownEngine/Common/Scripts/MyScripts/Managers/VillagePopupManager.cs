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

    //현재 선택중인 아이템 정보 가져오기
    public void GetCurrentItemInformation()
    {
        curItem = inventoryDetails.SetCurrentItemInformation();
        curQuantity = itemStore.quantity;
    }

    //여기다 OK 버튼 누르면 실행되는 메소드 만들어야함
    public void BuyItem()
    {
        GetCurrentItemInformation();
        playerData.BuyItem(curItem, curQuantity);
        itemStore.SetPlayerGold();
        //인벤토리에 아이템 넣어주면됨
        curItem.TargetInventoryName = "RogueMainInventory";
        inventory.AddItem(curItem, curQuantity);
        inventory.SaveInventory();

        OffBuyPopup();
    }

    public void SellItem()
    {
        GetCurrentItemInformation();
        //현재 아이템 몇개 판매할껀지 알아와서 그만큼 플레이어 돈에 추가하고 아이템 삭제하면됨
    }
}
