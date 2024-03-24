using System;
using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;

namespace MoreMountains.TopDownEngine
{
	[RequireComponent(typeof(Rigidbody))]
	[RequireComponent(typeof(Collider))]
	[RequireComponent(typeof(CharacterController))]
	[AddComponentMenu("TopDown Engine/Character/Core/TopDown Controller 3D")]

    /// <summary>
    /// 유니티의 기본 캐릭터 컨트롤러 위에 추가하기 위해 초기에 유니티의 캐릭터 Motor.js를 개조한 컨트롤러로, 다음을 처리합니다
    /// </summary>
    public class TopDownController3D : TopDownController
	{
        /// 접지 상태를 계산하는 가능한 모드. 심플은 접지가 평평하고 평평한 경우에만 사용해야 합니다
        public enum GroundedComputationModes { Simple, Advanced }

        /// 이 문자로 보낸 현재 입력
        [MMReadOnly]
		[Tooltip("이 캐릭터로 보낸 현재 입력")]
		public Vector3 InputMoveDirection = Vector3.zero;

        /// 가능한 다양한 업데이트 모드
        public enum UpdateModes { Update, FixedUpdate }
        /// 점프할 때 속도를 전달하는 가능한 방법들
        public enum VelocityTransferOnJump { NoTransfer, InitialVelocity, FloorVelocity, Relative }

		[Header("Settings")]
        /// 이동 계산이 Update에서 이루어져야 하는지 FixedUpdate에서 이루어져야 하는지 FixedUpdate가 권장되는 선택입니다.
        [Tooltip("이동 계산이 Update에서 이루어져야 하는지 FixedUpdate에서 이루어져야 하는지 FixedUpdate가 권장되는 선택입니다.")]
		public UpdateModes UpdateMode = UpdateModes.FixedUpdate;
        /// 이동하는 지면에서 점프할 때 속도에 영향을 미치는 방법
        [Tooltip("이동하는 지면에서 점프할 때 속도에 영향을 미치는 방법")]
		public VelocityTransferOnJump VelocityTransferMethod = VelocityTransferOnJump.FloorVelocity;
        
		[Header("레이캐스트")]
        /// 장애물로 간주할 층(이동을 방지함)
        [Tooltip("장애물로 간주할 층(이동을 방지함)")]
		public LayerMask ObstaclesLayerMask = LayerManager.ObstaclesLayerMask;
        /// 아래로 드리우는 광선의 길이
        [Tooltip("아래로 드리우는 광선의 길이")]
		public float GroundedRaycastLength = 5f;
        /// 캐릭터가 더 이상 접지된 것으로 간주되지 않는 지면까지의 거리
        [Tooltip("캐릭터가 더 이상 접지된 것으로 간주되지 않는 지면까지의 거리")]
		public float MinimumGroundedDistance = 0.2f;
        /// 접지 상태를 계산하기 위해 선택한 모드입니다. 심플은 접지가 평평하고 평평한 경우에만 사용해야 합니다
        [Tooltip("접지 상태를 계산하기 위해 선택한 모드입니다. 심플은 접지가 평평하고 평평한 경우에만 사용해야 합니다")]
		public GroundedComputationModes GroundedComputationMode = GroundedComputationModes.Advanced;
        /// 단계를 넘길 때 체크해야 할 임계값. 캐릭터에 작은 단계를 통과하는 데 문제가 있는 경우 이 값을 조정하십시오
        [Tooltip("단계를 넘길 때 체크해야 할 임계값. 캐릭터에 작은 단계를 통과하는 데 문제가 있는 경우 이 값을 조정하십시오")] 
		public float GroundNormalHeightThreshold = 0.2f;
        /// 외력이 0이 되는 속도
        [Tooltip("외력이 0이 되는 속도")]
		public float ImpactFalloff = 5f;
        
		[Header("움직임")]

        /// 캐릭터가 떨어지는 동안 가질 수 있는 최대 수직 속도
        [Tooltip("캐릭터가 떨어지는 동안 가질 수 있는 최대 수직 속도")]
		public float MaximumFallSpeed = 20.0f;
        /// 비탈길을 걸을 때 속도를 곱하는 인자. x는 각도, y는 인자
        [Tooltip("비탈길을 걸을 때 속도를 곱하는 인자. x는 각도, y는 인자")]
		public AnimationCurve SlopeSpeedMultiplier = new AnimationCurve(new Keyframe(-90, 1), new Keyframe(0, 1), new Keyframe(90, 0));

