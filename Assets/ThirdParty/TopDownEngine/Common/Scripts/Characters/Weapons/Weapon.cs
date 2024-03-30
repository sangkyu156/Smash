using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 확장될 예정인 이 기본 클래스(예제는 ProjectileWeapon.cs 참조)는 발사 속도(실제 사용 속도) 및 탄약 재장전을 처리합니다.
    /// </summary>
    [SelectionBase]
	public class Weapon : MMMonoBehaviour 
	{
		[MMInspectorGroup("ID", true, 7)]
		/// the name of the weapon, only used for debugging
		[Tooltip("디버깅에만 사용되는 무기의 이름")]
		public string WeaponName;
		/// the possible use modes for the trigger (semi auto : the Player needs to release the trigger to fire again, auto : the Player can hold the trigger to fire repeatedly
		public enum TriggerModes { SemiAuto, Auto }
		
		/// the possible states the weapon can be in
		public enum WeaponStates { WeaponIdle, WeaponStart, WeaponDelayBeforeUse, WeaponUse, WeaponDelayBetweenUses, WeaponStop, WeaponReloadNeeded, WeaponReloadStart, WeaponReload, WeaponReloadStop, WeaponInterrupted }

		/// whether or not the weapon is currently active
		[MMReadOnly]
		[Tooltip("무기가 현재 활성화되어 있는지 여부")]
		public bool WeaponCurrentlyActive = true;

		[MMInspectorGroup("Use", true, 10)]
        /// 이것이 사실이라면 이 무기는 입력을 읽을 수 있으며(보통 CharacterHandleWeapon 기능을 통해), 그렇지 않으면 플레이어 입력이 비활성화됩니다.
        [Tooltip("이것이 사실이라면 이 무기는 입력을 읽을 수 있으며(보통 CharacterHandleWeapon 기능을 통해), 그렇지 않으면 플레이어 입력이 비활성화됩니다.")]
		public bool InputAuthorized = true;
		/// is this weapon on semi or full auto ?
		[Tooltip("이 무기는 반자동인가요 아니면 완전 자동인가요??")]
		public TriggerModes TriggerMode = TriggerModes.Auto;
		/// the delay before use, that will be applied for every shot
		[Tooltip("사용 전 지연 시간은 모든 샷에 적용됩니다.")]
		public float DelayBeforeUse = 0f;
		/// whether or not the delay before used can be interrupted by releasing the shoot button (if true, releasing the button will cancel the delayed shot)
		[Tooltip("촬영 버튼을 놓아 사용 전 지연을 중단할 수 있는지 여부(참인 경우 버튼을 놓으면 지연된 촬영이 취소됩니다)")]
		public bool DelayBeforeUseReleaseInterruption = true;
		/// the time (in seconds) between two shots		
		[Tooltip("두 샷 사이의 시간(초)")]
		public float TimeBetweenUses = 1f;
		/// whether or not the time between uses can be interrupted by releasing the shoot button (if true, releasing the button will cancel the time between uses)
		[Tooltip("촬영 버튼을 놓아 사용 사이의 시간을 중단할 수 있는지 여부(참인 경우 버튼을 놓으면 사용 사이의 시간이 취소됩니다)")]
		public bool TimeBetweenUsesReleaseInterruption = true;

		[Header("Burst Mode")] 
		/// if this is true, the weapon will activate repeatedly for every shoot request
		[Tooltip("이것이 사실이라면, 사격 요청이 있을 때마다 무기가 반복적으로 활성화됩니다.")]
		public bool UseBurstMode = false;
		/// the amount of 'shots' in a burst sequence
		[Tooltip("버스트 시퀀스의 '샷' 양")]
		public int BurstLength = 3;
		/// the time between shots in a burst sequence (in seconds)
		[Tooltip("버스트 시퀀스의 샷 간 시간(초)")]
		public float BurstTimeBetweenShots = 0.1f;

		[MMInspectorGroup("Magazine", true, 11)]
		/// whether or not the weapon is magazine based. If it's not, it'll just take its ammo inside a global pool
		[Tooltip("무기가 탄창 기반인지 여부. 그렇지 않은 경우에는 글로벌 풀 내에서 탄약을 가져갈 것입니다.")]
		public bool MagazineBased = false;
		/// the size of the magazine
		[Tooltip("탄창의 크기")]
		public int MagazineSize = 30;
		/// if this is true, pressing the fire button when a reload is needed will reload the weapon. Otherwise you'll need to press the reload button
		[Tooltip("이것이 사실이라면 재장전이 필요할 때 발사 버튼을 누르면 무기가 재장전됩니다. 그렇지 않으면 새로고침 버튼을 눌러야 합니다.")]
		public bool AutoReload;
		/// if this is true, reload will automatically happen right after the last bullet is shot, without the need for input
		[Tooltip("이것이 사실이라면 입력할 필요 없이 마지막 총알이 발사된 직후 자동으로 재장전이 수행됩니다.")]
		public bool NoInputReload = false;
		/// the time it takes to reload the weapon
		[Tooltip("무기를 재장전하는데 걸리는 시간")]
		public float ReloadTime = 2f;
		/// the amount of ammo consumed everytime the weapon fires
		[Tooltip("무기가 발사될 때마다 소모되는 탄약의 양")]
		public int AmmoConsumedPerShot = 1;
		/// if this is set to true, the weapon will auto destroy when there's no ammo left
		[Tooltip("이것이 true로 설정되면 남은 탄약이 없을 때 무기가 자동으로 파괴됩니다.")]
		public bool AutoDestroyWhenEmpty;
		/// the delay (in seconds) before weapon destruction if empty
		[Tooltip("비어 있는 경우 무기 파괴 전 지연 시간(초)")]
		public float AutoDestroyWhenEmptyDelay = 1f;
		/// if this is true, the weapon won't try and reload if the ammo is empty, when using WeaponAmmo
		[Tooltip("이것이 사실이라면 WeaponAmmo를 사용할 때 탄약이 비어 있으면 무기가 재장전을 시도하지 않습니다.")]
		public bool PreventReloadIfAmmoEmpty = false;
		/// the current amount of ammo loaded inside the weapon
		[MMReadOnly]
		[Tooltip("현재 무기 내부에 장전된 탄약의 양")]
		public int CurrentAmmoLoaded = 0;

		[MMInspectorGroup("Position", true, 12)]
		/// an offset that will be applied to the weapon once attached to the center of the WeaponAttachment transform.
		[Tooltip("WeaponAttachment 변환의 중심에 연결되면 무기에 적용될 오프셋입니다.")]
		public Vector3 WeaponAttachmentOffset = Vector3.zero;
		/// should that weapon be flipped when the character flips?
		[Tooltip("캐릭터가 뒤집힐 때 해당 무기도 뒤집어야 합니까?")]
		public bool FlipWeaponOnCharacterFlip = true;
		/// the FlipValue will be used to multiply the model's transform's localscale on flip. Usually it's -1,1,1, but feel free to change it to suit your model's specs
		[Tooltip("FlipValue는 뒤집을 때 모델의 변환 로컬 스케일을 곱하는 데 사용됩니다. 일반적으로 -1,1,1이지만 모델 사양에 맞게 자유롭게 변경하세요.")]
		public Vector3 RightFacingFlipValue = new Vector3(1, 1, 1);
		/// the FlipValue will be used to multiply the model's transform's localscale on flip. Usually it's -1,1,1, but feel free to change it to suit your model's specs
		[Tooltip("FlipValue는 뒤집을 때 모델의 변환 로컬 스케일을 곱하는 데 사용됩니다. 일반적으로 -1,1,1이지만 모델 사양에 맞게 자유롭게 변경하세요.")]
		public Vector3 LeftFacingFlipValue = new Vector3(-1, 1, 1);
		/// a transform to use as the spawn point for weapon use (if null, only offset will be considered, otherwise the transform without offset)
		[Tooltip("무기 사용을 위한 스폰 지점으로 사용할 변환(null인 경우 오프셋만 고려되고 그렇지 않으면 오프셋 없는 변환)")]
		public Transform WeaponUseTransform;
		/// if this is true, the weapon will flip to match the character's orientation
		[Tooltip("이것이 사실이라면 무기는 캐릭터의 방향에 맞춰 뒤집힐 것입니다.")]
		public bool WeaponShouldFlip = true;

		[MMInspectorGroup("IK", true, 13)]
		/// the transform to which the character's left hand should be attached to
		[Tooltip("캐릭터의 왼손이 연결되어야 하는 변환")]
		public Transform LeftHandHandle;
		/// the transform to which the character's right hand should be attached to
		[Tooltip("캐릭터의 오른손이 연결되어야 하는 변환")]
		public Transform RightHandHandle;

		[MMInspectorGroup("Movement", true, 14)]
		/// if this is true, a multiplier will be applied to movement while the weapon is active
		[Tooltip("이것이 사실이라면 무기가 활성화된 동안 이동에 승수가 적용됩니다.")]
		public bool ModifyMovementWhileAttacking = false;
		/// the multiplier to apply to movement while attacking
		[Tooltip("공격하는 동안 이동에 적용할 승수")]
		public float MovementMultiplier = 0f;
		/// if this is true all movement will be prevented (even flip) while the weapon is active
		[Tooltip("이것이 사실이라면 무기가 활성화되어 있는 동안 모든 움직임이 금지됩니다(뒤집기 포함).")]
		public bool PreventAllMovementWhileInUse = false;
		/// if this is true all aim will be prevented while the weapon is active
		[Tooltip("이것이 사실이라면 무기가 활성화된 동안 모든 조준이 방지됩니다.")]
		public bool PreventAllAimWhileInUse = false;

		[MMInspectorGroup("Recoil", true, 15)]
		/// the force to apply to push the character back when shooting - positive values will push the character back, negative values will launch it forward, turning that recoil into a thrust
		[Tooltip("사격 시 캐릭터를 뒤로 밀기 위해 적용할 힘 - 양수 값은 캐릭터를 뒤로 밀고, 음수 값은 캐릭터를 앞으로 발사하여 반동을 추력으로 바꿉니다.")]
		public float RecoilForce = 0f;

		[MMInspectorGroup("Animation", true, 16)]
		/// the other animators (other than the Character's) that you want to update every time this weapon gets used
		[Tooltip("이 무기를 사용할 때마다 업데이트하려는 다른 애니메이터(캐릭터 제외)")]
		public List<Animator> Animators;
		/// If this is true, sanity checks will be performed to make sure animator parameters exist before updating them. Turning this to false will increase performance but will throw errors if you're trying to update non existing parameters. Make sure your animator has the required parameters.
		[Tooltip("이것이 사실이라면 애니메이터 매개변수를 업데이트하기 전에 존재하는지 확인하기 위해 온전성 검사가 수행됩니다. 이를 false로 설정하면 성능이 향상되지만 존재하지 않는 매개변수를 업데이트하려고 하면 오류가 발생합니다. 애니메이터에 필수 매개변수가 있는지 확인하세요.")]
		public bool PerformAnimatorSanityChecks = false;
		/// if this is true, the weapon's animator(s) will mirror the animation parameter of the owner character (that way your weapon's animator will be able to "know" if the character is walking, jumping, etc)
		[Tooltip("이것이 사실이라면 무기의 애니메이터는 소유자 캐릭터의 애니메이션 매개변수를 미러링합니다(그러면 무기의 애니메이터는 캐릭터가 걷고 있는지, 점프하는지 등을 '알' 수 있습니다).")]
		public bool MirrorCharacterAnimatorParameters = false;

		[MMInspectorGroup("Animation Parameters Names", true, 17)]
		/// the ID of the weapon to pass to the animator
		[Tooltip("애니메이터에게 전달할 무기의 ID")]
		public int WeaponAnimationID = 0;
        /// 무기의 유휴 애니메이션 매개변수 이름: 무기가 사용되는 경우를 제외하고는 항상 true입니다.
        [Tooltip("무기의 유휴 애니메이션 매개변수 이름: 무기가 사용되는 경우를 제외하고는 항상 true입니다.")]
		public string IdleAnimationParameter;
        /// 무기의 시작 애니메이션 매개변수 이름: 무기가 사용되기 시작하는 프레임에서 true
        [Tooltip("무기의 시작 애니메이션 매개변수 이름: 무기가 사용되기 시작하는 프레임에서 true")]
		public string StartAnimationParameter;
        /// 사용 전 무기의 지연 이름 애니메이션 매개변수: 무기가 활성화되었지만 아직 사용되지 않은 경우 true
        [Tooltip("사용 전 무기의 지연 이름 애니메이션 매개변수: 무기가 활성화되었지만 아직 사용되지 않은 경우 true")]
		public string DelayBeforeUseAnimationParameter;
        /// 무기의 일회용 애니메이션 매개변수 이름: 무기가 활성화(발사)되는 각 프레임에서 true
        [Tooltip("무기의 일회용 애니메이션 매개변수 이름: 무기가 활성화(발사)되는 각 프레임에서 true")]
		public string SingleUseAnimationParameter;
        /// 사용 중인 무기의 이름 애니메이션 매개변수: 무기가 발사를 시작했지만 아직 멈추지 않은 각 프레임에서 true
        [Tooltip("사용 중인 무기의 이름 애니메이션 매개변수: 무기가 발사를 시작했지만 아직 멈추지 않은 각 프레임에서 true")]
		public string UseAnimationParameter;
        /// 각 사용 애니메이션 매개변수 사이의 무기 지연 이름: 무기가 사용 중인 경우 true
        [Tooltip("각 사용 애니메이션 매개변수 사이의 무기 지연 이름: 무기가 사용 중인 경우 true")]
		public string DelayBetweenUsesAnimationParameter;
        /// 무기 정지 애니메이션 매개변수의 이름: 사격 후와 다음 사격 또는 무기 정지 전 true
        [Tooltip("무기 정지 애니메이션 매개변수의 이름: 사격 후와 다음 사격 또는 무기 정지 전 true")]
		public string StopAnimationParameter;
        /// 무기 재장전 시작 애니메이션 매개변수의 이름
        [Tooltip("무기 재장전 시작 애니메이션 매개변수의 이름")]
		public string ReloadStartAnimationParameter;
        /// 무기 재장전 애니메이션 매개변수의 이름
        [Tooltip("무기 재장전 애니메이션 매개변수의 이름")]
		public string ReloadAnimationParameter;
        /// 무기 재장전 종료 애니메이션 매개변수의 이름
        [Tooltip("무기 재장전 종료 애니메이션 매개변수의 이름")]
		public string ReloadStopAnimationParameter;
        /// 무기 재장전 종료 애니메이션 매개변수의 이름
        [Tooltip("무기 각도 애니메이션 매개변수의 이름")]
		public string WeaponAngleAnimationParameter;
        /// 무기의 각도 애니메이션 매개변수 이름. 항상 캐릭터가 현재 향하고 있는 방향을 기준으로 조정됩니다.
        [Tooltip("무기의 각도 애니메이션 매개변수 이름. 항상 캐릭터가 현재 향하고 있는 방향을 기준으로 조정됩니다.")]
		public string WeaponAngleRelativeAnimationParameter;
        /// 이 무기가 장착되어 있거나 사용되지 않는 한 true로 보낼 매개변수의 이름입니다. 여기에 정의된 다른 모든 매개변수는 Weapon 클래스 자체에 의해 업데이트되고 무기와 캐릭터에 전달되지만 이 매개변수는 CharacterHandleWeapon에 의해서만 업데이트됩니다.
        [Tooltip("이 무기가 장착되어 있거나 사용되지 않는 한 true로 보낼 매개변수의 이름입니다. 여기에 정의된 다른 모든 매개변수는 Weapon 클래스 자체에 의해 업데이트되고 무기와 캐릭터에 전달되지만 이 매개변수는 CharacterHandleWeapon에 의해서만 업데이트됩니다.")]
		public string EquippedAnimationParameter;
        
		[MMInspectorGroup("Feedbacks", true, 18)]
		/// the feedback to play when the weapon starts being used
		[Tooltip("무기가 사용되기 시작할 때 재생할 피드백")]
		public MMFeedbacks WeaponStartMMFeedback;
		/// the feedback to play while the weapon is in use
		[Tooltip("무기를 사용하는 동안 플레이할 피드백")]
		public MMFeedbacks WeaponUsedMMFeedback;
		/// if set, this feedback will be used randomly instead of WeaponUsedMMFeedback
		[Tooltip("설정된 경우 이 피드백은 WeaponUsedMMFeedback 대신 무작위로 사용됩니다.")]
		public MMFeedbacks WeaponUsedMMFeedbackAlt;
		/// the feedback to play when the weapon stops being used
		[Tooltip("무기 사용이 중단될 때 재생할 피드백")]
		public MMFeedbacks WeaponStopMMFeedback;
		/// the feedback to play when the weapon gets reloaded
		[Tooltip("무기가 재장전될 때 재생할 피드백")]
		public MMFeedbacks WeaponReloadMMFeedback;
		/// the feedback to play when the weapon gets reloaded
		[Tooltip("무기가 재장전될 때 재생할 피드백")]
		public MMFeedbacks WeaponReloadNeededMMFeedback;
		/// the feedback to play when the weapon can't reload as there's no more ammo available. You'll need PreventReloadIfAmmoEmpty to be true for this to work
		[Tooltip("더 이상 사용할 수 있는 탄약이 없어 무기를 재장전할 수 없을 때 재생할 피드백입니다. 이것이 작동하려면 PreventReloadIfAmmoEmpty가 true여야 합니다.")]
		public MMFeedbacks WeaponReloadImpossibleMMFeedback;
        
		[MMInspectorGroup("Settings", true, 19)]
		/// If this is true, the weapon will initialize itself on start, otherwise it'll have to be init manually, usually by the CharacterHandleWeapon class
		[Tooltip("이것이 사실이라면 무기는 시작 시 자체적으로 초기화되고, 그렇지 않으면 일반적으로 CharacterHandleWeapon 클래스에 의해 수동으로 초기화되어야 합니다.")]
		public bool InitializeOnStart = false;
		/// whether or not this weapon can be interrupted 
		[Tooltip("이 무기를 방해할 수 있는지 여부")]
		public bool Interruptable = false;

		/// the name of the inventory item corresponding to this weapon. Automatically set (if needed) by InventoryEngineWeapon
		public string WeaponID { get; set; }
		/// the weapon's owner
		public Character Owner { get; protected set; }
		/// the weapon's owner's CharacterHandleWeapon component
		public CharacterHandleWeapon CharacterHandleWeapon { get; set; }
		/// if true, the weapon is flipped
		[MMReadOnly]
		[Tooltip("사실이라면 지금 무기가 뒤집어진 것입니다.")]
		public bool Flipped;
		/// the WeaponAmmo component optionnally associated to this weapon
		public WeaponAmmo WeaponAmmo { get; protected set; }
        /// 무기의 상태 머신
        public MMStateMachine<WeaponStates> WeaponState;

		protected SpriteRenderer _spriteRenderer;
		protected WeaponAim _weaponAim;
		protected float _movementMultiplierStorage = 1f;

		public float MovementMultiplierStorage
		{
			get => _movementMultiplierStorage;
			set => _movementMultiplierStorage = value;
		}
		protected Animator _ownerAnimator;
		protected WeaponPreventShooting _weaponPreventShooting;
		protected float _delayBeforeUseCounter = 0f;
		protected float _delayBetweenUsesCounter = 0f;
		protected float _reloadingCounter = 0f;
		protected bool _triggerReleased = false;
		protected bool _reloading = false;
		protected ComboWeapon _comboWeapon;
		protected TopDownController _controller;
		protected CharacterMovement _characterMovement;
		protected Vector3 _weaponOffset;
		protected Vector3 _weaponAttachmentOffset;
		protected Transform _weaponAttachment;
		protected List<HashSet<int>> _animatorParameters;
		protected HashSet<int> _ownerAnimatorParameters;
		protected bool _controllerIs3D = false;
        
		protected const string _aliveAnimationParameterName = "Alive";
		protected int _idleAnimationParameter;
		protected int _startAnimationParameter;
		protected int _delayBeforeUseAnimationParameter;
		protected int _singleUseAnimationParameter;
		protected int _useAnimationParameter;
		protected int _delayBetweenUsesAnimationParameter;
		protected int _stopAnimationParameter;
		protected int _reloadStartAnimationParameter;
		protected int _reloadAnimationParameter;
		protected int _reloadStopAnimationParameter;
		protected int _weaponAngleAnimationParameter;
		protected int _weaponAngleRelativeAnimationParameter;
		protected int _aliveAnimationParameter;
		protected int _comboInProgressAnimationParameter;
		protected int _equippedAnimationParameter;
		protected float _lastShootRequestAt = -float.MaxValue;
		protected float _lastTurnWeaponOnAt = -float.MaxValue;

		//내가만든함수
        protected float attackComboTime = 0.3f;
		protected float exitTime = 0.84f;
        //protected Animator animator;

        private void Awake()
        {
            if (InitializeOnStart)
            {
                Initialization();
            }
        }

        /// <summary>
        /// On start we initialize our weapon
        /// </summary>
        protected virtual void Start()
		{

		}

		/// <summary>
		/// Initialize this weapon.
		/// </summary>
		public virtual void Initialization()
		{
			Flipped = false;
			_spriteRenderer = this.gameObject.GetComponent<SpriteRenderer>();
			_comboWeapon = this.gameObject.GetComponent<ComboWeapon>();
			_weaponPreventShooting = this.gameObject.GetComponent<WeaponPreventShooting>();

			WeaponState = new MMStateMachine<WeaponStates>(gameObject, true);
			WeaponState.ChangeState(WeaponStates.WeaponIdle);
			WeaponAmmo = GetComponent<WeaponAmmo>();
			_animatorParameters = new List<HashSet<int>>();
			_weaponAim = GetComponent<WeaponAim>();
			InitializeAnimatorParameters();
			if (WeaponAmmo == null)
			{
				CurrentAmmoLoaded = MagazineSize;
			}
			InitializeFeedbacks();       
		}

		protected virtual void InitializeFeedbacks()
		{
			WeaponStartMMFeedback?.Initialization(this.gameObject);
			WeaponUsedMMFeedback?.Initialization(this.gameObject);
			WeaponUsedMMFeedbackAlt?.Initialization(this.gameObject);
			WeaponStopMMFeedback?.Initialization(this.gameObject);
			WeaponReloadNeededMMFeedback?.Initialization(this.gameObject);
			WeaponReloadMMFeedback?.Initialization(this.gameObject);
		}

		/// <summary>
		/// Initializes the combo weapon, if it's one
		/// </summary>
		public virtual void InitializeComboWeapons()
		{
			if (_comboWeapon != null)
			{
				_comboWeapon.Initialization();
			}
		}

		/// <summary>
		/// Sets the weapon's owner
		/// </summary>
		/// <param name="newOwner">New owner.</param>
		public virtual void SetOwner(Character newOwner, CharacterHandleWeapon handleWeapon)
		{
			Owner = newOwner;
			if (Owner != null)
			{
				CharacterHandleWeapon = handleWeapon;
				_characterMovement = Owner.GetComponent<Character>()?.FindAbility<CharacterMovement>();
				_controller = Owner.GetComponent<TopDownController>();

				_controllerIs3D = Owner.GetComponent<TopDownController3D>() != null;

				if (CharacterHandleWeapon.AutomaticallyBindAnimator)
				{
					if (CharacterHandleWeapon.CharacterAnimator != null)
					{
						_ownerAnimator = CharacterHandleWeapon.CharacterAnimator;
					}
					if (_ownerAnimator == null)
					{
						_ownerAnimator = CharacterHandleWeapon.gameObject.GetComponentInParent<Character>().CharacterAnimator;
					}
					if (_ownerAnimator == null)
					{
						_ownerAnimator = CharacterHandleWeapon.gameObject.GetComponentInParent<Animator>();
                    }
				}
			}
		}

        /// <summary>
        /// 입력에 의해 호출되어 무기를 켭니다.
        /// </summary>
        public virtual void WeaponInputStart()
		{
			if (_reloading)
			{
				return;
			}

			if (WeaponState.CurrentState == WeaponStates.WeaponIdle)
			{
				_triggerReleased = false;
				TurnWeaponOn();
			}
		}

		/// <summary>
		/// Describes what happens when the weapon's input gets released
		/// </summary>
		public virtual void WeaponInputReleased()
		{
			
		}

        /// <summary>
        /// 무기가 시작되면 어떤 일이 일어나는지 설명합니다.
        /// </summary>
        public virtual void TurnWeaponOn()
		{
			if (!InputAuthorized && (Time.time - _lastTurnWeaponOnAt < TimeBetweenUses))
			{
				return;
			}

			_lastTurnWeaponOnAt = Time.time;
			
			TriggerWeaponStartFeedback();
			WeaponState.ChangeState(WeaponStates.WeaponStart);
			if ((_characterMovement != null) && (ModifyMovementWhileAttacking))
			{
				_movementMultiplierStorage = _characterMovement.MovementSpeedMultiplier;
				_characterMovement.MovementSpeedMultiplier = MovementMultiplier;
			}
			if (_comboWeapon != null)
			{
				_comboWeapon.WeaponStarted(this);
			}
			if (PreventAllMovementWhileInUse && (_characterMovement != null) && (_controller != null))
			{
				_characterMovement.SetMovement(Vector2.zero);
				_characterMovement.MovementForbidden = true;
			}
			if (PreventAllAimWhileInUse && (_weaponAim != null))
			{
				_weaponAim.AimControlActive = false;
			}
		}

		/// <summary>
		/// On Update, we check if the weapon is or should be used
		/// </summary>
		protected virtual void Update()
		{
            FlipWeapon();
			ApplyOffset();
		}

        /// <summary>
        /// LateUpdate에서 무기 상태를 처리합니다.
        /// </summary>
        protected virtual void LateUpdate()
		{
            //if(gameObject.tag != "Enemy")
            ProcessWeaponState();
        }

        /// <summary>
        /// lastUpdate마다 호출되어 무기의 상태 시스템을 처리합니다.
        /// </summary>
        protected virtual void ProcessWeaponState()
		{
			if (WeaponState == null) { return; }
			
			UpdateAnimator();

			switch (WeaponState.CurrentState)
			{
				case WeaponStates.WeaponIdle:
					CaseWeaponIdle();
					break;

				case WeaponStates.WeaponStart:
					CaseWeaponStart();
					break;

				case WeaponStates.WeaponDelayBeforeUse:
					CaseWeaponDelayBeforeUse();
					break;

				case WeaponStates.WeaponUse:
					CaseWeaponUse();
					break;

				case WeaponStates.WeaponDelayBetweenUses:
					CaseWeaponDelayBetweenUses();
					break;

				case WeaponStates.WeaponStop:
					CaseWeaponStop();
					break;

				case WeaponStates.WeaponReloadNeeded:
					CaseWeaponReloadNeeded();
					break;

				case WeaponStates.WeaponReloadStart:
					CaseWeaponReloadStart();
					break;

				case WeaponStates.WeaponReload:
					CaseWeaponReload();
					break;

				case WeaponStates.WeaponReloadStop:
					CaseWeaponReloadStop();
					break;

				case WeaponStates.WeaponInterrupted:
					CaseWeaponInterrupted();
					break;
			}
		}

		/// <summary>
		/// If the weapon is idle, we reset the movement multiplier
		/// </summary>
		public virtual void CaseWeaponIdle()
		{
			ResetMovementMultiplier();
		}

        /// <summary>
        /// 무기가 시작되면 무기 설정에 따라 지연 또는 발사로 전환됩니다.
        /// </summary>
        public virtual void CaseWeaponStart()
		{
            //StartCoroutine(CheckAnimationState());

            if (DelayBeforeUse > 0)
			{
				_delayBeforeUseCounter = DelayBeforeUse;
				WeaponState.ChangeState(WeaponStates.WeaponDelayBeforeUse);
			}
			else
			{
				StartCoroutine(ShootRequestCo());
			}
		}

        //attackDelayTime 뒤에 공격 가능하도록
		public virtual void AttackAllow()
		{
            InputAuthorized = true;
			//Debug.Log("공격버튼 활성화");
        }

        //콤보 파라미터 변경
  //      public virtual void ComboChange()
		//{
  //          switch (animator.GetInteger("Combo"))
  //          {
  //              case 0: animator.SetInteger("Combo", 1); attackComboTime = 0.3f; break;
  //              case 1: animator.SetInteger("Combo", 2); break;
  //              case 2: animator.SetInteger("Combo", 0); break;
  //          }

  //          AttackAllow();
  //      }

		//public virtual IEnumerator CheckAnimationState()
		//{
  //          while (animator.GetInteger("Combo") == 0 && !animator.GetCurrentAnimatorStateInfo(0).IsName("Attack01"))
		//	{
  //              InputAuthorized = false;
  //              yield return new WaitForSecondsRealtime(0.78f);
  //              animator.SetInteger("Combo", 1);
  //              InputAuthorized = true;
  //              yield return new WaitForSecondsRealtime(0.3f);
  //              animator.SetInteger("Combo", 0);
		//		yield break;
  //          }

		//	while (animator.GetInteger("Combo") == 1 && !animator.GetCurrentAnimatorStateInfo(0).IsName("Attack02_1"))
		//	{
  //              InputAuthorized = false;
  //              yield return new WaitForSecondsRealtime(0.78f);
  //              animator.SetInteger("Combo", 2);
  //              InputAuthorized = true;
  //              yield return new WaitForSecondsRealtime(0.3f);
  //              animator.SetInteger("Combo", 0);
  //              yield break;
  //          }

		//	while (animator.GetInteger("Combo") == 2 && !animator.GetCurrentAnimatorStateInfo(0).IsName("Attack03_1"))
		//	{
		//		switch (PlayerDataManager.GetThirdAttack().ToString())
		//		{
		//			case "Holy": CreateManager.Instance.ThirdAttack_HolySpawn(); break;
		//			case "Ice": CreateManager.Instance.ThirdAttack_IceSpawn(); break;
  //              }				
  //              InputAuthorized = false;
  //              yield return new WaitForSecondsRealtime(0.78f);
  //              animator.SetInteger("Combo", 0);
  //              InputAuthorized = true;
  //              yield break;
  //          }

		//	//while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < exitTime)
		//	//{
		//	//             //애니메이션 재생 중 실행되는 부분
		//	//             yield return null;
		//	//}

		//	//애니메이션 완료 후 실행되는 부분
		//}


            /// <summary>
            /// If we're in delay before use, we wait until our delay is passed and then request a shoot
            /// </summary>
        public virtual void CaseWeaponDelayBeforeUse()
		{
			_delayBeforeUseCounter -= Time.deltaTime;
			if (_delayBeforeUseCounter <= 0)
			{
				StartCoroutine(ShootRequestCo());
			}
		}

        /// <summary>
        /// 무기를 사용할 때 무기를 사용한 다음 사용 사이에 지연으로 전환합니다.
        /// </summary>
        public virtual void CaseWeaponUse()
		{
			WeaponUse();
			_delayBetweenUsesCounter = TimeBetweenUses;
			WeaponState.ChangeState(WeaponStates.WeaponDelayBetweenUses);
		}

		/// <summary>
		/// When in delay between uses, we either turn our weapon off or make a shoot request
		/// </summary>
		public virtual void CaseWeaponDelayBetweenUses()
		{
			if (_triggerReleased && TimeBetweenUsesReleaseInterruption)
			{
				TurnWeaponOff();
				return;
			}
            
			_delayBetweenUsesCounter -= Time.deltaTime;
			if (_delayBetweenUsesCounter <= 0)
			{
				if ((TriggerMode == TriggerModes.Auto) && !_triggerReleased)
				{
					StartCoroutine(ShootRequestCo());
				}
				else
				{
					TurnWeaponOff();
				}
			}
		}

        /// <summary>
        /// 무기 정지시 유휴 상태로 전환
        /// </summary>
        public virtual void CaseWeaponStop()
		{
			WeaponState.ChangeState(WeaponStates.WeaponIdle);
        }

		/// <summary>
		/// If a reload is needed, we mention it and switch to idle
		/// </summary>
		public virtual void CaseWeaponReloadNeeded()
		{
			ReloadNeeded();
			ResetMovementMultiplier();
			WeaponState.ChangeState(WeaponStates.WeaponIdle);
		}

		/// <summary>
		/// on reload start, we reload the weapon and switch to reload
		/// </summary>
		public virtual void CaseWeaponReloadStart()
		{
			ReloadWeapon();
			_reloadingCounter = ReloadTime;
			WeaponState.ChangeState(WeaponStates.WeaponReload);
		}

		/// <summary>
		/// on reload, we reset our movement multiplier, and switch to reload stop once our reload delay has passed
		/// </summary>
		public virtual void CaseWeaponReload()
		{
			ResetMovementMultiplier();
			_reloadingCounter -= Time.deltaTime;
			if (_reloadingCounter <= 0)
			{
				WeaponState.ChangeState(WeaponStates.WeaponReloadStop);
			}
		}

		/// <summary>
		/// on reload stop, we swtich to idle and load our ammo
		/// </summary>
		public virtual void CaseWeaponReloadStop()
		{
			_reloading = false;
			WeaponState.ChangeState(WeaponStates.WeaponIdle);
			if (WeaponAmmo == null)
			{
				CurrentAmmoLoaded = MagazineSize;
			}
		}

		/// <summary>
		/// on weapon interrupted, we turn our weapon off and switch back to idle
		/// </summary>
		public virtual void CaseWeaponInterrupted()
		{
			TurnWeaponOff();
			ResetMovementMultiplier();
			WeaponState.ChangeState(WeaponStates.WeaponIdle);
		}

		/// <summary>
		/// Call this method to interrupt the weapon
		/// </summary>
		public virtual void Interrupt()
		{
			if (Interruptable)
			{
				WeaponState.ChangeState(WeaponStates.WeaponInterrupted);
			}
		}




        /// <summary>
        /// 무기가 발사될 수 있는지 여부를 결정합니다.
        /// </summary>
        public virtual IEnumerator ShootRequestCo()
		{
			if (Time.time - _lastShootRequestAt < TimeBetweenUses)
			{
				yield break;
			}
			
			int remainingShots = UseBurstMode ? BurstLength : 1;
			float interval = UseBurstMode ? BurstTimeBetweenShots : 1;

			while (remainingShots > 0)
			{
				ShootRequest();
				_lastShootRequestAt = Time.time;
				remainingShots--;
				yield return MMCoroutine.WaitFor(interval);
			}
		}

		public virtual void ShootRequest()
		{
			// if we have a weapon ammo component, we determine if we have enough ammunition to shoot
			if (_reloading)
			{
				return;
			}

			if (_weaponPreventShooting != null)
			{
				if (!_weaponPreventShooting.ShootingAllowed())
				{
					return;
				}
			}

			if (MagazineBased)
			{
				if (WeaponAmmo != null)
				{
					if (WeaponAmmo.EnoughAmmoToFire())
					{
						WeaponState.ChangeState(WeaponStates.WeaponUse);
					}
					else
					{
						if (AutoReload && MagazineBased)
						{
							InitiateReloadWeapon();
						}
						else
						{
							WeaponState.ChangeState(WeaponStates.WeaponReloadNeeded);
						}
					}
				}
				else
				{
					if (CurrentAmmoLoaded > 0)
					{
						WeaponState.ChangeState(WeaponStates.WeaponUse);
						CurrentAmmoLoaded -= AmmoConsumedPerShot;
					}
					else
					{
						if (AutoReload)
						{
							InitiateReloadWeapon();
						}
						else
						{
							WeaponState.ChangeState(WeaponStates.WeaponReloadNeeded);
						}
					}
				}
			}
			else
			{
				if (WeaponAmmo != null)
				{
					if (WeaponAmmo.EnoughAmmoToFire())
					{
						WeaponState.ChangeState(WeaponStates.WeaponUse);
					}
					else
					{
						WeaponState.ChangeState(WeaponStates.WeaponReloadNeeded);
					}
				}
				else
				{
					WeaponState.ChangeState(WeaponStates.WeaponUse);
				}
			}
		}

        /// <summary>
        /// 무기 사용 시 해당 소리가 재생됩니다.
        /// </summary>
        public virtual void WeaponUse()
		{
			ApplyRecoil();
			TriggerWeaponUsedFeedback();
		}

        /// <summary>
        /// 필요한 경우 반동을 적용합니다.
        /// </summary>
        protected virtual void ApplyRecoil()
		{
			if ((RecoilForce != 0f) && (_controller != null))
			{
				if (Owner != null)
				{
					if (!_controllerIs3D)
					{
						if (Flipped)
						{
							_controller.Impact(this.transform.right, RecoilForce);
						}
						else
						{
							_controller.Impact(-this.transform.right, RecoilForce);
						}
					}
					else
					{
						_controller.Impact(-this.transform.forward, RecoilForce);
					}
				}                
			}
		}

		/// <summary>
		/// Called by input, turns the weapon off if in auto mode
		/// </summary>
		public virtual void WeaponInputStop()
		{
			if (_reloading)
			{
				return;
			}
			_triggerReleased = true;
			if ((_characterMovement != null) && (ModifyMovementWhileAttacking))
			{
				_characterMovement.MovementSpeedMultiplier = _movementMultiplierStorage;
				_movementMultiplierStorage = 1f;
			}
		}

        /// <summary>
        /// 무기를 끕니다.
        /// </summary>
        public virtual void TurnWeaponOff()
		{
			if ((WeaponState.CurrentState == WeaponStates.WeaponIdle || WeaponState.CurrentState == WeaponStates.WeaponStop))
			{
				return;
			}
			_triggerReleased = true;

			TriggerWeaponStopFeedback();
			WeaponState.ChangeState(WeaponStates.WeaponStop);
			ResetMovementMultiplier();
			if (_comboWeapon != null)
			{
				_comboWeapon.WeaponStopped(this);
			}
			if (PreventAllMovementWhileInUse && (_characterMovement != null))
			{
				_characterMovement.MovementForbidden = false;
			}
			if (PreventAllAimWhileInUse && (_weaponAim != null))
			{
				_weaponAim.AimControlActive = true;
			}

			if (NoInputReload)
			{
				bool needToReload = false;
				if (WeaponAmmo != null)
				{
					needToReload = !WeaponAmmo.EnoughAmmoToFire();
				}
				else
				{
					needToReload = (CurrentAmmoLoaded <= 0);
				}
                
				if (needToReload)
				{
					InitiateReloadWeapon();
				}
			}
		}

		protected virtual void ResetMovementMultiplier()
		{
			if ((_characterMovement != null) && (ModifyMovementWhileAttacking))
			{
				_characterMovement.MovementSpeedMultiplier = _movementMultiplierStorage;
				_movementMultiplierStorage = 1f;
			}
		}

		/// <summary>
		/// Describes what happens when the weapon needs a reload
		/// </summary>
		public virtual void ReloadNeeded()
		{
			TriggerWeaponReloadNeededFeedback();
		}

		/// <summary>
		/// Initiates a reload
		/// </summary>
		public virtual void InitiateReloadWeapon()
		{
			if (PreventReloadIfAmmoEmpty && WeaponAmmo && WeaponAmmo.CurrentAmmoAvailable == 0)
			{
				WeaponReloadImpossibleMMFeedback?.PlayFeedbacks();
				return;
			}
			
			// if we're already reloading, we do nothing and exit
			if (_reloading || !MagazineBased)
			{
				return;
			}
			if (PreventAllMovementWhileInUse && (_characterMovement != null))
			{
				_characterMovement.MovementForbidden = false;
			}
			if (PreventAllAimWhileInUse && (_weaponAim != null))
			{
				_weaponAim.AimControlActive = true;
			}
			WeaponState.ChangeState(WeaponStates.WeaponReloadStart);
			_reloading = true;
		}

		/// <summary>
		/// Reloads the weapon
		/// </summary>
		/// <param name="ammo">Ammo.</param>
		protected virtual void ReloadWeapon()
		{
			if (MagazineBased)
			{
				TriggerWeaponReloadFeedback();
			}
		}

		/// <summary>
		/// Flips the weapon.
		/// </summary>
		public virtual void FlipWeapon()
		{
			if (!WeaponShouldFlip)
			{
				return;
			}
			
			if (Owner == null)
			{
				return;
			}

			if (Owner.Orientation2D == null)
			{
				return;
			}

			if (FlipWeaponOnCharacterFlip)
			{
				Flipped = !Owner.Orientation2D.IsFacingRight;
				if (_spriteRenderer != null)
				{
					_spriteRenderer.flipX = Flipped;
				}
				else
				{
					transform.localScale = Flipped ? LeftFacingFlipValue : RightFacingFlipValue;
				}
			}

			if (_comboWeapon != null)
			{
				_comboWeapon.FlipUnusedWeapons();
			}
		}            
        
		/// <summary>
		/// Destroys the weapon
		/// </summary>
		/// <returns>The destruction.</returns>
		public virtual IEnumerator WeaponDestruction()
		{
			yield return new WaitForSeconds(AutoDestroyWhenEmptyDelay);
			// if we don't have ammo anymore, and need to destroy our weapon, we do it
			TurnWeaponOff();
			Destroy(this.gameObject);

			if (WeaponID != null)
			{
				// we remove it from the inventory
				List<int> weaponList = Owner.gameObject.GetComponentInParent<Character>()?.FindAbility<CharacterInventory>().WeaponInventory.InventoryContains(WeaponID);
				if (weaponList.Count > 0)
				{
					Owner.gameObject.GetComponentInParent<Character>()?.FindAbility<CharacterInventory>().WeaponInventory.DestroyItem(weaponList[0]);
				}
			}
		}

        /// <summary>
        /// 인스펙터에 지정된 오프셋을 적용합니다.
        /// </summary>
        public virtual void ApplyOffset()
		{

			if (!WeaponCurrentlyActive)
			{
				return;
			}
            
			_weaponAttachmentOffset = WeaponAttachmentOffset;

			if (Owner == null)
			{
				return;
			}

			if (Owner.Orientation2D != null)
			{
				if (Flipped)
				{
					_weaponAttachmentOffset.x = -WeaponAttachmentOffset.x;
				}
                
				// we apply the offset
				if (transform.parent != null)
				{
					_weaponOffset = transform.parent.position + _weaponAttachmentOffset;
					transform.position = _weaponOffset;
				}
			}
			else
			{
				if (transform.parent != null)
				{
					_weaponOffset = _weaponAttachmentOffset;
					transform.localPosition = _weaponOffset;
				}
			}           
		}

		/// <summary>
		/// Plays the weapon's start sound
		/// </summary>
		protected virtual void TriggerWeaponStartFeedback()
		{
            WeaponStartMMFeedback?.PlayFeedbacks(this.transform.position);
        }

        /// <summary>
        /// 무기의 사용 소리를 재생합니다
        /// </summary>
        protected virtual void TriggerWeaponUsedFeedback()
		{
			if (WeaponUsedMMFeedbackAlt != null)
			{
				int random = MMMaths.RollADice(2);
				if (random > 1)
				{
					WeaponUsedMMFeedbackAlt?.PlayFeedbacks(this.transform.position);
				}
				else
				{
					WeaponUsedMMFeedback?.PlayFeedbacks(this.transform.position);
				}
			}
			else
			{
				WeaponUsedMMFeedback?.PlayFeedbacks(this.transform.position);    
			}
            
		}

		/// <summary>
		/// Plays the weapon's stop sound
		/// </summary>
		protected virtual void TriggerWeaponStopFeedback()
		{            
			WeaponStopMMFeedback?.PlayFeedbacks(this.transform.position);
		}

		/// <summary>
		/// Plays the weapon's reload needed sound
		/// </summary>
		protected virtual void TriggerWeaponReloadNeededFeedback()
		{
			WeaponReloadNeededMMFeedback?.PlayFeedbacks(this.transform.position);
		}

		/// <summary>
		/// Plays the weapon's reload sound
		/// </summary>
		protected virtual void TriggerWeaponReloadFeedback()
		{
			WeaponReloadMMFeedback?.PlayFeedbacks(this.transform.position);
		}

		/// <summary>
		/// Adds required animator parameters to the animator parameters list if they exist
		/// </summary>
		public virtual void InitializeAnimatorParameters()
		{
			if (Animators.Count > 0)
			{
				for (int i = 0; i < Animators.Count; i++)
				{
					_animatorParameters.Add(new HashSet<int>());
					AddParametersToAnimator(Animators[i], _animatorParameters[i]);
					if (!PerformAnimatorSanityChecks)
					{
						Animators[i].logWarnings = false;
					}

					if (MirrorCharacterAnimatorParameters)
					{
						MMAnimatorMirror mirror = Animators[i].gameObject.AddComponent<MMAnimatorMirror>();
						mirror.SourceAnimator = _ownerAnimator;
						mirror.TargetAnimator = Animators[i];
						mirror.Initialization();
					}
				}                
			}            

			if (_ownerAnimator != null)
			{
				_ownerAnimatorParameters = new HashSet<int>();
				AddParametersToAnimator(_ownerAnimator, _ownerAnimatorParameters);
				if (!PerformAnimatorSanityChecks)
				{
					_ownerAnimator.logWarnings = false;
				}
			}
		}

		protected virtual void AddParametersToAnimator(Animator animator, HashSet<int> list)
		{
			MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, EquippedAnimationParameter, out _equippedAnimationParameter, AnimatorControllerParameterType.Bool, list);
			MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, WeaponAngleAnimationParameter, out _weaponAngleAnimationParameter, AnimatorControllerParameterType.Float, list);
			MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, WeaponAngleRelativeAnimationParameter, out _weaponAngleRelativeAnimationParameter, AnimatorControllerParameterType.Float, list);
			MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, IdleAnimationParameter, out _idleAnimationParameter, AnimatorControllerParameterType.Bool, list);
			MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, StartAnimationParameter, out _startAnimationParameter, AnimatorControllerParameterType.Bool, list);
			MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, DelayBeforeUseAnimationParameter, out _delayBeforeUseAnimationParameter, AnimatorControllerParameterType.Bool, list);
			MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, DelayBetweenUsesAnimationParameter, out _delayBetweenUsesAnimationParameter, AnimatorControllerParameterType.Bool, list);
			MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, StopAnimationParameter, out _stopAnimationParameter, AnimatorControllerParameterType.Bool, list);
			MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, ReloadStartAnimationParameter, out _reloadStartAnimationParameter, AnimatorControllerParameterType.Bool, list);
			MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, ReloadStopAnimationParameter, out _reloadStopAnimationParameter, AnimatorControllerParameterType.Bool, list);
			MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, ReloadAnimationParameter, out _reloadAnimationParameter, AnimatorControllerParameterType.Bool, list);
			MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, SingleUseAnimationParameter, out _singleUseAnimationParameter, AnimatorControllerParameterType.Bool, list);
			MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, UseAnimationParameter, out _useAnimationParameter, AnimatorControllerParameterType.Bool, list);

			if (_comboWeapon != null)
			{
				MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, _comboWeapon.ComboInProgressAnimationParameter, out _comboInProgressAnimationParameter, AnimatorControllerParameterType.Bool, list);
			}
		}

        /// <summary>
        /// 캐릭터의 애니메이터에 매개변수를 보내려면 이를 재정의합니다. 이는 캐릭터에 의해 사이클당 한 번씩 호출됩니다.
        /// Early, Normal 및 Late process() 이후의 클래스입니다.
        /// </summary>
        public virtual void UpdateAnimator()
		{
			for (int i = 0; i < Animators.Count; i++)
			{
				UpdateAnimator(Animators[i], _animatorParameters[i]);
			}

			if ((_ownerAnimator != null) && (WeaponState != null) && (_ownerAnimatorParameters != null))
			{
				UpdateAnimator(_ownerAnimator, _ownerAnimatorParameters);
			}
		}

		protected virtual void UpdateAnimator(Animator animator, HashSet<int> list)
		{
			MMAnimatorExtensions.UpdateAnimatorBool(animator, _equippedAnimationParameter, true, list);
			MMAnimatorExtensions.UpdateAnimatorBool(animator, _idleAnimationParameter, (WeaponState.CurrentState == Weapon.WeaponStates.WeaponIdle), list, PerformAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorBool(animator, _startAnimationParameter, (WeaponState.CurrentState == Weapon.WeaponStates.WeaponStart), list, PerformAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorBool(animator, _delayBeforeUseAnimationParameter, (WeaponState.CurrentState == Weapon.WeaponStates.WeaponDelayBeforeUse), list, PerformAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorBool(animator, _useAnimationParameter, (WeaponState.CurrentState == Weapon.WeaponStates.WeaponDelayBeforeUse || WeaponState.CurrentState == Weapon.WeaponStates.WeaponUse || WeaponState.CurrentState == Weapon.WeaponStates.WeaponDelayBetweenUses), list, PerformAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorBool(animator, _singleUseAnimationParameter, (WeaponState.CurrentState == Weapon.WeaponStates.WeaponUse), list, PerformAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorBool(animator, _delayBetweenUsesAnimationParameter, (WeaponState.CurrentState == Weapon.WeaponStates.WeaponDelayBetweenUses), list, PerformAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorBool(animator, _stopAnimationParameter, (WeaponState.CurrentState == Weapon.WeaponStates.WeaponStop), list, PerformAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorBool(animator, _reloadStartAnimationParameter, (WeaponState.CurrentState == Weapon.WeaponStates.WeaponReloadStart), list, PerformAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorBool(animator, _reloadAnimationParameter, (WeaponState.CurrentState == Weapon.WeaponStates.WeaponReload), list, PerformAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorBool(animator, _reloadStopAnimationParameter, (WeaponState.CurrentState == Weapon.WeaponStates.WeaponReloadStop), list, PerformAnimatorSanityChecks);

			if (Owner != null)
			{
				MMAnimatorExtensions.UpdateAnimatorBool(animator, _aliveAnimationParameter, (Owner.ConditionState.CurrentState != CharacterStates.CharacterConditions.Dead), list, PerformAnimatorSanityChecks);
			}

			if (_weaponAim != null)
			{
				MMAnimatorExtensions.UpdateAnimatorFloat(animator, _weaponAngleAnimationParameter, _weaponAim.CurrentAngle, list, PerformAnimatorSanityChecks);
				MMAnimatorExtensions.UpdateAnimatorFloat(animator, _weaponAngleRelativeAnimationParameter, _weaponAim.CurrentAngleRelative, list, PerformAnimatorSanityChecks);
			}
			else
			{
				MMAnimatorExtensions.UpdateAnimatorFloat(animator, _weaponAngleAnimationParameter, 0f, list, PerformAnimatorSanityChecks);
				MMAnimatorExtensions.UpdateAnimatorFloat(animator, _weaponAngleRelativeAnimationParameter, 0f, list, PerformAnimatorSanityChecks);
			}

			if (_comboWeapon != null)
			{
				MMAnimatorExtensions.UpdateAnimatorBool(animator, _comboInProgressAnimationParameter, _comboWeapon.ComboInProgress, list, PerformAnimatorSanityChecks);
			}
		}
	}
}