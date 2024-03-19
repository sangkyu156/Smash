using UnityEngine;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;
using Random = UnityEngine.Random;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 예를 들어 Megaman에 나타나는 플랫폼과 같이 나타나거나 사라지는 루프에서 실행되도록 하려면 이 클래스를 개체(일반적으로 플랫폼이지만 실제로는 무엇이든 될 수 있음)에 추가합니다.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Environment/Appear and Disappear")]
	public class AppearDisappear : TopDownMonoBehaviour
	{
		/// the possible states this object can be in
		public enum AppearDisappearStates { Visible, Hidden, VisibleToHidden, HiddenToVisible }
		/// the possible start modes (automatic will start on Start, PlayerContact when an object with the Player tag collides with the object, and Script lets you trigger that manually
		public enum StartModes { Automatic, PlayerContact, Script }

		public enum CyclingModes { Forever, Limited}
        
		[Header("Settings")]

        /// 개체가 지금 활성화되어 있는지 여부
        [Tooltip("개체가 지금 활성화되어 있는지 여부")]
		public bool Active = true;
        /// 객체가 시작되어야 하는 초기 상태(표시 또는 숨김)
        [Tooltip("객체가 시작되어야 하는 초기 상태(표시 또는 숨김)")]
		public AppearDisappearStates InitialState;
        /// 객체를 활성화하는 방법
        [Tooltip("객체를 활성화하는 방법")]
		public StartModes StartMode = StartModes.Automatic;
        /// 객체가 상태를 순환하는 방법(영원히, 제한된 횟수 또는 전혀)
        [Tooltip("객체가 상태를 순환하는 방법(영원히, 제한된 횟수 또는 전혀)")]
		public CyclingModes CyclingMode = CyclingModes.Forever;
        /// 이 개체가 중지되기 전에 통과할 수 있는 주기 수( CyclingMode가 Limited인 경우에만 사용됨)
        [Tooltip("이 개체가 중지되기 전에 통과할 수 있는 주기 수( CyclingMode가 Limited인 경우에만 사용됨)")]
		[MMEnumCondition("CyclingMode", (int)CyclingModes.Limited)]
		public int CyclesAmount = 1;


		[Header("Timing")]

        /// 객체의 첫 번째 상태 변경에 적용할 초기 오프셋(초)
        [MMVector("Min", "Max")]
		[Tooltip("객체의 첫 번째 상태 변경에 적용할 초기 오프셋(초)")]
		public Vector2 InitialOffset = new Vector2(0f, 0f);
        /// 표시 상태의 최소 및 최대 기간(초)
        [MMVector("Min", "Max")]
		[Tooltip("표시 상태의 최소 및 최대 기간(초)")]
		public Vector2 VisibleDuration = new Vector2(1f, 1f);
        /// 숨겨진 상태의 최소 및 최대 기간(초)
        [MMVector("Min", "Max")]
		[Tooltip("숨겨진 상태의 최소 및 최대 기간(초)")]
		public Vector2 HiddenDuration = new Vector2(1f, 1f);
        /// 숨겨진 상태에 표시되는 최소 및 최대 기간(초)
        [MMVector("Min", "Max")]
		[Tooltip("숨겨진 상태에 표시되는 최소 및 최대 기간(초)")]
		public Vector2 VisibleToHiddenDuration = new Vector2(1f, 1f);
        /// 숨김 상태에서 표시 상태까지의 최소 및 최대 기간(초)
        [MMVector("Min", "Max")]
		[Tooltip("숨김 상태에서 표시 상태까지의 최소 및 최대 기간(초)")]
		public Vector2 HiddenToVisibleDuration = new Vector2(1f, 1f);
                
		[Header("Feedbacks")]

        /// 표시 상태에 도달할 때 트리거할 피드백
        [Tooltip("표시 상태에 도달할 때 트리거할 피드백")]
		public MMFeedbacks VisibleFeedback;
        /// 공개 상태에서 숨겨진 상태에 도달할 때 트리거되는 피드백
        [Tooltip("공개 상태에서 숨겨진 상태에 도달할 때 트리거되는 피드백")]
		public MMFeedbacks VisibleToHiddenFeedback;
        /// 숨겨진 상태에 도달할 때 트리거할 피드백
        [Tooltip("숨겨진 상태에 도달할 때 트리거할 피드백")]
		public MMFeedbacks HiddenFeedback;
        /// 숨겨진 상태에서 보이는 상태에 도달할 때 트리거되는 피드백
        [Tooltip("숨겨진 상태에서 보이는 상태에 도달할 때 트리거되는 피드백")]
		public MMFeedbacks HiddenToVisibleFeedback;

		[Header("Bindings")]
        /// 업데이트할 애니메이터
        [Tooltip("업데이트할 애니메이터")]
		public Animator TargetAnimator;
        /// 표시하거나 숨길 게임 개체
        [Tooltip("표시하거나 숨길 게임 개체")]
		public GameObject TargetModel;
        /// 상태가 변경될 때 객체가 애니메이터(동일한 수준으로 설정)를 업데이트해야 하는지 여부
        [Tooltip("상태가 변경될 때 객체가 애니메이터(동일한 수준으로 설정)를 업데이트해야 하는지 여부")]
		public bool UpdateAnimator = true;
        /// 상태를 변경할 때 객체가 Collider 또는 Collider2D(동일한 레벨로 설정)를 업데이트해야 하는지 여부
        [Tooltip("상태를 변경할 때 객체가 Collider 또는 Collider2D(동일한 레벨로 설정)를 업데이트해야 하는지 여부")]
		public bool EnableDisableCollider = true;
        /// 상태가 변경될 때 객체가 모델을 숨기거나 표시해야 하는지 여부
        [Tooltip("상태가 변경될 때 객체가 모델을 숨기거나 표시해야 하는지 여부")]
		public bool ShowHideModel = false;

		[Header("Trigger Area")]

        /// 문자의 존재를 감지하는 데 사용되는 영역
        [Tooltip("문자의 존재를 감지하는 데 사용되는 영역")]
		public CharacterDetector TriggerArea;
        /// 캐릭터가 해당 영역에 있을 때 이 구성요소가 나타나는 것을 방지해야 하는지 여부
        [Tooltip("캐릭터가 해당 영역에 있을 때 이 구성요소가 나타나는 것을 방지해야 하는지 여부")]
		public bool PreventAppearWhenCharacterInArea = true;
        /// 캐릭터가 해당 영역에 있을 때 이 구성 요소가 사라지는 것을 방지해야 하는지 여부
        [Tooltip("캐릭터가 해당 영역에 있을 때 이 구성 요소가 사라지는 것을 방지해야 하는지 여부")]
		public bool PreventDisappearWhenCharacterInArea = false;

		[Header("Debug")]

        /// 이 객체의 현재 상태
        [MMReadOnly]
		[Tooltip("이 객체의 현재 상태")]
		public AppearDisappearStates _currentState;
        /// 이 객체가 다음에 있을 상태
        [MMReadOnly]
		[Tooltip("이 객체가 다음에 있을 상태")]
		public AppearDisappearStates _nextState;
        /// 이 객체가 다음에 있을 상태
        [MMReadOnly]
		[Tooltip("이 객체가 다음에 있을 상태")]
		public float _lastStateChangedAt = 0f;
		[MMReadOnly]
		[Tooltip("이 객체가 다음에 있을 상태")]
		public int _cyclesLeft;

		protected const string _animationParameter = "Visible";

		protected float _visibleDuration;
		protected float _hiddenDuration;
		protected float _visibleToHiddenDuration;
		protected float _hiddenToVisibleDuration;

		protected float _nextChangeIn;
		protected MMFeedbacks _nextFeedback;

		protected Collider _collider;
		protected Collider2D _collider2D;

		protected bool _characterInTriggerArea = false;

		/// <summary>
		/// On start we initialize our object
		/// </summary>
		protected virtual void Start()
		{
			Initialization();
		}

		/// <summary>
		/// On Init, we set our active state, grab components, and determine our next state
		/// </summary>
		protected virtual void Initialization()
		{
			_currentState = InitialState;
			_lastStateChangedAt = Time.time;
			_cyclesLeft = CyclesAmount;

			Active = (StartMode == StartModes.Automatic);

			if (_currentState == AppearDisappearStates.HiddenToVisible) { _currentState = AppearDisappearStates.Visible; }
			if (_currentState == AppearDisappearStates.VisibleToHidden) { _currentState = AppearDisappearStates.Hidden; }

			if (TargetAnimator == null)
			{
				TargetAnimator = this.gameObject.GetComponent<Animator>();	
			}
			
			_collider = this.gameObject.GetComponent<Collider>();
			_collider2D = this.gameObject.GetComponent<Collider2D>();

			RandomizeDurations();

			_visibleDuration += Random.Range(InitialOffset.x, InitialOffset.y);
			_hiddenDuration += Random.Range(InitialOffset.x, InitialOffset.y);

			UpdateBoundComponents(_currentState == AppearDisappearStates.Visible);

			DetermineNextState();
		}

		/// <summary>
		/// Activates or disables the appearing/disappearing behaviour
		/// </summary>
		/// <param name="status"></param>
		public virtual void Activate(bool status)
		{
			Active = status;
		}
        
		/// <summary>
		/// On Update we process our state machine
		/// </summary>
		protected virtual void Update()
		{
			ProcessTriggerArea();
			ProcessStateMachine();
		}

		protected virtual void ProcessTriggerArea()
		{
			_characterInTriggerArea = false;
			if (TriggerArea == null)
			{
				return;
			}
			_characterInTriggerArea = TriggerArea.CharacterInArea;
		}

		/// <summary>
		/// Changes the state to the next if time requires it
		/// </summary>
		protected virtual void ProcessStateMachine()
		{
			if (!Active)
			{
				return;
			}

			if (Time.time - _lastStateChangedAt > _nextChangeIn)
			{
				ChangeState();
			}
		}

		/// <summary>
		/// Determines the next state this object should be in
		/// </summary>
		protected virtual void DetermineNextState()
		{
			switch (_currentState)
			{
				case AppearDisappearStates.Visible:
					_nextChangeIn = _visibleDuration;
					_nextState = AppearDisappearStates.VisibleToHidden;
					_nextFeedback = VisibleToHiddenFeedback;
					break;
				case AppearDisappearStates.Hidden:
					_nextChangeIn = _hiddenDuration;
					_nextState = AppearDisappearStates.HiddenToVisible;
					_nextFeedback = HiddenToVisibleFeedback;
					break;
				case AppearDisappearStates.HiddenToVisible:
					_nextChangeIn = _hiddenToVisibleDuration;
					_nextState = AppearDisappearStates.Visible;
					_nextFeedback = VisibleFeedback;
					break;
				case AppearDisappearStates.VisibleToHidden:
					_nextChangeIn = _visibleToHiddenDuration;
					_nextState = AppearDisappearStates.Hidden;
					_nextFeedback = HiddenFeedback;
					break;
			}
		}

		/// <summary>
		/// Changes the state for the next in line
		/// </summary>
		public virtual void ChangeState()
		{
			if (((_nextState == AppearDisappearStates.HiddenToVisible) || (_nextState == AppearDisappearStates.Visible))
			    && _characterInTriggerArea
			    && PreventAppearWhenCharacterInArea)
			{
				return;
			}

			if (((_nextState == AppearDisappearStates.VisibleToHidden) || (_nextState == AppearDisappearStates.Hidden))
			    && _characterInTriggerArea
			    && PreventDisappearWhenCharacterInArea)
			{
				return;
			}

			_lastStateChangedAt = Time.time;
			_currentState = _nextState;
			_nextFeedback?.PlayFeedbacks();
			RandomizeDurations();

			if (_currentState == AppearDisappearStates.Hidden)
			{
				UpdateBoundComponents(false);   
			}

			if (_currentState == AppearDisappearStates.Visible)
			{
				UpdateBoundComponents(true);
			}
            
			DetermineNextState();

			if (CyclingMode == CyclingModes.Limited)
			{
				if (_currentState == AppearDisappearStates.Hidden || _currentState == AppearDisappearStates.Visible)
				{
					_cyclesLeft--;
					if (_cyclesLeft <= 0)
					{
						Active = false;
					}
				}
			}
		}

		/// <summary>
		/// Updates animator, collider and renderer according to the visible state
		/// </summary>
		/// <param name="visible"></param>
		protected virtual void UpdateBoundComponents(bool visible)
		{
			if (UpdateAnimator && (TargetAnimator != null))
			{
				TargetAnimator.SetBool(_animationParameter, visible);
			}
			if (EnableDisableCollider)
			{
				if (_collider != null)
				{
					_collider.enabled = visible;
				}
				if (_collider2D != null)
				{
					_collider2D.enabled = visible;
				}
			}
			if (ShowHideModel && (TargetModel != null))
			{
				TargetModel.SetActive(visible);
			}
		}

		/// <summary>
		/// Randomizes the durations of each state based on the mins and maxs set in the inpector
		/// </summary>
		protected virtual void RandomizeDurations()
		{
			_visibleDuration = Random.Range(VisibleDuration.x, VisibleDuration.y);
			_hiddenDuration = Random.Range(HiddenDuration.x, HiddenDuration.y);
			_visibleToHiddenDuration = Random.Range(VisibleToHiddenDuration.x, VisibleToHiddenDuration.y);
			_hiddenToVisibleDuration = Random.Range(HiddenToVisibleDuration.x, HiddenToVisibleDuration.y);
		}

		/// <summary>
		/// When colliding with another object, we check if it's a player, and if it is and we are supposed to start on player contact, we enable our object
		/// </summary>
		/// <param name="collider"></param>
		public virtual void OnTriggerEnter2D(Collider2D collider)
		{
			if (StartMode != StartModes.PlayerContact)
			{
				return;
			}

			if (collider.CompareTag("Player"))
			{
				_lastStateChangedAt = Time.time;
				Activate(true);
			}
		}

		public virtual void ResetCycling()
		{
			_cyclesLeft = CyclesAmount;
		}
	}
}