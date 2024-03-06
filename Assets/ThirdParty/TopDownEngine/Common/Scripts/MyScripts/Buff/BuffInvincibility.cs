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

            GameObject effect = CreateManager.Instantiate("Battlefield/Buff/BuffInvincibilityEffect");
            effect.transform.position = other.transform.position;

            Destroy(this.gameObject);
        }
    }
}
