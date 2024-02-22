using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageZone : MonoBehaviour
{
    string curStage = string.Empty;

    public GameObject stagePopup;
    

    void Start()
    {
        curStage = gameObject.name;
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
            case "01": GameManager.Instance.SetCurrentStage(Define.Stage.Stage01); stagePopup.SetActive(true); break;
            case "02": GameManager.Instance.SetCurrentStage(Define.Stage.Stage02); stagePopup.SetActive(true); break;
            case "03": GameManager.Instance.SetCurrentStage(Define.Stage.Stage03); stagePopup.SetActive(true); break;
            case "04": GameManager.Instance.SetCurrentStage(Define.Stage.Stage04); stagePopup.SetActive(true); break;
            case "05": GameManager.Instance.SetCurrentStage(Define.Stage.Stage05); stagePopup.SetActive(true); break;
            case "06": GameManager.Instance.SetCurrentStage(Define.Stage.Stage06); stagePopup.SetActive(true); break;
            case "07": GameManager.Instance.SetCurrentStage(Define.Stage.Stage07); stagePopup.SetActive(true); break;
            case "08": GameManager.Instance.SetCurrentStage(Define.Stage.Stage08); stagePopup.SetActive(true); break;
            case "09": GameManager.Instance.SetCurrentStage(Define.Stage.Stage09); stagePopup.SetActive(true); break;
            case "10": GameManager.Instance.SetCurrentStage(Define.Stage.Stage10); stagePopup.SetActive(true); break;
            default: GameManager.Instance.SetCurrentStage(Define.Stage.Stage01); break;
        }
    }
}
