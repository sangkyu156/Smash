using UnityEngine;
using System.Collections;
using UnityEngine.Events;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 대상 주위의 카메라 궤도를 만드는 데 사용되는 클래스
    /// </summary>
    [AddComponentMenu("More Mountains/Tools/Camera/MMOrbitalCamera")]
	public class MMOrbitalCamera : MonoBehaviour
	{
        /// 이 카메라에 가능한 입력 모드
        public enum Modes { Mouse, Touch }

		[Header("Setup")]
        /// 선택한 입력 모드
        public Modes Mode = Modes.Touch;
        /// 주위를 공전할 물체
        public Transform Target;
        /// 궤도를 선회하는 동안 적용할 오프셋
        public Vector3 TargetOffset;
        /// 현재 목표까지의 거리
        [MMReadOnly]        
		public float DistanceToTarget = 5f;

		[Header("Rotation")]
        /// 회전 활성화 여부
        public bool RotationEnabled = true;
        /// 회전 속도
        public Vector2 RotationSpeed = new Vector2(200f, 200f);
        /// 최소 수직 각도 제한
        public int MinVerticalAngleLimit = -80;
        /// 최대 수직 각도 제한
        public int MaxVerticalAngleLimit = 80;

		[Header("Zoom")]
        /// 확대/축소 활성화 여부
        public bool ZoomEnabled = true;
        /// 사용자가 확대할 수 있는 최소 거리
        public float MinimumZoomDistance = 0.6f;
        /// 사용자가 축소할 수 있는 최대 거리
        public float MaximumZoomDistance = 20;
        /// 줌 보간 속도
        public int ZoomSpeed = 40;
        /// 확대/축소에 적용할 감쇠
        public float ZoomDampening = 5f;

		[Header("Mouse Zoom")]
        /// 마우스 휠을 스크롤하여 확대/축소되는 속도
        public float MouseWheelSpeed = 10f;
        /// 마우스 휠을 고정할 최대 값
        public float MaxMouseWheelClamp = 10f;

		[Header("Steps")]
        /// 단계를 트리거하기까지의 거리
        public float StepThreshold = 1;
        /// 단계가 충족될 때 트리거되는 이벤트
        public UnityEvent StepFeedback;

		protected float _angleX = 0f;
		protected float _angleY = 0f;
		protected float _currentDistance;
		protected float _desiredDistance;
		protected Quaternion _currentRotation;
		protected Quaternion _desiredRotation;
		protected Quaternion _rotation;
		protected Vector3 _position;
		protected float _scrollWheelAmount = 0;
		protected float _stepBuffer = 0f;

		/// <summary>
		/// On Start we initialize our orbital camera
		/// </summary>
		protected virtual void Start()
		{
			Initialization();
		}

		/// <summary>
		/// On init we store our positions and rotations
		/// </summary>
		public virtual void Initialization()
		{
			// if no target is set, we throw an error and exit
			if (Target == null)
			{
				Debug.LogError(this.gameObject.name + " : the MMOrbitalCamera doesn't have a target.");
				return;
			}

			DistanceToTarget = Vector3.Distance(Target.position, transform.position);
			_currentDistance = DistanceToTarget;
			_desiredDistance = DistanceToTarget;

			_position = transform.position;
			_rotation = transform.rotation;
			_currentRotation = transform.rotation;
			_desiredRotation = transform.rotation;

			_angleX = Vector3.Angle(Vector3.right, transform.right);
			_angleY = Vector3.Angle(Vector3.up, transform.up);
		}

		/// <summary>
		/// On late update we rotate, zoom, detect steps and finally apply our movement
		/// </summary>
		protected virtual void LateUpdate()
		{
			if (Target == null)
			{
				return;
			}

			Rotation();
			Zoom();
			StepDetection();
			ApplyMovement();
		}

		/// <summary>
		/// Rotates the camera around the object
		/// </summary>
		protected virtual void Rotation()
		{
			if (!RotationEnabled)
			{
				return;
			}

			if (Mode == Modes.Touch && (Input.touchCount > 0))
			{
				if ((Input.touches[0].phase == TouchPhase.Moved) && (Input.touchCount == 1))
				{
					float screenHeight = Screen.currentResolution.height;
					if (Input.touches[0].position.y < screenHeight/4)
					{
						return;
					}

					float swipeSpeed = Input.touches[0].deltaPosition.magnitude / Input.touches[0].deltaTime;

					_angleX += Input.touches[0].deltaPosition.x * RotationSpeed.x * Time.deltaTime * swipeSpeed * 0.00001f;
					_angleY -= Input.touches[0].deltaPosition.y * RotationSpeed.y * Time.deltaTime * swipeSpeed * 0.00001f;
					_stepBuffer += Input.touches[0].deltaPosition.x;

					_angleY = MMMaths.ClampAngle(_angleY, MinVerticalAngleLimit, MaxVerticalAngleLimit);
					_desiredRotation = Quaternion.Euler(_angleY, _angleX, 0);
					_currentRotation = transform.rotation;

					_rotation = Quaternion.Lerp(_currentRotation, _desiredRotation, Time.deltaTime * ZoomDampening);
					transform.rotation = _rotation;
				}
				else if (Input.touchCount == 1 && Input.touches[0].phase == TouchPhase.Began)
				{
					_desiredRotation = transform.rotation;
				}

				if (transform.rotation != _desiredRotation)
				{
					_rotation = Quaternion.Lerp(transform.rotation, _desiredRotation, Time.deltaTime * ZoomDampening);
					transform.rotation = _rotation;
				}
			}
			else if (Mode == Modes.Mouse)
			{
				_angleX += Input.GetAxis("Mouse X") * RotationSpeed.x * Time.deltaTime;
				_angleY += -Input.GetAxis("Mouse Y") * RotationSpeed.y * Time.deltaTime;
				_angleY = Mathf.Clamp(_angleY, MinVerticalAngleLimit, MaxVerticalAngleLimit);

				_desiredRotation = Quaternion.Euler(new Vector3(_angleY, _angleX, 0));
				_currentRotation = transform.rotation;
				_rotation = Quaternion.Lerp(_currentRotation, _desiredRotation, Time.deltaTime * ZoomDampening);
				transform.rotation = _rotation;
			}            
		}

		/// <summary>
		/// Detects steps 
		/// </summary>
		protected virtual void StepDetection()
		{
			if (Mathf.Abs(_stepBuffer) > StepThreshold)
			{
				StepFeedback?.Invoke();
				_stepBuffer = 0f;
			}
		}

		/// <summary>
		/// Zooms
		/// </summary>
		protected virtual void Zoom()
		{
			if (!ZoomEnabled)
			{
				return;
			}

			if (Mode == Modes.Touch && (Input.touchCount > 0))
			{
				if (Input.touchCount == 2)
				{
					Touch firstTouch = Input.GetTouch(0);
					Touch secondTouch = Input.GetTouch(1);

					Vector2 firstTouchPreviousPosition = firstTouch.position - firstTouch.deltaPosition;
					Vector2 secondTouchPreviousPosition = secondTouch.position - secondTouch.deltaPosition;

					float previousTouchDeltaMagnitude = (firstTouchPreviousPosition - secondTouchPreviousPosition).magnitude;
					float thisTouchDeltaMagnitude = (firstTouch.position - secondTouch.position).magnitude;
					float deltaMagnitudeDifference = previousTouchDeltaMagnitude - thisTouchDeltaMagnitude;

					_desiredDistance += deltaMagnitudeDifference * Time.deltaTime * ZoomSpeed * Mathf.Abs(_desiredDistance) * 0.001f;
					_desiredDistance = Mathf.Clamp(_desiredDistance, MinimumZoomDistance, MaximumZoomDistance);
					_currentDistance = Mathf.Lerp(_currentDistance, _desiredDistance, Time.deltaTime * ZoomDampening);
				}
			}
			else if (Mode == Modes.Mouse)
			{
				_scrollWheelAmount += - Input.GetAxis("Mouse ScrollWheel") * MouseWheelSpeed;
				_scrollWheelAmount = Mathf.Clamp(_scrollWheelAmount, -MaxMouseWheelClamp, MaxMouseWheelClamp);
                
				float deltaMagnitudeDifference = _scrollWheelAmount;

				_desiredDistance += deltaMagnitudeDifference * Time.deltaTime * ZoomSpeed * Mathf.Abs(_desiredDistance) * 0.001f;
				_desiredDistance = Mathf.Clamp(_desiredDistance, MinimumZoomDistance, MaximumZoomDistance);
				_currentDistance = Mathf.Lerp(_currentDistance, _desiredDistance, Time.deltaTime * ZoomDampening);

			}
		}

		/// <summary>
		/// Moves the transform
		/// </summary>
		protected virtual void ApplyMovement()
		{
			_position = Target.position - (_rotation * Vector3.forward * _currentDistance + TargetOffset);
			transform.position = _position;
		}
	}
}