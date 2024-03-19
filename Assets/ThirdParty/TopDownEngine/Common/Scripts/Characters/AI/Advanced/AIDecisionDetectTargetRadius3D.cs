using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 결정은 TargetLayer 레이어 마스크의 개체가 지정된 반경 내에 있으면 true를 반환하고, 그렇지 않으면 false를 반환합니다. 또한 뇌의 대상을 해당 개체로 설정합니다.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Decisions/AIDecisionDetectTargetRadius3D")]
	//[RequireComponent(typeof(Character))]
	public class AIDecisionDetectTargetRadius3D : AIDecision
	{
        /// 목표물을 검색할 반경
        [Tooltip("목표물을 검색할 반경")]
		public float Radius = 3f;
        /// 적용할 오프셋(충돌기 중심에서)
        [Tooltip("적용할 오프셋(충돌기 중심에서)")]
		public Vector3 DetectionOriginOffset = new Vector3(0, 0, 0);
        /// 대상을 검색할 레이어
        [Tooltip("대상을 검색할 레이어")]
		public LayerMask TargetLayerMask;
        /// 시야를 차단하는 레이어
        [Tooltip("시야를 차단하는 레이어")]
		public LayerMask ObstacleMask = LayerManager.ObstaclesLayerMask;
        /// 장애물을 확인하는 빈도(초)
        [Tooltip("장애물을 확인하는 빈도(초)")]
		public float TargetCheckFrequency = 1f;
        /// 이것이 사실이라면 이 AI는 자신(또는 그 자식)을 대상으로 간주할 수 있습니다.
        [Tooltip("이것이 사실이라면 이 AI는 자신(또는 그 자식)을 대상으로 간주할 수 있습니다.")] 
		public bool CanTargetSelf = false;
        /// 중첩 감지가 획득할 수 있는 최대 대상 수
        [Tooltip("중첩 감지가 획득할 수 있는 최대 대상 수")]
		public int OverlapMaximum = 10;

		protected Collider _collider;
		protected Vector3 _raycastOrigin;
		protected Character _character;
		protected Color _gizmoColor = Color.yellow;
		protected bool _init = false;
		protected Vector3 _raycastDirection;
		protected Collider[] _hits;
		protected float _lastTargetCheckTimestamp = 0f;
		protected bool _lastReturnValue = false;
		protected List<Transform> _potentialTargets;

		/// <summary>
		/// On init we grab our Character component
		/// </summary>
		public override void Initialization()
		{
			_lastTargetCheckTimestamp = 0f;
			_potentialTargets = new List<Transform>();
			_character = this.gameObject.GetComponentInParent<Character>();
			_collider = this.gameObject.GetComponentInParent<Collider>();
			_gizmoColor.a = 0.25f;
			_init = true;
			_lastReturnValue = false;
			_hits = new Collider[OverlapMaximum];
		}

		/// <summary>
		/// On Decide we check for our target
		/// </summary>
		/// <returns></returns>
		public override bool Decide()
		{
			return DetectTarget();
		}

		/// <summary>
		/// Returns true if a target is found within the circle
		/// </summary>
		/// <returns></returns>
		protected virtual bool DetectTarget()
		{
			// we check if there's a need to detect a new target
			if (Time.time - _lastTargetCheckTimestamp < TargetCheckFrequency)
			{
				return _lastReturnValue;
			}
			_potentialTargets.Clear();

			_lastTargetCheckTimestamp = Time.time;
			_raycastOrigin = _collider.bounds.center + DetectionOriginOffset / 2;
			int numberOfCollidersFound = Physics.OverlapSphereNonAlloc(_raycastOrigin, Radius, _hits, TargetLayerMask);

			// if there are no targets around, we exit
			if (numberOfCollidersFound == 0)
			{
				_lastReturnValue = false;
				return false;
			}
            
			// we go through each collider found
			int min = Mathf.Min(OverlapMaximum, numberOfCollidersFound);
			for (int i = 0; i < min; i++)
			{
				if (_hits[i] == null)
				{
					continue;
				}
                
				if (!CanTargetSelf)
				{
					if ((_hits[i].gameObject == _brain.Owner) || (_hits[i].transform.IsChildOf(this.transform)))
					{
						continue;
					}    
				}
                
				_potentialTargets.Add(_hits[i].gameObject.transform);
			}
            
			// we sort our targets by distance
			_potentialTargets.Sort(delegate(Transform a, Transform b)
			{return Vector3.Distance(this.transform.position,a.transform.position)
				.CompareTo(
					Vector3.Distance(this.transform.position,b.transform.position) );
			});
            
			// we return the first unobscured target
			foreach (Transform t in _potentialTargets)
			{
				_raycastDirection = t.position - _raycastOrigin;
				RaycastHit hit = MMDebug.Raycast3D(_raycastOrigin, _raycastDirection, _raycastDirection.magnitude, ObstacleMask.value, Color.yellow, true);
				if (hit.collider == null)
				{
					_brain.Target = t;
					_lastReturnValue = true;
					return true;
				}
			}

			_lastReturnValue = false;
			return false;
		}

		/// <summary>
		/// Draws gizmos for the detection circle
		/// </summary>
		protected virtual void OnDrawGizmosSelected()
		{
			_raycastOrigin = transform.position + DetectionOriginOffset / 2;

			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(_raycastOrigin, Radius);
			if (_init)
			{
				Gizmos.color = _gizmoColor;
				Gizmos.DrawSphere(_raycastOrigin, Radius);
			}            
		}
	}
}