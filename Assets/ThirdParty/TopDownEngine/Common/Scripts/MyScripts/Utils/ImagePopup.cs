using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using MoreMountains.TopDownEngine;

public class ImagePopup : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject popupPrefab; // 팝업창 프리팹을 설정합니다.
    private GameObject currentPopup; // 현재 팝업창을 저장합니다.

    Image thisImage;

    void Start()
    {
        // UI 오브젝트에 이벤트 리스너를 추가합니다.
        thisImage = GetComponent<Image>();
        //thisImage.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);
        thisImage.raycastTarget = true;
    }
    void ShowPopup()
    {
        // 마우스 위치에 팝업창을 생성합니다.
        currentPopup = CreateManager.Instantiate(popupPrefab, this.transform);
        Debug.Log("팝업생성함");
    }

    void HidePopup()
    {
        // 팝업창을 제거합니다.
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
