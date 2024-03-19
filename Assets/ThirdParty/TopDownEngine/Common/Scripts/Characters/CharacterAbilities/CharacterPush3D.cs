using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 강체를 밀 수 있도록 캐릭터에 이 기능을 추가하세요.
    /// Animator parameters : Pushing (bool)
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/Abilities/Character Push 3D")] 
	public class CharacterPush3D : CharacterAbility
	{
		[Header("Physics interaction")]
		/// if this is true, the controller will be able to apply forces to colliding rigidbodies
		[Tooltip("이것이 사실이라면 컨트롤러는 충돌하는 강체에 힘을 가할 수 있습니다.")]
		public bool AllowPhysicsInteractions = true;
		/// the length of the ray to cast in front of the character to detect pushables
		[Tooltip("pushable을 감지하기 위해 캐릭터 앞에 투사할 광선의 길이")]
		public float PhysicsInteractionsRaycastLength = 0.05f;
		/// the offset to apply to the origin of the physics interaction raycast (by default, the character's collider's center
		[Tooltip("물리 상호 작용 레이캐스트의 원점에 적용할 오프셋(기본적으로 캐릭터의 충돌체 중심)")]
		public Vector3 PhysicsInteractionsRaycastOffset = Vector3.zero;
		/// the force to apply when colliding with rigidbodies
		[Tooltip("강체와 충돌할 때 적용되는 힘")]
		public float PushPower = 1850f;

		protected const string _pushingAnimationParameterName = "Pushing";
		protected int _pushingAnimationParameter;
		protected CharacterController _characterController;
		protected RaycastHit _raycastHit;
		protected Rigidbody _pushedRigidbody;
		protected Vector3 _pushDirection;
		protected bool _pushing = false;
        
		/// <summary>
		/// On init, grabs controllers
		/// </summary>
		protected override void Initialization()
		{
			base.Initialization();
			_characterController = _controller.GetComponent<CharacterController>();
			_controller3D = _controller.GetComponent<TopDownController3D>();
		}

		/// <summary>
		/// Every frame, handles physics interactions
		/// </summary>
		public override void ProcessAbility()
		{
			base.ProcessAbility();

			if (!AbilityAuthorized
			    || ((_condition.CurrentState != CharacterStates.CharacterConditions.Normal) && (_condition.CurrentState != CharacterStates.CharacterConditions.ControlledMovement)))
			{
				return;
			}

			HandlePhysicsInteractions();
		}

		/// <summary>
		/// Checks for a pushable object and applies the specified force
		/// </summary>
		protected virtual void HandlePhysicsInteractions()
		{
			if (!AllowPhysicsInteractions)
			{
				return;
			}

			// we cast a ray towards our move direction to handle pushing objects
			Physics.Raycast(_controller3D.transform.position + _characterController.center + PhysicsInteractionsRaycastOffset, _controller.CurrentMovement.normalized, out _raycastHit, 
				_characterController.radius + _characterController.skinWidth + PhysicsInteractionsRaycastLength, _controller3D.ObstaclesLayerMask);

			_pushing = (_raycastHit.collider != null);
            
			if (_pushing)
			{
				HandlePush(_controller3D, _raycastHit, _raycastHit.point);
			}
		}

		/// <summary>
		/// Adds a force to the colliding object at the hit position, to interact with the physics world
		/// </summary>
		/// <param name="hit"></param>
		/// <param name="hitPosition"></param>
		protected virtual void HandlePush(TopDownController3D controller3D, RaycastHit hit, Vector3 hitPosition)
		{
			_pushedRigidbody = hit.collider.attachedRigidbody;
            
			if ((_pushedRigidbody == null) || (_pushedRigidbody.isKinematic))
			{
				return;
			}
            
			_pushDirection.x = controller3D.CurrentMovement.normalized.x;
			_pushDirection.y = 0;
			_pushDirection.z = controller3D.CurrentMovement.normalized.z;

			_pushedRigidbody.AddForceAtPosition(_pushDirection * PushPower * Time.deltaTime, hitPosition);
		}

		/// <summary>
		/// Adds required animator parameters to the animator parameters list if they exist
		/// </summary>
		protected override void InitializeAnimatorParameters()
		{
			RegisterAnimatorParameter (_pushingAnimationParameterName, AnimatorControllerParameterType.Float, out _pushingAnimationParameter);
		}

		/// <summary>
		/// Sends the current speed and the current value of the pushing state to the animator
		/// </summary>
		public override void UpdateAnimator()
		{
			MMAnimatorExtensions.UpdateAnimatorBool(_animator, _pushingAnimationParameter, _pushing,_character._animatorParameters, _character.RunAnimatorSanityChecks);
		}
	}
}