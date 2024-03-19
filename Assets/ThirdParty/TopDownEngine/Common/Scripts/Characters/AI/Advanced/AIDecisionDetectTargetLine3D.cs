using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 결정은 지정된 방향의 광선이나 박스캐스트에서 대상이 발견되면 true를 반환합니다.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Decisions/AIDecisionDetectTargetLine3D")]
	public class AIDecisionDetectTargetLine3D : AIDecision
	{
		/// the possible detection methods
		public enum DetectMethods { Ray, WideRay }
		/// the detection method
		[Tooltip("선택된 감지 방법: 광선은 단일 광선이고, 넓은 광선은 더 비싸지만 더 정확합니다.")]
		public DetectMethods DetectMethod = DetectMethods.Ray;
        /// 투사할 광선의 너비(WideRay 모드에만 있는 경우
        [Tooltip("투사할 광선의 너비(WideRay 모드에만 있는 경우")]
		public float RayWidth = 1f;
        /// 우리가 광선을 투사할 거리
        [Tooltip("우리가 광선을 투사할 거리")]
		public float DetectionDistance = 10f;
        /// the광선에 적용할 오프셋
        [Tooltip("광선에 적용할 오프셋")]
		public Vector3 DetectionOriginOffset = new Vector3(0,0,0);
        /// 대상을 검색하려는 레이어
        [Tooltip("대상을 검색하려는 레이어")]
		public LayerMask TargetLayer;
        /// 장애물이 설정된 레이어. 장애물이 광선을 차단합니다.
        [Tooltip("장애물이 설정된 레이어. 장애물이 광선을 차단합니다.")]
		public LayerMask ObstaclesLayer = LayerManager.ObstaclesLayerMask;
        /// 감지 레이캐스트에 대한 회전 참조로 사용할 변환입니다. 예를 들어 회전 모델이 있는 경우 여기에서 이를 참조 변환으로 설정하고 싶을 것입니다.
        [Tooltip("감지 레이캐스트에 대한 회전 참조로 사용할 변환입니다. 예를 들어 회전 모델이 있는 경우 여기에서 이를 참조 변환으로 설정하고 싶을 것입니다.")]
		public Transform ReferenceTransform;
        /// 이것이 사실이라면, 이 결정은 무기가 탐지 방향을 향하도록 강제할 것입니다.
        [Tooltip("이것이 사실이라면, 이 결정은 무기가 탐지 방향을 향하도록 강제할 것입니다.")]
		public bool ForceAimToDetectionDirection = false;
        /// 이것이 사실이라면 이 결정은 대상이 발견되지 않으면 AI Brain의 대상을 null로 설정합니다.
        [Tooltip("이것이 사실이라면 이 결정은 대상이 발견되지 않으면 AI Brain의 대상을 null로 설정합니다.")]
		public bool SetTargetToNullIfNoneIsFound = true;

		protected Vector3 _direction;
		protected float _distanceToTarget;
		protected Vector3 _raycastOrigin;
		protected Character _character;
		protected Color _gizmosColor = Color.yellow;
		protected Vector3 _gizmoCenter;
		protected Vector3 _gizmoSize;
		protected bool _init = false;
		protected CharacterHandleWeapon _characterHandleWeapon;

		/// <summary>
		/// On Init we grab our character
		/// </summary>
		public override void Initialization()
		{
			_character = this.gameObject.GetComponentInParent<Character>();
			_characterHandleWeapon = _character.FindAbility<CharacterHandleWeapon>();
			_gizmosColor.a = 0.25f;
			_init = true;
			if (ReferenceTransform == null)
			{
				ReferenceTransform = this.transform;
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
		/// Returns true if a target is found by the ray
		/// </summary>
		/// <returns></returns>
		protected virtual bool DetectTarget()
		{
			bool hit = false;
			_distanceToTarget = 0;
			Transform target = null;
			RaycastHit raycast;

			_direction = ReferenceTransform.forward;
            
			// we cast a ray to the left of the agent to check for a Player
			_raycastOrigin = ReferenceTransform.position + DetectionOriginOffset ;

			if (DetectMethod == DetectMethods.Ray)
			{
				raycast = MMDebug.Raycast3D(_raycastOrigin, _direction, DetectionDistance, TargetLayer, MMColors.Gold, true);
                
			}
			else
			{
				hit = Physics.BoxCast(_raycastOrigin, Vector3.one * (RayWidth * 0.5f), _direction, out raycast, ReferenceTransform.rotation, DetectionDistance, TargetLayer);
			}
                
			if (raycast.collider != null)
			{
				hit = true;
				_distanceToTarget = Vector3.Distance(_raycastOrigin, raycast.point);
				target = raycast.collider.gameObject.transform;
			}

			if (hit)
			{
				// we make sure there isn't an obstacle in between
				float distance = Vector3.Distance(target.transform.position, _raycastOrigin);
				RaycastHit raycastObstacle = MMDebug.Raycast3D(_raycastOrigin, (target.transform.position - _raycastOrigin).normalized, distance, ObstaclesLayer, Color.gray, true);
                
				if ((raycastObstacle.collider != null) && (_distanceToTarget > raycastObstacle.distance))
				{
					if (SetTargetToNullIfNoneIsFound) { _brain.Target = null; }
					return false;
				}
				else
				{
					// if there's no obstacle, we store our target and return true
					_brain.Target = target;
					return true;
				}
			}

			ForceDirection();
            
			if (SetTargetToNullIfNoneIsFound) { _brain.Target = null; }
			return false;           
		}

		/// <summary>
		/// Forces the weapon to aim in the selected direction if needed
		/// </summary>
		protected virtual void ForceDirection()
		{
			if (!ForceAimToDetectionDirection)
			{
				return;
			}
			if (_characterHandleWeapon == null)
			{
				return;
			}
			if (_characterHandleWeapon.CurrentWeapon == null)
			{
				return;
			}
			_characterHandleWeapon.CurrentWeapon.gameObject.MMGetComponentNoAlloc<WeaponAim3D>()?.SetCurrentAim(ReferenceTransform.forward);
		}
        
		/// <summary>
		/// Draws ray gizmos
		/// </summary>
		protected virtual void OnDrawGizmos()
		{
			if (DetectMethod != DetectMethods.WideRay)
			{
				return;
			}

			Gizmos.color = _gizmosColor;
            

            
			_gizmoCenter = DetectionOriginOffset + Vector3.forward * DetectionDistance / 2f;
			_gizmoSize.x = RayWidth;
			_gizmoSize.y = RayWidth;
			_gizmoSize.z = DetectionDistance;
            
			Matrix4x4 rotationMatrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
			if (ReferenceTransform != null)
			{
				Gizmos.matrix = ReferenceTransform.localToWorldMatrix;
			}
            
            
			Gizmos.DrawCube(_gizmoCenter, _gizmoSize);
            
            
		}
        
        
	}
}