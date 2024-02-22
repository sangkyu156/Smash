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
        Debug.Log("1");
        if (collision.transform.tag == "Player")
        {
            Debug.Log("2");
            damageTime -= Time.deltaTime;
            if (damageTime <= 0)
            {
                damageTime = 0.5f;
                collision.gameObject.GetComponent<Health>().Damage(10f, gameObject, 0.5f, 0.5f, LastDamageDirection);
                Debug.Log("3");
            }
        }
    }
}
