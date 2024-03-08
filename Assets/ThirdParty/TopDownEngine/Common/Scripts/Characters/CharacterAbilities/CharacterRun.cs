using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 구성 요소를 캐릭터에 추가하면 실행할 수 있습니다.
    /// Animator parameters : Running
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/Abilities/Character Run")] 
	public class CharacterRun : CharacterAbility
	{
        /// 이 방법은 능력 검사기 시작 부분에 도움말 상자 텍스트를 표시하는 데만 사용됩니다.
        public override string HelpBoxText() { return "이 구성 요소를 사용하면 실행 버튼을 누를 때 캐릭터가 속도(여기에 정의됨)를 변경할 수 있습니다."; }

		[Header("Speed")]

        /// 캐릭터가 달릴 때의 속도
        [Tooltip("캐릭터가 달릴 때의 속도")]
		public float RunSpeed = 16f;

        [Header("AutoRun")]

        /// 조이스틱을 충분히 멀리 움직이면 실행이 자동으로 실행되는지 여부
        [Tooltip("조이스틱을 충분히 멀리 움직이면 실행이 자동으로 실행되는지 여부")]
		public bool AutoRun = false;
        /// 조이스틱의 입력 임계값(정규화됨)
        [Tooltip("조이스틱의 입력 임계값(정규화됨)")]
		public float AutoRunThreshold = 0.6f;

		protected const string _runningAnimationParameterName = "Running";
		protected int _runningAnimationParameter;
		protected bool _runningStarted = false;

        private void Update()
        {
            if(gameObject.tag == "Player")
			{
                if(_movement.CurrentState == CharacterStates.MovementStates.Running)
				{
                    currentStamina -= staminaDecreaseRate * Time.deltaTime;
                    // 스테미나가 다 소진되면 더 이상 달릴 수 없음
                    if (currentStamina <= 0f)
                    {
                        currentStamina = 0f;
						RunStop();
                    }
                }
                else
                {
                    // 휴식 중에는 스테미나가 서서히 회복됨
                    if (currentStamina < maxStamina)
                    {
                        currentStamina += (staminaDecreaseRate / 2f) * Time.deltaTime;
                        // 최대 스테미나를 초과하지 않도록 함
                        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
                    }
                }

				GUIManager.Instance.UpdateStaminaBar(currentStamina, 0f, maxStamina, "PlayerStamina");
            }
        }

        /// <summary>
        /// 각 사이클이 시작될 때 실행 버튼을 눌렀거나 놓았는지 확인합니다.
        /// </summary>
        protected override void HandleInput()
		{
			if (AutoRun)
			{
				if (_inputManager.PrimaryMovement.magnitude > AutoRunThreshold)
				{
					_inputManager.RunButton.State.ChangeState(MMInput.ButtonStates.ButtonPressed);
				}
			}

			if (_inputManager.RunButton.State.CurrentState == MMInput.ButtonStates.ButtonDown || _inputManager.RunButton.State.CurrentState == MMInput.ButtonStates.ButtonPressed)
			{
				RunStart();
			}				
			if (_inputManager.RunButton.State.CurrentState == MMInput.ButtonStates.ButtonUp)
			{
				RunStop();
			}
			else
			{
				if (AutoRun)
				{
					if (_inputManager.PrimaryMovement.magnitude <= AutoRunThreshold)
					{
						_inputManager.RunButton.State.ChangeState(MMInput.ButtonStates.ButtonUp);
						RunStop();
					}
				}
			}          
		}

        /// <summary>
        /// 매 프레임마다 실행 상태에서 벗어나지 않도록 합니다.
        /// </summary>
        public override void ProcessAbility()
		{
			base.ProcessAbility();
			HandleRunningExit();
		}

        /// <summary>
        /// 실행 상태를 종료해야 하는지 확인합니다.
        /// </summary>
        protected virtual void HandleRunningExit()
		{
			if (_condition.CurrentState != CharacterStates.CharacterConditions.Normal)
			{
				StopAbilityUsedSfx();
			}
			if (_movement.CurrentState == CharacterStates.MovementStates.Running && AbilityInProgressSfx != null && _abilityInProgressSfx == null)
			{
				PlayAbilityUsedSfx();
			}
            // 달리고 있지만 접지되지 않은 경우 상태를 Falling으로 변경합니다.
            if (!_controller.Grounded
			    && (_condition.CurrentState == CharacterStates.CharacterConditions.Normal)
			    && (_movement.CurrentState == CharacterStates.MovementStates.Running))
			{
				_movement.ChangeState(CharacterStates.MovementStates.Falling);
				StopFeedbacks();
				StopSfx ();
			}
            // 충분히 빠르게 움직이지 않으면 다시 유휴 상태로 돌아갑니다.
            if ((Mathf.Abs(_controller.CurrentMovement.magnitude) < RunSpeed / 10) && (_movement.CurrentState == CharacterStates.MovementStates.Running))
			{
				_movement.ChangeState (CharacterStates.MovementStates.Idle);
				StopFeedbacks();
				StopSfx ();
			}
			if (!_controller.Grounded && _abilityInProgressSfx != null)
			{
				StopFeedbacks();
				StopSfx ();
			}
		}

        /// <summary>
        /// 캐릭터가 달리기 시작하게 만듭니다.
        /// </summary>
        public virtual void RunStart()
		{
			if ( !AbilityAuthorized // 능력이 허락되지 않는 경우
                 || (!_controller.Grounded) // 아니면 우리가 근거가 없다면
                 || (_condition.CurrentState != CharacterStates.CharacterConditions.Normal) // 아니면 우리가 정상적인 상태가 아닌 경우
                 || (_movement.CurrentState != CharacterStates.MovementStates.Walking) // 아니면 우리가 걷고 있지 않다면
                 || (currentStamina <= 5f)) //아니면 currentStamina가 5이하 이라면
            {
                // 우리는 아무것도 하지 않고 빠져나온다
                return;
			}

            //플레이어가 실행 버튼을 누르고 우리가 땅에 있고 웅크리지 않고 자유롭게 움직일 수 있다면 컨트롤러의 매개변수에서 이동 속도를 변경합니다.
            if (_characterMovement != null)
			{
				if(gameObject.tag == "Player")
				{
                    _characterMovement.MovementSpeed = PlayerDataManager.GetSpeed() * 1.5f;
                }
				else
				{
                    _characterMovement.MovementSpeed = RunSpeed;
                }
			}

            // 아직 실행 중이 아니면 소리를 트리거합니다.
            if (_movement.CurrentState != CharacterStates.MovementStates.Running)
			{
				PlayAbilityStartSfx();
				PlayAbilityUsedSfx();
				PlayAbilityStartFeedbacks();
				_runningStarted = true;
			}

			_movement.ChangeState(CharacterStates.MovementStates.Running);
		}

        /// <summary>
        /// 캐릭터의 달리기를 멈추게 합니다.
        /// </summary>
        public virtual void RunStop()
		{
			if (_runningStarted)
			{
                // 실행 버튼을 놓으면 걷는 속도로 되돌아갑니다.
                if ((_characterMovement != null))
				{
					_characterMovement.ResetSpeed();
					_movement.ChangeState(CharacterStates.MovementStates.Idle);
				}
				StopFeedbacks();
				StopSfx();
				_runningStarted = false;
			}            
		}

        /// <summary>
        /// 모든 실행 피드백을 중지합니다.
        /// </summary>
        protected virtual void StopFeedbacks()
		{
			if (_startFeedbackIsPlaying)
			{
				StopStartFeedbacks();
				PlayAbilityStopFeedbacks();
			}
		}

        /// <summary>
        /// 모든 실행 소리를 중지합니다.
        /// </summary>
        protected virtual void StopSfx()
		{
			StopAbilityUsedSfx();
			PlayAbilityStopSfx();
		}

        /// <summary>
        /// 필요한 애니메이터 매개변수가 있는 경우 애니메이터 매개변수 목록에 필수 애니메이터 매개변수를 추가합니다.
        /// </summary>
        protected override void InitializeAnimatorParameters()
		{
			RegisterAnimatorParameter (_runningAnimationParameterName, AnimatorControllerParameterType.Bool, out _runningAnimationParameter);
		}

        /// <summary>
        /// 각 주기가 끝나면 Running 상태를 캐릭터 애니메이터에게 보냅니다.
        /// </summary>
        public override void UpdateAnimator()
		{
			MMAnimatorExtensions.UpdateAnimatorBool(_animator, _runningAnimationParameter, (_movement.CurrentState == CharacterStates.MovementStates.Running),_character._animatorParameters, _character.RunAnimatorSanityChecks);
		}
	}
}