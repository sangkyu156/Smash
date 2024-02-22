using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 이 구성 요소를 개체에 추가하면 설정에 따른 보간 여부에 관계없이 업데이트 시 대상을 향해 이동됩니다.
    /// </summary>
    [AddComponentMenu("More Mountains/Tools/Movement/MMFollowTarget")]
	public class MMFollowTarget : MonoBehaviour
	{
		/// the possible update modes
		public enum UpdateModes { Update, FixedUpdate, LateUpdate }
		/// the possible follow modes
		public enum FollowModes { RegularLerp, MMLerp, MMSpring }
        /// 월드 공간에서 작동할지, 로컬 공간에서 작동할지 여부
        public enum PositionSpaces { World, Local }

		[Header("Follow Position")]
        /// 객체가 현재 대상의 위치를 ​​따르고 있는지 여부
        public bool FollowPosition = true;
        /// 이 객체가 X축의 대상을 따라야 하는지 여부
        [MMCondition("FollowPosition", true)]
		public bool FollowPositionX = true;
        /// 이 객체가 Y축의 대상을 따라야 하는지 여부
        [MMCondition("FollowPosition", true)]
		public bool FollowPositionY = true;
        /// 이 객체가 Z축의 대상을 따라야 하는지 여부
        [MMCondition("FollowPosition", true)]
		public bool FollowPositionZ = true;
		/// 월드 공간에서 작동할지, 로컬 공간에서 작동할지 여부
		[MMCondition("FollowPosition", true)] 
		public PositionSpaces PositionSpace = PositionSpaces.World;

		[Header("Follow Rotation")]
        /// whether or not the object is currently following its target's rotation
        /// 객체가 현재 대상의 회전을 따르고 있는지 여부
        public bool FollowRotation = true;

		[Header("Follow Scale")]
        /// whether or not the object is currently following its target's rotation
        /// 객체가 현재 대상의 회전을 따르고 있는지 여부
        public bool FollowScale = true;
        /// 다음을 따를 때 척도에 적용할 계수
        [MMCondition("FollowScale", true)]
		public float FollowScaleFactor = 1f;

		[Header("Target")]
        /// 따라야 할 목표
        public Transform Target;
        /// 뒤따르는 타겟에 적용할 오프셋
        [MMCondition("FollowPosition", true)]
		public Vector3 Offset;
		///whether or not to add the initial x distance to the offset
		[MMCondition("FollowPosition", true)]
		public bool AddInitialDistanceXToXOffset = false;
		///whether or not to add the initial y distance to the offset
		[MMCondition("FollowPosition", true)]
		public bool AddInitialDistanceYToYOffset = false;
		///whether or not to add the initial z distance to the offset
		[MMCondition("FollowPosition", true)]
		public bool AddInitialDistanceZToZOffset = false;

		[Header("Position Interpolation")]
        /// 움직임을 보간해야 하는지 여부
        public bool InterpolatePosition = true;
        /// 위치를 따라갈 때 사용할 팔로우 모드
        [MMCondition("InterpolatePosition", true)]
		public FollowModes FollowPositionMode = FollowModes.MMLerp;
        /// 추종자의 움직임을 보간하는 속도
        [MMCondition("InterpolatePosition", true)]
		public float FollowPositionSpeed = 10f;
		/// higher values mean more damping, less spring, low values mean less damping, more spring
		[MMEnumCondition("FollowPositionMode", (int)FollowModes.MMSpring)] 
		[Range(0.01f, 1.0f)]
		public float PositionSpringDamping = 0.3f;
		/// the frequency at which the spring should "vibrate", in Hz (1 : the spring will do one full period in one second)
		[MMEnumCondition("FollowPositionMode", (int)FollowModes.MMSpring)]
		public float PositionSpringFrequency = 3f;

		[Header("Rotation Interpolation")]
		/// whether or not we need to interpolate the movement
		public bool InterpolateRotation = true;
		/// the follow mode to use when interpolating the rotation
		[MMCondition("InterpolateRotation", true)]
		public FollowModes FollowRotationMode = FollowModes.MMLerp;
		/// the speed at which to interpolate the follower's rotation
		[MMCondition("InterpolateRotation", true)]
		public float FollowRotationSpeed = 10f;

		[Header("Scale Interpolation")]
		/// whether or not we need to interpolate the scale
		public bool InterpolateScale = true;
		/// the follow mode to use when interpolating the scale
		[MMCondition("InterpolateScale", true)]
		public FollowModes FollowScaleMode = FollowModes.MMLerp;
		/// the speed at which to interpolate the follower's scale
		[MMCondition("InterpolateScale", true)]
		public float FollowScaleSpeed = 10f;

		[Header("Mode")]
		/// the update at which the movement happens
		public UpdateModes UpdateMode = UpdateModes.Update;
		/// if this is true, this component will self disable when its host game object gets disabled
		public bool DisableSelfOnSetActiveFalse = false;
        
		[Header("Distances")]
		/// whether or not to force a minimum distance between the object and its target before it starts following
		public bool UseMinimumDistanceBeforeFollow = false;
		/// the minimum distance to keep between the object and its target
		public float MinimumDistanceBeforeFollow = 1f;
		/// whether or not we want to make sure the object is never too far away from its target
		public bool UseMaximumDistance = false;
		/// the maximum distance at which the object can be away from its target
		public float MaximumDistance = 1f;

		[Header("Anchor")] 
		/// if this is true, the movement will be constrained around the initial position
		public bool AnchorToInitialPosition;
		/// the maximum distance around the initial position at which the transform can move
		[MMCondition("AnchorToInitialPosition", true)]
		public float MaxDistanceToAnchor = 1f;
        
		protected bool _localSpace { get { return PositionSpace == PositionSpaces.Local; } }

		protected Vector3 _velocity = Vector3.zero;
		protected Vector3 _newTargetPosition;        
		protected Vector3 _initialPosition;
		protected Vector3 _lastTargetPosition;
		protected Vector3 _direction;
		protected Vector3 _newPosition;
		protected Vector3 _newScale;
		protected Quaternion _newTargetRotation;
		protected Quaternion _initialRotation;

		//항상 카메라 보도록 하기위해 만든 변수
		Transform _mainCamera;

        /// <summary>
        /// On start we store our initial position
        /// </summary>
        protected virtual void Start()
		{
			Initialization();

			if(_mainCamera == null)
			{
                _mainCamera = GameObject.FindWithTag("MainCamera").transform;
            }
		}

		/// <summary>
		/// Initializes the follow
		/// </summary>
		public virtual void Initialization()
		{
			SetInitialPosition();
			SetOffset();
		}

		/// <summary>
		/// Prevents the object from following the target anymore
		/// </summary>
		public virtual void StopFollowing()
		{
			FollowPosition = false;
		}

		/// <summary>
		/// Makes the object follow the target
		/// </summary>
		public virtual void StartFollowing()
		{
			FollowPosition = true;
			SetInitialPosition();
		}

		/// <summary>
		/// Stores the initial position
		/// </summary>
		protected virtual void SetInitialPosition()
		{
			_initialPosition = _localSpace ? this.transform.localPosition : this.transform.position;
			_initialRotation = this.transform.rotation;
			_lastTargetPosition = _localSpace ? this.transform.localPosition : this.transform.position;
		}

		/// <summary>
		/// Adds initial offset to the offset if needed
		/// </summary>
		protected virtual void SetOffset()
		{
			if (Target == null)
			{
				return;
			}
			Vector3 difference = this.transform.position - Target.transform.position;
			Offset.x = AddInitialDistanceXToXOffset ? difference.x : Offset.x;
			Offset.y = AddInitialDistanceYToYOffset ? difference.y : Offset.y;
			Offset.z = AddInitialDistanceZToZOffset ? difference.z : Offset.z;
		}

		/// <summary>
		/// At update we follow our target 
		/// </summary>
		protected virtual void Update()
		{
            if (gameObject.activeSelf)
            {
                transform.LookAt(_mainCamera);
            }

            if (Target == null)
			{
				return;
			}
			if (UpdateMode == UpdateModes.Update)
			{
				FollowTargetRotation();
				FollowTargetScale();
				FollowTargetPosition();
			}
		}

		/// <summary>
		/// At fixed update we follow our target 
		/// </summary>
		protected virtual void FixedUpdate()
		{
			if (UpdateMode == UpdateModes.FixedUpdate)
			{
				FollowTargetRotation();
				FollowTargetScale();
				FollowTargetPosition();
			}
		}

		/// <summary>
		/// At late update we follow our target 
		/// </summary>
		protected virtual void LateUpdate()
		{
			if (UpdateMode == UpdateModes.LateUpdate)
			{
				FollowTargetRotation();
				FollowTargetScale();
				FollowTargetPosition();
			}
		}

		/// <summary>
		/// Follows the target, lerping the position or not based on what's been defined in the inspector
		/// </summary>
		protected virtual void FollowTargetPosition()
		{
			if (Target == null)
			{
				return;
			}

			if (!FollowPosition)
			{
				return;
			}

			_newTargetPosition = Target.position + Offset;
			if (!FollowPositionX) { _newTargetPosition.x = _initialPosition.x; }
			if (!FollowPositionY) { _newTargetPosition.y = _initialPosition.y; }
			if (!FollowPositionZ) { _newTargetPosition.z = _initialPosition.z; }

			float trueDistance = 0f;
			_direction = (_newTargetPosition - this.transform.position).normalized;
			trueDistance = Vector3.Distance(this.transform.position, _newTargetPosition);
            
			float interpolatedDistance = trueDistance;
			if (InterpolatePosition)
			{
				switch (FollowPositionMode)
				{
					case FollowModes.MMLerp:
						interpolatedDistance = MMMaths.Lerp(0f, trueDistance, FollowPositionSpeed, Time.deltaTime);
						interpolatedDistance = ApplyMinMaxDistancing(trueDistance, interpolatedDistance);
						this.transform.Translate(_direction * interpolatedDistance, Space.World);
						break;
					case FollowModes.RegularLerp:
						interpolatedDistance = Mathf.Lerp(0f, trueDistance, Time.deltaTime * FollowPositionSpeed);
						interpolatedDistance = ApplyMinMaxDistancing(trueDistance, interpolatedDistance);
						this.transform.Translate(_direction * interpolatedDistance, Space.World);
						break;
					case FollowModes.MMSpring:
						_newPosition = this.transform.position;
						MMMaths.Spring(ref _newPosition, _newTargetPosition, ref _velocity, PositionSpringDamping, PositionSpringFrequency, FollowPositionSpeed, Time.deltaTime);
						if (_localSpace)
						{
							this.transform.localPosition = _newPosition;   
						}
						else
						{
							this.transform.position = _newPosition;    
						}
						break;
				}                
			}
			else
			{
				interpolatedDistance = ApplyMinMaxDistancing(trueDistance, interpolatedDistance);
				this.transform.Translate(_direction * interpolatedDistance, Space.World);
			}

			if (AnchorToInitialPosition)
			{
				if (Vector3.Distance(this.transform.position, _initialPosition) > MaxDistanceToAnchor)
				{
					if (_localSpace)
					{
						this.transform.localPosition = _initialPosition + Vector3.ClampMagnitude(this.transform.localPosition - _initialPosition, MaxDistanceToAnchor);   
					}
					else
					{
						this.transform.position = _initialPosition + Vector3.ClampMagnitude(this.transform.position - _initialPosition, MaxDistanceToAnchor);    
					}
				}
			}
		}

		/// <summary>
		/// Applies minimal and maximal distance rules to the interpolated distance
		/// </summary>
		/// <param name="trueDistance"></param>
		/// <param name="interpolatedDistance"></param>
		/// <returns></returns>
		protected virtual float ApplyMinMaxDistancing(float trueDistance, float interpolatedDistance)
		{
			if (UseMinimumDistanceBeforeFollow && (trueDistance - interpolatedDistance < MinimumDistanceBeforeFollow))
			{
				interpolatedDistance = 0f;
			}

			if (UseMaximumDistance && (trueDistance - interpolatedDistance >= MaximumDistance))
			{
				interpolatedDistance = trueDistance - MaximumDistance;
			}

			return interpolatedDistance;
		}

		/// <summary>
		/// Makes the object follow its target's rotation
		/// </summary>
		protected virtual void FollowTargetRotation()
		{
			if (Target == null)
			{
				return;
			}

			if (!FollowRotation)
			{
				return;
			}

			_newTargetRotation = Target.rotation;

			if (InterpolateRotation)
			{
				switch (FollowRotationMode)
				{
					case FollowModes.MMLerp:
						this.transform.rotation = MMMaths.Lerp(this.transform.rotation, _newTargetRotation, FollowRotationSpeed, Time.deltaTime);
						break;
					case FollowModes.RegularLerp:
						this.transform.rotation = Quaternion.Lerp(this.transform.rotation, _newTargetRotation, Time.deltaTime * FollowRotationSpeed);
						break;
					case FollowModes.MMSpring:
						this.transform.rotation = MMMaths.Lerp(this.transform.rotation, _newTargetRotation, FollowRotationSpeed, Time.deltaTime);
						break;
				}
			}
			else
			{
				this.transform.rotation = _newTargetRotation;
			}
		}

		/// <summary>
		/// Makes the object follow its target's scale
		/// </summary>
		protected virtual void FollowTargetScale()
		{
			if (Target == null)
			{
				return;
			}

			if (!FollowScale)
			{
				return;
			}

			_newScale = Target.localScale * FollowScaleFactor;

			if (InterpolateScale)
			{
				switch (FollowScaleMode)
				{
					case FollowModes.MMLerp:
						this.transform.localScale = MMMaths.Lerp(this.transform.localScale, _newScale, FollowScaleSpeed, Time.deltaTime);
						break;
					case FollowModes.RegularLerp:
						this.transform.localScale = Vector3.Lerp(this.transform.localScale, _newScale, Time.deltaTime * FollowScaleSpeed);
						break;
					case FollowModes.MMSpring:
						this.transform.localScale = MMMaths.Lerp(this.transform.localScale, _newScale, FollowScaleSpeed, Time.deltaTime);
						break;
				}
			}
			else
			{
				this.transform.localScale = _newScale;
			}
		}
        
		public virtual void ChangeFollowTarget(Transform newTarget) => Target = newTarget;

		protected virtual void OnDisable()
		{
			if (DisableSelfOnSetActiveFalse)
			{
				this.enabled = false;
			}
		}
	}
}