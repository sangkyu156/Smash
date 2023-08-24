using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationEvent : MonoBehaviour
{
    public GameObject weaponFX1;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void weaponFX1_On()
    {
        if (weaponFX1.activeSelf == false)
            weaponFX1.SetActive(true);
    }

    public void weaponFX1_Off()
    {
        weaponFX1.SetActive(false);
    }
}
