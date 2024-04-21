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
    [MMInformation("���⿡�� �г� ���� ��Ҹ� ���ε��ؾ� �մϴ�.", MMInformationAttribute.InformationType.Info, false)]
    /// ������ �����̳� ��ü
    public Image Icon;
    /// ���� �����̳� ��ü
    public TextMeshProUGUI Title;
    /// ������ ���� �����̳� ��ü
    public TextMeshProUGUI ShortDescription;
    /// ���� �����̳� ��ü
    public TextMeshProUGUI Description;
    /// ���� �����̳� ��ü
    public TextMeshProUGUI SkillLevel;
    // �������ִµ� ǥ���ϴ� ������Ʈ
    public GameObject MoneyObject;
    // ���� �������ִµ�
    public TextMeshProUGUI CurMoney;
    // �ʿ��ѵ�
    public TextMeshProUGUI MoneyNeeded;
    // ���ż���
    public TextMeshProUGUI QuantityText;
    // �ʱ�ȭ�� �ȳ�����
    public TextMeshProUGUI ResetInfo;
    // ���Թ�ư
    public Button buyButton;
    // �ʱ�ȭ ok��ư
    public Button ResetOK;
         
    public Skillbase curSkill = null;
    int Quantity = 0;//���ż���

    public void DisplaySkillDetails(Skillbase skill)
    {
        StartCoroutine(FillDetailFields(skill, 0f));
        MoneyObject.SetActive(true);
        CurMoney.text = DataManager.Instance.datas.CurPlayerGold.ToString();
    }

    /// <summary>
    /// �׸��� ��Ÿ�����ͷ� �پ��� ���� ���� �ʵ带 ä��ϴ�.
    /// </summary>
    /// <returns>���λ��� �ʵ�.</returns>
    /// <param name="item">Item.</param>
    /// <param name="initialDelay">�ʱ� ����.</param>
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
        //���� �ִ뷹������ Ȯ��
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
        //���� �ִ뷹������ Ȯ��
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
            returnValue += curSkill.price[i - 1];//curSkill.price�� [0] ���� �����Ѵ�.
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
            returnValue += curSkill.price[i - 1];//curSkill.price�� [0] ���� �����Ѵ�.
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
