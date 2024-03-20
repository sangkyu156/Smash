using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 클래스를 직접 사용하지 마십시오. 2D 문자의 경우 TopDownController2D를 사용하거나 3D 문자의 경우 TopDownController3D를 사용하십시오. 
	/// 이 두 클래스는 모두 이 클래스에서 상속됩니다
    /// </summary>
    public class TopDownController : TopDownMonoBehaviour 
	{
		[Header("Gravity")]
		/// the current gravity to apply to our character (positive goes down, negative goes up, higher value, higher acceleration)
		[Tooltip("캐릭터에 적용할 현재 중력(양수는 낮아지고, 음수는 올라갑니다. 값이 높을수록 가속도도 높아집니다)")]
		public float Gravity = 40f;
		/// whether or not the gravity is currently being applied to this character
		[Tooltip("현재 이 캐릭터에 중력이 적용되고 있는지 여부")]
		public bool GravityActive = true;

		[Header("General Raycasts")]
		/// by default, the length of the raycasts used to get back to normal size will be auto generated based on your character's normal/standing height, but here you can specify a different value
		[Tooltip("기본적으로 일반 크기로 돌아가는 데 사용되는 레이캐스트의 길이는 캐릭터의 일반/서 있는 키를 기준으로 자동 생성되지만 여기서는 다른 값을 지정할 수 있습니다.")]
		public float CrouchedRaycastLengthMultiplier = 1f;
		/// if this is true, extra raycasts will be cast on all 4 sides to detect obstacles and feed the CollidingWithCardinalObstacle bool, only useful when working with grid movement, or if you need that info for some reason
		[Tooltip("이것이 사실이라면 장애물을 감지하고 CollidingWithCardinalObstacle bool을 제공하기 위해 추가 레이캐스트가 4개 측면 모두에 캐스팅됩니다. 이는 그리드 이동 작업을 할 때나 어떤 이유로든 해당 정보가 필요한 경우에만 유용합니다.")]
		public bool PerformCardinalObstacleRaycastDetection = false;

		/// the current speed of the character
		[MMReadOnly]
		[Tooltip("현재 캐릭터의 속도")]
		public Vector3 Speed;
		/// the current velocity
		[MMReadOnly]
		[Tooltip("현재 속도(단위/초)")]
		public Vector3 Velocity;
		/// the velocity of the character last frame
		[MMReadOnly]
		[Tooltip("캐릭터의 마지막 프레임 속도")]
		public Vector3 VelocityLastFrame;
		/// the current acceleration
		[MMReadOnly]
		[Tooltip("현재 가속도")]
		public Vector3 Acceleration;
		/// whether or not the character is grounded
		[MMReadOnly]
		[Tooltip("캐릭터가 접지되어 있는지 여부")]
		public bool Grounded;
		/// whether or not the character got grounded this frame
		[MMReadOnly]
		[Tooltip("캐릭터가 이 프레임에 접지되었는지 여부")]
		public bool JustGotGrounded;
		/// the current movement of the character
		[MMReadOnly]
		[Tooltip("현재 캐릭터의 움직임")]
		public Vector3 CurrentMovement;
		/// the direction the character is going in
		[MMReadOnly]
		[Tooltip("캐릭터가 가는 방향")]
		public Vector3 CurrentDirection;
		/// the current friction
		[MMReadOnly]
		[Tooltip("the current friction")]
		public float Friction;
		/// the current added force, to be added to the character's movement
		[MMReadOnly]
		[Tooltip("캐릭터의 움직임에 추가될 현재 추가된 힘")]
		public Vector3 AddedForce;
		/// whether or not the character is in free movement mode or not
		[MMReadOnly]
		[Tooltip("캐릭터가 자유 이동 모드에 있는지 여부")]
		public bool FreeMovement = true;
        
		/// the collider's center coordinates
		public virtual Vector3 ColliderCenter { get { return Vector3.zero; }  }
		/// the collider's bottom coordinates
		public virtual Vector3 ColliderBottom { get { return Vector3.zero; }  }
		/// the collider's top coordinates
		public virtual Vector3 ColliderTop { get { return Vector3.zero; }  }
		/// the object (if any) below our character
		public GameObject ObjectBelow { get; set; }
		/// the surface modifier object below our character (if any)
		public SurfaceModifier SurfaceModifierBelow { get; set; }
		public virtual Vector3 AppliedImpact { get { return _impact; } }
		/// whether or not the character is on a moving platform
		public virtual bool OnAMovingPlatform { get; set; }
		/// the speed of the moving platform
		public virtual Vector3 MovingPlatformSpeed { get; set; }

		// the obstacle left to this controller (only updated if DetectObstacles is called)
		public GameObject DetectedObstacleLeft { get; set; }
		// the obstacle right to this controller (only updated if DetectObstacles is called)
		public GameObject DetectedObstacleRight { get; set; }
		// the obstacle up to this controller (only updated if DetectObstacles is called)
		public GameObject DetectedObstacleUp { get; set; }
		// the obstacle down to this controller (only updated if DetectObstacles is called)
		public GameObject DetectedObstacleDown { get; set; }
		// true if an obstacle was detected in any of the cardinal directions
		public bool CollidingWithCardinalObstacle { get; set; }

		protected Vector3 _positionLastFrame;
		protected Vector3 _speedComputation;
		protected bool _groundedLastFrame;
		protected Vector3 _impact;		
		protected const float _smallValue=0.0001f;

		/// <summary>
		/// On awake, we initialize our current direction
		/// </summary>
		protected virtual void Awake()
		{			
			CurrentDirection = transform.forward;
		}

		/// <summary>
		/// On update, we check if we're grounded, and determine the direction
		/// </summary>
		protected virtual void Update()
		{
			CheckIfGrounded ();
			HandleFriction ();
			DetermineDirection ();
		}

		/// <summary>
		/// Computes the speed
		/// </summary>
		protected virtual void ComputeSpeed ()
		{
			if (Time.deltaTime != 0f)
			{
				Speed = (this.transform.position - _positionLastFrame) / Time.deltaTime;
			}			
			// we round the speed to 2 decimals
			Speed.x = Mathf.Round(Speed.x * 100f) / 100f;
			Speed.y = Mathf.Round(Speed.y * 100f) / 100f;
			Speed.z = Mathf.Round(Speed.z * 100f) / 100f;
			_positionLastFrame = this.transform.position;
		}

		/// <summary>
		/// Determines the controller's current direction
		/// </summary>
		protected virtual void DetermineDirection()
		{
			
		}

		/// <summary>
		/// Performs obstacle detection "manually"
		/// </summary>
		/// <param name="distance"></param>
		/// <param name="offset"></param>
		public virtual void DetectObstacles(float distance, Vector3 offset)
		{

		}

		/// <summary>
		/// Called at FixedUpdate
		/// </summary>
		protected virtual void FixedUpdate()
		{

		}

		/// <summary>
		/// On LateUpdate, computes the speed of the agent
		/// </summary>
		protected virtual void LateUpdate()
		{
		}

		/// <summary>
		/// Checks if the character is grounded
		/// </summary>
		protected virtual void CheckIfGrounded()
		{
			JustGotGrounded = (!_groundedLastFrame && Grounded);
			_groundedLastFrame = Grounded;
		}
        
		/// <summary>
		/// Use this to apply an impact to a controller, moving it in the specified direction at the specified force
		/// </summary>
		/// <param name="direction"></param>
		/// <param name="force"></param>
		public virtual void Impact(Vector3 direction, float force)
		{

		}

		/// <summary>
		/// Sets gravity active or inactive
		/// </summary>
		/// <param name="status"></param>
		public virtual void SetGravityActive(bool status)
		{
			GravityActive = status;
		}

		/// <summary>
		/// Adds the specified force to the controller
		/// </summary>
		/// <param name="movement"></param>
		public virtual void AddForce(Vector3 movement)
		{

		}

        /// <summary>
        /// 컨트롤러의 현재 움직임을 지정된 Vector3으로 설정합니다
        /// </summary>
        /// <param name="movement"></param>
        public virtual void SetMovement(Vector3 movement)
		{

		}

        /// <summary>
        /// 컨트롤러를 지정된 위치(in world space)로 이동합니다
        /// </summary>
        /// <param name="newPosition"></param>
        public virtual void MovePosition(Vector3 newPosition)
		{
			
		}

		/// <summary>
		/// Resizes the controller's collider
		/// </summary>
		/// <param name="newHeight"></param>
		public virtual void ResizeColliderHeight(float newHeight, bool translateCenter = false)
		{

		}

		/// <summary>
		/// Resets the controller's collider size
		/// </summary>
		public virtual void ResetColliderSize()
		{

		}

		/// <summary>
		/// Returns true if the controller's collider can go back to original size without hitting an obstacle, false otherwise
		/// </summary>
		/// <returns></returns>
		public virtual bool CanGoBackToOriginalSize()
		{
			return true;
		}

		/// <summary>
		/// Turns the controller's collisions on
		/// </summary>
		public virtual void CollisionsOn()
		{

		}

		/// <summary>
		/// Turns the controller's collisions off
		/// </summary>
		public virtual void CollisionsOff()
		{

		}

		/// <summary>
		/// Sets the controller's rigidbody to Kinematic (or not kinematic)
		/// </summary>
		/// <param name="state"></param>
		public virtual void SetKinematic(bool state)
		{

		}

		/// <summary>
		/// Handles friction collisions
		/// </summary>
		protected virtual void HandleFriction()
		{

		}

        /// <summary>
        /// 이 컨트롤러의 모든 값을 재설정합니다
        /// </summary>
        public virtual void Reset()
		{
			_impact = Vector3.zero;
			GravityActive = true;
			Speed = Vector3.zero;
			Velocity = Vector3.zero;
			VelocityLastFrame = Vector3.zero;
			Acceleration = Vector3.zero;
			Grounded = true;
			JustGotGrounded = false;
			CurrentMovement = Vector3.zero;
			CurrentDirection = Vector3.zero;
			AddedForce = Vector3.zero;
		}
	}
}