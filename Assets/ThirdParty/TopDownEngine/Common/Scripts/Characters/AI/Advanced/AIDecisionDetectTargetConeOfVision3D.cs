using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 결정은 MMConeOfVision이 하나 이상의 대상을 감지한 경우 true를 반환하고 이를 Brain의 대상으로 설정합니다.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Decisions/AIDecisionDetectTargetConeOfVision3D")]
	public class AIDecisionDetectTargetConeOfVision3D : AIDecision
	{
        /// 이는 사실입니다. 이 결정은 대상을 찾을 수 없는 경우 AI Brain의 대상을 null로 설정합니다.
        [Tooltip("if이는 사실입니다. 이 결정은 대상을 찾을 수 없는 경우 AI Brain의 대상을 null로 설정합니다.")]
		public bool SetTargetToNullIfNoneIsFound = true;

		public MMConeOfVision TargetConeOfVision;

		/// <summary>
		/// On Init we grab our MMConeOfVision
		/// </summary>
		public override void Initialization()
		{
			base.Initialization();
			if (TargetConeOfVision == null)
			{
				TargetConeOfVision = this.gameObject.GetComponent<MMConeOfVision>();    
			}
		}

		/// <summary>
		/// On Decide we look for a target
		/// </summary>
		/// <returns></returns>
		public override bool Decide()
		{
			return DetectTarget();
		}

		/// <summary>
		/// If the MMConeOfVision has at least one target, it becomes our new brain target and this decision is true, otherwise it's false.
		/// </summary>
		/// <returns></returns>
		protected virtual bool DetectTarget()
		{
			if (TargetConeOfVision.VisibleTargets.Count == 0)
			{
				if (SetTargetToNullIfNoneIsFound) { _brain.Target = null; }               
				return false;
			}
			else
			{
				_brain.Target = TargetConeOfVision.VisibleTargets[0];
				return true;
			}
		}
	}
}