using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 모델이 무기의 타겟을 조준하도록 강제하는 데 사용되는 클래스
    /// </summary>
    [AddComponentMenu("TopDown Engine/Weapons/Weapon Model")]
	public class WeaponModel : TopDownMonoBehaviour
	{
		[Header("Model")]
		/// a unique ID that will be used to hide / show this model when the corresponding weapon gets equipped
		[Tooltip("해당 무기가 장착될 때 이 모델을 숨기거나 표시하는 데 사용되는 고유 ID")]
		public string WeaponID = "WeaponID";
		/// a GameObject to show/hide for this model, usually nested right below the logic level of the WeaponModel
		[Tooltip("이 모델에 대해 표시/숨기기 위한 GameObject. 일반적으로 WeaponModel의 로직 레벨 바로 아래에 중첩됩니다.")]
		public GameObject TargetModel;

		[Header("Aim")]
		/// if this is true, the model will aim at the parent weapon's target
		[Tooltip("이것이 사실이라면 모델은 상위 무기의 목표물을 조준할 것입니다.")]
		public bool AimWeaponModelAtTarget = true;
		/// if this is true, the model's aim will be vertically locked (no up/down aiming)
		[Tooltip("이것이 사실이라면 모델의 조준은 수직으로 고정됩니다(위/아래 조준 없음).")]
		public bool LockVerticalRotation = true;

		[Header("Animator")]
		/// whether or not to add the target animator to the real weapon's animator list
		[Tooltip("실제 무기의 애니메이터 목록에 대상 애니메이터를 추가할지 여부")]
		public bool AddAnimator = false;
		/// the animator to send weapon animation parameters to
		[Tooltip("무기 애니메이션 매개변수를 보낼 애니메이터")]
		public Animator TargetAnimator;

		[Header("SpawnTransform")]
		/// whether or not to override the weapon use transform
		[Tooltip("무기 사용 변환을 재정의할지 여부")]
		public bool OverrideWeaponUseTransform = false;
		/// a transform to use as the spawn point for weapon use (if null, only offset will be considered, otherwise the transform without offset)
		[Tooltip("무기 사용을 위한 스폰 지점으로 사용할 변환(null인 경우 오프셋만 고려되고 그렇지 않으면 오프셋 없는 변환)")]
		public Transform WeaponUseTransform;

		[Header("IK")]
		/// whether or not to use IK with this model
		[Tooltip("이 모델에 IK를 사용할지 여부")]
		public bool UseIK = false;
		/// the transform to which the character's left hand should be attached to
		[Tooltip("캐릭터의 왼손이 연결되어야 하는 변환")]
		public Transform LeftHandHandle;
		/// the transform to which the character's right hand should be attached to
		[Tooltip("캐릭터의 오른손이 연결되어야 하는 변환")]
		public Transform RightHandHandle;

		[Header("Feedbacks")]
		/// if this is true, the model's feedbacks will replace the original weapon's feedbacks
		[Tooltip("이것이 사실이라면 모델의 피드백이 원래 무기의 피드백을 대체합니다.")]
		public bool BindFeedbacks = true;
		/// the feedback to play when the weapon starts being used
		[Tooltip("무기가 사용되기 시작할 때 재생할 피드백")]
		public MMFeedbacks WeaponStartMMFeedback;
		/// the feedback to play while the weapon is in use
		[Tooltip("무기를 사용하는 동안 플레이할 피드백")]
		public MMFeedbacks WeaponUsedMMFeedback;
		/// the feedback to play when the weapon stops being used
		[Tooltip("무기 사용이 중단될 때 재생할 피드백")]
		public MMFeedbacks WeaponStopMMFeedback;
		/// the feedback to play when the weapon gets reloaded
		[Tooltip("무기가 재장전될 때 재생할 피드백")]
		public MMFeedbacks WeaponReloadMMFeedback;
		/// the feedback to play when the weapon gets reloaded
		[Tooltip("무기가 재장전될 때 재생할 피드백")]
		public MMFeedbacks WeaponReloadNeededMMFeedback;

		public CharacterHandleWeapon Owner { get; set; }
		
		protected List<CharacterHandleWeapon> _handleWeapons;
		protected WeaponAim _weaponAim;
		protected Vector3 _rotationDirection;

		protected virtual void Awake()
		{
			Hide();
		}

		/// <summary>
		/// On Start we grab our CharacterHandleWeapon component
		/// </summary>
		protected virtual void Start()
		{
			_handleWeapons = this.GetComponentInParent<Character>()?.FindAbilities<CharacterHandleWeapon>();
		}

		/// <summary>
		/// Aims the weapon model at the target
		/// </summary>
		protected virtual void Update()
		{
			if (!AimWeaponModelAtTarget)
			{
				return;
			}

			if (_weaponAim == null)
			{
				foreach (CharacterHandleWeapon handleWeapon in _handleWeapons)
				{
					if (handleWeapon.CurrentWeapon != null)
					{
						_weaponAim = handleWeapon.CurrentWeapon.gameObject.MMGetComponentNoAlloc<WeaponAim>();
					}
				}               
			}
			else
			{
				_rotationDirection = _weaponAim.CurrentAim.normalized;
				if (LockVerticalRotation)
				{
					_rotationDirection.y = 0;
				}
				this.transform.LookAt(_weaponAim.transform.position + 10f * _rotationDirection);
			}
		}

		public virtual void Show(CharacterHandleWeapon handleWeapon)
		{
			Owner = handleWeapon;
			TargetModel.SetActive(true);
		}

		public virtual void Hide()
		{
			TargetModel.SetActive(false);
		}
	}
}