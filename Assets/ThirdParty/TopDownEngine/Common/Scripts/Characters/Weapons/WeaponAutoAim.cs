using System;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 2D 및 3D 사양에 맞게 확장되어 자동 조준의 기본 사항을 처리하는 추상 클래스입니다.
    /// 확장 구성요소는 조준 구성요소가 있는 무기에 배치되어야 합니다.
    /// </summary>
    [RequireComponent(typeof(Weapon))]
	public abstract class WeaponAutoAim : TopDownMonoBehaviour
	{
		[Header("Layer Masks")]
		/// the layermask on which to look for aim targets
		[Tooltip("목표 대상을 찾을 레이어 마스크")]
		public LayerMask TargetsMask;
		/// the layermask on which to look for obstacles
		[Tooltip("장애물을 찾는 레이어 마스크")]
		public LayerMask ObstacleMask = LayerManager.ObstaclesLayerMask;

		[Header("Scan for Targets")]
		/// the radius (in units) around the character within which to search for targets
		[Tooltip("대상을 검색할 캐릭터 주위의 반경(단위)")]
		public float ScanRadius = 15f;
		/// the size of the boxcast that will be performed to verify line of fire
		[Tooltip("사선 확인을 위해 수행될 박스캐스트의 크기")]
		public Vector2 LineOfFireBoxcastSize = new Vector2(0.1f, 0.1f);
		/// the duration (in seconds) between 2 scans for targets
		[Tooltip("2개의 대상 스캔 사이의 기간(초)")]
		public float DurationBetweenScans = 1f;
		/// an offset to apply to the weapon's position for scan 
		[Tooltip("스캔을 위해 무기 위치에 적용할 오프셋")]
		public Vector3 DetectionOriginOffset = Vector3.zero;
		/// if this is true, auto aim scan will only acquire new targets if the owner is in the idle state 
		[Tooltip("이것이 사실이라면 자동 조준 스캔은 소유자가 유휴 상태에 있는 경우에만 새 목표를 획득합니다.")]
		public bool OnlyAcquireTargetsIfOwnerIsIdle = false;
		
		[Header("Weapon Rotation")]
		/// the rotation mode to apply when a target is found
		[Tooltip("대상을 찾았을 때 적용할 회전 모드")]
		public WeaponAim.RotationModes RotationMode;
		/// if this is true, the auto aim direction will also be passed as the last non null direction, so the weapon will keep aiming in that direction should the target be lost
		[Tooltip("이것이 사실인 경우 자동 조준 방향도 null이 아닌 마지막 방향으로 전달되므로 대상을 잃어도 무기는 해당 방향으로 계속 조준합니다.")]
		public bool ApplyAutoAimAsLastDirection = true;
        
		[Header("Camera Target")]
		/// whether or not this component should take control of the camera target when a camera is found
		[Tooltip("카메라가 발견되었을 때 이 구성요소가 카메라 대상을 제어해야 하는지 여부")]
		public bool MoveCameraTarget = true;
		/// the normalized distance (between 0 and 1) at which the camera target should be, on a line going from the weapon owner (0) to the auto aim target (1) 
		[Tooltip("무기 소유자(0)에서 자동 조준 대상(1)까지 가는 선상에 카메라 대상이 있어야 하는 정규화된 거리(0과 1 사이)입니다.")]
		[Range(0f, 1f)]
		public float CameraTargetDistance = 0.5f;
		/// the maximum distance from the weapon owner at which the camera target can be
		[Tooltip("카메라 대상이 무기 소유자로부터 도달할 수 있는 최대 거리")]
		[MMCondition("MoveCameraTarget", true)]
		public float CameraTargetMaxDistance = 10f;
		/// the speed at which to move the camera target
		[Tooltip("카메라 대상을 이동하는 속도")]
		[MMCondition("MoveCameraTarget", true)]
		public float CameraTargetSpeed = 5f;
		/// if this is true, the camera target will move back to the character if no target is found
		[Tooltip("이것이 사실인 경우, 대상을 찾을 수 없으면 카메라 대상이 캐릭터로 다시 이동합니다.")]
		[MMCondition("MoveCameraTarget", true)]
		public bool MoveCameraToCharacterIfNoTarget = false;

		[Header("Aim Marker")]
		/// An AimMarker prefab to use to show where this auto aim weapon is aiming
		[Tooltip("이 자동 조준 무기가 조준하는 위치를 표시하는 데 사용할 AimMarker 프리팹")]
		public AimMarker AimMarkerPrefab;
		/// if this is true, the aim marker will be removed when the weapon gets destroyed
		[Tooltip("이것이 사실이라면 무기가 파괴되면 조준 마커가 제거됩니다.")]
		public bool DestroyAimMarkerOnWeaponDestroy = true;

		[Header("Feedback")]
		/// A feedback to play when a target is found and we didn't have one already
		[Tooltip("목표를 찾았지만 아직 목표가 없었을 때 플레이할 피드백")]
		public MMFeedbacks FirstTargetFoundFeedback;
		/// a feedback to play when we already had a target and just found a new one
		[Tooltip("이미 목표가 있고 방금 새 목표를 찾았을 때 플레이할 수 있는 피드백")]
		public MMFeedbacks NewTargetFoundFeedback;
		/// a feedback to play when no more targets are found, and we just lost our last target
		[Tooltip("더 이상 목표를 찾을 수 없고 마지막 목표를 잃었을 때 플레이하기 위한 피드백")]
		public MMFeedbacks NoMoreTargetsFeedback;

		[Header("Debug")]
		/// the current target of the auto aim module
		[Tooltip("자동 조준 모듈의 현재 목표")]
		[MMReadOnly]
		public Transform Target;
		/// whether or not to draw a debug sphere around the weapon to show its aim radius
		[Tooltip("조준 반경을 표시하기 위해 무기 주위에 디버그 구를 그릴지 여부")]
		public bool DrawDebugRadius = true;
        
		protected float _lastScanTimestamp = 0f;
		protected WeaponAim _weaponAim;
		protected WeaponAim.AimControls _originalAimControl;
		protected WeaponAim.RotationModes _originalRotationMode;
		protected Vector3 _raycastOrigin;
		protected Weapon _weapon;
		protected bool _originalMoveCameraTarget;
		protected Transform _targetLastFrame;
		protected AimMarker _aimMarker;

		/// <summary>
		/// On Awake we initialize our component
		/// </summary>
		protected virtual void Start()
		{
			Initialization();
		}

		/// <summary>
		/// On init we grab our WeaponAim
		/// </summary>
		protected virtual void Initialization()
		{
			_weaponAim = this.gameObject.GetComponent<WeaponAim>();
			_weapon = this.gameObject.GetComponent<Weapon>();
			_isOwnerNull = _weapon.Owner == null;
			if (_weaponAim == null)
			{
				Debug.LogWarning(this.name + " : the WeaponAutoAim on this object requires that you add either a WeaponAim2D or WeaponAim3D component to your weapon.");
				return;
			}

			_originalAimControl = _weaponAim.AimControl;
			_originalRotationMode = _weaponAim.RotationMode;
			_originalMoveCameraTarget = _weaponAim.MoveCameraTargetTowardsReticle;

			FirstTargetFoundFeedback?.Initialization(this.gameObject);
			NewTargetFoundFeedback?.Initialization(this.gameObject);
			NoMoreTargetsFeedback?.Initialization(this.gameObject);

			if (AimMarkerPrefab != null)
			{
				_aimMarker = Instantiate(AimMarkerPrefab);
				_aimMarker.name = this.gameObject.name + "_AimMarker";
				_aimMarker.Disable();
			}
		}

		/// <summary>
		/// On Update, we setup our ray origin, scan periodically and set aim if needed
		/// </summary>
		protected virtual void Update()
		{
			if (_weaponAim == null)
			{
				return;
			}

			DetermineRaycastOrigin();
			ScanIfNeeded();
			HandleTarget();
			HandleMoveCameraTarget();
			HandleTargetChange();
			_targetLastFrame = Target;
		}

		/// <summary>
		/// A method used to compute the origin of the detection casts
		/// </summary>
		protected abstract void DetermineRaycastOrigin();

		/// <summary>
		/// This method should define how the scan for targets is performed
		/// </summary>
		/// <returns></returns>
		protected abstract bool ScanForTargets();

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public virtual bool CanAcquireNewTargets() 
		{
			if (OnlyAcquireTargetsIfOwnerIsIdle && !_isOwnerNull)
			{
				if (_weapon.Owner.MovementState.CurrentState != CharacterStates.MovementStates.Idle)
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Sends aim coordinates to the weapon aim component
		/// </summary>
		protected abstract void SetAim();

		/// <summary>
		/// Moves the camera target towards the auto aim target if needed
		/// </summary>
		protected Vector3 _newCamTargetPosition;
		protected Vector3 _newCamTargetDirection;
		protected bool _isOwnerNull;

		/// <summary>
		/// Checks for target changes and triggers the appropriate methods if needed
		/// </summary>
		protected virtual void HandleTargetChange()
		{
			if (Target == _targetLastFrame)
			{
				return;
			}

			if (_aimMarker != null)
			{
				_aimMarker.SetTarget(Target);
			}

			if (Target == null)
			{
				NoMoreTargets();
				return;
			}

			if (_targetLastFrame == null)
			{
				FirstTargetFound();
				return;
			}

			if ((_targetLastFrame != null) && (Target != null))
			{
				NewTargetFound();
			}
		}

		/// <summary>
		/// When no more targets are found, and we just lost one, we play a dedicated feedback
		/// </summary>
		protected virtual void NoMoreTargets()
		{
			NoMoreTargetsFeedback?.PlayFeedbacks();
		}

		/// <summary>
		/// When a new target is found and we didn't have one already, we play a dedicated feedback
		/// </summary>
		protected virtual void FirstTargetFound()
		{
			FirstTargetFoundFeedback?.PlayFeedbacks();
		}

		/// <summary>
		/// When a new target is found, and we previously had another, we play a dedicated feedback
		/// </summary>
		protected virtual void NewTargetFound()
		{
			NewTargetFoundFeedback?.PlayFeedbacks();
		}

		/// <summary>
		/// Moves the camera target if needed
		/// </summary>
		protected virtual void HandleMoveCameraTarget()
		{
			bool targetIsNull = (Target == null);
			
			if (!MoveCameraTarget || (_isOwnerNull))
			{
				return;
			}

			if (!MoveCameraToCharacterIfNoTarget && targetIsNull)
			{
				return;
			}

			if (targetIsNull)
			{
				_newCamTargetPosition = _weapon.Owner.transform.position;
			}
			else
			{
				_newCamTargetPosition = Vector3.Lerp(_weapon.Owner.transform.position, Target.transform.position, CameraTargetDistance);	
			}
			
			_newCamTargetDirection = _newCamTargetPosition - this.transform.position;
            
			if (_newCamTargetDirection.magnitude > CameraTargetMaxDistance)
			{
				_newCamTargetDirection = _newCamTargetDirection.normalized * CameraTargetMaxDistance;
			}

			_newCamTargetPosition = this.transform.position + _newCamTargetDirection;

			_newCamTargetPosition = Vector3.Lerp(_weapon.Owner.CameraTarget.transform.position,
				_newCamTargetPosition,
				Time.deltaTime * CameraTargetSpeed);

			_weapon.Owner.CameraTarget.transform.position = _newCamTargetPosition;
		}

		/// <summary>
		/// Performs a periodic scan
		/// </summary>
		protected virtual void ScanIfNeeded()
		{
			if (Time.time - _lastScanTimestamp > DurationBetweenScans)
			{
				ScanForTargets();
				_lastScanTimestamp = Time.time;
			}
		}

		/// <summary>
		/// Sets aim if needed, otherwise reverts to the previous aim control mode
		/// </summary>
		protected virtual void HandleTarget()
		{
			if (Target == null)
			{
				_weaponAim.AimControl = _originalAimControl;
				_weaponAim.RotationMode = _originalRotationMode;
				_weaponAim.MoveCameraTargetTowardsReticle = _originalMoveCameraTarget;
			}
			else
			{
				_weaponAim.AimControl = WeaponAim.AimControls.Script;
				_weaponAim.RotationMode = RotationMode;
				if (MoveCameraTarget)
				{
					_weaponAim.MoveCameraTargetTowardsReticle = false;
				}
				SetAim();
			}
		}

		/// <summary>
		/// Draws a sphere around the weapon to show its auto aim radius
		/// </summary>
		protected virtual void OnDrawGizmos()
		{
			if (DrawDebugRadius)
			{
				Gizmos.color = Color.yellow;
				Gizmos.DrawWireSphere(_raycastOrigin, ScanRadius);
			}
		}

		/// <summary>
		/// On Disable, we hide our aim marker if needed
		/// </summary>
		protected virtual void OnDisable()
		{
			if (_aimMarker != null)
			{
				_aimMarker.Disable();
			}
		}

		protected void OnDestroy()
		{
			if (DestroyAimMarkerOnWeaponDestroy && (_aimMarker != null))
			{
				Destroy(_aimMarker.gameObject);
			}
		}
	}
}