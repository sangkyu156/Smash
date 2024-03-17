using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class StageSelectionManager : MonoBehaviour
{
    public GameObject[] stagePopups;

    public LayerMask targetLayer;
    int index;//선택한 스테이지의 인덱스 (stagePopups 배열 인덱스)
    bool isPopup;// 이값이 false이면 레이캐스트 중단
    float timer;// 팝업이 켜져있는지 확인하는 간격(초)

    private void Awake()
    {
        isPopup = false;
        int layerMask = 1 << LayerMask.NameToLayer("Stage");
        targetLayer = layerMask;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= 0.2f)
        {
            timer = 0;
            PopupGetActive();
        }

        if (isPopup == true)
            MouseCursorUpdate();
    }


    void MouseCursorUpdate()
    {
        //바꾼코딩
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        Debug.DrawRay(ray.origin, ray.direction * 100, Color.cyan);

        // 레이캐스트 수행, targetLayer = 충돌할 레이어
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, targetLayer) && Input.GetMouseButtonDown(0))
        {
            Debug.Log("스테이지 클릭함");
            string stageNumber = hit.collider.gameObject.name;

            stagePopups[GetCurrentStage(stageNumber)].SetActive(true);
        }
    }

    int GetCurrentStage(string _stageNumber)
    {
        switch (_stageNumber)
        {
            case "01": index = 0; GameManager.Instance.SetCurrentStage(Define.Stage.Stage01); break;
            case "02": index = 1; GameManager.Instance.SetCurrentStage(Define.Stage.Stage02); break;
            case "03": index = 2; GameManager.Instance.SetCurrentStage(Define.Stage.Stage03); break;
            case "04": index = 3; GameManager.Instance.SetCurrentStage(Define.Stage.Stage04); break;
            case "05": index = 4; GameManager.Instance.SetCurrentStage(Define.Stage.Stage05); break;
            case "06": index = 5; GameManager.Instance.SetCurrentStage(Define.Stage.Stage06); break;
            case "07": index = 6; GameManager.Instance.SetCurrentStage(Define.Stage.Stage07); break;
            case "08": index = 7; GameManager.Instance.SetCurrentStage(Define.Stage.Stage08); break;
            case "09": index = 8; GameManager.Instance.SetCurrentStage(Define.Stage.Stage09); break;
            case "10": index = 9; GameManager.Instance.SetCurrentStage(Define.Stage.Stage10); break;
            case "11": index = 10; GameManager.Instance.SetCurrentStage(Define.Stage.Stage11); break;
            case "12": index = 11; GameManager.Instance.SetCurrentStage(Define.Stage.Stage12); break;
            case "13": index = 12; GameManager.Instance.SetCurrentStage(Define.Stage.Stage13); break;
            case "14": index = 13; GameManager.Instance.SetCurrentStage(Define.Stage.Stage14); break;
        }

        return index;
    }

    //팝업 켜져있는지 확인하는 함수
    bool PopupGetActive()
    {
        int count = 0;

        for (int i = 0; i < stagePopups.Length; i++)
        {
            if (stagePopups[i].activeSelf)
            {
                isPopup = false;
            }
            else
                count++;
        }

        if(count == stagePopups.Length)
        {
            isPopup = true;
        }

        return isPopup;
    }
}
