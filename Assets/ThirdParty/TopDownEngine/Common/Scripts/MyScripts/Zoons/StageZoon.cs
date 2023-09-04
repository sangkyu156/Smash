using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageZoon : MonoBehaviour
{
    string curStage = string.Empty;

    void Start()
    {
        curStage = gameObject.name;
    }

    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
            PopupCreate();
    }

    void PopupCreate()
    {
        GameManager.Resource.Instantiate("Popups/StagePopup");
    }
}
