using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

namespace MoreMountains.Tools
{
    /// <summary>
    /// 스와이프로 가능한 방향
    /// </summary>
    public enum MMPossibleSwipeDirections { Up, Down, Left, Right }


	[System.Serializable]
	public class SwipeEvent : UnityEvent<MMSwipeEvent> {}

    /// <summary>
    /// 일반적으로 스와이프가 발생할 때 트리거되는 이벤트입니다. 여기에는 스와이프 "기본" 방향과 필요한 경우 자세한 정보(각도, 길이, 출발지 및 목적지)가 포함되어 있습니다.
    /// </summary>
    public struct MMSwipeEvent
	{
		public MMPossibleSwipeDirections SwipeDirection;
		public float SwipeAngle;
		public float SwipeLength;
		public Vector2 SwipeOrigin;
		public Vector2 SwipeDestination;
		public float SwipeDuration;

		/// <summary>
		/// Initializes a new instance of the <see cref="MoreMountains.Tools.MMSwipeEvent"/> struct.
		/// </summary>
		/// <param name="direction">Direction.</param>
		/// <param name="angle">Angle.</param>
		/// <param name="length">Length.</param>
		/// <param name="origin">Origin.</param>
		/// <param name="destination">Destination.</param>
		public MMSwipeEvent(MMPossibleSwipeDirections direction, float angle, float length, Vector2 origin, Vector2 destination, float swipeDuration)
		{
			SwipeDirection = direction;
			SwipeAngle = angle;
			SwipeLength = length;
			SwipeOrigin = origin;
			SwipeDestination = destination;
			SwipeDuration = swipeDuration;
		}

		static MMSwipeEvent e;
		public static void Trigger(MMPossibleSwipeDirections direction, float angle, float length, Vector2 origin, Vector2 destination, float swipeDuration)
		{
			e.SwipeDirection = direction;
			e.SwipeAngle = angle;
			e.SwipeLength = length;
			e.SwipeOrigin = origin;
			e.SwipeDestination = destination;
			e.SwipeDuration = swipeDuration;
			MMEventManager.TriggerEvent(e);
		}
	}

    /// <summary>
    /// 스와이프 관리자를 장면에 추가하면 스와이프가 발생할 때마다 MMSwipeEvents가 트리거됩니다. 검사기에서 스와이프의 최소 길이를 결정할 수 있습니다. 더 짧은 스와이프는 무시됩니다.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
	[AddComponentMenu("More Mountains/Tools/Controls/MMSwipeZone")]
	public class MMSwipeZone : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IPointerEnterHandler
	{
		/// the minimal length of a swipe
		[Tooltip("스와이프의 최소 길이")]
		public float MinimalSwipeLength = 50f;
		/// the maximum press length of a swipe
		[Tooltip("t스와이프의 최대 누름 길이")]
		public float MaximumPressLength = 10f;

		/// The method(s) to call when the zone is swiped
		[Tooltip("영역을 스와이프할 때 호출할 메서드")]
		public SwipeEvent ZoneSwiped;
		/// The method(s) to call while the zone is being pressed
		[Tooltip("영역을 누르고 있는 동안 호출할 메서드")]
		public UnityEvent ZonePressed;

		[Header("Mouse Mode")]
		[MMInformation("이를 true로 설정하면 실제로 버튼을 눌러야 트리거됩니다. 그렇지 않으면 간단한 호버만으로 트리거됩니다(터치 입력에 더 좋음).", MMInformationAttribute.InformationType.Info,false)]
		/// If you set this to true, you'll need to actually press the button for it to be triggered, otherwise a simple hover will trigger it (better for touch input).
		[Tooltip("이를 true로 설정하면 실제로 버튼을 눌러야 트리거됩니다. 그렇지 않으면 간단한 호버만으로 트리거됩니다(터치 입력에 더 좋음).")]
		public bool MouseMode = false;

		protected Vector2 _firstTouchPosition;
		protected float _angle;
		protected float _length;
		protected Vector2 _destination;
		protected Vector2 _deltaSwipe;
		protected MMPossibleSwipeDirections _swipeDirection;
		protected float _lastPointerUpAt = 0f;
		protected float _swipeStartedAt = 0f;
		protected float _swipeEndedAt = 0f;

		/// <summary>
		/// Invokes a swipe event with the correct properties
		/// </summary>
		protected virtual void Swipe()
		{
			float duration = _swipeEndedAt - _swipeStartedAt;
			MMSwipeEvent swipeEvent = new MMSwipeEvent (_swipeDirection, _angle, _length, _firstTouchPosition, _destination, duration);
			MMEventManager.TriggerEvent(swipeEvent);
			if (ZoneSwiped != null)
			{
				ZoneSwiped.Invoke (swipeEvent);
			}
		}

		/// <summary>
		/// Raises the press event
		/// </summary>
		protected virtual void Press()
		{
			if (ZonePressed != null)
			{
				ZonePressed.Invoke ();
			}
		}

		/// <summary>
		/// Triggers the bound pointer down action
		/// </summary>
		public virtual void OnPointerDown(PointerEventData data)
		{
			#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
			_firstTouchPosition = Mouse.current.position.ReadValue();
			#else
			_firstTouchPosition = Input.mousePosition;
			#endif
			_swipeStartedAt = Time.unscaledTime;
		}

		/// <summary>
		/// Triggers the bound pointer up action
		/// </summary>
		public virtual void OnPointerUp(PointerEventData data)
		{
			if (Time.frameCount == _lastPointerUpAt)
			{
				return;
			}

			#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            _destination = Mouse.current.position.ReadValue();
			#else
			_destination = Input.mousePosition;
			#endif
			_deltaSwipe = _destination - _firstTouchPosition;
			_length = _deltaSwipe.magnitude;

			// if the swipe has been long enough
			if (_length > MinimalSwipeLength)
			{
				_angle = MMMaths.AngleBetween (_deltaSwipe, Vector2.right);
				_swipeDirection = AngleToSwipeDirection (_angle);
				_swipeEndedAt = Time.unscaledTime;
				Swipe ();
			}

			// if it's just a press
			if (_deltaSwipe.magnitude < MaximumPressLength)
			{
				Press ();
			}

			_lastPointerUpAt = Time.frameCount;
		}

		/// <summary>
		/// Triggers the bound pointer enter action when touch enters zone
		/// </summary>
		public virtual void OnPointerEnter(PointerEventData data)
		{
			if (!MouseMode)
			{
				OnPointerDown (data);
			}
		}

		/// <summary>
		/// Triggers the bound pointer exit action when touch is out of zone
		/// </summary>
		public virtual void OnPointerExit(PointerEventData data)
		{
			if (!MouseMode)
			{
				OnPointerUp(data);	
			}
		}

		/// <summary>
		/// Determines a MMPossibleSwipeDirection out of an angle in degrees. 
		/// </summary>
		/// <returns>The to swipe direction.</returns>
		/// <param name="angle">Angle in degrees.</param>
		protected virtual MMPossibleSwipeDirections AngleToSwipeDirection(float angle)
		{
			if ((angle < 45) || (angle >= 315))
			{
				return MMPossibleSwipeDirections.Right;
			}
			if ((angle >= 45) && (angle < 135))
			{
				return MMPossibleSwipeDirections.Up;
			}
			if ((angle >= 135) && (angle < 225))
			{
				return MMPossibleSwipeDirections.Left;
			}
			if ((angle >= 225) && (angle < 315))
			{
				return MMPossibleSwipeDirections.Down;
			}
			return MMPossibleSwipeDirections.Right;
		}
	}
}