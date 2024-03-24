using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;
using System;
using Random = UnityEngine.Random;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 클래스는 캐릭터의 TopDownController 구성 요소를 조종합니다.
    /// 이곳은 점프, 돌진, 사격 등과 같은 캐릭터의 모든 게임 규칙을 구현하는 곳입니다.
    /// 애니메이터 매개변수: Grounded(bool), xSpeed(float), ySpeed(float),
    /// CollidingLeft (bool), CollidingRight (bool), CollidingBelow (bool), CollidingAbove (bool), Idle (bool)
    /// Random : 0과 1 사이의 무작위 부동 소수점, 매 프레임 업데이트, 예를 들어 상태 항목 전환에 변화를 추가하는 데 유용합니다.
    /// RandomConstant : 시작 시 생성되고 이 애니메이터의 전체 수명 동안 일정하게 유지되는 임의의 정수(0에서 1000 사이). 동일한 유형의 다른 캐릭터를 갖는 데 유용합니다.
    /// </summary>
    [SelectionBase]
	[AddComponentMenu("TopDown Engine/Character/Core/Character")] 
	public class Character : TopDownMonoBehaviour
	{
		/// the possible initial facing direction for your character
		public enum FacingDirections { West, East, North, South }

		public enum CharacterDimensions { Type2D, Type3D }
		[MMReadOnly]
		public CharacterDimensions CharacterDimension;

		/// the possible character types : player controller or AI (controlled by the computer)
		public enum CharacterTypes { Player, AI }

		[MMInformation("캐릭터 스크립트는 모든 캐릭터 능력의 필수 기반입니다. 귀하의 캐릭터는 AI가 제어하는 ​​비플레이어 캐릭터일 수도 있고, 플레이어가 제어하는 ​​플레이어 캐릭터일 수도 있습니다. 이 경우, PlayerID를 지정해야 하며, 이는 InputManager에 지정된 것과 일치해야 합니다. 일반적으로 'Player1', 'Player2' 등입니다.", MoreMountains.Tools.MMInformationAttribute.InformationType.Info,false)]
        /// 캐릭터는 플레이어가 제어합니까, 아니면 AI가 제어합니까?
        [Tooltip("캐릭터는 플레이어가 제어합니까, 아니면 AI가 제어합니까?")]
		public CharacterTypes CharacterType = CharacterTypes.AI;
        /// 캐릭터가 플레이어가 제어하는 ​​경우에만 사용됩니다. PlayerID는 입력 관리자의 PlayerID와 일치해야 합니다. 또한 Unity의 입력 설정을 일치시키는 데에도 사용됩니다. 따라서 Player1, Player2, Player3 또는 Player4를 유지하면 안전할 것입니다.
        [Tooltip("캐릭터가 플레이어가 제어하는 ​​경우에만 사용됩니다. PlayerID는 입력 관리자의 PlayerID와 일치해야 합니다. 또한 Unity의 입력 설정을 일치시키는 데에도 사용됩니다. 따라서 Player1, Player2, Player3 또는 Player4를 유지하면 안전할 것입니다.")]
		public string PlayerID = "";
		/// the various states of the character
		public CharacterStates CharacterState { get; protected set; }

		[Header("Animator")]
		[MMInformation("엔진은 이 캐릭터에 대한 애니메이터를 찾으려고 노력할 것입니다. 동일한 게임 객체에 있다면 그것을 찾았어야 합니다. 어딘가에 중첩되어 있는 경우 아래에 바인딩해야 합니다. 완전히 제거하기로 결정할 수도 있습니다. 이 경우 '메카님 사용'을 선택 취소하면 됩니다.", MoreMountains.Tools.MMInformationAttribute.InformationType.Info,false)]
		/// the character animator
		[Tooltip("이 클래스와 모든 능력이 매개변수를 업데이트해야 하는 캐릭터 애니메이터")]
		public Animator CharacterAnimator;
        /// 자신만의 애니메이션 시스템을 구현하려면 false로 설정하세요.
        [Tooltip("자신만의 애니메이션 시스템을 구현하려면 false로 설정하세요.")]
		public bool UseDefaultMecanim = true;
        /// 이것이 사실이라면 애니메이터 매개변수를 업데이트하기 전에 존재하는지 확인하기 위해 온전성 검사가 수행됩니다. 이를 false로 설정하면 성능이 향상되지만 존재하지 않는 매개변수를 업데이트하려고 하면 오류가 발생합니다. 애니메이터에 필수 매개변수가 있는지 확인하세요.
        [Tooltip("이것이 사실이라면 애니메이터 매개변수를 업데이트하기 전에 존재하는지 확인하기 위해 온전성 검사가 수행됩니다. 이를 false로 설정하면 성능이 향상되지만 존재하지 않는 매개변수를 업데이트하려고 하면 오류가 발생합니다. 애니메이터에 필수 매개변수가 있는지 확인하세요.")]
		public bool RunAnimatorSanityChecks = false;
        /// 이것이 사실인 경우 잠재적인 스팸을 방지하기 위해 연결된 애니메이터에 대한 애니메이터 로그가 꺼집니다.
        [Tooltip("이것이 사실인 경우 잠재적인 스팸을 방지하기 위해 연결된 애니메이터에 대한 애니메이터 로그가 꺼집니다.")]
		public bool DisableAnimatorLogs = true;

		[Header("Bindings")]
		[MMInformation("이것이 일반 스프라이트 기반 캐릭터이고 SpriteRenderer와 캐릭터가 동일한 GameObject에 있는 경우 이를 바인딩되지 않은 상태로 둡니다. 그렇지 않은 경우 실제 모델을 Character 개체의 부모로 지정하고 아래에 바인딩할 수 있습니다. 이에 대한 예는 3D 데모 캐릭터를 참조하세요. 그 뒤에 있는 아이디어는 모델이 움직이고 뒤집힐 수 있지만 충돌체는 변경되지 않은 상태로 유지된다는 것입니다.", MoreMountains.Tools.MMInformationAttribute.InformationType.Info,false)]
        /// 캐릭터를 조작하는 데 사용되는 '모델'(모든 게임오브젝트일 수 있음)입니다. 이상적으로는 충돌로 인한 혼란을 피하기 위해 충돌체/TopDown 컨트롤러/능력에서 분리(및 중첩)됩니다.
        [Tooltip("캐릭터를 조작하는 데 사용되는 '모델'(모든 게임오브젝트일 수 있음)입니다. 이상적으로는 충돌로 인한 혼란을 피하기 위해 충돌체/TopDown 컨트롤러/능력에서 분리(및 중첩)됩니다.")]
		public GameObject CharacterModel;
        /// 이 캐릭터와 관련된 Health 스크립트는 비어 있으면 자동으로 가져옵니다.
        [Tooltip("이 캐릭터와 관련된 Health 스크립트는 비어 있으면 자동으로 가져옵니다.")]
		public Health CharacterHealth;
        
		[Header("Events")]
		[MMInformation("여기서 상태 변경 시 해당 캐릭터가 이벤트를 트리거하도록 할지 여부를 정의할 수 있습니다. 자세한 내용은 MMTools의 상태 머신 문서를 참조하세요.", MoreMountains.Tools.MMInformationAttribute.InformationType.Info,false)]
        /// 이것이 사실이라면 캐릭터의 상태 머신은 상태에 들어가거나 나갈 때 이벤트를 내보냅니다.
        [Tooltip("이것이 사실이라면 캐릭터의 상태 머신은 상태에 들어가거나 나갈 때 이벤트를 내보냅니다.")]
		public bool SendStateChangeEvents = true;
        
		[Header("Abilities")]
        /// 추가 능력을 검색할 게임 개체 목록(일반적으로 캐릭터 아래에 중첩됨)
        [Tooltip("추가 능력을 검색할 게임 개체 목록(일반적으로 캐릭터 아래에 중첩됨)")]
		public List<GameObject> AdditionalAbilityNodes;
        
		[Header("AI")]
        /// 고급 AI인 경우 현재 이 캐릭터와 연결된 두뇌입니다. 기본적으로 엔진은 이 개체에 있는 개체를 선택하지만 원하는 경우 다른 개체를 연결할 수 있습니다.
        [Tooltip("고급 AI인 경우 현재 이 캐릭터와 연결된 두뇌입니다. 기본적으로 엔진은 이 개체에 있는 개체를 선택하지만 원하는 경우 다른 개체를 연결할 수 있습니다.")]
		public AIBrain CharacterBrain;

        /// 이 캐릭터를 모바일에 최적화할지 여부입니다. 모바일에서 시야 원뿔이 비활성화됩니다.
        [Tooltip("이 캐릭터를 모바일에 최적화할지 여부입니다. 모바일에서 시야 원뿔이 비활성화됩니다.")]
		public bool OptimizeForMobile = true;

		/// State Machines
		public MMStateMachine<CharacterStates.MovementStates> MovementState;
		public MMStateMachine<CharacterStates.CharacterConditions> ConditionState;

        /// 관련 카메라 및 입력 관리자
        public InputManager LinkedInputManager { get; protected set; }
        /// 이 캐릭터와 관련된 애니메이터
        public Animator _animator { get; protected set; }
		/// a list of animator parameters
		public HashSet<int> _animatorParameters { get; set; }
		/// this character's orientation 2D ability
		public CharacterOrientation2D Orientation2D { get; protected set; }
		/// this character's orientation 3D ability
		public CharacterOrientation3D Orientation3D { get; protected set; }
        /// 카메라의 초점으로 사용하고 대상을 따라갈 개체
        public GameObject CameraTarget { get; set; }
		/// the direction of the camera associated to this character
		public Vector3 CameraDirection { get; protected set; }

		protected CharacterAbility[] _characterAbilities;
		protected bool _abilitiesCachedOnce = false;
		protected TopDownController _controller;
		protected float _animatorRandomNumber;
		protected bool _spawnDirectionForced = false;

		protected const string _groundedAnimationParameterName = "Grounded";
		protected const string _aliveAnimationParameterName = "Alive";
		protected const string _currentSpeedAnimationParameterName = "CurrentSpeed";
		protected const string _xSpeedAnimationParameterName = "xSpeed";
		protected const string _ySpeedAnimationParameterName = "ySpeed";
		protected const string _zSpeedAnimationParameterName = "zSpeed";
		protected const string _xVelocityAnimationParameterName = "xVelocity";
		protected const string _yVelocityAnimationParameterName = "yVelocity";
		protected const string _zVelocityAnimationParameterName = "zVelocity";
		protected const string _idleAnimationParameterName = "Idle";
		protected const string _randomAnimationParameterName = "Random";
		protected const string _randomConstantAnimationParameterName = "RandomConstant";
		protected int _groundedAnimationParameter;
		protected int _aliveAnimationParameter;
		protected int _currentSpeedAnimationParameter;
		protected int _xSpeedAnimationParameter;
		protected int _ySpeedAnimationParameter;
		protected int _zSpeedAnimationParameter;
		protected int _xVelocityAnimationParameter;
		protected int _yVelocityAnimationParameter;
		protected int _zVelocityAnimationParameter;
		protected int _idleAnimationParameter;
		protected int _randomAnimationParameter;
		protected int _randomConstantAnimationParameter;
		protected bool _animatorInitialized = false;
		protected CharacterPersistence _characterPersistence;
		protected bool _onReviveRegistered;
		protected Coroutine _conditionChangeCoroutine;
		protected CharacterStates.CharacterConditions _lastState;


		/// <summary>
		/// Initializes this instance of the character
		/// </summary>
		protected virtual void Awake()
		{		
			Initialization();
        }

		/// <summary>
		/// Gets and stores input manager, camera and components
		/// </summary>
		protected virtual void Initialization()
		{            
			if (this.gameObject.MMGetComponentNoAlloc<TopDownController2D>() != null)
			{
				CharacterDimension = CharacterDimensions.Type2D;
			}
			if (this.gameObject.MMGetComponentNoAlloc<TopDownController3D>() != null)
			{
				CharacterDimension = CharacterDimensions.Type3D;
			}

			// we initialize our state machines
			MovementState = new MMStateMachine<CharacterStates.MovementStates>(gameObject,SendStateChangeEvents);
			ConditionState = new MMStateMachine<CharacterStates.CharacterConditions>(gameObject,SendStateChangeEvents);

			// we get the current input manager
			SetInputManager();
			// we store our components for further use 
			CharacterState = new CharacterStates();
			_controller = this.gameObject.GetComponent<TopDownController> ();
			if (CharacterHealth == null)
			{
				CharacterHealth = this.gameObject.GetComponent<Health> ();	
			}

			CacheAbilitiesAtInit();
			if (CharacterBrain == null)
			{
				CharacterBrain = this.gameObject.GetComponent<AIBrain>(); 
			}

			if (CharacterBrain != null)
			{
				CharacterBrain.Owner = this.gameObject;
			}

			Orientation2D = FindAbility<CharacterOrientation2D>();
			Orientation3D = FindAbility<CharacterOrientation3D>();
			_characterPersistence = FindAbility<CharacterPersistence>();

			AssignAnimator();

			// instantiate camera target
			if (CameraTarget == null)
			{
				CameraTarget = new GameObject();
			}            
			CameraTarget.transform.SetParent(this.transform);
			CameraTarget.transform.localPosition = Vector3.zero;
			CameraTarget.name = "CameraTarget";

			if (LinkedInputManager != null)
			{
				if (OptimizeForMobile && LinkedInputManager.IsMobile)
				{
					if (this.gameObject.MMGetComponentNoAlloc<MMConeOfVision2D>() != null)
					{
						this.gameObject.MMGetComponentNoAlloc<MMConeOfVision2D>().enabled = false;
					}
				}
			}            
		}

        /// <summary>
        /// 필요한 경우 능력을 캐시합니다.
        /// </summary>
        protected virtual void CacheAbilitiesAtInit()
		{
			if (_abilitiesCachedOnce)
			{
				return;
			}
			CacheAbilities();
		}
		
		/// <summary>
		/// Grabs abilities and caches them for further use
		/// Make sure you call this if you add abilities at runtime
		/// Ideally you'll want to avoid adding components at runtime, it's costly,
		/// and it's best to activate/disable components instead.
		/// But if you need to, call this method.
		/// </summary>
		public virtual void CacheAbilities()
		{
			// we grab all abilities at our level
			_characterAbilities = this.gameObject.GetComponents<CharacterAbility>();

			// if the user has specified more nodes
			if ((AdditionalAbilityNodes != null) && (AdditionalAbilityNodes.Count > 0))
			{
				// we create a temp list
				List<CharacterAbility> tempAbilityList = new List<CharacterAbility>();

				// we put all the abilities we've already found on the list
				for (int i = 0; i < _characterAbilities.Length; i++)
				{
					tempAbilityList.Add(_characterAbilities[i]);
				}

				// we add the ones from the nodes
				for (int j = 0; j < AdditionalAbilityNodes.Count; j++)
				{
					CharacterAbility[] tempArray = AdditionalAbilityNodes[j].GetComponentsInChildren<CharacterAbility>();
					foreach(CharacterAbility ability in tempArray)
					{
						tempAbilityList.Add(ability);
					}
				}

				_characterAbilities = tempAbilityList.ToArray();
			}
			_abilitiesCachedOnce = true;
		}

		/// <summary>
		/// Forces the (re)initialization of the character's abilities
		/// </summary>
		public virtual void ForceAbilitiesInitialization()
		{
			for (int i = 0; i < _characterAbilities.Length; i++)
			{
				_characterAbilities[i].ForceInitialization();
			}
			for (int j = 0; j < AdditionalAbilityNodes.Count; j++)
			{
				CharacterAbility[] tempArray = AdditionalAbilityNodes[j].GetComponentsInChildren<CharacterAbility>();
				foreach(CharacterAbility ability in tempArray)
				{
					ability.ForceInitialization();
				}
			}
		}

        /// <summary>
        /// 캐릭터가 특정 능력을 가지고 있는지 확인하는 방법
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T FindAbility<T>() where T:CharacterAbility
		{
			CacheAbilitiesAtInit();

			Type searchedAbilityType = typeof(T);
            
			foreach (CharacterAbility ability in _characterAbilities)
			{
				if (ability is T characterAbility)
				{
					return characterAbility;
				}
			}

			return null;
		}

		/// <summary>
		/// A method to check whether a Character has a certain ability or not
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public CharacterAbility FindAbilityByString(string abilityName)
		{
			CacheAbilitiesAtInit();
            
			foreach (CharacterAbility ability in _characterAbilities)
			{
				if (ability.GetType().Name == abilityName)
				{
					return ability;
				}
			}

			return null;
		}
		
		/// <summary>
		/// A method to check whether a Character has a certain ability or not
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public List<T> FindAbilities<T>() where T:CharacterAbility
		{
			CacheAbilitiesAtInit();

			List<T> resultList = new List<T>();
			Type searchedAbilityType = typeof(T);

			foreach (CharacterAbility ability in _characterAbilities)
			{
				if (ability is T characterAbility)
				{
					resultList.Add(characterAbility);
				}
			}

			return resultList;
		}

		/// <summary>
		/// Binds an animator to this character
		/// </summary>
		public virtual void AssignAnimator(bool forceAssignation = false)
		{
			if (_animatorInitialized && !forceAssignation)
			{
				return;
			}
            
			_animatorParameters = new HashSet<int>();

			if (CharacterAnimator != null)
			{
				_animator = CharacterAnimator;
			}
			else
			{
				_animator = this.gameObject.GetComponent<Animator>();
			}

			if (_animator != null)
			{
				if (DisableAnimatorLogs)
				{
					_animator.logWarnings = false;
				}
				InitializeAnimatorParameters();
			}

			_animatorInitialized = true;
		}

		/// <summary>
		/// Initializes the animator parameters.
		/// </summary>
		protected virtual void InitializeAnimatorParameters()
		{
			if (_animator == null) { return; }
			MMAnimatorExtensions.AddAnimatorParameterIfExists(_animator, _groundedAnimationParameterName, out _groundedAnimationParameter, AnimatorControllerParameterType.Bool, _animatorParameters);
			MMAnimatorExtensions.AddAnimatorParameterIfExists(_animator, _currentSpeedAnimationParameterName, out _currentSpeedAnimationParameter, AnimatorControllerParameterType.Float, _animatorParameters);
			MMAnimatorExtensions.AddAnimatorParameterIfExists(_animator, _xSpeedAnimationParameterName, out _xSpeedAnimationParameter, AnimatorControllerParameterType.Float, _animatorParameters);
			MMAnimatorExtensions.AddAnimatorParameterIfExists(_animator, _ySpeedAnimationParameterName, out _ySpeedAnimationParameter, AnimatorControllerParameterType.Float, _animatorParameters);
			MMAnimatorExtensions.AddAnimatorParameterIfExists(_animator, _zSpeedAnimationParameterName, out _zSpeedAnimationParameter, AnimatorControllerParameterType.Float, _animatorParameters);
			MMAnimatorExtensions.AddAnimatorParameterIfExists(_animator, _idleAnimationParameterName, out _idleAnimationParameter, AnimatorControllerParameterType.Bool, _animatorParameters);
			MMAnimatorExtensions.AddAnimatorParameterIfExists(_animator, _aliveAnimationParameterName, out _aliveAnimationParameter, AnimatorControllerParameterType.Bool, _animatorParameters);
			MMAnimatorExtensions.AddAnimatorParameterIfExists(_animator, _randomAnimationParameterName, out _randomAnimationParameter, AnimatorControllerParameterType.Float, _animatorParameters);
			MMAnimatorExtensions.AddAnimatorParameterIfExists(_animator, _randomConstantAnimationParameterName, out _randomConstantAnimationParameter, AnimatorControllerParameterType.Float, _animatorParameters);
			MMAnimatorExtensions.AddAnimatorParameterIfExists(_animator, _xVelocityAnimationParameterName, out _xVelocityAnimationParameter, AnimatorControllerParameterType.Float, _animatorParameters);
			MMAnimatorExtensions.AddAnimatorParameterIfExists(_animator, _yVelocityAnimationParameterName, out _yVelocityAnimationParameter, AnimatorControllerParameterType.Float, _animatorParameters);
			MMAnimatorExtensions.AddAnimatorParameterIfExists(_animator, _zVelocityAnimationParameterName, out _zVelocityAnimationParameter, AnimatorControllerParameterType.Float, _animatorParameters);
            
			// we update our constant float animation parameter
			int randomConstant = UnityEngine.Random.Range(0, 1000);
			MMAnimatorExtensions.UpdateAnimatorInteger(_animator, _randomConstantAnimationParameter, randomConstant, _animatorParameters, RunAnimatorSanityChecks);
		}

		/// <summary>
		/// Gets (if it exists) the InputManager matching the Character's Player ID
		/// </summary>
		public virtual void SetInputManager()
		{
			if (CharacterType == CharacterTypes.AI)
			{
				LinkedInputManager = null;
				UpdateInputManagersInAbilities();
				return;
			}

			// we get the corresponding input manager
			if (!string.IsNullOrEmpty(PlayerID))
			{
				LinkedInputManager = null;
				InputManager[] foundInputManagers = FindObjectsOfType(typeof(InputManager)) as InputManager[];
				foreach (InputManager foundInputManager in foundInputManagers)
				{
					if (foundInputManager.PlayerID == PlayerID)
					{
						LinkedInputManager = foundInputManager;
					}
				}
			}
			UpdateInputManagersInAbilities();
		}

		/// <summary>
		/// Sets a new input manager for this Character and all its abilities
		/// </summary>
		/// <param name="inputManager"></param>
		public virtual void SetInputManager(InputManager inputManager)
		{
			LinkedInputManager = inputManager;
			UpdateInputManagersInAbilities();
		}

		/// <summary>
		/// Updates the linked input manager for all abilities
		/// </summary>
		protected virtual void UpdateInputManagersInAbilities()
		{
			if (_characterAbilities == null)
			{
				return;
			}
			for (int i = 0; i < _characterAbilities.Length; i++)
			{
				_characterAbilities[i].SetInputManager(LinkedInputManager);
			}
		}

		/// <summary>
		/// Resets the input for all abilities
		/// </summary>
		public virtual void ResetInput()
		{
			if (_characterAbilities == null)
			{
				return;
			}
			foreach (CharacterAbility ability in _characterAbilities)
			{
				ability.ResetInput();
			}
		}

		/// <summary>
		/// Sets the player ID
		/// </summary>
		/// <param name="newPlayerID">New player ID.</param>
		public virtual void SetPlayerID(string newPlayerID)
		{
			PlayerID = newPlayerID;
			SetInputManager();
		}
		
		/// <summary>
		/// This is called every frame.
		/// </summary>
		protected virtual void Update()
		{		
			//케릭터 업테이트가 먼저 실행되고 인풋메니저가 다음에 실행되게하면 된다.
			EveryFrame();
        }

        /// <summary>
        /// 우리는 매 프레임마다 이 작업을 수행합니다. 이는 유연성을 높이기 위해 업데이트와 별개입니다.
        /// </summary>
        protected virtual void EveryFrame()
		{
            // 우리는 우리의 능력을 처리합니다
            EarlyProcessAbilities();
			ProcessAbilities();
			LateProcessAbilities();

            // 다양한 상태를 애니메이터에게 보냅니다. 
            UpdateAnimators();
		}

        /// <summary>
        /// 등록된 모든 능력의 EarlyProcess 메소드를 호출합니다.
        /// </summary>
        protected virtual void EarlyProcessAbilities()
		{
            foreach (CharacterAbility ability in _characterAbilities)
			{
				if (ability.enabled && ability.AbilityInitialized)
				{
                    ability.EarlyProcessAbility();
				}
			}
		}

        /// <summary>
        /// 등록된 모든 능력의 처리 방법을 호출합니다.
        /// </summary>
        protected virtual void ProcessAbilities()
		{
			foreach (CharacterAbility ability in _characterAbilities)
			{
				if (ability.enabled && ability.AbilityInitialized)
				{
					ability.ProcessAbility();
				}
			}
		}

        /// <summary>
        /// 등록된 모든 능력의 Late Process 메소드를 호출합니다.
        /// </summary>
        protected virtual void LateProcessAbilities()
		{
			foreach (CharacterAbility ability in _characterAbilities)
			{
				if (ability.enabled && ability.AbilityInitialized)
				{
					ability.LateProcessAbility();
				}
			}
		}


        /// <summary>
        /// 이는 Update()에서 호출되며 각 애니메이터 매개변수를 해당 상태 값으로 설정합니다.
        /// </summary>
        protected virtual void UpdateAnimators()
		{
			UpdateAnimationRandomNumber();

			if ((UseDefaultMecanim) && (_animator!= null))
			{
				MMAnimatorExtensions.UpdateAnimatorBool(_animator, _groundedAnimationParameter, _controller.Grounded,_animatorParameters, RunAnimatorSanityChecks);
				MMAnimatorExtensions.UpdateAnimatorBool(_animator, _aliveAnimationParameter, (ConditionState.CurrentState != CharacterStates.CharacterConditions.Dead),_animatorParameters, RunAnimatorSanityChecks);
				MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _currentSpeedAnimationParameter, _controller.CurrentMovement.magnitude, _animatorParameters, RunAnimatorSanityChecks);
				MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _xSpeedAnimationParameter, _controller.CurrentMovement.x,_animatorParameters, RunAnimatorSanityChecks);
				MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _ySpeedAnimationParameter, _controller.CurrentMovement.y,_animatorParameters, RunAnimatorSanityChecks);
				MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _zSpeedAnimationParameter, _controller.CurrentMovement.z,_animatorParameters, RunAnimatorSanityChecks);
				MMAnimatorExtensions.UpdateAnimatorBool(_animator, _idleAnimationParameter,(MovementState.CurrentState == CharacterStates.MovementStates.Idle),_animatorParameters, RunAnimatorSanityChecks);
				MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _randomAnimationParameter, _animatorRandomNumber, _animatorParameters, RunAnimatorSanityChecks);
				MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _xVelocityAnimationParameter, _controller.Velocity.x, _animatorParameters, RunAnimatorSanityChecks);
				MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _yVelocityAnimationParameter, _controller.Velocity.y, _animatorParameters, RunAnimatorSanityChecks);
				MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _zVelocityAnimationParameter, _controller.Velocity.z, _animatorParameters, RunAnimatorSanityChecks);


				foreach (CharacterAbility ability in _characterAbilities)
				{
					if (ability.enabled && ability.AbilityInitialized)
					{	
						ability.UpdateAnimator();
					}
				}
			}
		}
		
		public virtual void RespawnAt(Vector3 spawnPosition, FacingDirections facingDirection)
		{
			transform.position = spawnPosition;
			
			if (!gameObject.activeInHierarchy)
			{
				gameObject.SetActive(true);
				//Debug.LogError("Spawn : your Character's gameobject is inactive");
			}

			// we raise it from the dead (if it was dead)
			ConditionState.ChangeState(CharacterStates.CharacterConditions.Normal);
			// we re-enable its 2D collider
			if (this.gameObject.MMGetComponentNoAlloc<Collider2D>() != null)
			{
				this.gameObject.MMGetComponentNoAlloc<Collider2D>().enabled = true;
			}
			// we re-enable its 3D collider
			if (this.gameObject.MMGetComponentNoAlloc<Collider>() != null)
			{
				this.gameObject.MMGetComponentNoAlloc<Collider>().enabled = true;
			}

			// we make it handle collisions again
			_controller.enabled = true;
			_controller.CollisionsOn();
			_controller.Reset();

			// we kill all potential velocity
			if (this.gameObject.MMGetComponentNoAlloc<Rigidbody2D>() != null)
			{
				this.gameObject.MMGetComponentNoAlloc<Rigidbody2D>().velocity = Vector3.zero;
			}

			Reset();
			UnFreeze();

			if (CharacterHealth != null)
			{
				CharacterHealth.StoreInitialPosition();
				if (_characterPersistence != null)
				{
					if (_characterPersistence.Initialized)
					{
						if (CharacterHealth != null)
						{
							CharacterHealth.UpdateHealthBar(false);
						}
						return;
					}
				}
				CharacterHealth.ResetHealthToMaxHealth();
				CharacterHealth.Revive();
			}

			if (CharacterBrain != null)
			{
				CharacterBrain.enabled = true;
			}

			// facing direction
			if (FindAbility<CharacterOrientation2D>() != null)
			{
				FindAbility<CharacterOrientation2D>().InitialFacingDirection = facingDirection;
				FindAbility<CharacterOrientation2D>().Face(facingDirection);
			}
			// facing direction
			if (FindAbility<CharacterOrientation3D>() != null)
			{
				FindAbility<CharacterOrientation3D>().Face(facingDirection); 
			}
		}

		/// <summary>
		/// Makes the player respawn at the location passed in parameters
		/// </summary>
		/// <param name="spawnPoint">The location of the respawn.</param>
		public virtual void RespawnAt(Transform spawnPoint, FacingDirections facingDirection)
		{
			RespawnAt(spawnPoint.position, facingDirection);
		}

		/// <summary>
		/// Calls flip on all abilities
		/// </summary>
		public virtual void FlipAllAbilities()
		{
			foreach (CharacterAbility ability in _characterAbilities)
			{
				if (ability.enabled)
				{
					ability.Flip();
				}
			}
		}

		/// <summary>
		/// Generates a random number to send to the animator
		/// </summary>
		protected virtual void UpdateAnimationRandomNumber()
		{
			_animatorRandomNumber = Random.Range(0f, 1f);
		}

		/// <summary>
		/// Use this method to change the character's condition for a specified duration, and resetting it afterwards.
		/// You can also use this to disable gravity for a while, and optionally reset forces too.
		/// </summary>
		/// <param name="newCondition"></param>
		/// <param name="duration"></param>
		/// <param name="resetControllerForces"></param>
		/// <param name="disableGravity"></param>
		public virtual void ChangeCharacterConditionTemporarily(CharacterStates.CharacterConditions newCondition,
			float duration, bool resetControllerForces, bool disableGravity)
		{
			if (_conditionChangeCoroutine != null)
			{
				StopCoroutine(_conditionChangeCoroutine);
			}
			_conditionChangeCoroutine = StartCoroutine(ChangeCharacterConditionTemporarilyCo(newCondition, duration, resetControllerForces, disableGravity));
		}

        /// <summary>
        /// ChangeCharacterConditionTemporarily에 의해 명령된 임시 조건 변경을 처리하는 코루틴
        /// </summary>
        /// <param name="newCondition"></param>
        /// <param name="duration"></param>
        /// <param name="resetControllerForces"></param>
        /// <param name="disableGravity"></param>
        /// <returns></returns>
        protected virtual IEnumerator ChangeCharacterConditionTemporarilyCo(
			CharacterStates.CharacterConditions newCondition,
			float duration, bool resetControllerForces, bool disableGravity)
		{
			if (_lastState != newCondition) if ((_lastState != newCondition) && (this.ConditionState.CurrentState != newCondition))
			{
				_lastState = this.ConditionState.CurrentState;
			}
			
			this.ConditionState.ChangeState(newCondition);
			if (resetControllerForces) { _controller?.SetMovement(Vector2.zero); }
			if (disableGravity && (_controller != null)) { _controller.GravityActive = false; }
			yield return MMCoroutine.WaitFor(duration);
			this.ConditionState.ChangeState(_lastState);
			if (disableGravity && (_controller != null)) { _controller.GravityActive = true; }
		}

		/// <summary>
		/// Stores the associated camera direction
		/// </summary>
		public virtual void SetCameraDirection(Vector3 direction)
		{
			CameraDirection = direction;
		}

		/// <summary>
		/// Freezes this character.
		/// </summary>
		public virtual void Freeze()
		{
			_controller.SetGravityActive(false);
			_controller.SetMovement(Vector2.zero);
			ConditionState.ChangeState(CharacterStates.CharacterConditions.Frozen);
		}

		/// <summary>
		/// Unfreezes this character
		/// </summary>
		public virtual void UnFreeze()
		{
			_controller.SetGravityActive(true);
			ConditionState.ChangeState(CharacterStates.CharacterConditions.Normal);
		}

		/// <summary>
		/// Called to disable the player (at the end of a level for example. 
		/// It won't move and respond to input after this.
		/// </summary>
		public virtual void Disable()
		{
			this.enabled = false;
			_controller.enabled = false;			
		}
        
		/// <summary>
		/// Called when the Character dies. 
		/// Calls every abilities' Reset() method, so you can restore settings to their original value if needed
		/// </summary>
		public virtual void Reset()
		{
			_spawnDirectionForced = false;
			if (_characterAbilities == null)
			{
				return;
			}
			if (_characterAbilities.Length == 0)
			{
				return;
			}
			foreach (CharacterAbility ability in _characterAbilities)
			{
				if (ability.enabled)
				{
					ability.ResetAbility();
				}
			}
		}

		/// <summary>
		/// On revive, we force the spawn direction
		/// </summary>
		protected virtual void OnRevive()
		{
			if (CharacterBrain != null)
			{
				CharacterBrain.enabled = true;
				CharacterBrain.ResetBrain();
			}
		}

		protected virtual void OnDeath()
		{
			if (CharacterBrain != null)
			{
				CharacterBrain.TransitionToState("");
				CharacterBrain.enabled = false;
			}
			if (MovementState.CurrentState != CharacterStates.MovementStates.FallingDownHole)
			{
				MovementState.ChangeState(CharacterStates.MovementStates.Idle);
			}            
		}

		protected virtual void OnHit()
		{

		}

		/// <summary>
		/// OnEnable, we register our OnRevive event
		/// </summary>
		protected virtual void OnEnable ()
		{
			if (CharacterHealth != null)
			{
				if (!_onReviveRegistered)
				{
					CharacterHealth.OnRevive += OnRevive;
					_onReviveRegistered = true;
				}
				CharacterHealth.OnDeath += OnDeath;
				CharacterHealth.OnHit += OnHit;
			}
		}

		/// <summary>
		/// OnDisable, we unregister our OnRevive event
		/// </summary>
		protected virtual void OnDisable()
		{
			if (CharacterHealth != null)
			{
				CharacterHealth.OnDeath -= OnDeath;
				CharacterHealth.OnHit -= OnHit;
			}			
		}
    }
}