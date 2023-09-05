using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageZone : MonoBehaviour
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
        

        switch (curStage)
        {
            case "01": GameManager.Instance.SetCurrentStage(Define.Stage.Stage01); break;
            case "02": GameManager.Instance.SetCurrentStage(Define.Stage.Stage02); break;
            case "03": GameManager.Instance.SetCurrentStage(Define.Stage.Stage03); break;
            case "04": GameManager.Instance.SetCurrentStage(Define.Stage.Stage04); break;
            case "05": GameManager.Instance.SetCurrentStage(Define.Stage.Stage05); break;
            case "06": GameManager.Instance.SetCurrentStage(Define.Stage.Stage06); break;
            case "07": GameManager.Instance.SetCurrentStage(Define.Stage.Stage07); break;
            case "08": GameManager.Instance.SetCurrentStage(Define.Stage.Stage08); break;
            case "09": GameManager.Instance.SetCurrentStage(Define.Stage.Stage09); break;
            case "10": GameManager.Instance.SetCurrentStage(Define.Stage.Stage10); break;
            default: GameManager.Instance.SetCurrentStage(Define.Stage.Stage01); break;
        }

        GameManager.Resource.Instantiate("Popups/StagePopup");
    }
}
