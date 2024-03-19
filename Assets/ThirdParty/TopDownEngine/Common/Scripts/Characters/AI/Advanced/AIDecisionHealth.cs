using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 지정된 상태 조건이 충족되면 이 결정은 true를 반환합니다. 지정된 값보다 낮거나, 엄격하게 낮거나, 같거나, 높거나, 엄격하게 높게 설정할 수 있습니다.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Decisions/AIDecisionHealth")]
	//[RequireComponent(typeof(Health))]
	public class AIDecisionHealth : AIDecision
	{
		/// the different comparison modes
		public enum ComparisonModes { StrictlyLowerThan, LowerThan, Equals, GreaterThan, StrictlyGreaterThan }
        /// HealthValue를 평가할 비교 모드
        [Tooltip("HealthValue를 평가할 비교 모드")]
		public ComparisonModes TrueIfHealthIs;
        /// 비교할 건강 값
        [Tooltip("비교할 건강 값")]
		public int HealthValue;
        /// 이 비교를 한 번만 수행할지 여부
        [Tooltip("이 비교를 한 번만 수행할지 여부")]
		public bool OnlyOnce = true;

		protected Health _health;
		protected bool _once = false;

		/// <summary>
		/// On init we grab our Health component
		/// </summary>
		public override void Initialization()
		{
			_health = _brain.gameObject.GetComponentInParent<Health>();
		}

		/// <summary>
		/// On Decide we evaluate our current Health level
		/// </summary>
		/// <returns></returns>
		public override bool Decide()
		{
			return EvaluateHealth();
		}

		/// <summary>
		/// Compares our health value and returns true if the condition is met
		/// </summary>
		/// <returns></returns>
		protected virtual bool EvaluateHealth()
		{
			bool returnValue = false;

			if (OnlyOnce && _once)
			{
				return false;
			}

			if (_health == null)
			{
				Debug.LogWarning("You've added an AIDecisionHealth to " + this.gameObject.name + "'s AI Brain, but this object doesn't have a Health component.");
				return false;
			}

			if (!_health.isActiveAndEnabled)
			{
				return false;
			}
            
			if (TrueIfHealthIs == ComparisonModes.StrictlyLowerThan)
			{
				returnValue = (_health.CurrentHealth < HealthValue);
			}

			if (TrueIfHealthIs == ComparisonModes.LowerThan)
			{
				returnValue = (_health.CurrentHealth <= HealthValue);
			}

			if (TrueIfHealthIs == ComparisonModes.Equals)
			{
				returnValue = (_health.CurrentHealth == HealthValue);
			}

			if (TrueIfHealthIs == ComparisonModes.GreaterThan)
			{
				returnValue = (_health.CurrentHealth >= HealthValue);
			}

			if (TrueIfHealthIs == ComparisonModes.StrictlyGreaterThan)
			{
				returnValue = (_health.CurrentHealth > HealthValue);
			}

			if (returnValue)
			{
				_once = true;
				return true;
			}
			else
			{
				return false;
			}
		}
	}
}