using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonGototown : TopDownMonoBehaviour
{
    public void GotoTown()
    {
        LevelManager.Instance.GotoLevel("Village");
    }

    public void GotoTown2()
    {
        MMSceneLoadingManager.LoadScene("Village");
    }

    public void ClosePopup()
    {
        this.gameObject.SetActive(false);
    }
}
