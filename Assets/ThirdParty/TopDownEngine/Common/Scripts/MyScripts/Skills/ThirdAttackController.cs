using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdAttackController : MonoBehaviour
{
    public GameObject[] ThirdAttacks;

    private void OnEnable()
    {
        switch (PlayerDataManager.GetThirdAttack())
        {
            case PlayerDataManager.ThirdAttack.Holy: ThirdAttacks[0].SetActive(true); break;
            case PlayerDataManager.ThirdAttack.Ice: ThirdAttacks[1].SetActive(true); break;
            default:
                break;
        }
    }

    private void OnDisable()
    {
        foreach (GameObject item in ThirdAttacks)
        {
            item.SetActive(false);
        }
    }
}
