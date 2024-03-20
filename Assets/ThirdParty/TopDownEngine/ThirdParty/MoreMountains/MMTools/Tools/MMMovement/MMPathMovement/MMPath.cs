using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MoreMountains.Tools;

namespace MoreMountains.Tools
{
	[System.Serializable]
    /// <summary>
    /// 이 클래스는 MMPath의 노드를 설명합니다.
    /// </summary>
    public class MMPathMovementElement
	{
		/// the point that make up the path the object will follow
		public Vector3 PathElementPosition;
		/// a delay (in seconds) associated to each node
		public float Delay;
	}

    /// <summary>
    /// 이 구성 요소를 개체에 추가하면 다른 구성 요소에서 사용할 수 있는 경로를 정의할 수 있습니다.
    /// </summary>
    [AddComponentMenu("More Mountains/Tools/Movement/MMPath")]
	public class MMPath : MonoBehaviour 
	{
		/// the possible cycle options
		public enum CycleOptions
		{
			BackAndForth,
			Loop,
			OnlyOnce
		}

		/// the possible movement directions
		public enum MovementDirection
		{
			Ascending,
			Descending
		}

		[Header("Path")]
		[MMInformation("여기에서 '<b>주기 옵션</b>'을 선택할 수 있습니다. 앞뒤로 개체가 끝까지 경로를 따라 갔다가 원래 지점으로 돌아갑니다. 루프를 선택하면 경로가 닫히고 달리 지정될 때까지 개체가 경로를 따라 이동합니다. 한 번만 선택하면 개체는 첫 번째 지점에서 마지막 지점까지 경로를 따라 이동하고 해당 위치에 영원히 유지됩니다.", MoreMountains.Tools.MMInformationAttribute.InformationType.Info,false)]
		public CycleOptions CycleOption;

		[MMInformation("<b>경로</b>에 점을 추가한 다음(경로의 크기를 먼저 설정), 검사기를 사용하거나 장면 보기에서 직접 핸들을 움직여 점의 위치를 ​​지정하세요. 각 경로 요소에 대해 지연(초)을 지정할 수 있습니다. 점의 순서는 개체가 따르는 순서입니다.\n루프 경로의 경우 개체가 오름차순(1, 2, 3...) 또는 내림차순(마지막, 마지막-1, 마지막-2...) 순서입니다.", MoreMountains.Tools.MMInformationAttribute.InformationType.Info,false)]
		/// the initial movement direction : ascending > will go from the points 0 to 1, 2, etc ; descending > will go from the last point to last-1, last-2, etc
		public MovementDirection LoopInitialMovementDirection = MovementDirection.Ascending;
		/// the points that make up the path the object will follow
		public List<MMPathMovementElement> PathElements;
		/// another MMPath that you can reference. If set, the reference MMPath's data will replace this MMPath's
		public MMPath ReferenceMMPath;
		/// if this is true, this object will move to the 0 position of the reference path
		public bool AbsoluteReferencePath = false;
		/// the minimum distance to a point at which we'll arbitrarily decide the point's been reached
		public float MinDistanceToGoal = .1f;

		[Header("Gizmos")] 
		public bool LockHandlesOnXAxis = false;
		public bool LockHandlesOnYAxis = false;
		public bool LockHandlesOnZAxis = false;
        
		/// the original position of the transform, hidden and shouldn't be accessed
		protected Vector3 _originalTransformPosition;
		/// internal flag, hidden and shouldn't be accessed
		protected bool _originalTransformPositionStatus=false;
		/// if this is true, the object can move along the path
		public virtual bool CanMove { get; set; }
		/// if this is true, this path has gone through its Initialization method
		public virtual bool Initialized { get; set; }

		public virtual int Direction => _direction;

		protected bool _active=false;
		protected IEnumerator<Vector3> _currentPoint;
		protected int _direction = 1;
		protected Vector3 _initialPosition;
		protected Vector3 _initialPositionThisFrame;
		protected Vector3 _finalPosition;
		protected Vector3 _previousPoint = Vector3.zero;
		protected int _currentIndex;
		protected float _distanceToNextPoint;
		protected bool _endReached = false;

