using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartGuide : MonoBehaviour
{

    private void Start()
    {
        Time.timeScale = 0;
    }

    public void ClosePopup()
    {
        InputManager.Instance.InputDetectionActive = true;
        Time.timeScale = 1;
        this.gameObject.SetActive(false);
    }
}
