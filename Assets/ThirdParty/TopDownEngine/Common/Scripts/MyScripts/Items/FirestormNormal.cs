using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirestormNormal : MonoBehaviour
{
    void Start()
    {
        //30�ʵ� ��Ȱ��ȭ
        Invoke("DisableGameObject", 30f);
    }

    void DisableGameObject()
    {
        gameObject.SetActive(false);
    }
}
