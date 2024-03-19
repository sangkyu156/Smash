using System;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// DamageResistanceProcessor에서 사용되는 이 클래스는 특정 유형의 손상에 대한 저항을 정의합니다.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/Health/Damage Resistance")]
	public class DamageResistance : TopDownMonoBehaviour
	{
		public enum DamageModifierModes { Multiplier, Flat }
		public enum KnockbackModifierModes { Multiplier, Flat }

		[Header("General")]
		/// The priority of this damage resistance. This will be used to determine in what order damage resistances should be evaluated. Lowest priority means evaluated first.
		[Tooltip("이 손상 저항의 우선순위입니다. 이는 손상 저항을 어떤 순서로 평가해야 하는지 결정하는 데 사용됩니다. 우선순위가 가장 낮다는 것은 먼저 평가된다는 의미입니다.")]
		public float Priority = 0;
		/// The label of this damage resistance. Used for organization, and to activate/disactivate a resistance by its label.
		[Tooltip("이 손상 저항의 라벨입니다. 조직화에 사용되며 라벨에 따라 저항을 활성화/비활성화하는 데 사용됩니다.")]
		public string Label = "";
		
		[Header("Damage Resistance Settings")]
		/// Whether this resistance impacts base damage or typed damage
		[Tooltip("이 저항이 기본 피해 또는 유형 피해에 영향을 미치는지 여부")]
		public DamageTypeModes DamageTypeMode = DamageTypeModes.BaseDamage;
		/// In TypedDamage mode, the type of damage this resistance will interact with
		[Tooltip("TypedDamage 모드에서 이 저항이 상호 작용하는 손상 유형")]
		[MMEnumCondition("DamageTypeMode", (int)DamageTypeModes.TypedDamage)]
		public DamageType TypeResistance;
		/// the way to reduce (or increase) received damage. Multiplier will multiply incoming damage by a multiplier, flat will subtract a constant value from incoming damage. 
		[Tooltip("받는 데미지를 감소(또는 증가)시키는 방법. 승수는 들어오는 피해에 승수를 곱하고, 플랫은 들어오는 피해에서 일정한 값을 뺍니다")]
		public DamageModifierModes DamageModifierMode = DamageModifierModes.Multiplier;

		[Header("Damage Modifiers")]
		/// In multiplier mode, the multiplier to apply to incoming damage. 0.5 will reduce it in half, while a value of 2 will create a weakness to the specified damage type, and damages will double.
		[Tooltip("승수 모드에서 들어오는 피해에 적용할 승수입니다. 0.5는 절반으로 감소하고, 2의 값은 지정된 손상 유형에 대한 약점을 생성하며 손상은 두 배로 증가합니다.")]
		[MMEnumCondition("DamageModifierMode", (int)DamageModifierModes.Multiplier)]
		public float DamageMultiplier = 0.25f;
		/// In flat mode, the amount of damage to subtract every time that type of damage is received
		[Tooltip("플랫 모드에서 해당 유형의 피해를 받을 때마다 차감되는 피해량")]
		[MMEnumCondition("DamageModifierMode", (int)DamageModifierModes.Flat)]
		public float FlatDamageReduction = 10f;
		/// whether or not incoming damage of the specified type should be clamped between a min and a max
		[Tooltip("지정된 유형의 들어오는 손상을 최소와 최대 사이에 고정해야 하는지 여부")] 
		public bool ClampDamage = false;
		/// the values between which to clamp incoming damage
		[Tooltip("들어오는 피해를 제한하는 값")]
		[MMVector("Min","Max")]
		public Vector2 DamageModifierClamps = new Vector2(0f,10f);

		[Header("Condition Change")]
		/// whether or not condition change for that type of damage is allowed or not
		[Tooltip("해당 유형의 손상에 대한 조건 변경이 허용되는지 여부")]
		public bool PreventCharacterConditionChange = false;
		/// whether or not movement modifiers are allowed for that type of damage or not
		[Tooltip("해당 유형의 손상에 대해 이동 수정자가 허용되는지 여부")]
		public bool PreventMovementModifier = false;
		
		[Header("Knockback")] 
		/// if this is true, knockback force will be ignored and not applied
		[Tooltip("이것이 사실이라면 밀어내는 힘은 무시되고 적용되지 않습니다.")]
		public bool ImmuneToKnockback = false;
		/// the way to reduce (or increase) received knockback. Multiplier will multiply incoming knockback intensity by a multiplier, flat will subtract a constant value from incoming knockback intensity. 
		[Tooltip("받는 넉백을 감소(또는 증가)시키는 방법. 승수는 들어오는 넉백 강도에 승수를 곱하고, 플랫은 들어오는 넉백 강도에서 상수 값을 뺍니다.")]
		public KnockbackModifierModes KnockbackModifierMode = KnockbackModifierModes.Multiplier;
		/// In multiplier mode, the multiplier to apply to incoming knockback. 0.5 will reduce it in half, while a value of 2 will create a weakness to the specified damage type, and knockback intensity will double.
		[Tooltip("승수 모드에서 들어오는 넉백에 적용할 승수입니다. 0.5는 절반으로 감소하고, 2의 값은 지정된 피해 유형에 대한 약점을 생성하며 밀쳐내기 강도는 두 배로 증가합니다.")]
		[MMEnumCondition("KnockbackModifierMode", (int)DamageModifierModes.Multiplier)]
		public float KnockbackMultiplier = 1f;
		/// In flat mode, the amount of knockback to subtract every time that type of damage is received
		[Tooltip("플랫 모드에서 해당 유형의 피해를 받을 때마다 빼야 하는 넉백의 양입니다.")]
		[MMEnumCondition("KnockbackModifierMode", (int)DamageModifierModes.Flat)]
		public float FlatKnockbackMagnitudeReduction = 10f;
		/// whether or not incoming knockback of the specified type should be clamped between a min and a max
		[Tooltip("지정된 유형의 들어오는 넉백을 최소와 최대 사이에 고정해야 하는지 여부")] 
		public bool ClampKnockback = false;
		/// the values between which to clamp incoming knockback magnitude
		[Tooltip("들어오는 넉백 크기를 고정하는 값 사이")]
		[MMCondition("ClampKnockback", true)]
		public float KnockbackMaxMagnitude = 10f;

		[Header("Feedbacks")]
		/// This feedback will only be triggered if damage of the matching type is received
		[Tooltip("이 피드백은 일치하는 유형의 손상을 받은 경우에만 트리거됩니다.")]
		public MMFeedbacks OnDamageReceived;
		/// whether or not this feedback can be interrupted (stopped) when that type of damage is interrupted
		[Tooltip("해당 유형의 손상이 중단될 때 이 피드백이 중단(중지)될 수 있는지 여부")]
		public bool InterruptibleFeedback = false;
		/// if this is true, the feedback will always be preventively stopped before playing
		[Tooltip("이것이 사실이라면 재생 전에 피드백이 항상 예방적으로 중지됩니다.")]
		public bool AlwaysInterruptFeedbackBeforePlay = false;
		/// whether this feedback should play if damage received is zero
		[Tooltip("받은 피해가 0인 경우 이 피드백을 재생할지 여부")]
		public bool TriggerFeedbackIfDamageIsZero = false;

		/// <summary>
		/// On awake we initialize our feedback
		/// </summary>
		protected virtual void Awake()
		{
			OnDamageReceived?.Initialization(this.gameObject);
		}
		
		/// <summary>
		/// When getting damage, goes through damage reduction and outputs the resulting damage
		/// </summary>
		/// <param name="damage"></param>
		/// <param name="type"></param>
		/// <param name="damageApplied"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public virtual float ProcessDamage(float damage, DamageType type, bool damageApplied)
		{
			if (!this.gameObject.activeInHierarchy)
			{
				return damage;
			}
			
			if ((type == null) && (DamageTypeMode != DamageTypeModes.BaseDamage))
			{
				return damage;
			}

			if ((type != null) && (DamageTypeMode == DamageTypeModes.BaseDamage))
			{
				return damage;
			}

			if ((type != null) && (type != TypeResistance))
			{
				return damage;
			}
			
			// applies damage modifier or reduction
			switch (DamageModifierMode)
			{
				case DamageModifierModes.Multiplier:
					damage = damage * DamageMultiplier;
					break;
				case DamageModifierModes.Flat:
					damage = damage - FlatDamageReduction;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
			
			// clamps damage
			damage = ClampDamage ? Mathf.Clamp(damage, DamageModifierClamps.x, DamageModifierClamps.y) : damage;

			if (damageApplied)
			{
				if (!TriggerFeedbackIfDamageIsZero && (damage == 0))
				{
					// do nothing
				}
				else
				{
					if (AlwaysInterruptFeedbackBeforePlay)
					{
						OnDamageReceived?.StopFeedbacks();
					}
					OnDamageReceived?.PlayFeedbacks(this.transform.position);	
				}
			}

			return damage;
		}
		
		/// <summary>
		/// Processes the knockback input value and returns it potentially modified by damage resistances
		/// </summary>
		/// <param name="damage"></param>
		/// <param name="type"></param>
		/// <param name="damageApplied"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public virtual Vector3 ProcessKnockback(Vector3 knockback, DamageType type)
		{
			if (!this.gameObject.activeInHierarchy)
			{
				return knockback;
			}

			if ((type == null) && (DamageTypeMode != DamageTypeModes.BaseDamage))
			{
				return knockback;
			}

			if ((type != null) && (DamageTypeMode == DamageTypeModes.BaseDamage))
			{
				return knockback;
			}

			if ((type != null) && (type != TypeResistance))
			{
				return knockback;
			}

			// applies damage modifier or reduction
			switch (KnockbackModifierMode)
			{
				case KnockbackModifierModes.Multiplier:
					knockback = knockback * KnockbackMultiplier;
					break;
				case KnockbackModifierModes.Flat:
					float magnitudeReduction = Mathf.Clamp(Mathf.Abs(knockback.magnitude) - FlatKnockbackMagnitudeReduction, 0f, Single.MaxValue);
					knockback = knockback.normalized * magnitudeReduction * Mathf.Sign(knockback.magnitude);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			// clamps damage
			knockback = ClampKnockback ? Vector3.ClampMagnitude(knockback, KnockbackMaxMagnitude) : knockback;

			return knockback;
		}
	}
}
