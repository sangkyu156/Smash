using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffSpeedup : MonoBehaviour
{
    float speedIncreaseTime = 5;//������ �ӵ��� ��ŭ �����Ǿ���ϴ���
    float increaseSpeed = 2; //��ŭ �ӵ��� �����ؾ��ϴ���
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            speedIncreaseTime = 5;
            increaseSpeed = 2;

            other.GetComponent<CharacterMovement>().ApplyMovementMultiplier(increaseSpeed, speedIncreaseTime);
            CreateManager.Instance.BuffSpeedupEffectStart(speedIncreaseTime);
            other.GetComponent<PlayerEffectsController>().SpeedupPlay();

            gameObject.SetActive(false);
        }
    }
}
