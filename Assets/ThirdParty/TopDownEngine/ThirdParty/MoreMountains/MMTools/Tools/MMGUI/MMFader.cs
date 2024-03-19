using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using MoreMountains.Tools;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 페이드를 멈추는 데 사용되는 이벤트
    /// </summary>
    public struct MMFadeStopEvent
	{
		/// an ID that has to match the one on the fader
		public int ID;
		public bool Restore;
        
		public MMFadeStopEvent(int id = 0, bool restore = false)
		{
			Restore = restore;
			ID = id;
		}
		static MMFadeStopEvent e;
		public static void Trigger(int id = 0, bool restore = false)
		{
			e.ID = id;
			e.Restore = restore;
			MMEventManager.TriggerEvent(e);
		}
	}

    /// <summary>
    /// 페이더를 켜거나 끄는 데 사용되는 이벤트
    /// </summary>
    public struct MMFadeEvent
	{
		/// an ID that has to match the one on the fader
		public int ID;
		/// the duration of the fade, in seconds
		public float Duration;
		/// the alpha to aim for
		public float TargetAlpha;
		/// the curve to apply to the fade
		public MMTweenType Curve;
		/// whether or not this fade should ignore timescale
		public bool IgnoreTimeScale;
		/// a world position for a target object. Useless for regular fades, but can be useful for alt implementations (circle fade for example)
		public Vector3 WorldPosition;


		/// <summary>
		/// Initializes a new instance of the <see cref="MoreMountains.MMInterface.MMFadeEvent"/> struct.
		/// </summary>
		/// <param name="duration">Duration, in seconds.</param>
		/// <param name="targetAlpha">Target alpha, from 0 to 1.</param>
		public MMFadeEvent(float duration, float targetAlpha, MMTweenType tween, int id=0, 
			bool ignoreTimeScale = true, Vector3 worldPosition = new Vector3())
		{
			ID = id;
			Duration = duration;
			TargetAlpha = targetAlpha;
			Curve = tween;
			IgnoreTimeScale = ignoreTimeScale;
			WorldPosition = worldPosition;
		}
		static MMFadeEvent e;
		public static void Trigger(float duration, float targetAlpha)
		{
			Trigger(duration, targetAlpha, new MMTweenType(MMTween.MMTweenCurve.EaseInCubic));
		}
		public static void Trigger(float duration, float targetAlpha, MMTweenType tween, int id = 0, 
			bool ignoreTimeScale = true, Vector3 worldPosition = new Vector3())
		{
			e.ID = id;
			e.Duration = duration;
			e.TargetAlpha = targetAlpha;
			e.Curve = tween;
			e.IgnoreTimeScale = ignoreTimeScale;
			e.WorldPosition = worldPosition;
			MMEventManager.TriggerEvent(e);
		}
	}
     
	public struct MMFadeInEvent
	{
		/// an ID that has to match the one on the fader
		public int ID;
		/// the duration of the fade, in seconds
		public float Duration;
		/// the curve to apply to the fade
		public MMTweenType Curve;
		/// whether or not this fade should ignore timescale
		public bool IgnoreTimeScale;
		/// a world position for a target object. Useless for regular fades, but can be useful for alt implementations (circle fade for example)
		public Vector3 WorldPosition;

		/// <summary>
		/// Initializes a new instance of the <see cref="MoreMountains.MMInterface.MMFadeInEvent"/> struct.
		/// </summary>
		/// <param name="duration">Duration.</param>
		public MMFadeInEvent(float duration, MMTweenType tween, int id = 0, 
			bool ignoreTimeScale = true, Vector3 worldPosition = new Vector3())
		{
			ID = id;
			Duration = duration;
			Curve = tween;
			IgnoreTimeScale = ignoreTimeScale;
			WorldPosition = worldPosition;
		}
		static MMFadeInEvent e;
		public static void Trigger(float duration, MMTweenType tween, int id = 0, 
			bool ignoreTimeScale = true, Vector3 worldPosition = new Vector3())
		{
			e.ID = id;
			e.Duration = duration;
			e.Curve = tween;
			e.IgnoreTimeScale = ignoreTimeScale;
			e.WorldPosition = worldPosition;
			MMEventManager.TriggerEvent(e);
		}
	}

	public struct MMFadeOutEvent
	{
		/// an ID that has to match the one on the fader
		public int ID;
		/// the duration of the fade, in seconds
		public float Duration;
		/// the curve to apply to the fade
		public MMTweenType Curve;
		/// whether or not this fade should ignore timescale
		public bool IgnoreTimeScale;
		/// a world position for a target object. Useless for regular fades, but can be useful for alt implementations (circle fade for example)
		public Vector3 WorldPosition;

		/// <summary>
		/// Initializes a new instance of the <see cref="MoreMountains.MMInterface.MMFadeOutEvent"/> struct.
		/// </summary>
		/// <param name="duration">Duration.</param>
		public MMFadeOutEvent(float duration, MMTweenType tween, int id = 0, 
			bool ignoreTimeScale = true, Vector3 worldPosition = new Vector3())
		{
			ID = id;
			Duration = duration;
			Curve = tween;
			IgnoreTimeScale = ignoreTimeScale;
			WorldPosition = worldPosition;
		}

		static MMFadeOutEvent e;
		public static void Trigger(float duration, MMTweenType tween, int id = 0, 
			bool ignoreTimeScale = true, Vector3 worldPosition = new Vector3())
		{
			e.ID = id;
			e.Duration = duration;
			e.Curve = tween;
			e.IgnoreTimeScale = ignoreTimeScale;
			e.WorldPosition = worldPosition;
			MMEventManager.TriggerEvent(e);
		}
	}

    /// <summary>
    /// Fader 클래스는 이미지에 배치될 수 있으며 MMFadeEvents를 가로채서 그에 따라 자체적으로 켜거나 끌 것입니다.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
	[RequireComponent(typeof(Image))]
	[AddComponentMenu("More Mountains/Tools/GUI/MMFader")]
	public class MMFader : MonoBehaviour, MMEventListener<MMFadeEvent>, MMEventListener<MMFadeInEvent>, MMEventListener<MMFadeOutEvent>, MMEventListener<MMFadeStopEvent>
	{
		public enum ForcedInitStates { None, Active, Inactive }
        
		[Header("Identification")]
		/// the ID for this fader (0 is default), set more IDs if you need more than one fader
		[Tooltip("이 페이더의 ID(기본값은 0), 하나 이상의 페이더가 필요한 경우 더 많은 ID를 설정하십시오.")]
		public int ID;
        
		[Header("Opacity")]
		/// the opacity the fader should be at when inactive
		[Tooltip("비활성 상태일 때 페이더의 불투명도")]
		public float InactiveAlpha = 0f;
		/// the opacity the fader should be at when active
		[Tooltip("활성화되었을 때 페이더의 불투명도")]
		public float ActiveAlpha = 1f;
		/// determines whether a state should be forced on init
		[Tooltip("초기화 시 상태를 강제로 적용해야 하는지 여부를 결정합니다.")]
		public ForcedInitStates ForcedInitState = ForcedInitStates.Inactive;
        
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
		/// whether or not this fader can cause a fade if the requested final alpha is the same as the current one
		[Tooltip("요청된 최종 알파가 현재 알파와 동일한 경우 이 페이더가 페이드를 일으킬 수 있는지 여부")] 
		public bool CanFadeToCurrentAlpha = true;

		[Header("Interaction")]
		/// whether or not the fader should block raycasts when visible
		[Tooltip("페이더가 보일 때 레이캐스트를 차단해야 하는지 여부")]
		public bool ShouldBlockRaycasts = false;

		[Header("Debug")]
		[MMInspectorButton("FadeIn1Second")]
		public bool FadeIn1SecondButton;
		[MMInspectorButton("FadeOut1Second")]
		public bool FadeOut1SecondButton;
		[MMInspectorButton("DefaultFade")]
		public bool DefaultFadeButton;
		[MMInspectorButton("ResetFader")]
		public bool ResetFaderButton;

		protected CanvasGroup _canvasGroup;
		protected Image _image;

		protected float _initialAlpha;
		protected float _currentTargetAlpha;
		protected float _currentDuration;
		protected MMTweenType _currentCurve;

		protected bool _fading = false;
		protected float _fadeStartedAt;
		protected bool _frameCountOne;

		/// <summary>
		/// Test method triggered by an inspector button
		/// </summary>
		protected virtual void ResetFader()
		{
			_canvasGroup.alpha = InactiveAlpha;
		}

		/// <summary>
		/// Test method triggered by an inspector button
		/// </summary>
		protected virtual void DefaultFade()
		{
			MMFadeEvent.Trigger(DefaultDuration, ActiveAlpha, DefaultTween, ID);
		}

		/// <summary>
		/// Test method triggered by an inspector button
		/// </summary>
		protected virtual void FadeIn1Second()
		{
			MMFadeInEvent.Trigger(1f, new MMTweenType(MMTween.MMTweenCurve.LinearTween));
		}

		/// <summary>
		/// Test method triggered by an inspector button
		/// </summary>
		protected virtual void FadeOut1Second()
		{
			MMFadeOutEvent.Trigger(1f, new MMTweenType(MMTween.MMTweenCurve.LinearTween));
		}

		/// <summary>
		/// On Start, we initialize our fader
		/// </summary>
		protected virtual void Awake()
		{
			Initialization();
		}

		/// <summary>
		/// On init, we grab our components, and disable/hide everything
		/// </summary>
		protected virtual void Initialization()
		{
			_canvasGroup = GetComponent<CanvasGroup>();
			_image = GetComponent<Image>();

			if (ForcedInitState == ForcedInitStates.Inactive)
			{
				_canvasGroup.alpha = InactiveAlpha;    
				_image.enabled = false;
			}
			else if (ForcedInitState == ForcedInitStates.Active)
			{
				_canvasGroup.alpha = ActiveAlpha;    
				_image.enabled = true;
			}
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

			if (_frameCountOne)
			{
				if (Time.frameCount <= 2)
				{
					_canvasGroup.alpha = _initialAlpha;
					return;
				}
				_fadeStartedAt = IgnoreTimescale ? Time.unscaledTime : Time.time;
				currentTime = _fadeStartedAt;
				_frameCountOne = false;
			}
                        
			float endTime = _fadeStartedAt + _currentDuration;
			if (currentTime - _fadeStartedAt < _currentDuration)
			{
				float result = MMTween.Tween(currentTime, _fadeStartedAt, endTime, _initialAlpha, _currentTargetAlpha, _currentCurve);
				_canvasGroup.alpha = result;
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
			_canvasGroup.alpha = _currentTargetAlpha;
			_fading = false;
			if (_canvasGroup.alpha == InactiveAlpha)
			{
				DisableFader();
			}
		}

		/// <summary>
		/// Disables the fader.
		/// </summary>
		protected virtual void DisableFader()
		{
			_image.enabled = false;
			if (ShouldBlockRaycasts)
			{
				_canvasGroup.blocksRaycasts = false;
			}
		}

		/// <summary>
		/// Enables the fader.
		/// </summary>
		protected virtual void EnableFader()
		{
			_image.enabled = true;
			if (ShouldBlockRaycasts)
			{
				_canvasGroup.blocksRaycasts = true;
			}
		}

		/// <summary>
		/// Starts fading this fader from the specified initial alpha to the target
		/// </summary>
		/// <param name="initialAlpha"></param>
		/// <param name="endAlpha"></param>
		/// <param name="duration"></param>
		/// <param name="curve"></param>
		/// <param name="id"></param>
		/// <param name="ignoreTimeScale"></param>
		protected virtual void StartFading(float initialAlpha, float endAlpha, float duration, MMTweenType curve, int id, bool ignoreTimeScale)
		{
			if (id != ID)
			{
				return;
			}

			if ((!CanFadeToCurrentAlpha) && (_canvasGroup.alpha == endAlpha))
			{
				return;
			}
            
			IgnoreTimescale = ignoreTimeScale;
			EnableFader();
			_fading = true;
			_initialAlpha = initialAlpha;
			_currentTargetAlpha = endAlpha;
			_fadeStartedAt = IgnoreTimescale ? Time.unscaledTime : Time.time;
			_currentCurve = curve;
			_currentDuration = duration;
			if (Time.frameCount == 1)
			{
				_frameCountOne = true;
			}
		}

		/// <summary>
		/// When catching a fade event, we fade our image in or out
		/// </summary>
		/// <param name="fadeEvent">Fade event.</param>
		public virtual void OnMMEvent(MMFadeEvent fadeEvent)
		{
			_currentTargetAlpha = (fadeEvent.TargetAlpha == -1) ? ActiveAlpha : fadeEvent.TargetAlpha;
			StartFading(_canvasGroup.alpha, _currentTargetAlpha, fadeEvent.Duration, fadeEvent.Curve, fadeEvent.ID, fadeEvent.IgnoreTimeScale);
		}

		/// <summary>
		/// When catching an MMFadeInEvent, we fade our image in
		/// </summary>
		/// <param name="fadeEvent">Fade event.</param>
		public virtual void OnMMEvent(MMFadeInEvent fadeEvent)
		{
			StartFading(InactiveAlpha, ActiveAlpha, fadeEvent.Duration, fadeEvent.Curve, fadeEvent.ID, fadeEvent.IgnoreTimeScale);
		}

		/// <summary>
		/// When catching an MMFadeOutEvent, we fade our image out
		/// </summary>
		/// <param name="fadeEvent">Fade event.</param>
		public virtual void OnMMEvent(MMFadeOutEvent fadeEvent)
		{
			StartFading(ActiveAlpha, InactiveAlpha, fadeEvent.Duration, fadeEvent.Curve, fadeEvent.ID, fadeEvent.IgnoreTimeScale);
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
					_canvasGroup.alpha = _initialAlpha;
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
		protected virtual void OnDisable()
		{
			this.MMEventStopListening<MMFadeEvent>();
			this.MMEventStopListening<MMFadeStopEvent>();
			this.MMEventStopListening<MMFadeInEvent>();
			this.MMEventStopListening<MMFadeOutEvent>();
		}
	}
}