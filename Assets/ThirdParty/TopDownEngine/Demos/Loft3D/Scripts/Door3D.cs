using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// Loft 데모에서 3D 문을 처리하는 클래스
    /// </summary>
    public class Door3D : TopDownMonoBehaviour
	{
		[Header("Angles")]

		/// the min angle the door can open at
		[Tooltip("문이 열릴 수 있는 최소 각도")]
		public float MinAngle = 90f;
		/// the max angle the door can open at
		[Tooltip("문이 열릴 수 있는 최대 각도")]
		public float MaxAngle = 270f;
		/// the min angle at which the door locks when open
		[Tooltip("문이 열렸을 때 문이 잠기는 최소 각도")]
		public float MinAngleLock = 90f;
		/// the max angle at which the door locks when open
		[Tooltip("문이 열렸을 때 문이 잠기는 최대 각도")]
		public float MaxAngleLock = 270f;
		[Header("Safe Lock")]
		/// the duration of the "safe lock", a period during which the door is set to kinematic, to prevent glitches. That period ends after that safe lock duration, once the player has exited the door's area
		[Tooltip("결함을 방지하기 위해 도어가 운동학적으로 설정되는 기간인 '안전 잠금' 기간. 해당 기간은 플레이어가 문 영역을 벗어나면 안전 잠금 기간 이후 종료됩니다.")]
		public float SafeLockDuration = 1f;

		[Header("Binding")]

		/// the rigidbody associated to this door
		[Tooltip("the rigidbody associated to this door")]
		public Rigidbody Door;

		protected Vector3 _eulerAngles;
		protected Vector3 _initialPosition;
		protected Vector2 _initialDirection;
		protected Vector2 _currentDirection;
		protected float _lastContactTimestamp;
		protected Vector3 _minAngleRotation;
		protected Vector3 _maxAngleRotation;

		/// <summary>
		/// On start we compute our initial direction and rotation
		/// </summary>
		protected virtual void Start()
		{
			Door = Door.gameObject.GetComponent<Rigidbody>();
			_initialDirection.x = Door.transform.right.x;
			_initialDirection.y = Door.transform.right.z;

			_minAngleRotation = Vector3.zero;
			_minAngleRotation.y = MinAngleLock;
			_maxAngleRotation = Vector3.zero;
			_maxAngleRotation.y = MaxAngleLock;

			_initialPosition = Door.transform.position;
		}

		/// <summary>
		/// On Update we we lock the door if needed
		/// </summary>
		protected virtual void Update()
		{
			_currentDirection.x = Door.transform.right.x;
			_currentDirection.y = Door.transform.right.z;
			float Angle = MMMaths.AngleBetween(_initialDirection, _currentDirection);

			if ((Angle > MinAngle) && (Angle < MaxAngle) && (!Door.isKinematic))
			{
				if (Angle > 180)
				{
					Door.transform.localRotation = Quaternion.Euler(_maxAngleRotation);
				}
				else
				{
					Door.transform.localRotation = Quaternion.Euler(_minAngleRotation);
				}
				Door.transform.position = _initialPosition;
				Door.collisionDetectionMode = CollisionDetectionMode.Discrete;
				Door.isKinematic = true;
			}

			// if enough time has passed we reset our door
			if ((Time.time - _lastContactTimestamp > SafeLockDuration) && (Door.isKinematic))
			{
				Door.isKinematic = false;
			}
		}

		/// <summary>
		/// While we're colliding with something, we store the timestamp for future use
		/// </summary>
		/// <param name="collider"></param>
		protected virtual void OnTriggerStay(Collider collider)
		{
			_lastContactTimestamp = Time.time;
		}
	}
}