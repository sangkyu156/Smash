using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 결정은 TargetLayer 레이어 마스크의 개체가 시선에 들어오면 true를 반환합니다. 또한 뇌의 대상을 해당 개체로 설정합니다. 광선 모드를 선택할 수 있습니다. 이 경우 시야선은 실제 선(레이캐스트)이 되거나 더 넓어집니다(이 경우 스피어캐스트를 사용함). 광선 원점에 대한 오프셋과 이를 차단하는 장애물 레이어 마스크를 지정할 수도 있습니다.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Decisions/AIDecisionDetectTargetLine2D")]
	//[RequireComponent(typeof(Character))]
	//[RequireComponent(typeof(CharacterOrientation2D))]
	public class AIDecisionDetectTargetLine2D : AIDecision
	{
		/// the possible detection methods
		public enum DetectMethods { Ray, WideRay }
		/// the possible detection directions
		public enum DetectionDirections { Front, Back, Both }
		/// the detection method
		[Tooltip("선택된 감지 방법: 광선은 단일 광선이고, 넓은 광선은 더 비싸지만 더 정확합니다.")]
		public DetectMethods DetectMethod = DetectMethods.Ray;
		/// the detection direction
		[Tooltip("감지 방향: 앞, 뒤 또는 둘 다")]
		public DetectionDirections DetectionDirection = DetectionDirections.Front;
        /// 투사할 광선의 너비(WideRay 모드에만 있는 경우)
        [Tooltip("투사할 광선의 너비(WideRay 모드에만 있는 경우)")]
		public float RayWidth = 1f;
        /// 우리가 광선을 투사할 거리
        [Tooltip("우리가 광선을 투사할 거리")]
		public float DetectionDistance = 10f;
        /// 광선에 적용할 오프셋
        [Tooltip("광선에 적용할 오프셋")]
		public Vector3 DetectionOriginOffset = new Vector3(0,0,0);
        /// 대상을 검색하려는 레이어
        [Tooltip("대상을 검색하려는 레이어")]
		public LayerMask TargetLayer;
        /// 장애물이 설정된 레이어입니다. 장애물이 광선을 차단합니다.
        [Tooltip("장애물이 설정된 레이어입니다. 장애물이 광선을 차단합니다.")]
		public LayerMask ObstaclesLayer = LayerManager.ObstaclesLayerMask;
        /// 감지 레이캐스트에 대한 회전 참조로 사용할 변환입니다. 예를 들어 회전 모델이 있는 경우 여기에서 이를 참조 변환으로 설정하고 싶을 것입니다.
        [Tooltip("감지 레이캐스트에 대한 회전 참조로 사용할 변환입니다. 예를 들어 회전 모델이 있는 경우 여기에서 이를 참조 변환으로 설정하고 싶을 것입니다.")]
		public Transform ReferenceTransform;
        /// 이것이 사실이라면 이 결정은 대상이 발견되지 않으면 AI Brain의 대상을 null로 설정합니다.
        [Tooltip("이것이 사실이라면 이 결정은 대상이 발견되지 않으면 AI Brain의 대상을 null로 설정합니다.")]
		public bool SetTargetToNullIfNoneIsFound = true;

		protected Vector2 _direction;
		protected Vector2 _facingDirection;
		protected float _distanceToTarget;
		protected Vector2 _raycastOrigin;
		protected Character _character;
		protected CharacterOrientation2D _orientation2D;
		protected bool _drawLeftGizmo = false;
		protected bool _drawRightGizmo = false;
		protected Color _gizmosColor = Color.yellow;
		protected Vector3 _gizmoCenter;
		protected Vector3 _gizmoSize;
		protected bool _init = false;
		protected Vector2 _boxcastSize = Vector2.zero;
		protected bool _isFacingRight = true;

		protected Vector2 _transformRight { get { return ReferenceTransform.right; } }
		protected Vector2 _transformLeft { get { return -ReferenceTransform.right; } }
		protected Vector2 _transformUp { get { return ReferenceTransform.up; } }
		protected Vector2 _transformDown { get { return -ReferenceTransform.up; } }

		/// <summary>
		/// On Init we grab our character
		/// </summary>
		public override void Initialization()
		{
			_character = this.gameObject.GetComponentInParent<Character>();
			_orientation2D = _character?.FindAbility<CharacterOrientation2D>();
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
			RaycastHit2D raycast;
			_drawLeftGizmo = false;
			_drawRightGizmo = false;

			_boxcastSize.x = DetectionDistance / 5f;
			_boxcastSize.y = RayWidth;

			if (_orientation2D == null)
			{
				_facingDirection = _transformRight;
				_isFacingRight = true;
			}
			else
			{
				_isFacingRight = _orientation2D.IsFacingRight;
				_facingDirection = _orientation2D.IsFacingRight ? _transformRight : _transformLeft;    
			}
            
			// we cast a ray to the left of the agent to check for a Player
			_raycastOrigin.x = transform.position.x + _facingDirection.x * DetectionOriginOffset.x / 2;
			_raycastOrigin.y = transform.position.y + DetectionOriginOffset.y;

			// we cast it to the left	
			if ((DetectionDirection == DetectionDirections.Both)
			    || ((DetectionDirection == DetectionDirections.Front) && (!_isFacingRight))
			    || ((DetectionDirection == DetectionDirections.Back) && (_isFacingRight)))
			{
				if (DetectMethod == DetectMethods.Ray)
				{
					raycast = MMDebug.RayCast(_raycastOrigin, _transformLeft, DetectionDistance, TargetLayer, MMColors.Gold, true);
				}
				else
				{
					raycast = Physics2D.BoxCast(_raycastOrigin - Vector2.right * _boxcastSize.x / 2f, _boxcastSize, 0f, Vector2.left, DetectionDistance - _boxcastSize.x, TargetLayer);
					MMDebug.RayCast(_raycastOrigin + _transformUp * RayWidth/2f, _transformLeft, DetectionDistance, TargetLayer, MMColors.Gold, true);
					MMDebug.RayCast(_raycastOrigin - _transformUp * RayWidth / 2f, _transformLeft, DetectionDistance, TargetLayer, MMColors.Gold, true);
					MMDebug.RayCast(_raycastOrigin - _transformUp * RayWidth / 2f + _transformLeft * DetectionDistance, _transformUp, RayWidth, TargetLayer, MMColors.Gold, true);
					_drawLeftGizmo = true;
				}
                
				// if we see a player
				if (raycast)
				{
					hit = true;
					_direction = Vector2.left;
					_distanceToTarget = Vector2.Distance(_raycastOrigin, raycast.point);
					target = raycast.collider.gameObject.transform;
				}
			}

			// we cast a ray to the right of the agent to check for a Player	
			if ((DetectionDirection == DetectionDirections.Both)
			    || ((DetectionDirection == DetectionDirections.Front) && (_isFacingRight))
			    || ((DetectionDirection == DetectionDirections.Back) && (!_isFacingRight)))
			{
				if (DetectMethod == DetectMethods.Ray)
				{
					raycast = MMDebug.RayCast(_raycastOrigin, _transformRight, DetectionDistance, TargetLayer, MMColors.DarkOrange, true);
				}
				else
				{
					raycast = Physics2D.BoxCast(_raycastOrigin + Vector2.right * _boxcastSize.x / 2f, _boxcastSize, 0f, Vector2.right, DetectionDistance - _boxcastSize.x, TargetLayer);
					MMDebug.RayCast(_raycastOrigin + _transformUp * RayWidth / 2f, _transformRight, DetectionDistance, TargetLayer, MMColors.DarkOrange, true);
					MMDebug.RayCast(_raycastOrigin - _transformUp * RayWidth / 2f, _transformRight, DetectionDistance, TargetLayer, MMColors.DarkOrange, true);
					MMDebug.RayCast(_raycastOrigin - _transformUp * RayWidth / 2f + _transformRight * DetectionDistance, _transformUp, RayWidth, TargetLayer, MMColors.DarkOrange, true);
					_drawLeftGizmo = true;
				}
                
				if (raycast)
				{
					hit = true;
					_direction = Vector2.right;
					_distanceToTarget = Vector2.Distance(_raycastOrigin, raycast.point);
					target = raycast.collider.gameObject.transform;
				}
			}

			if (hit)
			{
				// we make sure there isn't an obstacle in between
				float distance = Vector2.Distance((Vector2)target.transform.position, _raycastOrigin);
				RaycastHit2D raycastObstacle = MMDebug.RayCast(_raycastOrigin, ((Vector2)target.transform.position - _raycastOrigin).normalized, distance, ObstaclesLayer, Color.gray, true);
                
				if (raycastObstacle && _distanceToTarget > raycastObstacle.distance)
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
			if (SetTargetToNullIfNoneIsFound) { _brain.Target = null; }
			return false;           
		}
        
		/// <summary>
		/// Draws ray gizmos
		/// </summary>
		protected virtual void OnDrawGizmos()
		{
			if ((DetectMethod != DetectMethods.WideRay) || !_init)
			{
				return;
			}

			Gizmos.color = _gizmosColor;

			_raycastOrigin.x = transform.position.x + _facingDirection.x * DetectionOriginOffset.x / 2;
			_raycastOrigin.y = transform.position.y + DetectionOriginOffset.y;

			if ((DetectionDirection == DetectionDirections.Both)
			    || ((DetectionDirection == DetectionDirections.Front) && (!_isFacingRight))
			    || ((DetectionDirection == DetectionDirections.Back) && (_isFacingRight)))
			{
				_gizmoCenter = (Vector3)_raycastOrigin + Vector3.left * DetectionDistance / 2f;
				_gizmoSize.x = DetectionDistance;
				_gizmoSize.y = RayWidth;
				_gizmoSize.z = 1f;
				Gizmos.DrawCube(_gizmoCenter, _gizmoSize);
			}

			if ((DetectionDirection == DetectionDirections.Both)
			    || ((DetectionDirection == DetectionDirections.Front) && (_isFacingRight))
			    || ((DetectionDirection == DetectionDirections.Back) && (!_isFacingRight)))
			{
				_gizmoCenter = (Vector3)_raycastOrigin + Vector3.right * DetectionDistance / 2f;
				_gizmoSize.x = DetectionDistance;
				_gizmoSize.y = RayWidth;
				_gizmoSize.z = 1f;
				Gizmos.DrawCube(_gizmoCenter, _gizmoSize);
			}
		}
	}
}