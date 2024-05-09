using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirestormNormal : MonoBehaviour
{
    void Start()
    {
        //30초뒤 비활성화
        Invoke("DisableGameObject", 30f);
    }

    void DisableGameObject()
    {
        gameObject.SetActive(false);
    }
}