		[Header("가파른 표면")]
        /// 현재 표면의 일반적인 벡터
        [Tooltip("현재 표면의 일반적인 벡터")]
		[MMReadOnly]
		public Vector3 GroundNormal = Vector3.zero;
        /// 캐릭터가 가파른 표면에 서 있는 동안 미끄러져야 하는지 여부
        [Tooltip("캐릭터가 가파른 표면에 서 있는 동안 미끄러져야 하는지 여부")]
		public bool SlideOnSteepSurfaces = true;
        /// 캐릭터가 미끄러지는 속도
        [Tooltip("캐릭터가 미끄러지는 속도")]
		public float SlidingSpeed = 15f;
        /// 미끄러져 내려올 때 선수가 속도를 조절하는 것
        [Tooltip("미끄러져 내려올 때 선수가 속도를 조절하는 것")]
		public float SlidingSpeedControl = 0.4f;
        /// 미끄러져 내려가는 동안 플레이어가 방향에 대해 가지고 있는 컨트롤
        [Tooltip("미끄러져 내려가는 동안 플레이어가 방향에 대해 가지고 있는 컨트롤")]
		public float SlidingDirectionControl = 1f;

        /// 충돌기의 중심 좌표를 반환합니다
        public override Vector3 ColliderCenter { get { return this.transform.position + _characterController.center; } }
        /// 충돌기의 하단 좌표를 반환합니다
        public override Vector3 ColliderBottom { get { return this.transform.position + _characterController.center + Vector3.down * _characterController.bounds.extents.y; } }
        /// 충돌기의 맨 위 좌표를 반환합니다
        public override Vector3 ColliderTop { get { return this.transform.position + _characterController.center + Vector3.up * _characterController.bounds.extents.y; } }
        /// 캐릭터가 가파른 경사를 미끄러져 내려오든 말든
        public virtual bool IsSliding() { return (Grounded && SlideOnSteepSurfaces && TooSteep()); }
        /// 캐릭터가 위에서 충돌하고 있는지 여부
        public virtual bool CollidingAbove() { return (_collisionFlags & CollisionFlags.CollidedAbove) != 0; }
        /// 현재 표면이 너무 가파른지 아닌지 여부
        public virtual bool TooSteep() { return (GroundNormal.y <= Mathf.Cos(_characterController.slopeLimit * Mathf.Deg2Rad)); }
        /// 문자가 이 프레임이 너무 가파르지 않은 경사/지상에 방금 진입했는지 여부
        public virtual bool ExitedTooSteepSlopeThisFrame { get; set; }
        ///  캐릭터가 움직이는 플랫폼에 있는지 여부
        public override bool OnAMovingPlatform { get { return ShouldMoveWithPlatformThisFrame(); } }
        /// 움직이는 플랫폼의 속도
        public override Vector3 MovingPlatformSpeed
		{
			get { return _movingPlatformVelocity;  }
		}
        
		protected Transform _transform;
		protected Rigidbody _rigidBody;
		protected Collider _collider;
		protected CharacterController _characterController;
		protected float _originalColliderHeight;
		protected Vector3 _originalColliderCenter;
		protected Vector3 _originalSizeRaycastOrigin;
		protected Vector3 _lastGroundNormal = Vector3.zero;
		protected WaitForFixedUpdate _waitForFixedUpdate = new WaitForFixedUpdate();
		protected bool _detached = false;

		// 이동 플랫폼
		protected Transform _movingPlatformHitCollider;
		protected Transform _movingPlatformCurrentHitCollider;
		protected Vector3 _movingPlatformCurrentHitColliderLocal;
		protected Vector3 _movingPlatformCurrentGlobalPoint;
		protected Quaternion _movingPlatformLocalRotation;
		protected Quaternion _movingPlatformGlobalRotation;
		protected Matrix4x4 _lastMovingPlatformMatrix;
		protected Vector3 _movingPlatformVelocity;
		protected bool _newMovingPlatform;

		// char movement
		protected CollisionFlags _collisionFlags;
		protected Vector3 _frameVelocity = Vector3.zero;
		protected Vector3 _hitPoint = Vector3.zero;
		protected Vector3 _lastHitPoint = new Vector3(Mathf.Infinity, 0, 0);

		// velocity
		protected Vector3 _newVelocity;
		protected Vector3 _lastHorizontalVelocity;
		protected Vector3 _newHorizontalVelocity;
		protected Vector3 _motion;
		protected Vector3 _idealVelocity;
		protected Vector3 _idealDirection;
		protected Vector3 _horizontalVelocityDelta;
		protected float _stickyOffset = 0f;

