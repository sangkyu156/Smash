using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvincibilityPotionItem : MonoBehaviour
{
    float invincibilityTime = 3;
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            if (gameObject.name == "InvincibilityPotionNormal")
            {
                invincibilityTime = 3;
            }
            else if (gameObject.name == "InvincibilityPotionRare")
            {
                invincibilityTime = 6;
            }
            else//¿Ø¥œ≈©
            {
                invincibilityTime = 10;
            }

            other.GetComponent<Health>().DamageDisabled(invincibilityTime);
            CreateManager.Instance.BuffInvincibilityEffectStart(invincibilityTime);
            other.GetComponent<PlayerEffectsController>().invincibilityPlay();

            gameObject.SetActive(false);
        }
    }
}
