using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;
using System.Collections.Generic;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
	/// ���⸦ ����� �� �ֵ��� ĳ���Ϳ� �� Ŭ������ �߰��մϴ�. 
	/// �� ���� ��Ҵ� ���� �˻�⿡�� ���ǵ� ���� ������ �ִϸ��̼� �ִϸ����� �Ű� ������ ������� �ִϸ��̼��� Ʈ�����մϴ�(�ش� �Ű� ������ �ִϸ����Ϳ� �ִ� ���).
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/Abilities/Character Handle Weapon")]
	public class CharacterHandleWeapon : CharacterAbility
	{
		/// This method is only used to display a helpbox text at the beginning of the ability's inspector
		public override string HelpBoxText() { return "�� ���� ��Ҹ� ����ϸ� ĳ���Ͱ� ���⸦ ���� ��� ����� �� �ֽ��ϴ�. ���Ⱑ ������ �۾��� Weapon Ŭ������ ���ǵǾ� �ֽ��ϴ�. �̴� ���� ��ü�� �ƴ϶� ���⸦ ��� �ִ� '��'�� ������ ������ ���Դϴ�. ���⼭�� ĳ���Ͱ� ������ �ʱ� ���⸦ �����ϰ�, ���� �Ⱦ��� ����ϰ�, ���� �������� ������ �� �ֽ��ϴ�(ĳ���� ���� ��ȯ�� ���� �� ���� ���� ��ü�� ���� �ְ� ���� ���� �κ��� ���� ����)."; }

		[Header("Weapon")]

        /// ĳ���Ͱ� ������ �ʱ� ����
        [Tooltip("ĳ���Ͱ� ������ �ʱ� ����")]
		public Weapon InitialWeapon;
        /// true�� �����Ǹ� ĳ���Ͱ� PickableWeapons�� ���� �� �ֽ��ϴ�.
        [Tooltip("true�� �����Ǹ� ĳ���Ͱ� PickableWeapons�� ���� �� �ֽ��ϴ�.")]
		public bool CanPickupWeapons = true;

        [Header("Feedbacks")]
        /// ���⸦ ����� ������ ĳ���� ���ؿ��� Ʈ���ŵǴ� �ǵ��
        [Tooltip("���⸦ ����� ������ ĳ���� ���ؿ��� Ʈ���ŵǴ� �ǵ��")]
		public MMFeedbacks WeaponUseFeedback;

		[Header("Binding")]
        /// ���Ⱑ ������ ��ġ�Դϴ�. ��ĭ���� ���θ� �̰��� �˴ϴ�. ��ȯ.
        [Tooltip("���Ⱑ ������ ��ġ�Դϴ�. ��ĭ���� ���θ� �̰��� �˴ϴ�. ��ȯ.")]
		public Transform WeaponAttachment;
        /// �߻�ü�� ������ ��ġ(����ξ ������)
        [Tooltip("�߻�ü�� ������ ��ġ(����ξ ������)")]
		public Transform ProjectileSpawn;
        /// �̰��� ����̶�� �� �ִϸ����ʹ� �ڵ����� ���⿡ ���ε��˴ϴ�.
        [Tooltip("�̰��� ����̶�� �� �ִϸ����ʹ� �ڵ����� ���⿡ ���ε��˴ϴ�.")]
		public bool AutomaticallyBindAnimator = true;
        /// �� ����� ������Ʈ�ؾ� �ϴ� AmmoDisplay�� ID
        [Tooltip("�� ����� ������Ʈ�ؾ� �ϴ� AmmoDisplay�� ID")]
		public int AmmoDisplayID = 0;
        /// �̰��� ����̶�� ������ ��� IK�� �ڵ����� �����˴ϴ�.
        [Tooltip("�̰��� ����̶�� ������ ��� IK�� �ڵ����� �����˴ϴ�.")]
		public bool AutoIK = true;

		[Header("Input")]
        /// �̰��� ����̶�� �ڵ� �������� ���� �߻� ��ư�� ���� �ʿ䰡 �����ϴ�.
        [Tooltip("�̰��� ����̶�� �ڵ� �������� ���� �߻� ��ư�� ���� �ʿ䰡 �����ϴ�.")]
		public bool ContinuousPress = false;
        /// ������ �޴� ĳ���Ͱ� ������ �ߴ��ؾ� �ϴ��� ����(���Ⱑ �ߴ� �������� ǥ�õ� ��쿡�� �۵���)
        [Tooltip("������ �޴� ĳ���Ͱ� ������ �ߴ��ؾ� �ϴ��� ����(���Ⱑ �ߴ� �������� ǥ�õ� ��쿡�� �۵���)")]
		public bool GettingHitInterruptsAttack = false;
        /// ���� ���� �Ӱ谪 �̻����� �и� ���Ⱑ �߻�Ǵ��� ����
        [Tooltip("���� ���� �Ӱ谪 �̻����� �и� ���Ⱑ �߻�Ǵ��� ����")]
		public bool UseSecondaryAxisThresholdToShoot = false;
        /// �̰��� ����̶�� ForcedWeaponAimControl ���� �� ĳ���Ͱ� ������ ��� ���⿡ ����˴ϴ�.
        [Tooltip("�̰��� ����̶�� ForcedWeaponAimControl ���� �� ĳ���Ͱ� ������ ��� ���⿡ ����˴ϴ�.")]
		public bool ForceWeaponAimControl = false;
        /// ForceWeaponAimControl�� true�� ��� �� ĳ���Ͱ� ������ ��� ���⿡ ������ AimControls ���
        [Tooltip("ForceWeaponAimControl�� true�� ��� �� ĳ���Ͱ� ������ ��� ���⿡ ������ AimControls ���")]
		[MMCondition("ForceWeaponAimControl", true)]
		public WeaponAim.AimControls ForcedWeaponAimControl = WeaponAim.AimControls.PrimaryMovement;
        /// �̰��� ����̶�� ĳ���ʹ� ����ؼ� ���⸦ �߻��� ���Դϴ�.
        [Tooltip("�̰��� ����̶�� ĳ���ʹ� ����ؼ� ���⸦ �߻��� ���Դϴ�.")]
		public bool ForceAlwaysShoot = false;

		[Header("Buffering")]
        /// ���� �Է��� ���۸��ؾ� �ϴ��� ���θ� ���� �ٸ� ������ ����Ǵ� ���� ������ �غ��� �� �־� ���� ������ �� �������ϴ�.
        [Tooltip("NewInputExtendsBuffer�ð� ���� ���Է� �صθ� ����ؼ� �������� ���ϴ°�")]
		public bool BufferInput;
        /// �̰��� ����̶�� ��� ���ο� �Է��� ���۸� �����ŵ�ϴ�.
        [MMCondition("BufferInput", true)]
		[Tooltip("�̰��� ����̶�� ��� ���ο� �Է��� ���۸� �����ŵ�ϴ�.")]
		public bool NewInputExtendsBuffer;
        /// ������ �ִ� ���� �ð�(��)
        [MMCondition("BufferInput", true)]
		[Tooltip("������ �ִ� ���� �ð�(��)")]
		public float MaximumBufferDuration = 0.25f;
        /// �̰��� ����̰� �� ĳ���Ͱ� GridMovement�� ����ϰ� �ִٸ� �Է��� �Ϻ��� Ÿ�Ͽ� ���� ���� Ʈ���ŵ˴ϴ�.
        [MMCondition("BufferInput", true)]
		[Tooltip("�̰��� ����̰� �� ĳ���Ͱ� GridMovement�� ����ϰ� �ִٸ� �Է��� �Ϻ��� Ÿ�Ͽ� ���� ���� Ʈ���ŵ˴ϴ�.")]
		public bool RequiresPerfectTile = false;
        
		[Header("Debug")]

        /// ĳ���Ͱ� ���� �����ϰ� �ִ� ����
        [MMReadOnly]
		[Tooltip("ĳ���Ͱ� ���� �����ϰ� �ִ� ����")]
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
        /// �پ��� ���� ��Ҹ� ��� ������ �ʱ�ȭ�մϴ�.
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
        /// ���� �׸� ���� �Է� �� Ʈ���� �޼��带 �����ɴϴ�.
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
        /// ĳ���Ͱ� �Ѱ��� �����ϰ� ����ϴ�.
        /// </summary>
        public virtual void ShootStart()
		{
            // ���ѿ��� Shoot �۾��� Ȱ��ȭ�Ǿ� ������ ��� �����ϰ�, �׷��� ������ �ƹ� �۾��� �������� �ʽ��ϴ�. �÷��̾ ������ �츮�� �ƹ��͵� ���� �ʽ��ϴ�.
            if (!AbilityAuthorized
			    || (CurrentWeapon == null)
			    || (_condition.CurrentState != CharacterStates.CharacterConditions.Normal))
			{
				return;
			}

            //  �Է��� ���۸��ϱ�� �����߰� ���Ⱑ ���� ��� ���̶��
            if (BufferInput && (CurrentWeapon.WeaponState.CurrentState != Weapon.WeaponStates.WeaponIdle))
			{
                // ���� ���۸� ���� �ƴϰų� ������ �� �Է��� ���۸� Ȯ���ϴ� ��� ���۸� ���¸� true�� �����մϴ�.
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
        /// ĳ������ Shoot�� �ߴܵ˴ϴ�
        /// </summary>
        public virtual void ShootStop()
		{
            // ���ѿ��� Shoot �۾��� Ȱ��ȭ�Ǿ� ������ ��� �����ϰ� �׷��� ������ �ƹ� �۾��� �������� �ʽ��ϴ�.
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
        /// ���⸦ ������ ������ŵ�ϴ�.
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
        /// ĳ������ ���� ���⸦ �Ű������� ���޵� ����� �����մϴ�.
        /// </summary>
        /// <param name="newWeapon">The new weapon.</param>
        public virtual void ChangeWeapon(Weapon newWeapon, string weaponID, bool combo = false)
		{
            // ĳ���Ͱ� �̹� ���⸦ ������ �ִٸ�, �Ѱ��� ���߰� �մϴ�.
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
					//������Ʈ�� �׾����� ���⸦ ���ֱ淡 �Ⱦ��ְ� ���⸸ ����
					CurrentWeapon.gameObject.SetActive(false);
                    //Destroy(CurrentWeapon.gameObject);
                }
			}

			if (newWeapon != null)
			{
				InstantiateWeapon(newWeapon, weaponID, combo);
			}
			else
			{
				//CurrentWeapon = null;
			}

			if (OnWeaponChange != null)
			{
				OnWeaponChange();
			}
		}

        /// <summary>
        /// ������ ���⸦ �ν��Ͻ�ȭ�մϴ�.
        /// </summary>
        /// <param name="newWeapon"></param>
        /// <param name="weaponID"></param>
        /// <param name="combo"></param>
        protected virtual void InstantiateWeapon(Weapon newWeapon, string weaponID, bool combo = false)
		{
			if (!combo)
			{
				if(CurrentWeapon == null)
				{
                    CurrentWeapon = (Weapon)Instantiate(newWeapon, WeaponAttachment.transform.position + newWeapon.WeaponAttachmentOffset, WeaponAttachment.transform.rotation);
				}
				else
					CurrentWeapon.gameObject.SetActive(true);
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