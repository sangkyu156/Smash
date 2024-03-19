using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using MoreMountains.Tools;

namespace MoreMountains.Tools
{
    /// <summary>
    /// Fader 클래스는 이미지에 배치될 수 있으며 MMFadeEvents를 가로채서 그에 따라 자체적으로 켜거나 끌 것입니다.
    /// 이 특정 페이더는 왼쪽에서 오른쪽으로, 오른쪽에서 왼쪽으로, 위에서 아래로 또는 아래에서 위로 이동합니다.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
	[AddComponentMenu("More Mountains/Tools/GUI/MMFaderDirectional")]
	public class MMFaderDirectional : MonoBehaviour, MMEventListener<MMFadeEvent>, MMEventListener<MMFadeInEvent>, MMEventListener<MMFadeOutEvent>, MMEventListener<MMFadeStopEvent>
	{
        /// 이 페이더가 움직일 수 있는 가능한 방향
        public enum Directions { TopToBottom, LeftToRight, RightToLeft, BottomToTop }

		[Header("Identification")]
		/// the ID for this fader (0 is default), set more IDs if you need more than one fader
		[Tooltip("이 페이더의 ID(기본값은 0), 하나 이상의 페이더가 필요한 경우 더 많은 ID를 설정하십시오.")]
		public int ID;

		[Header("Directional Fader")]
		/// the direction this fader should move in when fading in
		[Tooltip("페이드 인할 때 이 페이더가 움직여야 하는 방향")]
		public Directions FadeInDirection = Directions.LeftToRight;
		/// the direction this fader should move in when fading out
		[Tooltip("페이드 아웃 시 이 페이더가 움직여야 하는 방향")]
		public Directions FadeOutDirection = Directions.LeftToRight;
        
		[Header("Timing")]
		/// the default duration of the fade in/out
		[Tooltip("페이드 인/아웃의 기본 지속 시간")]
		public float DefaultDuration = 0.2f;
		/// the default curve to use for this fader
		[Tooltip("이 페이더에 사용할 기본 곡선")]
		public MMTweenType DefaultTween = new MMTweenType(MMTween.MMTweenCurve.LinearTween);
		/// whether or not the fade should happen in unscaled time 
		[Tooltip("스케일링되지 않은 시간에 페이드가 발생해야 하는지 여부")]
		public bool IgnoreTimescale = true;
		/// whether or not to automatically disable this fader on init
		[Tooltip("초기화 시 이 페이더를 자동으로 비활성화할지 여부")]
		public bool DisableOnInit = true;

		[Header("Delay")]
		/// a delay (in seconds) to apply before playing this fade
		[Tooltip("이 페이드를 재생하기 전에 적용할 지연(초)")]
		public float InitialDelay = 0f;

		[Header("Interaction")]
		/// whether or not the fader should block raycasts when visible
		[Tooltip("페이더가 보일 때 레이캐스트를 차단해야 하는지 여부")]
		public bool ShouldBlockRaycasts = false; 

		/// the width of the fader
		public virtual float Width { get { return _rectTransform.rect.width; } }
		/// the height of the fader
		public virtual float Height { get { return _rectTransform.rect.height; } }

		[Header("Debug")]
		[MMInspectorButton("FadeIn1Second")]
		public bool FadeIn1SecondButton;
		[MMInspectorButton("FadeOut1Second")]
		public bool FadeOut1SecondButton;
		[MMInspectorButton("DefaultFade")]
		public bool DefaultFadeButton;
		[MMInspectorButton("ResetFader")]
		public bool ResetFaderButton;

		protected RectTransform _rectTransform;
		protected CanvasGroup _canvasGroup;
		protected float _currentDuration;
		protected MMTweenType _currentCurve;
		protected bool _fading = false;
		protected float _fadeStartedAt;
		protected Vector2 _initialPosition;

		protected Vector2 _fromPosition;
		protected Vector2 _toPosition;
		protected Vector2 _newPosition;
		protected bool _active;
		protected bool _initialized = false;

		/// <summary>
		/// Test method triggered by an inspector button
		/// </summary>
		protected virtual void ResetFader()
		{
			_rectTransform.anchoredPosition = _initialPosition;
		}