		// move position
		protected RaycastHit _movePositionHit;
		protected Vector3 _capsulePoint1;
		protected Vector3 _capsulePoint2;
		protected Vector3 _movePositionDirection;
		protected float _movePositionDistance;

		// collision detection
		protected RaycastHit _cardinalRaycast;
        
		protected float _smallestDistance = Single.MaxValue;
		protected float _longestDistance = Single.MinValue;

		protected RaycastHit _smallestRaycast;
		protected RaycastHit _emptyRaycast = new RaycastHit();
		protected Vector3 _downRaycastsOffset;

		protected Vector3 _moveWithPlatformMoveDistance;
		protected Vector3 _moveWithPlatformGlobalPoint;
		protected Quaternion _moveWithPlatformGlobalRotation;
		protected Quaternion _moveWithPlatformRotationDiff;
		protected RaycastHit _raycastDownHit;
		protected Vector3 _raycastDownDirection = Vector3.down;
		protected RaycastHit _canGoBackHeadCheck;
		protected bool _tooSteepLastFrame;

        /// <summary>
        /// 깨어있는 상태에서 향후 사용할 수 있도록 다양한 구성 요소를 저장합니다
        /// </summary>
        protected override void Awake()
		{
			base.Awake();

			_characterController = this.gameObject.GetComponent<CharacterController>();
			_transform = this.transform;
			_rigidBody = this.gameObject.GetComponent<Rigidbody>();
			_collider = this.gameObject.GetComponent<Collider>();
			_originalColliderHeight = _characterController.height;
			_originalColliderCenter = _characterController.center;
		}

        #region Update

        /// <summary>
        /// 업데이트가 늦어질 때 우리는 우리가 가지고 있는 영향을 적용하고 우리의 속도를 다음 프레임에 사용하기 위해 저장합니다
        /// </summary>
        protected override void LateUpdate()
		{
			base.LateUpdate();
			VelocityLastFrame = Velocity;
		}

        /// <summary>
        /// Update에서 Update Mode가 Update로 설정된 경우 Update 계산을 처리합니다
        /// </summary>
        protected override void Update()
		{
			base.Update();
			if (UpdateMode == UpdateModes.Update)
			{
				ProcessUpdate();
			}
		}

        /// <summary>
        /// FixedUpdate에서 UpdateMode가 FixedUpdate로 설정된 경우 업데이트 계산을 처리합니다
        /// </summary>
        protected override void FixedUpdate()
		{
			base.FixedUpdate();
			ApplyImpact();
			GetMovingPlatformVelocity();
			if (UpdateMode == UpdateModes.FixedUpdate)
			{
				ProcessUpdate();
			}
		}

        /// <summary>
        /// 새로운 속도를 계산하고 캐릭터를 이동합니다
        /// </summary>
        protected virtual void ProcessUpdate()
		{
			if (_transform == null)
			{
				return;
			}

			if (!FreeMovement)
			{
				return;
			}

			_newVelocity = Velocity;
			_positionLastFrame = _transform.position;

			AddInput();
			AddGravity();
			MoveWithPlatform();
			ComputeVelocityDelta();
			MoveCharacterController();
			DetectNewMovingPlatform();
			ComputeNewVelocity();
			ManualControllerColliderHit();
			HandleGroundContact();
			ComputeSpeed();
		}

        /// <summary>
        /// 현재 위치한 경사면과 입력에 따라 새로운 속도를 결정합니다
        /// </summary>
        protected virtual void AddInput()
		{
			if (Grounded && TooSteep())
			{
				_idealVelocity.x = GroundNormal.x;
				_idealVelocity.y = 0;
				_idealVelocity.z = GroundNormal.z;
				_idealVelocity = _idealVelocity.normalized;
				_idealDirection = Vector3.Project(InputMoveDirection, _idealVelocity);
				_idealVelocity = _idealVelocity + (_idealDirection * SlidingSpeedControl) + (InputMoveDirection - _idealDirection) * SlidingDirectionControl;
				_idealVelocity = _idealVelocity * SlidingSpeed;
			}
			else
			{
				_idealVelocity = CurrentMovement;
			}

			if (VelocityTransferMethod == VelocityTransferOnJump.FloorVelocity)
			{
				_idealVelocity += _frameVelocity;
				_idealVelocity.y = 0;
			}

			if (Grounded)
			{
				Vector3 sideways = Vector3.Cross(Vector3.up, _idealVelocity);
				_idealVelocity = Vector3.Cross(sideways, GroundNormal).normalized * _idealVelocity.magnitude;
			}

			_newVelocity = _idealVelocity;
			_newVelocity.y = Grounded ? Mathf.Min(_newVelocity.y, 0) : _newVelocity.y;
		}

