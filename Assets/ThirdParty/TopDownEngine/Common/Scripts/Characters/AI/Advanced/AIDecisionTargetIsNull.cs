using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 결정은 Brain의 현재 목표가 null이면 true를 반환하고, 그렇지 않으면 false를 반환합니다.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Decisions/AIDecisionTargetIsNull")]
	public class AIDecisionTargetIsNull : AIDecision
	{        
		/// <summary>
		/// On Decide we check whether the Target is null
		/// </summary>
		/// <returns></returns>
		public override bool Decide()
		{
			return CheckIfTargetIsNull();
		}

		/// <summary>
		/// Returns true if the Brain's Target is null
		/// </summary>
		/// <returns></returns>
		protected virtual bool CheckIfTargetIsNull()
		{
			if (_brain.Target == null)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
	}
}