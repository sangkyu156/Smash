using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Invoke("CutSceneEND", 44.5f);
    }

    public void CutSceneEND()
    {
        LevelManager.Instance.GotoLevel("Village");
    }
}
