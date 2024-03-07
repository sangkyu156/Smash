using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffSpeedup : MonoBehaviour
{
    float speedIncreaseTime = 5;//증가한 속도가 얼만큼 유지되어야하는지
    float increaseSpeed = 2; //얼만큼 속도가 증가해야하는지
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            speedIncreaseTime = 5;
            increaseSpeed = 2;

            other.GetComponent<CharacterMovement>().ApplyMovementMultiplier(increaseSpeed, speedIncreaseTime);
            CreateManager.Instance.BuffSpeedupEffectStart(speedIncreaseTime);

            GameObject effect = CreateManager.Instantiate("Battlefield/Buff/BuffSpeedupEffect");
            effect.transform.position = other.transform.position;

            Destroy(this.gameObject);
        }
    }
}
