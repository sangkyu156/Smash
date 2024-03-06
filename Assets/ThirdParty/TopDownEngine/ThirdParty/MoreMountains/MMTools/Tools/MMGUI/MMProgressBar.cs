using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
#if MM_TEXTMESHPRO
using TMPro;
#endif
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 이 막대를 개체에 추가하고 막대(아마도 스크립트가 있는 동일한 개체)에 연결하면 최소값과 최대값 사이에 있는 현재 값을 기준으로 막대 개체의 크기를 조정할 수 있습니다.
    /// 사용 사례는 HealthBar.cs 스크립트를 참조하세요.
    /// </summary>
    [MMRequiresConstantRepaint]
	[AddComponentMenu("More Mountains/Tools/GUI/MMProgressBar")]
	public class MMProgressBar : MMMonoBehaviour
	{
		public enum MMProgressBarStates {Idle, Decreasing, Increasing, InDecreasingDelay, InIncreasingDelay }
        /// 가능한 채우기 모드
        public enum FillModes { LocalScale, FillAmount, Width, Height, Anchor }
        /// 가능한 채우기 방향(로컬 축척 및 채우기 양에만 해당)
        public enum BarDirections { LeftToRight, RightToLeft, UpToDown, DownToUp }
        /// 바가 작동할 수 있는 가능한 시간 척도
        public enum TimeScales { UnscaledTime, Time }
        /// t막대 채우기에 애니메이션을 적용할 수 있는 방법
        public enum BarFillModes { SpeedBased, FixedDuration }
        
		[MMInspectorGroup("Bindings", true, 10)]
        /// 선택사항 - 이 바에 연관된 플레이어의 ID
        [Tooltip("선택사항 - 이 바에 연관된 플레이어의 ID")]
		public string PlayerID;
        /// 메인, 전경 바
        [Tooltip("메인, 전경 바")]
		public Transform ForegroundBar;
        /// 값에서 더 낮은 새 값으로 이동할 때 표시되는 지연된 막대
        [Tooltip("값에서 더 낮은 새 값으로 이동할 때 표시되는 지연된 막대")]
		[FormerlySerializedAs("DelayedBar")] 
		public Transform DelayedBarDecreasing;
        /// 값에서 더 높은 새 값으로 이동할 때 표시되는 지연된 막대
        [Tooltip("값에서 더 높은 새 값으로 이동할 때 표시되는 지연된 막대")]
		public Transform DelayedBarIncreasing;
        
		[MMInspectorGroup("Fill Settings", true, 11)]
        /// 막대에 연결된 값이 0%일 때 도달할 로컬 배율 또는 채우기 양 값
        [FormerlySerializedAs("StartValue")] 
		[Range(0f,1f)]
		[Tooltip("막대에 연결된 값이 0%일 때 도달할 로컬 배율 또는 채우기 양 값")]
		public float MinimumBarFillValue = 0f;
        /// 막대가 가득 찼을 때 도달할 로컬 배율 또는 채우기 양 값
        [FormerlySerializedAs("EndValue")] 
		[Range(0f,1f)]
		[Tooltip("막대가 가득 찼을 때 도달할 로컬 배율 또는 채우기 양 값")]
		public float MaximumBarFillValue = 1f;
        /// 시작 시 막대 값을 초기화할지 여부
        [Tooltip("시작 시 막대 값을 초기화할지 여부")]
		public bool SetInitialFillValueOnStart = false;
        /// 막대의 초기 값
        [MMCondition("SetInitialFillValueOnStart", true)]
		[Range(0f,1f)]
		[Tooltip("막대의 초기 값")]
		public float InitialFillValue = 0f;
        /// 이 막대가 움직이는 방향
        [Tooltip("이 막대가 움직이는 방향")]
		public BarDirections BarDirection = BarDirections.LeftToRight;
        /// 전경 막대의 채우기 모드
        [Tooltip("전경 막대의 채우기 모드")]
		public FillModes FillMode = FillModes.LocalScale;
        /// 막대가 조정된 시간 또는 조정되지 않은 시간에 작동할지 여부를 정의합니다(예를 들어 시간이 느려지는 경우 계속 움직일지 여부)
        [Tooltip("막대가 조정된 시간 또는 조정되지 않은 시간에 작동할지 여부를 정의합니다(예를 들어 시간이 느려지는 경우 계속 움직일지 여부)")]
		public TimeScales TimeScale = TimeScales.UnscaledTime;
        /// 선택한 채우기 애니메이션 모드
        [Tooltip("선택한 채우기 애니메이션 모드")]
		public BarFillModes BarFillMode = BarFillModes.SpeedBased;

		[MMInspectorGroup("Foreground Bar Settings", true, 12)]
        /// 전경 막대가 소리를 내야 하는지 여부
        [Tooltip("전경 막대가 소리를 내야 하는지 여부")]
		public bool LerpForegroundBar = true;
        /// 전경 막대를 이동하는 속도
        [Tooltip("전경 막대를 이동하는 속도")]
		[MMCondition("LerpForegroundBar", true)]
		public float LerpForegroundBarSpeedDecreasing = 15f;
        /// 값이 증가하는 경우 전경 막대를 이동하는 속도
        [Tooltip("값이 증가하는 경우 전경 막대를 이동하는 속도")]
		[FormerlySerializedAs("LerpForegroundBarSpeed")]
		[MMCondition("LerpForegroundBar", true)]
		public float LerpForegroundBarSpeedIncreasing = 15f;
        /// 속도가 감소하는 경우 전경 막대를 이동하는 속도
        [Tooltip("속도가 감소하는 경우 전경 막대를 이동하는 속도")]
		[MMCondition("LerpForegroundBar", true)]
		public float LerpForegroundBarDurationDecreasing = 0.2f;
        /// 전경 막대의 각 업데이트에 소요되는 시간(고정 기간 막대 채우기 모드인 경우에만)
        [Tooltip("전경 막대의 각 업데이트에 소요되는 시간(고정 기간 막대 채우기 모드인 경우에만)")]
		[MMCondition("LerpForegroundBar", true)]
		public float LerpForegroundBarDurationIncreasing = 0.2f;
        /// 전경 막대 채우기 감소에 애니메이션을 적용할 때 사용할 곡선
        [Tooltip("전경 막대 채우기 감소에 애니메이션을 적용할 때 사용할 곡선")]
		[MMCondition("LerpForegroundBar", true)]
		public AnimationCurve LerpForegroundBarCurveDecreasing = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        /// 전경 막대 채우기 증가에 애니메이션을 적용할 때 사용할 곡선
        [Tooltip("전경 막대 채우기 증가에 애니메이션을 적용할 때 사용할 곡선")]
		[MMCondition("LerpForegroundBar", true)]
		public AnimationCurve LerpForegroundBarCurveIncreasing = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

		[MMInspectorGroup("Delayed Bar Decreasing", true, 13)]

        /// 지연된 막대가 움직이기 전의 지연 시간(초)
        [Tooltip("지연된 막대가 움직이기 전의 지연 시간(초)")]
		[FormerlySerializedAs("Delay")] 
		public float DecreasingDelay = 1f;
        /// 지연된 막대의 애니메이션이 불안정해야 하는지 여부
        [Tooltip("지연된 막대의 애니메이션이 불안정해야 하는지 여부")]
		[FormerlySerializedAs("LerpDelayedBar")] 
		public bool LerpDecreasingDelayedBar = true;
        /// 지연된 막대를 lerp하는 속도
        [Tooltip("지연된 막대를 lerp하는 속도")]
		[FormerlySerializedAs("LerpDelayedBarSpeed")] 
		[MMCondition("LerpDecreasingDelayedBar", true)]
		public float LerpDecreasingDelayedBarSpeed = 15f;
        /// 전경 막대의 각 업데이트에 소요되는 시간(고정 기간 막대 채우기 모드인 경우에만)
        [Tooltip("전경 막대의 각 업데이트에 소요되는 시간(고정 기간 막대 채우기 모드인 경우에만)")]
		[FormerlySerializedAs("LerpDelayedBarDuration")] 
		[MMCondition("LerpDecreasingDelayedBar", true)]
		public float LerpDecreasingDelayedBarDuration = 0.2f;
        /// 지연된 막대 채우기를 애니메이션화할 때 사용할 곡선
        [Tooltip("지연된 막대 채우기를 애니메이션화할 때 사용할 곡선")]
		[FormerlySerializedAs("LerpDelayedBarCurve")] 
		[MMCondition("LerpDecreasingDelayedBar", true)]
		public AnimationCurve LerpDecreasingDelayedBarCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

		[MMInspectorGroup("Delayed Bar Increasing", true, 18)]

        /// 지연된 막대가 움직이기 전의 지연 시간(초)
        [Tooltip("지연된 막대가 움직이기 전의 지연 시간(초)")]
		public float IncreasingDelay = 1f;
        /// 지연된 막대의 애니메이션이 불안정해야 하는지 여부
        [Tooltip("지연된 막대의 애니메이션이 불안정해야 하는지 여부")]
		public bool LerpIncreasingDelayedBar = true;
        /// 지연된 막대를 lerp하는 속도
        [Tooltip("지연된 막대를 lerp하는 속도")]
		[MMCondition("LerpIncreasingDelayedBar", true)]
		public float LerpIncreasingDelayedBarSpeed = 15f;
        /// 전경 막대의 각 업데이트에 소요되는 시간(고정 기간 막대 채우기 모드인 경우에만)
        [Tooltip("전경 막대의 각 업데이트에 소요되는 시간(고정 기간 막대 채우기 모드인 경우에만)")]
		[MMCondition("LerpIncreasingDelayedBar", true)]
		public float LerpIncreasingDelayedBarDuration = 0.2f;
        /// 지연된 막대 채우기를 애니메이션화할 때 사용할 곡선
        [Tooltip("지연된 막대 채우기를 애니메이션화할 때 사용할 곡선")]
		[MMCondition("LerpIncreasingDelayedBar", true)]
		public AnimationCurve LerpIncreasingDelayedBarCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

		[MMInspectorGroup("Bump", true, 14)]
        /// 값을 변경할 때 막대가 "범프"해야 하는지 여부
        [Tooltip("값을 변경할 때 막대가 \"범프\"해야 하는지 여부")]
		public bool BumpScaleOnChange = true;
        /// 값이 증가할 때 막대가 부딪혀야 하는지 여부
        [Tooltip("값이 증가할 때 막대가 부딪혀야 하는지 여부")]
		public bool BumpOnIncrease = false;
        /// 값이 감소할 때 막대가 부딪혀야 하는지 여부
        [Tooltip("값이 감소할 때 막대가 부딪혀야 하는지 여부")]
		public bool BumpOnDecrease = false;
        /// 범프 애니메이션의 지속 시간
        [Tooltip("범프 애니메이션의 지속 시간")]
		public float BumpDuration = 0.2f;
        /// 부딪힐 때 바가 깜박여야 하는지 여부
        [Tooltip("부딪힐 때 바가 깜박여야 하는지 여부")]
		public bool ChangeColorWhenBumping = true;
        /// 범프 전에 초기 막대 색상을 저장할지 여부
        [Tooltip("범프 전에 초기 막대 색상을 저장할지 여부")]
		public bool StoreBarColorOnPlay = true;
        /// 부딪힐 때 바에 적용할 색상
        [Tooltip("부딪힐 때 바에 적용할 색상")]
		[MMCondition("ChangeColorWhenBumping", true)]
		public Color BumpColor = Color.white;
        /// 범프 애니메이션을 매핑할 곡선
        [Tooltip("범프 애니메이션을 매핑할 곡선")]
		[FormerlySerializedAs("BumpAnimationCurve")]
		public AnimationCurve BumpScaleAnimationCurve = new AnimationCurve(new Keyframe(1, 1), new Keyframe(0.3f, 1.05f), new Keyframe(1, 1));
        /// 범프 애니메이션 색상 애니메이션을 매핑할 곡선
        [Tooltip("범프 애니메이션 색상 애니메이션을 매핑할 곡선")]
		public AnimationCurve BumpColorAnimationCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0));
        /// 지금 바가 부딪히는지 아닌지
        public bool Bumping { get; protected set; }

		[MMInspectorGroup("Events", true, 16)]

        /// 막대가 부딪힐 때마다 트리거되는 이벤트
        [Tooltip("막대가 부딪힐 때마다 트리거되는 이벤트")]
		public UnityEvent OnBump;
        /// 막대가 감소하기 시작할 때마다 트리거되는 이벤트
        [Tooltip("막대가 감소하기 시작할 때마다 트리거되는 이벤트")]
		public UnityEvent OnBarMovementDecreasingStart;
        /// 막대가 감소하는 것을 멈출 때마다 트리거되는 이벤트
        [Tooltip("막대가 감소하는 것을 멈출 때마다 트리거되는 이벤트")]
		public UnityEvent OnBarMovementDecreasingStop;
        /// 막대가 증가하기 시작할 때마다 트리거되는 이벤트
        [Tooltip("막대가 증가하기 시작할 때마다 트리거되는 이벤트")]
		public UnityEvent OnBarMovementIncreasingStart;
        /// 막대의 증가가 멈출 때마다 트리거되는 이벤트
        [Tooltip("막대의 증가가 멈출 때마다 트리거되는 이벤트")]
		public UnityEvent OnBarMovementIncreasingStop;

		[MMInspectorGroup("Text", true, 20)]
        /// 막대의 값으로 업데이트할 Text 객체
        [Tooltip("막대의 값으로 업데이트할 Text 객체")]
		public Text PercentageText;