        /// <summary>
        /// 새로운 속도와 추가된 힘에 중력을 더합니다
        /// </summary>
        protected virtual void AddGravity()
		{
			if (GravityActive)
			{
				if (Grounded)
				{
					_newVelocity.y = Mathf.Min(0, _newVelocity.y) - Gravity * Time.deltaTime;
				}
				else
				{
					_newVelocity.y = Velocity.y - Gravity * Time.deltaTime;
					_newVelocity.y = Mathf.Max(_newVelocity.y, -MaximumFallSpeed);
				}
			}
			_newVelocity += AddedForce;
			AddedForce = Vector3.zero;
		}

        /// <summary>
        /// 캐릭터 컨트롤러를 이동 및 회전하여 우리가 서 있을 수 있는 모든 이동 플랫폼을 따릅니다
        /// </summary>
        protected virtual void MoveWithPlatform()
		{
			if (ShouldMoveWithPlatformThisFrame())
			{
				_moveWithPlatformMoveDistance.x = _moveWithPlatformMoveDistance.y = _moveWithPlatformMoveDistance.z = 0f;
				_moveWithPlatformGlobalPoint = _movingPlatformCurrentHitCollider.TransformPoint(_movingPlatformCurrentHitColliderLocal);
				_moveWithPlatformMoveDistance = (_moveWithPlatformGlobalPoint - _movingPlatformCurrentGlobalPoint);
				if (_moveWithPlatformMoveDistance != Vector3.zero)
				{
					_characterController.Move(_moveWithPlatformMoveDistance);
				}
				_moveWithPlatformGlobalRotation = _movingPlatformCurrentHitCollider.rotation * _movingPlatformLocalRotation;
				_moveWithPlatformRotationDiff = _moveWithPlatformGlobalRotation * Quaternion.Inverse(_movingPlatformGlobalRotation);
				float yRotation = _moveWithPlatformRotationDiff.eulerAngles.y;
				if (yRotation != 0)
				{
					_transform.Rotate(0, yRotation, 0);
				}
			}
		}

        /// <summary>
        /// 캐릭터 컨트롤러에 적용할 모션 벡터를 계산합니다
        /// </summary>
        protected virtual void ComputeVelocityDelta()
		{
			_motion = _newVelocity * Time.deltaTime;
			_horizontalVelocityDelta.x = _motion.x;
			_horizontalVelocityDelta.y = 0f;
			_horizontalVelocityDelta.z = _motion.z;
			_stickyOffset = Mathf.Max(_characterController.stepOffset, _horizontalVelocityDelta.magnitude);
			if (Grounded)
			{
				_motion -= _stickyOffset * Vector3.up;
			}
		}

        /// <summary>
        /// 계산된 _motion을 기준으로 문자 컨트롤러를 이동합니다
        /// </summary>
        protected virtual void MoveCharacterController()
		{
			GroundNormal.x = GroundNormal.y = GroundNormal.z = 0f;

			_collisionFlags = _characterController.Move(_motion); // controller move

			_lastHitPoint = _hitPoint;
			_lastGroundNormal = GroundNormal;
		}

        /// <summary>
        /// 우리가 서 있을 수 있는 움직이는 플랫폼을 감지합니다
        /// </summary>
        protected virtual void DetectNewMovingPlatform()
		{
			if (_movingPlatformCurrentHitCollider != _movingPlatformHitCollider)
			{
				if (_movingPlatformHitCollider != null)
				{
					_movingPlatformCurrentHitCollider = _movingPlatformHitCollider;
					_lastMovingPlatformMatrix = _movingPlatformHitCollider.localToWorldMatrix;
					_newMovingPlatform = true;
				}
			}
		}

        /// <summary>
        /// 우리의 위치와 마지막 프레임에 따라 새로운 Velocity 값을 결정합니다
        /// </summary>
        protected virtual void ComputeNewVelocity()
		{
			Velocity = _newVelocity;
			Acceleration = (Velocity - VelocityLastFrame) / Time.deltaTime;
		}

