using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ProgressValueText : MonoBehaviour
{
    public TextMeshProUGUI progressValue;
    int stage;

    private void OnEnable()
    {
        stage = (int)GameManager.Instance.stage;
        progressValue.text = stage + "/50";
    }
}
