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
}
