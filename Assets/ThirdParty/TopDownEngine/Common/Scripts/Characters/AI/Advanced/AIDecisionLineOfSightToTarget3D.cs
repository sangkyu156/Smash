using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 결정은 3D로 에이전트와 뇌의 타겟 사이에 직선상에 장애물이 없으면 참이 됩니다.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Decisions/AIDecisionLineOfSightToTarget3D")]
	public class AIDecisionLineOfSightToTarget3D : AIDecision
	{
        /// 시선이 존재하는지 여부를 결정하려고 할 때 장애물로 간주할 레이어 마스크
        [Tooltip("시선이 존재하는지 여부를 결정하려고 할 때 장애물로 간주할 레이어 마스크")]
		public LayerMask ObstacleLayerMask = LayerManager.ObstaclesLayerMask;
        /// 에이전트에서 대상으로 광선을 캐스팅할 때 충돌체 중심에서 적용할 오프셋입니다.
        [Tooltip("에이전트에서 대상으로 광선을 캐스팅할 때 충돌체 중심에서 적용할 오프셋입니다.")]
		public Vector3 LineOfSightOffset = new Vector3(0, 0, 0);

		protected Vector3 _directionToTarget;
		protected Collider _collider;
		protected Vector3 _raycastOrigin;

		/// <summary>
		/// On init we grab our collider
		/// </summary>
		public override void Initialization()
		{
			_collider = this.gameObject.GetComponentInParent<Collider>();
		}

		/// <summary>
		/// On Decide we check whether we have a line of sight
		/// </summary>
		/// <returns></returns>
		public override bool Decide()
		{
			return CheckLineOfSight();
		}

		/// <summary>
		/// Checks whether there are obstacles between the agent and the target
		/// </summary>
		/// <returns></returns>
		protected virtual bool CheckLineOfSight()
		{
			if (_brain.Target == null)
			{
				return false;
			}

			_raycastOrigin = _collider.bounds.center + LineOfSightOffset / 2;
			_directionToTarget = _brain.Target.transform.position - _raycastOrigin;
            
			RaycastHit hit = MMDebug.Raycast3D(_raycastOrigin, _directionToTarget.normalized, _directionToTarget.magnitude, ObstacleLayerMask, Color.yellow, true);
			if (hit.collider == null)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
	}
}