#if MM_TEXTMESHPRO
        /// 막대의 값으로 업데이트할 TMPro 텍스트 객체
        [Tooltip("막대의 값으로 업데이트할 TMPro 텍스트 객체")]
		public TMP_Text PercentageTextMeshPro;
#endif

        /// 바의 값 표시에 항상 추가되는 접두사
        [Tooltip("바의 값 표시에 항상 추가되는 접두사")]
		public string TextPrefix;
        /// 막대의 값 표시에 항상 추가되는 접미사
        [Tooltip("막대의 값 표시에 항상 추가되는 접미사")]
		public string TextSuffix;
        /// 막대를 표시할 때 막대의 값에 항상 적용할 값 승수
        [Tooltip("막대를 표시할 때 막대의 값에 항상 적용할 값 승수")]
		public float TextValueMultiplier = 1f;
        /// 텍스트가 표시되어야 하는 형식
        [Tooltip("텍스트가 표시되어야 하는 형식")]
		public string TextFormat = "{000}";
        /// 현재 값 이후의 합계를 표시할지 여부
        [Tooltip("현재 값 이후의 합계를 표시할지 여부")]
		public bool DisplayTotal = false;
        /// DisplayTotal이 true인 경우 현재 값과 합계 사이에 넣을 구분 기호입니다.
        [Tooltip("DisplayTotal이 true인 경우 현재 값과 합계 사이에 넣을 구분 기호입니다.")]
		[MMCondition("DisplayTotal", true)]
		public string TotalSeparator = " / ";

		[MMInspectorGroup("Debug", true, 15)]
        /// DebugSet 버튼을 누르면 막대가 이동할 값
        [Tooltip("DebugSet 버튼을 누르면 막대가 이동할 값")]
		[Range(0f, 1f)] 
		public float DebugNewTargetValue;

		[MMInspectorButton("DebugUpdateBar")]
		public bool DebugUpdateBarButton;
		[MMInspectorButton("DebugSetBar")]
		public bool DebugSetBarButton;
		[MMInspectorButton("Bump")]
		public bool TestBumpButton;
		[MMInspectorButton("Plus10Percent")]
		public bool Plus10PercentButton;
		[MMInspectorButton("Minus10Percent")]
		public bool Minus10PercentButton;
        
		[MMInspectorGroup("Debug Read Only", true, 19)]
        /// 막대의 현재 진행 상황(이상적으로는 읽기 전용)
        [Tooltip("막대의 현재 진행 상황(이상적으로는 읽기 전용)")]
		[Range(0f,1f)]
		public float BarProgress;
        /// 막대의 현재 진행 상황(이상적으로는 읽기 전용)
        [Tooltip("막대의 현재 진행 상황(이상적으로는 읽기 전용)")]
		[Range(0f,1f)]
		public float BarTarget;
        /// 지연된 막대의 현재 진행 상황이 증가하고 있습니다.
        [Tooltip("지연된 막대의 현재 진행 상황이 증가하고 있습니다.")]
		[Range(0f,1f)]
		public float DelayedBarIncreasingProgress;
        /// 지연된 막대의 현재 진행 상황이 감소하고 있습니다.
        [Tooltip("지연된 막대의 현재 진행 상황이 감소하고 있습니다.")]
		[Range(0f,1f)]
		public float DelayedBarDecreasingProgress;

		protected bool _initialized;
		protected Vector2 _initialBarSize;
		protected Color _initialColor;
		protected Vector3 _initialScale;
		protected Image _foregroundImage;
		protected Image _delayedDecreasingImage;
		protected Image _delayedIncreasingImage;
		protected Vector3 _targetLocalScale = Vector3.one;
		protected float _newPercent;
		protected float _percentLastTimeBarWasUpdated;
		protected float _lastUpdateTimestamp;
		protected float _time;
		protected float _deltaTime;
		protected int _direction;
		protected Coroutine _coroutine;
		protected bool _coroutineShouldRun = false;
		protected bool _isDelayedBarIncreasingNotNull;
		protected bool _isDelayedBarDecreasingNotNull;
		protected bool _actualUpdate;
		protected Vector2 _anchorVector;
		protected float _delayedBarDecreasingProgress;
		protected float _delayedBarIncreasingProgress;
		protected MMProgressBarStates CurrentState = MMProgressBarStates.Idle;
		protected string _updatedText;
		protected string _totalText;
		protected bool _isForegroundBarNotNull;
		protected bool _isForegroundImageNotNull;
		protected bool _isPercentageTextNotNull;
		protected bool _isPercentageTextMeshProNotNull;

		#region PUBLIC_API
        
		/// <summary>
		/// Updates the bar's values, using a normalized value
		/// </summary>
		/// <param name="normalizedValue"></param>
		public virtual void UpdateBar01(float normalizedValue) 
		{
			UpdateBar(Mathf.Clamp01(normalizedValue), 0f, 1f);
		}
        
		/// <summary>
		/// Updates the bar's values based on the specified parameters
		/// </summary>
		/// <param name="currentValue">Current value.</param>
		/// <param name="minValue">Minimum value.</param>
		/// <param name="maxValue">Max value.</param>
		public virtual void UpdateBar(float currentValue,float minValue,float maxValue) 
		{
			if (!_initialized)
			{
				Initialization();
			}

			if (StoreBarColorOnPlay)
			{
				StoreInitialColor();	
			}

			if (!this.gameObject.activeInHierarchy)
			{
				this.gameObject.SetActive(true);    
			}
            
			_newPercent = MMMaths.Remap(currentValue, minValue, maxValue, MinimumBarFillValue, MaximumBarFillValue);
	        
			_actualUpdate = (BarTarget != _newPercent);
	        
			if (!_actualUpdate)
			{
				return;
			}
	        
			if (CurrentState != MMProgressBarStates.Idle)
			{
				if ((CurrentState == MMProgressBarStates.Decreasing) ||
				    (CurrentState == MMProgressBarStates.InDecreasingDelay))
				{
					if (_newPercent >= BarTarget)
					{
						StopCoroutine(_coroutine);
						SetBar01(BarTarget);
					}
				}
				if ((CurrentState == MMProgressBarStates.Increasing) ||
				    (CurrentState == MMProgressBarStates.InIncreasingDelay))
				{
					if (_newPercent <= BarTarget)
					{
						StopCoroutine(_coroutine);
						SetBar01(BarTarget);
					}
				}
			}
	        
			_percentLastTimeBarWasUpdated = BarProgress;
			_delayedBarDecreasingProgress = DelayedBarDecreasingProgress;
			_delayedBarIncreasingProgress = DelayedBarIncreasingProgress;
	        
			BarTarget = _newPercent;
			
			if ((_newPercent != _percentLastTimeBarWasUpdated) && !Bumping)
			{
				Bump();
			}

			DetermineDeltaTime();
			_lastUpdateTimestamp = _time;
	        
			DetermineDirection();
			if (_direction < 0)
			{
				OnBarMovementDecreasingStart?.Invoke();
			}
			else
			{
				OnBarMovementIncreasingStart?.Invoke();
			}
		        
			if (_coroutine != null)
			{
				StopCoroutine(_coroutine);
			}
			_coroutineShouldRun = true;     
		    

			if (this.gameObject.activeInHierarchy)
			{
				_coroutine = StartCoroutine(UpdateBarsCo());
			}
			else
			{
				SetBar(currentValue, minValue, maxValue);
			}

			UpdateText();
		}

		/// <summary>
		/// Sets the bar value to the one specified 
		/// </summary>
		/// <param name="currentValue"></param>
		/// <param name="minValue"></param>
		/// <param name="maxValue"></param>
		public virtual void SetBar(float currentValue, float minValue, float maxValue)
		{
			float newPercent = MMMaths.Remap(currentValue, minValue, maxValue, 0f, 1f);
			SetBar01(newPercent);
		}

		/// <summary>
		/// Sets the bar value to the normalized value set in parameter
		/// </summary>
		/// <param name="newPercent"></param>
		public virtual void SetBar01(float newPercent)
		{
			if (!_initialized)
			{
				Initialization();
			}

			newPercent = MMMaths.Remap(newPercent, 0f, 1f, MinimumBarFillValue, MaximumBarFillValue);
			BarProgress = newPercent;
			DelayedBarDecreasingProgress = newPercent;
			DelayedBarIncreasingProgress = newPercent;
			//_newPercent = newPercent;
			BarTarget = newPercent;
			_percentLastTimeBarWasUpdated = newPercent;
			_delayedBarDecreasingProgress = DelayedBarDecreasingProgress;
			_delayedBarIncreasingProgress = DelayedBarIncreasingProgress;
			SetBarInternal(newPercent, ForegroundBar, _foregroundImage, _initialBarSize);
			SetBarInternal(newPercent, DelayedBarDecreasing, _delayedDecreasingImage, _initialBarSize);
			SetBarInternal(newPercent, DelayedBarIncreasing, _delayedIncreasingImage, _initialBarSize);
			UpdateText();
			_coroutineShouldRun = false;
			CurrentState = MMProgressBarStates.Idle;
		}
        
		#endregion PUBLIC_API

		#region START
        
		/// <summary>
		/// On start we store our image component
		/// </summary>
		protected virtual void Start()
		{
			Initialization();
		}

		protected virtual void OnEnable()
		{
			if (!_initialized)
			{
				return;
			}

			StoreInitialColor();
		}

		public virtual void Initialization()
		{
			_isForegroundBarNotNull = ForegroundBar != null;
			_isDelayedBarDecreasingNotNull = DelayedBarDecreasing != null;
			_isDelayedBarIncreasingNotNull = DelayedBarIncreasing != null;
			_isPercentageTextNotNull = PercentageText != null;
			#if MM_TEXTMESHPRO
			_isPercentageTextMeshProNotNull = PercentageTextMeshPro != null;
			#endif
			_initialScale = this.transform.localScale;

			if (_isForegroundBarNotNull)
			{
				_foregroundImage = ForegroundBar.GetComponent<Image>();
				_isForegroundImageNotNull = _foregroundImage != null;
				_initialBarSize = _foregroundImage.rectTransform.sizeDelta;
			}
			if (_isDelayedBarDecreasingNotNull)
			{
				_delayedDecreasingImage = DelayedBarDecreasing.GetComponent<Image>();
			}
			if (_isDelayedBarIncreasingNotNull)
			{
				_delayedIncreasingImage = DelayedBarIncreasing.GetComponent<Image>();
			}
			_initialized = true;

			StoreInitialColor();

			_percentLastTimeBarWasUpdated = BarProgress;

			if (SetInitialFillValueOnStart)
			{
				SetBar01(InitialFillValue);
			}
		}

		protected virtual void StoreInitialColor()
		{
			if (!Bumping && _isForegroundImageNotNull)
			{
				_initialColor = _foregroundImage.color;
			}
		}
        
		#endregion START

		#region TESTS

		/// <summary>
		/// This test method, called via the inspector button of the same name, lets you test what happens when you update the bar to a certain value
		/// </summary>
		protected virtual void DebugUpdateBar()
		{
			this.UpdateBar01(DebugNewTargetValue);
		}
        
		/// <summary>
		/// Test method
		/// </summary>
		protected virtual void DebugSetBar()
		{
			this.SetBar01(DebugNewTargetValue);
		}

		/// <summary>
		/// Test method
		/// </summary>
		public virtual void Plus10Percent()
		{
			float newProgress = BarTarget + 0.1f;
			newProgress = Mathf.Clamp(newProgress, 0f, 1f);
			UpdateBar01(newProgress);
		}
        
		/// <summary>
		/// Test method
		/// </summary>
		public virtual void Minus10Percent()
		{
			float newProgress = BarTarget - 0.1f;
			newProgress = Mathf.Clamp(newProgress, 0f, 1f);
			UpdateBar01(newProgress);
		}


		#endregion TESTS

		protected virtual void UpdateText()
		{
			_updatedText = TextPrefix + (BarTarget * TextValueMultiplier).ToString(TextFormat);
			if (DisplayTotal)
			{
				_updatedText += TotalSeparator + (TextValueMultiplier).ToString(TextFormat);
			}
			_updatedText += TextSuffix;
			if (_isPercentageTextNotNull)
			{
				PercentageText.text = _updatedText;
			}
			#if MM_TEXTMESHPRO
			if (_isPercentageTextMeshProNotNull)
			{
				PercentageTextMeshPro.text = _updatedText;
			}
			#endif
		}
        
		/// <summary>
		/// On Update we update our bars
		/// </summary>
		protected virtual IEnumerator UpdateBarsCo()
		{
			while (_coroutineShouldRun)
			{
				DetermineDeltaTime();
				DetermineDirection();
				UpdateBars();
				yield return null;
			}

			CurrentState = MMProgressBarStates.Idle;
			yield break;
		}
		
		protected virtual void DetermineDeltaTime()
		{
			_deltaTime = (TimeScale == TimeScales.Time) ? Time.deltaTime : Time.unscaledDeltaTime;
			_time = (TimeScale == TimeScales.Time) ? Time.time : Time.unscaledTime;
		}

		protected virtual void DetermineDirection()
		{
			_direction = (_newPercent > _percentLastTimeBarWasUpdated) ? 1 : -1;
		}

		/// <summary>
		/// Updates the foreground bar's scale
		/// </summary>
		protected virtual void UpdateBars()
		{
			float newFill;
			float newFillDelayed;
			float t1, t2 = 0f;
			
			// if the value is decreasing
			if (_direction < 0)
			{
				newFill = ComputeNewFill(LerpForegroundBar, LerpForegroundBarSpeedDecreasing, LerpForegroundBarDurationDecreasing, LerpForegroundBarCurveDecreasing, 0f, _percentLastTimeBarWasUpdated, out t1);
				SetBarInternal(newFill, ForegroundBar, _foregroundImage, _initialBarSize);
				SetBarInternal(newFill, DelayedBarIncreasing, _delayedIncreasingImage, _initialBarSize);

				BarProgress = newFill;
				DelayedBarIncreasingProgress = newFill;

				CurrentState = MMProgressBarStates.Decreasing;
				
				if (_time - _lastUpdateTimestamp > DecreasingDelay)
				{
					newFillDelayed = ComputeNewFill(LerpDecreasingDelayedBar, LerpDecreasingDelayedBarSpeed, LerpDecreasingDelayedBarDuration, LerpDecreasingDelayedBarCurve, DecreasingDelay,_delayedBarDecreasingProgress, out t2);
					SetBarInternal(newFillDelayed, DelayedBarDecreasing, _delayedDecreasingImage, _initialBarSize);

					DelayedBarDecreasingProgress = newFillDelayed;
					CurrentState = MMProgressBarStates.InDecreasingDelay;
				}
			}
			else // if the value is increasing
			{
				newFill = ComputeNewFill(LerpForegroundBar, LerpForegroundBarSpeedIncreasing, LerpForegroundBarDurationIncreasing, LerpForegroundBarCurveIncreasing, 0f, _delayedBarIncreasingProgress, out t1);
				SetBarInternal(newFill, DelayedBarIncreasing, _delayedIncreasingImage, _initialBarSize);
				
				DelayedBarIncreasingProgress = newFill;
				CurrentState = MMProgressBarStates.Increasing;

				if (DelayedBarIncreasing == null)
				{
					newFill = ComputeNewFill(LerpForegroundBar, LerpForegroundBarSpeedIncreasing, LerpForegroundBarDurationIncreasing, LerpForegroundBarCurveIncreasing, 0f, _percentLastTimeBarWasUpdated, out t2);
					SetBarInternal(newFill, DelayedBarDecreasing, _delayedDecreasingImage, _initialBarSize);
					SetBarInternal(newFill, ForegroundBar, _foregroundImage, _initialBarSize);
					
					BarProgress = newFill;	
					DelayedBarDecreasingProgress = newFill;
					CurrentState = MMProgressBarStates.InDecreasingDelay;
				}
				else
				{
					if (_time - _lastUpdateTimestamp > IncreasingDelay)
					{
						newFillDelayed = ComputeNewFill(LerpIncreasingDelayedBar, LerpForegroundBarSpeedIncreasing, LerpForegroundBarDurationIncreasing, LerpForegroundBarCurveIncreasing, IncreasingDelay, _delayedBarDecreasingProgress, out t2);
					
						SetBarInternal(newFillDelayed, DelayedBarDecreasing, _delayedDecreasingImage, _initialBarSize);
						SetBarInternal(newFillDelayed, ForegroundBar, _foregroundImage, _initialBarSize);
					
						BarProgress = newFillDelayed;	
						DelayedBarDecreasingProgress = newFillDelayed;
						CurrentState = MMProgressBarStates.InDecreasingDelay;
					}
				}
			}
			
			if ((t1 >= 1f) && (t2 >= 1f))
			{
				_coroutineShouldRun = false;
				if (_direction > 0)
				{
					OnBarMovementIncreasingStop?.Invoke();
				}
				else
				{
					OnBarMovementDecreasingStop?.Invoke();
				}
			}
		}

		protected virtual float ComputeNewFill(bool lerpBar, float barSpeed, float barDuration, AnimationCurve barCurve, float delay, float lastPercent, out float t)
		{
			float newFill = 0f;
			t = 0f;
			if (lerpBar)
			{
				float delta = 0f;
				float timeSpent = _time - _lastUpdateTimestamp - delay;
				float speed = barSpeed;
				if (speed == 0f) { speed = 1f; }
				
				float duration = (BarFillMode == BarFillModes.FixedDuration) ? barDuration : (Mathf.Abs(_newPercent - lastPercent)) / speed;
				
				delta = MMMaths.Remap(timeSpent, 0f, duration, 0f, 1f);
				delta = Mathf.Clamp(delta, 0f, 1f);
				t = delta;
				if (t < 1f)
				{
					delta = barCurve.Evaluate(delta);
					newFill = Mathf.LerpUnclamped(lastPercent, _newPercent, delta);	
				}
				else
				{
					newFill = _newPercent;
				}
			}
			else
			{
				newFill = _newPercent;
			}

			newFill = Mathf.Clamp( newFill, 0f, 1f);

			return newFill;
		}

		protected virtual void SetBarInternal(float newAmount, Transform bar, Image image, Vector2 initialSize)
		{
			if (bar == null)
			{
				return;
			}
			
			switch (FillMode)
			{
				case FillModes.LocalScale:
					_targetLocalScale = Vector3.one;
					switch (BarDirection)
					{
						case BarDirections.LeftToRight:
							_targetLocalScale.x = newAmount;
							break;
						case BarDirections.RightToLeft:
							_targetLocalScale.x = 1f - newAmount;
							break;
						case BarDirections.DownToUp:
							_targetLocalScale.y = newAmount;
							break;
						case BarDirections.UpToDown:
							_targetLocalScale.y = 1f - newAmount;
							break;
					}

					bar.localScale = _targetLocalScale;
					break;

				case FillModes.Width:
					if (image == null)
					{
						return;
					}
					float newSizeX = MMMaths.Remap(newAmount, 0f, 1f, 0, initialSize.x);
					image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newSizeX);
					break;

				case FillModes.Height:
					if (image == null)
					{
						return;
					}
					float newSizeY = MMMaths.Remap(newAmount, 0f, 1f, 0, initialSize.y);
					image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newSizeY);
					break;

				case FillModes.FillAmount:
					if (image == null)
					{
						return;
					}
					image.fillAmount = newAmount;
					break;
				case FillModes.Anchor:
					if (image == null)
					{
						return;
					}
					switch (BarDirection)
					{
						case BarDirections.LeftToRight:
							_anchorVector.x = 0f;
							_anchorVector.y = 0f;
							image.rectTransform.anchorMin = _anchorVector;
							_anchorVector.x = newAmount;
							_anchorVector.y = 1f;
							image.rectTransform.anchorMax = _anchorVector;
							break;
						case BarDirections.RightToLeft:
							_anchorVector.x = newAmount;
							_anchorVector.y = 0f;
							image.rectTransform.anchorMin = _anchorVector;
							_anchorVector.x = 1f;
							_anchorVector.y = 1f;
							image.rectTransform.anchorMax = _anchorVector;
							break;
						case BarDirections.DownToUp:
							_anchorVector.x = 0f;
							_anchorVector.y = 0f;
							image.rectTransform.anchorMin = _anchorVector;
							_anchorVector.x = 1f;
							_anchorVector.y = newAmount;
							image.rectTransform.anchorMax = _anchorVector;
							break;
						case BarDirections.UpToDown:
							_anchorVector.x = 0f;
							_anchorVector.y = newAmount;
							image.rectTransform.anchorMin = _anchorVector;
							_anchorVector.x = 1f;
							_anchorVector.y = 1f;
							image.rectTransform.anchorMax = _anchorVector;
							break;
					}
					break;
			}
		}

		#region  Bump

		/// <summary>
		/// Triggers a camera bump
		/// </summary>
		public virtual void Bump()
		{
			bool shouldBump = false;

			if (!_initialized)
			{
				return;
			}
			
			DetermineDirection();
			
			if (BumpOnIncrease && (_direction > 0))
			{
				shouldBump = true;
			}
			
			if (BumpOnDecrease && (_direction < 0))
			{
				shouldBump = true;
			}
			
			if (BumpScaleOnChange)
			{
				shouldBump = true;
			}

			if (!shouldBump)
			{
				return;
			}
			
			if (this.gameObject.activeInHierarchy)
			{
				StartCoroutine(BumpCoroutine());
			}

			OnBump?.Invoke();
		}

		/// <summary>
		/// A coroutine that (usually quickly) changes the scale of the bar 
		/// </summary>
		/// <returns>The coroutine.</returns>
		protected virtual IEnumerator BumpCoroutine()
		{
			float journey = 0f;

			Bumping = true;

			while (journey <= BumpDuration)
			{
				journey = journey + _deltaTime;
				float percent = Mathf.Clamp01(journey / BumpDuration);
				float curvePercent = BumpScaleAnimationCurve.Evaluate(percent);
				float colorCurvePercent = BumpColorAnimationCurve.Evaluate(percent);
				this.transform.localScale = curvePercent * _initialScale;

				if (ChangeColorWhenBumping && _isForegroundImageNotNull)
				{
					_foregroundImage.color = Color.Lerp(_initialColor, BumpColor, colorCurvePercent);
				}
				yield return null;
			}
			if (ChangeColorWhenBumping && _isForegroundImageNotNull)
			{
				_foregroundImage.color = _initialColor;
			}
			Bumping = false;
			yield return null;
		}

		#endregion Bump

		#region ShowHide

		/// <summary>
		/// A simple method you can call to show the bar (set active true)
		/// </summary>
		public virtual void ShowBar()
		{
			this.gameObject.SetActive(true);
		}

		/// <summary>
		/// Hides (SetActive false) the progress bar object, after an optional delay
		/// </summary>
		/// <param name="delay"></param>
		public virtual void HideBar(float delay)
		{
			if (delay <= 0)
			{
				this.gameObject.SetActive(false);
			}
			else if (this.gameObject.activeInHierarchy)
			{
				StartCoroutine(HideBarCo(delay));
			}
		}

		/// <summary>
		/// An internal coroutine used to handle the disabling of the progress bar after a delay
		/// </summary>
		/// <param name="delay"></param>
		/// <returns></returns>
		protected virtual IEnumerator HideBarCo(float delay)
		{
			yield return MMCoroutine.WaitFor(delay);
			this.gameObject.SetActive(false);
		}

		#endregion ShowHide
		
	}
}