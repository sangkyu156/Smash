using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 3D�� ��� x �� z ����, 2D�� ��� x �� y �������� ���� �̵�(�ȱ�, ���������� �ٱ�, ���ٴϱ� ��)�� ó���ϵ��� ĳ���Ϳ� �� ����� �߰��ϼ���.
    /// �ִϸ����� �Ű�����: �ӵ�(float), �ȱ�(bool)
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/Abilities/Character Movement")] 
	public class CharacterMovement : CharacterAbility 
	{
        /// ĳ���Ϳ� ������ ȸ�� ���
        public enum Movements { Free, Strict2DirectionsHorizontal, Strict2DirectionsVertical, Strict4Directions, Strict8Directions }


		/// ���� ���� �̵� �ӵ�
		[SerializeField]
		public float MovementSpeed { get; set; }
        /// �̰��� ����̶�� �̵��� �����˴ϴ�(������ ����).
        public bool MovementForbidden { get; set; }

		[Header("Direction")]

        /// ĳ���Ͱ� 2D������ 4���� �Ǵ� 8�������� �����Ӱ� ������ �� �ִ��� ����
        [Tooltip("ĳ���Ͱ� 2D������ 4���� �Ǵ� 8�������� �����Ӱ� ������ �� �ִ��� ����")]
		public Movements Movement = Movements.Free;

		[Header("Settings")]

        /// �ش� ������ ������ �Է��� ���εǾ����� ����
        [Tooltip("�ش� ������ ������ �Է��� ���εǾ����� ����")]
		public bool InputAuthorized = true;
        /// �Է��� �Ƴ��α׿��� �ϴ��� ����
        [Tooltip("�Է��� �Ƴ��α׿��� �ϴ��� ����")]
		public bool AnalogInput = false;
        /// �ٸ� ��ũ��Ʈ���� �Է��� �����ؾ� �ϴ��� ����
        [Tooltip("�ٸ� ��ũ��Ʈ���� �Է��� �����ؾ� �ϴ��� ����")]
		public bool ScriptDrivenInput = false;

		[Header("Speed")]

        /// ĳ���Ͱ� ���� ���� �ӵ�
        [Tooltip("ĳ���Ͱ� ���� ���� �ӵ�")]
		public float WalkSpeed = 6f;
        /// �� ������Ұ� ��Ʈ�ѷ��� �������� �����ؾ� �ϴ��� ����
        [Tooltip("�� ������Ұ� ��Ʈ�ѷ��� �������� �����ؾ� �ϴ��� ����")]
		public bool ShouldSetMovement = true;
        /// ĳ���Ͱ� �� �̻� ���� ���·� ���ֵ��� �ʴ� �ӵ� �Ӱ谪
        [Tooltip("ĳ���Ͱ� �� �̻� ���� ���·� ���ֵ��� �ʴ� �ӵ� �Ӱ谪")]
		public float IdleThreshold = 0.05f;

        [Header("Acceleration")]

        /// ���� �ӵ��� ������ ���ӵ� / 0f : ���ӵ� ����, ���� �ְ� �ӵ�
        [Tooltip("���� �ӵ��� ������ ���ӵ� / 0f : ���ӵ� ����, ���� �ְ� �ӵ�")]
		public float Acceleration = 10f;
        /// ���� �ӵ��� ������ ���� / 0f : ���� ����, ��� ����
        [Tooltip("���� �ӵ��� ������ ���� / 0f : ���� ����, ��� ����")]
		public float Deceleration = 10f;

        /// �̵� �ӵ��� �������� ����
        [Tooltip("�̵� �ӵ��� �������� ����")]
		public bool InterpolateMovementSpeed = false;
		public float MovementSpeedMaxMultiplier { get; set; } = float.MaxValue;
		private float _movementSpeedMultiplier;
        /// ���� ��� ������ �¼�
        public float MovementSpeedMultiplier
		{
			get => Mathf.Min(_movementSpeedMultiplier, MovementSpeedMaxMultiplier);
			set => _movementSpeedMultiplier = value;
		}
        /// ���ؽ�Ʈ ���(movement ���� ��)�� ���� ����Ǵ� ���� �̵��� ������ �¼�
        public Stack<float> ContextSpeedStack = new Stack<float>();
		public float ContextSpeedMultiplier => ContextSpeedStack.Count > 0 ? ContextSpeedStack.Peek() : 1;

		[Header("��ũ �ǵ��")]
        /// �ȴ� ���� ������ ���ڵ�
        [Tooltip("�ȴ� ���� ������ ���ڵ�")]
		public ParticleSystem[] WalkParticles;

		[Header("��ġ �� �׶��� �ǵ��")]
        /// ���� ���� �� ������ ���ڵ�
        [Tooltip("���� ���� �� ������ ���ڵ�")]
		public ParticleSystem[] TouchTheGroundParticles;
        /// ���� ���� �� Ʈ������ sfx
        [Tooltip("���� ���� �� Ʈ������ sfx")]
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
        /// �ʱ�ȭ �� �̵� �ӵ��� WalkSpeed�� �����մϴ�.
        /// </summary>
        protected override void Initialization()
		{
			base.Initialization ();
			ResetAbility();
		}

        /// <summary>
        /// ĳ���� �̵� ���� �� �ӵ��� �缳���մϴ�
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
        /// 3���� �н� �� �� ��° �н��� �ɷ�ġ�Դϴ�. Update()��� �����Ͻø� �˴ϴ�
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
        /// ��� ����Ŭ�� �� ó���� ȣ��Ǹ�, ���õǵ��� �ǵ��Ǹ�, ������ �����Ǹ� �Է� �� ȣ�� ����� ã���ϴ�
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
        /// ���� �̵� ���� �����մϴ�.
        /// </summary>
        /// <param name="value">Horizontal move value, between -1 and 1 - positive : will move to the right, negative : will move left </param>
        public virtual void SetMovement(Vector2 value)
		{
			_horizontalMovement = value.x;
			_verticalMovement = value.y;
		}

        /// <summary>
        /// �̵��� ���� �κ��� �����մϴ�
        /// </summary>
        /// <param name="value"></param>
        public virtual void SetHorizontalMovement(float value)
		{
			_horizontalMovement = value;
		}

        /// <summary>
        /// �̵��� ���� �κ��� �����մϴ�
        /// </summary>
        /// <param name="value"></param>
        public virtual void SetVerticalMovement(float value)
		{
			_verticalMovement = value;
		}

        /// <summary>
        /// ������ �Ⱓ ���� �̵� �¼��� �����մϴ�
        /// </summary>
        /// <param name="movementMultiplier"></param>
        /// <param name="duration"></param>
        public virtual void ApplyMovementMultiplier(float movementMultiplier, float duration)
		{
			StartCoroutine(ApplyMovementMultiplierCo(movementMultiplier, duration));
		}

        /// <summary>
        /// Ư�� �Ⱓ ���ȸ� �̵� �¼��� �����ϴ� �� ���Ǵ� �ڷ�ƾ
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
        /// �� ���ؽ�Ʈ �ӵ� �¼� ����
        /// </summary>
        /// <param name="newMovementSpeedMultiplier"></param>
        public virtual void SetContextSpeedMultiplier(float newMovementSpeedMultiplier)
		{
			ContextSpeedStack.Push(newMovementSpeedMultiplier);
		}

        /// <summary>
        /// ���ؽ�Ʈ �ӵ� �¼��� ���� ������ �ǵ����ϴ�
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
        /// ������ �̵� ��带 ����Ͽ� �÷��̾� �Է��� �����մϴ�
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
        /// Update() �� ȣ��Ǹ�, ���� �̵��� ó���մϴ�
        /// </summary>
        protected virtual void HandleMovement()
		{
            // �츮�� �� �̻� ���� �ʴ´ٸ�, �츮�� �츮�� �ȴ� �Ҹ��� ������
            if ((_movement.CurrentState != CharacterStates.MovementStates.Walking) && _startFeedbackIsPlaying)
			{
				StopStartFeedbacks();
			}

            // �츮�� �� �̻� ���� �ʴ´ٸ�, �츮�� �츮�� �ȴ� �Ҹ��� ������
            if (_movement.CurrentState != CharacterStates.MovementStates.Walking && _abilityInProgressSfx != null)
			{
				StopAbilityUsedSfx();
			}

			if (_movement.CurrentState == CharacterStates.MovementStates.Walking && _abilityInProgressSfx == null)
			{
				PlayAbilityUsedSfx();
			}

            // �̵��� ���ܵǰų� ĳ���Ͱ� �װų�/frozen/�̵��� �Ұ����� ���, �츮�� �����ϰ� �ƹ��͵� ���� �ʽ��ϴ�
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

            // ĳ���Ͱ� �����Ǿ� ���� ������ ���� ���� �����̰ų� ���� ���� ��� ���¸� Falling(����)���� �����մϴ�
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

            // ���� �츮�� �� �̻� �������� �ʰ� �ȴ´ٸ�, �츮�� ���� ���·� ���ư� ���Դϴ�
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
        /// ĳ���Ͱ� ���� ������ �� �߻��ϴ� �۾��� �����մϴ�
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
        /// ĳ���Ͱ� ���ο� ������ �� �߻��ϴ� �۾��� �����մϴ�
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

		//ĳ���� ������� ���ο�� �����Ѵ�.
		public void CharacterConditionSlow()
		{
            _movement.ChangeState(CharacterStates.MovementStates.Slow);
        }

        //ĳ���� ������� ��ŷ���� �����Ѵ�.
        public void CharacterConditionWalking()
        {
            _movement.ChangeState(CharacterStates.MovementStates.Walking);
        }

        /// <summary>
        /// ��Ʈ�ѷ��� �̵��մϴ�
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
        /// �� �����Ӹ��� ��� ���� ��Ҵ��� Ȯ���ϰ�, �׷��ٸ� ���¸� �����ϰ� ���� ȿ���� Ʈ�����մϴ�.
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
        /// ���� ���� ��ƼŬ, ������ ���� ��ƼŬ�� �Ҹ��� ����մϴ�
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
        /// �� ĳ������ �ӵ��� �缳���մϴ� (�޸��ٰ� ������ ���)
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
        /// Respawn���� �ӵ��� �缳���մϴ�
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
        /// ��� ���� ���ɼ��� �ִ� ��� ��ũ ��ƼŬ �ý����� ��Ȱ��ȭ�մϴ�
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
        /// ��Ȱ��ȭ �� ������ ����� �� �ִ� ��� ���� ���� �մϴ�
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
        /// �ʿ��� �ִϸ����� �Ű������� �ִ� ��� �ִϸ����� �Ű����� ��Ͽ� �߰��մϴ�
        /// </summary>
        protected override void InitializeAnimatorParameters()
		{
			RegisterAnimatorParameter (_speedAnimationParameterName, AnimatorControllerParameterType.Float, out _speedAnimationParameter);
			RegisterAnimatorParameter (_walkingAnimationParameterName, AnimatorControllerParameterType.Bool, out _walkingAnimationParameter);
			RegisterAnimatorParameter (_idleAnimationParameterName, AnimatorControllerParameterType.Bool, out _idleAnimationParameter);
		}

        /// <summary>
        /// ���� �ӵ��� Walking ������ ���� ���� �ִϸ����ͷ� �����ϴ�
        /// </summary>
        public override void UpdateAnimator()
		{
			MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _speedAnimationParameter, Mathf.Abs(_controller.CurrentMovement.magnitude),_character._animatorParameters, _character.RunAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorBool(_animator, _walkingAnimationParameter, (_movement.CurrentState == CharacterStates.MovementStates.Walking),_character._animatorParameters, _character.RunAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorBool(_animator, _idleAnimationParameter, (_movement.CurrentState == CharacterStates.MovementStates.Idle),_character._animatorParameters, _character.RunAnimatorSanityChecks);
		}
	}
}