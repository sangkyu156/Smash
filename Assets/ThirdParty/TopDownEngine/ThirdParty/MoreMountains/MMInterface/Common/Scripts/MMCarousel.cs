using UnityEngine;
using System.Collections;
using System;
using MoreMountains.Tools;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace MoreMountains.MMInterface
{
    /// <summary>
    /// HorizontalLayoutGroup에 배치된 UI 요소의 캐러셀을 처리하는 클래스입니다.
    /// 캐러셀의 모든 요소는 동일한 너비를 가져야 합니다.
    /// </summary>
    public class MMCarousel : MonoBehaviour
	{
		[Header("Binding")]
        /// 캐러셀의 모든 요소를 ​​포함하는 레이아웃 그룹
        public HorizontalLayoutGroup Content;

		public Camera UICamera;

		[Header("Optional Buttons Binding")]
        /// 캐러셀을 왼쪽으로 이동시키는 버튼
        public MMTouchButton LeftButton;
        /// 캐러셀을 오른쪽으로 이동시키는 버튼
        public MMTouchButton RightButton;

		[Header("Carousel Setup")]
        /// 초기 및 현재 인덱스
        public int CurrentIndex = 0;
        /// 매번 이동해야 하는 캐러셀의 항목 수
        public int Pagination = 1;
        /// 도달했을 때 움직임이 멈추는 거리의 비율
        public float ThresholdInPercent = 1f;

		[Header("Speed")]
        /// 캐러셀의 이동 지속 시간(초)
        public float MoveDuration = 0.05f;

		[Header("Focus")]
        /// 처음에 포커스가 있어야 하는 캐러셀 항목을 여기에 바인딩합니다.
        public GameObject InitialFocus;
        /// 이것이 사실이라면 마우스는 시작 시 강제로 다시 돌아옵니다.
        public bool ForceMouseVisible = true;

		[Header("Keyboard/Gamepad")]
		/// the number 
		//public int 


		//protected float ElementWidth { get { return (Content.minWidth - (Content.spacing * (Content.flexibleWidth - 1))) / Content.flexibleWidth; }}
		protected float _elementWidth;
		protected int _contentLength = 0;
		protected float _spacing;
		protected Vector2 _initialPosition;
		protected RectTransform _rectTransform;

		protected bool _lerping = false;
		protected float _lerpStartedTimestamp;
		protected Vector2 _startPosition;
		protected Vector2 _targetPosition;

        private float currentTime = 0.0f;

        /// <summary>
        /// On Start we initialize our carousel
        /// </summary>
        protected virtual void Start()
		{
			Initialization ();
		}

        /// <summary>
        /// 캐러셀을 초기화하고, 직사각형 변환을 가져오고, 요소의 크기를 계산하고, 위치를 초기화합니다.
        /// </summary>
        protected virtual void Initialization()
		{
			_rectTransform = Content.gameObject.GetComponent<RectTransform> ();
			_initialPosition = _rectTransform.anchoredPosition;

			// we compute the Content's element width
			_contentLength = 0;
			foreach (Transform tr in Content.transform) 
			{ 
				_elementWidth = tr.gameObject.MMGetComponentNoAlloc<RectTransform>().sizeDelta.x;
				_contentLength++;
			}
			_spacing = Content.spacing;

			// we position our carousel at the desired initial index
			_rectTransform.anchoredPosition = DeterminePosition ();

			if (InitialFocus != null)
			{
				EventSystem.current.SetSelectedGameObject(InitialFocus, null);
			}

			if (ForceMouseVisible)
			{
				Cursor.visible = true;
			}
		}

        /// <summary>
        /// 캐러셀을 왼쪽으로 이동합니다.
        /// </summary>
        public virtual void MoveLeft()
		{
			if (!CanMoveLeft())
			{
				return;
			}
			else
			{				
				CurrentIndex -= Pagination;
				MoveToCurrentIndex ();	
			}
		}

        /// <summary>
        /// 캐러셀을 오른쪽으로 이동합니다.
        /// </summary>
        public virtual void MoveRight()
		{
			if (!CanMoveRight())
			{
				return;
			}
			else
			{
				CurrentIndex += Pagination;
				MoveToCurrentIndex ();	
			}
		}

        /// <summary>
        /// 현재 인덱스로 이동을 시작합니다.
        /// </summary>
        protected virtual void MoveToCurrentIndex ()
		{
			_startPosition = _rectTransform.anchoredPosition;
			_targetPosition = DeterminePosition ();
			_lerping = true;
			_lerpStartedTimestamp = Time.time;
		}

        /// <summary>
        /// 현재 인덱스 값을 기준으로 목표 위치를 결정합니다.
        /// </summary>
        /// <returns>The position.</returns>
        protected virtual Vector2 DeterminePosition()
		{
			return _initialPosition - (Vector2.right * CurrentIndex * (_elementWidth + _spacing));
		}

		public virtual bool CanMoveLeft()
		{
			return (CurrentIndex - Pagination >= 0);
				
		}

        /// <summary>
        /// 이 캐러셀이 오른쪽으로 이동할 수 있는지 여부를 결정합니다.
        /// </summary>
        /// <returns><c>true</c> if this instance can move right; otherwise, <c>false</c>.</returns>
        public virtual bool CanMoveRight()
		{
			return (CurrentIndex + Pagination < _contentLength);
		}

        /// <summary>
        /// 업데이트 시 필요한 경우 캐러셀을 이동하고 버튼 상태를 처리합니다.
        /// </summary>
        protected virtual void Update()
		{
			if (_lerping)
			{
				LerpPosition ();
			}
			HandleButtons ();
			HandleFocus ();
		}

		protected virtual void HandleFocus()
		{
			if (!_lerping && Time.timeSinceLevelLoad > 0.5f)
			{
				if (EventSystem.current.currentSelectedGameObject != null)
				{
					if (UICamera.WorldToScreenPoint(EventSystem.current.currentSelectedGameObject.transform.position).x < 0)
					{
						MoveLeft ();
					}
					if (UICamera.WorldToScreenPoint(EventSystem.current.currentSelectedGameObject.transform.position).x > Screen.width)
					{
						MoveRight ();
					}	
				}
			}
		}

        /// <summary>
        /// 버튼을 처리하고 필요한 경우 버튼을 활성화 및 비활성화합니다.
        /// </summary>
        protected virtual void HandleButtons()
		{
			if (LeftButton != null) 
			{ 
				if (CanMoveLeft())
				{
					LeftButton.EnableButton (); 
				}
				else
				{
					LeftButton.DisableButton (); 
				}	
			}
			if (RightButton != null) 
			{ 
				if (CanMoveRight())
				{
					RightButton.EnableButton (); 
				}
				else
				{
					RightButton.DisableButton (); 
				}	
			}
		}

        /// <summary>
        /// 캐러셀의 위치를 ​​Lerps합니다.
        /// </summary>
        protected virtual void LerpPosition()
		{
            // 현재 시간을 업데이트합니다.
            currentTime += Time.unscaledDeltaTime;

			float percentageComplete = currentTime / MoveDuration;

            // 시작 위치에서 끝 위치로 선형 보간합니다.
            _rectTransform.anchoredPosition = Vector2.Lerp(_startPosition, _targetPosition, currentTime / MoveDuration);

            //Lerp를 완료하면 _isLerping을 false로 설정합니다.
            if (percentageComplete >= ThresholdInPercent)
            {
				currentTime = 0;
                _lerping = false;
            }

   //         float timeSinceStarted = Time.time - _lerpStartedTimestamp;
			//float percentageComplete = timeSinceStarted / MoveDuration;

			//_rectTransform.anchoredPosition = Vector2.Lerp (_startPosition, _targetPosition, percentageComplete);

            //Lerp를 완료하면 _isLerping을 false로 설정합니다.
   //         if (percentageComplete >= ThresholdInPercent)
			//{
			//	_lerping = false;
			//}
		}
	}
}