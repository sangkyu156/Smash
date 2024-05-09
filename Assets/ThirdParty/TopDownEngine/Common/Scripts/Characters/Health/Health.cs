using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// �ٸ� Ŭ������ ������ �� �ֵ��� ���� ���� ����� ������ Ʈ���ŵǴ� �̺�Ʈ
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
    /// �� Ŭ������ ��ü�� ü���� �����ϰ�, �������� ü�� �ٸ� �����ϸ�, ���ظ� ���� �� �Ͼ�� �ϰ� ���� �� �Ͼ�� ���� ó���մϴ�.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/Core/Health")]
    public class Health : MMMonoBehaviour
    {
        [MMInspectorGroup("Bindings", true, 3)]

        /// ��Ȱ��ȭ�� ��(�׷��� ������ ���)
        [Tooltip("��Ȱ��ȭ�� ��(�׷��� ������ ���)")]
        public GameObject Model;
        public GameObject enemy;

        [MMInspectorGroup("Status", true, 29)]

        /// ĳ������ ���� ü��
        [MMReadOnly]
        [Tooltip("ĳ������ ���� ü��")]
        public float CurrentHealth;
        /// �̰��� ����̶�� �� ��ü�� ���� ���ظ� ���� �� �����ϴ�.
        [MMReadOnly]
        [Tooltip("�̰��� ����̶�� �� ��ü�� ���� ���ظ� ���� �� �����ϴ�.")]
        public bool Invulnerable = false;

        [MMInspectorGroup("Health", true, 5)]

        [MMInformation("�� ���� ��Ҹ� ��ü�� �߰��ϸ� ü���� ����� �ջ��� �Ծ� ���������� ���� �� �ֽ��ϴ�.", MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]
        /// ��ü�� �ʱ� ü��
        [Tooltip("��ü�� �ʱ� ü��")]
        public float InitialHealth = 10;
        /// ��ü�� �ִ� ü��
        [Tooltip("��ü�� �ִ� ü��")]
        public float MaximumHealth = 10;
        /// �̰��� ����̶�� �� ĳ���Ͱ� Ȱ��ȭ�� ������(���� ����� ���۵� ��) ü�� ���� �缳���˴ϴ�.
        [Tooltip("�̰��� ����̶�� �� ĳ���Ͱ� Ȱ��ȭ�� ������(���� ����� ���۵� ��) ü�� ���� �缳���˴ϴ�.")]
        public bool ResetHealthOnEnable = true;

        [MMInspectorGroup("Damage", true, 6)]

        [MMInformation("���⼭�� ��ü�� �ջ�� �� �ν��Ͻ�ȭ�� ȿ���� ���� FX�� ������ �� ������, ��ü�� �浹�� �� �����̴� �ð��� ������ �� �ֽ��ϴ�(��������Ʈ���� �۵�).", MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]
        /// �� Health ��ü�� �ջ�� �� �ִ��� ����
        [Tooltip("�� Health ��ü�� �ջ�� �� �ִ��� ����")]
        public bool ImmuneToDamage = false;
        /// ���ظ� �Ծ��� �� �÷����� �ǵ��
        [Tooltip("���ظ� �Ծ��� �� �÷����� �ǵ��")]
        public MMFeedbacks DamageMMFeedbacks;
        /// �̰��� ����̶�� ���� ���� Intensity �Ű������� MMFeedbacks�� ���޵Ǿ� ���ذ� �����Կ� ���� ���� ������ �ǵ���� Ʈ������ �� �ֽ��ϴ�.
        [Tooltip("�̰��� ����̶�� ���� ���� Intensity �Ű������� MMFeedbacks�� ���޵Ǿ� ���ذ� �����Կ� ���� ���� ������ �ǵ���� Ʈ������ �� �ֽ��ϴ�.")]
        public bool FeedbackIsProportionalToDamage = false;
        /// �̰��� true�� �����ϸ� �� ��ü�� ���ظ� �ִ� �ٸ� ��ü�� ��ü ���ظ� ���� �ʽ��ϴ�.
        [Tooltip("�̰��� true�� �����ϸ� �� ��ü�� ���ظ� �ִ� �ٸ� ��ü�� ��ü ���ظ� ���� �ʽ��ϴ�.")]
        public bool PreventTakeSelfDamage = false;

        [MMInspectorGroup("Knockback", true, 63)]

        /// �� ��ü�� ���� �˹鿡 �鿪���� ����
        [Tooltip("�� ��ü�� ���� �˹鿡 �鿪���� ����")]
        public bool ImmuneToKnockback = false;
        /// ���� ���ذ� 0�� ��� �� ��ü�� ���� �˹鿡 �鿪���� ����
        [Tooltip("���� ���ذ� 0�� ��� �� ��ü�� ���� �˹鿡 �鿪���� ����")]
        public bool ImmuneToKnockbackIfZeroDamage = false;
        /// ������ �˹� ���� ����Ǵ� �¼��Դϴ�. 0�� ��� �˹��� ����ϰ�, 0.5�� ������ ���̸�, 1�� ȿ���� ������, 2�� �˹� ���� �� ��� �ø��ϴ�.
        [Tooltip("������ �˹� ���� ����Ǵ� �¼��Դϴ�. 0�� ��� �˹��� ����ϰ�, 0.5�� ������ ���̸�, 1�� ȿ���� ������, 2�� �˹� ���� �� ��� �ø��ϴ�.")]
        public float KnockbackForceMultiplier = 1f;

        [MMInspectorGroup("Death", true, 53)]

        [MMInformation("���⼭�� ��ü�� ���� �� �ν��Ͻ�ȭ�� ȿ��, ��ü�� ������ ��(topdown controller �ʿ�), ���� ������ �߰��� ����Ʈ ��, ĳ���Ͱ� �ٽ� �����Ǿ�� �ϴ� ��ġ(���÷��̾� ĳ���Ϳ��� �ش�)�� ������ �� �ֽ��ϴ�.", MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]
        /// �� ��ü�� ��� �� �ı��Ǿ�� �ϴ��� ����
        [Tooltip("�� ��ü�� ��� �� �ı��Ǿ�� �ϴ��� ����")]
        public bool DestroyOnDeath = true;
        /// ĳ���Ͱ� �ı��ǰų� ��Ȱ��ȭ�Ǳ� ���� �ð�(��)
        [Tooltip("ĳ���Ͱ� �ı��ǰų� ��Ȱ��ȭ�Ǳ� ���� �ð�(��)")]
        public float DelayBeforeDestruction = 0f;
        /// ��ü�� ü���� 0�� ������ �� �÷��̾ ��� ����Ʈ
        [Tooltip("��ü�� ü���� 0�� ������ �� �÷��̾ ��� ����Ʈ")]
        public int PointsWhenDestroyed;
        /// false�� �����Ǹ� ĳ���ʹ� ����� ��ġ���� �ٽ� �����ǰ�, �׷��� ������ �ʱ� ��ġ(����� ���۵� ��)�� �̵��˴ϴ�.
        [Tooltip("false�� �����Ǹ� ĳ���ʹ� ����� ��ġ���� �ٽ� �����ǰ�, �׷��� ������ �ʱ� ��ġ(����� ���۵� ��)�� �̵��˴ϴ�.")]
        public bool RespawnAtInitialLocation = false;
        /// �̰��� ����̶�� ��Ʈ�ѷ��� ��� �� ��Ȱ��ȭ�˴ϴ�.
        [Tooltip("�̰��� ����̶�� ��Ʈ�ѷ��� ��� �� ��Ȱ��ȭ�˴ϴ�.")]
        public bool DisableControllerOnDeath = true;
        /// �̰��� ����̶��, ��� �� ���� ��� ��Ȱ��ȭ�˴ϴ�(���� ������ ���).
        [Tooltip("�̰��� ����̶��, ��� �� ���� ��� ��Ȱ��ȭ�˴ϴ�(���� ������ ���).")]
        public bool DisableModelOnDeath = true;
        /// �̰��� ����̶�� ĳ���Ͱ� ���� �� �浹�� ���� ���Դϴ�.
        [Tooltip("�̰��� ����̶�� ĳ���Ͱ� ���� �� �浹�� ���� ���Դϴ�.")]
        public bool DisableCollisionsOnDeath = true;
        /// �̰��� ����̶�� ĳ���Ͱ� ���� �� ���� �浹�⿡���� �浹�� �����ϴ�.
        [Tooltip("�̰��� ����̶�� ĳ���Ͱ� ���� �� ���� �浹�⿡���� �浹�� �����ϴ�.")]
        public bool DisableChildCollisionsOnDeath = false;
        /// �� ��ü�� ��� �� ���̾ �����ؾ� �ϴ��� ����
        [Tooltip("�� ��ü�� ��� �� ���̾ �����ؾ� �ϴ��� ����")]
        public bool ChangeLayerOnDeath = false;
        /// �� ��ü�� ��� �� ���̾ �����ؾ� �ϴ��� ����
        [Tooltip("�� ��ü�� ��� �� ���̾ ��������� �����ؾ� �ϴ��� ����")]
        public bool ChangeLayersRecursivelyOnDeath = false;
        /// �� ĳ���Ͱ� ���� �� �̵��ؾ� �ϴ� ���̾�
        [Tooltip("�� ĳ���Ͱ� ���� �� �̵��ؾ� �ϴ� ���̾�")]
        public MMLayer LayerOnDeath;
        /// ���� �� �÷����� �ǵ��
        [Tooltip("���� �� �÷����� �ǵ��")]
        public MMFeedbacks DeathMMFeedbacks;

        /// �̰��� ����̶�� ��Ȱ �� ������ �缳���˴ϴ�.
        [Tooltip("�̰��� ����̶�� ��Ȱ �� ������ �缳���˴ϴ�.")]
        public bool ResetColorOnRevive = true;
        /// ������ �����ϴ� ������ ���̴��� �Ӽ� �̸�
        [Tooltip("������ �����ϴ� ������ ���̴��� �Ӽ� �̸�")]
        [MMCondition("ResetColorOnRevive", true)]
        public string ColorMaterialPropertyName = "_Color";
        /// �̰��� ����̶�� �� ������Ҵ� ����� �ν��Ͻ����� �۾��ϴ� ��� ��� Ư�� ����� ����մϴ�.
        [Tooltip("�̰��� ����̶�� �� ������Ҵ� ����� �ν��Ͻ����� �۾��ϴ� ��� ��� Ư�� ����� ����մϴ�.")]
        public bool UseMaterialPropertyBlocks = false;

        [MMInspectorGroup("Shared Health and Damage Resistance", true, 12)]
        /// ��� �ǰ��� ���𷺼ǵǴ� �ٸ� �ǰ� ���� ���(�Ϲ������� �ٸ� ĳ����)
        [Tooltip("��� �ǰ��� ���𷺼ǵǴ� �ٸ� �ǰ� ���� ���(�Ϲ������� �ٸ� ĳ����)")]
        public Health MasterHealth;
        /// �� Health�� �������� �޾��� �� ó���ϴ� �� ����� DamageResistanceProcessor
        [Tooltip("�� Health�� �������� �޾��� �� ó���ϴ� �� ����� DamageResistanceProcessor")]
        public DamageResistanceProcessor TargetDamageResistanceProcessor;

        [MMInspectorGroup("Animator", true, 14)]
        /// ���� �ִϸ��̼� �Ű������� ������ ��� �ִϸ������Դϴ�. ���� ���� ��Ҵ� ��� �ִ� ��� �ڵ� ���ε��� �õ��մϴ�.
        [Tooltip("���� �ִϸ��̼� �Ű������� ������ ��� �ִϸ������Դϴ�. ���� ���� ��Ҵ� ��� �ִ� ��� �ڵ� ���ε��� �õ��մϴ�.")]
        public Animator TargetAnimator;
        /// �̰��� ����� ��� �������� ������ �����ϱ� ���� ����� �ִϸ����Ϳ� ���� �ִϸ����� �αװ� �����ϴ�.
        [Tooltip("�̰��� ����� ��� �������� ������ �����ϱ� ���� ����� �ִϸ����Ϳ� ���� �ִϸ����� �αװ� �����ϴ�.")]
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

        //�������� ����
        string thisTag;
        int thisLayer;
        int invincibilityCount;//�������� �ߺ����� ���� Ƚ��
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
        /// Awake������ �ǰ��� �ʱ�ȭ�մϴ�.
        /// </summary>
        protected virtual void Awake()
        {
            Initialization();
            InitializeCurrentHealth();
            _brain = GetComponentInChildren<AIBrain>();

        }

        /// <summary>
        /// �����ϸ� �ִϸ����͸� ����ϴ�.
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
        /// ������ ���� ��Ҹ� ��� �ջ��� ������ �ʱ� ������ ����ϴ�.
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
        /// ��� �ִϸ����͸� ����ϴ�.
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
        /// �����ϴٸ� �ִϸ����͸� ã�� ���ε��մϴ�.
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
        /// �߰� ����� ���� �ʱ� ��ġ�� �����մϴ�.
        /// </summary>
        public virtual void StoreInitialPosition()
        {
            _initialPosition = this.transform.position;
        }

        /// <summary>
        /// ���¸� �ʱ� �� �Ǵ� ���� ������ �ʱ�ȭ�մϴ�.
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
        /// ��ü�� Ȱ��ȭ�Ǹ�(���� ��� �ٽ� ������ ��) �ʱ� ü�� ������ �����մϴ�.
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
        /// ��Ȱ��ȭ�ϸ� ������ �ı��� ����Ǵ� ���� �����մϴ�.
        /// </summary>
        protected virtual void OnDisable()
        {
            CancelInvoke();
        }

        #endregion

        /// <summary>
        /// �� Health ���� ��Ұ� �̹� �����ӿ� �ջ�� �� ������ true�� ��ȯ�ϰ�, �׷��� ������ false�� ��ȯ�մϴ�.
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

            // �̹� 0���� ������ �ƹ��͵� ���� �ʰ� �����մϴ�.
            if ((CurrentHealth <= 0) && (InitialHealth != 0))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// ��ü�� �ջ��� ���� �� ȣ��˴ϴ�
        /// </summary>
        /// <param name="damage">�սǵ� �ǰ� ����Ʈ�� ���Դϴ�.</param>
        /// <param name="instigator">���ظ� ���� ��ü�Դϴ�.</param>
        /// <param name="flickerDuration">�ջ��� ���� �� ��ü�� ���ڿ��� �ϴ� �ð�(��) - �� �̻� ������ �ʰ� retrocompatibility�� ������ �ʵ��� �����˴ϴ�</param>
        /// <param name="invincibilityDuration">Ÿ�� �� ª�� ������ ���� �ð�.</param>
        public virtual void Damage(float damage, GameObject instigator, float flickerDuration, float invincibilityDuration, Vector3 damageDirection, List<TypedDamage> typedDamages = null)
        {
            Debug.Log($"[{this.gameObject.name}] �� [{damage}] ������ ����");
            if (!CanTakeDamageThisFrame())
            {
                return;
            }

            damage = ComputeDamageOutput(damage, typedDamages, true);

            // �츮�� ���ط� ĳ������ �ǰ��� ����߸��ϴ�
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

            // ĳ���Ͱ� �߻�ü, �÷��̾� �� ���� �浹�ϴ� ���� �����մϴ�
            if (invincibilityDuration > 0)
            {
                DamageDisabled();
                StartCoroutine(DamageEnabled(invincibilityDuration));
            }

            // �츮�� ���ظ� ���� ����� �����մϴ�
            MMDamageTakenEvent.Trigger(this, instigator, CurrentHealth, damage, previousHealth);

            // �츮�� �ִϸ����͸� ������Ʈ�մϴ�
            if (TargetAnimator != null)
            {
                TargetAnimator.SetTrigger("Damage");
            }

            // �츮�� �ǵ���� ����մϴ�
            if (FeedbackIsProportionalToDamage)
            {
                DamageMMFeedbacks?.PlayFeedbacks(this.transform.position, damage);
            }
            else
            {
                DamageMMFeedbacks?.PlayFeedbacks(this.transform.position);
            }

            // ���� ǥ������ ������Ʈ�մϴ�
            UpdateHealthBar(true);

            // �츮�� ��� ���� ���� ������ ó���մϴ�
            ComputeCharacterConditionStateChanges(typedDamages);
            ComputeCharacterMovementMultipliers(typedDamages);

            // �ǰ��� 0�� �����ϸ� �ǰ��� 0���� �����մϴ� (useful for the healthbar)
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
                    //�ڱ� �ڽ��� �±׸�Ȯ���ؼ� �����̺�Ʈ Ʈ���� �ϴ� �Լ� ��������
                    TagCheckEventOccurs();
                    CurrentHealth = 0;
                    Kill();
                }

            }
        }

        //�ڱ��ڽ� �±�Ȯ���� �̺�Ʈ Ʈ������
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
        /// ������ ������� �ð��� ������ ���� ��� ���ظ� �����մϴ�. //�ð��� ������ ���� ��� ���� = �������� ��, �������ظ� �����ٴ� ����
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
        /// ������ �� ���� ���ر��� �����Ͽ� �ð��� ������ ���� ��� ���ظ� �����մϴ�(���� ��� ��). //�ð��� ������ ���� ��� ���� = �������� ��, �������ظ� �����ٴ� ����
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
        /// ������ ������ �ð��� ������ ���� ��� ���ظ� �����մϴ�. //�ð��� ������ ���� ��� ���� = �������� ��, �������ظ� �����ٴ� ����
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
        /// ������ �ݺ� Ƚ�� ���� �ð��� ������ ���� �ջ��� �����մϴ�(ù ��° �ջ� ������ �����Ͽ� �˻�⿡�� ������ �������� ���� ����� �� ���� ������ �� ����).
        /// ���������� ���ظ� �ߴ��� �� �ִ��� ������ �� �ֽ��ϴ�. �� ��� InterruptAllDamageOverTime()�� ȣ���ϸ� �̷��� ������ �ߴܵǸ�, ���� ��� ���� ġ���ϴ� �� �����մϴ�.
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
        /// �ð��� ������ ���� �ջ��� ���ϴ� �� ���Ǵ� �ڷ�ƾ
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
        /// ������ ������ ó���� �� �� ü���� �Ծ�� �ϴ� ���ظ� ��ȯ�մϴ�.
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
            // �츮�� �������� ������ ���� ���ظ� ó���մϴ�.
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
        /// ������ ����ϰ� �ʿ��� ��� ���� ���� ������ �����մϴ�.
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
        /// ���� ����� ���캸�� �ʿ��� ��� �̵� �¼��� �����մϴ�.
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
        /// ������ ���� ó���Ͽ� ���ο� �˹� ���� �����մϴ�.
        /// </summary>
        /// <param name="knockbackForce"></param>
        /// <param name="typedDamages"></param>
        /// <returns></returns>
        public virtual Vector3 ComputeKnockbackForce(Vector3 knockbackForce, List<TypedDamage> typedDamages = null)
        {
            return (TargetDamageResistanceProcessor == null) ? knockbackForce : TargetDamageResistanceProcessor.ProcessKnockbackForce(knockbackForce, typedDamages); ;

        }

        /// <summary>
        /// �� ü���� �з��� �� ������ true�� ��ȯ�ϰ�, �׷��� ������ false�� ��ȯ�մϴ�.
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
        /// ĳ���͸� ���̰�, ��� ȿ���� �ν��Ͻ�ȭ�ϰ�, ����Ʈ�� ó���ϴ� ���� �۾��� �����մϴ�.
        /// </summary>
        public virtual void Kill()
        {
            if (ImmuneToDamage)
            {
                return;
            }

            if (_character != null)
            {
                // �츮�� ���� ���¸� true�� �����߽��ϴ�
                _character.ConditionState.ChangeState(CharacterStates.CharacterConditions.Dead);
                _character.Reset();

                if (_character.CharacterType == Character.CharacterTypes.Player)
                {
                    TopDownEngineEvent.Trigger(TopDownEngineEventTypes.PlayerDeath, _character);
                }
            }
            SetHealth(0);

            // �߰� ���ظ� �����ϰڽ��ϴ�
            StopAllDamageOverTime();
            DamageDisabled();

            DeathMMFeedbacks?.PlayFeedbacks(this.transform.position);

            // �ʿ��� ��� ����Ʈ�� �߰��մϴ�.
            if (PointsWhenDestroyed != 0)
            {
                // GameManager�� ������ �� �ִ� ���ο� ����Ʈ �̺�Ʈ(�� �̸� ������ �� �ִ� �ٸ� Ŭ����)�� �����ϴ�.
                TopDownEnginePointEvent.Trigger(PointsMethods.Add, PointsWhenDestroyed);
            }

            if (TargetAnimator != null)
            {
                TargetAnimator.SetTrigger("Death");
            }
            // �������� �浹�� �����ϵ��� ����ϴ�.
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

                // ��Ʈ�ѷ��� �ִ� ��� �浹�� �����ϰ� �������� ��Ȱ�� ���� �Ű������� �����ϰ� ������ ���� �����մϴ�.
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
                // ��ħ�� �츮�� �� ��ü�� �ı��Ѵ�
                if (_brain != null)
                    _brain.BrainActive = false;
                DestroyObject();
            }
        }

        /// <summary>
        /// �� ��ü�� �ǻ츮����.
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
        /// ĳ������ ������ ���� ��ü�� �ı��ϰų� �ı��Ϸ��� �õ��մϴ�.
        /// </summary>
        protected virtual void DestroyObject()
        {
            if (_autoRespawn == null)
            {
                if (DestroyOnDeath)
                {
                    //������Ʈ ȸ��
                    if (thisLayer == 13 || thisLayer == 14)//13 - ���ʹ�, 14 - ���߿��ʹ�
                        gameObject.SetActive(false);
                }
            }
            else
            {
                _autoRespawn.Kill();
            }
        }

        //�ڽ��� �±׸� Ȯ���� �˸´� ȸ���Լ� ȣ��
        void TagCheckReturnPool()
        {
            switch (thisTag)
            {
                case "Slime": CreateManager.Instance.ReturnPool(this.gameObject.GetComponent<Slime>()); Debug.Log("������ ȸ����"); break;
            }
        }

        #region HealthManipulationAPIs


        /// <summary>
        /// ���� ���¸� ������ �� ������ �����ϰ� ���� ǥ������ ������Ʈ�մϴ�
        /// </summary>
        /// <param name="newValue"></param>
        public virtual void SetHealth(float newValue)
        {
            CurrentHealth = newValue;
            UpdateHealthBar(false);
            HealthChangeEvent.Trigger(this, newValue);
        }

        /// <summary>
        /// ĳ���Ͱ� ü���� ���� �� ȣ��˴ϴ�(��: a stimpack for example����).
        /// </summary>
        /// <param name="health">ĳ���Ͱ� ��� �ǰ�.</param>
        /// <param name="instigator">ĳ���Ϳ��� �ǰ��� �ִ� ��.</param>
        public virtual void ReceiveHealth(float health, GameObject instigator)
        {
            // �� ����� ĳ������ ü�¿� ü���� �߰��ϰ� �ִ� ü��(MaxHealth)�� �ʰ��ϴ� ���� �����մϴ�.
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
        /// ĳ������ ü���� �ִ밪���� �缳���մϴ�.
        /// </summary>
        public virtual void ResetHealthToMaxHealth()
        {
            SetHealth(MaximumHealth);
        }

        /// <summary>
        /// ĳ������ ���� ǥ������ ������ ���� ��Ĩ�ϴ�
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

        //�ִ�ü���� �����մϴ�.
        public virtual void UpdateMaxHealth(float healthValue)
        {
            MaximumHealth += healthValue;
            CurrentHealth += healthValue;

            UpdateHealthBar(true);
        }

        //�ִ�ü��, ����ü���� �ʱ�ȭ �մϴ�.
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
        /// ĳ���Ͱ� ���ظ� ���� �ʵ��� ����
        /// </summary>
        public virtual void DamageDisabled()
        {
            Invulnerable = true;
        }

        //ĳ���Ͱ� �Ķ����(��) ���� ���ظ� ���� �ʽ��ϴ�. �Ķ����(��)���Ŀ��� �ٽ� ���ظ� �Խ��ϴ�.
        public virtual void DamageDisabled(float invincibilityTime_)
        {
            Invulnerable = true;
            StartCoroutine(DamageEnabled(invincibilityTime_));
        }

        /// <summary>
        /// ĳ���Ͱ� ���ظ� ���� �� �ְ� ���ݴϴ�.
        /// </summary>
        public virtual void DamageEnabled()
        {
            Invulnerable = false;
        }

        /// <summary>
        /// ĳ���Ͱ� ������ ���� �Ŀ� �ٽ� ���ظ� ���� �� �ֵ��� �մϴ�.
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