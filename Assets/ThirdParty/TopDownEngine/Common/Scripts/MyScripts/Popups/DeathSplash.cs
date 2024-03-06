using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.ParticleSystem;

public class DeathSplash : MonoBehaviour
{

    public void GoToVillage_Death()
    {
        GameManager.Instance.stage = Define.Stage.Stage00;
        LevelManager.Instance.isClear = false;
        LevelManager.Instance.GotoLevel("Village");
    }

    public void GoToRestart_Death()
    {
        LevelManager.Instance.isClear = false;
        LevelManager.Instance.GotoLevel(SceneManager.GetActiveScene().name);
    }
}
