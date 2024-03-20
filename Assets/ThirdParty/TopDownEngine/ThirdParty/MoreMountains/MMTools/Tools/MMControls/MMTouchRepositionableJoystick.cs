using UnityEngine;
using UnityEngine.EventSystems;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MoreMountains.Tools
{
    /// <summary>
    /// 이 구성 요소를 UI 직사각형에 추가하면 조이스틱의 감지 영역 역할을 합니다.
    /// 이 구성 요소는 MMTouchJoystick 클래스를 확장하므로 다른 조이스틱을 추가할 필요가 없습니다. 감지 영역이자 스틱 자체입니다.
    /// </summary>
    [AddComponentMenu("More Mountains/Tools/Controls/MMTouchRepositionableJoystick")]
	public class MMTouchRepositionableJoystick : MMTouchJoystick, IPointerDownHandler
	{
		[MMInspectorGroup("Repositionable Joystick", true, 22)]
		/// the canvas group to use as the joystick's knob
		[Tooltip("the canvas group to use as the joystick's knob")]
		public CanvasGroup KnobCanvasGroup;
		/// the canvas group to use as the joystick's background
		[Tooltip("the canvas group to use as the joystick's background")]
		public CanvasGroup BackgroundCanvasGroup;
		/// if this is true, the joystick won't be able to travel beyond the bounds of the top level canvas
		[Tooltip("if this is true, the joystick won't be able to travel beyond the bounds of the top level canvas")]
		public bool ConstrainToInitialRectangle = true;
		/// if this is true, the joystick will return back to its initial position when released
		[Tooltip("if this is true, the joystick will return back to its initial position when released")]
		public bool ResetPositionToInitialOnRelease = false;

		protected Vector3 _initialPosition;
		protected Vector3 _newPosition;
		protected CanvasGroup _knobCanvasGroup;
		protected RectTransform _rectTransform;

		/// <summary>
		/// On Start, we instantiate our joystick's image if there's one
		/// </summary>
		protected override void Start()
		{
			base.Start();

			// we store the detection zone's initial position
			_rectTransform = GetComponent<RectTransform>();
			_initialPosition = _rectTransform.position;
		}

		/// <summary>
		/// On init we set our knob transform
		/// </summary>
		public override void Initialize()
		{
			base.Initialize();
			SetKnobTransform(KnobCanvasGroup.transform);
			_canvasGroup = KnobCanvasGroup;
			_initialOpacity = _canvasGroup.alpha;
		}

		/// <summary>
		/// When the zone is pressed, we move our joystick accordingly
		/// </summary>
		/// <param name="data">Data.</param>
		public override void OnPointerDown(PointerEventData data)
		{
			base.OnPointerDown(data);
			
			// if we're in "screen space - camera" render mode
			if (ParentCanvasRenderMode == RenderMode.ScreenSpaceCamera)
			{
				_newPosition = TargetCamera.ScreenToWorldPoint(data.position);
			}
			// otherwise
			else
			{
				_newPosition = data.position;
			}
			_newPosition.z = this.transform.position.z;
			
			if (!WithinBounds())
			{
				return;
			}

			// we define a new neutral position
			BackgroundCanvasGroup.transform.position = _newPosition;
			SetNeutralPosition(_newPosition);
			_knobTransform.position = _newPosition;
		}

		/// <summary>
		/// Returns true if the joystick's new position is within the bounds of the top level canvas
		/// </summary>
		/// <returns></returns>
		protected virtual bool WithinBounds()
		{
			if (!ConstrainToInitialRectangle)
			{
				return true;
			}
			return RectTransformUtility.RectangleContainsScreenPoint(_rectTransform, _newPosition);
		}

		/// <summary>
		/// When the player lets go of the stick, we restore our stick's position if needed
		/// </summary>
		/// <param name="eventData">Event data.</param>
		public override void OnPointerUp(PointerEventData eventData)
		{
			base.OnPointerUp(eventData);

			if (ResetPositionToInitialOnRelease)
			{
				BackgroundCanvasGroup.transform.position = _initialPosition;
				_knobTransform.position = _initialPosition;
			}
		}
		
		
		#if UNITY_EDITOR
		/// <summary>
		/// Draws gizmos if needed
		/// </summary>
		protected override void OnDrawGizmos()
		{
			if (!DrawGizmos)
			{
				return;
			}

			Handles.color = MMColors.Orange;
			if (!Application.isPlaying)
			{
				if (KnobCanvasGroup != null)
				{
					Handles.DrawWireDisc(KnobCanvasGroup.transform.position, Vector3.forward, ComputedMaxRange);	
				}
				else
				{
					Handles.DrawWireDisc(this.transform.position, Vector3.forward, ComputedMaxRange);	
				}
			}
			else
			{
				Handles.DrawWireDisc(_neutralPosition, Vector3.forward, ComputedMaxRange);
			}
		}
		#endif
	}
}