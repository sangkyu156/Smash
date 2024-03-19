using System.Collections.Generic;
using System.Linq;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Serialization;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 구성요소를 Health 구성요소에 연결하면 저항, 피해 감소/증가 처리, 상태 변경, 이동 승수, 피드백 등을 통해 들어오는 피해를 처리할 수 있습니다.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/Health/DamageResistanceProcessor")]
	public class DamageResistanceProcessor : TopDownMonoBehaviour
	{
		[Header("Damage Resistance List")]
		
		/// If this is true, this component will try to auto-fill its list of damage resistances from the ones found in its children 
		[Tooltip("이것이 사실이라면 이 구성요소는 하위 항목에서 발견된 피해 저항 목록을 자동으로 채우려고 시도합니다.")]
		public bool AutoFillDamageResistanceList = true;
		/// If this is true, disabled resistances will be ignored by the auto fill 
		[Tooltip("이것이 사실이라면 비활성화된 저항은 자동 채우기에 의해 무시됩니다.\r\n")]
		public bool IgnoreDisabledResistances = true;
		/// If this is true, damage from damage types that this processor has no resistance for will be ignored
		[Tooltip("이것이 사실이라면 이 프로세서에 저항이 없는 손상 유형으로 인한 손상은 무시됩니다.")]
		public bool IgnoreUnknownDamageTypes = false;
		
		/// the list of damage resistances this processor will handle. Auto filled if AutoFillDamageResistanceList is true
		[FormerlySerializedAs("DamageResitanceList")] 
		[Tooltip("이 프로세서가 처리할 손상 저항 목록입니다. AutoFillDamageResistanceList가 true이면 자동으로 채워집니다.")]
		public List<DamageResistance> DamageResistanceList;

		/// <summary>
		/// On awake we initialize our processor
		/// </summary>
		protected virtual void Awake()
		{
			Initialization();
		}

		/// <summary>
		/// Auto finds resistances if needed and sorts them
		/// </summary>
		protected virtual void Initialization()
		{
			if (AutoFillDamageResistanceList)
			{
				DamageResistance[] foundResistances =
					this.gameObject.GetComponentsInChildren<DamageResistance>(
						includeInactive: !IgnoreDisabledResistances);
				if (foundResistances.Length > 0)
				{
					DamageResistanceList = foundResistances.ToList();	
				}
			}
			SortDamageResistanceList();
		}

		/// <summary>
		/// A method used to reorder the list of resistances, based on priority by default.
		/// Don't hesitate to override this method if you'd like your resistances to be handled in a different order
		/// </summary>
		public virtual void SortDamageResistanceList()
		{
			// we sort the list by priority
			DamageResistanceList.Sort((p1,p2)=>p1.Priority.CompareTo(p2.Priority));
		}
		
		/// <summary>
		/// Processes incoming damage through the list of resistances, and outputs the final damage value
		/// </summary>
		/// <param name="damage"></param>
		/// <param name="typedDamages"></param>
		/// <param name="damageApplied"></param>
		/// <returns></returns>
		public virtual float ProcessDamage(float damage, List<TypedDamage> typedDamages, bool damageApplied)
		{
			float totalDamage = 0f;
			if (DamageResistanceList.Count == 0) // if we don't have resistances, we output raw damage
			{
				totalDamage = damage;
				if (typedDamages != null)
				{
					foreach (TypedDamage typedDamage in typedDamages)
					{
						totalDamage += typedDamage.DamageCaused;
					}
				}
				if (IgnoreUnknownDamageTypes)
				{
					totalDamage = damage;
				}
				return totalDamage;
			}
			else // if we do have resistances
			{
				totalDamage = damage;
				
				foreach (DamageResistance resistance in DamageResistanceList)
				{
					totalDamage = resistance.ProcessDamage(totalDamage, null, damageApplied);
				}

				if (typedDamages != null)
				{
					foreach (TypedDamage typedDamage in typedDamages)
					{
						float currentDamage = typedDamage.DamageCaused;
						
						bool atLeastOneResistanceFound = false;
						foreach (DamageResistance resistance in DamageResistanceList)
						{
							if (resistance.TypeResistance == typedDamage.AssociatedDamageType)
							{
								atLeastOneResistanceFound = true;
							}
							currentDamage = resistance.ProcessDamage(currentDamage, typedDamage.AssociatedDamageType, damageApplied);
						}
						if (IgnoreUnknownDamageTypes && !atLeastOneResistanceFound)
						{
							// we don't add to the total
						}
						else
						{
							totalDamage += currentDamage;	
						}
						
					}
				}
				
				return totalDamage;
			}
		}

		public virtual void SetResistanceByLabel(string searchedLabel, bool active)
		{
			foreach (DamageResistance resistance in DamageResistanceList)
			{
				if (resistance.Label == searchedLabel)
				{
					resistance.gameObject.SetActive(active);
				}
			}
		}

		/// <summary>
		/// When interrupting all damage over time of the specified type, stops their associated feedbacks if needed
		/// </summary>
		/// <param name="damageType"></param>
		public virtual void InterruptDamageOverTime(DamageType damageType)
		{
			foreach (DamageResistance resistance in DamageResistanceList)
			{
				if ( resistance.gameObject.activeInHierarchy &&
					((resistance.DamageTypeMode == DamageTypeModes.BaseDamage) ||
				        (resistance.TypeResistance == damageType))
				    && resistance.InterruptibleFeedback)
				{
					resistance.OnDamageReceived?.StopFeedbacks();
				}
			}
		}

		/// <summary>
		/// Checks if any of the resistances prevents the character from changing condition, and returns true if that's the case, false otherwise
		/// </summary>
		/// <param name="typedDamage"></param>
		/// <returns></returns>
		public virtual bool CheckPreventCharacterConditionChange(DamageType typedDamage)
		{
			foreach (DamageResistance resistance in DamageResistanceList)
			{
				if (!resistance.gameObject.activeInHierarchy)
				{
					continue;
				}
				
				if (typedDamage == null)
				{
					if ((resistance.DamageTypeMode == DamageTypeModes.BaseDamage) &&
					    (resistance.PreventCharacterConditionChange))
					{
						return true;	
					}
				}
				else
				{
					if ((resistance.TypeResistance == typedDamage) &&
					    (resistance.PreventCharacterConditionChange))
					{
						return true;
					}
				}
			}
			return false;
		}

		/// <summary>
		/// Checks if any of the resistances prevents the character from changing condition, and returns true if that's the case, false otherwise
		/// </summary>
		/// <param name="typedDamage"></param>
		/// <returns></returns>
		public virtual bool CheckPreventMovementModifier(DamageType typedDamage)
		{
			foreach (DamageResistance resistance in DamageResistanceList)
			{
				if (!resistance.gameObject.activeInHierarchy)
				{
					continue;
				}
				if (typedDamage == null)
				{
					if ((resistance.DamageTypeMode == DamageTypeModes.BaseDamage) &&
					    (resistance.PreventMovementModifier))
					{
						return true;	
					}
				}
				else
				{
					if ((resistance.TypeResistance == typedDamage) &&
					    (resistance.PreventMovementModifier))
					{
						return true;
					}
				}
			}
			return false;
		}
		
		/// <summary>
		/// Returns true if the resistances on this processor make it immune to knockback, false otherwise
		/// </summary>
		/// <param name="typedDamage"></param>
		/// <returns></returns>
		public virtual bool CheckPreventKnockback(List<TypedDamage> typedDamages)
		{
			if ((typedDamages == null) || (typedDamages.Count == 0))
			{
				foreach (DamageResistance resistance in DamageResistanceList)
				{
					if (!resistance.gameObject.activeInHierarchy)
					{
						continue;
					}

					if ((resistance.DamageTypeMode == DamageTypeModes.BaseDamage) &&
					    (resistance.ImmuneToKnockback))
					{
						return true;	
					}
				}
			}
			else
			{
				foreach (TypedDamage typedDamage in typedDamages)
				{
					foreach (DamageResistance resistance in DamageResistanceList)
					{
						if (!resistance.gameObject.activeInHierarchy)
						{
							continue;
						}

						if (typedDamage == null)
						{
							if ((resistance.DamageTypeMode == DamageTypeModes.BaseDamage) &&
							    (resistance.ImmuneToKnockback))
							{
								return true;	
							}
						}
						else
						{
							if ((resistance.TypeResistance == typedDamage.AssociatedDamageType) &&
							    (resistance.ImmuneToKnockback))
							{
								return true;
							}
						}
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Processes the input knockback force through the various resistances
		/// </summary>
		/// <param name="knockback"></param>
		/// <param name="typedDamages"></param>
		/// <returns></returns>
		public virtual Vector3 ProcessKnockbackForce(Vector3 knockback, List<TypedDamage> typedDamages)
		{
			if (DamageResistanceList.Count == 0) // if we don't have resistances, we output raw knockback value
			{
				return knockback;
			}
			else // if we do have resistances
			{
				foreach (DamageResistance resistance in DamageResistanceList)
				{
					knockback = resistance.ProcessKnockback(knockback, null);
				}

				if (typedDamages != null)
				{
					foreach (TypedDamage typedDamage in typedDamages)
					{
						foreach (DamageResistance resistance in DamageResistanceList)
						{
							if (IgnoreDisabledResistances && !resistance.isActiveAndEnabled)
							{
								continue;
							}
							knockback = resistance.ProcessKnockback(knockback, typedDamage.AssociatedDamageType);
						}
					}
				}

				return knockback;
			}
		}
	}
}