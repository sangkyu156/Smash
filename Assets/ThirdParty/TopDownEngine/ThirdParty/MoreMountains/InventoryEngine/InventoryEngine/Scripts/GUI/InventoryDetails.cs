using DG.DemiEditor;
using MoreMountains.Tools;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Codice.CM.Common.CmCallContext;

namespace MoreMountains.InventoryEngine
{
    /// <summary>
    /// GUI에서 항목의 세부정보를 표시하는 데 사용되는 클래스
    /// </summary>
    public class InventoryDetails : MonoBehaviour, MMEventListener<MMInventoryEvent>
    {
        /// 품목 세부정보를 표시할 참조 재고
        [MMInformation("이 세부정보 패널에 콘텐츠 세부정보를 표시하려는 인벤토리의 이름을 여기에 지정하세요. 전역으로 만들기로 결정할 수도 있습니다. 그렇게 하면 인벤토리에 관계없이 모든 항목의 세부정보가 표시됩니다.", MMInformationAttribute.InformationType.Info, false)]
        public string TargetInventoryName;
        public string PlayerID = "Player1";
        /// 이 패널을 전역으로 만들면 무시됩니다.
        public bool Global = false;
        /// 세부정보가 현재 숨겨져 있는지 여부 
        public bool Hidden { get; protected set; }

        [Header("Default")]
        [MMInformation("HideOnEmptySlot을 선택하면 빈 슬롯을 선택하면 세부정보 패널이 표시되지 않습니다.", MMInformationAttribute.InformationType.Info, false)]
        /// 현재 선택된 슬롯이 비어 있을 때 세부정보 패널을 숨겨야 하는지 여부
        public bool HideOnEmptySlot = true;
        [MMInformation("여기서 세부정보 패널의 모든 필드에 대한 기본값을 설정할 수 있습니다. 이 값은 선택된 항목이 없을 때 표시됩니다(이 경우 패널을 숨기지 않도록 선택한 경우).", MMInformationAttribute.InformationType.Info, false)]
        /// 아무것도 제공되지 않을 때 표시할 제목
        public string DefaultTitle;
        /// 아무것도 제공되지 않을 때 표시할 간단한 설명
        public string DefaultShortDescription;
        /// 아무것도 제공되지 않을 때 표시할 설명
        public string DefaultDescription;
        /// 아무것도 제공되지 않을 때 표시할 수량
        public string DefaultQuantity;
        /// 아무것도 제공되지 않을 때 표시할 아이콘
        public Sprite DefaultIcon;

        [Header("Behaviour")]
        [MMInformation("여기에서 시작 시 세부정보 패널을 숨길지 여부를 결정할 수 있습니다.", MMInformationAttribute.InformationType.Info, false)]
        /// 시작 시 세부정보 패널을 숨길지 여부
        public bool HideOnStart = true;

        [Header("Components")]
        [MMInformation("여기에서 패널 구성 요소를 바인딩해야 합니다.", MMInformationAttribute.InformationType.Info, false)]
        /// 아이콘 컨테이너 객체
        public Image Icon;
        /// 제목 컨테이너 객체
        public TextMeshProUGUI Title;
        /// 간단한 설명 컨테이너 객체
        public TextMeshProUGUI ShortDescription;
        /// 설명 컨테이너 객체
        public TextMeshProUGUI Description;
        /// 수량 컨테이너 객체
        public TextMeshProUGUI Quantity;

        protected float _fadeDelay = 0.2f;
        public CanvasGroup _canvasGroup;

        //내가만든 변수
        public GameObject marker;
        InventoryItem curItem;

        /// <summary>
        /// 시작 시 캔버스 그룹을 가져와 저장하고 현재 숨김 상태를 확인합니다.
        /// </summary>
        protected virtual void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        protected virtual void Start()
        {
            if (HideOnStart)
            {
                _canvasGroup.alpha = 0;
            }

            if (_canvasGroup.alpha == 0)
            {
                Hidden = true;
            }
            else
            {
                Hidden = false;
            }
        }

        //아이템정보창 숨기기
        public virtual void DisplayDetailsHidden()
        {
            StartCoroutine(MMFade.FadeCanvasGroup(_canvasGroup, _fadeDelay, 0f));
            Hidden = true;
        }

        //현재 선택중인 아이템 정보 반환
        public InventoryItem SetCurrentItemInformation()
        {
            return curItem;
        }

