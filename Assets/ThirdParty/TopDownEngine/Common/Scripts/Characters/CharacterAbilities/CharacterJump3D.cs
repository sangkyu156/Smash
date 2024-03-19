using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 능력을 사용하면 캐릭터가 3D로 점프할 수 있습니다.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/Abilities/Character Jump 3D")]
	public class CharacterJump3D : CharacterAbility 
	{
		[Header("Jump Settings")]
        /// 점프가 누르기에 비례해야 하는지 여부(그렇다면 버튼을 놓으면 점프가 중지됩니다)
        [Tooltip("점프가 누르기에 비례해야 하는지 여부(그렇다면 버튼을 놓으면 점프가 중지됩니다)")]
		public bool JumpProportionalToPress = true;
        /// 점프 시작 후 점프 버튼을 놓기 전까지의 최소 시간은 효과가 있습니다.
        [Tooltip("점프 시작 후 점프 버튼을 놓기 전까지의 최소 시간은 효과가 있습니다.")]
		public float MinimumPressTime = 0.4f;
        /// 점프에 적용되는 힘, 점프가 높을수록 점프 속도가 빨라집니다.
        [Tooltip("점프에 적용되는 힘, 점프가 높을수록 점프 속도가 빨라집니다.")]
		public float JumpForce = 800f;
        /// 점프해야 할 높이
        [Tooltip("점프해야 할 높이")]
		public float JumpHeight = 4f;

		[Header("Slopes")]
        /// 너무 가파른 경사면에 서서 걸을 수 없는 경우 캐릭터가 점프할 수 있는지 여부
        [Tooltip("너무 가파른 경사면에 서서 걸을 수 없는 경우 캐릭터가 점프할 수 있는지 여부")]
		public bool CanJumpOnTooSteepSlopes = true;
        /// 걷기에 너무 가파른 경사면에 서 있으면 점프 카운터가 재설정되어야 하는지 여부
        [Tooltip("걷기에 너무 가파른 경사면에 서 있으면 점프 카운터가 재설정되어야 하는지 여부")]
		public bool ResetJumpsOnTooSteepSlopes = false;
        
		[Header("Number of Jumps")]
        /// 허용되는 최대 점프 수 (0 : 점프 없음, 1 : 일반 점프, 2 : 더블 점프 등...)
        [Tooltip("허용되는 최대 점프 수 (0 : 점프 없음, 1 : 일반 점프, 2 : 더블 점프 등...)")]
		public int NumberOfJumps = 1;
        /// 캐릭터에게 남은 점프 횟수
        [MMReadOnly]
		[Tooltip("캐릭터에게 남은 점프 횟수")]
		public int NumberOfJumpsLeft = 0;

		[Header("Feedbacks")]
        /// 점프가 시작될 때 재생할 피드백
        [Tooltip("점프가 시작될 때 재생할 피드백")]
		public MMFeedbacks JumpStartFeedback;
        /// 점프가 멈출 때 재생할 피드백
        [Tooltip("점프가 멈출 때 재생할 피드백")]
		public MMFeedbacks JumpStopFeedback;

		protected bool _doubleJumping;
		protected Vector3 _jumpForce;
		protected Vector3 _jumpOrigin;
		protected CharacterButtonActivation _characterButtonActivation;
		protected CharacterCrouch _characterCrouch;
		protected bool _jumpStopped = false;
		protected float _jumpStartedAt = 0f;
		protected bool _buttonReleased = false;
		protected int _initialNumberOfJumps;

		protected const string _jumpingAnimationParameterName = "Jumping";
		protected const string _doubleJumpingAnimationParameterName = "DoubleJumping";
		protected const string _hitTheGroundAnimationParameterName = "HitTheGround";
		protected int _jumpingAnimationParameter;
		protected int _doubleJumpingAnimationParameter;
		protected int _hitTheGroundAnimationParameter;

		/// <summary>
		/// On init we grab other components
		/// </summary>
		protected override void Initialization()
		{
			base.Initialization ();
			ResetNumberOfJumps();
			_jumpStopped = true;
			_characterButtonActivation = _character?.FindAbility<CharacterButtonActivation> ();
			_characterCrouch = _character?.FindAbility<CharacterCrouch> ();
			JumpStartFeedback?.Initialization(this.gameObject);
			JumpStopFeedback?.Initialization(this.gameObject);
			_initialNumberOfJumps = NumberOfJumps;
		}

		/// <summary>
		/// Watches for input and triggers a jump if needed
		/// </summary>
		protected override void HandleInput()
		{
			base.HandleInput();
			// if movement is prevented, or if the character is dead/frozen/can't move, we exit and do nothing
			if (!AbilityAuthorized
			    || (_condition.CurrentState != CharacterStates.CharacterConditions.Normal))
			{
				return;
			}
			if (_inputManager.JumpButton.State.CurrentState == MMInput.ButtonStates.ButtonDown)
			{
				JumpStart();
			}
			if (_inputManager.JumpButton.State.CurrentState == MMInput.ButtonStates.ButtonUp)
			{               
				_buttonReleased = true;                               
			}
		}

        /// <summary>
        /// 처리 능력에 대해서는 점프를 멈춰야 하는지 확인합니다.
        /// </summary>
        public override void ProcessAbility()
		{
			if (_controller.JustGotGrounded)
			{
				ResetNumberOfJumps();
			}

			// if movement is prevented, or if the character is dead/frozen/can't move, we exit and do nothing
			if (!AbilityAuthorized
			    || (_condition.CurrentState != CharacterStates.CharacterConditions.Normal))
			{
				return;
			}

			if (!_jumpStopped
			    &&
			    ((_movement.CurrentState == CharacterStates.MovementStates.Idle)
			     || (_movement.CurrentState == CharacterStates.MovementStates.Walking)
			     || (_movement.CurrentState == CharacterStates.MovementStates.Running)
			     || (_movement.CurrentState == CharacterStates.MovementStates.Crouching)
			     || (_movement.CurrentState == CharacterStates.MovementStates.Crawling)
			     || (_movement.CurrentState == CharacterStates.MovementStates.Pushing)
			     || (_movement.CurrentState == CharacterStates.MovementStates.Falling)
			    ))
			{
				JumpStop();
			}

			if (_movement.CurrentState == CharacterStates.MovementStates.Jumping)
			{
				if (_buttonReleased 
				    && !_jumpStopped
				    && JumpProportionalToPress 
				    && (Time.time - _jumpStartedAt > MinimumPressTime))
				{
					JumpStop();
				}
	            
				if (!_jumpStopped)
				{
					if ((this.transform.position.y - _jumpOrigin.y > JumpHeight)
					    || CeilingTest())
					{
						JumpStop();
						_controller3D.Grounded = _controller3D.IsGroundedTest();
						if (_controller.Grounded)
						{
							ResetNumberOfJumps();  
						}
					}
					else
					{
						_jumpForce = Vector3.up * JumpForce * Time.deltaTime;
						_controller.AddForce(_jumpForce);
					}
				}
			}

			if (!ResetJumpsOnTooSteepSlopes && _controller3D.ExitedTooSteepSlopeThisFrame && _controller3D.Grounded)
			{
				ResetNumberOfJumps();
			}
		}

		/// <summary>
		/// Returns true if a ceiling's found above the character, false otherwise
		/// </summary>
		protected virtual bool CeilingTest()
		{
			bool returnValue = _controller3D.CollidingAbove();
			return returnValue;
		}

		/// <summary>
		/// On jump start, we change our state to jumping
		/// </summary>
		public virtual void JumpStart()
		{
			if (!EvaluateJumpConditions())
			{
				return;
			}

			// we decrease the number of jumps left
			NumberOfJumpsLeft = NumberOfJumpsLeft - 1;

			_movement.ChangeState(CharacterStates.MovementStates.Jumping);	
			MMCharacterEvent.Trigger(_character, MMCharacterEventTypes.Jump);
			JumpStartFeedback?.PlayFeedbacks(this.transform.position);
			_jumpOrigin = this.transform.position;
			_jumpStopped = false;
			_jumpStartedAt = Time.time;
			_controller.Grounded = false;
			_controller.GravityActive = false;
			_buttonReleased = false;

			PlayAbilityStartSfx();
			PlayAbilityUsedSfx();
			PlayAbilityStartFeedbacks();
		}

		/// <summary>
		/// Stops the jump
		/// </summary>
		public virtual void JumpStop()
		{
			_controller.GravityActive = true;
			if (_controller.Velocity.y > 0)
			{
				_controller.Velocity.y = 0f;
			}
			_jumpStopped = true;
			_buttonReleased = false;
			PlayAbilityStopSfx();
			StopAbilityUsedSfx();
			StopStartFeedbacks();
			PlayAbilityStopFeedbacks();
			JumpStopFeedback?.PlayFeedbacks(this.transform.position);
		}

		/// <summary>
		/// Resets the number of jumps.
		/// </summary>
		public virtual void ResetNumberOfJumps()
		{
			bool shouldResetJumps = true;

			if (!ResetJumpsOnTooSteepSlopes)
			{
				if (_controller3D.TooSteep())
				{
					shouldResetJumps = false;
				}
			}

			if (shouldResetJumps)
			{
				NumberOfJumpsLeft = NumberOfJumps;
			}
		}

		/// <summary>
		/// Evaluates the jump conditions and returns true if a jump can be performed
		/// </summary>
		/// <returns></returns>
		protected virtual bool EvaluateJumpConditions()
		{
			if (!AbilityAuthorized)
			{
				return false;
			}
			if (_characterButtonActivation != null)
			{
				if (_characterButtonActivation.AbilityAuthorized
				    && _characterButtonActivation.InButtonActivatedZone
				    && _characterButtonActivation.PreventJumpInButtonActivatedZone)
				{
					return false;
				}
			}

			if (!CanJumpOnTooSteepSlopes)
			{
				if (_controller3D.TooSteep())
				{
					return false;
				}
			}

			if (_characterCrouch != null)
			{
				if (_characterCrouch.InATunnel)
				{
					return false;
				}
			}

			if (CeilingTest())
			{
				return false;
			}

			if (NumberOfJumpsLeft <= 0)
			{
				return false;
			}

			if (_movement.CurrentState == CharacterStates.MovementStates.Dashing)
			{
				return false;
			}
			return true;
		}

		/// <summary>
		/// Adds required animator parameters to the animator parameters list if they exist
		/// </summary>
		protected override void InitializeAnimatorParameters()
		{
			RegisterAnimatorParameter (_jumpingAnimationParameterName, AnimatorControllerParameterType.Bool, out _jumpingAnimationParameter);
			RegisterAnimatorParameter (_doubleJumpingAnimationParameterName, AnimatorControllerParameterType.Bool, out _doubleJumpingAnimationParameter);
			RegisterAnimatorParameter (_hitTheGroundAnimationParameterName, AnimatorControllerParameterType.Bool, out _hitTheGroundAnimationParameter);
		}

		/// <summary>
		/// At the end of each cycle, sends Jumping states to the Character's animator
		/// </summary>
		public override void UpdateAnimator()
		{
			MMAnimatorExtensions.UpdateAnimatorBool(_animator, _jumpingAnimationParameter, (_movement.CurrentState == CharacterStates.MovementStates.Jumping),_character._animatorParameters, _character.RunAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorBool(_animator, _doubleJumpingAnimationParameter, _doubleJumping,_character._animatorParameters, _character.RunAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorBool (_animator, _hitTheGroundAnimationParameter, _controller.JustGotGrounded, _character._animatorParameters, _character.RunAnimatorSanityChecks);
		}
	}
}