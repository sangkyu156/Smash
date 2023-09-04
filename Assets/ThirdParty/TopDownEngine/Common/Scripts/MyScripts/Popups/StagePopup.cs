using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StagePopup : MonoBehaviour
{


    void Start()
    {
        Time.timeScale = 0f;
    }

    void Update()
    {
        
    }

    public void PopupClose()
    {
        Time.timeScale = 1f;
        Destroy(gameObject);
    }
}
