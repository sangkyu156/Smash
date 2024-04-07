using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;
using MoreMountains.TopDownEngine;

public class RewardGold : MonoBehaviour
{
    public TextMeshProUGUI rewardGoldText;
    public float duration = 5f;
    public int rewardGold = 0;

    void Start()
    {
        DOTween.defaultTimeScaleIndependent = true;

        rewardGold = GameManager.Instance.SetRewardGold();

        // ���� ����
        int startNumber = 1;

        // DOTween�� ����Ͽ� ���� ���ں��� ��ǥ ���ڱ��� 5�� ���� ������Ű�� �ִϸ��̼� ����
        DOTween.To(() => startNumber, x => startNumber = x, rewardGold, duration)
            .OnUpdate(() => rewardGoldText.text = startNumber.ToString())
            .SetEase(Ease.InCirc);
    }
}
