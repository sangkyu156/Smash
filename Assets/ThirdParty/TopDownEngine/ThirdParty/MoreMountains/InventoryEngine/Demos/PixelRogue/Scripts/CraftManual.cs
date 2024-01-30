using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

//[System.Serializable]
//public class Craft
//{
//    public string craftName;
//    public GameObject go_prefab;
//    public GameObject go_PreviewPrefab;
//}

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
    //public Transform tf_Player;  

    public LayerMask targetLayer;
    [SerializeField]
    private float range;

    //public CraftManual(GameObject preview)
    //{
    //    go_Preview = preview;
    //}

    private void Awake()
    {
        int layerMask = 1 << LayerMask.NameToLayer("Ground");
        targetLayer = layerMask;
    }

    private void Start()
    {
        SlotClick();
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

    public void SlotClick()
    {
        go_Preview = Instantiate(go_Preview, Vector3.zero, Quaternion.identity);
        isPreviewActivated = true;
        //go_BaseUI.SetActive(false);
    }

    void PreviewPositionUpdate()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        Debug.DrawRay(ray.origin, ray.direction * 100, Color.cyan);

        // 레이캐스트 수행, targetLayer = 충돌할 레이어
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, targetLayer))
        {
            Vector3 newPosition = hit.point;
            newPosition.y = newPosition.y + (go_Preview.transform.lossyScale.y * 0.5f);
            go_Preview.transform.position = newPosition;
            Debug.Log("돌맹이 위치 조정함");
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