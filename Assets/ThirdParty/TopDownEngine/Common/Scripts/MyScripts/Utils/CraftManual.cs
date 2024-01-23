using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Craft
{
    public string craftName; // �̸�
    public GameObject go_prefab; // ���� ��ġ �� ������
    public GameObject go_PreviewPrefab; // �̸� ���� ������
}

public class CraftManual : MonoBehaviour
{
    private bool isActivated = false; 
    private bool isPreviewActivated = false; 

    //[SerializeField]
    //private GameObject go_BaseUI; 

    //[SerializeField]
    //private Craft[] craft_fire;  

    public GameObject go_Preview; 
    //private GameObject go_Prefab; 

    //[SerializeField]
    public Transform tf_Player;  

    private RaycastHit hitInfo;
    [SerializeField]
    private LayerMask layerMask;
    [SerializeField]
    private float range;


    public void SlotClick(int _slotNumber)
    {
        Debug.Log($"�÷��̾� = {tf_Player.name}");
        Debug.Log($"ť�� = {go_Preview.name}");
        go_Preview = Instantiate(go_Preview, tf_Player.position + tf_Player.forward, Quaternion.identity);
        isPreviewActivated = true;
        //go_BaseUI.SetActive(false);
    }

    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Tab) && !isPreviewActivated)
        //    Window();

        if (isPreviewActivated)
            PreviewPositionUpdate();

        //if (Input.GetButtonDown("Fire1"))
        //    Build();

        //if (Input.GetKeyDown(KeyCode.Escape))
        //    Cancel();
    }

    private void PreviewPositionUpdate()
    {
        if (Physics.Raycast(tf_Player.position, new Vector3(0, 0, 31), out hitInfo, range, layerMask))
        {
            if (hitInfo.transform != null)
            {
                Vector3 _location = hitInfo.point;
                go_Preview.transform.position = _location;

                Debug.Log(_location);
                Debug.Log(go_Preview.transform.position);
            }
        }
    }

    //private void Build()
    //{
    //    if (isPreviewActivated && go_Preview.GetComponent<PreviewObject>().isBuildable())
    //    {
    //        Instantiate(go_Prefab, hitInfo.point, Quaternion.identity);
    //        Destroy(go_Preview);
    //        isActivated = false;
    //        isPreviewActivated = false;
    //        go_Preview = null;
    //        go_Prefab = null;
    //    }
    //}

    //private void Window()
    //{
    //    if (!isActivated)
    //        OpenWindow();
    //    else
    //        CloseWindow();
    //}

    //private void OpenWindow()
    //{
    //    isActivated = true;
    //    go_BaseUI.SetActive(true);
    //}

    //private void CloseWindow()
    //{
    //    isActivated = false;
    //    go_BaseUI.SetActive(false);
    //}

    //private void Cancel()
    //{
    //    if (isPreviewActivated)
    //        Destroy(go_Preview);

    //    isActivated = false;
    //    isPreviewActivated = false;

    //    go_Preview = null;
    //    go_Prefab = null;

    //    go_BaseUI.SetActive(false);
    //}
}