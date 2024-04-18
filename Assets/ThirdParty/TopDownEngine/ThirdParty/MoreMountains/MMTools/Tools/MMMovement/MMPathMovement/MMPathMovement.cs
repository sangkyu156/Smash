using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using Random = UnityEngine.Random;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 이 구성요소를 개체에 추가하면 개체는 해당 검사기에서 정의된 경로를 따라 이동할 수 있습니다.
    /// </summary>
    [AddComponentMenu("More Mountains/Tools/Movement/MMPathMovement")]
	public class MMPathMovement : MonoBehaviour 
	{
        /// 가능한 이동 유형
        public enum PossibleAccelerationType { ConstantSpeed, EaseOut, AnimationCurve }
        /// 가능한 사이클 옵션
        public enum CycleOptions { BackAndForth, Loop, OnlyOnce, StopAtBounds, Random }
        /// 가능한 이동 방향
        public enum MovementDirection { Ascending, Descending }
        /// 업데이트, 고정 업데이트 또는 늦은 업데이트 시 패스 진행 여부를 결정합니다.
        public enum UpdateModes { Update, FixedUpdate, LateUpdate }
        /// 아무것도 없는 위치, 이 객체의 회전 또는 이 객체의 부모 회전에 경로를 정렬할지 여부
        public enum AlignmentModes { None, ThisRotation, ParentRotation }

		[Header("Path")]
		[MMInformation("여기에서 '<b>주기 옵션</b>'을 선택할 수 있습니다. 앞뒤로 개체가 끝까지 경로를 따라 갔다가 원래 지점으로 돌아갑니다. 루프를 선택하면 경로가 닫히고 달리 지정될 때까지 개체가 경로를 따라 이동합니다. 한 번만 선택하면 개체는 첫 번째 지점에서 마지막 지점까지 경로를 따라 이동하고 해당 위치에 영원히 유지됩니다.", MoreMountains.Tools.MMInformationAttribute.InformationType.Info,false)]
		public CycleOptions CycleOption;

		[MMInformation("<b>경로</b>에 점을 추가한 다음(경로의 크기를 먼저 설정), 검사기를 사용하거나 장면 보기에서 직접 핸들을 움직여 점의 위치를 ​​지정하세요. 각 경로 요소에 대해 지연(초)을 지정할 수 있습니다. 점의 순서는 개체가 따르는 순서입니다.\n루프 경로의 경우 개체가 오름차순(1, 2, 3...) 또는 내림차순(마지막, 마지막-1, 마지막-2...) 순서입니다.", MoreMountains.Tools.MMInformationAttribute.InformationType.Info,false)]
		/// the initial movement direction : ascending > will go from the points 0 to 1, 2, etc ; descending > will go from the last point to last-1, last-2, etc
		[Tooltip("초기 이동 방향: 오름차순 > 지점 0에서 1, 2 등으로 이동합니다. 내림차순 > 마지막 지점에서 last-1, last-2 등으로 이동합니다.")]
		public MovementDirection LoopInitialMovementDirection = MovementDirection.Ascending;
		/// the points that make up the path the object will follow
		[Tooltip("물체가 따라갈 경로를 구성하는 점")]
		public List<MMPathMovementElement> PathElements;

		[Header("Path Alignment")] 
		/// whether to align the path on nothing, this object's rotation, or this object's parent's rotation
		[Tooltip("아무것도 없는 위치, 이 개체의 회전 또는 이 개체의 부모 회전에 경로를 정렬할지 여부")]
		public AlignmentModes AlignmentMode = AlignmentModes.None;
		
		[Header("Movement")]
		[MMInformation("경로가 크롤링되는 <b>속도</b>를 설정하고 움직임이 일정해야 하는지 아니면 완화되어야 하는지를 설정하세요.", MoreMountains.Tools.MMInformationAttribute.InformationType.Info,false)]
		/// the movement speed
		[Tooltip("the movement speed")]
		public float MovementSpeed = 1;
        /// 물체가 이동하는 현재 속도를 반환합니다.
        public Vector3 CurrentSpeed { get; protected set; }
		/// the movement type of the object
		[Tooltip("물체의 이동 유형")]
		public PossibleAccelerationType AccelerationType = PossibleAccelerationType.ConstantSpeed;
		/// the acceleration to apply to an object traveling between two points of the path.
		[Tooltip("경로의 두 지점 사이를 이동하는 객체에 적용되는 가속도입니다.")] 
		public AnimationCurve Acceleration = new AnimationCurve(new Keyframe(0,1f),new Keyframe(1f,0f));
		/// the chosen update mode (update, fixed update, late update)
		[Tooltip("선택한 업데이트 모드(업데이트, 고정 업데이트, 늦은 업데이트)")]
		public UpdateModes UpdateMode = UpdateModes.Update;

		[Header("Settings")]
		[MMInformation("<b>MinDistanceToGoal</b>은 경로의 지점에 (거의) 도달했는지 확인하는 데 사용됩니다. 여기에 있는 다른 2가지 설정은 디버그 전용이므로 변경하지 마세요.", MoreMountains.Tools.MMInformationAttribute.InformationType.Info,false)]
		/// the minimum distance to a point at which we'll arbitrarily decide the point's been reached
		[Tooltip("지점에 도달했다고 임의로 결정하는 지점까지의 최소 거리")]
		public float MinDistanceToGoal = .1f;
		/// the original position of the transform, hidden and shouldn't be accessed
		[Tooltip("변환의 원래 위치는 숨겨져 있으며 접근하면 안 됩니다.")]
		protected Vector3 _originalTransformPosition;
        /// 이것이 사실이라면 물체는 경로를 따라 움직일 수 있습니다
        public virtual bool CanMove { get; set; }
        
		protected bool _originalTransformPositionStatus = false;
		protected bool _active=false;
		protected IEnumerator<Vector3> _currentPoint;
		protected int _direction = 1;
		protected Vector3 _initialPosition;
		protected Vector3 _finalPosition;
		protected Vector3 _previousPoint = Vector3.zero;
		protected float _waiting=0;
		protected int _currentIndex;
		protected float _distanceToNextPoint;
		protected bool _endReached = false;
		protected Vector3 _positionLastFrame;
		protected Vector3 _vector3Zero = Vector3.zero;

		/// <summary>
		/// Initialization
		/// </summary>
		protected virtual void Awake ()
		{
			Initialization ();
		}

		/// <summary>
		/// On Start we store our initial position
		/// </summary>
		protected virtual void Start()
		{
			_originalTransformPosition = transform.position;
		}

		/// <summary>
		/// A public method you can call to reset the path
		/// </summary>
		public virtual void ResetPath()
		{
			Initialization();
			CanMove = false;
			transform.position = _originalTransformPosition;
		}

		/// <summary>
		/// Flag inits, initial movement determination, and object positioning
		/// </summary>
		protected virtual void Initialization()
		{
			// on Start, we set our active flag to true
			_active=true;
			_endReached = false;
			CanMove = true;

			// if the path is null we exit
			if(PathElements == null || PathElements.Count < 1)
			{
				return;
			}

			// we set our initial direction based on the settings
			if (LoopInitialMovementDirection == MovementDirection.Ascending)
			{
				_direction=1;
			}
			else
			{
				_direction=-1;
			}

			// we initialize our path enumerator
			_currentPoint = GetPathEnumerator();
			_previousPoint = _currentPoint.Current;
			_currentPoint.MoveNext();

			// initial positioning
			if (!_originalTransformPositionStatus)
			{
				_originalTransformPositionStatus = true;
				_originalTransformPosition = transform.position;
			}
			transform.position = PointPosition(_currentPoint.Current);
		}

		protected virtual void FixedUpdate()
		{
			if (UpdateMode == UpdateModes.FixedUpdate)
			{
				ExecuteUpdate();
			}
		}

		protected virtual void LateUpdate()
		{
			if (UpdateMode == UpdateModes.LateUpdate)
			{
				ExecuteUpdate();
			}
		}

		protected virtual void Update()
		{
			if (UpdateMode == UpdateModes.Update)
			{
				ExecuteUpdate();
			}
		}

		/// <summary>
		/// Override this to describe what happens when a point is reached
		/// </summary>
		protected virtual void PointReached()
		{

		}

		/// <summary>
		/// Override this to describe what happens when the end of the path is reached
		/// </summary>
		protected virtual void EndReached()
		{

		}

		/// <summary>
		/// On update we keep moving along the path
		/// </summary>
		protected virtual void ExecuteUpdate () 
		{
			// if the path is null we exit, if we only go once and have reached the end we exit, if we can't move we exit
			if(PathElements == null 
			   || PathElements.Count < 1
			   || _endReached
			   || !CanMove
			  )
			{
				CurrentSpeed = _vector3Zero;
				return;
			}

			Move ();

			_positionLastFrame = this.transform.position;
		}

		/// <summary>
		/// Moves the object and determines when a point has been reached
		/// </summary>
		protected virtual void Move()
		{
			// we wait until we can proceed
			_waiting -= Time.deltaTime;
			if (_waiting > 0)
			{
				CurrentSpeed = Vector3.zero;
				return;
			}

			// we store our initial position to compute the current speed at the end of the udpate	
			_initialPosition = transform.position;

			// we move our object
			MoveAlongThePath();

			// we decide if we've reached our next destination or not, if yes, we move our destination to the next point 
			_distanceToNextPoint = (transform.position - (PointPosition(_currentPoint.Current))).magnitude;
			if(_distanceToNextPoint < MinDistanceToGoal)
			{
				//we check if we need to wait
				if (PathElements.Count > _currentIndex)
				{
					_waiting = PathElements[_currentIndex].Delay;				 
				}
				PointReached();
				_previousPoint = _currentPoint.Current;
				_currentPoint.MoveNext();
			}

			// we determine the current speed		
			_finalPosition = this.transform.position;
			if (Time.deltaTime != 0f)
			{
				CurrentSpeed = (_finalPosition - _initialPosition) / Time.deltaTime;
			}

			if (_endReached) 
			{
				EndReached();
				CurrentSpeed = Vector3.zero;
			}
		}

		/// <summary>
		/// Moves the object along the path according to the specified movement type.
		/// </summary>
		public virtual void MoveAlongThePath()
		{
			switch (AccelerationType)
			{
				case PossibleAccelerationType.ConstantSpeed:
					transform.position = Vector3.MoveTowards (transform.position, PointPosition(_currentPoint.Current), Time.deltaTime * MovementSpeed);
					break;
				
				case PossibleAccelerationType.EaseOut:
					transform.position = Vector3.Lerp (transform.position, PointPosition(_currentPoint.Current), Time.deltaTime * MovementSpeed);
					break;

				case PossibleAccelerationType.AnimationCurve:
					float distanceBetweenPoints = Vector3.Distance (_previousPoint, _currentPoint.Current);

					if (distanceBetweenPoints <= 0)
					{
						return;
					}

					float remappedDistance = 1 - MMMaths.Remap (_distanceToNextPoint, 0f, distanceBetweenPoints, 0f, 1f);
					float speedFactor = Acceleration.Evaluate (remappedDistance);

					transform.position = Vector3.MoveTowards (transform.position, PointPosition(_currentPoint.Current), Time.deltaTime * MovementSpeed * speedFactor);
					break;
			}
		}

		/// <summary>
		/// Returns the current target point in the path
		/// </summary>
		/// <returns>The path enumerator.</returns>
		public virtual IEnumerator<Vector3> GetPathEnumerator()
		{

			// if the path is null we exit
			if(PathElements == null || PathElements.Count < 1)
			{
				yield break;
			}

			int index = 0;
			_currentIndex = index;
			while (true)
			{
				_currentIndex = index;
				yield return PathElements[index].PathElementPosition;
				
				if(PathElements.Count <= 1)
				{
					continue;
				}

				// if the path is looping
				switch(CycleOption)
				{
					case CycleOptions.Loop:
						index = index + _direction;
						if (index < 0)
						{
							index = PathElements.Count - 1;
						}
						else if (index > PathElements.Count - 1)
						{
							index = 0;
						}
						break;

					case CycleOptions.BackAndForth:
						if (index <= 0)
						{
							_direction = 1;
						}
						else if (index >= PathElements.Count - 1)
						{
							_direction = -1;
						}
						index = index + _direction;
						break;

					case CycleOptions.OnlyOnce:
						if (index <= 0)
						{
							_direction = 1;
						}
						else if (index >= PathElements.Count - 1)
						{
							_direction = 0;
							CurrentSpeed = Vector3.zero;
							_endReached = true;
						}
						index = index + _direction;
						break;
                    
					case CycleOptions.Random:
						int newIndex = index;
						if (PathElements.Count > 1)
						{
							while (newIndex == index)
							{
								newIndex = Random.Range(0, PathElements.Count);
							}    
						}
						index = newIndex;
						break;

					case CycleOptions.StopAtBounds:
						if (index <= 0)
						{
							if (_direction == -1)
							{
								CurrentSpeed = Vector3.zero;
								_endReached = true;
							}
							_direction = 1;
						}
						else if (index >= PathElements.Count - 1)
						{
							if (_direction == 1)
							{
								CurrentSpeed = Vector3.zero;
								_endReached = true;
							}
							_direction = -1;
						}
						index = index + _direction;
						break;
				}
			}
		}

		/// <summary>
		/// Call this method to force a change in direction at any time
		/// </summary>
		public virtual void ChangeDirection()
		{
			_direction = -_direction;
			_currentPoint.MoveNext();
		}

		/// <summary>
		/// On DrawGizmos, we draw lines to show the path the object will follow
		/// </summary>
		protected virtual void OnDrawGizmos()
		{	
			#if UNITY_EDITOR
			if (PathElements == null)
			{
				return;
			}

			if (PathElements.Count == 0)
			{
				return;
			}
							
			// if we haven't stored the object's original position yet, we do it
			if (_originalTransformPositionStatus == false)
			{
				_originalTransformPosition = this.transform.position;
				_originalTransformPositionStatus = true;
			}
			// if we're not in runtime mode and the transform has changed, we update our position
			if (transform.hasChanged && _active==false)
			{
				_originalTransformPosition = this.transform.position;
			}
			// for each point in the path
			for (int i=0;i<PathElements.Count;i++)
			{
				// we draw a green point 
				MMDebug.DrawGizmoPoint(PointPosition(i),0.2f,Color.green);

				// we draw a line towards the next point in the path
				if ((i+1)<PathElements.Count)
				{
					Gizmos.color=Color.white;
					Gizmos.DrawLine(PointPosition(i), PointPosition(i + 1));
				}
				// we draw a line from the first to the last point if we're looping
				if ( (i == PathElements.Count-1) && (CycleOption == CycleOptions.Loop) )
				{
					Gizmos.color=Color.white;
					Gizmos.DrawLine(PointPosition(0), PointPosition(i));
				}
			}

			// if the game is playing, we add a blue point to the destination, and a red point to the last visited point
			if (Application.isPlaying)
			{
				MMDebug.DrawGizmoPoint(PointPosition(_currentPoint.Current), 0.2f, Color.blue);
				MMDebug.DrawGizmoPoint(PointPosition(_previousPoint),0.2f,Color.red);
			}
			#endif
		}

		public virtual Vector3 PointPosition(int index)
		{
			return PointPosition(PathElements[index].PathElementPosition);
		}

		public virtual Vector3 PointPosition(Vector3 relativePointPosition)
		{
			switch (AlignmentMode)
			{
				case AlignmentModes.None:
					return _originalTransformPosition + relativePointPosition;
				case AlignmentModes.ThisRotation:
					return _originalTransformPosition + this.transform.rotation *  relativePointPosition;
				case AlignmentModes.ParentRotation:
					return _originalTransformPosition + this.transform.parent.rotation *  relativePointPosition;
			}
			return Vector3.zero;
		}

		/// <summary>
		/// Updates the original transform position.
		/// </summary>
		/// <param name="newOriginalTransformPosition">New original transform position.</param>
		public virtual void UpdateOriginalTransformPosition(Vector3 newOriginalTransformPosition)
		{
			_originalTransformPosition = newOriginalTransformPosition;
		}

		/// <summary>
		/// Gets the original transform position.
		/// </summary>
		/// <returns>The original transform position.</returns>
		public virtual Vector3 GetOriginalTransformPosition()
		{
			return _originalTransformPosition;
		}

		/// <summary>
		/// Sets the original transform position status.
		/// </summary>
		/// <param name="status">If set to <c>true</c> status.</param>
		public virtual void SetOriginalTransformPositionStatus(bool status)
		{
			_originalTransformPositionStatus = status;
		}

		/// <summary>
		/// Gets the original transform position status.
		/// </summary>
		/// <returns><c>true</c>, if original transform position status was gotten, <c>false</c> otherwise.</returns>
		public virtual bool GetOriginalTransformPositionStatus()
		{
			return _originalTransformPositionStatus ;
		}
	}
}