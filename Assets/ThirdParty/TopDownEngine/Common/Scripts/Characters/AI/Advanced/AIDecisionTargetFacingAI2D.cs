using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// Brain의 현재 대상이 이 캐릭터를 향하고 있는 경우 이 결정은 true로 반환됩니다. 예, Ghosts에만 적용됩니다. 하지만 이제 당신도 사용할 수 있습니다!
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Decisions/AIDecisionTargetFacingAI2D")]
	public class AIDecisionTargetFacingAI2D : AIDecision
	{
		protected CharacterOrientation2D _orientation2D;
        
		/// <summary>
		/// On Decide we check whether the Target is facing us
		/// </summary>
		/// <returns></returns>
		public override bool Decide()
		{
			return EvaluateTargetFacingDirection();
		}

		/// <summary>
		/// Returns true if the Brain's Target is facing us (this will require that the Target has a Character component)
		/// </summary>
		/// <returns></returns>
		protected virtual bool EvaluateTargetFacingDirection()
		{
			if (_brain.Target == null)
			{
				return false;
			}

			_orientation2D = _brain.Target.gameObject.GetComponent<Character>()?.FindAbility<CharacterOrientation2D>();
			if (_orientation2D != null)
			{
				if (_orientation2D.IsFacingRight && (this.transform.position.x > _orientation2D.transform.position.x))
				{
					return true;
				}
				if (!_orientation2D.IsFacingRight && (this.transform.position.x < _orientation2D.transform.position.x))
				{
					return true;
				}
			}            

			return false;
		}
	}
}