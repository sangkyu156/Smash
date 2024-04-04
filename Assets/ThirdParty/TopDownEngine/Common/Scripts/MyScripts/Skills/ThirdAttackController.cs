using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdAttackController : MonoBehaviour
{
    public GameObject[] ThirdAttacks;

    private void OnEnable()
    {
        switch (DataManager.Instance.datas.curThirdAttack)
        {
            case ThirdAttack.Holy: ThirdAttacks[0].SetActive(true); break;
            case ThirdAttack.Ice: ThirdAttacks[1].SetActive(true); break;
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
