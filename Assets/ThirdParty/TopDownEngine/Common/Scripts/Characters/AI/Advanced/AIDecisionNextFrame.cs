using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 결정은 이 결정이 설정된 상태에 들어갈 때 true를 반환합니다.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Decisions/AIDecisionNextFrame")]
	public class AIDecisionNextFrame : AIDecision
	{
		/// <summary>
		/// We return true on Decide
		/// </summary>
		/// <returns></returns>
		public override bool Decide()
		{
			return true;
		}
	}
}