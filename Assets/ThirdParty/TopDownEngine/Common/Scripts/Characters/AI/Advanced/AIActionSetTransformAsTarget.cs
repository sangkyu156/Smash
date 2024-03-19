using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 지정된 변환을 대상으로 설정하는 데 사용되는 AIACtion
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Actions/AIActionSetTransformAsTarget")]
	public class AIActionSetTransformAsTarget : AIAction
	{
		public Transform TargetTransform;
		public bool OnlyRunOnce = true;
    
		protected bool _alreadyRan = false;
    
		/// <summary>
		/// On init we initialize our action
		/// </summary>
		public override void Initialization()
		{
			base.Initialization();
			_alreadyRan = false;
		}

		/// <summary>
		/// Sets a new target
		/// </summary>
		public override void PerformAction()
		{
			if (OnlyRunOnce && _alreadyRan)
			{
				return;
			}
			_brain.Target = TargetTransform;
		}

		/// <summary>
		/// On enter state we reset our flag
		/// </summary>
		public override void OnEnterState()
		{
			base.OnEnterState();
			_alreadyRan = false;
		}
	}
}