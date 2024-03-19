using UnityEngine;
using MoreMountains.Tools;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 구성 요소를 캐릭터에 추가하면 버튼 영역을 활성화할 수 있습니다.
    /// Animator parameters : Activating (bool)
    /// </summary>
    [MMHiddenProperties("AbilityStopFeedbacks")]
	[AddComponentMenu("TopDown Engine/Character/Abilities/Character Button Activation")] 
	public class CharacterButtonActivation : CharacterAbility 
	{
		/// This method is only used to display a helpbox text at the beginning of the ability's inspector
		public override string HelpBoxText() { return "This component allows your character to interact with button powered objects (dialogue zones, switches...). "; }
		/// true if the character is in a dialogue zone
		public bool InButtonActivatedZone {get;set;}
		/// true if the zone is automated
		public bool InButtonAutoActivatedZone { get; set; }
        /// 이것이 사실이라면 버튼이 활성화된 영역에 있는 동안 캐릭터는 점프할 수 없습니다.
        [Tooltip("이것이 사실이라면 버튼이 활성화된 영역에 있는 동안 캐릭터는 점프할 수 없습니다.")]
		public bool PreventJumpInButtonActivatedZone = true;
        /// 이 캐릭터가 있는 현재 버튼 활성화 구역
        [Tooltip("이 캐릭터가 있는 현재 버튼 활성화 구역")]
		[MMReadOnly]
		public ButtonActivated ButtonActivatedZone;

		protected bool _activating = false;
		protected const string _activatingAnimationParameterName = "Activating";
		protected int _activatingAnimationParameter;
        
		/// <summary>
		/// Gets and stores components for further use
		/// </summary>
		protected override void Initialization()
		{
			base.Initialization();
			InButtonActivatedZone = false;
			ButtonActivatedZone = null;
			InButtonAutoActivatedZone = false;

		}

		/// <summary>
		/// Every frame, we check the input to see if we need to pause/unpause the game
		/// </summary>
		protected override void HandleInput()
		{
			if (!AbilityAuthorized)
			{
				return;
			}
			if (InButtonActivatedZone && (ButtonActivatedZone != null))
			{
				bool buttonPressed = false;
				switch (ButtonActivatedZone.InputType)
				{
					case ButtonActivated.InputTypes.Default:
						buttonPressed = (_inputManager.InteractButton.State.CurrentState == MMInput.ButtonStates.ButtonDown);
						break;
					#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
						case ButtonActivated.InputTypes.Button:
						case ButtonActivated.InputTypes.Key:
							buttonPressed = ButtonActivatedZone.InputActionPerformed;
							break;
					#else
						case ButtonActivated.InputTypes.Button:
							buttonPressed = (Input.GetButtonDown(_character.PlayerID + "_" + ButtonActivatedZone.InputButton));
							break;
						case ButtonActivated.InputTypes.Key:
							buttonPressed = (Input.GetKeyDown(ButtonActivatedZone.InputKey));
							break;
					#endif
				}

				if (buttonPressed)
				{
					ButtonActivation();
				}
			}
		}
        
		/// <summary>
		/// Tries to activate the button activated zone
		/// </summary>
		protected virtual void ButtonActivation()
		{
			// if the player is in a button activated zone, we handle it
			if ((InButtonActivatedZone)
			    && (ButtonActivatedZone!=null)
			    && (_condition.CurrentState == CharacterStates.CharacterConditions.Normal || _condition.CurrentState == CharacterStates.CharacterConditions.Frozen)
			    && (_movement.CurrentState != CharacterStates.MovementStates.Dashing))
			{
				// if the button can only be activated while grounded and if we're not grounded, we do nothing and exit
				if (ButtonActivatedZone.CanOnlyActivateIfGrounded && !_controller.Grounded)
				{
					return;
				}

				// if it's an auto activated zone, we do nothing	
				if (ButtonActivatedZone.AutoActivation)
				{
					return;
				}

				// we trigger a character event
				MMCharacterEvent.Trigger(_character, MMCharacterEventTypes.ButtonActivation);

				ButtonActivatedZone.TriggerButtonAction();
				PlayAbilityStartFeedbacks();
				_activating = true;
			}
		}

		/// <summary>	
		/// On Death we lose any connection we may have had to a button activated zone	
		/// </summary>	
		protected override void OnDeath()
		{
			base.OnDeath();
			InButtonActivatedZone = false;
			ButtonActivatedZone = null;
			InButtonAutoActivatedZone = false;
		}

		/// <summary>
		/// Adds required animator parameters to the animator parameters list if they exist
		/// </summary>
		protected override void InitializeAnimatorParameters()
		{
			RegisterAnimatorParameter (_activatingAnimationParameterName, AnimatorControllerParameterType.Bool, out _activatingAnimationParameter);
		}

		/// <summary>
		/// At the end of the ability's cycle, we send our current crouching and crawling states to the animator
		/// </summary>
		public override void UpdateAnimator()
		{
			MMAnimatorExtensions.UpdateAnimatorBool(_animator, _activatingAnimationParameter, _activating, _character._animatorParameters, _character.RunAnimatorSanityChecks);
			if (_activating && (ButtonActivatedZone != null) && (ButtonActivatedZone.AnimationTriggerParameterName != ""))
			{
				_animator.SetTrigger(ButtonActivatedZone.AnimationTriggerParameterName);
			}
			_activating = false;
		}
	}
}