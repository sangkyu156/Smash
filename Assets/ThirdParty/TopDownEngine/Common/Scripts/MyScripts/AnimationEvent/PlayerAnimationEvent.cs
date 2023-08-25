using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationEvent : MonoBehaviour
{
    public GameObject weaponFX1;
    public GameObject weaponFX2;
    public GameObject weaponFX3;

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

    public void weaponFX2_On()
    {
        if (weaponFX2.activeSelf == false)
            weaponFX2.SetActive(true);
    }

    public void weaponFX2_Off()
    {
        weaponFX2.SetActive(false);
    }

    public void weaponFX3_On()
    {
        if (weaponFX3.activeSelf == false)
            weaponFX3.SetActive(true);
    }

    public void weaponFX3_Off()
    {
        weaponFX3.SetActive(false);
    }
}
