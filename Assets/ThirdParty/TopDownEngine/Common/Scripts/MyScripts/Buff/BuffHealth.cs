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
            //캐릭터의 최대 체력에서 30% 구하기
            float healHealth = GetMaxHealthPercentage(other.GetComponent<Health>().MaximumHealth);

            other.GetComponent<Health>().ReceiveHealth(healHealth, this.gameObject);

            GameObject effect = CreateManager.Instantiate("Battlefield/Buff/BuffHealthEffect");
            effect.transform.position = other.transform.position;

            Destroy(this.gameObject);
        }
    }

    public float GetMaxHealthPercentage(float maxHealth)
    {
        return maxHealth * 0.3f;
    }
}
