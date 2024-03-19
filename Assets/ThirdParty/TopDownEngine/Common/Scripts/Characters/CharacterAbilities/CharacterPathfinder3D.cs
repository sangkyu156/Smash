using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using UnityEngine.AI;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 클래스를 3D 캐릭터에 추가하면 내비메시를 탐색할 수 있습니다(물론 장면에 내비메시가 있는 경우).
    /// </summary>
    [MMHiddenProperties("AbilityStartFeedbacks", "AbilityStopFeedbacks")]
	[AddComponentMenu("TopDown Engine/Character/Abilities/Character Pathfinder 3D")]
	public class CharacterPathfinder3D : CharacterAbility
	{
		public enum PathRefreshModes { None, TimeBased, SpeedThresholdBased }
		
		[Header("PathfindingTarget")]

		/// the target the character should pathfind to
		[Tooltip("캐릭터가 경로를 찾아야 하는 대상")]
		public Transform Target;
		/// the distance to waypoint at which the movement is considered complete
		[Tooltip("이동이 완료된 것으로 간주되는 웨이포인트까지의 거리")]
		public float DistanceToWaypointThreshold = 1f;
		/// if the target point can't be reached, the distance threshold around that point in which to look for an alternative end point
		[Tooltip("목표 지점에 도달할 수 없는 경우 대체 끝점을 찾기 위한 해당 지점 주변의 거리 임계값")]
		public float ClosestPointThreshold = 3f;
		/// a minimum delay (in seconds) between two navmesh requests - longer delay means better performance but less accuracy
		[Tooltip("두 개의 navmesh 요청 사이의 최소 지연(초) - 지연 시간이 길수록 성능은 향상되지만 정확도는 떨어집니다.")]
		public float MinimumDelayBeforePollingNavmesh = 0.1f;

		[Header("Path Refresh")]
		/// the chosen mode in which to refresh the path (none : nothing will happen and path will only refresh on set new destination,
		/// time based : path will refresh every x seconds, speed threshold based : path will refresh every x seconds if the character's speed is below a certain threshold
		[Tooltip("경로를 새로 고치기 위해 선택한 모드(없음: 아무 일도 일어나지 않고 경로는 새 대상이 설정된 경우에만 새로 고쳐집니다. " +
                 "time based : 경로는 x초마다 새로 고쳐집니다., speed threshold based : 캐릭터의 속도가 특정 임계값보다 낮으면 경로가 x초마다 새로 고쳐집니다.")]
		public PathRefreshModes PathRefreshMode = PathRefreshModes.None;
		/// the speed under which the path should be recomputed, usually if the character blocks against an obstacle
		[Tooltip("일반적으로 캐릭터가 장애물을 막는 경우 경로를 다시 계산해야 하는 속도입니다.")]
		[MMEnumCondition("PathRefreshMode", (int)PathRefreshModes.SpeedThresholdBased)]
		public float RefreshSpeedThreshold = 1f;
		/// the interval at which to refresh the path, in seconds
		[Tooltip("경로를 새로 고치는 간격(초)")]
		[MMEnumCondition("PathRefreshMode", (int)PathRefreshModes.TimeBased, (int)PathRefreshModes.SpeedThresholdBased)]
		public float RefreshInterval = 2f;

		[Header("Debug")]
		/// whether or not we should draw a debug line to show the current path of the character
		[Tooltip("캐릭터의 현재 경로를 표시하기 위해 디버그 라인을 그려야 하는지 여부")]
		public bool DebugDrawPath;

		/// the current path
		[MMReadOnly]
		[Tooltip("현재 경로")]
		public UnityEngine.AI.NavMeshPath AgentPath;
		/// a list of waypoints the character will go through
		[MMReadOnly]
		[Tooltip("캐릭터가 통과할 웨이포인트 목록")]
		public Vector3[] Waypoints;
		/// the index of the next waypoint
		[MMReadOnly]
		[Tooltip("다음 웨이포인트의 인덱스")]
		public int NextWaypointIndex;
		/// the direction of the next waypoint
		[MMReadOnly]
		[Tooltip("다음 웨이포인트 방향")]
		public Vector3 NextWaypointDirection;
		/// the distance to the next waypoint
		[MMReadOnly]
		[Tooltip("다음 웨이포인트까지의 거리")]
		public float DistanceToNextWaypoint;

		public event System.Action<int, int, float> OnPathProgress;

		public virtual void InvokeOnPathProgress(int waypointIndex, int waypointsLength, float distance)
		{
			OnPathProgress?.Invoke(waypointIndex, waypointsLength, distance);
		}

		protected int _waypoints;
		protected Vector3 _direction;
		protected Vector2 _newMovement;
		protected Vector3 _lastValidTargetPosition;
		protected Vector3 _closestStartNavmeshPosition;
		protected Vector3 _closestTargetNavmeshPosition;
		protected UnityEngine.AI.NavMeshHit _navMeshHit;
		protected bool _pathFound;
		protected float _lastRequestAt = -Single.MaxValue;

		protected override void Initialization()
		{
			base.Initialization();
			AgentPath = new UnityEngine.AI.NavMeshPath();
			_lastValidTargetPosition = this.transform.position;
			Array.Resize(ref Waypoints, 5);
		}

		/// <summary>
		/// Sets a new destination the character will pathfind to
		/// </summary>
		/// <param name="destinationTransform"></param>
		public virtual void SetNewDestination(Transform destinationTransform)
		{
			if (destinationTransform == null)
			{
				Target = null;
				return;
			}
			Target = destinationTransform;
			DeterminePath(this.transform.position, Target.position);
		}

		/// <summary>
		/// On Update, we draw the path if needed, determine the next waypoint, and move to it if needed
		/// </summary>
		public override void ProcessAbility()
		{
			if (Target == null)
			{
				return;
			}

			if (!AbilityAuthorized
			    || (_condition.CurrentState != CharacterStates.CharacterConditions.Normal))
			{
				return;
			}

			PerformRefresh();
			DrawDebugPath();
			DetermineNextWaypoint();
			DetermineDistanceToNextWaypoint();
			MoveController();
		}
        
		/// <summary>
		/// Moves the controller towards the next point
		/// </summary>
		protected virtual void MoveController()
		{
			if ((Target == null) || (NextWaypointIndex <= 0))
			{
				_characterMovement.SetMovement(Vector2.zero);
				return;
			}
			else
			{
				_direction = (Waypoints[NextWaypointIndex] - this.transform.position).normalized;
				_newMovement.x = _direction.x;
				_newMovement.y = _direction.z;
				_characterMovement.SetMovement(_newMovement);
			}
		}

		protected virtual void PerformRefresh()
		{
			if (PathRefreshMode == PathRefreshModes.None)
			{
				return;
			}
			
			if (NextWaypointIndex <= 0)
			{
				return;
			}

			bool refreshNeeded = false;

			if (Time.time - _lastRequestAt > RefreshInterval)
			{
				refreshNeeded = true;
				_lastRequestAt = Time.time;
			}

			if (PathRefreshMode == PathRefreshModes.SpeedThresholdBased)
			{
				if (_controller.Speed.magnitude > RefreshSpeedThreshold)
				{
					refreshNeeded = false;
				}
			}

			if (refreshNeeded)
			{
				DeterminePath(this.transform.position, Target.position, true);
			}
		}
        
		/// <summary>
		/// Determines the next path position for the agent. NextPosition will be zero if a path couldn't be found
		/// </summary>
		/// <param name="startingPos"></param>
		/// <param name="targetPos"></param>
		/// <returns></returns>        
		protected virtual void DeterminePath(Vector3 startingPosition, Vector3 targetPosition, bool ignoreDelay = false)
		{
			if (!ignoreDelay && (Time.time - _lastRequestAt < MinimumDelayBeforePollingNavmesh))
			{
				return;
			}
			
			_lastRequestAt = Time.time;
			
			NextWaypointIndex = 0;
			
			// we find the closest position to the starting position on the navmesh
			_closestStartNavmeshPosition = startingPosition;
			if (UnityEngine.AI.NavMesh.SamplePosition(startingPosition, out _navMeshHit, ClosestPointThreshold, UnityEngine.AI.NavMesh.AllAreas))
			{
				_closestStartNavmeshPosition = _navMeshHit.position;
			}
                
			// we find the closest position to the target position on the navmesh
			_closestTargetNavmeshPosition = targetPosition;
			if (UnityEngine.AI.NavMesh.SamplePosition(targetPosition, out _navMeshHit, ClosestPointThreshold, UnityEngine.AI.NavMesh.AllAreas))
			{
				_closestTargetNavmeshPosition = _navMeshHit.position;
			}

			_pathFound = UnityEngine.AI.NavMesh.CalculatePath(_closestStartNavmeshPosition, _closestTargetNavmeshPosition, UnityEngine.AI.NavMesh.AllAreas, AgentPath);
			if (_pathFound)
			{
				_lastValidTargetPosition = _closestTargetNavmeshPosition;
			}
			else
			{
				UnityEngine.AI.NavMesh.CalculatePath(startingPosition, _lastValidTargetPosition, UnityEngine.AI.NavMesh.AllAreas, AgentPath);
			}

			// Waypoints = AgentPath.corners;
			_waypoints = AgentPath.GetCornersNonAlloc(Waypoints);
			if (_waypoints >= Waypoints.Length)
			{
				Array.Resize(ref Waypoints, _waypoints +5);
				_waypoints = AgentPath.GetCornersNonAlloc(Waypoints);
			}
			if (_waypoints >= 2)
			{
				NextWaypointIndex = 1;
			}

			InvokeOnPathProgress(NextWaypointIndex, Waypoints.Length, Vector3.Distance(this.transform.position, Waypoints[NextWaypointIndex]));
		}
        
		/// <summary>
		/// Determines the next waypoint based on the distance to it
		/// </summary>
		protected virtual void DetermineNextWaypoint()
		{
			if (_waypoints <= 0)
			{
				return;
			}
			if (NextWaypointIndex < 0)
			{
				return;
			}

			var distance = Vector3.Distance(this.transform.position, Waypoints[NextWaypointIndex]);
			if (distance <= DistanceToWaypointThreshold)
			{
				if (NextWaypointIndex + 1 < _waypoints)
				{
					NextWaypointIndex++;
				}
				else
				{
					NextWaypointIndex = -1;
				}
				InvokeOnPathProgress(NextWaypointIndex, _waypoints, distance);
			}
		}

		/// <summary>
		/// Determines the distance to the next waypoint
		/// </summary>
		protected virtual void DetermineDistanceToNextWaypoint()
		{
			if (NextWaypointIndex <= 0)
			{
				DistanceToNextWaypoint = 0;
			}
			else
			{
				DistanceToNextWaypoint = Vector3.Distance(this.transform.position, Waypoints[NextWaypointIndex]);
			}
		}

		/// <summary>
		/// Draws a debug line to show the current path
		/// </summary>
		protected virtual void DrawDebugPath()
		{
			if (DebugDrawPath)
			{
				if (_waypoints <= 0)
				{
					if (Target != null)
					{
						DeterminePath(transform.position, Target.position);
					}
				}
				for (int i = 0; i < _waypoints - 1; i++)
				{
					Debug.DrawLine(Waypoints[i], Waypoints[i + 1], Color.red);
				}
			}
		}
	}
}