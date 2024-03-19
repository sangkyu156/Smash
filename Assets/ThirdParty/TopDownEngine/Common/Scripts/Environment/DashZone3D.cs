using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 영역을 트리거 충돌체에 추가하면 진입 시 3D 캐릭터에 대시가 자동으로 트리거됩니다.
    /// </summary>
    [RequireComponent(typeof(Collider))]
	[AddComponentMenu("TopDown Engine/Environment/Dash Zone 3D")]
	public class DashZone3D : TopDownMonoBehaviour
	{
		[Header("Bindings")]

		/// the collider of the obstacle you want to dash over
		[Tooltip("당신이 돌진하고 싶은 장애물의 충돌체")]
		public Collider CoverObstacleCollider;
		/// the (optional) exit dash zone on the other side of the collider
		[Tooltip("충돌체 반대편의 (선택 사항) 출구 대시 영역")]
		public List<DashZone3D> ExitDashZones;

		[Header("DashSettings")]

		/// the distance of the dash triggered when entering the zone
		[Tooltip("구역에 들어갈 때 발동되는 대시의 거리")]
		public float DashDistance = 3f;
		/// the duration of the dash
		[Tooltip("대시의 지속 시간")]
		public float DashDuration;
		/// the curve to apply to the dash
		[Tooltip("대시에 적용할 곡선")]
		public AnimationCurve DashCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

		[Header("Settings")]

		/// the max angle at which the character should approach the obstacle for the dash to happen
		[Tooltip("대시가 발생하기 위해 캐릭터가 장애물에 접근해야 하는 최대 각도")]
		public float MaxFacingAngle = 90f;
		/// the duration in seconds before re-enabling all triggers in the zone
		[Tooltip("해당 영역의 모든 트리거를 다시 활성화하기 전까지의 시간(초)")]
		public float TriggerResetDuration = 1f;
		/// if this is false, the dash won't happen
		[Tooltip("이것이 거짓이면 대시가 발생하지 않습니다.")]
		public bool DashAuthorized = true;

		protected CharacterDash3D _characterDash3D;
		protected CharacterHandleWeapon _characterHandleWeapon;
		protected WeaponAim3D _weaponAim3D;
		protected CharacterOrientation3D _characterOrientation3D;
		protected CharacterOrientation3D.RotationModes _rotationMode;
		protected WeaponAim.AimControls _weaponAimControl;
		protected Character _character;
		protected Collider _collider;
		protected WaitForSeconds _dashWaitForSeconds;
		protected WaitForSeconds _triggerResetForSeconds;
		protected Vector3 _direction1;
		protected Vector3 _direction2;
		protected bool _dashInProgress = false;

		/// <summary>
		/// On start we initialize our zone
		/// </summary>
		protected virtual void Start()
		{
			Initialization();
		}

		/// <summary>
		/// Grabs collider and readies the wait for seconds
		/// </summary>
		protected virtual void Initialization()
		{
			_collider = this.gameObject.GetComponent<Collider>();
			_collider.isTrigger = true;
			_dashWaitForSeconds = new WaitForSeconds(DashDuration);
			_triggerResetForSeconds = new WaitForSeconds(TriggerResetDuration);
		}

		/// <summary>
		/// On trigger enter, we ready our dash
		/// </summary>
		/// <param name="collider"></param>
		protected virtual void OnTriggerEnter(Collider collider)
		{
			TestForDash(collider);
		}

		/// <summary>
		/// On trigger enter, we ready our dash
		/// </summary>
		/// <param name="collider"></param>
		protected virtual void OnTriggerStay(Collider collider)
		{
			TestForDash(collider);
		}

		/// <summary>
		/// Makes sure the collider matches our specs, and the angle is right. 
		/// If yes, triggers the dash sequence
		/// </summary>
		protected virtual void TestForDash(Collider collider)
		{
			// if there's already a dash in progress, we do nothing and exit
			if ((_dashInProgress == true) || !DashAuthorized)
			{
				return;
			}

			// we make sure it's the right kind of character
			_character = collider.gameObject.MMGetComponentNoAlloc<Character>();
			_characterDash3D = _character?.FindAbility<CharacterDash3D>();
			if (_characterDash3D == null)
			{
				return;
			}

			// we make sure the angle is right
			_characterOrientation3D = _character?.FindAbility<CharacterOrientation3D>();

			_direction1 = (_character.CharacterModel.transform.forward ).normalized;
			_direction1.y = 0f;
			_direction2 = (this.transform.forward).normalized;
			_direction2.y = 0f;
            
			float angle = Vector3.Angle(_direction1, _direction2);
			if (angle > MaxFacingAngle)
			{
				return;
			}

			// we trigger the dash
			_characterHandleWeapon = _character?.FindAbility<CharacterHandleWeapon>();

			// we start the sequence
			StartCoroutine(DashSequence());
		}

		/// <summary>
		/// Sets the dash properties, triggers the dash, and resets everything afterwards
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator DashSequence()
		{
			_dashInProgress = true;

			// we set our dash's properties
			_characterDash3D.DashDistance = DashDistance;
			_characterDash3D.DashCurve = DashCurve;
			_characterDash3D.DashDuration = DashDuration;
			_characterDash3D.DashDirection = this.transform.forward;

			// we turn off input detection
			_character.LinkedInputManager.InputDetectionActive = false;

			// we force rotation on the model
			if (_characterOrientation3D != null)
			{
				_rotationMode = _characterOrientation3D.RotationMode;
				_characterOrientation3D.RotationMode = CharacterOrientation3D.RotationModes.MovementDirection;
				_characterOrientation3D.ForcedRotation = true;
				_characterOrientation3D.ForcedRotationDirection = ((this.transform.position + this.transform.forward * 10) - this.transform.position).normalized;
			}

			// we force rotation on the weapon
			if (_characterHandleWeapon != null)
			{
				if (_characterHandleWeapon.CurrentWeapon != null)
				{
					_weaponAim3D = _characterHandleWeapon.CurrentWeapon.gameObject.MMGetComponentNoAlloc<WeaponAim3D>();
					if (_weaponAim3D != null)
					{
						_weaponAimControl = _weaponAim3D.AimControl;
						_weaponAim3D.AimControl = WeaponAim.AimControls.Script;
						_weaponAim3D.SetCurrentAim(((this.transform.position + this.transform.forward * 10) - this.transform.position).normalized);
					}
				}
			}

			// we disable obstacle collisions
			CoverObstacleCollider.enabled = false;

			// we turn triggers off
			SetColliderTrigger(false);
			foreach(DashZone3D dashZone in ExitDashZones)
			{
				dashZone.SetColliderTrigger(false);
			}            

			// we start dashing
			_characterDash3D.DashStart();
                       
			// we wait for the duration of the dash
			yield return _dashWaitForSeconds;

			// we put everything back as it was
			_character.LinkedInputManager.InputDetectionActive = true;
			if (_characterOrientation3D != null)
			{
				_characterOrientation3D.ForcedRotation = false;
				_characterOrientation3D.RotationMode = _rotationMode;
			}            
			if (_weaponAim3D != null)
			{
				_weaponAim3D.AimControl = _weaponAimControl;
			}            
			CoverObstacleCollider.enabled = true;

			// we wait to turn the triggers back on again
			yield return _triggerResetForSeconds;

			// we turn our triggers back on
			SetColliderTrigger(true);
			foreach (DashZone3D dashZone in ExitDashZones)
			{
				dashZone.SetColliderTrigger(true);
			}

			_dashInProgress = false;
		}

		/// <summary>
		/// Sets this collider's trigger off or on
		/// </summary>
		/// <param name="status"></param>
		public virtual void SetColliderTrigger(bool status)
		{
			_collider.enabled = status;
		}
        
	}
}