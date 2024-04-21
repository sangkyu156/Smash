using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;

public class qweqweqw : MonoBehaviour
{
    void Start()
    {
        Sexgood();
    }

    void Sexgood()
    {
        for (int i = 0; i < 5; i++)
        {
            if(i == 2)
            {
                Debug.Log($"{i}");
                //break;
                return;
            }
        }

        Debug.Log("qwe");
    }
}
