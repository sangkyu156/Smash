using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace MoreMountains.Tools
{	
	[System.Serializable]
	public class AxisEvent : UnityEvent<float> {}

    /// <summary>
    /// 이 구성요소를 GUI 이미지에 추가하여 축 역할을 하도록 하세요.
    /// 눌려진 상태, 계속 눌려진 상태 및 인스펙터에서 해당 작업에 대한 해제된 작업을 바인딩합니다.
    /// 마우스 및 멀티 터치 처리
    /// </summary>
    [RequireComponent(typeof(Rect))]
	[RequireComponent(typeof(CanvasGroup))]
	[AddComponentMenu("More Mountains/Tools/Controls/MMTouchAxis")]
	public class MMTouchAxis : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IPointerEnterHandler
	{
		public enum ButtonStates { Off, ButtonDown, ButtonPressed, ButtonUp }
		[Header("Binding")]
		/// The method(s) to call when the axis gets pressed down
		[Tooltip("축을 눌렀을 때 호출할 메서드")]
		public UnityEvent AxisPressedFirstTime;
		/// The method(s) to call when the axis gets released
		[Tooltip("축이 해제될 때 호출할 메서드")]
		public UnityEvent AxisReleased;
		/// The method(s) to call while the axis is being pressed
		[Tooltip("축을 누르고 있는 동안 호출할 메서드")]
		public AxisEvent AxisPressed;

		[Header("Pressed Behaviour")]
		[MMInformation("여기에서는 버튼을 눌렀을 때 버튼의 불투명도를 설정할 수 있습니다. 시각적 피드백에 유용합니다.", MMInformationAttribute.InformationType.Info,false)]
		/// the new opacity to apply to the canvas group when the axis is pressed
		[Tooltip("축을 누를 때 캔버스 그룹에 적용할 새로운 불투명도")]
		public float PressedOpacity = 0.5f;
		/// the value to send the bound method when the axis is pressed
		[Tooltip("축을 눌렀을 때 바인딩된 메서드를 보낼 값")]
		public float AxisValue;

		[Header("Mouse Mode")]
		[MMInformation("이를 true로 설정하면 실제로 축을 눌러야 트리거됩니다. 그렇지 않으면 간단한 호버만으로 트리거됩니다(터치 입력에 더 좋음).", MMInformationAttribute.InformationType.Info,false)]
		/// If you set this to true, you'll need to actually press the axis for it to be triggered, otherwise a simple hover will trigger it (better for touch input).
		[Tooltip("이를 true로 설정하면 실제로 축을 눌러야 트리거됩니다. 그렇지 않으면 간단한 호버만으로 트리거됩니다(터치 입력에 더 좋음).")]
		public bool MouseMode = false;

		public ButtonStates CurrentState { get; protected set; }

		protected CanvasGroup _canvasGroup;
		protected float _initialOpacity;

		/// <summary>
		/// On Start, we get our canvasgroup and set our initial alpha
		/// </summary>
		protected virtual void Awake()
		{
			_canvasGroup = GetComponent<CanvasGroup>();
			if (_canvasGroup!=null)
			{
				_initialOpacity = _canvasGroup.alpha;
			}
			ResetButton();
		}

		/// <summary>
		/// Every frame, if the touch zone is pressed, we trigger the bound method if it exists
		/// </summary>
		protected virtual void Update()
		{
			if (AxisPressed != null)
			{
				if (CurrentState == ButtonStates.ButtonPressed)
				{
					AxisPressed.Invoke(AxisValue);
				}
			}
		}

		/// <summary>
		/// At the end of every frame, we change our button's state if needed
		/// </summary>
		protected virtual void LateUpdate()
		{
			if (CurrentState == ButtonStates.ButtonUp)
			{
				CurrentState = ButtonStates.Off;
			}
			if (CurrentState == ButtonStates.ButtonDown)
			{
				CurrentState = ButtonStates.ButtonPressed;
			}
		}

		/// <summary>
		/// Triggers the bound pointer down action
		/// </summary>
		public virtual void OnPointerDown(PointerEventData data)
		{
			if (CurrentState != ButtonStates.Off)
			{
				return;
			}

			CurrentState = ButtonStates.ButtonDown;
			if (_canvasGroup!=null)
			{
				_canvasGroup.alpha=PressedOpacity;
			}
			if (AxisPressedFirstTime!=null)
			{
				AxisPressedFirstTime.Invoke();
			}
		}

		/// <summary>
		/// Triggers the bound pointer up action
		/// </summary>
		public virtual void OnPointerUp(PointerEventData data)
		{
			if (CurrentState != ButtonStates.ButtonPressed && CurrentState != ButtonStates.ButtonDown)
			{
				return;
			}

			CurrentState = ButtonStates.ButtonUp;
			if (_canvasGroup!=null)
			{
				_canvasGroup.alpha=_initialOpacity;
			}
			if (AxisReleased != null)
			{
				AxisReleased.Invoke();
			}
			AxisPressed.Invoke(0);
		}

		/// <summary>
		/// OnEnable, we reset our button state
		/// </summary>
		protected virtual void OnEnable()
		{
			ResetButton();
		}

		/// <summary>
		/// Resets the button's state and opacity
		/// </summary>
		protected virtual void ResetButton()
		{
			CurrentState = ButtonStates.Off;
			_canvasGroup.alpha = _initialOpacity;
			CurrentState = ButtonStates.Off;
		}

		/// <summary>
		/// Triggers the bound pointer enter action when touch enters zone
		/// </summary>
		public virtual void OnPointerEnter(PointerEventData data)
		{
			if (!MouseMode)
			{
				OnPointerDown (data);
			}
		}

		/// <summary>
		/// Triggers the bound pointer exit action when touch is out of zone
		/// </summary>
		public virtual void OnPointerExit(PointerEventData data)
		{
			if (!MouseMode)
			{
				OnPointerUp(data);	
			}
		}
	}
}