        /// <summary>
        /// 현재 슬롯이 비어 있는지 여부에 따라 디스플레이 코루틴 또는 패널 페이드를 시작합니다.
        /// </summary>
        /// <param name="item">Item.</param>
        public virtual void DisplayDetails(InventoryItem item)
        {
            if (InventoryItem.IsNull(item))
            {
                if (HideOnEmptySlot && !Hidden)
                {
                    StartCoroutine(MMFade.FadeCanvasGroup(_canvasGroup, _fadeDelay, 0f));
                    Hidden = true;
                }
                if (!HideOnEmptySlot)
                {
                    StartCoroutine(FillDetailFieldsWithDefaults(0));
                }
                //빈공간 누르면 '바이','셀'버튼 비활성화
                _canvasGroup.blocksRaycasts = false;
            }
            else
            {
                StartCoroutine(FillDetailFields(item, 0f));

                if (HideOnEmptySlot && Hidden)
                {
                    StartCoroutine(MMFade.FadeCanvasGroup(_canvasGroup, _fadeDelay, 1f));
                    Hidden = false;

                    //여기 까지 왔다면 선택한 슬롯이 해당 인벤토리이기 떄문에 활성화 시켜줍니다.
                    marker.gameObject.SetActive(true);
                    _canvasGroup.alpha = 1;
                    _canvasGroup.blocksRaycasts = true;
                }
            }
        }

        /// <summary>
        /// 항목의 메타데이터로 다양한 세부 정보 필드를 채웁니다.
        /// </summary>
        /// <returns>세부사항 필드.</returns>
        /// <param name="item">Item.</param>
        /// <param name="initialDelay">초기 지연.</param>
        protected virtual IEnumerator FillDetailFields(InventoryItem item, float initialDelay)
        {
            yield return new WaitForSeconds(initialDelay);
            if (Title != null) { Title.text = item.ItemName; }
            switch (item.grade)
            {
                case Define.Grade.Normal: Title.color = Color.white; break;
                case Define.Grade.Rare: Title.color = new Color(0.83f, 0.83f, 0f); break;
                case Define.Grade.Unique: Title.color = new Color(0.83f, 0f, 0f); break;
            }
            if (ShortDescription != null) { ShortDescription.text = item.ShortDescription; }
            if (Description != null) { Description.text = item.Description; }
            if (Quantity != null) { Quantity.text = item.Quantity.ToString(); }
            if (Icon != null) { Icon.sprite = item.Icon; }

            if (HideOnEmptySlot && !Hidden && (item.Quantity == 0))
            {
                StartCoroutine(MMFade.FadeCanvasGroup(_canvasGroup, _fadeDelay, 0f));
                Hidden = true;
            }
        }

        /// <summary>
        /// 세부사항 필드를 기본값으로 채웁니다.
        /// </summary>
        /// <returns>기본값이 포함된 세부정보 필드입니다.</returns>
        /// <param name="initialDelay">초기 지연.</param>
        protected virtual IEnumerator FillDetailFieldsWithDefaults(float initialDelay)
        {
            yield return new WaitForSeconds(initialDelay);
            if (Title != null) { Title.text = DefaultTitle; }
            if (ShortDescription != null) { ShortDescription.text = DefaultShortDescription; }
            if (Description != null) { Description.text = DefaultDescription; }
            if (Quantity != null) { Quantity.text = DefaultQuantity; }
            if (Icon != null) { Icon.sprite = DefaultIcon; }
        }

        /// <summary>
        /// MMInventoryEvents를 포착하고 필요한 경우 세부 정보를 표시합니다.
        /// </summary>
        /// <param name="inventoryEvent">인벤토리 이벤트.</param>
        public virtual void OnMMEvent(MMInventoryEvent inventoryEvent)
        {
            // 이 이벤트가 재고 표시와 관련이 없으면 마커를 비활성화 시키고, 자기자신의 알파값을 0으로 설정합니다.
            if (!Global && (inventoryEvent.TargetInventoryName != this.TargetInventoryName))
            {
                marker.gameObject.SetActive(false);
                _canvasGroup.alpha = 0;
                _canvasGroup.blocksRaycasts = false;
                Hidden = true;
                return;
            }

            if (inventoryEvent.PlayerID != PlayerID)
            {
                return;
            }

            switch (inventoryEvent.InventoryEventType)
            {
                case MMInventoryEventType.Click:
                case MMInventoryEventType.Select:
                    curItem = inventoryEvent.EventItem;
                    DisplayDetails(inventoryEvent.EventItem);
                    break;
                case MMInventoryEventType.UseRequest:
                    DisplayDetails(inventoryEvent.EventItem);
                    break;
                case MMInventoryEventType.InventoryOpens:
                    DisplayDetails(inventoryEvent.EventItem);
                    break;
                case MMInventoryEventType.Drop:
                    DisplayDetails(null);
                    break;
                case MMInventoryEventType.EquipRequest:
                    DisplayDetails(null);
                    break;
            }
        }

        /// <summary>
        /// 활성화하면 MMInventoryEvents 수신을 시작합니다.
        /// </summary>
        protected virtual void OnEnable()
        {
            this.MMEventStartListening<MMInventoryEvent>();
        }

        /// <summary>
        /// 비활성화되면 MMInventoryEvents 수신이 중지됩니다.
        /// </summary>
        protected virtual void OnDisable()
        {
            this.MMEventStopListening<MMInventoryEvent>();
        }
    }
}