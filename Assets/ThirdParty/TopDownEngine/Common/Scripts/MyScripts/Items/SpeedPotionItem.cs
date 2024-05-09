using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedPotionItem : MonoBehaviour
{
    float speedIncreaseTime = 5;//증가한 속도가 얼만큼 유지되어야하는지
    float increaseSpeed = 2; //얼만큼 속도가 증가해야하는지
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
            else//유니크
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