		/// <summary>
		/// Initialization
		/// </summary>
		protected virtual void Start ()
		{
			if (!Initialized)
			{
				Initialization ();	
			}
		}

		/// <summary>
		/// Flag inits, initial movement determination, and object positioning
		/// </summary>
		public virtual void Initialization()
		{
			// on Start, we set our active flag to true
			_active=true;
			_endReached = false;
			CanMove = true;

			// we copy our reference if needed
			if ((ReferenceMMPath != null) && (ReferenceMMPath.PathElements != null || ReferenceMMPath.PathElements.Count > 0))
			{
				if (AbsoluteReferencePath)
				{
					this.transform.position = ReferenceMMPath.transform.position;
				}
				PathElements = ReferenceMMPath.PathElements;
			}

			// if the path is null we exit
			if (PathElements == null || PathElements.Count < 1)
			{
				return;
			}

			// if the first path element isn't at 0, we offset everything
			if (PathElements[0].PathElementPosition != Vector3.zero)
			{
				Vector3 path0Position = PathElements[0].PathElementPosition;
				this.transform.position += path0Position;
				
				foreach (MMPathMovementElement element in PathElements)
				{
					element.PathElementPosition -= path0Position;
				}
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
			_initialPosition = this.transform.position;
			
			_currentPoint = GetPathEnumerator();
			_previousPoint = _currentPoint.Current;
			_currentPoint.MoveNext();

			// initial positioning
			if (!_originalTransformPositionStatus)
			{
				_originalTransformPositionStatus = true;
				_originalTransformPosition = transform.position;
			}
			transform.position = _originalTransformPosition + _currentPoint.Current;
		}

		public int CurrentIndex()
		{
			return _currentIndex;
		}

		public Vector3 CurrentPoint()
		{
			return _initialPosition + _currentPoint.Current;
		}

		public Vector3 CurrentPositionRelative()
		{
			return _currentPoint.Current;
		}

		/// <summary>
		/// On update we keep moving along the path
		/// </summary>
		protected virtual void Update () 
		{
			// if the path is null we exit, if we only go once and have reached the end we exit, if we can't move we exit
			if(PathElements == null 
			   || PathElements.Count < 1
			   || _endReached
			   || !CanMove
			  )
			{
				return;
			}

			ComputePath ();
		}

		/// <summary>
		/// Moves the object and determines when a point has been reached
		/// </summary>
		protected virtual void ComputePath()
		{
			// we store our initial position to compute the current speed at the end of the udpate	
			_initialPositionThisFrame = this.transform.position;
            
			// we decide if we've reached our next destination or not, if yes, we move our destination to the next point 
			_distanceToNextPoint = (this.transform.position - (_originalTransformPosition + _currentPoint.Current)).magnitude;
			if(_distanceToNextPoint < MinDistanceToGoal)
			{
				_previousPoint = _currentPoint.Current;
				_currentPoint.MoveNext();
			}

			// we determine the current speed		
			_finalPosition = this.transform.position;
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
				if (CycleOption == CycleOptions.Loop)
				{
					index = index + _direction;
					if(index < 0)
					{
						index = PathElements.Count-1;
					}
					else if(index > PathElements.Count - 1)
					{
						index = 0;
					}
				}

				if (CycleOption == CycleOptions.BackAndForth)
				{
					if(index <= 0)
					{
						_direction = 1;
					}
					else if(index >= PathElements.Count - 1)
					{
						_direction = -1;
					}
					index = index + _direction;
				}

				if (CycleOption == CycleOptions.OnlyOnce)
				{
					if(index <= 0)
					{
						_direction = 1;
					}
					else if(index >= PathElements.Count - 1)
					{
						_direction = 0;
						_endReached = true;
					}
					index = index + _direction;
				}
			}
		}

		/// <summary>
		/// Call this method to force a change in direction at any time
		/// </summary>
		public virtual void ChangeDirection()
		{
			_direction = - _direction;
			_currentPoint.MoveNext();
		}