		/// <summary>
		/// Test method triggered by an inspector button
		/// </summary>
		protected virtual void DefaultFade()
		{
			MMFadeEvent.Trigger(DefaultDuration, 1f, DefaultTween, ID, IgnoreTimescale, this.transform.position);
		}

		/// <summary>
		/// Test method triggered by an inspector button
		/// </summary>
		protected virtual void FadeIn1Second()
		{
			MMFadeInEvent.Trigger(1f, DefaultTween, ID, IgnoreTimescale, this.transform.position);
		}

		/// <summary>
		/// Test method triggered by an inspector button
		/// </summary>
		protected virtual void FadeOut1Second()
		{
			MMFadeOutEvent.Trigger(1f, DefaultTween, ID, IgnoreTimescale, this.transform.position);
		}

		/// <summary>
		/// On Start, we initialize our fader
		/// </summary>
		protected virtual void Start()
		{
			Initialization();
		}

		/// <summary>
		/// On init, we grab our components, and disable/hide everything
		/// </summary>
		//protected virtual IEnumerator Initialization()
		protected virtual void Initialization()
		{
			_canvasGroup = this.gameObject.GetComponent<CanvasGroup>();
			_rectTransform = this.gameObject.GetComponent<RectTransform>();
			_initialPosition = _rectTransform.anchoredPosition;
			if (DisableOnInit)
			{
				DisableFader();
			}
			_initialized = true;
		}

		/// <summary>
		/// On Update, we update our alpha 
		/// </summary>
		protected virtual void Update()
		{
			if (_canvasGroup == null) { return; }

			if (_fading)
			{
				Fade();
			}
		}

		/// <summary>
		/// Fades the canvasgroup towards its target alpha
		/// </summary>
		protected virtual void Fade()
		{
			float currentTime = IgnoreTimescale ? Time.unscaledTime : Time.time;
			float endTime = _fadeStartedAt + _currentDuration;

			if (currentTime - _fadeStartedAt < _currentDuration)
			{
				_newPosition = MMTween.Tween(currentTime, _fadeStartedAt, endTime, _fromPosition, _toPosition, _currentCurve);
				_rectTransform.anchoredPosition = _newPosition;
			}
			else
			{
				StopFading();
			}
		}

		/// <summary>
		/// Stops the fading.
		/// </summary>
		protected virtual void StopFading()
		{
			_rectTransform.anchoredPosition = _toPosition;
			_fading = false;

			if (_initialPosition != _toPosition)
			{
				DisableFader();
			}
		}

		/// <summary>
		/// Starts a fade
		/// </summary>
		/// <param name="fadingIn"></param>
		/// <param name="duration"></param>
		/// <param name="curve"></param>
		/// <param name="id"></param>
		/// <param name="ignoreTimeScale"></param>
		/// <param name="worldPosition"></param>
		protected virtual IEnumerator StartFading(bool fadingIn, float duration, MMTweenType curve, int id,
			bool ignoreTimeScale, Vector3 worldPosition)
		{
			if (id != ID)
			{
				yield break;
			}

			if (InitialDelay > 0f)
			{
				yield return MMCoroutine.WaitFor(InitialDelay);
			}

			if (!_initialized)
			{
				Initialization();
			}

			if (curve == null)
			{
				curve = DefaultTween;
			}
            
			IgnoreTimescale = ignoreTimeScale;
			EnableFader();
			_fading = true;

			_fadeStartedAt = IgnoreTimescale ? Time.unscaledTime : Time.time;
			_currentCurve = curve;
			_currentDuration = duration;

			_fromPosition = _rectTransform.anchoredPosition;
			_toPosition = fadingIn ? _initialPosition : ExitPosition();

			_newPosition = MMTween.Tween(0f, 0f, duration, _fromPosition, _toPosition, _currentCurve);
			_rectTransform.anchoredPosition = _newPosition;
		}

		/// <summary>
		/// Determines the position of the fader before entry
		/// </summary>
		/// <returns></returns>
		protected virtual Vector2 BeforeEntryPosition()
		{
			switch (FadeInDirection)
			{
				case Directions.BottomToTop:
					return _initialPosition + Vector2.down * Height;
				case Directions.LeftToRight:
					return _initialPosition + Vector2.left * Width;
				case Directions.RightToLeft:
					return _initialPosition + Vector2.right * Width;
				case Directions.TopToBottom:
					return _initialPosition + Vector2.up * Height;
			}
			return Vector2.zero;
		}

