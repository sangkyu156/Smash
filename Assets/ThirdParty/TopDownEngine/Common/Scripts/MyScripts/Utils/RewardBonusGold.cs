using DG.Tweening;
using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RewardBonusGold : MonoBehaviour
{
    public TextMeshProUGUI rewardGoldText;
    public float duration = 4;
    public int rewardGold = 0;

    void Start()
    {
        DOTween.defaultTimeScaleIndependent = true;

        rewardGold = (GameManager.Instance.SetRewardGold() * (DataManager.Instance.datas.S_ClearReward * 3)) / 100;

        // ���� ����
        int startNumber = 1;

        // DOTween�� ����Ͽ� ���� ���ں��� ��ǥ ���ڱ��� 5�� ���� ������Ű�� �ִϸ��̼� ����
        DOTween.To(() => startNumber, x => startNumber = x, rewardGold, duration)
            .OnUpdate(() => rewardGoldText.text = startNumber.ToString())
            .SetEase(Ease.InCirc);
    }
}
