using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleDestroy : MonoBehaviour
{
    new private ParticleSystem particleSystem;

    private void Awake()
    {
        particleSystem = GetComponent<ParticleSystem>();
    }

    private void Start()
    {
        Invoke("ObjectDestroy", particleSystem.main.duration + 0.2f);
    }

    void ObjectDestroy()
    {
        Destroy(gameObject);
    }
}