        /// <summary>
        /// 접지 접촉, 속도 전달 및 이동 플랫폼을 처리합니다
        /// </summary>
        protected virtual void HandleGroundContact()
		{
			Grounded = _characterController.isGrounded;
            
			if (Grounded && !IsGroundedTest())
			{
				Grounded = false;
				if ((VelocityTransferMethod == VelocityTransferOnJump.InitialVelocity ||
				     VelocityTransferMethod == VelocityTransferOnJump.FloorVelocity))
				{
					_frameVelocity = _movingPlatformVelocity;
					Velocity += _movingPlatformVelocity;
				}
			}
			else if (!Grounded && IsGroundedTest())
			{
				if (_detached && Velocity.y <= 0)
				{
					Grounded = true;
					_detached = false;
				}
				SubstractNewPlatformVelocity();
			}

			if (ShouldMoveWithPlatformThisFrame())
			{
				_movingPlatformCurrentHitColliderLocal = _movingPlatformCurrentHitCollider.InverseTransformPoint(_movingPlatformCurrentGlobalPoint);
				_movingPlatformGlobalRotation = _transform.rotation;
				_movingPlatformLocalRotation = Quaternion.Inverse(_movingPlatformCurrentHitCollider.rotation) * _movingPlatformGlobalRotation;
			}

			ExitedTooSteepSlopeThisFrame = (_tooSteepLastFrame && !TooSteep());
            
			_tooSteepLastFrame = TooSteep();
		}

        /// <summary>
        /// 현재의 움직임에 따라 방향을 결정합니다
        /// </summary>
        protected override void DetermineDirection()
		{
			if (CurrentMovement.magnitude > 0f)
			{
				CurrentDirection = CurrentMovement.normalized;
			}
		}

        /// <summary>
        /// 접지면과의 마찰을 처리합니다. 곧 제공될 예정입니다
        /// </summary>
        protected override void HandleFriction()
		{
			//TODO - coming soon
		}

        /// <summary>
        /// 컨트롤러를 지면에서 강제로 분리합니다
        /// </summary>
        public virtual void DetachFromGround()
		{
			_detached = true;
		}

		public virtual void DetachFromMovingPlatform()
		{
			_movingPlatformVelocity = Vector3.zero;
			_movingPlatformCurrentHitCollider = null;
			_movingPlatformHitCollider = null;
		}

        #endregion

        #region Rigidbody push mechanics

        /// <summary>
        /// 이 방법은 유감스럽게도 많은 쓰레기를 발생시키는 일반 OnControllerColliderHit을 보상합니다.
        /// 이를 위해, 우리의 지면을 정상으로 만들기 위해 아래쪽으로 광선을 던지고, (잠재적으로) 강체를 밀어내기 위해 전류 이동 방향으로 광선을 던집니다
        /// </summary>

        protected virtual void ManualControllerColliderHit()
		{
			HandleAdvancedGroundDetection();
		}

		protected virtual void HandleAdvancedGroundDetection()
		{
			if (GroundedComputationMode != GroundedComputationModes.Advanced)
			{
				return;
			}
            
			_smallestDistance = Single.MaxValue;
			_longestDistance = Single.MinValue;
			_smallestRaycast = _emptyRaycast;

            // 우리는 4개의 광선을 아래쪽으로 던져서 땅을 정상으로 만듭니다
            float offset = _characterController.radius;
            
			_downRaycastsOffset.x = 0f;
			_downRaycastsOffset.y = 0f;
			_downRaycastsOffset.z = 0f;
			CastRayDownwards();
			_downRaycastsOffset.x = -offset;
			_downRaycastsOffset.y = offset;
			_downRaycastsOffset.z = 0f;
			CastRayDownwards();
			_downRaycastsOffset.x = 0f;
			_downRaycastsOffset.y = offset;
			_downRaycastsOffset.z = -offset;
			CastRayDownwards();
			_downRaycastsOffset.x = offset;
			_downRaycastsOffset.y = offset;
			_downRaycastsOffset.z = 0f;
			CastRayDownwards();
			_downRaycastsOffset.x = 0f;
			_downRaycastsOffset.y = offset;
			_downRaycastsOffset.z = offset;
			CastRayDownwards();

            // 우리는 우리의 가장 짧은 광선을 처리합니다
            if (_smallestRaycast.collider != null)
			{
				float adjustedDistance = AdjustDistance(_smallestRaycast.distance);
                
				if (_smallestRaycast.normal.y > 0 && _smallestRaycast.normal.y > GroundNormal.y )
				{
					if ( (Mathf.Abs(_smallestRaycast.point.y - _lastHitPoint.y) < GroundNormalHeightThreshold) && ( (_smallestRaycast.point != _lastHitPoint) || (_lastGroundNormal == Vector3.zero)))
					{
						GroundNormal = _smallestRaycast.normal;
					}
					else
					{
						GroundNormal = _lastGroundNormal;
					}
                    
					_movingPlatformHitCollider = _smallestRaycast.collider.transform;
					_hitPoint = _smallestRaycast.point;
					_frameVelocity.x = _frameVelocity.y = _frameVelocity.z = 0f;
				}    
			} 
		}

