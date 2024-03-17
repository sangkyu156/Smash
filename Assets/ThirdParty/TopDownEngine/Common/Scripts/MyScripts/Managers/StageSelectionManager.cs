using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class StageSelectionManager : MonoBehaviour
{
    public GameObject[] stagePopups;

    public LayerMask targetLayer;
    int index;//������ ���������� �ε��� (stagePopups �迭 �ε���)
    bool isPopup;// �̰��� false�̸� ����ĳ��Ʈ �ߴ�
    float timer;// �˾��� �����ִ��� Ȯ���ϴ� ����(��)

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
        //�ٲ��ڵ�
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        Debug.DrawRay(ray.origin, ray.direction * 100, Color.cyan);

        // ����ĳ��Ʈ ����, targetLayer = �浹�� ���̾�
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, targetLayer) && Input.GetMouseButtonDown(0))
        {
            Debug.Log("�������� Ŭ����");
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

    //�˾� �����ִ��� Ȯ���ϴ� �Լ�
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
