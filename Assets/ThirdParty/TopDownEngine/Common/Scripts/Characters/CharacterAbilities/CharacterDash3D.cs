using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;
using MoreMountains.InventoryEngine;
using MoreMountains.Feedbacks;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 3D 캐릭터에 이 기능을 추가하면 대시(지정된 시간에 지정된 거리를 이동)할 수 있습니다.
    /// Animation parameters :
    /// Dashing : 캐릭터가 현재 돌진 중이면 true
    /// DashStarted : 대시가 시작되면 true
    /// DashingDirectionX : 정규화된 대시 방향의 x 구성요소
    /// DashingDirectionY : 정규화된 대시 방향의 y 구성요소
    /// DashingDirectionZ : 정규화된 대시 방향의 z 구성요소
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/Abilities/Character Dash 3D")]
	public class CharacterDash3D : CharacterAbility
	{
		/// the possible dash modes (fixed = always the same direction)
		public enum DashModes { Fixed, MainMovement, SecondaryMovement, MousePosition, ModelDirection }
		/// the possible spaces the dash should happen in, either in world coordinates or local ones
		public enum DashSpaces { World, Local }

		/// the current dash mode
		[Tooltip("현재 대시 모드(고정: 항상 같은 방향, MainMovement: 일반적으로 왼쪽 스틱, SecondaryMovement: 일반적으로 오른쪽 스틱, MousePosition: 커서 위치")]
		public DashModes DashMode = DashModes.MainMovement;

		[Header("Dash")]
		/// the space in which the dash should happen, either local or world
		public DashSpaces DashSpace = DashSpaces.World;
        /// 문자를 기준으로 대시의 방향
        [Tooltip("문자를 기준으로 대시의 방향")]
		public Vector3 DashDirection = Vector3.forward;
        /// 커버할 수 있는 거리
        [Tooltip("커버할 수 있는 거리")]
		public float DashDistance = 10f;
        /// 대시의 지속 시간
        [Tooltip("대시의 지속 시간")]
		public float DashDuration = 0.5f;
        /// 대시의 가속도에 적용할 곡선
        [Tooltip("대시의 가속도에 적용할 곡선")]
		public AnimationCurve DashCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));
        /// 이것이 사실이라면 점프하는 동안 대시가 허용되고, 그렇지 않으면 무시됩니다.
        [Tooltip("이것이 사실이라면 점프하는 동안 대시가 허용되고, 그렇지 않으면 무시됩니다.")]
		public bool AllowDashWhenJumping = false;

		[Header("Cooldown")]
        /// 이 능력의 재사용 대기시간
        [Tooltip("이 능력의 재사용 대기시간")]
		public MMCooldown Cooldown;
        
		[Header("Damage")]
        /// 이것이 사실이라면 이 캐릭터는 대시가 진행되는 동안 어떠한 피해도 받지 않을 것입니다.
        [Tooltip("이것이 사실이라면 이 캐릭터는 대시가 진행되는 동안 어떠한 피해도 받지 않을 것입니다.")]
		public bool InvincibleWhileDashing = false; 

		[Header("Feedbacks")]
        /// 대시할 때 플레이할 피드백
        [Tooltip("대시할 때 플레이할 피드백")]
		public MMFeedbacks DashFeedback;

		protected bool _dashing;
		protected bool _dashStartedThisFrame;
		protected float _dashTimer;
		protected Vector3 _dashOrigin;
		protected Vector3 _dashDestination;
		protected Vector3 _newPosition;
		protected Vector3 _oldPosition;
		protected Vector3 _dashAngle = Vector3.zero;
		protected Vector3 _inputDirection;
		protected Vector3 _dashAnimParameterDirection;
		protected Plane _playerPlane;
		protected Camera _mainCamera;
		protected const string _dashingAnimationParameterName = "Dashing";
		protected const string _dashStartedAnimationParameterName = "DashStarted";
		protected const string _dashingDirectionXAnimationParameterName = "DashingDirectionX";
		protected const string _dashingDirectionYAnimationParameterName = "DashingDirectionY";
		protected const string _dashingDirectionZAnimationParameterName = "DashingDirectionZ";
		protected int _dashingAnimationParameter;
		protected int _dashStartedAnimationParameter;
		protected int _dashingDirectionXAnimationParameter;
		protected int _dashingDirectionYAnimationParameter;
		protected int _dashingDirectionZAnimationParameter;
		protected CharacterOrientation3D _characterOrientation3D;
        
		/// <summary>
		/// On init we initialize our cooldown and feedback
		/// </summary>
		protected override void Initialization()
		{
			base.Initialization();
			_playerPlane = new Plane(Vector3.up, Vector3.zero);
			_characterOrientation3D = _character.FindAbility<CharacterOrientation3D>();
			_mainCamera = Camera.main;
			Cooldown.Initialization();
			DashFeedback?.Initialization(this.gameObject);
		}

		/// <summary>
		/// Watches for input and starts a dash if needed
		/// </summary>
		protected override void HandleInput()
		{
			base.HandleInput();
			if (!AbilityAuthorized
			    || (!Cooldown.Ready())
			    || (_condition.CurrentState != CharacterStates.CharacterConditions.Normal))
			{
				return;
			}

			if (!AllowDashWhenJumping && (_movement.CurrentState == CharacterStates.MovementStates.Jumping))
			{
				return;
			}

			if (_inputManager.DashButton.State.CurrentState == MMInput.ButtonStates.ButtonDown)
			{
				DashStart();
			}
		}

		/// <summary>
		/// Starts a dash
		/// </summary>
		public virtual void DashStart()
		{
			if (!Cooldown.Ready())
			{
				return;
			}
			Cooldown.Start();

			_movement.ChangeState(CharacterStates.MovementStates.Dashing);
			_dashing = true;
			_dashTimer = 0f;
			_dashOrigin = this.transform.position;
			_controller.FreeMovement = false;
			_controller3D.DetachFromMovingPlatform();
			DashFeedback?.PlayFeedbacks(this.transform.position);
			PlayAbilityStartFeedbacks();
			_dashStartedThisFrame = true;

			if (InvincibleWhileDashing)
			{
				_health.DamageDisabled();
			}

			HandleDashMode();
		}

		protected virtual void HandleDashMode()
		{
			float angle  = 0f;
			switch (DashMode)
			{
				case DashModes.MainMovement:
					angle = Vector3.SignedAngle(this.transform.forward, _controller.CurrentDirection.normalized, Vector3.up);
					_dashDestination = this.transform.position + DashDirection.normalized * DashDistance;
					_dashAngle.y = angle;
					_dashDestination = MMMaths.RotatePointAroundPivot(_dashDestination, this.transform.position, _dashAngle);
					break;

				case DashModes.Fixed:
					_dashDestination = this.transform.position + DashDirection.normalized * DashDistance;
					break;

				case DashModes.SecondaryMovement:
					_inputDirection = _character.LinkedInputManager.SecondaryMovement;
					_inputDirection.z = _inputDirection.y;
					_inputDirection.y = 0;

					angle = Vector3.SignedAngle(this.transform.forward, _inputDirection.normalized, Vector3.up);
					_dashDestination = this.transform.position + DashDirection.normalized * DashDistance;
					_dashAngle.y = angle;
					_dashDestination = MMMaths.RotatePointAroundPivot(_dashDestination, this.transform.position, _dashAngle);

					_controller.CurrentDirection = (_dashDestination - this.transform.position).normalized;
					break;
				
				case DashModes.ModelDirection:
					_dashDestination = this.transform.position + _characterOrientation3D.ModelDirection.normalized * DashDistance;
					break;

				case DashModes.MousePosition:
					Ray ray = _mainCamera.ScreenPointToRay(InputManager.Instance.MousePosition);
					Debug.DrawRay(ray.origin, ray.direction * 100, Color.yellow);
					float distance;
					_playerPlane.SetNormalAndPosition(_playerPlane.normal, this.transform.position);
					if (_playerPlane.Raycast(ray, out distance))
					{
						_inputDirection = ray.GetPoint(distance);
					}

					angle = Vector3.SignedAngle(this.transform.forward, (_inputDirection - this.transform.position).normalized, Vector3.up);
					_dashDestination = this.transform.position + DashDirection.normalized * DashDistance;
					_dashAngle.y = angle;
					_dashDestination = MMMaths.RotatePointAroundPivot(_dashDestination, this.transform.position, _dashAngle);

					_controller.CurrentDirection = (_dashDestination - this.transform.position).normalized;
					break;
			}
		}

		/// <summary>
		/// Stops the dash
		/// </summary>
		public virtual void DashStop()
		{
			Cooldown.Stop();
			_movement.ChangeState(CharacterStates.MovementStates.Idle);
			_dashing = false;
			_controller.FreeMovement = true;
			DashFeedback?.StopFeedbacks();
			StopStartFeedbacks();
			PlayAbilityStopFeedbacks();
			if (InvincibleWhileDashing)
			{
				_health.DamageEnabled();
			}
		}

        /// <summary>
        /// 프로세스 능력에서 현재 대시 중인 경우 캐릭터를 이동합니다.
        /// </summary>
        public override void ProcessAbility()
		{
			base.ProcessAbility();
			Cooldown.Update();

			if (_dashing)
			{
				if (_dashTimer < DashDuration)
				{
					_dashAnimParameterDirection = (_dashDestination - _dashOrigin).normalized;
					if (DashSpace == DashSpaces.World)
					{
						_newPosition = Vector3.Lerp(_dashOrigin, _dashDestination, DashCurve.Evaluate(_dashTimer / DashDuration));	
						_dashTimer += Time.deltaTime;
						_controller.MovePosition(_newPosition);
					}
					else
					{
						_oldPosition = _dashTimer == 0 ? _dashOrigin : _newPosition;
						_newPosition = Vector3.Lerp(_dashOrigin, _dashDestination, DashCurve.Evaluate(_dashTimer / DashDuration));
						_dashTimer += Time.deltaTime;
						_controller.MovePosition(this.transform.position + _newPosition - _oldPosition);
					}
				}
				else
				{
					DashStop();                   
				}
			}
		}
        
		/// <summary>
		/// Adds required animator parameters to the animator parameters list if they exist
		/// </summary>
		protected override void InitializeAnimatorParameters()
		{
			RegisterAnimatorParameter(_dashingAnimationParameterName, AnimatorControllerParameterType.Bool, out _dashingAnimationParameter);
			RegisterAnimatorParameter(_dashStartedAnimationParameterName, AnimatorControllerParameterType.Bool, out _dashStartedAnimationParameter);
			RegisterAnimatorParameter(_dashingDirectionXAnimationParameterName, AnimatorControllerParameterType.Float, out _dashingDirectionXAnimationParameter);
			RegisterAnimatorParameter(_dashingDirectionYAnimationParameterName, AnimatorControllerParameterType.Float, out _dashingDirectionYAnimationParameter);
			RegisterAnimatorParameter(_dashingDirectionZAnimationParameterName, AnimatorControllerParameterType.Float, out _dashingDirectionZAnimationParameter);
		}

		/// <summary>
		/// At the end of each cycle, we send our Running status to the character's animator
		/// </summary>
		public override void UpdateAnimator()
		{
			MMAnimatorExtensions.UpdateAnimatorBool(_animator, _dashingAnimationParameter, (_movement.CurrentState == CharacterStates.MovementStates.Dashing), _character._animatorParameters, _character.RunAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorBool(_animator, _dashStartedAnimationParameter, _dashStartedThisFrame, _character._animatorParameters, _character.RunAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _dashingDirectionXAnimationParameter, _dashAnimParameterDirection.x, _character._animatorParameters, _character.RunAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _dashingDirectionYAnimationParameter, _dashAnimParameterDirection.y, _character._animatorParameters, _character.RunAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _dashingDirectionZAnimationParameter, _dashAnimParameterDirection.z, _character._animatorParameters, _character.RunAnimatorSanityChecks);

			_dashStartedThisFrame = false;
		}
	}
}