        /// <summary>
        /// 아래쪽으로 광선을 던지고 필요한 경우 거리를 조정합니다
        /// </summary>
        protected virtual void CastRayDownwards()
		{
			if (_smallestDistance <= MinimumGroundedDistance)
			{
				return;
			}
            
			Physics.Raycast(this._transform.position + _characterController.center + _downRaycastsOffset, _raycastDownDirection, out _raycastDownHit, 
				_characterController.height/2f + GroundedRaycastLength, ObstaclesLayerMask);
            
			if (_raycastDownHit.collider != null)
			{
				float adjustedDistance = AdjustDistance(_raycastDownHit.distance);
                
				if (adjustedDistance < _smallestDistance) { _smallestDistance = adjustedDistance; _smallestRaycast = _raycastDownHit; }
				if (adjustedDistance > _longestDistance) { _longestDistance = adjustedDistance; }
			}
		}

        /// <summary>
        /// 캐릭터의 극한과 지면 사이의 실제 거리를 반환합니다
        /// </summary>
        /// <param name="distance"></param>
        /// <returns></returns>
        protected float AdjustDistance(float distance)
		{
			float adjustedDistance = distance - _characterController.height / 2f -
			                         _characterController.skinWidth;
			return adjustedDistance;
		}

		protected Vector3 _onTriggerEnterPushbackDirection;

        /// <summary>
        /// 다른 것으로 트리거할 때는 움직이는 플랫폼인지 확인하고 필요한 경우 스스로 밀어냅니다
        /// </summary>
        /// <param name="other"></param>
        protected virtual void OnTriggerEnter(Collider other)
		{
            // 방아쇠를 당길 때, 우리가 움직이는 플랫폼과 충돌할 때, 우리는 우리 자신을 반대 방향으로 밀어냅니다
            if (other.gameObject.MMGetComponentNoAlloc<MovingPlatform3D>() != null)
			{
				if (this.transform.position.y < other.transform.position.y)
				{
					_onTriggerEnterPushbackDirection = (this.transform.position - other.transform.position).normalized;
					this.Impact(_onTriggerEnterPushbackDirection.normalized, other.gameObject.MMGetComponentNoAlloc<MovingPlatform3D>().PushForce);
				}
			}
		}

        #endregion

        #region Moving Platforms

        /// <summary>
        /// 현재 이동하는 플랫폼의 속도를 가져옵니다
        /// </summary>
        protected virtual void GetMovingPlatformVelocity()
		{
			if (_movingPlatformCurrentHitCollider != null)
			{
				if (!_newMovingPlatform && (Time.deltaTime != 0f))
				{
					_movingPlatformVelocity = (
						_movingPlatformCurrentHitCollider.localToWorldMatrix.MultiplyPoint3x4(_movingPlatformCurrentHitColliderLocal)
						- _lastMovingPlatformMatrix.MultiplyPoint3x4(_movingPlatformCurrentHitColliderLocal)
					) / Time.deltaTime;
				}
				_lastMovingPlatformMatrix = _movingPlatformCurrentHitCollider.localToWorldMatrix;
				_newMovingPlatform = false;
			}
			else
			{
				_movingPlatformVelocity = Vector3.zero;
			}
		}

        /// <summary>
        /// 상대 속도를 계산합니다
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator SubstractNewPlatformVelocity()
		{
			if ((VelocityTransferMethod == VelocityTransferOnJump.InitialVelocity ||
			     VelocityTransferMethod == VelocityTransferOnJump.FloorVelocity))
			{
				if (_newMovingPlatform)
				{
					Transform platform = _movingPlatformCurrentHitCollider;
					yield return _waitForFixedUpdate;
					if (Grounded && platform == _movingPlatformCurrentHitCollider)
					{
						yield return 1;
					}
				}
				Velocity -= _movingPlatformVelocity;
			}
		}

        /// <summary>
        /// 우리의 캐릭터가 이 프레임을 움직이는 플랫폼과 함께 움직여야 하는지 여부
        /// </summary>
        /// <returns></returns>
        protected virtual bool ShouldMoveWithPlatformThisFrame()
		{
			return (
				(Grounded || VelocityTransferMethod == VelocityTransferOnJump.Relative)
				&& _movingPlatformCurrentHitCollider != null
			);
		}

