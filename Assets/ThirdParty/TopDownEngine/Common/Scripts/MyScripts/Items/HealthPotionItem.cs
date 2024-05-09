using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPotionItem : MonoBehaviour
{
    float healHealth = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            if(gameObject.name == "HealthPotionNormal")
            {
                healHealth = GetMaxHealthPercentage1(other.GetComponent<Health>().MaximumHealth);
            }
            else if(gameObject.name == "HealthPotionRare")
            {
                healHealth = GetMaxHealthPercentage2(other.GetComponent<Health>().MaximumHealth);
            }
            else//¿Ø¥œ≈©
            {
                healHealth = GetMaxHealthPercentage3(other.GetComponent<Health>().MaximumHealth);
            }
            
            other.GetComponent<Health>().ReceiveHealth(healHealth, this.gameObject);
            other.GetComponent<PlayerEffectsController>().Heal_1_Play();

            gameObject.SetActive(false);
        }
    }

    public float GetMaxHealthPercentage1(float maxHealth)
    {
        return maxHealth * 0.1f;
    }

    public float GetMaxHealthPercentage2(float maxHealth)
    {
        return maxHealth * 0.2f;
    }

    public float GetMaxHealthPercentage3(float maxHealth)
    {
        return maxHealth * 0.3f;
    }
}
