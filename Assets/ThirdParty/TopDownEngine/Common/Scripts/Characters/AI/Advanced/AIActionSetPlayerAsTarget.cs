using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 현재 플레이어 캐릭터를 대상으로 설정하는 데 사용되는 AIACtion
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Actions/AIActionSetPlayerAsTarget")]
	public class AIActionSetPlayerAsTarget : AIAction
	{
		public bool OnlyRunOnce = true;
        
		protected bool _alreadyRan = false;
        
		/// <summary>
		/// On init we initialize our action
		/// </summary>
		public override void Initialization()
		{
			if(!ShouldInitialize) return;
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

			if (LevelManager.HasInstance && LevelManager.Instance.Players != null && LevelManager.Instance.Players[0] != null)
			{
				_brain.Target = LevelManager.Instance.Players[0].transform;
			}
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