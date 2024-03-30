using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 3D의 경우 x 및 z 방향, 2D의 경우 x 및 y 방향으로 지상 이동(걷기, 잠재적으로 뛰기, 기어다니기 등)을 처리하도록 캐릭터에 이 기능을 추가하세요.
    /// 애니메이터 매개변수: 속도(float), 걷기(bool)
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/Abilities/Character Movement")] 
	public class CharacterMovement : CharacterAbility 
	{
        /// 캐릭터에 가능한 회전 모드
        public enum Movements { Free, Strict2DirectionsHorizontal, Strict2DirectionsVertical, Strict4Directions, Strict8Directions }


		/// 현재 기준 이동 속도
		[SerializeField]
		public float MovementSpeed { get; set; }
        /// 이것이 사실이라면 이동이 금지됩니다(뒤집기 포함).
        public bool MovementForbidden { get; set; }

		[Header("Direction")]

        /// 캐릭터가 2D에서만 4방향 또는 8방향으로 자유롭게 움직일 수 있는지 여부
        [Tooltip("캐릭터가 2D에서만 4방향 또는 8방향으로 자유롭게 움직일 수 있는지 여부")]
		public Movements Movement = Movements.Free;

		[Header("Settings")]

        /// 해당 시점의 움직임 입력이 승인되었는지 여부
        [Tooltip("해당 시점의 움직임 입력이 승인되었는지 여부")]
		public bool InputAuthorized = true;
        /// 입력이 아날로그여야 하는지 여부
        [Tooltip("입력이 아날로그여야 하는지 여부")]
		public bool AnalogInput = false;
        /// 다른 스크립트에서 입력을 설정해야 하는지 여부
        [Tooltip("다른 스크립트에서 입력을 설정해야 하는지 여부")]
		public bool ScriptDrivenInput = false;

		[Header("Speed")]

        /// 캐릭터가 걸을 때의 속도
        [Tooltip("캐릭터가 걸을 때의 속도")]
		public float WalkSpeed = 6f;
        /// 이 구성요소가 컨트롤러의 움직임을 설정해야 하는지 여부
        [Tooltip("이 구성요소가 컨트롤러의 움직임을 설정해야 하는지 여부")]
		public bool ShouldSetMovement = true;
        /// 캐릭터가 더 이상 유휴 상태로 간주되지 않는 속도 임계값
        [Tooltip("캐릭터가 더 이상 유휴 상태로 간주되지 않는 속도 임계값")]
		public float IdleThreshold = 0.05f;

        [Header("Acceleration")]

        /// 현재 속도에 적용할 가속도 / 0f : 가속도 없음, 순간 최고 속도
        [Tooltip("현재 속도에 적용할 가속도 / 0f : 가속도 없음, 순간 최고 속도")]
		public float Acceleration = 10f;
        /// 현재 속도에 적용할 감속 / 0f : 감속 없음, 즉시 정지
        [Tooltip("현재 속도에 적용할 감속 / 0f : 감속 없음, 즉시 정지")]
		public float Deceleration = 10f;

        /// 이동 속도를 보간할지 말지
        [Tooltip("이동 속도를 보간할지 말지")]
		public bool InterpolateMovementSpeed = false;
		public float MovementSpeedMaxMultiplier { get; set; } = float.MaxValue;
		private float _movementSpeedMultiplier;
        /// 수평 운동에 적용할 승수
        public float MovementSpeedMultiplier
		{
			get => Mathf.Min(_movementSpeedMultiplier, MovementSpeedMaxMultiplier);
			set => _movementSpeedMultiplier = value;
		}
        /// 컨텍스트 요소(movement 구역 등)에 의해 적용되는 수평 이동에 적용할 승수
        public Stack<float> ContextSpeedStack = new Stack<float>();
		public float ContextSpeedMultiplier => ContextSpeedStack.Count > 0 ? ContextSpeedStack.Peek() : 1;

		[Header("워크 피드백")]
        /// 걷는 동안 유발할 입자들
        [Tooltip("걷는 동안 유발할 입자들")]
		public ParticleSystem[] WalkParticles;

		[Header("터치 더 그라운드 피드백")]
        /// 땅에 닿을 때 유발할 입자들
        [Tooltip("땅에 닿을 때 유발할 입자들")]
		public ParticleSystem[] TouchTheGroundParticles;
        /// 땅에 닿을 때 트리거할 sfx
        [Tooltip("땅에 닿을 때 트리거할 sfx")]
		public AudioClip[] TouchTheGroundSfx;

		protected float _movementSpeed;
		protected float _horizontalMovement;
		protected float _verticalMovement;
		protected Vector3 _movementVector;
		protected Vector2 _currentInput = Vector2.zero;
		protected Vector2 _normalizedInput;
		protected Vector2 _lerpedInput = Vector2.zero;
		protected float _acceleration = 0f;
		protected bool _walkParticlesPlaying = false;

		protected const string _speedAnimationParameterName = "Speed";
		protected const string _walkingAnimationParameterName = "Walking";
		protected const string _idleAnimationParameterName = "Idle";
		protected int _speedAnimationParameter;
		protected int _walkingAnimationParameter;
		protected int _idleAnimationParameter;

        /// <summary>
        /// 초기화 시 이동 속도를 WalkSpeed로 설정합니다.
        /// </summary>
        protected override void Initialization()
		{
			base.Initialization ();
			ResetAbility();
		}

        /// <summary>
        /// 캐릭터 이동 상태 및 속도를 재설정합니다
        /// </summary>
        public override void ResetAbility()
        {
	        base.ResetAbility();
            if (gameObject.tag == "Player" && SceneManager.GetActiveScene().name != "LevelSelect2")
            {
                MovementSpeed = PlayerDataManager.GetSpeed();
            }
            else
            {
                MovementSpeed = WalkSpeed;
            }
            //MovementSpeed = WalkSpeed;
            
            if (ContextSpeedStack != null)
			{
				ContextSpeedStack.Clear();
			}
			if ((_movement != null) && (_movement.CurrentState != CharacterStates.MovementStates.FallingDownHole))
			{
				_movement.ChangeState(CharacterStates.MovementStates.Idle);
			}
			MovementSpeedMultiplier = 1f;
			MovementForbidden = false;

			foreach (ParticleSystem system in TouchTheGroundParticles)
			{
				if (system != null)
				{
					system.Stop();
				}				
			}
			foreach (ParticleSystem system in WalkParticles)
			{
				if (system != null)
				{
					system.Stop();
				}				
			}
		}

        /// <summary>
        /// 3개의 패스 중 두 번째 패스는 능력치입니다. Update()라고 생각하시면 됩니다
        /// </summary>
        public override void ProcessAbility()
		{
			base.ProcessAbility();
			
			HandleFrozen();			

            if (!AbilityAuthorized
			    || ((_condition.CurrentState != CharacterStates.CharacterConditions.Normal) && (_condition.CurrentState != CharacterStates.CharacterConditions.ControlledMovement)))
			{
				if (AbilityAuthorized)
				{
					StopAbilityUsedSfx();
				}
				return;
			}
			HandleDirection();
			HandleMovement();

            HandleSlow();
            Feedbacks ();
		}

        /// <summary>
        /// 기능 사이클의 맨 처음에 호출되며, 무시되도록 의도되며, 조건이 충족되면 입력 및 호출 방법을 찾습니다
        /// </summary>
        protected override void HandleInput()
		{
			if (ScriptDrivenInput)
			{
				return;
			}

			if (InputAuthorized)
			{
				_horizontalMovement = _horizontalInput;
				_verticalMovement = _verticalInput;
			}
			else
			{
				_horizontalMovement = 0f;
				_verticalMovement = 0f;
			}	
		}

        /// <summary>
        /// 수평 이동 값을 설정합니다.
        /// </summary>
        /// <param name="value">Horizontal move value, between -1 and 1 - positive : will move to the right, negative : will move left </param>
        public virtual void SetMovement(Vector2 value)
		{
			_horizontalMovement = value.x;
			_verticalMovement = value.y;
		}

        /// <summary>
        /// 이동의 수평 부분을 설정합니다
        /// </summary>
        /// <param name="value"></param>
        public virtual void SetHorizontalMovement(float value)
		{
			_horizontalMovement = value;
		}

        /// <summary>
        /// 이동의 수직 부분을 설정합니다
        /// </summary>
        /// <param name="value"></param>
        public virtual void SetVerticalMovement(float value)
		{
			_verticalMovement = value;
		}

        /// <summary>
        /// 지정된 기간 동안 이동 승수를 적용합니다
        /// </summary>
        /// <param name="movementMultiplier"></param>
        /// <param name="duration"></param>
        public virtual void ApplyMovementMultiplier(float movementMultiplier, float duration)
		{
			StartCoroutine(ApplyMovementMultiplierCo(movementMultiplier, duration));
		}

        /// <summary>
        /// 특정 기간 동안만 이동 승수를 적용하는 데 사용되는 코루틴
        /// </summary>
        /// <param name="movementMultiplier"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        protected virtual IEnumerator ApplyMovementMultiplierCo(float movementMultiplier, float duration)
		{
			if (_characterMovement == null)
			{
				yield break;
			}
			SetContextSpeedMultiplier(movementMultiplier);
			yield return MMCoroutine.WaitFor(duration);
			ResetContextSpeedMultiplier();
		}

        /// <summary>
        /// 새 컨텍스트 속도 승수 스택
        /// </summary>
        /// <param name="newMovementSpeedMultiplier"></param>
        public virtual void SetContextSpeedMultiplier(float newMovementSpeedMultiplier)
		{
			ContextSpeedStack.Push(newMovementSpeedMultiplier);
		}

        /// <summary>
        /// 컨텍스트 속도 승수를 이전 값으로 되돌립니다
        /// </summary>
        public virtual void ResetContextSpeedMultiplier()
		{
			if (ContextSpeedStack.Count <= 0)
			{
				return;
			}
			
			ContextSpeedStack.Pop();
		}

        /// <summary>
        /// 선택한 이동 모드를 고려하여 플레이어 입력을 수정합니다
        /// </summary>
        protected virtual void HandleDirection()
		{
			switch (Movement)
			{
				case Movements.Free:
					// do nothing
					break;
				case Movements.Strict2DirectionsHorizontal:
					_verticalMovement = 0f;
					break;
				case Movements.Strict2DirectionsVertical:
					_horizontalMovement = 0f;
					break;
				case Movements.Strict4Directions:
					if (Mathf.Abs(_horizontalMovement) > Mathf.Abs(_verticalMovement))
					{
						_verticalMovement = 0f;
					}
					else
					{
						_horizontalMovement = 0f;
					}
					break;
				case Movements.Strict8Directions:
					_verticalMovement = Mathf.Round(_verticalMovement);
					_horizontalMovement = Mathf.Round(_horizontalMovement);
					break;
			}
		}

        /// <summary>
        /// Update() 시 호출되며, 수평 이동을 처리합니다
        /// </summary>
        protected virtual void HandleMovement()
		{
            // 우리가 더 이상 걷지 않는다면, 우리는 우리의 걷는 소리를 멈추죠
            if ((_movement.CurrentState != CharacterStates.MovementStates.Walking) && _startFeedbackIsPlaying)
			{
				StopStartFeedbacks();
			}

            // 우리가 더 이상 걷지 않는다면, 우리는 우리의 걷는 소리를 멈추죠
            if (_movement.CurrentState != CharacterStates.MovementStates.Walking && _abilityInProgressSfx != null)
			{
				StopAbilityUsedSfx();
			}

			if (_movement.CurrentState == CharacterStates.MovementStates.Walking && _abilityInProgressSfx == null)
			{
				PlayAbilityUsedSfx();
			}

            // 이동이 차단되거나 캐릭터가 죽거나/frozen/이동이 불가능한 경우, 우리는 종료하고 아무것도 하지 않습니다
            if ( !AbilityAuthorized
			     || (_condition.CurrentState != CharacterStates.CharacterConditions.Normal) )
			{
				return;				
			}
            
			CheckJustGotGrounded();

			if (MovementForbidden)
			{
				_horizontalMovement = 0f;
				_verticalMovement = 0f;
			}

            // 캐릭터가 접지되어 있지 않지만 현재 유휴 상태이거나 보행 중인 경우 상태를 Falling(폴링)으로 변경합니다
            if (!_controller.Grounded
			    && (_condition.CurrentState == CharacterStates.CharacterConditions.Normal)
			    && (
				    (_movement.CurrentState == CharacterStates.MovementStates.Walking)
				    || (_movement.CurrentState == CharacterStates.MovementStates.Idle)
			    ))
			{
				_movement.ChangeState(CharacterStates.MovementStates.Falling);
			}

			if (_controller.Grounded && (_movement.CurrentState == CharacterStates.MovementStates.Falling))
			{
				_movement.ChangeState(CharacterStates.MovementStates.Idle);
			}

			if ( _controller.Grounded
			     && (_controller.CurrentMovement.magnitude > IdleThreshold)
			     && ( _movement.CurrentState == CharacterStates.MovementStates.Idle))
			{
				_movement.ChangeState(CharacterStates.MovementStates.Walking);	
				PlayAbilityStartSfx();	
				PlayAbilityUsedSfx();
				PlayAbilityStartFeedbacks();
			}

            // 만약 우리가 더 이상 움직이지 않고 걷는다면, 우리는 유휴 상태로 돌아갈 것입니다
            if ((_movement.CurrentState == CharacterStates.MovementStates.Walking) 
			    && (_controller.CurrentMovement.magnitude <= IdleThreshold))
			{
				_movement.ChangeState(CharacterStates.MovementStates.Idle);
				PlayAbilityStopSfx();
				PlayAbilityStopFeedbacks();
			}

			if (ShouldSetMovement)
			{
				SetMovement ();	
			}
		}

        /// <summary>
        /// 캐릭터가 동결 상태일 때 발생하는 작업을 설명합니다
        /// </summary>
        protected virtual void HandleFrozen()
		{
			if (!AbilityAuthorized)
			{
				return;
			}
			if (_condition.CurrentState == CharacterStates.CharacterConditions.Frozen)
			{
				_horizontalMovement = 0f;
				_verticalMovement = 0f;
				SetMovement();
			}
		}

        /// <summary>
        /// 캐릭터가 슬로우 상태일 때 발생하는 작업을 설명합니다
        /// </summary>
        protected virtual void HandleSlow()
        {
            if (!AbilityAuthorized)
            {
                return;
            }
            if (_movement.CurrentState == CharacterStates.MovementStates.Slow)
            {
				_horizontalMovement = WalkSpeed * 0.5f;
                _verticalMovement = WalkSpeed * 0.5f;
                SetMovement();
            }
        }

		//캐릭터 컨디션을 슬로우로 변경한다.
		public void CharacterConditionSlow()
		{
            _movement.ChangeState(CharacterStates.MovementStates.Slow);
        }

        //캐릭터 컨디션을 워킹으로 변경한다.
        public void CharacterConditionWalking()
        {
            _movement.ChangeState(CharacterStates.MovementStates.Walking);
        }

        /// <summary>
        /// 컨트롤러를 이동합니다
        /// </summary>
        protected virtual void SetMovement()
		{
			_movementVector = Vector3.zero;
			_currentInput = Vector2.zero;

			_currentInput.x = _horizontalMovement;
			_currentInput.y = _verticalMovement;
            
			_normalizedInput = _currentInput.normalized;

			float interpolationSpeed = 1f;
            
			if ((Acceleration == 0) || (Deceleration == 0))
			{
				_lerpedInput = AnalogInput ? _currentInput : _normalizedInput;
			}
			else
			{
				if (_normalizedInput.magnitude == 0)
				{
					_acceleration = Mathf.Lerp(_acceleration, 0f, Deceleration * Time.deltaTime);
					_lerpedInput = Vector2.Lerp(_lerpedInput, _lerpedInput * _acceleration, Time.deltaTime * Deceleration);
					interpolationSpeed = Deceleration;
				}
				else
				{
					_acceleration = Mathf.Lerp(_acceleration, 1f, Acceleration * Time.deltaTime);
					_lerpedInput = AnalogInput ? Vector2.ClampMagnitude (_currentInput, _acceleration) : Vector2.ClampMagnitude(_normalizedInput, _acceleration);
					interpolationSpeed = Acceleration;
				}
			}		
			
			_movementVector.x = _lerpedInput.x;
			_movementVector.y = 0f;
			_movementVector.z = _lerpedInput.y;

			if (gameObject.tag == "Player")
			{
                Quaternion v3Rotation = Quaternion.Euler(0f, cinemachineVirtualCamera.transform.eulerAngles.y, 0f);

                _movementVector = v3Rotation * _movementVector;
            }

            if (InterpolateMovementSpeed)
			{
				_movementSpeed = Mathf.Lerp(_movementSpeed, MovementSpeed * ContextSpeedMultiplier * MovementSpeedMultiplier, interpolationSpeed * Time.deltaTime);
			}
			else
			{
                _movementSpeed = MovementSpeed * MovementSpeedMultiplier * ContextSpeedMultiplier;
			}

			_movementVector *= _movementSpeed;

			if (_movementVector.magnitude > MovementSpeed * ContextSpeedMultiplier * MovementSpeedMultiplier)
			{
				_movementVector = Vector3.ClampMagnitude(_movementVector, MovementSpeed);
			}

			if ((_currentInput.magnitude <= IdleThreshold) && (_controller.CurrentMovement.magnitude < IdleThreshold))
			{
				_movementVector = Vector3.zero;
            }

			_controller.SetMovement (_movementVector);
        }

        /// <summary>
        /// 매 프레임마다 방금 땅에 닿았는지 확인하고, 그렇다면 상태를 변경하고 입자 효과를 트리거합니다.
        /// </summary>
        protected virtual void CheckJustGotGrounded()
		{
			// if the character just got grounded
			if (_controller.JustGotGrounded)
			{
				_movement.ChangeState(CharacterStates.MovementStates.Idle);
			}
		}

        /// <summary>
        /// 걸을 때는 파티클, 착지할 때는 파티클과 소리를 재생합니다
        /// </summary>
        protected virtual void Feedbacks ()
		{
			if (_controller.Grounded)
			{
				if (_controller.CurrentMovement.magnitude > IdleThreshold)	
				{			
					foreach (ParticleSystem system in WalkParticles)
					{				
						if (!_walkParticlesPlaying && (system != null))
						{
							system.Play();		
						}
						_walkParticlesPlaying = true;
					}	
				}
				else
				{
					foreach (ParticleSystem system in WalkParticles)
					{						
						if (_walkParticlesPlaying && (system != null))
						{
							system.Stop();		
							_walkParticlesPlaying = false;
						}
					}
				}
			}
			else
			{
				foreach (ParticleSystem system in WalkParticles)
				{						
					if (_walkParticlesPlaying && (system != null))
					{
						system.Stop();		
						_walkParticlesPlaying = false;
					}
				}
			}

			if (_controller.JustGotGrounded)
			{
				foreach (ParticleSystem system in TouchTheGroundParticles)
				{
					if (system != null)
					{
						system.Clear();
						system.Play();
					}					
				}
				foreach (AudioClip clip in TouchTheGroundSfx)
				{
					MMSoundManagerSoundPlayEvent.Trigger(clip, MMSoundManager.MMSoundManagerTracks.Sfx, this.transform.position);
				}
			}
		}

        /// <summary>
        /// 이 캐릭터의 속도를 재설정합니다 (달리다가 걸을때 등등)
        /// </summary>
        public virtual void ResetSpeed()
		{
			if (gameObject.tag == "Player")
			{
				MovementSpeed = PlayerDataManager.GetSpeed();
			}
			else
			{
				MovementSpeed = WalkSpeed;
			}
			//MovementSpeed = WalkSpeed;
		}

        /// <summary>
        /// Respawn에서 속도를 재설정합니다
        /// </summary>
        protected override void OnRespawn()
		{
			ResetSpeed();
			MovementForbidden = false;
		}

		protected override void OnDeath()
		{
			base.OnDeath();
			DisableWalkParticles();
		}

        /// <summary>
        /// 재생 중일 가능성이 있는 모든 워크 파티클 시스템을 비활성화합니다
        /// </summary>
        protected virtual void DisableWalkParticles()
		{
			if (WalkParticles.Length > 0)
			{
				foreach (ParticleSystem walkParticle in WalkParticles)
				{
					if (walkParticle != null)
					{
						walkParticle.Stop();
					}
				}
			}
		}

        /// <summary>
        /// 비활성화 시 여전히 재생할 수 있는 모든 것을 꺼야 합니다
        /// </summary>
        protected override void OnDisable()
		{
			base.OnDisable ();
			DisableWalkParticles();
			PlayAbilityStopSfx();
			PlayAbilityStopFeedbacks();
			StopAbilityUsedSfx();
		}

        /// <summary>
        /// 필요한 애니메이터 매개변수가 있는 경우 애니메이터 매개변수 목록에 추가합니다
        /// </summary>
        protected override void InitializeAnimatorParameters()
		{
			RegisterAnimatorParameter (_speedAnimationParameterName, AnimatorControllerParameterType.Float, out _speedAnimationParameter);
			RegisterAnimatorParameter (_walkingAnimationParameterName, AnimatorControllerParameterType.Bool, out _walkingAnimationParameter);
			RegisterAnimatorParameter (_idleAnimationParameterName, AnimatorControllerParameterType.Bool, out _idleAnimationParameter);
		}

        /// <summary>
        /// 현재 속도와 Walking 상태의 현재 값을 애니메이터로 보냅니다
        /// </summary>
        public override void UpdateAnimator()
		{
			MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _speedAnimationParameter, Mathf.Abs(_controller.CurrentMovement.magnitude),_character._animatorParameters, _character.RunAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorBool(_animator, _walkingAnimationParameter, (_movement.CurrentState == CharacterStates.MovementStates.Walking),_character._animatorParameters, _character.RunAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorBool(_animator, _idleAnimationParameter, (_movement.CurrentState == CharacterStates.MovementStates.Idle),_character._animatorParameters, _character.RunAnimatorSanityChecks);
		}
	}
}