using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffHeart : MonoBehaviour
{
    float plusHealth = 10;
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            plusHealth = 10;

            other.GetComponent<Health>().UpdateMaxHealth(plusHealth);

            GameObject effect = CreateManager.Instantiate("Battlefield/Buff/BuffHeartEffect");
            effect.transform.position = other.transform.position;

            Destroy(this.gameObject);
        }
    }
}