        #endregion

        #region Collider Resizing

        /// <summary>
        /// 이 인스턴스가 원래 크기로 돌아갈 수 있는지 여부를 결정합니다.
        /// </summary>
        /// <returns><c>true</c> if this instance can go back to original size; otherwise, <c>false</c>.</returns>
        public override bool CanGoBackToOriginalSize()
		{
			// if we're already at original size, we return true
			if (_collider.bounds.size.y == _originalColliderHeight)
			{
				return true;
			}
			float headCheckDistance = _originalColliderHeight * transform.localScale.y * CrouchedRaycastLengthMultiplier;

			// we cast two rays above our character to check for obstacles. If we didn't hit anything, we can go back to original size, otherwise we can't
			_originalSizeRaycastOrigin = ColliderTop + transform.up * _smallValue;

			_canGoBackHeadCheck = MMDebug.Raycast3D(_originalSizeRaycastOrigin, transform.up, headCheckDistance, ObstaclesLayerMask, Color.cyan, true);
			if (_canGoBackHeadCheck.collider != null)
			{
				return false;
			}
			else
			{
				return true;
			}
		}

        /// <summary>
        /// 충돌기의 크기를 모수로 설정된 새 크기로 조정합니다
        /// </summary>
        /// <param name="newSize">New size.</param>
        public override void ResizeColliderHeight(float newHeight, bool translateCenter = false)
		{
			float newYOffset = _originalColliderCenter.y - (_originalColliderHeight - newHeight) / 2;
			_characterController.height = newHeight;
			_characterController.center = ((_originalColliderHeight - newHeight) / 2) * Vector3.up;

			if (translateCenter)
			{
				this.transform.Translate((newYOffset / 2f) * Vector3.up);	
			}
		}

        /// <summary>
        /// 충돌기를 초기 크기로 되돌립니다
        /// </summary>
        public override void ResetColliderSize()
		{
			_characterController.height = _originalColliderHeight;
			_characterController.center = _originalColliderCenter;
		}

        #endregion

        #region Grounded Tests

        /// <summary>
        /// 캐릭터의 접지 여부
        /// </summary>
        /// <returns></returns>
        public virtual bool IsGroundedTest()
		{
			bool grounded = false;
			if (GroundedComputationMode == GroundedComputationModes.Advanced)
			{
				if (_smallestDistance <= MinimumGroundedDistance)
				{
					grounded = (GroundNormal.y > 0.01) ;
				}    
			}
			else
			{
				grounded = true;
				GroundNormal.x = 0;
				GroundNormal.y = 1;
				GroundNormal.z = 0;
			}
            
			return grounded;
		}

