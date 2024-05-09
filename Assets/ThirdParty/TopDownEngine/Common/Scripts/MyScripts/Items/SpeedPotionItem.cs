using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedPotionItem : MonoBehaviour
{
    float speedIncreaseTime = 5;//������ �ӵ��� ��ŭ �����Ǿ���ϴ���
    float increaseSpeed = 2; //��ŭ �ӵ��� �����ؾ��ϴ���
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            increaseSpeed = 2;

            if (gameObject.name == "SpeedPotionNormal")
            {
                speedIncreaseTime = 10;
            }
            else if (gameObject.name == "SpeedPotionRare")
            {
                speedIncreaseTime = 20;
            }
            else//����ũ
            {
                speedIncreaseTime = 30;
            }

            other.GetComponent<CharacterMovement>().ApplyMovementMultiplier(increaseSpeed, speedIncreaseTime);
            CreateManager.Instance.BuffSpeedupEffectStart(speedIncreaseTime);
            other.GetComponent<PlayerEffectsController>().SpeedupPlay();

            gameObject.SetActive(false);
        }
    }
}
