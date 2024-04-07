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

        // 시작 숫자
        int startNumber = 1;

        // DOTween을 사용하여 시작 숫자부터 목표 숫자까지 5초 동안 증가시키는 애니메이션 실행
        DOTween.To(() => startNumber, x => startNumber = x, rewardGold, duration)
            .OnUpdate(() => rewardGoldText.text = startNumber.ToString())
            .SetEase(Ease.InCirc);
    }
}
