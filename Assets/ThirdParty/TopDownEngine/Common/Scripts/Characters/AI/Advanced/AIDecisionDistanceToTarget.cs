using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 결정은 현재 Brain 목표가 지정된 범위 내에 있으면 true를 반환하고, 그렇지 않으면 false를 반환합니다.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Decisions/AIDecisionDistanceToTarget")]
	public class AIDecisionDistanceToTarget : AIDecision
	{
        /// 가능한 비교 모드
        public enum ComparisonModes { StrictlyLowerThan, LowerThan, Equals, GreaterThan, StrictlyGreaterThan }//엄격히낮음, 보다 낮은, 같음, 보다 큼, 엄밀히 말하면 보다 큼
        /// 비교 모드
        [Tooltip("비교 모드")]
		public ComparisonModes ComparisonMode = ComparisonModes.GreaterThan;
        /// 비교할 거리
        [Tooltip("비교할 거리")]
		public float Distance;
        [Tooltip("무기 (등록해야함)")]
        public CharacterHandleWeapon TargetHandleWeaponAbility;

        /// <summary>
        /// Decide에서 목표까지의 거리를 확인합니다.
        /// </summary>
        /// <returns></returns>
        public override bool Decide()
		{
			if(EvaluateDistance() == false)
                TargetHandleWeaponAbility?.ForceStop();

            return EvaluateDistance();
		}

        /// <summary>
        /// 거리 조건이 충족되면 true를 반환합니다.
        /// </summary>
        /// <returns></returns>
        protected virtual bool EvaluateDistance()
		{
			if (_brain.Target == null)
			{
				return false;
			}

			float distance = Vector3.Distance(this.transform.position, _brain.Target.position);

			if (ComparisonMode == ComparisonModes.StrictlyLowerThan)
			{
				return (distance < Distance);
			}
			if (ComparisonMode == ComparisonModes.LowerThan)
			{
                return (distance <= Distance);
			}
			if (ComparisonMode == ComparisonModes.Equals)
			{
				return (distance == Distance);
			}
			if (ComparisonMode == ComparisonModes.GreaterThan)
			{
				return (distance >= Distance);
			}
			if (ComparisonMode == ComparisonModes.StrictlyGreaterThan)
			{
				return (distance > Distance);
			}

            return false;
		}
	}
}