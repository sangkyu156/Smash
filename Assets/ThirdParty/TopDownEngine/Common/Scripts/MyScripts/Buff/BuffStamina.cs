using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffStamina : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            other.GetComponent<CharacterRun>().StaminaReset();
            other.GetComponent<PlayerEffectsController>().StaminaPlay();

            gameObject.SetActive(false);
        }
    }
}
