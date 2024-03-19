using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// PerformAction에서 순찰 방향을 반전시킵니다.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Actions/AIActionInvertPatrolDirection")]
	public class AIActionInvertPatrolDirection : AIAction
	{
		[Header("Invert Patrol Action Bindings")]
        /// AIActionMovePatrol2D를 사용하여 순찰 방향을 반전시킵니다.
        [Tooltip("AIActionMovePatrol2D를 사용하여 순찰 방향을 반전시킵니다.")]
		public AIActionMovePatrol2D _movePatrol2D;
        /// AIActionMovePatrol3D를 사용하여 순찰 방향을 반전시킵니다.
        [Tooltip("AIActionMovePatrol3D를 사용하여 순찰 방향을 반전시킵니다.")]
		public AIActionMovePatrol3D _movePatrol3D;
        
		/// <summary>
		/// On init we grab our actions
		/// </summary>
		public override void Initialization()
		{
			if(!ShouldInitialize) return;
			base.Initialization();
			if (_movePatrol2D == null)
			{
				_movePatrol2D = this.gameObject.GetComponentInParent<AIActionMovePatrol2D>();    
			}
			if (_movePatrol3D == null)
			{
				_movePatrol3D = this.gameObject.GetComponentInParent<AIActionMovePatrol3D>();    
			}
		}

		/// <summary>
		/// Inverts the patrol direction
		/// </summary>
		public override void PerformAction()
		{
			_movePatrol2D?.ChangeDirection();
			_movePatrol3D?.ChangeDirection();
		}
	}
}