        /// <summary>
        /// 접지 체크
        /// </summary>
        protected override void CheckIfGrounded()
		{
			JustGotGrounded = (!_groundedLastFrame && Grounded);
			_groundedLastFrame = Grounded;
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// 콜라이더를 활성화 합니다.
		/// </summary>
		public override void CollisionsOn()
		{
			_collider.enabled = true;
		}

        /// <summary>
        /// 콜라이더를 비활성화 합니다.
        /// </summary>
        public override void CollisionsOff()
		{
			_collider.enabled = false;
		}

        /// <summary>
        /// 기본 충돌 점검을 수행하고 충돌 물체 정보를 저장합니다
        /// </summary>
        /// <param name="distance"></param>
        /// <param name="offset"></param>
        public override void DetectObstacles(float distance, Vector3 offset)
		{
			if (!PerformCardinalObstacleRaycastDetection)
			{
				return;
			}
            
			CollidingWithCardinalObstacle = false;
			// right
			_cardinalRaycast = MMDebug.Raycast3D(this.transform.position + offset, Vector3.right, distance, ObstaclesLayerMask, Color.yellow, true);
			if (_cardinalRaycast.collider != null) { DetectedObstacleRight = _cardinalRaycast.collider.gameObject; CollidingWithCardinalObstacle = true; } else { DetectedObstacleRight = null; }
			// left
			_cardinalRaycast = MMDebug.Raycast3D(this.transform.position + offset, Vector3.left, distance, ObstaclesLayerMask, Color.yellow, true);
			if (_cardinalRaycast.collider != null) { DetectedObstacleLeft = _cardinalRaycast.collider.gameObject; CollidingWithCardinalObstacle = true; } else { DetectedObstacleLeft = null; }
			// up
			_cardinalRaycast = MMDebug.Raycast3D(this.transform.position + offset, Vector3.forward, distance, ObstaclesLayerMask, Color.yellow, true);
			if (_cardinalRaycast.collider != null) { DetectedObstacleUp = _cardinalRaycast.collider.gameObject; CollidingWithCardinalObstacle = true; } else { DetectedObstacleUp = null; }
			// down
			_cardinalRaycast = MMDebug.Raycast3D(this.transform.position + offset, Vector3.back, distance, ObstaclesLayerMask, Color.yellow, true);
			if (_cardinalRaycast.collider != null) { DetectedObstacleDown = _cardinalRaycast.collider.gameObject; CollidingWithCardinalObstacle = true; } else { DetectedObstacleDown = null; }
		}

        /// <summary>
        /// 저장된 임팩트를 캐릭터에 적용합니다
        /// </summary>
        protected virtual void ApplyImpact()
		{
			if (_impact.magnitude > 0.2f)
			{
				_characterController.Move(_impact * Time.deltaTime);
			}
			_impact = Vector3.Lerp(_impact, Vector3.zero, ImpactFalloff * Time.deltaTime);
		}

        /// <summary>
        /// 지정한 방향과 크기의 힘을 문자에 추가합니다
        /// </summary>
        /// <param name="movement"></param>
        public override void AddForce(Vector3 movement)
		{
			AddedForce += movement;
		}

        /// <summary>
        /// 지정된 방향과 힘의 특성에 충격을 가합니다
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="force"></param>
        public override void Impact(Vector3 direction, float force)
		{
			direction = direction.normalized;
			if (direction.y < 0) { direction.y = -direction.y; }
			_impact += direction.normalized * force;
		}

        /// <summary>
        /// 캐릭터의 현재 입력 방향과 크기를 설정합니다
        /// </summary>
        /// <param name="movement"></param>
        public override void SetMovement(Vector3 movement)
		{
			CurrentMovement = movement;

			Vector3 directionVector;
			directionVector = movement;
			if (directionVector != Vector3.zero)
			{
				float directionLength = directionVector.magnitude;
				directionVector = directionVector / directionLength;
				directionLength = Mathf.Min(1, directionLength);
				directionLength = directionLength * directionLength;
				directionVector = directionVector * directionLength;
			}
			InputMoveDirection = transform.rotation * directionVector;
		}

        /// <summary>
        /// 이 캐릭터의 강체 운동학적인지 여부를 바꿉니다
        /// </summary>
        /// <param name="state"></param>
        public override void SetKinematic(bool state)
		{
			_rigidBody.isKinematic = state;
		}

        /// <summary>
        /// 장애물을 피하려고 할 때 이 캐릭터를 지정된 위치로 이동합니다
        /// </summary>
        /// <param name="newPosition"></param>
        public override void MovePosition(Vector3 newPosition)
		{
            
			_movePositionDirection = (newPosition - this.transform.position);
			_movePositionDistance = Vector3.Distance(this.transform.position, newPosition) ;

			_capsulePoint1 =    this.transform.position 
			                    + _characterController.center 
			                    - (Vector3.up * _characterController.height / 2f) 
			                    + Vector3.up * _characterController.skinWidth 
			                    + Vector3.up * _characterController.radius;
			_capsulePoint2 =    this.transform.position
			                    + _characterController.center
			                    + (Vector3.up * _characterController.height / 2f)
			                    - Vector3.up * _characterController.skinWidth
			                    - Vector3.up * _characterController.radius;

			if (!Physics.CapsuleCast(_capsulePoint1, _capsulePoint2, _characterController.radius, _movePositionDirection, out _movePositionHit, _movePositionDistance, ObstaclesLayerMask))
			{
				this.transform.position = newPosition;
			}
		}

        /// <summary>
        /// 재설정 시 벡터와 변환을 재설정합니다
        /// </summary>
        public override void Reset()
		{
			base.Reset();
			_idealDirection = Vector3.zero;
			_idealVelocity = Vector3.zero;
			_newVelocity = Vector3.zero;
			_movingPlatformVelocity = Vector3.zero;
			_movingPlatformCurrentHitCollider = null;
			_movingPlatformHitCollider = null;
			_lastGroundNormal = Vector3.zero;
			_detached = false;
			_frameVelocity = Vector3.zero;
			_hitPoint = Vector3.zero;
			_lastHitPoint = new Vector3(Mathf.Infinity, 0, 0);
		}

		#endregion
        
	}
}