using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 다른 클래스가 수신할 수 있도록 상태 값이 변경될 때마다 트리거되는 이벤트
    /// </summary>
    public struct HealthChangeEvent
    {
        public Health AffectedHealth;
        public float NewHealth;

        public HealthChangeEvent(Health affectedHealth, float newHealth)
        {
            AffectedHealth = affectedHealth;
            NewHealth = newHealth;
        }

        static HealthChangeEvent e;
        public static void Trigger(Health affectedHealth, float newHealth)
        {
            e.AffectedHealth = affectedHealth;
            e.NewHealth = newHealth;
            MMEventManager.TriggerEvent(e);
        }
    }

    /// <summary>
    /// 이 클래스는 개체의 체력을 관리하고, 잠재적인 체력 바를 조종하며, 피해를 입을 때 일어나는 일과 죽을 때 일어나는 일을 처리합니다.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/Core/Health")]
    public class Health : MMMonoBehaviour
    {
        [MMInspectorGroup("Bindings", true, 3)]

        /// 비활성화할 모델(그렇게 설정된 경우)
        [Tooltip("비활성화할 모델(그렇게 설정된 경우)")]
        public GameObject Model;
        public GameObject enemy;

        [MMInspectorGroup("Status", true, 29)]

        /// 캐릭터의 현재 체력
        [MMReadOnly]
        [Tooltip("캐릭터의 현재 체력")]
        public float CurrentHealth;
        /// 이것이 사실이라면 이 개체는 현재 피해를 입을 수 없습니다.
        [MMReadOnly]
        [Tooltip("이것이 사실이라면 이 개체는 현재 피해를 입을 수 없습니다.")]
        public bool Invulnerable = false;

        [MMInspectorGroup("Health", true, 5)]

        [MMInformation("이 구성 요소를 개체에 추가하면 체력이 생기고 손상을 입어 잠재적으로 죽을 수 있습니다.", MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]
        /// 객체의 초기 체력
        [Tooltip("객체의 초기 체력")]
        public float InitialHealth = 10;
        /// 개체의 최대 체력
        [Tooltip("개체의 최대 체력")]
        public float MaximumHealth = 10;
        /// 이것이 사실이라면 이 캐릭터가 활성화될 때마다(보통 장면이 시작될 때) 체력 값이 재설정됩니다.
        [Tooltip("이것이 사실이라면 이 캐릭터가 활성화될 때마다(보통 장면이 시작될 때) 체력 값이 재설정됩니다.")]
        public bool ResetHealthOnEnable = true;

        [MMInspectorGroup("Damage", true, 6)]

        [MMInformation("여기서는 개체가 손상될 때 인스턴스화할 효과와 사운드 FX를 지정할 수 있으며, 개체가 충돌할 때 깜박이는 시간도 지정할 수 있습니다(스프라이트에만 작동).", MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]
        /// 이 Health 개체가 손상될 수 있는지 여부
        [Tooltip("이 Health 개체가 손상될 수 있는지 여부")]
        public bool ImmuneToDamage = false;
        /// 피해를 입었을 때 플레이할 피드백
        [Tooltip("피해를 입었을 때 플레이할 피드백")]
        public MMFeedbacks DamageMMFeedbacks;
        /// 이것이 사실이라면 피해 값은 Intensity 매개변수로 MMFeedbacks에 전달되어 피해가 증가함에 따라 더욱 강렬한 피드백을 트리거할 수 있습니다.
        [Tooltip("이것이 사실이라면 피해 값은 Intensity 매개변수로 MMFeedbacks에 전달되어 피해가 증가함에 따라 더욱 강렬한 피드백을 트리거할 수 있습니다.")]
        public bool FeedbackIsProportionalToDamage = false;
        /// 이것을 true로 설정하면 이 객체에 피해를 주는 다른 객체는 자체 피해를 입지 않습니다.
        [Tooltip("이것을 true로 설정하면 이 객체에 피해를 주는 다른 객체는 자체 피해를 입지 않습니다.")]
        public bool PreventTakeSelfDamage = false;

        [MMInspectorGroup("Knockback", true, 63)]

        /// 이 물체가 피해 넉백에 면역인지 여부
        [Tooltip("이 물체가 피해 넉백에 면역인지 여부")]
        public bool ImmuneToKnockback = false;
        /// 받은 피해가 0인 경우 이 개체가 피해 넉백에 면역인지 여부
        [Tooltip("받은 피해가 0인 경우 이 개체가 피해 넉백에 면역인지 여부")]
        public bool ImmuneToKnockbackIfZeroDamage = false;
        /// 들어오는 넉백 힘에 적용되는 승수입니다. 0은 모든 넉백을 취소하고, 0.5는 반으로 줄이며, 1은 효과가 없으며, 2는 넉백 힘을 두 배로 늘립니다.
        [Tooltip("들어오는 넉백 힘에 적용되는 승수입니다. 0은 모든 넉백을 취소하고, 0.5는 반으로 줄이며, 1은 효과가 없으며, 2는 넉백 힘을 두 배로 늘립니다.")]
        public float KnockbackForceMultiplier = 1f;

        [MMInspectorGroup("Death", true, 53)]

        [MMInformation("여기서는 개체가 죽을 때 인스턴스화할 효과, 개체에 적용할 힘(topdown controller 필요), 게임 점수에 추가할 포인트 수, 캐릭터가 다시 생성되어야 하는 위치(비플레이어 캐릭터에만 해당)를 설정할 수 있습니다.", MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]
        /// 이 개체가 사망 시 파괴되어야 하는지 여부
        [Tooltip("이 개체가 사망 시 파괴되어야 하는지 여부")]
        public bool DestroyOnDeath = true;
        /// 캐릭터가 파괴되거나 비활성화되기 전의 시간(초)
        [Tooltip("캐릭터가 파괴되거나 비활성화되기 전의 시간(초)")]
        public float DelayBeforeDestruction = 0f;
        /// 개체의 체력이 0에 도달할 때 플레이어가 얻는 포인트
        [Tooltip("개체의 체력이 0에 도달할 때 플레이어가 얻는 포인트")]
        public int PointsWhenDestroyed;
        /// false로 설정되면 캐릭터는 사망한 위치에서 다시 생성되고, 그렇지 않으면 초기 위치(장면이 시작될 때)로 이동됩니다.
        [Tooltip("false로 설정되면 캐릭터는 사망한 위치에서 다시 생성되고, 그렇지 않으면 초기 위치(장면이 시작될 때)로 이동됩니다.")]
        public bool RespawnAtInitialLocation = false;
        /// 이것이 사실이라면 컨트롤러는 사망 시 비활성화됩니다.
        [Tooltip("이것이 사실이라면 컨트롤러는 사망 시 비활성화됩니다.")]
        public bool DisableControllerOnDeath = true;
        /// 이것이 사실이라면, 사망 시 모델이 즉시 비활성화됩니다(모델이 설정된 경우).
        [Tooltip("이것이 사실이라면, 사망 시 모델이 즉시 비활성화됩니다(모델이 설정된 경우).")]
        public bool DisableModelOnDeath = true;
        /// 이것이 사실이라면 캐릭터가 죽을 때 충돌이 꺼질 것입니다.
        [Tooltip("이것이 사실이라면 캐릭터가 죽을 때 충돌이 꺼질 것입니다.")]
        public bool DisableCollisionsOnDeath = true;
        /// 이것이 사실이라면 캐릭터가 죽을 때 하위 충돌기에서도 충돌이 꺼집니다.
        [Tooltip("이것이 사실이라면 캐릭터가 죽을 때 하위 충돌기에서도 충돌이 꺼집니다.")]
        public bool DisableChildCollisionsOnDeath = false;
        /// 이 객체가 사망 시 레이어를 변경해야 하는지 여부
        [Tooltip("이 객체가 사망 시 레이어를 변경해야 하는지 여부")]
        public bool ChangeLayerOnDeath = false;
        /// 이 객체가 사망 시 레이어를 변경해야 하는지 여부
        [Tooltip("이 객체가 사망 시 레이어를 재귀적으로 변경해야 하는지 여부")]
        public bool ChangeLayersRecursivelyOnDeath = false;
        /// 이 캐릭터가 죽을 때 이동해야 하는 레이어
        [Tooltip("이 캐릭터가 죽을 때 이동해야 하는 레이어")]
        public MMLayer LayerOnDeath;
        /// 죽을 때 플레이할 피드백
        [Tooltip("죽을 때 플레이할 피드백")]
        public MMFeedbacks DeathMMFeedbacks;

        /// 이것이 사실이라면 부활 시 색상이 재설정됩니다.
        [Tooltip("이것이 사실이라면 부활 시 색상이 재설정됩니다.")]
        public bool ResetColorOnRevive = true;
        /// 색상을 정의하는 렌더러 셰이더의 속성 이름
        [Tooltip("색상을 정의하는 렌더러 셰이더의 속성 이름")]
        [MMCondition("ResetColorOnRevive", true)]
        public string ColorMaterialPropertyName = "_Color";
        /// 이것이 사실이라면 이 구성요소는 재료의 인스턴스에서 작업하는 대신 재료 특성 블록을 사용합니다.
        [Tooltip("이것이 사실이라면 이 구성요소는 재료의 인스턴스에서 작업하는 대신 재료 특성 블록을 사용합니다.")]
        public bool UseMaterialPropertyBlocks = false;

        [MMInspectorGroup("Shared Health and Damage Resistance", true, 12)]
        /// 모든 건강이 리디렉션되는 다른 건강 구성 요소(일반적으로 다른 캐릭터)
        [Tooltip("모든 건강이 리디렉션되는 다른 건강 구성 요소(일반적으로 다른 캐릭터)")]
        public Health MasterHealth;
        /// 이 Health가 데미지를 받았을 때 처리하는 데 사용할 DamageResistanceProcessor
        [Tooltip("이 Health가 데미지를 받았을 때 처리하는 데 사용할 DamageResistanceProcessor")]
        public DamageResistanceProcessor TargetDamageResistanceProcessor;

        [MMInspectorGroup("Animator", true, 14)]
        /// 죽음 애니메이션 매개변수를 전달할 대상 애니메이터입니다. 상태 구성 요소는 비어 있는 경우 자동 바인딩을 시도합니다.
        [Tooltip("죽음 애니메이션 매개변수를 전달할 대상 애니메이터입니다. 상태 구성 요소는 비어 있는 경우 자동 바인딩을 시도합니다.")]
        public Animator TargetAnimator;
        /// 이것이 사실인 경우 잠재적인 스팸을 방지하기 위해 연결된 애니메이터에 대한 애니메이터 로그가 꺼집니다.
        [Tooltip("이것이 사실인 경우 잠재적인 스팸을 방지하기 위해 연결된 애니메이터에 대한 애니메이터 로그가 꺼집니다.")]
        public bool DisableAnimatorLogs = true;

        public float LastDamage { get; set; }
        public Vector3 LastDamageDirection { get; set; }
        public bool Initialized => _initialized;

        // hit delegate
        public delegate void OnHitDelegate();
        public OnHitDelegate OnHit;

        // respawn delegate
        public delegate void OnReviveDelegate();
        public OnReviveDelegate OnRevive;

        // death delegate
        public delegate void OnDeathDelegate();
        public OnDeathDelegate OnDeath;

        protected Vector3 _initialPosition;
        protected Renderer _renderer;
        protected Character _character;
        protected CharacterMovement _characterMovement;
        protected TopDownController _controller;

        protected MMHealthBar _healthBar;
        protected Collider2D _collider2D;
        protected Collider _collider3D;
        protected CharacterController _characterController;
        protected bool _initialized = false;
        protected Color _initialColor;
        protected AutoRespawn _autoRespawn;
        protected int _initialLayer;
        protected MaterialPropertyBlock _propertyBlock;
        protected bool _hasColorProperty = false;

        //내가만든 변수
        string thisTag;
        int thisLayer;
        int invincibilityCount;//무적버프 중복으로 먹은 횟수
        PlayerEffectsController playerParticle;
        AIBrain _brain;
        protected class InterruptiblesDamageOverTimeCoroutine
        {
            public Coroutine DamageOverTimeCoroutine;
            public DamageType DamageOverTimeType;
        }

        protected List<InterruptiblesDamageOverTimeCoroutine> _interruptiblesDamageOverTimeCoroutines;
        protected List<InterruptiblesDamageOverTimeCoroutine> _damageOverTimeCoroutines;

        #region Initialization

        /// <summary>
        /// Awake에서는 건강을 초기화합니다.
        /// </summary>
        protected virtual void Awake()
        {
            Initialization();
            InitializeCurrentHealth();
            _brain = GetComponentInChildren<AIBrain>();

        }

        /// <summary>
        /// 시작하면 애니메이터를 잡습니다.
        /// </summary>
        protected virtual void Start()
        {
            thisTag = this.gameObject.tag;
            thisLayer = this.gameObject.layer;
            GrabAnimator();
            if (gameObject.tag == "Player")
            {
                CurrentHealth = DataManager.Instance.datas.Heath;
                MaximumHealth = DataManager.Instance.datas.Heath;
                playerParticle = GetComponent<PlayerEffectsController>();
            }
        }

        /// <summary>
        /// 유용한 구성 요소를 잡고 손상을 입히고 초기 색상을 얻습니다.
        /// </summary>
        public virtual void Initialization()
        {
            _character = this.gameObject.GetComponentInParent<Character>();

            if (Model != null)
            {
                Model.SetActive(true);
            }

            if (gameObject.GetComponentInParent<Renderer>() != null)
            {
                _renderer = GetComponentInParent<Renderer>();
            }
            if (_character != null)
            {
                _characterMovement = _character.FindAbility<CharacterMovement>();
                if (_character.CharacterModel != null)
                {
                    if (_character.CharacterModel.GetComponentInChildren<Renderer>() != null)
                    {
                        _renderer = _character.CharacterModel.GetComponentInChildren<Renderer>();
                    }
                }
            }
            if (_renderer != null)
            {
                if (UseMaterialPropertyBlocks && (_propertyBlock == null))
                {
                    _propertyBlock = new MaterialPropertyBlock();
                }

                if (ResetColorOnRevive)
                {
                    if (UseMaterialPropertyBlocks)
                    {
                        if (_renderer.sharedMaterial.HasProperty(ColorMaterialPropertyName))
                        {
                            _hasColorProperty = true;
                            _initialColor = _renderer.sharedMaterial.GetColor(ColorMaterialPropertyName);
                        }
                    }
                    else
                    {
                        if (_renderer.material.HasProperty(ColorMaterialPropertyName))
                        {
                            _hasColorProperty = true;
                            _initialColor = _renderer.material.GetColor(ColorMaterialPropertyName);
                        }
                    }
                }
            }

            _interruptiblesDamageOverTimeCoroutines = new List<InterruptiblesDamageOverTimeCoroutine>();
            _damageOverTimeCoroutines = new List<InterruptiblesDamageOverTimeCoroutine>();
            _initialLayer = gameObject.layer;

            _autoRespawn = this.gameObject.GetComponentInParent<AutoRespawn>();
            _healthBar = this.gameObject.GetComponentInParent<MMHealthBar>();
            _controller = this.gameObject.GetComponentInParent<TopDownController>();
            _characterController = this.gameObject.GetComponentInParent<CharacterController>();
            _collider2D = this.gameObject.GetComponentInParent<Collider2D>();
            _collider3D = this.gameObject.GetComponentInParent<Collider>();

            DamageMMFeedbacks?.Initialization(this.gameObject);
            DeathMMFeedbacks?.Initialization(this.gameObject);

            StoreInitialPosition();
            _initialized = true;

            DamageEnabled();
        }

        /// <summary>
        /// 대상 애니메이터를 잡습니다.
        /// </summary>
        protected virtual void GrabAnimator()
        {
            if (TargetAnimator == null)
            {
                BindAnimator();
            }

            if ((TargetAnimator != null) && DisableAnimatorLogs)
            {
                TargetAnimator.logWarnings = false;
            }
        }

        /// <summary>
        /// 가능하다면 애니메이터를 찾아 바인딩합니다.
        /// </summary>
        protected virtual void BindAnimator()
        {
            if (_character != null)
            {
                if (_character.CharacterAnimator != null)
                {
                    TargetAnimator = _character.CharacterAnimator;
                }
                else
                {
                    TargetAnimator = GetComponent<Animator>();
                }
            }
            else
            {
                TargetAnimator = GetComponent<Animator>();
            }
        }

        /// <summary>
        /// 추가 사용을 위해 초기 위치를 저장합니다.
        /// </summary>
        public virtual void StoreInitialPosition()
        {
            _initialPosition = this.transform.position;
        }

        /// <summary>
        /// 상태를 초기 값 또는 현재 값으로 초기화합니다.
        /// </summary>
        public virtual void InitializeCurrentHealth()
        {
            if (MasterHealth == null)
            {
                SetHealth(InitialHealth);
            }
            else
            {
                if (MasterHealth.Initialized)
                {
                    SetHealth(MasterHealth.CurrentHealth);
                }
                else
                {
                    SetHealth(MasterHealth.InitialHealth);
                }
            }
        }

        /// <summary>
        /// 객체가 활성화되면(예를 들어 다시 생성될 때) 초기 체력 수준을 복원합니다.
        /// </summary>
        protected virtual void OnEnable()
        {
            if (ResetHealthOnEnable)
            {
                InitializeCurrentHealth();
            }
            if (Model != null)
            {
                Model.SetActive(true);
            }
            DamageEnabled();
        }

        /// <summary>
        /// 비활성화하면 지연된 파괴가 실행되는 것을 방지합니다.
        /// </summary>
        protected virtual void OnDisable()
        {
            CancelInvoke();
        }

        #endregion

        /// <summary>
        /// 이 Health 구성 요소가 이번 프레임에 손상될 수 있으면 true를 반환하고, 그렇지 않으면 false를 반환합니다.
        /// </summary>
        /// <returns></returns>
        public virtual bool CanTakeDamageThisFrame()
        {
            // if the object is invulnerable, we do nothing and exit
            if (Invulnerable || ImmuneToDamage)
            {
                return false;
            }

            if (!this.enabled)
            {
                return false;
            }

            // 이미 0보다 낮으면 아무것도 하지 않고 종료합니다.
            if ((CurrentHealth <= 0) && (InitialHealth != 0))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 개체가 손상을 입을 때 호출됩니다
        /// </summary>
        /// <param name="damage">손실될 건강 포인트의 양입니다.</param>
        /// <param name="instigator">피해를 입힌 개체입니다.</param>
        /// <param name="flickerDuration">손상을 입은 후 물체가 깜박여야 하는 시간(초) - 더 이상 사용되지 않고 retrocompatibility이 깨지지 않도록 유지됩니다</param>
        /// <param name="invincibilityDuration">타격 후 짧은 무적의 지속 시간.</param>
        public virtual void Damage(float damage, GameObject instigator, float flickerDuration, float invincibilityDuration, Vector3 damageDirection, List<TypedDamage> typedDamages = null)
        {
            Debug.Log($"[{this.gameObject.name}] 가 [{damage}] 데미지 입음");
            if (!CanTakeDamageThisFrame())
            {
                return;
            }

            damage = ComputeDamageOutput(damage, typedDamages, true);

            // 우리는 피해로 캐릭터의 건강을 떨어뜨립니다
            float previousHealth = CurrentHealth;
            if (MasterHealth != null)
            {
                previousHealth = MasterHealth.CurrentHealth;
                MasterHealth.SetHealth(MasterHealth.CurrentHealth - damage);
            }
            else
            {
                SetHealth(CurrentHealth - damage);
            }

            LastDamage = damage;
            LastDamageDirection = damageDirection;
            if (OnHit != null)
            {
                OnHit();
            }

            // 캐릭터가 발사체, 플레이어 및 적과 충돌하는 것을 방지합니다
            if (invincibilityDuration > 0)
            {
                DamageDisabled();
                StartCoroutine(DamageEnabled(invincibilityDuration));
            }

            // 우리는 피해를 입은 사건을 유발합니다
            MMDamageTakenEvent.Trigger(this, instigator, CurrentHealth, damage, previousHealth);

            // 우리는 애니메이터를 업데이트합니다
            if (TargetAnimator != null)
            {
                TargetAnimator.SetTrigger("Damage");
            }

            // 우리는 피드백을 재생합니다
            if (FeedbackIsProportionalToDamage)
            {
                DamageMMFeedbacks?.PlayFeedbacks(this.transform.position, damage);
            }
            else
            {
                DamageMMFeedbacks?.PlayFeedbacks(this.transform.position);
            }

            // 상태 표시줄을 업데이트합니다
            UpdateHealthBar(true);

            // 우리는 모든 조건 상태 변경을 처리합니다
            ComputeCharacterConditionStateChanges(typedDamages);
            ComputeCharacterMovementMultipliers(typedDamages);

            // 건강이 0에 도달하면 건강을 0으로 설정합니다 (useful for the healthbar)
            if (MasterHealth != null)
            {
                if (MasterHealth.CurrentHealth <= 0)
                {
                    MasterHealth.CurrentHealth = 0;
                    MasterHealth.Kill();
                }
            }
            else
            {
                if (CurrentHealth <= 0)
                {
                    //자기 자신의 태그를확인해서 게임이벤트 트리거 하는 함수 만들어야함
                    TagCheckEventOccurs();
                    CurrentHealth = 0;
                    Kill();
                }

            }
        }

        //자기자신 태그확인후 이벤트 트리거함
        public virtual void TagCheckEventOccurs()
        {
            switch (thisTag)
            {
                case "Player": MMGameEvent.Trigger("PlayerDie"); break;
                case "Slime": MMGameEvent.Trigger("SlimeDie"); break;
                case "Mushroom": MMGameEvent.Trigger("MushroomDie"); break;
                case "Cactus": MMGameEvent.Trigger("CactusDie"); break;
                case "Spider": MMGameEvent.Trigger("SpiderDie"); break;
                case "Skeleton": MMGameEvent.Trigger("SkeletonDie"); break;
                case "Beholder": MMGameEvent.Trigger("BeholderDie"); break;
            }
        }

        /// <summary>
        /// 유형에 관계없이 시간이 지남에 따라 모든 피해를 차단합니다. //시간이 지남에 따라 모든 피해 = 지속피해 즉, 지속피해를 끝낸다는 말임
        /// </summary>
        public virtual void InterruptAllDamageOverTime()
        {
            foreach (InterruptiblesDamageOverTimeCoroutine coroutine in _interruptiblesDamageOverTimeCoroutines)
            {
                StopCoroutine(coroutine.DamageOverTimeCoroutine);
            }
            _interruptiblesDamageOverTimeCoroutines.Clear();
        }

        /// <summary>
        /// 방해할 수 없는 피해까지 포함하여 시간이 지남에 따라 모든 피해를 차단합니다(보통 사망 시). //시간이 지남에 따라 모든 피해 = 지속피해 즉, 지속피해를 끝낸다는 말임
        /// </summary>
        public virtual void StopAllDamageOverTime()
        {
            foreach (InterruptiblesDamageOverTimeCoroutine coroutine in _damageOverTimeCoroutines)
            {
                StopCoroutine(coroutine.DamageOverTimeCoroutine);
            }
            _damageOverTimeCoroutines.Clear();
        }

        /// <summary>
        /// 지정된 유형의 시간이 지남에 따라 모든 피해를 차단합니다. //시간이 지남에 따라 모든 피해 = 지속피해 즉, 지속피해를 끝낸다는 말임
        /// </summary>
        /// <param name="damageType"></param>
        public virtual void InterruptAllDamageOverTimeOfType(DamageType damageType)
        {
            foreach (InterruptiblesDamageOverTimeCoroutine coroutine in _interruptiblesDamageOverTimeCoroutines)
            {
                if (coroutine.DamageOverTimeType == damageType)
                {
                    StopCoroutine(coroutine.DamageOverTimeCoroutine);
                }
            }
            TargetDamageResistanceProcessor?.InterruptDamageOverTime(damageType);
        }

        /// <summary>
        /// 지정된 반복 횟수 동안 시간이 지남에 따라 손상을 적용합니다(첫 번째 손상 적용을 포함하여 검사기에서 지정된 간격으로 빠른 계산을 더 쉽게 수행할 수 있음).
        /// 선택적으로 피해를 중단할 수 있는지 결정할 수 있습니다. 이 경우 InterruptAllDamageOverTime()을 호출하면 이러한 적용이 중단되며, 예를 들어 독을 치료하는 데 유용합니다.
        /// </summary>
        /// <param name="damage"></param>
        /// <param name="instigator"></param>
        /// <param name="flickerDuration"></param>
        /// <param name="invincibilityDuration"></param>
        /// <param name="damageDirection"></param>
        /// <param name="typedDamages"></param>
        /// <param name="amountOfRepeats"></param>
        /// <param name="durationBetweenRepeats"></param>
        /// <param name="interruptible"></param>
        public virtual void DamageOverTime(float damage, GameObject instigator, float flickerDuration,
            float invincibilityDuration, Vector3 damageDirection, List<TypedDamage> typedDamages = null,
            int amountOfRepeats = 0, float durationBetweenRepeats = 1f, bool interruptible = true, DamageType damageType = null)
        {
            if (ComputeDamageOutput(damage, typedDamages, false) == 0)
            {
                return;
            }

            InterruptiblesDamageOverTimeCoroutine damageOverTime = new InterruptiblesDamageOverTimeCoroutine();
            damageOverTime.DamageOverTimeType = damageType;
            damageOverTime.DamageOverTimeCoroutine = StartCoroutine(DamageOverTimeCo(damage, instigator, flickerDuration,
                invincibilityDuration, damageDirection, typedDamages, amountOfRepeats, durationBetweenRepeats,
                interruptible));
            _damageOverTimeCoroutines.Add(damageOverTime);
            if (interruptible)
            {
                _interruptiblesDamageOverTimeCoroutines.Add(damageOverTime);
            }
        }

        /// <summary>
        /// 시간이 지남에 따라 손상을 가하는 데 사용되는 코루틴
        /// </summary>
        /// <param name="damage"></param>
        /// <param name="instigator"></param>
        /// <param name="flickerDuration"></param>
        /// <param name="invincibilityDuration"></param>
        /// <param name="damageDirection"></param>
        /// <param name="typedDamages"></param>
        /// <param name="amountOfRepeats"></param>
        /// <param name="durationBetweenRepeats"></param>
        /// <param name="interruptible"></param>
        /// <param name="damageType"></param>
        /// <returns></returns>
        protected virtual IEnumerator DamageOverTimeCo(float damage, GameObject instigator, float flickerDuration,
            float invincibilityDuration, Vector3 damageDirection, List<TypedDamage> typedDamages = null,
            int amountOfRepeats = 0, float durationBetweenRepeats = 1f, bool interruptible = true, DamageType damageType = null)
        {
            for (int i = 0; i < amountOfRepeats; i++)
            {
                Damage(damage, instigator, flickerDuration, invincibilityDuration, damageDirection, typedDamages);
                yield return MMCoroutine.WaitFor(durationBetweenRepeats);
            }
        }

        /// <summary>
        /// 잠재적 저항을 처리한 후 이 체력이 입어야 하는 피해를 반환합니다.
        /// </summary>
        /// <param name="damage"></param>
        /// <returns></returns>
        public virtual float ComputeDamageOutput(float damage, List<TypedDamage> typedDamages = null, bool damageApplied = false)
        {
            if (Invulnerable || ImmuneToDamage)
            {
                return 0;
            }

            float totalDamage = 0f;
            // 우리는 잠재적인 저항을 통해 피해를 처리합니다.
            if (TargetDamageResistanceProcessor != null)
            {
                if (TargetDamageResistanceProcessor.isActiveAndEnabled)
                {
                    totalDamage = TargetDamageResistanceProcessor.ProcessDamage(damage, typedDamages, damageApplied);
                }
            }
            else
            {
                totalDamage = damage;
                if (typedDamages != null)
                {
                    foreach (TypedDamage typedDamage in typedDamages)
                    {
                        totalDamage += typedDamage.DamageCaused;
                    }
                }
            }
            return totalDamage;
        }

        /// <summary>
        /// 저항을 통과하고 필요한 경우 조건 상태 변경을 적용합니다.
        /// </summary>
        /// <param name="typedDamages"></param>
        protected virtual void ComputeCharacterConditionStateChanges(List<TypedDamage> typedDamages)
        {
            if ((typedDamages == null) || (_character == null))
            {
                return;
            }

            foreach (TypedDamage typedDamage in typedDamages)
            {
                if (typedDamage.ForceCharacterCondition)
                {
                    if (TargetDamageResistanceProcessor != null)
                    {
                        if (TargetDamageResistanceProcessor.isActiveAndEnabled)
                        {
                            bool checkResistance =
                                TargetDamageResistanceProcessor.CheckPreventCharacterConditionChange(typedDamage.AssociatedDamageType);
                            if (checkResistance)
                            {
                                continue;
                            }
                        }
                    }
                    _character.ChangeCharacterConditionTemporarily(typedDamage.ForcedCondition, typedDamage.ForcedConditionDuration, typedDamage.ResetControllerForces, typedDamage.DisableGravity);
                }
            }

        }

        /// <summary>
        /// 저항 목록을 살펴보고 필요한 경우 이동 승수를 적용합니다.
        /// </summary>
        /// <param name="typedDamages"></param>
        protected virtual void ComputeCharacterMovementMultipliers(List<TypedDamage> typedDamages)
        {
            if ((typedDamages == null) || (_character == null))
            {
                return;
            }

            foreach (TypedDamage typedDamage in typedDamages)
            {
                if (typedDamage.ApplyMovementMultiplier)
                {
                    if (TargetDamageResistanceProcessor != null)
                    {
                        if (TargetDamageResistanceProcessor.isActiveAndEnabled)
                        {
                            bool checkResistance =
                                TargetDamageResistanceProcessor.CheckPreventMovementModifier(typedDamage.AssociatedDamageType);
                            if (checkResistance)
                            {
                                continue;
                            }
                        }
                    }
                    _characterMovement?.ApplyMovementMultiplier(typedDamage.MovementMultiplier,
                        typedDamage.MovementMultiplierDuration);
                }
            }
        }

        /// <summary>
        /// 저항을 통해 처리하여 새로운 넉백 힘을 결정합니다.
        /// </summary>
        /// <param name="knockbackForce"></param>
        /// <param name="typedDamages"></param>
        /// <returns></returns>
        public virtual Vector3 ComputeKnockbackForce(Vector3 knockbackForce, List<TypedDamage> typedDamages = null)
        {
            return (TargetDamageResistanceProcessor == null) ? knockbackForce : TargetDamageResistanceProcessor.ProcessKnockbackForce(knockbackForce, typedDamages); ;

        }

        /// <summary>
        /// 이 체력이 밀려날 수 있으면 true를 반환하고, 그렇지 않으면 false를 반환합니다.
        /// </summary>
        /// <param name="typedDamages"></param>
        /// <returns></returns>
        public virtual bool CanGetKnockback(List<TypedDamage> typedDamages)
        {
            if (ImmuneToKnockback)
            {
                return false;
            }
            if (TargetDamageResistanceProcessor != null)
            {
                if (TargetDamageResistanceProcessor.isActiveAndEnabled)
                {
                    bool checkResistance = TargetDamageResistanceProcessor.CheckPreventKnockback(typedDamages);
                    if (checkResistance)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// 캐릭터를 죽이고, 사망 효과를 인스턴스화하고, 포인트를 처리하는 등의 작업을 수행합니다.
        /// </summary>
        public virtual void Kill()
        {
            if (ImmuneToDamage)
            {
                return;
            }

            if (_character != null)
            {
                // 우리는 죽은 상태를 true로 설정했습니다
                _character.ConditionState.ChangeState(CharacterStates.CharacterConditions.Dead);
                _character.Reset();

                if (_character.CharacterType == Character.CharacterTypes.Player)
                {
                    TopDownEngineEvent.Trigger(TopDownEngineEventTypes.PlayerDeath, _character);
                }
            }
            SetHealth(0);

            // 추가 피해를 방지하겠습니다
            StopAllDamageOverTime();
            DamageDisabled();

            DeathMMFeedbacks?.PlayFeedbacks(this.transform.position);

            // 필요한 경우 포인트를 추가합니다.
            if (PointsWhenDestroyed != 0)
            {
                // GameManager가 포착할 수 있는 새로운 포인트 이벤트(및 이를 수신할 수 있는 다른 클래스)를 보냅니다.
                TopDownEnginePointEvent.Trigger(PointsMethods.Add, PointsWhenDestroyed);
            }

            if (TargetAnimator != null)
            {
                TargetAnimator.SetTrigger("Death");
            }
            // 이제부터 충돌을 무시하도록 만듭니다.
            if (DisableCollisionsOnDeath)
            {
                if (_collider2D != null)
                {
                    _collider2D.enabled = false;
                }
                if (_collider3D != null)
                {
                    _collider3D.enabled = false;
                }

                // 컨트롤러가 있는 경우 충돌을 제거하고 잠재적인 부활을 위한 매개변수를 복원하고 죽음의 힘을 적용합니다.
                if (_controller != null)
                {
                    _controller.CollisionsOff();
                }

                if (DisableChildCollisionsOnDeath)
                {
                    foreach (Collider2D collider in this.gameObject.GetComponentsInChildren<Collider2D>())
                    {
                        collider.enabled = false;
                    }
                    foreach (Collider collider in this.gameObject.GetComponentsInChildren<Collider>())
                    {
                        collider.enabled = false;
                    }
                }
            }

            if (ChangeLayerOnDeath)
            {
                gameObject.layer = LayerOnDeath.LayerIndex;
                if (ChangeLayersRecursivelyOnDeath)
                {
                    this.transform.ChangeLayersRecursively(LayerOnDeath.LayerIndex);
                }
            }

            OnDeath?.Invoke();
            MMLifeCycleEvent.Trigger(this, MMLifeCycleEventTypes.Death);

            if (DisableControllerOnDeath && (_controller != null))
            {
                _controller.enabled = false;
            }

            if (DisableControllerOnDeath && (_characterController != null))
            {
                _characterController.enabled = false;
            }

            if (DisableModelOnDeath && (Model != null))
            {
                Model.SetActive(false);
            }

            if (DelayBeforeDestruction > 0f)
            {
                Invoke("DestroyObject", DelayBeforeDestruction);
            }
            else
            {
                // 마침내 우리는 그 물체를 파괴한다
                if (_brain != null)
                    _brain.BrainActive = false;
                DestroyObject();
            }
        }

        /// <summary>
        /// 이 개체를 되살리세요.
        /// </summary>
        public virtual void Revive()
        {
            if (!_initialized)
            {
                return;
            }

            if (_collider2D != null)
            {
                _collider2D.enabled = true;
            }
            if (_collider3D != null)
            {
                _collider3D.enabled = true;
            }
            if (DisableChildCollisionsOnDeath)
            {
                foreach (Collider2D collider in this.gameObject.GetComponentsInChildren<Collider2D>())
                {
                    collider.enabled = true;
                }
                foreach (Collider collider in this.gameObject.GetComponentsInChildren<Collider>())
                {
                    collider.enabled = true;
                }
            }
            if (ChangeLayerOnDeath)
            {
                gameObject.layer = _initialLayer;
                if (ChangeLayersRecursivelyOnDeath)
                {
                    this.transform.ChangeLayersRecursively(_initialLayer);
                }
            }
            if (_characterController != null)
            {
                _characterController.enabled = true;
            }
            if (_controller != null)
            {
                _controller.enabled = true;
                _controller.CollisionsOn();
                _controller.Reset();
            }
            if (_character != null)
            {
                _character.ConditionState.ChangeState(CharacterStates.CharacterConditions.Normal);
            }
            if (ResetColorOnRevive && (_renderer != null))
            {
                if (UseMaterialPropertyBlocks)
                {
                    _renderer.GetPropertyBlock(_propertyBlock);
                    _propertyBlock.SetColor(ColorMaterialPropertyName, _initialColor);
                    _renderer.SetPropertyBlock(_propertyBlock);
                }
                else
                {
                    _renderer.material.SetColor(ColorMaterialPropertyName, _initialColor);
                }
            }

            if (RespawnAtInitialLocation)
            {
                transform.position = _initialPosition;
            }
            if (_healthBar != null)
            {
                _healthBar.Initialization();
            }

            Initialization();
            InitializeCurrentHealth();
            OnRevive?.Invoke();
            MMLifeCycleEvent.Trigger(this, MMLifeCycleEventTypes.Revive);
        }

        /// <summary>
        /// 캐릭터의 설정에 따라 물체를 파괴하거나 파괴하려고 시도합니다.
        /// </summary>
        protected virtual void DestroyObject()
        {
            if (_autoRespawn == null)
            {
                if (DestroyOnDeath)
                {
                    //오브젝트 회수
                    if (thisLayer == 13 || thisLayer == 14)//13 - 에너미, 14 - 공중에너미
                        gameObject.SetActive(false);
                }
            }
            else
            {
                _autoRespawn.Kill();
            }
        }

        //자신의 태그를 확인후 알맞는 회수함수 호출
        void TagCheckReturnPool()
        {
            switch (thisTag)
            {
                case "Slime": CreateManager.Instance.ReturnPool(this.gameObject.GetComponent<Slime>()); Debug.Log("슬라임 회수됨"); break;
            }
        }

        #region HealthManipulationAPIs


        /// <summary>
        /// 현재 상태를 지정된 새 값으로 설정하고 상태 표시줄을 업데이트합니다
        /// </summary>
        /// <param name="newValue"></param>
        public virtual void SetHealth(float newValue)
        {
            CurrentHealth = newValue;
            UpdateHealthBar(false);
            HealthChangeEvent.Trigger(this, newValue);
        }

        /// <summary>
        /// 캐릭터가 체력을 얻을 때 호출됩니다(예: a stimpack for example에서).
        /// </summary>
        /// <param name="health">캐릭터가 얻는 건강.</param>
        /// <param name="instigator">캐릭터에게 건강을 주는 것.</param>
        public virtual void ReceiveHealth(float health, GameObject instigator)
        {
            // 이 기능은 캐릭터의 체력에 체력을 추가하고 최대 체력(MaxHealth)을 초과하는 것을 방지합니다.
            if (MasterHealth != null)
            {
                MasterHealth.SetHealth(Mathf.Min(CurrentHealth + health, MaximumHealth));
            }
            else
            {
                SetHealth(Mathf.Min(CurrentHealth + health, MaximumHealth));
            }

            UpdateHealthBar(true);
        }

        /// <summary>
        /// 캐릭터의 체력을 최대값으로 재설정합니다.
        /// </summary>
        public virtual void ResetHealthToMaxHealth()
        {
            SetHealth(MaximumHealth);
        }

        /// <summary>
        /// 캐릭터의 상태 표시줄을 강제로 새로 고칩니다
        /// </summary>
        public virtual void UpdateHealthBar(bool show)
        {
            if (_healthBar != null)
            {
                _healthBar.UpdateBar(CurrentHealth, 0f, MaximumHealth, show);
            }

            if (gameObject.tag == "Player")
                GUIManager.Instance.UpdateHealthBarText(CurrentHealth, MaximumHealth);

            if (MasterHealth == null)
            {
                if (_character != null)
                {
                    if (_character.CharacterType == Character.CharacterTypes.Player)
                    {
                        // We update the health bar
                        if (GUIManager.HasInstance)
                        {
                            GUIManager.Instance.UpdateHealthBar(CurrentHealth, 0f, MaximumHealth, _character.PlayerID);
                        }
                    }
                }
            }
        }

        //최대체력을 변경합니다.
        public virtual void UpdateMaxHealth(float healthValue)
        {
            MaximumHealth += healthValue;
            CurrentHealth += healthValue;

            UpdateHealthBar(true);
        }

        //최대체력, 현재체력을 초기화 합니다.
        public virtual void UpdateMaxHealth()
        {
            InitialHealth = DataManager.Instance.datas.Heath;
            MaximumHealth = DataManager.Instance.datas.Heath;
            CurrentHealth = DataManager.Instance.datas.Heath;
            UpdateHealthBar(true);
        }

        #endregion

        #region DamageDisablingAPIs

        /// <summary>
        /// 캐릭터가 피해를 입지 않도록 방지
        /// </summary>
        public virtual void DamageDisabled()
        {
            Invulnerable = true;
        }

        //캐릭터가 파라미터(초) 동안 피해를 입지 않습니다. 파라미터(초)이후에는 다시 피해를 입습니다.
        public virtual void DamageDisabled(float invincibilityTime_)
        {
            Invulnerable = true;
            StartCoroutine(DamageEnabled(invincibilityTime_));
        }

        /// <summary>
        /// 캐릭터가 피해를 입을 수 있게 해줍니다.
        /// </summary>
        public virtual void DamageEnabled()
        {
            Invulnerable = false;
        }

        /// <summary>
        /// 캐릭터가 지정된 지연 후에 다시 피해를 입을 수 있도록 합니다.
        /// </summary>
        /// <returns>The layer collision.</returns>
        public virtual IEnumerator DamageEnabled(float delay)
        {
            invincibilityCount++;
            yield return new WaitForSeconds(delay);
            invincibilityCount--;
            if (invincibilityCount <= 0)
                Invulnerable = false;
        }

        #endregion
    }
}