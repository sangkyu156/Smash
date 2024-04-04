using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AcidRain : MonoBehaviour
{
    float damageTime = 0;
    public Vector3 LastDamageDirection;
    private void OnCollisionStay(Collision collision)
    {
        if (collision.transform.tag == "Player")
        {
            damageTime -= Time.deltaTime;
            if (damageTime <= 0)
            {
                damageTime = 0.5f;
                collision.gameObject.GetComponent<Health>().Damage(10f, gameObject, 0.5f, 0.5f, LastDamageDirection);
            }
        }
    }
}
