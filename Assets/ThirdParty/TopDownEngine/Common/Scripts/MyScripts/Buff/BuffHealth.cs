using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffHealth : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            //ĳ������ �ִ� ü�¿��� 30% ���ϱ�
            float healHealth = GetMaxHealthPercentage(other.GetComponent<Health>().MaximumHealth);

            other.GetComponent<Health>().ReceiveHealth(healHealth, this.gameObject);
            other.GetComponent<PlayerEffectsController>().Heal_1_Play();

            gameObject.SetActive(false);
        }
    }

    public float GetMaxHealthPercentage(float maxHealth)
    {
        return maxHealth * 0.3f;
    }
}
