using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 대상을 null로 설정하고 재설정하는 작업
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Actions/AIActionResetTarget")]
	public class AIActionResetTarget : AIAction
	{
		/// <summary>
		/// we reset our target
		/// </summary>
		public override void PerformAction()
		{
			_brain.Target = null;
		}
	}
}