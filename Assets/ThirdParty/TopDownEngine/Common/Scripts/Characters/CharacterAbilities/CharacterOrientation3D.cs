using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 기능을 캐릭터에 추가하면 이동 방향이나 무기 회전 방향을 향하도록 회전할 수 있습니다.
    /// </summary>
    [MMHiddenProperties("AbilityStartFeedbacks", "AbilityStopFeedbacks")]
	[AddComponentMenu("TopDown Engine/Character/Abilities/Character Orientation 3D")]
	public class CharacterOrientation3D : CharacterAbility
	{
		/// the possible rotation modes
		public enum RotationModes { None, MovementDirection, WeaponDirection, Both }
		/// the possible rotation speeds
		public enum RotationSpeeds { Instant, Smooth, SmoothAbsolute }

		[Header("Rotation Mode")]

        /// 캐릭터가 이동 방향, 무기 방향 또는 둘 다를 향해야 하는지 여부 또는 없음
        [Tooltip("캐릭터가 이동 방향, 무기 방향 또는 둘 다를 향해야 하는지 여부 또는 없음")]
		public RotationModes RotationMode = RotationModes.None;
        /// 이것이 false이면 회전이 발생하지 않습니다.
        [Tooltip("이것이 false이면 회전이 발생하지 않습니다.")]
		public bool CharacterRotationAuthorized = true;

		[Header("Movement Direction")]

        /// 이것이 사실이라면 모델을 다음 방향으로 회전시킵니다.
        [Tooltip("이것이 사실이라면 모델을 다음 방향으로 회전시킵니다.")]
		public bool ShouldRotateToFaceMovementDirection = true;
        /// 현재 회전 모드
        [MMCondition("ShouldRotateToFaceMovementDirection", true)]
		[Tooltip("현재 회전 모드")]
		public RotationSpeeds MovementRotationSpeed = RotationSpeeds.Instant;
        /// 방향을 향해 회전하려는 객체입니다. 비어 있으면 캐릭터 모델을 사용합니다.
        [MMCondition("ShouldRotateToFaceMovementDirection", true)]
		[Tooltip("방향을 향해 회전하려는 객체입니다. 비어 있으면 캐릭터 모델을 사용합니다.")]
		public GameObject MovementRotatingModel;
        /// 방향을 향해 회전하는 속도(부드럽고 절대적인 경우에만 해당)
        [MMCondition("ShouldRotateToFaceMovementDirection", true)]
		[Tooltip("방향을 향해 회전하는 속도(부드럽고 절대적인 경우에만 해당)")]
		public float RotateToFaceMovementDirectionSpeed = 10f;
        /// 회전을 시작하는 임계값(절대 모드에만 해당)
        [MMCondition("ShouldRotateToFaceMovementDirection", true)]
		[Tooltip("회전을 시작하는 임계값(절대 모드에만 해당)")]
		public float AbsoluteThresholdMovement = 0.5f;
        /// 모델의 방향v
        [MMReadOnly]
		[Tooltip("모델의 방향")]
		public Vector3 ModelDirection;
        /// 각도 값의 모델 방향
        [MMReadOnly]
		[Tooltip("각도 값의 모델 방향")]
		public Vector3 ModelAngles;

		[Header("Weapon Direction")]

        /// 이것이 사실이라면 모델을 무기 방향으로 회전시킵니다.
        [Tooltip("이것이 사실이라면 모델을 무기 방향으로 회전시킵니다.")]
		public bool ShouldRotateToFaceWeaponDirection = true;
		/// the current rotation mode
		[MMCondition("ShouldRotateToFaceWeaponDirection", true)]
		[Tooltip("현재 회전 모드")]
		public RotationSpeeds WeaponRotationSpeed = RotationSpeeds.Instant;
		/// the object we want to rotate towards direction. If left empty, we'll use the Character's model
		[MMCondition("ShouldRotateToFaceWeaponDirection", true)]
		[Tooltip("방향을 향해 회전하려는 객체입니다. 비어 있으면 캐릭터 모델을 사용합니다.")]
		public GameObject WeaponRotatingModel;
		/// the speed at which to rotate towards direction (smooth and absolute only)
		[MMCondition("ShouldRotateToFaceWeaponDirection", true)]
		[Tooltip("방향을 향해 회전하는 속도(부드럽고 절대적인 경우에만 해당)")]
		public float RotateToFaceWeaponDirectionSpeed = 10f;
		/// the threshold after which we start rotating (absolute mode only)
		[MMCondition("ShouldRotateToFaceWeaponDirection", true)]
		[Tooltip("회전을 시작하는 임계값(절대 모드에만 해당)")]
		public float AbsoluteThresholdWeapon = 0.5f;
		/// the threshold after which we start rotating (absolute mode only)
		[MMCondition("ShouldRotateToFaceWeaponDirection", true)]
		[Tooltip("회전을 시작하는 임계값(절대 모드에만 해당)")]
		public bool LockVerticalRotation = true;

		[Header("Animation")]

		/// the speed at which the instant rotation animation parameter float resets to 0
		[Tooltip("순간 회전 애니메이션 매개변수 float가 0으로 재설정되는 속도")]
		public float RotationSpeedResetSpeed = 2f;
		/// the speed at which the YRotationOffsetSmoothed should lerp
		[Tooltip("YRotationOffsetSmoothed가 lerp해야 하는 속도")]
		public float RotationOffsetSmoothSpeed = 1f;

		[Header("Forced Rotation")]

		/// whether the character is being applied a forced rotation
		[Tooltip("캐릭터가 강제 회전을 적용하고 있는지 여부")]
		public bool ForcedRotation = false;
        /// 외부 스크립트에 의해 적용된 강제 회전
        [MMCondition("ForcedRotation", true)]
		[Tooltip("외부 스크립트에 의해 적용된 강제 회전")]
		public Vector3 ForcedRotationDirection;

		public Vector3 RelativeSpeed { get { return _relativeSpeed; } }
		public Vector3 RelativeSpeedNormalized { get { return _relativeSpeedNormalized; } }
		public float RotationSpeed { get { return _rotationSpeed; } }
		public Vector3 CurrentDirection { get { return _currentDirection; } }

		protected CharacterHandleWeapon _characterHandleWeapon;
		protected CharacterRun _characterRun;
		protected Vector3 _lastRegisteredVelocity;
		protected Vector3 _rotationDirection;
		protected Vector3 _lastMovement = Vector3.zero;
		protected Vector3 _lastAim = Vector3.zero;
		protected Vector3 _relativeSpeed;
		protected Vector3 _remappedSpeed = Vector3.zero;
		protected Vector3 _relativeMaximum;
		protected Vector3 _relativeSpeedNormalized;
		protected bool _secondaryMovementTriggered = false;
		protected Quaternion _tmpRotation;
		protected Quaternion _newMovementQuaternion;
		protected Quaternion _newWeaponQuaternion;
		protected bool _shouldRotateTowardsWeapon;
		protected float _rotationSpeed;
		protected float _modelAnglesYLastFrame;
		protected float _yRotationOffset;
		protected float _yRotationOffsetSmoothed;
		protected Vector3 _currentDirection;
		protected Vector3 _weaponRotationDirection;
		protected Vector3 _positionLastFrame;
		protected Vector3 _newSpeed;
		protected bool _controllerIsNull;
		protected const string _relativeForwardSpeedAnimationParameterName = "RelativeForwardSpeed";
		protected const string _relativeLateralSpeedAnimationParameterName = "RelativeLateralSpeed";
		protected const string _remappedForwardSpeedAnimationParameterName = "RemappedForwardSpeedNormalized";
		protected const string _remappedLateralSpeedAnimationParameterName = "RemappedLateralSpeedNormalized";
		protected const string _relativeForwardSpeedNormalizedAnimationParameterName = "RelativeForwardSpeedNormalized";
		protected const string _relativeLateralSpeedNormalizedAnimationParameterName = "RelativeLateralSpeedNormalized";
		protected const string _remappedSpeedNormalizedAnimationParameterName = "RemappedSpeedNormalized";
		protected const string _rotationSpeeddAnimationParameterName = "YRotationSpeed";
		protected const string _yRotationOffsetAnimationParameterName = "YRotationOffset";
		protected const string _yRotationOffsetSmoothedAnimationParameterName = "YRotationOffsetSmoothed";
		protected int _relativeForwardSpeedAnimationParameter;
		protected int _relativeLateralSpeedAnimationParameter;
		protected int _remappedForwardSpeedAnimationParameter;
		protected int _remappedLateralSpeedAnimationParameter;
		protected int _relativeForwardSpeedNormalizedAnimationParameter;
		protected int _relativeLateralSpeedNormalizedAnimationParameter;
		protected int _remappedSpeedNormalizedAnimationParameter;
		protected int _rotationSpeeddAnimationParameter;
		protected int _yRotationOffsetAnimationParameter;
		protected int _yRotationOffsetSmoothedAnimationParameter;
		
		/// <summary>
		/// On init we grab our model if necessary
		/// </summary>
		protected override void Initialization()
		{
			base.Initialization();

			if ((_model == null) && (MovementRotatingModel == null) && (WeaponRotatingModel == null))
			{
				Debug.LogError("CharacterOrientation3D on "+this.name+" : you need to set a CharacterModel on your Character component, and/or specify MovementRotatingModel and WeaponRotatingModel on your CharacterOrientation3D inspector. Check the documentation to learn more about this.");
			}

			if (MovementRotatingModel == null)
			{
				MovementRotatingModel = _model;
			}
			_characterRun = _character?.FindAbility<CharacterRun>();
			_characterHandleWeapon = _character?.FindAbility<CharacterHandleWeapon>();
			if (WeaponRotatingModel == null)
			{
				WeaponRotatingModel = _model;
			}
			_controllerIsNull = _controller == null;
		}

		/// <summary>
		/// Every frame we rotate towards the direction
		/// </summary>
		public override void ProcessAbility()
		{
			base.ProcessAbility();

			if ((MovementRotatingModel == null) && (WeaponRotatingModel == null))
			{
				return;
			}

			if (!AbilityAuthorized)
			{
				return;
			}

			if (GameManager.Instance.Paused)
			{
				return;
			}
			if (CharacterRotationAuthorized)
			{
				RotateToFaceMovementDirection();
				RotateToFaceWeaponDirection();
				RotateModel();
			}
		}


		protected virtual void FixedUpdate()
		{
			ComputeRelativeSpeeds();
		}


        /// <summary>
        /// 현재 방향을 향하도록 플레이어 모델을 회전합니다.
        /// </summary>
        protected virtual void RotateToFaceMovementDirection()
		{
            // 우리 방향을 향하지 않아야 한다면 아무것도 하지 않고 나가면 됩니다.
            if (!ShouldRotateToFaceMovementDirection) { return; }
			if ((RotationMode != RotationModes.MovementDirection) && (RotationMode != RotationModes.Both)) { return; }

			_currentDirection = ForcedRotation || _controllerIsNull ? ForcedRotationDirection : _controller.CurrentDirection;

            // 회전 모드가 순간적이면 단순히 방향을 향하도록 회전합니다.
            if (MovementRotationSpeed == RotationSpeeds.Instant)
			{
				if (_currentDirection != Vector3.zero)
				{
					_newMovementQuaternion = Quaternion.LookRotation(_currentDirection);
				}
			}

			// if the rotation mode is smooth, we lerp towards our direction
			if (MovementRotationSpeed == RotationSpeeds.Smooth)
			{
				if (_currentDirection != Vector3.zero)
				{
					_tmpRotation = Quaternion.LookRotation(_currentDirection);
					_newMovementQuaternion = Quaternion.Slerp(MovementRotatingModel.transform.rotation, _tmpRotation, Time.deltaTime * RotateToFaceMovementDirectionSpeed);
				}
			}

			// if the rotation mode is smooth, we lerp towards our direction even if the input has been released
			if (MovementRotationSpeed == RotationSpeeds.SmoothAbsolute)
			{
				if (_currentDirection.normalized.magnitude >= AbsoluteThresholdMovement)
				{
					_lastMovement = _currentDirection;
				}
				if (_lastMovement != Vector3.zero)
				{
					_tmpRotation = Quaternion.LookRotation(_lastMovement);
					_newMovementQuaternion = Quaternion.Slerp(MovementRotatingModel.transform.rotation, _tmpRotation, Time.deltaTime * RotateToFaceMovementDirectionSpeed);
				}
			}

			ModelDirection = MovementRotatingModel.transform.forward.normalized;
			ModelAngles = MovementRotatingModel.transform.eulerAngles;
		}

		/// <summary>
		/// Rotates the character so it faces the weapon's direction
		/// </summary>
		protected virtual void RotateToFaceWeaponDirection()
		{
			_newWeaponQuaternion = Quaternion.identity;
			_weaponRotationDirection = Vector3.zero;
			_shouldRotateTowardsWeapon = false;

			// if we're not supposed to face our direction, we do nothing and exit
			if (!ShouldRotateToFaceWeaponDirection) { return; }
			if ((RotationMode != RotationModes.WeaponDirection) && (RotationMode != RotationModes.Both)) { return; }
			if (_characterHandleWeapon == null) { return; }
			if (_characterHandleWeapon.WeaponAimComponent == null) { return; }

			_shouldRotateTowardsWeapon = true;

			_rotationDirection = _characterHandleWeapon.WeaponAimComponent.CurrentAim.normalized;

			if (LockVerticalRotation)
			{
				_rotationDirection.y = 0;
			}

			_weaponRotationDirection = _rotationDirection;

			MMDebug.DebugDrawArrow(this.transform.position, _rotationDirection, Color.red);

			// if the rotation mode is instant, we simply rotate to face our direction
			if (WeaponRotationSpeed == RotationSpeeds.Instant)
			{
				if (_rotationDirection != Vector3.zero)
				{
					_newWeaponQuaternion = Quaternion.LookRotation(_rotationDirection);
				}
			}

			// if the rotation mode is smooth, we lerp towards our direction
			if (WeaponRotationSpeed == RotationSpeeds.Smooth)
			{
				if (_rotationDirection != Vector3.zero)
				{
					_tmpRotation = Quaternion.LookRotation(_rotationDirection);
					_newWeaponQuaternion = Quaternion.Slerp(WeaponRotatingModel.transform.rotation, _tmpRotation, Time.deltaTime * RotateToFaceWeaponDirectionSpeed);
				}
			}

			// if the rotation mode is smooth, we lerp towards our direction even if the input has been released
			if (WeaponRotationSpeed == RotationSpeeds.SmoothAbsolute)
			{
				if (_rotationDirection.normalized.magnitude >= AbsoluteThresholdWeapon)
				{
					_lastMovement = _rotationDirection;
				}
				if (_lastMovement != Vector3.zero)
				{
					_tmpRotation = Quaternion.LookRotation(_lastMovement);
					_newWeaponQuaternion = Quaternion.Slerp(WeaponRotatingModel.transform.rotation, _tmpRotation, Time.deltaTime * RotateToFaceWeaponDirectionSpeed);
				}
			}
		}

		/// <summary>
		/// Rotates models if needed
		/// </summary>
		protected virtual void RotateModel()
		{
			MovementRotatingModel.transform.rotation = _newMovementQuaternion;

			if (_shouldRotateTowardsWeapon && (_weaponRotationDirection != Vector3.zero))
			{
				WeaponRotatingModel.transform.rotation = _newWeaponQuaternion;
			}
		}

		/// <summary>
		/// Computes the relative speeds
		/// </summary>
		protected virtual void ComputeRelativeSpeeds()
		{
			if ((MovementRotatingModel == null) && (WeaponRotatingModel == null))
			{
				return;
			}
            
			if (Time.deltaTime != 0f)
			{
				_newSpeed = (this.transform.position - _positionLastFrame) / Time.deltaTime;
			}

			// relative speed
			if ((_characterHandleWeapon == null) || (_characterHandleWeapon.CurrentWeapon == null))
			{
				_relativeSpeed = MovementRotatingModel.transform.InverseTransformVector(_newSpeed);
			}
			else
			{
				_relativeSpeed = WeaponRotatingModel.transform.InverseTransformVector(_newSpeed);
			}

			// remapped speed

			float maxSpeed = 0f;
			if (_characterMovement != null)
			{
                //if (gameObject.tag == "Player")
                //{
                //    maxSpeed = PlayerDataManager.GetSpeed();
                //}
                //else
                //{
                //    maxSpeed = _characterMovement.WalkSpeed;
                //}
                maxSpeed = _characterMovement.WalkSpeed;
            }
			if (_characterRun != null)
			{
				maxSpeed = _characterRun.RunSpeed;
			}
            
			_relativeMaximum = _character.transform.TransformVector(Vector3.one);
			
			_remappedSpeed.x = MMMaths.Remap(_relativeSpeed.x, 0f, maxSpeed, 0f, _relativeMaximum.x);
			_remappedSpeed.y = MMMaths.Remap(_relativeSpeed.y, 0f, maxSpeed, 0f, _relativeMaximum.y);
			_remappedSpeed.z = MMMaths.Remap(_relativeSpeed.z, 0f, maxSpeed, 0f, _relativeMaximum.z);
			
			// relative speed normalized
			_relativeSpeedNormalized = _relativeSpeed.normalized;
			_yRotationOffset = _modelAnglesYLastFrame - ModelAngles.y;

			_yRotationOffsetSmoothed = Mathf.Lerp(_yRotationOffsetSmoothed, _yRotationOffset, RotationOffsetSmoothSpeed * Time.deltaTime);
            
			// RotationSpeed
			if (Mathf.Abs(_modelAnglesYLastFrame - ModelAngles.y) > 1f)
			{
				_rotationSpeed = Mathf.Abs(_modelAnglesYLastFrame - ModelAngles.y);
			}
			else
			{
				_rotationSpeed -= Time.time * RotationSpeedResetSpeed;
			}
			if (_rotationSpeed <= 0f)
			{
				_rotationSpeed = 0f;
			}

			_modelAnglesYLastFrame = ModelAngles.y;
			_positionLastFrame = this.transform.position;
		}

		/// <summary>
		/// Forces the character's model to face in the specified direction
		/// </summary>
		/// <param name="direction"></param>
		public virtual void Face(Character.FacingDirections direction)
		{
			switch (direction)
			{
				case Character.FacingDirections.East:
					_newMovementQuaternion = Quaternion.LookRotation(Vector3.right);
					break;
				case Character.FacingDirections.North:
					_newMovementQuaternion = Quaternion.LookRotation(Vector3.forward);
					break;
				case Character.FacingDirections.South:
					_newMovementQuaternion = Quaternion.LookRotation(Vector3.back);
					break;
				case Character.FacingDirections.West:
					_newMovementQuaternion = Quaternion.LookRotation(Vector3.left);
					break;
			}
		}

		/// <summary>
		/// Forces the character's model to face the specified angles
		/// </summary>
		/// <param name="angles"></param>
		public virtual void Face(Vector3 angles)
		{
			_newMovementQuaternion = Quaternion.LookRotation(Quaternion.Euler(angles) * Vector3.forward);
		}

		/// <summary>
		/// Adds required animator parameters to the animator parameters list if they exist
		/// </summary>
		protected override void InitializeAnimatorParameters()
		{
			RegisterAnimatorParameter(_rotationSpeeddAnimationParameterName, AnimatorControllerParameterType.Float, out _rotationSpeeddAnimationParameter);
			RegisterAnimatorParameter(_relativeForwardSpeedAnimationParameterName, AnimatorControllerParameterType.Float, out _relativeForwardSpeedAnimationParameter);
			RegisterAnimatorParameter(_relativeLateralSpeedAnimationParameterName, AnimatorControllerParameterType.Float, out _relativeLateralSpeedAnimationParameter);
			RegisterAnimatorParameter(_remappedForwardSpeedAnimationParameterName, AnimatorControllerParameterType.Float, out _remappedForwardSpeedAnimationParameter);
			RegisterAnimatorParameter(_remappedLateralSpeedAnimationParameterName, AnimatorControllerParameterType.Float, out _remappedLateralSpeedAnimationParameter);
			RegisterAnimatorParameter(_relativeForwardSpeedNormalizedAnimationParameterName, AnimatorControllerParameterType.Float, out _relativeForwardSpeedNormalizedAnimationParameter);
			RegisterAnimatorParameter(_relativeLateralSpeedNormalizedAnimationParameterName, AnimatorControllerParameterType.Float, out _relativeLateralSpeedNormalizedAnimationParameter);
			RegisterAnimatorParameter(_remappedSpeedNormalizedAnimationParameterName, AnimatorControllerParameterType.Float, out _remappedSpeedNormalizedAnimationParameter);
			RegisterAnimatorParameter(_yRotationOffsetAnimationParameterName, AnimatorControllerParameterType.Float, out _yRotationOffsetAnimationParameter);
			RegisterAnimatorParameter(_yRotationOffsetSmoothedAnimationParameterName, AnimatorControllerParameterType.Float, out _yRotationOffsetSmoothedAnimationParameter);
		}

		/// <summary>
		/// Sends the current speed and the current value of the Walking state to the animator
		/// </summary>
		public override void UpdateAnimator()
		{
			MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _rotationSpeeddAnimationParameter, _rotationSpeed, _character._animatorParameters, _character.RunAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _relativeForwardSpeedAnimationParameter, _relativeSpeed.z, _character._animatorParameters, _character.RunAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _relativeLateralSpeedAnimationParameter, _relativeSpeed.x, _character._animatorParameters, _character.RunAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _remappedForwardSpeedAnimationParameter, _remappedSpeed.z, _character._animatorParameters, _character.RunAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _remappedLateralSpeedAnimationParameter, _remappedSpeed.x, _character._animatorParameters, _character.RunAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _relativeForwardSpeedNormalizedAnimationParameter, _relativeSpeedNormalized.z, _character._animatorParameters, _character.RunAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _relativeLateralSpeedNormalizedAnimationParameter, _relativeSpeedNormalized.x, _character._animatorParameters, _character.RunAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _remappedSpeedNormalizedAnimationParameter, _remappedSpeed.magnitude, _character._animatorParameters, _character.RunAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _yRotationOffsetAnimationParameter, _yRotationOffset, _character._animatorParameters, _character.RunAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _yRotationOffsetSmoothedAnimationParameter, _yRotationOffsetSmoothed, _character._animatorParameters, _character.RunAnimatorSanityChecks);
		}
	}
}