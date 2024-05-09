using MoreMountains.InventoryEngine;
using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelperButton : MonoBehaviour
{
    public GameObject helperPopup_KR;
    public GameObject helperPopup_EN;

    public void OnHelperPopup()
    {
        Time.timeScale = 0;
        if(TextUtil.languageNumber == 0)
        {
            LevelManager.Instance.HelperPopupIsOpen = true;
            helperPopup_KR.SetActive(true);
        }
        else
        {
            LevelManager.Instance.HelperPopupIsOpen = true;
            helperPopup_EN.SetActive(true);
        }
    }

    public void OffHelperPopup()
    {
        Time.timeScale = 1;
        LevelManager.Instance.HelperPopupIsOpen = false;
        helperPopup_KR.SetActive(false);
        helperPopup_EN.SetActive(false);
    }
}
