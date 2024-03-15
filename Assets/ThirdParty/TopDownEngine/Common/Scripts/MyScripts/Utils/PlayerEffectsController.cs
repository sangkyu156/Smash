using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEffectsController : MonoBehaviour
{
    public ParticleSystem heal_1; //¹öÇÁÈú
    public ParticleSystem heal_2; //HealRock Èú
    public ParticleSystem heart;  
    public ParticleSystem invincibility;
    public ParticleSystem speedup;
    public ParticleSystem stamina;

    public void Heal_1_Play()
    {
        if(!heal_1.gameObject.activeSelf)
            heal_1.gameObject.SetActive(true);

        heal_1.Play();
    }

    public void Heal_2_Play()
    {
        if (!heal_2.gameObject.activeSelf)
            heal_2.gameObject.SetActive(true);

        heal_2.Play();
    }

    public void HeartPlay()
    {
        if (!heart.gameObject.activeSelf)
            heart.gameObject.SetActive(true);

        heart.Play();
    }

    public void invincibilityPlay()
    {
        if (!invincibility.gameObject.activeSelf)
            invincibility.gameObject.SetActive(true);

        invincibility.Play();
    }

    public void SpeedupPlay()
    {
        if (!speedup.gameObject.activeSelf)
            speedup.gameObject.SetActive(true);

        speedup.Play();
    }

    public void StaminaPlay()
    {
        if (!stamina.gameObject.activeSelf)
            stamina.gameObject.SetActive(true);

        stamina.Play();
    }
}
