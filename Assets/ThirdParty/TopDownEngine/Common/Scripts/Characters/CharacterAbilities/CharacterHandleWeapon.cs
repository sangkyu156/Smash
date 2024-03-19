using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;
using System.Collections.Generic;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
	/// 무기를 사용할 수 있도록 캐릭터에 이 클래스를 추가합니다. 
	/// 이 구성 요소는 무기 검사기에서 정의된 현재 무기의 애니메이션 애니메이터 매개 변수를 기반으로 애니메이션을 트리거합니다(해당 매개 변수가 애니메이터에 있는 경우).
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/Abilities/Character Handle Weapon")]
	public class CharacterHandleWeapon : CharacterAbility
	{
		/// This method is only used to display a helpbox text at the beginning of the ability's inspector
		public override string HelpBoxText() { return "This component will allow your character to pickup and use weapons. What the weapon will do is defined in the Weapon classes. This just describes the behaviour of the 'hand' holding the weapon, not the weapon itself. Here you can set an initial weapon for your character to start with, allow weapon pickup, and specify a weapon attachment (a transform inside of your character, could be just an empty child gameobject, or a subpart of your model."; }

		[Header("Weapon")]

        /// 캐릭터가 소유한 초기 무기
        [Tooltip("캐릭터가 소유한 초기 무기")]
		public Weapon InitialWeapon;
        /// true로 설정되면 캐릭터가 PickableWeapons를 집을 수 있습니다.
        [Tooltip("true로 설정되면 캐릭터가 PickableWeapons를 집을 수 있습니다.")]
		public bool CanPickupWeapons = true;

		[Header("Feedbacks")]
        /// 무기를 사용할 때마다 캐릭터 수준에서 트리거되는 피드백
        [Tooltip("무기를 사용할 때마다 캐릭터 수준에서 트리거되는 피드백")]
		public MMFeedbacks WeaponUseFeedback;

		[Header("Binding")]
        /// 무기가 부착될 위치입니다. 빈칸으로 놔두면 이것이 됩니다. 변환.
        [Tooltip("무기가 부착될 위치입니다. 빈칸으로 놔두면 이것이 됩니다. 변환.")]
		public Transform WeaponAttachment;
        /// 발사체가 생성될 위치(비워두어도 안전함)
        [Tooltip("발사체가 생성될 위치(비워두어도 안전함)")]
		public Transform ProjectileSpawn;
        /// 이것이 사실이라면 이 애니메이터는 자동으로 무기에 바인딩됩니다.
        [Tooltip("이것이 사실이라면 이 애니메이터는 자동으로 무기에 바인딩됩니다.")]
		public bool AutomaticallyBindAnimator = true;
        /// 이 기능이 업데이트해야 하는 AmmoDisplay의 ID
        [Tooltip("이 기능이 업데이트해야 하는 AmmoDisplay의 ID")]
		public int AmmoDisplayID = 0;
        /// 이것이 사실이라면 가능한 경우 IK가 자동으로 설정됩니다.
        [Tooltip("이것이 사실이라면 가능한 경우 IK가 자동으로 설정됩니다.")]
		public bool AutoIK = true;

		[Header("Input")]
        /// 이것이 사실이라면 자동 재장전을 위해 발사 버튼을 놓을 필요가 없습니다.
        [Tooltip("이것이 사실이라면 자동 재장전을 위해 발사 버튼을 놓을 필요가 없습니다.")]
		public bool ContinuousPress = false;
        /// 공격을 받는 캐릭터가 공격을 중단해야 하는지 여부(무기가 중단 가능으로 표시된 경우에만 작동함)
        [Tooltip("공격을 받는 캐릭터가 공격을 중단해야 하는지 여부(무기가 중단 가능으로 표시된 경우에만 작동함)")]
		public bool GettingHitInterruptsAttack = false;
        /// 보조 축을 임계값 이상으로 밀면 무기가 발사되는지 여부
        [Tooltip("보조 축을 임계값 이상으로 밀면 무기가 발사되는지 여부")]
		public bool UseSecondaryAxisThresholdToShoot = false;
        /// 이것이 사실이라면 ForcedWeaponAimControl 모드는 이 캐릭터가 장착한 모든 무기에 적용됩니다.
        [Tooltip("이것이 사실이라면 ForcedWeaponAimControl 모드는 이 캐릭터가 장착한 모든 무기에 적용됩니다.")]
		public bool ForceWeaponAimControl = false;
        /// ForceWeaponAimControl이 true인 경우 이 캐릭터가 장착한 모든 무기에 적용할 AimControls 모드
        [Tooltip("ForceWeaponAimControl이 true인 경우 이 캐릭터가 장착한 모든 무기에 적용할 AimControls 모드")]
		[MMCondition("ForceWeaponAimControl", true)]
		public WeaponAim.AimControls ForcedWeaponAimControl = WeaponAim.AimControls.PrimaryMovement;
        /// 이것이 사실이라면 캐릭터는 계속해서 무기를 발사할 것입니다.
        [Tooltip("이것이 사실이라면 캐릭터는 계속해서 무기를 발사할 것입니다.")]
		public bool ForceAlwaysShoot = false;

		[Header("Buffering")]
        /// 공격 입력을 버퍼링해야 하는지 여부를 통해 다른 공격이 수행되는 동안 공격을 준비할 수 있어 공격 연결이 더 쉬워집니다.
        [Tooltip("공격 입력을 버퍼링해야 하는지 여부를 통해 다른 공격이 수행되는 동안 공격을 준비할 수 있어 공격 연결이 더 쉬워집니다.")]
		public bool BufferInput;
        /// 이것이 사실이라면 모든 새로운 입력은 버퍼를 연장시킵니다.
        [MMCondition("BufferInput", true)]
		[Tooltip("이것이 사실이라면 모든 새로운 입력은 버퍼를 연장시킵니다.")]
		public bool NewInputExtendsBuffer;
        /// 버퍼의 최대 지속 시간(초)
        [MMCondition("BufferInput", true)]
		[Tooltip("버퍼의 최대 지속 시간(초)")]
		public float MaximumBufferDuration = 0.25f;
        /// 이것이 사실이고 이 캐릭터가 GridMovement를 사용하고 있다면 입력은 완벽한 타일에 있을 때만 트리거됩니다.
        [MMCondition("BufferInput", true)]
		[Tooltip("이것이 사실이고 이 캐릭터가 GridMovement를 사용하고 있다면 입력은 완벽한 타일에 있을 때만 트리거됩니다.")]
		public bool RequiresPerfectTile = false;
        
		[Header("Debug")]

        /// 캐릭터가 현재 장착하고 있는 무기
        [MMReadOnly]
		[Tooltip("캐릭터가 현재 장착하고 있는 무기")]
		public Weapon CurrentWeapon;

		/// the ID / index of this CharacterHandleWeapon. This will be used to determine what handle weapon ability should equip a weapon.
		/// If you create more Handle Weapon abilities, make sure to override and increment this  
		public virtual int HandleWeaponID { get { return 1; } }

		/// an animator to update when the weapon is used
		public Animator CharacterAnimator { get; set; }
		/// the weapon's weapon aim component, if it has one
		public WeaponAim WeaponAimComponent { get { return _weaponAim; } }

		public delegate void OnWeaponChangeDelegate();
		/// a delegate you can hook to, to be notified of weapon changes
		public OnWeaponChangeDelegate OnWeaponChange;

		protected float _fireTimer = 0f;
		protected float _secondaryHorizontalMovement;
		protected float _secondaryVerticalMovement;
		protected WeaponAim _weaponAim;
		protected ProjectileWeapon _projectileWeapon;
		protected WeaponIK _weaponIK;
		protected Transform _leftHandTarget = null;
		protected Transform _rightHandTarget = null;
		protected float _bufferEndsAt = 0f;
		protected bool _buffering = false;
		protected const string _weaponEquippedAnimationParameterName = "WeaponEquipped";
		protected const string _weaponEquippedIDAnimationParameterName = "WeaponEquippedID";
		protected int _weaponEquippedAnimationParameter;
		protected int _weaponEquippedIDAnimationParameter;
		protected CharacterGridMovement _characterGridMovement;
		protected List<WeaponModel> _weaponModels;
        

        /// <summary>
        /// Sets the weapon attachment
        /// </summary>
        protected override void PreInitialization()
		{
			base.PreInitialization();
			// filler if the WeaponAttachment has not been set
			if (WeaponAttachment == null)
			{
				WeaponAttachment = transform;
			}
		}

		// Initialization
		protected override void Initialization()
		{
			base.Initialization();
			Setup();
		}

		/// <summary>
		/// Grabs various components and inits stuff
		/// </summary>
		public virtual void Setup()
		{
			_character = this.gameObject.GetComponentInParent<Character>();
			_characterGridMovement = _character?.FindAbility<CharacterGridMovement>();
			_weaponModels = new List<WeaponModel>();
			foreach (WeaponModel model in _character.gameObject.GetComponentsInChildren<WeaponModel>())
			{
				_weaponModels.Add(model);
			}
			CharacterAnimator = _animator;
			// filler if the WeaponAttachment has not been set
			if (WeaponAttachment == null)
			{
				WeaponAttachment = transform;
			}
			if ((_animator != null) && (AutoIK))
			{
				_weaponIK = _animator.GetComponent<WeaponIK>();
			}
			// we set the initial weapon
			if (InitialWeapon != null)
			{
				if (CurrentWeapon != null)
				{
					if (CurrentWeapon.name != InitialWeapon.name)
					{
						ChangeWeapon(InitialWeapon, InitialWeapon.WeaponName, false);    
					}
				}
				else
				{
					ChangeWeapon(InitialWeapon, InitialWeapon.WeaponName, false);    
				}
			}
		}

		/// <summary>
		/// Every frame we check if it's needed to update the ammo display
		/// </summary>
		public override void ProcessAbility()
		{
			base.ProcessAbility();
			HandleCharacterState();
			HandleFeedbacks();
			UpdateAmmoDisplay();
			HandleBuffer();
		}

		/// <summary>
		/// Checks character state and stops shooting if not in normal state
		/// </summary>
		protected virtual void HandleCharacterState()
		{
			if (_condition.CurrentState != CharacterStates.CharacterConditions.Normal)
			{
				ShootStop();
			}
		}

		/// <summary>
		/// Triggers the weapon used feedback if needed
		/// </summary>
		protected virtual void HandleFeedbacks()
		{
			if (CurrentWeapon != null)
			{
				if (CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponUse)
				{
					WeaponUseFeedback?.PlayFeedbacks();
				}
			}
		}

        /// <summary>
        /// 누른 항목에 따라 입력 및 트리거 메서드를 가져옵니다.
        /// </summary>
        protected override void HandleInput()
		{
			if (!AbilityAuthorized
			    || (_condition.CurrentState != CharacterStates.CharacterConditions.Normal)
			    || (CurrentWeapon == null))
			{
				return;
			}

			bool inputAuthorized = true;
			if (CurrentWeapon != null)
			{
				inputAuthorized = CurrentWeapon.InputAuthorized;
			}

			if (ForceAlwaysShoot)
			{
				ShootStart();
			}
			
			if (inputAuthorized && ((_inputManager.ShootButton.State.CurrentState == MMInput.ButtonStates.ButtonDown) || (_inputManager.ShootAxis == MMInput.ButtonStates.ButtonDown)))
			{
				ShootStart();
			}

			bool buttonPressed =
				(_inputManager.ShootButton.State.CurrentState == MMInput.ButtonStates.ButtonPressed) ||
				(_inputManager.ShootAxis == MMInput.ButtonStates.ButtonPressed); 
                
			if (inputAuthorized && ContinuousPress && (CurrentWeapon.TriggerMode == Weapon.TriggerModes.Auto) && buttonPressed)
			{
				ShootStart();
			}
            
			if (_inputManager.ReloadButton.State.CurrentState == MMInput.ButtonStates.ButtonDown)
			{
				Reload();
			}

			if (inputAuthorized && ((_inputManager.ShootButton.State.CurrentState == MMInput.ButtonStates.ButtonUp) || (_inputManager.ShootAxis == MMInput.ButtonStates.ButtonUp)))
			{
				ShootStop();
				CurrentWeapon.WeaponInputReleased();
			}

			if ((CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponDelayBetweenUses)
			    && ((_inputManager.ShootAxis == MMInput.ButtonStates.Off) && (_inputManager.ShootButton.State.CurrentState == MMInput.ButtonStates.Off))
			    && !(UseSecondaryAxisThresholdToShoot && (_inputManager.SecondaryMovement.magnitude > _inputManager.Threshold.magnitude)))
			{
				CurrentWeapon.WeaponInputStop();
			}

			if (inputAuthorized && UseSecondaryAxisThresholdToShoot && (_inputManager.SecondaryMovement.magnitude > _inputManager.Threshold.magnitude))
			{
				ShootStart();
			}
		}

		/// <summary>
		/// Triggers an attack if the weapon is idle and an input has been buffered
		/// </summary>
		protected virtual void HandleBuffer()
		{
			if (CurrentWeapon == null)
			{
				return;
			}
            
			// if we are currently buffering an input and if the weapon is now idle
			if (_buffering && (CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponIdle))
			{
				// and if our buffer is still valid, we trigger an attack
				if (Time.time < _bufferEndsAt)
				{
					ShootStart();
				}
				else
				{
					_buffering = false;
				}                
			}
		}

		/// <summary>
		/// Causes the character to start shooting
		/// </summary>
		public virtual void ShootStart()
		{
			// if the Shoot action is enabled in the permissions, we continue, if not we do nothing.  If the player is dead we do nothing.
			if (!AbilityAuthorized
			    || (CurrentWeapon == null)
			    || (_condition.CurrentState != CharacterStates.CharacterConditions.Normal))
			{
				return;
			}

			//  if we've decided to buffer input, and if the weapon is in use right now
			if (BufferInput && (CurrentWeapon.WeaponState.CurrentState != Weapon.WeaponStates.WeaponIdle))
			{
				// if we're not already buffering, or if each new input extends the buffer, we turn our buffering state to true
				ExtendBuffer();
			}

			if (BufferInput && RequiresPerfectTile && (_characterGridMovement != null))            
			{
				if (!_characterGridMovement.PerfectTile)
				{
					ExtendBuffer();
					return;
				}
				else
				{
					_buffering = false;
				}
			}
			PlayAbilityStartFeedbacks();
			CurrentWeapon.WeaponInputStart();
		}

		/// <summary>
		/// Extends the duration of the buffer if needed
		/// </summary>
		protected virtual void ExtendBuffer()
		{
			if (!_buffering || NewInputExtendsBuffer)
			{
				_buffering = true;
				_bufferEndsAt = Time.time + MaximumBufferDuration;
			}
		}

		/// <summary>
		/// Causes the character to stop shooting
		/// </summary>
		public virtual void ShootStop()
		{
			// if the Shoot action is enabled in the permissions, we continue, if not we do nothing
			if (!AbilityAuthorized
			    || (CurrentWeapon == null))
			{
				return;
			}

			if (CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponIdle)
			{
				return;
			}

			if ((CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponReload)
			    || (CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponReloadStart)
			    || (CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponReloadStop))
			{
				return;
			}

			if ((CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponDelayBeforeUse) && (!CurrentWeapon.DelayBeforeUseReleaseInterruption))
			{
				return;
			}

			if ((CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponDelayBetweenUses) && (!CurrentWeapon.TimeBetweenUsesReleaseInterruption))
			{
				return;
			}

			if (CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponUse) 
			{
				return;
			}

			ForceStop();
		}

		/// <summary>
		/// Forces the weapon to stop 
		/// </summary>
		public virtual void ForceStop()
		{
			StopStartFeedbacks();
			PlayAbilityStopFeedbacks();
			if (CurrentWeapon != null)
			{
				CurrentWeapon.TurnWeaponOff();    
			}
		}

		/// <summary>
		/// Reloads the weapon
		/// </summary>
		public virtual void Reload()
		{
			if (CurrentWeapon != null)
			{
				CurrentWeapon.InitiateReloadWeapon();
			}
		}

		/// <summary>
		/// Changes the character's current weapon to the one passed as a parameter
		/// </summary>
		/// <param name="newWeapon">The new weapon.</param>
		public virtual void ChangeWeapon(Weapon newWeapon, string weaponID, bool combo = false)
		{
			// if the character already has a weapon, we make it stop shooting
			if (CurrentWeapon != null)
			{
				CurrentWeapon.TurnWeaponOff();
				if (!combo)
				{
					ShootStop();

					if (_weaponAim != null) { _weaponAim.RemoveReticle(); }
					if (_character._animator != null)
					{
						AnimatorControllerParameter[] parameters = _character._animator.parameters;
						foreach(AnimatorControllerParameter parameter in parameters)
						{
							if (parameter.name == CurrentWeapon.EquippedAnimationParameter)
							{
								MMAnimatorExtensions.UpdateAnimatorBool(_animator, CurrentWeapon.EquippedAnimationParameter, false);
							}
						}
					}
					Destroy(CurrentWeapon.gameObject);
				}
			}

			if (newWeapon != null)
			{
				InstantiateWeapon(newWeapon, weaponID, combo);
			}
			else
			{
				CurrentWeapon = null;
			}

			if (OnWeaponChange != null)
			{
				OnWeaponChange();
			}
		}

		/// <summary>
		/// Instantiates the specified weapon
		/// </summary>
		/// <param name="newWeapon"></param>
		/// <param name="weaponID"></param>
		/// <param name="combo"></param>
		protected virtual void InstantiateWeapon(Weapon newWeapon, string weaponID, bool combo = false)
		{
			if (!combo)
			{
				CurrentWeapon = (Weapon)Instantiate(newWeapon, WeaponAttachment.transform.position + newWeapon.WeaponAttachmentOffset, WeaponAttachment.transform.rotation);
			}

			CurrentWeapon.name = newWeapon.name;
			CurrentWeapon.transform.parent = WeaponAttachment.transform;
			CurrentWeapon.transform.localPosition = newWeapon.WeaponAttachmentOffset;
			CurrentWeapon.SetOwner(_character, this);
			CurrentWeapon.WeaponID = weaponID;
			CurrentWeapon.FlipWeapon();
			_weaponAim = CurrentWeapon.gameObject.MMGetComponentNoAlloc<WeaponAim>();

			HandleWeaponAim();

			// we handle (optional) inverse kinematics (IK) 
			HandleWeaponIK();

			// we handle the weapon model
			HandleWeaponModel(newWeapon, weaponID, combo, CurrentWeapon);

			// we turn off the gun's emitters.
			CurrentWeapon.Initialization();
			CurrentWeapon.InitializeComboWeapons();
			CurrentWeapon.InitializeAnimatorParameters();
			InitializeAnimatorParameters();
		}

		/// <summary>
		/// Applies aim if possible
		/// </summary>
		protected virtual void HandleWeaponAim()
		{
			if ((_weaponAim != null) && (_weaponAim.enabled))
			{
				if (ForceWeaponAimControl)
				{
					_weaponAim.AimControl = ForcedWeaponAimControl;
				}
				_weaponAim.ApplyAim();
			}
		}

		/// <summary>
		/// Sets IK handles if needed
		/// </summary>
		protected virtual void HandleWeaponIK()
		{
			if (_weaponIK != null)
			{
				_weaponIK.SetHandles(CurrentWeapon.LeftHandHandle, CurrentWeapon.RightHandHandle);
			}
			_projectileWeapon = CurrentWeapon.gameObject.MMFGetComponentNoAlloc<ProjectileWeapon>();
			if (_projectileWeapon != null)
			{
				_projectileWeapon.SetProjectileSpawnTransform(ProjectileSpawn);
			}
		}

		protected virtual void HandleWeaponModel(Weapon newWeapon, string weaponID, bool combo = false, Weapon weapon = null)
		{
			if (_weaponModels == null)
			{
				return;
			}
			foreach (WeaponModel model in _weaponModels)
			{
				if (model.Owner == this)
				{
					model.Hide();	
				}
				
				if (model.WeaponID == weaponID)
				{
					model.Show(this);
					if (model.UseIK)
					{
						_weaponIK.SetHandles(model.LeftHandHandle, model.RightHandHandle);
					}
					if (weapon != null)
					{
						if (model.BindFeedbacks)
						{
							weapon.WeaponStartMMFeedback = model.WeaponStartMMFeedback;
							weapon.WeaponUsedMMFeedback = model.WeaponUsedMMFeedback;
							weapon.WeaponStopMMFeedback = model.WeaponStopMMFeedback;
							weapon.WeaponReloadMMFeedback = model.WeaponReloadMMFeedback;
							weapon.WeaponReloadNeededMMFeedback = model.WeaponReloadNeededMMFeedback;
						}
						if (model.AddAnimator)
						{
							weapon.Animators.Add(model.TargetAnimator);
						}
						if (model.OverrideWeaponUseTransform)
						{
							weapon.WeaponUseTransform = model.WeaponUseTransform;
						}
					}
				}
			}
		}

		/// <summary>
		/// Flips the current weapon if needed
		/// </summary>
		public override void Flip()
		{
		}

		/// <summary>
		/// Updates the ammo display bar and text.
		/// </summary>
		public virtual void UpdateAmmoDisplay()
		{
			if ((GUIManager.HasInstance) && (_character.CharacterType == Character.CharacterTypes.Player))
			{
				if (CurrentWeapon == null)
				{
					GUIManager.Instance.SetAmmoDisplays(false, _character.PlayerID, AmmoDisplayID);
					return;
				}

				if (!CurrentWeapon.MagazineBased && (CurrentWeapon.WeaponAmmo == null))
				{
					GUIManager.Instance.SetAmmoDisplays(false, _character.PlayerID, AmmoDisplayID);
					return;
				}

				if (CurrentWeapon.WeaponAmmo == null)
				{
					GUIManager.Instance.SetAmmoDisplays(true, _character.PlayerID, AmmoDisplayID);
					GUIManager.Instance.UpdateAmmoDisplays(CurrentWeapon.MagazineBased, 0, 0, CurrentWeapon.CurrentAmmoLoaded, CurrentWeapon.MagazineSize, _character.PlayerID, AmmoDisplayID, false);
					return;
				}
				else
				{
					GUIManager.Instance.SetAmmoDisplays(true, _character.PlayerID, AmmoDisplayID); 
					GUIManager.Instance.UpdateAmmoDisplays(CurrentWeapon.MagazineBased, CurrentWeapon.WeaponAmmo.CurrentAmmoAvailable + CurrentWeapon.CurrentAmmoLoaded, CurrentWeapon.WeaponAmmo.MaxAmmo, CurrentWeapon.CurrentAmmoLoaded, CurrentWeapon.MagazineSize, _character.PlayerID, AmmoDisplayID, true);
					return;
				}
			}
		}

		/// <summary>
		/// Adds required animator parameters to the animator parameters list if they exist
		/// </summary>
		protected override void InitializeAnimatorParameters()
		{
			if (CurrentWeapon == null)
			{ return; }

			RegisterAnimatorParameter(_weaponEquippedAnimationParameterName, AnimatorControllerParameterType.Bool, out _weaponEquippedAnimationParameter);
			RegisterAnimatorParameter(_weaponEquippedIDAnimationParameterName, AnimatorControllerParameterType.Int, out _weaponEquippedIDAnimationParameter);
		}

		/// <summary>
		/// Override this to send parameters to the character's animator. This is called once per cycle, by the Character
		/// class, after Early, normal and Late process().
		/// </summary>
		public override void UpdateAnimator()
		{
			MMAnimatorExtensions.UpdateAnimatorBool(_animator, _weaponEquippedAnimationParameter, (CurrentWeapon != null), _character._animatorParameters, _character.RunAnimatorSanityChecks);
			if (CurrentWeapon == null)
			{
				MMAnimatorExtensions.UpdateAnimatorInteger(_animator, _weaponEquippedIDAnimationParameter, -1, _character._animatorParameters, _character.RunAnimatorSanityChecks);
				return;
			}
			else
			{
				MMAnimatorExtensions.UpdateAnimatorInteger(_animator, _weaponEquippedIDAnimationParameter, CurrentWeapon.WeaponAnimationID, _character._animatorParameters, _character.RunAnimatorSanityChecks);
			}
		}

		protected override void OnHit()
		{
			base.OnHit();
			if (GettingHitInterruptsAttack && (CurrentWeapon != null))
			{
				CurrentWeapon.Interrupt();
			}
		}

		protected override void OnDeath()
		{
			base.OnDeath();
			ShootStop();
			if (CurrentWeapon != null)
			{
				ChangeWeapon(null, "");
			}
		}

		protected override void OnRespawn()
		{
			base.OnRespawn();
			Setup();
		}
	}
}