using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 결정은 MMConeOfVision이 하나 이상의 대상을 감지한 경우 true를 반환하고 이를 Brain의 대상으로 설정합니다.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Decisions/AIDecisionDetectTargetConeOfVision2D")]
	public class AIDecisionDetectTargetConeOfVision2D : AIDecision
	{
        /// 이것이 사실이라면 이 결정은 대상이 발견되지 않으면 AI Brain의 대상을 null로 설정합니다.
        [Tooltip("이것이 사실이라면 이 결정은 대상이 발견되지 않으면 AI Brain의 대상을 null로 설정합니다.")]
		public bool SetTargetToNullIfNoneIsFound = true;

		[Header("Bindings")]
        /// theVision 2D의 원뿔이 회전합니다.
        [Tooltip("Vision 2D의 원뿔이 회전합니다.")]
		public MMConeOfVision2D TargetConeOfVision2D;

		/// <summary>
		/// On Init we grab our MMConeOfVision
		/// </summary>
		public override void Initialization()
		{
			base.Initialization();
			if (TargetConeOfVision2D == null)
			{
				TargetConeOfVision2D = this.gameObject.GetComponent<MMConeOfVision2D>(); 
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
			if (TargetConeOfVision2D.VisibleTargets.Count == 0)
			{
				if (SetTargetToNullIfNoneIsFound)
				{
					_brain.Target = null;
				}

				return false;
			}
			else
			{
				_brain.Target = TargetConeOfVision2D.VisibleTargets[0];
				return true;
			}
		}
	}
}