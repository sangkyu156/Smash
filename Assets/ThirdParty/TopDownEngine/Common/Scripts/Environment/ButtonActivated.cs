using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;
using System.Collections.Generic;
using UnityEngine.Events;
using DG.Tweening;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 특정 영역에서 버튼을 눌렀을 때 무언가를 활성화하려면 이 클래스를 확장하세요.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Environment/Button Activated")]
	public class ButtonActivated : TopDownMonoBehaviour 
	{
		public enum ButtonActivatedRequirements { Character, ButtonActivator, Either, None }
		public enum InputTypes { Default, Button, Key }

		[Header("Requirements")]
		[MMInformation("여기에서 이 영역과 상호 작용하는 데 필요한 사항을 지정할 수 있습니다. ButtonActivation 캐릭터 능력이 필요합니까? 플레이어만 상호작용할 수 있나요?", MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]
        /// 이것이 사실이라면 ButtonActivator 클래스가 있는 객체는 이 영역과 상호 작용할 수 있습니다.
        [Tooltip("이것이 사실이라면 ButtonActivator 클래스가 있는 객체는 이 영역과 상호 작용할 수 있습니다.")]
		public ButtonActivatedRequirements ButtonActivatedRequirement = ButtonActivatedRequirements.Either;
        /// 이것이 사실이라면 플레이어 캐릭터만 활성화할 수 있습니다.
        [Tooltip("이것이 사실이라면 플레이어 캐릭터만 활성화할 수 있습니다.")]
		public bool RequiresPlayerType = true;
        /// 이것이 사실이라면 이 영역은 캐릭터가 필요한 능력을 가지고 있는 경우에만 활성화될 수 있습니다.
        [Tooltip("이것이 사실이라면 이 영역은 캐릭터가 필요한 능력을 가지고 있는 경우에만 활성화될 수 있습니다.")]
		public bool RequiresButtonActivationAbility = true;
        
		[Header("Activation Conditions")]

		[MMInformation("여기에서 해당 영역이 상호 작용하는 방식을 구체적으로 지정할 수 있습니다. 자동으로 활성화하거나 접지된 경우에만 활성화하거나 활성화를 완전히 방지할 수 있습니다.", MoreMountains.Tools.MMInformationAttribute.InformationType.Info,false)]
        /// 이것이 거짓이면 영역을 활성화할 수 없습니다.
        [Tooltip("이것이 거짓이면 영역을 활성화할 수 없습니다.")]
		public bool Activable = true;
        /// true인 경우 버튼을 누르든 안 누르든 영역이 활성화됩니다.
        [Tooltip("true인 경우 버튼을 누르든 안 누르든 영역이 활성화됩니다.")]
		public bool AutoActivation = false;
        /// 캐릭터를 활성화하기 위해 캐릭터가 영역 내에 있어야 하는 지연 시간(초)
        [MMCondition("AutoActivation", true)]
		[Tooltip("캐릭터를 활성화하기 위해 캐릭터가 영역 내에 있어야 하는 지연 시간(초)")]
		public float AutoActivationDelay = 0f;
        /// 이것이 사실이라면 구역을 나가면 자동 활성화 지연이 재설정됩니다.
        [MMCondition("AutoActivation", true)]
		[Tooltip("이것이 사실이라면 구역을 나가면 자동 활성화 지연이 재설정됩니다.")]
		public bool AutoActivationDelayResetsOnExit = true;
        /// false로 설정하면 접지되지 않은 동안에는 구역이 활성화되지 않습니다.
        [Tooltip("false로 설정하면 접지되지 않은 동안에는 구역이 활성화되지 않습니다.")]
		public bool CanOnlyActivateIfGrounded = false;
        /// CharacterBehaviorState가 플레이어의 영역 진입에 대한 알림을 받도록 하려면 이를 true로 설정합니다.
        [Tooltip("캐릭터 행동 상태가 플레이어의 영역 진입에 대한 알림을 받도록 하려면 이를 true로 설정합니다.")]
		public bool ShouldUpdateState = true;
        /// 이것이 true인 경우, 다른 개체가 들어오면 Enter가 다시 트리거되지 않으며 마지막 개체가 나갈 때만 Exit가 트리거됩니다.
        [Tooltip("이것이 true인 경우, 다른 개체가 들어오면 Enter가 다시 트리거되지 않으며 마지막 개체가 나갈 때만 Exit가 트리거됩니다.")]
		public bool OnlyOneActivationAtOnce = true;
        /// 이 특정 버튼 활성화 영역과 상호 작용할 수 있는 모든 레이어가 포함된 레이어 마스크
        [Tooltip("이 특정 버튼 활성화 영역과 상호 작용할 수 있는 모든 레이어가 포함된 레이어 마스크")]
		public LayerMask TargetLayerMask = ~0;

		[Header("Number of Activations")]

		[MMInformation("해당 영역을 영원히 또는 제한된 횟수만 상호 작용할 수 있도록 결정할 수 있으며 사용 간 지연 시간(초 단위)을 지정할 수 있습니다.", MoreMountains.Tools.MMInformationAttribute.InformationType.Info,false)]
        /// false로 설정하면 활성화 횟수는 MaxNumberOfActivations가 됩니다.
        [Tooltip("false로 설정하면 활성화 횟수는 MaxNumberOfActivations가 됩니다.")]
		public bool UnlimitedActivations = true;
        /// 해당 영역과 상호작용할 수 있는 횟수
        [Tooltip("해당 영역과 상호작용할 수 있는 횟수")]
		public int MaxNumberOfActivations = 0;
        /// 영역을 활성화할 수 없는 동안 활성화 후 지연(초)
        [Tooltip("영역을 활성화할 수 없는 동안 활성화 후 지연(초)")]
		public float DelayBetweenUses = 0f;
        /// 이것이 사실이라면 영역은 마지막 사용 후 자동으로 비활성화됩니다(영구적으로 또는 수동으로 다시 활성화할 때까지).
        [Tooltip("이것이 사실이라면 영역은 마지막 사용 후 자동으로 비활성화됩니다(영구적으로 또는 수동으로 다시 활성화할 때까지).")]
		public bool DisableAfterUse = false;

		[Header("Input")]

        /// 선택한 입력 유형(기본값, 버튼 또는 키)
        [Tooltip("선택한 입력 유형(기본값, 버튼 또는 키)")]
		public InputTypes InputType = InputTypes.Default;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
			/// the input action to use for this button activated object
			public InputActionProperty InputSystemAction = new InputActionProperty(
				new InputAction(
					name: "ButtonActivatedAction",
					type: InputActionType.Button, 
					binding: "Keyboard/space", 
					interactions: "Press(behavior=2)"));
#else
        /// 이 영역을 활성화하는 데 사용되는 선택된 버튼 문자열
        [MMEnumCondition("InputType", (int)InputTypes.Button)]
			[Tooltip("이 영역을 활성화하는 데 사용되는 선택된 버튼 문자열")]
			public string InputButton = "Interact";
        /// 이 영역을 활성화하는 데 사용되는 키
        [MMEnumCondition("InputType", (int)InputTypes.Key)]
			[Tooltip("이 영역을 활성화하는 데 사용되는 키")]
			public KeyCode InputKey = KeyCode.Space;
		#endif

		[Header("Animation")]

        /// 영역을 활성화할 때 캐릭터에 대해 트리거될 수 있는 (절대 선택 사항) 애니메이션 매개변수
        [Tooltip("영역을 활성화할 때 캐릭터에 대해 트리거될 수 있는 (절대 선택 사항) 애니메이션 매개변수")]
		public string AnimationTriggerParameterName;

		[Header("Visual Prompt")]

		[MMInformation("이 영역에서 플레이어에게 상호작용 가능함을 알려주는 시각적 메시지를 표시하도록 할 수 있습니다.", MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]
        /// 이것이 사실이라면 제대로 설정되었는지 묻는 메시지가 표시됩니다.
        [Tooltip("이것이 사실이라면 제대로 설정되었는지 묻는 메시지가 표시됩니다.")]
		public bool UseVisualPrompt = true;
        /// 프롬프트를 표시하기 위해 인스턴스화할 게임 객체
        [MMCondition("UseVisualPrompt", true)]
		[Tooltip("프롬프트를 표시하기 위해 인스턴스화할 게임 객체")]
		public ButtonPrompt ButtonPromptPrefab;
        /// 버튼 프롬프트에 표시할 텍스트
        [MMCondition("UseVisualPrompt", true)]
		[Tooltip("버튼 프롬프트에 표시할 텍스트")]
		public string ButtonPromptText = "A";
        /// 버튼 프롬프트에 표시할 텍스트
        [MMCondition("UseVisualPrompt", true)]
		[Tooltip("버튼 프롬프트에 표시할 텍스트")]
		public Color ButtonPromptColor = MMColors.LawnGreen;
        /// 프롬프트 텍스트의 색상
        [MMCondition("UseVisualPrompt", true)]
		[Tooltip("프롬프트 텍스트의 색상")]
		public Color ButtonPromptTextColor = MMColors.White;
        /// true인 경우 플레이어가 해당 영역에 있든 없든 "buttonA" 프롬프트가 항상 표시됩니다.
        [MMCondition("UseVisualPrompt", true)]
		[Tooltip("true인 경우 플레이어가 해당 영역에 있든 없든 \"buttonA\" 프롬프트가 항상 표시됩니다.")]
		public bool AlwaysShowPrompt = true;
        /// true인 경우 플레이어가 영역과 충돌할 때 "buttonA" 프롬프트가 표시됩니다.
        [MMCondition("UseVisualPrompt", true)]
		[Tooltip("true인 경우 플레이어가 영역과 충돌할 때 \"buttonA\" 프롬프트가 표시됩니다.")]
		public bool ShowPromptWhenColliding = true;
        /// true인 경우 사용 후 프롬프트가 숨겨집니다.
        [MMCondition("UseVisualPrompt", true)]
		[Tooltip("true인 경우 사용 후 프롬프트가 숨겨집니다.")]
		public bool HidePromptAfterUse = false;
        /// 실제 버튼의 위치객체 중심을 기준으로 한 프롬프트
        [MMCondition("UseVisualPrompt", true)]
		[Tooltip("실제 버튼의 위치객체 중심을 기준으로 한 프롬프트")]
		public Vector3 PromptRelativePosition = Vector3.zero;
        /// 실제 버튼의 회전 프롬프트
        [MMCondition("UseVisualPrompt", true)]
		[Tooltip("실제 버튼의 회전 프롬프트")]
		public Vector3 PromptRotation = Vector3.zero;

		[Header("Feedbacks")]
        /// 영역이 활성화될 때 재생할 피드백
        [Tooltip("영역이 활성화될 때 재생할 피드백")]
		public MMFeedbacks ActivationFeedback;
        /// 영역이 활성화되려고 하지만 활성화되지 않을 때 재생하기 위한 피드백
        [Tooltip("영역이 활성화되려고 하지만 활성화되지 않을 때 재생하기 위한 피드백")]
		public MMFeedbacks DeniedFeedback;
        /// 영역에 들어갈 때 재생할 피드백	
        [Tooltip("영역에 들어갈 때 재생할 피드백")]
		public MMFeedbacks EnterFeedback;
        /// 해당 영역을 벗어날 때 플레이할 피드백
        [Tooltip("해당 영역을 벗어날 때 플레이할 피드백")]
		public MMFeedbacks ExitFeedback;

		[Header("Actions")]
        /// 이 영역이 활성화될 때 트리거할 UnityEvent
        [Tooltip("이 영역이 활성화될 때 트리거할 UnityEvent")]
		public UnityEvent OnActivation;
        /// 이 영역이 종료될 때 트리거되는 UnityEvent
        [Tooltip("이 영역이 종료될 때 트리거되는 UnityEvent")]
		public UnityEvent OnExit;
        /// 캐릭터가 영역 내에 있을 때 트리거되는 UnityEvent
        [Tooltip("캐릭터가 영역 내에 있을 때 트리거되는 UnityEvent")]
		public UnityEvent OnStay;

		protected Animator _buttonPromptAnimator;
		protected ButtonPrompt _buttonPrompt;
		protected Collider _collider;
		protected Collider2D _collider2D;
		protected bool _promptHiddenForever = false;
		protected CharacterButtonActivation _characterButtonActivation;
		protected int _numberOfActivationsLeft;
		protected float _lastActivationTimestamp;
		protected List<GameObject> _collidingObjects;
		protected Character _currentCharacter;
		protected bool _staying = false;
		protected Coroutine _autoActivationCoroutine;

		//내가만든 변수
		protected GameObject _mainCamera;

        public bool AutoActivationInProgress { get; set; }
		public float AutoActivationStartedAt { get; set; }
		public bool InputActionPerformed
		{
			get
			{
				#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
					return InputSystemAction.action.WasPressedThisFrame();
				#else
					return false;
				#endif
			}
		}

		/// <summary>
		/// On Enable, we initialize our ButtonActivated zone
		/// </summary>
		protected virtual void OnEnable()
		{
			Initialization ();
		}

		/// <summary>
		/// Grabs components and shows prompt if needed
		/// </summary>
		public virtual void Initialization()
		{
			_collider = this.gameObject.GetComponent<Collider>();
			_collider2D = this.gameObject.GetComponent<Collider2D>();
			_numberOfActivationsLeft = MaxNumberOfActivations;
			_collidingObjects = new List<GameObject>();

			ActivationFeedback?.Initialization(this.gameObject);
			DeniedFeedback?.Initialization(this.gameObject);
			EnterFeedback?.Initialization(this.gameObject);
			ExitFeedback?.Initialization(this.gameObject);

			if (AlwaysShowPrompt)
			{
				ShowPrompt();
			}
			
			#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
				InputSystemAction.action.Enable();
			#endif
		}
		
		/// <summary>
		/// On disable we disable our input action if needed
		/// </summary>
		protected virtual void OnDisable()
		{
			#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
				InputSystemAction.action.Disable();
			#endif
		}

		protected virtual IEnumerator TriggerButtonActionCo()
		{
			if (AutoActivationDelay <= 0f)
			{
				TriggerButtonAction();
				yield break;
			}
			else
			{
				AutoActivationInProgress = true;
				AutoActivationStartedAt = Time.time;
				yield return MMCoroutine.WaitFor(AutoActivationDelay);
				AutoActivationInProgress = false;
				TriggerButtonAction();
				yield break;
			}
		}

		/// <summary>
		/// When the input button is pressed, we check whether or not the zone can be activated, and if yes, trigger ZoneActivated
		/// </summary>
		public virtual void TriggerButtonAction()
		{
			if (!CheckNumberOfUses())
			{
				PromptError();
				return;
			}

			_staying = true;
			ActivateZone();
		}

		public virtual void TriggerExitAction(GameObject collider)
		{
			_staying = false;
			if (OnExit != null)
			{
				OnExit.Invoke();
			}
		}

		/// <summary>
		/// Makes the zone activable
		/// </summary>
		public virtual void MakeActivable()
		{
			Activable = true;
		}

		/// <summary>
		/// Makes the zone unactivable
		/// </summary>
		public virtual void MakeUnactivable()
		{
			Activable = false;
		}

		/// <summary>
		/// Makes the zone activable if it wasn't, unactivable if it was activable.
		/// </summary>
		public virtual void ToggleActivable()
		{
			Activable = !Activable;
		}

		protected virtual void Update()
		{
			if (_staying && (OnStay != null))
			{
				OnStay.Invoke();
			}

			//버튼이 활성화 됐을때만 화면 따라다니도록
			if(_buttonPrompt != null)
			{
                if (_buttonPrompt.gameObject.activeSelf)
                {
					_buttonPrompt.transform.LookAt(_mainCamera.transform);
                }
            }
        }

		/// <summary>
		/// Activates the zone
		/// </summary>
		protected virtual void ActivateZone()
		{
			if (OnActivation != null)
			{
				OnActivation.Invoke();
			}

			_lastActivationTimestamp = Time.time;

			ActivationFeedback?.PlayFeedbacks(this.transform.position);

			if (HidePromptAfterUse)
			{
				_promptHiddenForever = true;
				HidePrompt();	
			}	
			_numberOfActivationsLeft--;

			if (DisableAfterUse && (_numberOfActivationsLeft <= 0))
			{
				DisableZone();
			}
		}

		/// <summary>
		/// Triggers an error 
		/// </summary>
		public virtual void PromptError()
		{
			if (_buttonPromptAnimator != null)
			{
				_buttonPromptAnimator.SetTrigger("Error");
			}
			DeniedFeedback?.PlayFeedbacks(this.transform.position);
		}

        /// <summary>
        /// 버튼 A 프롬프트를 표시합니다.
        /// </summary>
        public virtual void ShowPrompt()
		{
			if (!UseVisualPrompt || _promptHiddenForever || (ButtonPromptPrefab == null))
			{
				return;
			}

            // 영역 상단에 깜박이는 A 프롬프트를 추가합니다.
            if (_buttonPrompt == null)
			{
				_buttonPrompt = (ButtonPrompt)Instantiate(ButtonPromptPrefab);
				_buttonPrompt.Initialization();
				_buttonPromptAnimator = _buttonPrompt.gameObject.MMGetComponentNoAlloc<Animator>();
				_mainCamera = GameObject.FindWithTag("MainCamera");
            }
			
			if (_collider != null)
			{
				_buttonPrompt.transform.position = _collider.bounds.center + PromptRelativePosition;
			}
			if (_collider2D != null)
			{
				_buttonPrompt.transform.position = _collider2D.bounds.center + PromptRelativePosition;
			}
			_buttonPrompt.transform.parent = transform;
			_buttonPrompt.transform.localEulerAngles = PromptRotation;
			_buttonPrompt.SetText(ButtonPromptText);
			_buttonPrompt.SetBackgroundColor(ButtonPromptColor);
			_buttonPrompt.SetTextColor(ButtonPromptTextColor);
			_buttonPrompt.Show();
		}

		/// <summary>
		/// Hides the button A prompt.
		/// </summary>
		public virtual void HidePrompt()
		{
			if (_buttonPrompt != null)
			{
				_buttonPrompt.Hide();
			}
		}

		/// <summary>
		/// Disables the button activated zone
		/// </summary>
		public virtual void DisableZone()
		{
			Activable = false;
            
			if (_collider != null)
			{
				_collider.enabled = false;
			}

			if (_collider2D != null)
			{
				_collider2D.enabled = false;
			}
	            
			if (ShouldUpdateState && (_characterButtonActivation != null))
			{
				_characterButtonActivation.InButtonActivatedZone = false;
				_characterButtonActivation.ButtonActivatedZone = null;
			}
		}

		/// <summary>
		/// Enables the button activated zone
		/// </summary>
		public virtual void EnableZone()
		{
			Activable = true;
            
			if (_collider != null)
			{
				_collider.enabled = true;
			}

			if (_collider2D != null)
			{
				_collider2D.enabled = true;
			}
		}

		/// <summary>
		/// Handles enter collision with 2D triggers
		/// </summary>
		/// <param name="collidingObject">Colliding object.</param>
		protected virtual void OnTriggerEnter2D (Collider2D collidingObject)
		{
			TriggerEnter (collidingObject.gameObject);
		}
		/// <summary>
		/// Handles enter collision with 2D triggers
		/// </summary>
		/// <param name="collidingObject">Colliding object.</param>
		protected virtual void OnTriggerExit2D (Collider2D collidingObject)
		{
			TriggerExit (collidingObject.gameObject);
		}
		/// <summary>
		/// Handles enter collision with 2D triggers
		/// </summary>
		/// <param name="collidingObject">Colliding object.</param>
		protected virtual void OnTriggerEnter (Collider collidingObject)
		{
			TriggerEnter (collidingObject.gameObject);
		}
		/// <summary>
		/// Handles enter collision with 2D triggers
		/// </summary>
		/// <param name="collidingObject">Colliding object.</param>
		protected virtual void OnTriggerExit (Collider collidingObject)
		{
			TriggerExit (collidingObject.gameObject);
		}
        
		/// <summary>
		/// Triggered when something collides with the button activated zone
		/// </summary>
		/// <param name="collider">Something colliding with the water.</param>
		protected virtual void TriggerEnter(GameObject collider)
		{            
			if (!CheckConditions(collider))
			{
				return;
			}

			// if we can only activate this zone when grounded, we check if we have a controller and if it's not grounded,
			// we do nothing and exit
			if (CanOnlyActivateIfGrounded)
			{
				if (collider != null)
				{
					TopDownController controller = collider.gameObject.MMGetComponentNoAlloc<TopDownController>();
					if (controller != null)
					{
						if (!controller.Grounded)
						{
							return;
						}
					}
				}
			}

			// at this point the object is colliding and authorized, we add it to our list
			_collidingObjects.Add(collider.gameObject);
			if (!TestForLastObject(collider))
			{
				return;
			}
            
			EnterFeedback?.PlayFeedbacks(this.transform.position);

			if (ShouldUpdateState)
			{
				_characterButtonActivation = collider.gameObject.MMGetComponentNoAlloc<Character>()?.FindAbility<CharacterButtonActivation>();
				if (_characterButtonActivation != null)
				{
					_characterButtonActivation.InButtonActivatedZone = true;
					_characterButtonActivation.ButtonActivatedZone = this;
					_characterButtonActivation.InButtonAutoActivatedZone = AutoActivation;
				}
			}

			if (AutoActivation)
			{
				_autoActivationCoroutine = StartCoroutine(TriggerButtonActionCo());
			}	

			// if we're not already showing the prompt and if the zone can be activated, we show it
			if (ShowPromptWhenColliding)
			{
				ShowPrompt();	
			}
		}

		/// <summary>
		/// Triggered when something exits the water
		/// </summary>
		/// <param name="collider">Something colliding with the dialogue zone.</param>
		protected virtual void TriggerExit(GameObject collider)
		{
			if (!CheckConditions(collider))
			{
				return;
			}

			_collidingObjects.Remove(collider.gameObject);
			if (!TestForLastObject(collider))
			{
				return;
			}
            
			AutoActivationInProgress = false;
			if (_autoActivationCoroutine != null)
			{
				StopCoroutine(_autoActivationCoroutine);
			}

			if (ShouldUpdateState)
			{
				_characterButtonActivation = collider.gameObject.MMGetComponentNoAlloc<Character>()?.FindAbility<CharacterButtonActivation>();
				if (_characterButtonActivation != null)
				{
					_characterButtonActivation.InButtonActivatedZone=false;
					_characterButtonActivation.ButtonActivatedZone=null;		
				}
			}

			ExitFeedback?.PlayFeedbacks(this.transform.position);

			if ((_buttonPrompt!=null) && !AlwaysShowPrompt)
			{
				HidePrompt();	
			}

			TriggerExitAction(collider);
		}

		/// <summary>
		/// Tests if the object exiting our zone is the last remaining one
		/// </summary>
		/// <param name="collider"></param>
		/// <returns></returns>
		protected virtual bool TestForLastObject(GameObject collider)
		{
			if (OnlyOneActivationAtOnce)
			{
				if (_collidingObjects.Count > 0)
				{
					bool lastObject = true;
					foreach (GameObject obj in _collidingObjects)
					{
						if ((obj != null) && (obj != collider))
						{
							lastObject = false;
						}
					}
					return lastObject;
				}                    
			}
			return true;            
		}

		/// <summary>
		/// Checks the remaining number of uses and eventual delay between uses and returns true if the zone can be activated.
		/// </summary>
		/// <returns><c>true</c>, if number of uses was checked, <c>false</c> otherwise.</returns>
		public virtual bool CheckNumberOfUses()
		{
			if (!Activable)
			{
				return false;
			}

			if (Time.time - _lastActivationTimestamp < DelayBetweenUses)
			{
				return false;
			}

			if (UnlimitedActivations)
			{
				return true;
			}

			if (_numberOfActivationsLeft == 0)
			{
				return false;
			}

			if (_numberOfActivationsLeft > 0)
			{
				return true;
			}
			return false;
		}

		/// <summary>
		/// Determines whether or not this zone should be activated
		/// </summary>
		/// <returns><c>true</c>, if conditions was checked, <c>false</c> otherwise.</returns>
		/// <param name="character">Character.</param>
		/// <param name="characterButtonActivation">Character button activation.</param>
		protected virtual bool CheckConditions(GameObject collider)
		{
			if (!MMLayers.LayerInLayerMask(collider.layer, TargetLayerMask))
			{
				return false;
			}
			
			Character character = collider.gameObject.MMGetComponentNoAlloc<Character>();

			switch (ButtonActivatedRequirement)
			{
				case ButtonActivatedRequirements.Character:
					if (character == null)
					{
						return false;
					}
					break;

				case ButtonActivatedRequirements.ButtonActivator:
					if (collider.gameObject.MMGetComponentNoAlloc<ButtonActivator>() == null)
					{
						return false;
					}
					break;

				case ButtonActivatedRequirements.Either:
					if ((character == null) && (collider.gameObject.MMGetComponentNoAlloc<ButtonActivator>() == null))
					{
						return false;
					}
					break;
			}

			if (RequiresPlayerType)
			{
				if (character == null)
				{
					return false;
				}
				if (character.CharacterType != Character.CharacterTypes.Player)
				{
					return false;
				}
			}

			if (RequiresButtonActivationAbility)
			{
				CharacterButtonActivation characterButtonActivation = collider.gameObject.MMGetComponentNoAlloc<Character>()?.FindAbility<CharacterButtonActivation>();
				// we check that the object colliding with the water is actually a TopDown controller and a character
				if (characterButtonActivation==null)
				{
					return false;	
				}					
			}

			return true;
		}
	}
}