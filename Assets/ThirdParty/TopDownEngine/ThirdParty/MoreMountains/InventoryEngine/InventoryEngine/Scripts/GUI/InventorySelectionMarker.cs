using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using UnityEngine.EventSystems;

namespace MoreMountains.InventoryEngine
{	
	[RequireComponent(typeof(RectTransform))]
    /// <summary>
    /// 이 클래스는 현재 선택된 슬롯을 표시하는 선택 마커를 처리합니다.
    /// </summary>
    public class InventorySelectionMarker : MonoBehaviour 
	{
		[MMInformation("선택 마커는 현재 선택을 강조 표시합니다. 여기에서 전환 속도와 최소 거리 임계값을 정의할 수 있습니다(일반적으로 기본값으로 두어도 괜찮습니다).", MMInformationAttribute.InformationType.Info,false)]
        /// 선택 마커가 한 슬롯에서 다른 슬롯으로 이동하는 속도
        public float TransitionSpeed=5f;
        /// 마커가 이동을 멈추는 임계 거리
        public float MinimalTransitionDistance=0.01f;

		protected RectTransform _rectTransform;
		protected GameObject _currentSelection;
		protected Vector3 _originPosition;
		protected Vector3 _originLocalScale;
		protected Vector3 _originSizeDelta;
		protected float _originTime;
		protected bool _originIsNull=true;
		protected float _deltaTime;

        /// <summary>
        /// 시작 시 관련 각형 변환을 얻습니다.
        /// </summary>
        void Start () 
		{
			_rectTransform = GetComponent<RectTransform>();
		}

        /// <summary>
        /// 업데이트 시 현재 선택된 개체를 가져오고 필요한 경우 마커를 해당 개체로 이동합니다.
        /// </summary>
        void Update () 
		{			
			_currentSelection = EventSystem.current.currentSelectedGameObject;
			if (_currentSelection == null)
			{
				return;
			}

			if (_currentSelection.gameObject.MMGetComponentNoAlloc<InventorySlot>() == null)
			{
				return;
			}

			if (Vector3.Distance(transform.position,_currentSelection.transform.position) > MinimalTransitionDistance)
			{
				if (_originIsNull)
				{
					_originIsNull=false;
					_originPosition = transform.position;
					_originLocalScale = _rectTransform.localScale;
					_originSizeDelta = _rectTransform.sizeDelta;
					_originTime = Time.unscaledTime;
				} 
				_deltaTime =  (Time.unscaledTime - _originTime)*TransitionSpeed;
				transform.position= Vector3.Lerp(_originPosition,_currentSelection.transform.position,_deltaTime);
				_rectTransform.localScale = Vector3.Lerp(_originLocalScale, _currentSelection.GetComponent<RectTransform>().localScale,_deltaTime);
				_rectTransform.sizeDelta = Vector3.Lerp(_originSizeDelta, _currentSelection.GetComponent<RectTransform>().sizeDelta, _deltaTime);
			}
			else
			{
				_originIsNull=true;
			}
		}
	}
}