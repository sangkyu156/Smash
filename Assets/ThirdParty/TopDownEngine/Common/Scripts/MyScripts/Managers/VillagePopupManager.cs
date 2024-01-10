using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VillagePopupManager : MonoBehaviour
{
    public GameObject BuyPopup;

    private void Start()
    {
        OffBuyPopup();
    }

    public void OnBuyPopup()
    { BuyPopup.SetActive(true); }

    public void OffBuyPopup()
    { BuyPopup.SetActive(false); }
}
