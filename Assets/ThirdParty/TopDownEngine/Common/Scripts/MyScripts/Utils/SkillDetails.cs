using MoreMountains.InventoryEngine;
using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class SkillDetails : MonoBehaviour
{
    [Header("Components")]
    [MMInformation("여기에서 패널 구성 요소를 바인딩해야 합니다.", MMInformationAttribute.InformationType.Info, false)]
    /// 아이콘 컨테이너 객체
    public Image Icon;
    /// 제목 컨테이너 객체
    public TextMeshProUGUI Title;
    /// 간단한 설명 컨테이너 객체
    public TextMeshProUGUI ShortDescription;
    /// 설명 컨테이너 객체
    public TextMeshProUGUI Description;
    /// 수량 컨테이너 객체
    public TextMeshProUGUI SkillLevel;
    // 가지고있는돈 표시하는 오브젝트
    public GameObject MoneyObject;
    // 현재 가지고있는돈
    public TextMeshProUGUI CurMoney;
    // 필요한돈
    public TextMeshProUGUI MoneyNeeded;
    // 구매수량
    public TextMeshProUGUI QuantityText;
    // 초기화시 안내문구
    public TextMeshProUGUI ResetInfo;
    // 구입버튼
    public Button buyButton;
    // 초기화 ok버튼
    public Button ResetOK;
         
    public Skillbase curSkill = null;
    int Quantity = 0;//구매수량

    public void DisplaySkillDetails(Skillbase skill)
    {
        StartCoroutine(FillDetailFields(skill, 0f));
        MoneyObject.SetActive(true);
        CurMoney.text = DataManager.Instance.datas.CurPlayerGold.ToString();
    }

    /// <summary>
    /// 항목의 메타데이터로 다양한 세부 정보 필드를 채웁니다.
    /// </summary>
    /// <returns>세부사항 필드.</returns>
    /// <param name="item">Item.</param>
    /// <param name="initialDelay">초기 지연.</param>
    protected virtual IEnumerator FillDetailFields(Skillbase skill, float initialDelay)
    {
        yield return new WaitForSeconds(initialDelay);
        curSkill = skill;
        if (Title != null) { Title.text = skill.SkillName; }
        if (ShortDescription != null) { ShortDescription.text = skill.ShortDescription; }
        if (Description != null) { Description.text = skill.Description; }
        if (SkillLevel != null) { SkillLevel.text = skill.SkillLevel.ToString(); }
        if (Icon != null) { Icon.sprite = skill.SkillIcon; }
        ResetOK.interactable = true;
        ResetPurchaseQuantity();
    }

    public void ResetPurchaseQuantity()
    {
        Quantity = 0;
        MoneyNeeded.text = "0";
        QuantityText.text = Quantity.ToString();
    }

    public void SkillPointUpbutton()
    {
        if(curSkill == null) return;
        //현재 최대레벨인지 확인
        if(curSkill.maxLevel <= curSkill.SkillLevel)
        {
            return;
        }

        Quantity++;
        if (curSkill.maxLevel < curSkill.SkillLevel + Quantity)
            Quantity--;

        int totalPrice = 0;
        for (int i = curSkill.SkillLevel; i < Quantity + curSkill.SkillLevel; i++)
        {
            totalPrice += curSkill.price[i];
        }

        MoneyNeeded.text = totalPrice.ToString();
        QuantityText.text = Quantity.ToString();

        if (totalPrice > DataManager.Instance.datas.CurPlayerGold)
            buyButton.interactable = false;
        else
            buyButton.interactable = true;
    }

    public void SkillPointDownbutton()
    {
        if (curSkill == null) return;
        //현재 최대레벨인지 확인
        if (curSkill.maxLevel <= curSkill.SkillLevel)
        {
            return;
        }

        Quantity--;
        if (Quantity < 0)
            Quantity++;

        int totalPrice = 0;
        for (int i = curSkill.SkillLevel; i < Quantity + curSkill.SkillLevel; i++)
        {
            totalPrice += curSkill.price[i];
        }

        MoneyNeeded.text = totalPrice.ToString();
        QuantityText.text = Quantity.ToString();

        if (totalPrice > DataManager.Instance.datas.CurPlayerGold)
            buyButton.interactable = false;
        else
            buyButton.interactable = true;
    }

    public void SkillBuyButton()
    {
        if (curSkill == null) return;
        DataManager.Instance.datas.CurPlayerGold -= int.Parse(MoneyNeeded.text);
        switch(curSkill.SkillNumber)
        {
            case 0: DataManager.Instance.datas.S_Health += Quantity; break;
            case 1: DataManager.Instance.datas.S_ClearReward += Quantity; break;
            case 2: DataManager.Instance.datas.S_Stamina += Quantity; break;
        }
        DataManager.Instance.PlayerRefresh();
        CurMoney.text = DataManager.Instance.datas.CurPlayerGold.ToString();

        MMGameEvent.Trigger("SkillBuy");
        DisplaySkillDetails(curSkill);
        DataManager.Instance.DataSave();
    }

    public void ResetInformation()
    {
        if (curSkill == null) return;
        int returnValue = 0;

        for (int i = curSkill.SkillLevel; i > 0;i--)
        {
            returnValue += curSkill.price[i - 1];//curSkill.price는 [0] 부터 시작한다.
        }
        ResetInfo.text = TextUtil.GetText("game:info:refundablegold") + $"<color=#FF7D00>{(returnValue - 500)}</color>";

        if (DataManager.Instance.datas.CurPlayerGold < 500)
            ResetOK.interactable = false;
    }

    public void ResetButton()
    {
        if (curSkill == null) return;
        int returnValue = 0;

        for (int i = curSkill.SkillLevel; i > 0; i--)
        {
            returnValue += curSkill.price[i - 1];//curSkill.price는 [0] 부터 시작한다.
        }

        DataManager.Instance.datas.CurPlayerGold += (returnValue - 500);
        switch(curSkill.SkillNumber)
        {
            case 0: DataManager.Instance.datas.S_Health = 0; break;
            case 1: DataManager.Instance.datas.S_ClearReward = 0; break;
            case 2: DataManager.Instance.datas.S_Stamina = 0; break;
        }

        DataManager.Instance.PlayerRefresh();
        MMGameEvent.Trigger("SkillBuy");
        DisplaySkillDetails(curSkill);
        DataManager.Instance.DataSave();
    }

    public void LockSkill()
    {
        buyButton.interactable = false;
    }
}
