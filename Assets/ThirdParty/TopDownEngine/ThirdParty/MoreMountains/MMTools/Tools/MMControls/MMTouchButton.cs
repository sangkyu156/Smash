using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 이 구성 요소를 GUI 이미지에 추가하여 버튼 역할을 하도록 하세요.
    /// 눌려진 상태, 계속 눌려진 상태 및 인스펙터에서 해당 작업에 대한 해제된 작업을 바인딩합니다.
    /// 마우스 및 멀티 터치 처리
    /// </summary>
    [RequireComponent(typeof(Rect))]
	[RequireComponent(typeof(CanvasGroup))]
	[AddComponentMenu("More Mountains/Tools/Controls/MMTouchButton")]
	public class MMTouchButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IPointerEnterHandler, ISubmitHandler
	{
		[Header("Interaction")] 
		/// whether or not this button can be interacted with
		public bool Interactable = true;

        /// 버튼에 가능한 다양한 상태는 다음과 같습니다.
        /// Off(기본 유휴 상태), ButtonDown(처음으로 버튼을 눌렀음), ButtonPressed(버튼을 눌렀음), ButtonUp(버튼을 놓는 중), 비활성화(클릭할 수 없지만 화면에는 계속 표시됨)
        /// ButtonDown 및 ButtonUp은 한 프레임만 지속되며, 다른 프레임은 누르는 동안 지속됩니다/비활성화/아무 작업도 수행하지 않습니다.
        public enum ButtonStates { Off, ButtonDown, ButtonPressed, ButtonUp, Disabled }
		[Header("Binding")]
		/// The method(s) to call when the button gets pressed down
		[Tooltip("버튼을 눌렀을 때 호출할 메서드")]
		public UnityEvent ButtonPressedFirstTime;
		/// The method(s) to call when the button gets released
		[Tooltip("버튼을 놓을 때 호출할 메서드")]
		public UnityEvent ButtonReleased;
		/// The method(s) to call while the button is being pressed
		[Tooltip("버튼을 누르고 있는 동안 호출할 메서드")]
		public UnityEvent ButtonPressed;

		[Header("Sprite Swap")]
		[MMInformation("여기에서 비활성화된 상태와 누른 상태에 대해 다른 스프라이트와 다른 색상을 원하는지 정의할 수 있습니다.", MMInformationAttribute.InformationType.Info,false)]
		/// the sprite to use on the button when it's in the disabled state
		[Tooltip("비활성화된 상태에 있을 때 버튼에 사용할 스프라이트")]
		public Sprite DisabledSprite;
		/// whether or not to change color when the button is disabled
		[Tooltip("버튼이 비활성화되었을 때 색상을 변경할지 여부")]
		public bool DisabledChangeColor = false;
		/// the color to use when the button is disabled
		[Tooltip("버튼이 비활성화되었을 때 사용할 색상")]
		[MMCondition("DisabledChangeColor", true)]
		public Color DisabledColor = Color.white;
		/// the sprite to use on the button when it's in the pressed state
		[Tooltip("버튼이 눌려진 상태일 때 버튼에 사용할 스프라이트")]
		public Sprite PressedSprite;
		/// whether or not to change the button color on press
		[Tooltip("누를 때 버튼 색상을 변경할지 여부")]
		public bool PressedChangeColor = false;
		/// the color to use when the button is pressed
		[Tooltip("버튼을 눌렀을 때 사용할 색상")]
		[MMCondition("PressedChangeColor", true)]
		public Color PressedColor= Color.white;
		/// the sprite to use on the button when it's in the highlighted state
		[Tooltip("강조 표시된 상태에 있을 때 버튼에 사용할 스프라이트")]
		public Sprite HighlightedSprite;
		/// whether or not to change color when highlighting the button
		[Tooltip("버튼을 강조 표시할 때 색상을 변경할지 여부")]
		public bool HighlightedChangeColor = false;
		/// the color to use when the button is highlighted 
		[Tooltip("버튼이 강조 표시될 때 사용할 색상")]
		[MMCondition("HighlightedChangeColor", true)]
		public Color HighlightedColor = Color.white;

		[Header("Opacity")]
		[MMInformation("여기에서는 버튼을 눌렀을 때, 유휴 상태일 때, 비활성화되었을 때 버튼의 불투명도를 다르게 설정할 수 있습니다. 시각적 피드백에 유용합니다.", MMInformationAttribute.InformationType.Info,false)]
		/// the new opacity to apply to the canvas group when the button is pressed
		[Tooltip("버튼을 눌렀을 때 캔버스 그룹에 적용할 불투명도")]
		public float PressedOpacity = 1f;
		/// the new opacity to apply to the canvas group when the button is idle
		[Tooltip("버튼이 유휴 상태일 때 캔버스 그룹에 적용할 새로운 불투명도")]
		public float IdleOpacity = 1f;
		/// the new opacity to apply to the canvas group when the button is disabled
		[Tooltip("버튼이 비활성화되었을 때 캔버스 그룹에 적용할 새로운 불투명도")]
		public float DisabledOpacity = 1f;

		[Header("Delays")]
		[MMInformation("버튼을 처음 눌렀을 때와 버튼을 놓을 때 적용할 지연을 여기에서 지정합니다. 일반적으로 0으로 유지합니다.", MMInformationAttribute.InformationType.Info,false)]
		/// the delay to apply to events when the button gets pressed for the first time
		[Tooltip("버튼을 처음 눌렀을 때 이벤트에 적용되는 지연")]
		public float PressedFirstTimeDelay = 0f;
		/// the delay to apply to events when the button gets released
		[Tooltip("버튼을 놓을 때 이벤트에 적용할 지연")]
		public float ReleasedDelay = 0f;

		[Header("Buffer")]
		/// the duration (in seconds) after a press during which the button can't be pressed again
		[Tooltip("버튼을 누른 후 다시 누를 수 없는 기간(초)")]
		public float BufferDuration = 0f;

		[Header("Animation")]
		[MMInformation("여기서 애니메이터를 바인딩하고 다양한 상태에 대한 애니메이션 매개변수 이름을 지정할 수 있습니다.", MMInformationAttribute.InformationType.Info,false)]
		/// an animator you can bind to this button to have its states updated to reflect the button's states
		[Tooltip("이 버튼에 바인딩하여 버튼의 상태를 반영하도록 상태를 업데이트할 수 있는 애니메이터")]
		public Animator Animator;
		/// the name of the animation parameter to turn true when the button is idle
		[Tooltip("버튼이 유휴 상태일 때 true로 전환되는 애니메이션 매개변수의 이름")]
		public string IdleAnimationParameterName = "Idle";
		/// the name of the animation parameter to turn true when the button is disabled
		[Tooltip("버튼이 비활성화되었을 때 true로 바뀔 애니메이션 매개변수의 이름")]
		public string DisabledAnimationParameterName = "Disabled";
		/// the name of the animation parameter to turn true when the button is pressed
		[Tooltip("버튼을 눌렀을 때 true가 되는 애니메이션 매개변수의 이름")]
		public string PressedAnimationParameterName = "Pressed";

		[Header("Mouse Mode")]
		[MMInformation("이것을 true로 설정하면 실제로 버튼을 눌러야 트리거됩니다. 그렇지 않으면 간단한 마우스 오버만으로 트리거됩니다(터치 입력을 하려는 경우 선택하지 않은 상태로 두는 것이 좋습니다).", MMInformationAttribute.InformationType.Info,false)]
		/// If you set this to true, you'll need to actually press the button for it to be triggered, otherwise a simple hover will trigger it (better for touch input).
		[Tooltip("이를 true로 설정하면 실제로 버튼을 눌러야 트리거됩니다. 그렇지 않으면 간단한 호버만으로 트리거됩니다(터치 입력에 더 좋음).")]
		public bool MouseMode = false;

		public bool ReturnToInitialSpriteAutomatically { get; set; }

		/// the current state of the button (off, down, pressed or up)
		public ButtonStates CurrentState { get; protected set; }

		public event System.Action<PointerEventData.FramePressState, PointerEventData> ButtonStateChange;

		protected bool _zonePressed = false;
		protected CanvasGroup _canvasGroup;
		protected float _initialOpacity;
		protected Animator _animator;
		protected Image _image;
		protected Sprite _initialSprite;
		protected Color _initialColor;
		protected float _lastClickTimestamp = 0f;
		protected Selectable _selectable;

		/// <summary>
		/// On Start, we get our canvasgroup and set our initial alpha
		/// </summary>
		protected virtual void Awake()
		{
			Initialization ();
		}

		/// <summary>
		/// On init we grab our Image, Animator and CanvasGroup and set them up
		/// </summary>
		protected virtual void Initialization()
		{
			ReturnToInitialSpriteAutomatically = true;

			_selectable = GetComponent<Selectable> ();

			_image = GetComponent<Image> ();
			if (_image != null)
			{
				_initialColor = _image.color;
				_initialSprite = _image.sprite;
			}

			_animator = GetComponent<Animator> ();
			if (Animator != null)
			{
				_animator = Animator;
			}

			_canvasGroup = GetComponent<CanvasGroup>();
			if (_canvasGroup!=null)
			{
				_initialOpacity = IdleOpacity;
				_canvasGroup.alpha = _initialOpacity;
				_initialOpacity = _canvasGroup.alpha;
			}
			ResetButton();
		}

		/// <summary>
		/// Every frame, if the touch zone is pressed, we trigger the OnPointerPressed method, to detect continuous press
		/// </summary>
		protected virtual void Update()
		{
			switch (CurrentState)
			{
				case ButtonStates.Off:
					SetOpacity (IdleOpacity);
					if ((_image != null) && (ReturnToInitialSpriteAutomatically))
					{
						_image.color = _initialColor;
						_image.sprite = _initialSprite;
					}
					if (_selectable != null)
					{
						_selectable.interactable = true;
						if (EventSystem.current.currentSelectedGameObject == this.gameObject)
						{
							if ((_image != null) && HighlightedChangeColor)
							{
								_image.color = HighlightedColor;
							}
							if (HighlightedSprite != null)
							{
								_image.sprite = HighlightedSprite;
							}
						}
					}
					break;

				case ButtonStates.Disabled:
					SetOpacity (DisabledOpacity);
					if (_image != null)
					{
						if (DisabledSprite != null)
						{
							_image.sprite = DisabledSprite;	
						}
						if (DisabledChangeColor)
						{
							_image.color = DisabledColor;	
						}
					}
					if (_selectable != null)
					{
						_selectable.interactable = false;
					}
					break;

				case ButtonStates.ButtonDown:

					break;

				case ButtonStates.ButtonPressed:
					SetOpacity (PressedOpacity);
					OnPointerPressed();
					if (_image != null)
					{
						if (PressedSprite != null)
						{
							_image.sprite = PressedSprite;
						}
						if (PressedChangeColor)
						{
							_image.color = PressedColor;	
						}
					}
					break;

				case ButtonStates.ButtonUp:

					break;
			}
			UpdateAnimatorStates ();
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
		/// Triggers the ButtonStateChange event for the specified state
		/// </summary>
		/// <param name="newState"></param>
		/// <param name="data"></param>
		public virtual void InvokeButtonStateChange(PointerEventData.FramePressState newState, PointerEventData data)
		{
			ButtonStateChange?.Invoke(newState, data);
		}
			
		/// <summary>
		/// Triggers the bound pointer down action
		/// </summary>
		public virtual void OnPointerDown(PointerEventData data)
		{
			if (!Interactable)
			{
				return;
			}
			
			if (Time.time - _lastClickTimestamp < BufferDuration)
			{
				return;
			}

			if (CurrentState != ButtonStates.Off)
			{
				return;
			}
			CurrentState = ButtonStates.ButtonDown;
			_lastClickTimestamp = Time.time;
			InvokeButtonStateChange(PointerEventData.FramePressState.Pressed, data);
			if ((Time.timeScale != 0) && (PressedFirstTimeDelay > 0))
			{
				Invoke ("InvokePressedFirstTime", PressedFirstTimeDelay);	
			}
			else
			{
				ButtonPressedFirstTime.Invoke();
			}
		}
		
		/// <summary>
		/// Raises the ButtonPressedFirstTime event
		/// </summary>
		protected virtual void InvokePressedFirstTime()
		{
			if (ButtonPressedFirstTime!=null)
			{
				ButtonPressedFirstTime.Invoke();
			}
		}

		/// <summary>
		/// Triggers the bound pointer up action
		/// </summary>
		public virtual void OnPointerUp(PointerEventData data)
		{
			if (!Interactable)
			{
				return;
			}
			if (CurrentState != ButtonStates.ButtonPressed && CurrentState != ButtonStates.ButtonDown)
			{
				return;
			}

			CurrentState = ButtonStates.ButtonUp;
			InvokeButtonStateChange(PointerEventData.FramePressState.Released, data);
			if ((Time.timeScale != 0) && (ReleasedDelay > 0))
			{
				Invoke ("InvokeReleased", ReleasedDelay);
			}
			else
			{
				ButtonReleased.Invoke();
			}
		}

		/// <summary>
		/// Invokes the ButtonReleased event
		/// </summary>
		protected virtual void InvokeReleased()
		{
			if (ButtonReleased != null)
			{
				ButtonReleased.Invoke();
			}			
		}

		/// <summary>
		/// Triggers the bound pointer pressed action
		/// </summary>
		public virtual void OnPointerPressed()
		{
			if (!Interactable)
			{
				return;
			}
			CurrentState = ButtonStates.ButtonPressed;
			if (ButtonPressed != null)
			{
				ButtonPressed.Invoke();
			}
		}

		/// <summary>
		/// Resets the button's state and opacity
		/// </summary>
		protected virtual void ResetButton()
		{
			SetOpacity(_initialOpacity);
			CurrentState = ButtonStates.Off;
		}

		/// <summary>
		/// Triggers the bound pointer enter action when touch enters zone
		/// </summary>
		public virtual void OnPointerEnter(PointerEventData data)
		{
			if (!Interactable)
			{
				return;
			}
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
			if (!Interactable)
			{
				return;
			}
			if (!MouseMode)
			{
				OnPointerUp(data);	
			}
		}
		/// <summary>
		/// OnEnable, we reset our button state
		/// </summary>
		protected virtual void OnEnable()
		{
			ResetButton();
		}

		/// <summary>
		/// On disable we reset our flags and disable the button
		/// </summary>
		private void OnDisable()
		{
			bool wasActive = CurrentState != ButtonStates.Off && CurrentState != ButtonStates.Disabled && CurrentState != ButtonStates.ButtonUp;
			DisableButton();
			CurrentState = ButtonStates.Off; 
			if (wasActive)
			{
				InvokeButtonStateChange(PointerEventData.FramePressState.Released, null);
				ButtonReleased?.Invoke();
			}
		}

		/// <summary>
		/// Prevents the button from receiving touches
		/// </summary>
		public virtual void DisableButton()
		{
			CurrentState = ButtonStates.Disabled;
		}

		/// <summary>
		/// Allows the button to receive touches
		/// </summary>
		public virtual void EnableButton()
		{
			if (CurrentState == ButtonStates.Disabled)
			{
				CurrentState = ButtonStates.Off;	
			}
		}

		/// <summary>
		/// Sets the canvas group's opacity to the requested value
		/// </summary>
		/// <param name="newOpacity"></param>
		protected virtual void SetOpacity(float newOpacity)
		{
			if (_canvasGroup!=null)
			{
				_canvasGroup.alpha = newOpacity;
			}
		}

		/// <summary>
		/// Updates animator states based on the current state of the button
		/// </summary>
		protected virtual void UpdateAnimatorStates ()
		{
			if (_animator == null)
			{
				return;
			}
			if (DisabledAnimationParameterName != null)
			{
				_animator.SetBool (DisabledAnimationParameterName, (CurrentState == ButtonStates.Disabled));
			}
			if (PressedAnimationParameterName != null)
			{
				_animator.SetBool (PressedAnimationParameterName, (CurrentState == ButtonStates.ButtonPressed));
			}
			if (IdleAnimationParameterName != null)
			{
				_animator.SetBool (IdleAnimationParameterName, (CurrentState == ButtonStates.Off));
			}
		}

		/// <summary>
		/// On submit, raises the appropriate events
		/// </summary>
		/// <param name="eventData"></param>
		public virtual void OnSubmit(BaseEventData eventData)
		{
			if (ButtonPressedFirstTime!=null)
			{
				ButtonPressedFirstTime.Invoke();
			}
			if (ButtonReleased!=null)
			{
				ButtonReleased.Invoke ();
			}
		}
	}
}