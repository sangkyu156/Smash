using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffInvincibility : MonoBehaviour
{
    float invincibilityTime = 5;
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            invincibilityTime = 5;

            other.GetComponent<Health>().DamageDisabled(invincibilityTime);
            CreateManager.Instance.BuffInvincibilityEffectStart(invincibilityTime);
            other.GetComponent<PlayerEffectsController>().invincibilityPlay();

            gameObject.SetActive(false);
        }
    }
}
