using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartGuide : MonoBehaviour
{

    public void ClosePopup()
    {
        InputManager.Instance.InputDetectionActive = true;
        this.gameObject.SetActive(false);
    }
}
