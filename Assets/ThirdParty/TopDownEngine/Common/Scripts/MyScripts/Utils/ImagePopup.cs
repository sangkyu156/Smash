using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using MoreMountains.TopDownEngine;

public class ImagePopup : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject popupPrefab; // �˾�â �������� �����մϴ�.
    private GameObject currentPopup; // ���� �˾�â�� �����մϴ�.

    Image thisImage;

    void Start()
    {
        // UI ������Ʈ�� �̺�Ʈ �����ʸ� �߰��մϴ�.
        thisImage = GetComponent<Image>();
        //thisImage.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);
        thisImage.raycastTarget = true;
    }
    void ShowPopup()
    {
        // ���콺 ��ġ�� �˾�â�� �����մϴ�.
        currentPopup = CreateManager.Instantiate(popupPrefab, this.transform);
        Debug.Log("�˾�������");
    }

    void HidePopup()
    {
        // �˾�â�� �����մϴ�.
        Destroy(currentPopup);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ShowPopup();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HidePopup();
    }
}
