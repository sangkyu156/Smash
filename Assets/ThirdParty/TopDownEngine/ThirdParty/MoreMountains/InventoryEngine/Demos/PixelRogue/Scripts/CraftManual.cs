using MoreMountains.InventoryEngine;
using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class CraftManual : MonoBehaviour
{
    private bool isPreviewActivated = false; 

    public GameObject go_Preview; 
    public GameObject go_Prefab;
    public Inventory inventory;

    public LayerMask targetLayer;
    [SerializeField]
    private float range;

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
        if (isPreviewActivated)
        {
            PreviewPositionUpdate();
        }
    }

    public void SlotClick()
    {
        go_Preview = Instantiate(go_Preview, Vector3.zero, Quaternion.identity, inventory.ParentPreviewObjects.gameObject.transform);
        isPreviewActivated = true;

    }

    void PreviewPositionUpdate()
    {
        //바꾼코딩
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        Debug.DrawRay(ray.origin, ray.direction * 100, Color.cyan);

        // 레이캐스트 수행, targetLayer = 충돌할 레이어
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, targetLayer))
        {
            Vector3 newPosition = hit.point;
            newPosition.y = newPosition.y + 0.5f;
            go_Preview.transform.position = newPosition;
            Vector3 newRotation = go_Preview.transform.rotation.eulerAngles;
            newRotation.x = -90;
            go_Preview.transform.rotation = Quaternion.Euler(newRotation);
        }
    }

    public void Build()
    {
        if (isPreviewActivated && go_Preview.GetComponent<PreviewObject>().isInstallable)
        {
            int randomAngleY = Random.Range(0, 180);
            Quaternion randomRotation = Quaternion.Euler(-90, randomAngleY, 0);
            Instantiate(go_Prefab, go_Preview.transform.position, randomRotation);
            if(go_Preview !=  null)
                Destroy(go_Preview);
            isPreviewActivated = false;
            go_Preview = null;
            go_Prefab = null;
            inventory.isInstalling = false;
            gameObject.SetActive(false);
            Time.timeScale = 1;
            MMGameEvent.Trigger("Installed");
        }
    }

    public void BuildCancel()
    {
        if (go_Preview != null)
            Destroy(go_Preview);
        isPreviewActivated = false;
        go_Preview = null;
        go_Prefab = null;
        inventory.isInstalling = false;
        gameObject.SetActive(false);
        Time.timeScale = 1;
    }
}