		/// <summary>
		/// Determines the exit position of the fader
		/// </summary>
		/// <returns></returns>
		protected virtual Vector2 ExitPosition()
		{
			switch (FadeOutDirection)
			{
				case Directions.BottomToTop:
					return _initialPosition + Vector2.up * Height;
				case Directions.LeftToRight:
					return _initialPosition + Vector2.right * Width;
				case Directions.RightToLeft:
					return _initialPosition + Vector2.left * Width;
				case Directions.TopToBottom:
					return _initialPosition + Vector2.down * Height;
			}
			return Vector2.zero;
		}

		/// <summary>
		/// Disables the fader.
		/// </summary>
		protected virtual void DisableFader()
		{
			if (ShouldBlockRaycasts)
			{
				_canvasGroup.blocksRaycasts = false;
			}
			_active = false;
			_canvasGroup.alpha = 0;
			_rectTransform.anchoredPosition = BeforeEntryPosition();
			this.enabled = false;
		}

		/// <summary>
		/// Enables the fader.
		/// </summary>
		protected virtual void EnableFader()
		{
			this.enabled = true;
			if (ShouldBlockRaycasts)
			{
				_canvasGroup.blocksRaycasts = true;
			}
			_active = true;
			_canvasGroup.alpha = 1;
		}

		/// <summary>
		/// When catching a fade event, we fade our image in or out
		/// </summary>
		/// <param name="fadeEvent">Fade event.</param>
		public virtual void OnMMEvent(MMFadeEvent fadeEvent)
		{
			bool status = _active ? false : true;
			StartCoroutine(StartFading(status, fadeEvent.Duration, fadeEvent.Curve, fadeEvent.ID,
				fadeEvent.IgnoreTimeScale, fadeEvent.WorldPosition));
		}

		/// <summary>
		/// When catching an MMFadeInEvent, we fade our image in
		/// </summary>
		/// <param name="fadeEvent">Fade event.</param>
		public virtual void OnMMEvent(MMFadeInEvent fadeEvent)
		{
			StartCoroutine(StartFading(true, fadeEvent.Duration, fadeEvent.Curve, fadeEvent.ID,
				fadeEvent.IgnoreTimeScale, fadeEvent.WorldPosition));
		}

		/// <summary>
		/// When catching an MMFadeOutEvent, we fade our image out
		/// </summary>
		/// <param name="fadeEvent">Fade event.</param>
		public virtual void OnMMEvent(MMFadeOutEvent fadeEvent)
		{
			StartCoroutine(StartFading(false, fadeEvent.Duration, fadeEvent.Curve, fadeEvent.ID,
				fadeEvent.IgnoreTimeScale, fadeEvent.WorldPosition));
		}

		/// <summary>
		/// When catching an MMFadeStopEvent, we stop our fade
		/// </summary>
		/// <param name="fadeEvent">Fade event.</param>
		public virtual void OnMMEvent(MMFadeStopEvent fadeStopEvent)
		{
			if (fadeStopEvent.ID == ID)
			{
				_fading = false;
				if (fadeStopEvent.Restore)
				{
					_rectTransform.anchoredPosition = _initialPosition;
				}
			}
		}

		/// <summary>
		/// On enable, we start listening to events
		/// </summary>
		protected virtual void OnEnable()
		{
			this.MMEventStartListening<MMFadeEvent>();
			this.MMEventStartListening<MMFadeStopEvent>();
			this.MMEventStartListening<MMFadeInEvent>();
			this.MMEventStartListening<MMFadeOutEvent>();
		}

		/// <summary>
		/// On disable, we stop listening to events
		/// </summary>
		protected virtual void OnDestroy()
		{
			this.MMEventStopListening<MMFadeEvent>();
			this.MMEventStopListening<MMFadeStopEvent>();
			this.MMEventStopListening<MMFadeInEvent>();
			this.MMEventStopListening<MMFadeOutEvent>();
		}
	}
}