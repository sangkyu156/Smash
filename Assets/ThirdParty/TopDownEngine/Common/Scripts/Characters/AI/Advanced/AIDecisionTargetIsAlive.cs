using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 결정은 뇌의 현재 대상이 살아 있으면 true를 반환하고, 그렇지 않으면 false를 반환합니다.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Decisions/AIDecisionTargetIsAlive")]
	public class AIDecisionTargetIsAlive : AIDecision
	{
		protected Character _character;
        
		/// <summary>
		/// On Decide we check whether the Target is alive or dead
		/// </summary>
		/// <returns></returns>
		public override bool Decide()
		{
			return CheckIfTargetIsAlive();
		}

		/// <summary>
		/// Returns true if the Brain's Target is alive, false otherwise
		/// </summary>
		/// <returns></returns>
		protected virtual bool CheckIfTargetIsAlive()
		{
			if (_brain.Target == null)
			{
				return false;
			}

			_character = _brain.Target.gameObject.MMGetComponentNoAlloc<Character>();
			if (_character != null)
			{
				if (_character.ConditionState.CurrentState == CharacterStates.CharacterConditions.Dead)
				{
					return false;
				}
				else
				{ 
					return true;
				}
			}            

			return false;
		}
	}
}