		/// <summary>
		/// On DrawGizmos, we draw lines to show the path the object will follow
		/// </summary>
		protected virtual void OnDrawGizmos()
		{	
			#if UNITY_EDITOR
			if (PathElements==null)
			{
				return;
			}

			if (PathElements.Count==0)
			{
				return;
			}
							
			// if we haven't stored the object's original position yet, we do it
			if (_originalTransformPositionStatus==false)
			{
				_originalTransformPosition = this.transform.position;
				_originalTransformPositionStatus=true;
			}
			// if we're not in runtime mode and the transform has changed, we update our position
			if (transform.hasChanged && (_active == false))
			{
				_originalTransformPosition = this.transform.position;
			}
			// for each point in the path
			for (int i=0;i<PathElements.Count;i++)
			{
				// we draw a green point 
				MMDebug.DrawGizmoPoint(_originalTransformPosition+PathElements[i].PathElementPosition,0.2f,Color.green);

				// we draw a line towards the next point in the path
				if ((i+1)<PathElements.Count)
				{
					Gizmos.color=Color.white;
					Gizmos.DrawLine(_originalTransformPosition+PathElements[i].PathElementPosition,_originalTransformPosition+PathElements[i+1].PathElementPosition);
				}
				// we draw a line from the first to the last point if we're looping
				if ( (i == PathElements.Count-1) && (CycleOption == CycleOptions.Loop) )
				{
					Gizmos.color=Color.white;
					Gizmos.DrawLine(_originalTransformPosition+PathElements[0].PathElementPosition,_originalTransformPosition+PathElements[i].PathElementPosition);
				}
			}

			// if the game is playing, we add a blue point to the destination, and a red point to the last visited point
			if (Application.isPlaying)
			{
				if (_currentPoint != null)
				{
					MMDebug.DrawGizmoPoint(_originalTransformPosition + _currentPoint.Current,0.2f,Color.blue);
					MMDebug.DrawGizmoPoint(_originalTransformPosition + _previousPoint,0.2f,Color.red);	
				}
			}
			#endif


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

		/// <summary>
		/// A data structure 
		/// </summary>
		[System.Serializable] public struct Data
		{
			public static Data ForwardLoopingPath(Vector3 ctr, Vector3[] vtx, float wait) 
				=> new Data()
				{
					Center = ctr, Offsets = vtx, Delay = wait,
					Cycle = CycleOptions.Loop, Direction = MovementDirection.Ascending
				};
			public static Data ForwardBackAndForthPath(Vector3 ctr, Vector3[] vtx, float wait) 
				=> new Data()
				{
					Center = ctr, Offsets = vtx, Delay = wait,
					Cycle = CycleOptions.BackAndForth, Direction = MovementDirection.Ascending
				};
			public static Data ForwardOnlyOncePath(Vector3 ctr, Vector3[] vtx, float wait) 
				=> new Data()
				{
					Center = ctr, Offsets = vtx, Delay = wait,
					Cycle = CycleOptions.OnlyOnce, Direction = MovementDirection.Ascending
				};

			public Vector3 Center;
			public Vector3[] Offsets;
			public float Delay;
			public CycleOptions Cycle;
			public MovementDirection Direction;
		}
		
		/// <summary>
		/// Replaces this MMPath's settings with the ones passed in parameters
		/// </summary>
		/// <param name="configuration"></param>
		public void SetPath(in Data configuration)
		{
			if (configuration.Offsets == null) return;

			// same as on Start, we set our active flag to true
			_active = true;
			_endReached = false;
			CanMove = true;

			PathElements = PathElements ?? new List<MMPathMovementElement>(configuration.Offsets.Length);
			PathElements.Clear();

			foreach (var offset in configuration.Offsets)
			{
				PathElements.Add(new MMPathMovementElement() {Delay = configuration.Delay, PathElementPosition = offset});
			}

			// if the path is null we exit
			if (PathElements == null || PathElements.Count < 1)
			{
				return;
			}

			CycleOption = configuration.Cycle;

			// we set our initial direction based on the settings
			if (configuration.Direction == MovementDirection.Ascending)
			{
				_direction = 1;
			}
			else
			{
				_direction = -1;
			}

			_initialPosition = configuration.Center;
			_originalTransformPosition = configuration.Center;
			_currentPoint = GetPathEnumerator();
			_previousPoint = _currentPoint.Current;
			_currentPoint.MoveNext();
		}
	}
}