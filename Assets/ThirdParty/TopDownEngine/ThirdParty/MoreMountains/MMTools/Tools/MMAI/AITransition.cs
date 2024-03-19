using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 전환은 이러한 전환이 참인지 거짓인지에 관계없이 하나 이상의 결정과 대상 상태의 조합입니다. 전환의 예로는 "_적이 범위 내에 들어오면 사격 상태로 전환_"이 있습니다.
    /// </summary>
    [System.Serializable]
	public class AITransition 
	{
		/// this transition's decision
		public AIDecision Decision;
		/// the state to transition to if this Decision returns true
		public string TrueState;
		/// the state to transition to if this Decision returns false
		public string FalseState;
	}
}