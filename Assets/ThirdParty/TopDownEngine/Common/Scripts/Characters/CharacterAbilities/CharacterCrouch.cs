using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 능력을 사용하면 충돌기의 크기를 조정하는 웅크리기 버튼을 누를 때 캐릭터가 "웅크릴" 수 있습니다.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/Abilities/Character Crouch")]
	public class CharacterCrouch : CharacterAbility 
	{
		/// This method is only used to display a helpbox text at the beginning of the ability's inspector
		public override string HelpBoxText() { return "This component handles crouch and crawl behaviours. Here you can determine the crouch speed, and whether or not the collider should resize when crouched (to crawl into tunnels for example). If it should, please setup its new size here."; }

        /// 이것이 사실이라면 캐릭터는 ForcedCrouch 모드에 있는 것입니다. CrouchZone 또는 AI 스크립트가 이를 수행할 수 있습니다.
        [MMReadOnly]
		[Tooltip("이것이 사실이라면 캐릭터는 ForcedCrouch 모드에 있는 것입니다. CrouchZone 또는 AI 스크립트가 이를 수행할 수 있습니다.")]
		public bool ForcedCrouch = false;

		[Header("Crawl")]

        /// false로 설정하면 캐릭터는 기어갈 수 없고 단지 웅크릴 수 있습니다.
        [Tooltip("false로 설정하면 캐릭터는 기어갈 수 없고 단지 웅크릴 수 있습니다.")]
		public bool CrawlAuthorized = true;
        /// 웅크리고 있을 때 캐릭터의 속도
        [Tooltip("웅크리고 있을 때 캐릭터의 속도")]
		public float CrawlSpeed = 4f;

		[Space(10)]	
		[Header("Crouching")]

        /// 이것이 사실이라면 웅크려 있을 때 충돌체의 크기가 조정됩니다.
        [Tooltip("이것이 사실이라면 웅크려 있을 때 충돌체의 크기가 조정됩니다.")]
		public bool ResizeColliderWhenCrouched = false;
        /// 이것이 사실이라면 충돌체는 크기 조정 시 수직으로 변환됩니다. 이렇게 하면 중앙이 y:0에 있지 않은 경우 컨트롤러가 땅으로 순간이동하는 것을 방지할 수 있습니다.
        [Tooltip("이것이 사실이라면 충돌체는 크기 조정 시 수직으로 변환됩니다. 이렇게 하면 중앙이 y:0에 있지 않은 경우 컨트롤러가 땅으로 순간이동하는 것을 방지할 수 있습니다.")]
		[MMCondition("ResizeColliderWhenCrouched", true)]
		public bool TranslateColliderOnCrouch = false;
        /// 웅크리고 있을 때 충돌체에 적용할 크기(ResizeColliderWhenCrouched가 true인 경우 그렇지 않으면 무시됩니다)
        [Tooltip("웅크리고 있을 때 충돌체에 적용할 크기(ResizeColliderWhenCrouched가 true인 경우 그렇지 않으면 무시됩니다)")]
		public float CrouchedColliderHeight = 1.25f;

		[Space(10)]	
		[Header("Offset")]

        /// 웅크리고 있을 때 오프셋할 객체 목록
        [Tooltip("웅크리고 있을 때 오프셋할 객체 목록")]
		public List<GameObject> ObjectsToOffset;
        /// 웅크리고 있을 때 객체에 적용할 오프셋
        [Tooltip("웅크리고 있을 때 객체에 적용할 오프셋")]
		public Vector3 OffsetCrouch;
        /// 웅크리고 움직일 때 객체에 적용할 오프셋
        [Tooltip("웅크리고 움직일 때 객체에 적용할 오프셋")]
		public Vector3 OffsetCrawl;
        /// 객체를 오프셋하는 속도
        [Tooltip("객체를 오프셋하는 속도")]
		public float OffsetSpeed = 5f;

        /// 캐릭터가 지금 터널에 있어서 일어날 수 없는지 여부
        [MMReadOnly]
		[Tooltip("캐릭터가 지금 터널에 있어서 일어날 수 없는지 여부")]
		public bool InATunnel;

		protected List<Vector3> _objectsToOffsetOriginalPositions;
		protected const string _crouchingAnimationParameterName = "Crouching";
		protected const string _crawlingAnimationParameterName = "Crawling";
		protected int _crouchingAnimationParameter;
		protected int _crawlingAnimationParameter;
		protected bool _crouching = false;
		protected CharacterRun _characterRun;

		/// <summary>
		/// On Start(), we set our tunnel flag to false
		/// </summary>
		protected override void Initialization()
		{
			base.Initialization();
			InATunnel = false;
			_characterRun = _character.FindAbility<CharacterRun>();

			// we store our objects to offset's initial positions
			if (ObjectsToOffset.Count > 0)
			{
				_objectsToOffsetOriginalPositions = new List<Vector3> ();
				foreach(GameObject go in ObjectsToOffset)
				{
					if (go != null)
					{
						_objectsToOffsetOriginalPositions.Add(go.transform.localPosition);
					}					
				}
			}
		}

		/// <summary>
		/// Every frame, we check if we're crouched and if we still should be
		/// </summary>
		public override void ProcessAbility()
		{
			base.ProcessAbility();
			HandleForcedCrouch();
			DetermineState ();
			CheckExitCrouch();
			OffsetObjects ();
		}

		/// <summary>
		/// If we're in forced crouch state, we crouch
		/// </summary>
		protected virtual void HandleForcedCrouch()
		{
			if (ForcedCrouch && (_movement.CurrentState != CharacterStates.MovementStates.Crouching) && (_movement.CurrentState != CharacterStates.MovementStates.Crawling))
			{
				Crouch();
			}
		}

		/// <summary>
		/// At the start of the ability's cycle, we check if we're pressing down. If yes, we call Crouch()
		/// </summary>
		protected override void HandleInput()
		{			
			base.HandleInput ();

			// Crouch Detection : if the player is pressing "down" and if the character is grounded and the crouch action is enabled
			if (_inputManager.CrouchButton.State.CurrentState == MMInput.ButtonStates.ButtonDown)		
			{
				Crouch();
			}
		}

		/// <summary>
		/// Starts a forced crouch
		/// </summary>
		public virtual void StartForcedCrouch()
		{
			ForcedCrouch = true;
			_crouching = true;
		}

		/// <summary>
		/// Stops a forced crouch
		/// </summary>
		public virtual void StopForcedCrouch()
		{
			ForcedCrouch = false;
			_crouching = false;
		}

		/// <summary>
		/// If we're pressing down, we check if we can crouch or crawl, and change states accordingly
		/// </summary>
		protected virtual void Crouch()
		{
			if (!AbilityAuthorized// if the ability is not permitted
			    || (_condition.CurrentState != CharacterStates.CharacterConditions.Normal)// or if we're not in our normal stance
			    || (!_controller.Grounded))// or if we're grounded
				// we do nothing and exit
			{
				return;
			}				

			// if this is the first time we're here, we trigger our sounds
			if ((_movement.CurrentState != CharacterStates.MovementStates.Crouching) && (_movement.CurrentState != CharacterStates.MovementStates.Crawling))
			{
				// we play the crouch start sound 
				PlayAbilityStartFeedbacks();
				PlayAbilityStartSfx();
				PlayAbilityUsedSfx();
			}

			if (_movement.CurrentState == CharacterStates.MovementStates.Running)
			{
				_characterRun.RunStop();
			}

			_crouching = true;

			// we set the character's state to Crouching and if it's also moving we set it to Crawling
			_movement.ChangeState(CharacterStates.MovementStates.Crouching);
			if ( (Mathf.Abs(_horizontalInput) > 0) && (CrawlAuthorized) )
			{
				_movement.ChangeState(CharacterStates.MovementStates.Crawling);
			}

			// we resize our collider to match the new shape of our character (it's usually smaller when crouched)
			if (ResizeColliderWhenCrouched)
			{
				_controller.ResizeColliderHeight(CrouchedColliderHeight, TranslateColliderOnCrouch);		
			}

			// we change our character's speed
			if (_characterMovement != null)
			{
				_characterMovement.MovementSpeed = CrawlSpeed;
			}

			// we prevent movement if we can't crawl
			if (!CrawlAuthorized)
			{
				_characterMovement.MovementSpeed = 0f;
			}
		}

		protected virtual void OffsetObjects ()
		{
			// we move all the objects we want to move
			if (ObjectsToOffset.Count > 0)
			{
				for (int i = 0; i < ObjectsToOffset.Count; i++)
				{
					Vector3 newOffset = Vector3.zero;
					if (_movement.CurrentState == CharacterStates.MovementStates.Crouching)
					{
						newOffset = OffsetCrouch;
					}
					if (_movement.CurrentState == CharacterStates.MovementStates.Crawling)
					{
						newOffset = OffsetCrawl;
					}
					if (ObjectsToOffset[i] != null)
					{
						ObjectsToOffset[i].transform.localPosition = Vector3.Lerp(ObjectsToOffset[i].transform.localPosition, _objectsToOffsetOriginalPositions[i] + newOffset, Time.deltaTime * OffsetSpeed);
					}					
				}
			}
		}

		/// <summary>
		/// Runs every frame to check if we should switch from crouching to crawling or the other way around
		/// </summary>
		protected virtual void DetermineState()
		{
			if ((_movement.CurrentState == CharacterStates.MovementStates.Crouching) || (_movement.CurrentState == CharacterStates.MovementStates.Crawling))
			{
				if ( (_controller.CurrentMovement.magnitude > 0) && (CrawlAuthorized) )
				{
					_movement.ChangeState(CharacterStates.MovementStates.Crawling);
				}
				else
				{
					_movement.ChangeState(CharacterStates.MovementStates.Crouching);
				}
			}
		}

		/// <summary>
		/// Every frame, we check to see if we should exit the Crouching (or Crawling) state
		/// </summary>
		protected virtual void CheckExitCrouch()
		{				
			// if we're currently grounded
			if ( (_movement.CurrentState == CharacterStates.MovementStates.Crouching)
			     || (_movement.CurrentState == CharacterStates.MovementStates.Crawling)
			     || _crouching)
			{	
				if (_inputManager == null)
				{
					if (!ForcedCrouch)
					{
						ExitCrouch();
					}
					return;
				}

				// but we're not pressing down anymore, or we're not grounded anymore
				if ( (!_controller.Grounded) 
				     || ((_movement.CurrentState != CharacterStates.MovementStates.Crouching) 
				         && (_movement.CurrentState != CharacterStates.MovementStates.Crawling)
				         && (_inputManager.CrouchButton.State.CurrentState == MMInput.ButtonStates.Off) && (!ForcedCrouch))
				     || ((_inputManager.CrouchButton.State.CurrentState == MMInput.ButtonStates.Off) && (!ForcedCrouch)))
				{
					// we cast a raycast above to see if we have room enough to go back to normal size
					InATunnel = !_controller.CanGoBackToOriginalSize();

					// if the character is not in a tunnel, we can go back to normal size
					if (!InATunnel)
					{
						ExitCrouch();
					}
				}
			}
		}

		/// <summary>
		/// Returns the character to normal stance
		/// </summary>
		protected virtual void ExitCrouch()
		{
			_crouching = false;
	        
			// we return to normal walking speed
			if (_characterMovement != null)
			{
				_characterMovement.ResetSpeed();
			}

			// we play our exit sound
			StopAbilityUsedSfx();
			PlayAbilityStopSfx();
			StopStartFeedbacks();
			PlayAbilityStopFeedbacks();

			// we go back to Idle state and reset our collider's size
			if ((_movement.CurrentState == CharacterStates.MovementStates.Crawling) ||
			    (_movement.CurrentState == CharacterStates.MovementStates.Crouching))
			{
				_movement.ChangeState(CharacterStates.MovementStates.Idle);    
			}
            
			_controller.ResetColliderSize();
		}

		/// <summary>
		/// Adds required animator parameters to the animator parameters list if they exist
		/// </summary>
		protected override void InitializeAnimatorParameters()
		{
			RegisterAnimatorParameter (_crouchingAnimationParameterName, AnimatorControllerParameterType.Bool, out _crouchingAnimationParameter);
			RegisterAnimatorParameter (_crawlingAnimationParameterName, AnimatorControllerParameterType.Bool, out _crawlingAnimationParameter);
		}

		/// <summary>
		/// At the end of the ability's cycle, we send our current crouching and crawling states to the animator
		/// </summary>
		public override void UpdateAnimator()
		{
			MMAnimatorExtensions.UpdateAnimatorBool(_animator, _crouchingAnimationParameter, (_movement.CurrentState == CharacterStates.MovementStates.Crouching), _character._animatorParameters, _character.RunAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorBool(_animator,_crawlingAnimationParameter,(_movement.CurrentState == CharacterStates.MovementStates.Crawling), _character._animatorParameters, _character.RunAnimatorSanityChecks);
		}
	}
}