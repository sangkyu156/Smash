using System;
using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using UnityEngine.UI;

namespace MoreMountains.TopDownEngine
{
	[RequireComponent(typeof(Weapon))]
	public abstract class WeaponAim : TopDownMonoBehaviour, MMEventListener<TopDownEngineEvent>
	{
		/// the list of possible control modes
		public enum AimControls { Off, PrimaryMovement, SecondaryMovement, Mouse, Script, SecondaryThenPrimaryMovement, PrimaryThenSecondaryMovement, CharacterRotateCameraDirection }
		/// the list of possible rotation modes
		public enum RotationModes { Free, Strict2Directions, Strict4Directions, Strict8Directions }
		/// the possible types of reticles
		public enum ReticleTypes { None, Scene, UI }

		[Header("Control Mode")]
		[MMInformation("이 구성요소를 무기에 추가하면 무기를 조준(회전)할 수 있습니다. 세 가지 제어 모드를 지원합니다: 마우스(무기가 포인터를 향함), 기본 이동(현재 입력 방향을 향함) 또는 보조 이동(두 번째 입력 축을 향함, 트윈 스틱 슈터를 생각함).", MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]
		/// the aim control mode
		[Tooltip("the selected aim control mode")]
		public AimControls AimControl = AimControls.SecondaryMovement;
		/// if this is true, this script will be able to read input from its specified AimControl mode
		[Tooltip("이것이 사실이라면 이 스크립트는 지정된 AimControl 모드에서 입력을 읽을 수 있습니다.")]
		public bool AimControlActive = true;

		[Header("Weapon Rotation")]
		[MMInformation("여기에서 회전이 자유로운지, 4방향(위, 아래, 왼쪽, 오른쪽)으로 엄격한지, 8방향(동일 + 대각선)인지 정의할 수 있습니다. 회전 속도와 최소 및 최대 각도를 정의할 수도 있습니다. 예를 들어 캐릭터가 등을 조준하는 것을 원하지 않으면 최소 각도를 -90으로, 최대 각도를 90으로 설정합니다.", MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]
		/// the rotation mode
		[Tooltip("the rotation mode")]
		public RotationModes RotationMode = RotationModes.Free;
		/// the the speed at which the weapon reaches its new position. Set it to zero if you want movement to directly follow input
		[Tooltip("무기가 새로운 위치에 도달하는 속도. 입력에 직접적으로 움직임을 적용하려면 0으로 설정하세요.")]
		public float WeaponRotationSpeed = 1f;
		/// the minimum angle at which the weapon's rotation will be clamped
		[Range(-180, 180)]
		[Tooltip("무기의 회전이 고정되는 최소 각도")]
		public float MinimumAngle = -180f;
		/// the maximum angle at which the weapon's rotation will be clamped
		[Range(-180, 180)]
		[Tooltip("무기의 회전이 고정되는 최대 각도")]
		public float MaximumAngle = 180f;
		/// the minimum threshold at which the weapon's rotation magnitude will be considered 
		[Tooltip("무기의 회전 크기가 고려되는 최소 임계값")]
		public float MinimumMagnitude = 0.2f;

		[Header("Reticle")]
		[MMInformation("화면에 조준선을 표시하여 조준 위치를 확인할 수도 있습니다. 사용하지 않으려면 비워두세요. 조준선 거리를 0으로 설정하면 커서를 따라가고, 그렇지 않으면 무기 중심의 원 위에 있게 됩니다. 마우스를 따라가도록 요청할 수도 있고 마우스 포인터를 교체할 수도 있습니다. 또한 조준 각도를 반영하기 위해 포인터를 회전해야 할지 아니면 안정적으로 유지해야 할지 결정할 수도 있습니다.", MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]
		/// Defines whether the reticle is placed in the scene or in the UI
		[Tooltip("레티클이 장면에 배치되는지 아니면 UI에 배치되는지 정의합니다.")]
		public ReticleTypes ReticleType = ReticleTypes.None;
		/// the gameobject to display as the aim's reticle/crosshair. Leave it blank if you don't want a reticle
		[MMEnumCondition("ReticleType", (int)ReticleTypes.Scene, (int)ReticleTypes.UI)]
		[Tooltip("조준의 십자선/십자선으로 표시할 게임 개체입니다. 레티클을 원하지 않으면 비워두세요")]
		public GameObject Reticle;
		/// the distance at which the reticle will be from the weapon
		[MMEnumCondition("ReticleType", (int)ReticleTypes.Scene, (int)ReticleTypes.UI)]
		[Tooltip("레티클이 무기로부터 멀어지는 거리")]
		public float ReticleDistance;
		/// the height at which the reticle should position itself above the ground, when in Scene mode
		[MMEnumCondition("ReticleType", (int)ReticleTypes.Scene, (int)ReticleTypes.UI)]
		[Tooltip("장면 모드에 있을 때 레티클이 지면 위에 위치해야 하는 높이")]
		public float ReticleHeight;
		/// if set to true, the reticle will be placed at the mouse's position (like a pointer)
		[MMEnumCondition("ReticleType", (int)ReticleTypes.Scene, (int)ReticleTypes.UI)]
		[Tooltip("true로 설정하면 레티클이 마우스 위치(포인터처럼)에 배치됩니다.")]
		public bool ReticleAtMousePosition;
		/// if set to true, the reticle will rotate on itself to reflect the weapon's rotation. If not it'll remain stable.
		[MMEnumCondition("ReticleType", (int)ReticleTypes.Scene)]
		[Tooltip("true로 설정하면 조준선이 무기 회전을 반영하여 자체적으로 회전합니다. 그렇지 않으면 안정적으로 유지됩니다.")]
		public bool RotateReticle = false;
		/// if set to true, the reticle will replace the mouse pointer
		[MMEnumCondition("ReticleType", (int)ReticleTypes.Scene, (int)ReticleTypes.UI)]
		[Tooltip("true로 설정하면 레티클이 마우스 포인터를 대체합니다.")]
		public bool ReplaceMousePointer = true;
		/// the radius around the weapon rotation centre where the mouse will be ignored, to avoid glitches
		[MMEnumCondition("ReticleType", (int)ReticleTypes.Scene, (int)ReticleTypes.UI)]
		[Tooltip("결함을 방지하기 위해 마우스가 무시되는 무기 회전 중심 주위의 반경")]
		public float MouseDeadZoneRadius = 0.5f;
		/// if set to false, the reticle won't be added and displayed
		[MMEnumCondition("ReticleType", (int)ReticleTypes.Scene, (int)ReticleTypes.UI)]
		[Tooltip("false로 설정하면 레티클이 추가되거나 표시되지 않습니다.")]
		public bool DisplayReticle = true;

		[Header("CameraTarget")]
		/// whether the camera target should be moved towards the reticle to provide a better vision of the possible target. If you don't have a reticle, it'll be moved towards your aim direction.
		[Tooltip("가능한 타겟에 대한 더 나은 시야를 제공하기 위해 카메라 타겟을 레티클 쪽으로 이동해야 하는지 여부입니다. 조준선이 없으면 조준 방향으로 이동합니다.")]
		public bool MoveCameraTargetTowardsReticle = false;
		/// the offset to apply to the camera target along the transform / reticle line
		[Range(0f, 1f)]
		[Tooltip("변환/레티클 라인을 따라 카메라 타겟에 적용할 오프셋")]
		public float CameraTargetOffset = 0.3f;
		/// the maximum distance at which to move the camera target
		[MMCondition("MoveCameraTargetTowardsReticle", true)]
		[Tooltip("무기에서 카메라 대상을 이동하는 최대 거리")]
		public float CameraTargetMaxDistance = 10f;
		/// the speed at which the camera target should be moved
		[MMCondition("MoveCameraTargetTowardsReticle", true)]
		[Tooltip("카메라 대상이 이동되어야 하는 속도")]
		public float CameraTargetSpeed = 5f;

		public float CurrentAngleAbsolute { get; protected set; }
		/// the weapon's current rotation
		public Quaternion CurrentRotation { get { return transform.rotation; } }
		/// the weapon's current direction
		public Vector3 CurrentAim { get { return _currentAim; } }
		/// the weapon's current direction, absolute (flip independent)
		public Vector3 CurrentAimAbsolute { get { return _currentAimAbsolute; } }
		/// the current angle the weapon is aiming at
		public float CurrentAngle { get; protected set; }
		/// the current angle the weapon is aiming at, adjusted to compensate for the current orientation of the character
		public virtual float CurrentAngleRelative
		{
			get
			{
				if (_weapon != null)
				{
					if (_weapon.Owner != null)
					{
						return CurrentAngle;
					}
				}
				return 0;
			}
		}

		public virtual Weapon TargetWeapon => _weapon;
        
		protected Camera _mainCamera;
		protected Vector2 _lastNonNullMovement;
		protected Weapon _weapon;
		protected Vector3 _currentAim = Vector3.zero;
		protected Vector3 _currentAimAbsolute = Vector3.zero;
		protected Quaternion _lookRotation;
		protected Vector3 _direction;
		protected float[] _possibleAngleValues;
		protected Vector3 _mousePosition;
		protected Vector3 _lastMousePosition;
		protected float _additionalAngle;
		protected Quaternion _initialRotation;
		protected Plane _playerPlane;
		protected GameObject _reticle;
		protected Vector3 _reticlePosition;
		protected Vector3 _newCamTargetPosition;
		protected Vector3 _newCamTargetDirection;
		protected bool _initialized = false;
        
		/// <summary>
		/// On Start(), we trigger the initialization
		/// </summary>
		protected virtual void Start()
		{
			Initialization();
		}

		/// <summary>
		/// Grabs the weapon component, initializes the angle values
		/// </summary>
		protected virtual void Initialization()
		{
			_weapon = GetComponent<Weapon>();
			_mainCamera = Camera.main;

			if (RotationMode == RotationModes.Strict4Directions)
			{
				_possibleAngleValues = new float[5];
				_possibleAngleValues[0] = -180f;
				_possibleAngleValues[1] = -90f;
				_possibleAngleValues[2] = 0f;
				_possibleAngleValues[3] = 90f;
				_possibleAngleValues[4] = 180f;
			}
			if (RotationMode == RotationModes.Strict8Directions)
			{
				_possibleAngleValues = new float[9];
				_possibleAngleValues[0] = -180f;
				_possibleAngleValues[1] = -135f;
				_possibleAngleValues[2] = -90f;
				_possibleAngleValues[3] = -45f;
				_possibleAngleValues[4] = 0f;
				_possibleAngleValues[5] = 45f;
				_possibleAngleValues[6] = 90f;
				_possibleAngleValues[7] = 135f;
				_possibleAngleValues[8] = 180f;
			}
			_initialRotation = transform.rotation;
			InitializeReticle();
			_playerPlane = new Plane(Vector3.up, Vector3.zero);
			_initialized = true;
		}
        
		public virtual void ApplyAim()
		{
			Initialization();
			GetCurrentAim();
			DetermineWeaponRotation();
		}

        /// <summary>
        /// 새로운 지점을 향해 무기를 조준합니다.
        /// </summary>
        /// <param name="newAim">New aim.</param>
        public virtual void SetCurrentAim(Vector3 newAim, bool setAimAsLastNonNullMovement = false)
		{
			_currentAim = newAim;
		}

		protected virtual void GetCurrentAim()
		{

		}

		/// <summary>
		/// Every frame, we compute the aim direction and rotate the weapon accordingly
		/// </summary>
		protected virtual void Update()
		{

		}

		/// <summary>
		/// On LateUpdate, resets any additional angle
		/// </summary>
		protected virtual void LateUpdate()
		{
			ResetAdditionalAngle();
		}
        
		/// <summary>
		/// Determines the weapon's rotation
		/// </summary>
		protected virtual void DetermineWeaponRotation()
		{

		}

		/// <summary>
		/// Moves the weapon's reticle
		/// </summary>
		protected virtual void MoveReticle()
		{

		}

		/// <summary>
		/// Returns the position of the reticle
		/// </summary>
		/// <returns></returns>
		public virtual Vector3 GetReticlePosition()
		{
			return _reticle.transform.position;
		}

		/// <summary>
		/// Returns the current mouse position
		/// </summary>
		public virtual Vector3 GetMousePosition()
		{
			return _mainCamera.ScreenToWorldPoint(_mousePosition);
		}

		/// <summary>
		/// Rotates the weapon, optionnally applying a lerp to it.
		/// </summary>
		/// <param name="newRotation">New rotation.</param>
		protected virtual void RotateWeapon(Quaternion newRotation, bool forceInstant = false)
		{
			if (GameManager.Instance.Paused)
			{
				return;
			}
			// if the rotation speed is == 0, we have instant rotation
			if ((WeaponRotationSpeed == 0f) || forceInstant)
			{
				transform.rotation = newRotation;
			}
			// otherwise we lerp the rotation
			else
			{
				transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, WeaponRotationSpeed * Time.deltaTime);
			}
		}

		protected Vector3 _aimAtDirection;
		protected Quaternion _aimAtQuaternion;
        
		protected virtual void AimAt(Vector3 target)
		{
		}

		/// <summary>
		/// If a reticle has been set, instantiates the reticle and positions it
		/// </summary>
		protected virtual void InitializeReticle()
		{
           
		}

		/// <summary>
		/// This method defines how the character's camera target should move
		/// </summary>
		protected virtual void MoveTarget()
		{

		}

		/// <summary>
		/// Removes any remaining reticle
		/// </summary>
		public virtual void RemoveReticle()
		{
			if (_reticle != null)
			{
				Destroy(_reticle.gameObject);
			}
		}

		/// <summary>
		/// Hides (or shows) the reticle based on the DisplayReticle setting
		/// </summary>
		protected virtual void HideReticle()
		{
			if (_reticle != null)
			{
				if (GameManager.Instance.Paused)
				{
					_reticle.gameObject.SetActive(false);
					return;
				}
				_reticle.gameObject.SetActive(DisplayReticle);
			}
		}
        
		/// <summary>
		/// Hides or show the mouse pointer based on the settings
		/// </summary>
		protected virtual void HideMousePointer()
		{
			if (AimControl != AimControls.Mouse)
			{
				return;
			}
			if (GameManager.Instance.Paused)
			{
				Cursor.visible = true;
				return;
			}
			if (ReplaceMousePointer)
			{
				Cursor.visible = true;
			}
			else
			{
				Cursor.visible = true;
			}
		}

		/// <summary>
		/// On Destroy, we reinstate our cursor if needed
		/// </summary>
		protected void OnDestroy()
		{
			if (ReplaceMousePointer)
			{
				Cursor.visible = true;
			}
		}


		/// <summary>
		/// Adds additional angle to the weapon's rotation
		/// </summary>
		/// <param name="addedAngle"></param>
		public virtual void AddAdditionalAngle(float addedAngle)
		{
			_additionalAngle += addedAngle;
		}

		/// <summary>
		/// Resets the additional angle
		/// </summary>
		protected virtual void ResetAdditionalAngle()
		{
			_additionalAngle = 0;
		}

		protected virtual void AutoDetectWeaponMode()
		{
			if (_weapon.Owner.LinkedInputManager != null)
			{
				if ((_weapon.Owner.LinkedInputManager.ForceWeaponMode) && (AimControl != AimControls.Off))
				{
					AimControl = _weapon.Owner.LinkedInputManager.WeaponForcedMode;
				}

				if ((!_weapon.Owner.LinkedInputManager.ForceWeaponMode) && (_weapon.Owner.LinkedInputManager.IsMobile) && (AimControl == AimControls.Mouse))
				{
					AimControl = AimControls.PrimaryMovement;
				}
			}
		}

		public void OnMMEvent(TopDownEngineEvent engineEvent)
		{
			switch (engineEvent.EventType)
			{
				case TopDownEngineEventTypes.LevelStart:
					_initialized = false;
					Initialization();
					break;
			}
		}
        
		/// <summary>
		/// On enable we start listening for events
		/// </summary>
		protected virtual void OnEnable()
		{
			this.MMEventStartListening<TopDownEngineEvent>();
		}

		/// <summary>
		/// On disable we stop listening for events
		/// </summary>
		protected virtual void OnDisable()
		{
			this.MMEventStopListening<TopDownEngineEvent>();
		}
	}
}