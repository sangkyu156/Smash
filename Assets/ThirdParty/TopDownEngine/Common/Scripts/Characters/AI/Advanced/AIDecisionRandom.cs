using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 결정은 주사위를 굴리고 결과가 확률 값보다 작거나 같으면 true를 반환합니다.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Decisions/AIDecisionRandom")]
	public class AIDecisionRandom : AIDecision
	{
		[Header("Random")]
        /// 고려해야 할 총 개수('10개 중 5개'에서는 10개임)
        [Tooltip("고려해야 할 총 개수('10개 중 5개'에서는 10개임)")]
		public int TotalChance = 10;
        /// 주사위를 굴릴 때 결과가 확률보다 낮으면 이 결정이 적용됩니다. '10점 만점에 5점'이면 5점입니다.
        [Tooltip("주사위를 굴릴 때 결과가 확률보다 낮으면 이 결정이 적용됩니다. '10점 만점에 5점'이면 5점입니다.")]
		public int Odds = 4;

		protected Character _targetCharacter;

		/// <summary>
		/// On Decide we check if the odds are in our favour
		/// </summary>
		/// <returns></returns>
		public override bool Decide()
		{
			return EvaluateOdds();
		}

		/// <summary>
		/// Returns true if the Brain's Target is facing us (this will require that the Target has a Character component)
		/// </summary>
		/// <returns></returns>
		protected virtual bool EvaluateOdds()
		{
			int dice = MMMaths.RollADice(TotalChance);
			bool result = (dice <= Odds);
			return result;
		}
	}
}