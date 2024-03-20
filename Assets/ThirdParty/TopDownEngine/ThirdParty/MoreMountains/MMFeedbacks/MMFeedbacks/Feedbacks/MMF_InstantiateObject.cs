using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 이 피드백은 연관된 개체(일반적으로 VFX이지만 반드시 그런 것은 아님)를 인스턴스화하고 선택적으로 성능을 위해 개체 풀을 생성합니다.
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("이 피드백을 사용하면 피드백 위치(선택적 오프셋 포함)에서 해당 검사기에 지정된 개체를 인스턴스화할 수 있습니다. 성능을 절약하기 위해 초기화 시 개체 풀을 선택적으로(자동으로) 생성할 수도 있습니다. 이 경우 풀 크기(일반적으로 주어진 시간에 장면에 포함하려는 인스턴스화된 객체의 최대량)를 지정해야 합니다.")]
	[FeedbackPath("GameObject/Instantiate Object")]
	public class MMF_InstantiateObject : MMF_Feedback
	{
        /// 이 유형의 모든 피드백을 한 번에 비활성화하는 데 사용되는 정적 부울입니다.
        public static bool FeedbackTypeAuthorized = true;
        /// 인스턴스화된 객체를 배치하는 다양한 방법:
        /// - FeedbackPosition : 객체는 피드백 위치와 선택적 오프셋에서 인스턴스화됩니다.
        /// - Transform : 객체는 지정된 Transform 위치와 선택적 오프셋에서 인스턴스화됩니다.
        /// - WorldPosition: 객체는 지정된 월드 위치 벡터와 선택적 오프셋에서 인스턴스화됩니다.
        /// - Script  : 피드백 호출 시 매개변수로 전달되는 위치
        public enum PositionModes { FeedbackPosition, Transform, WorldPosition, Script }

		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.GameObjectColor; } }
		public override bool EvaluateRequiresSetup() { return (GameObjectToInstantiate == null); }
		public override string RequiredTargetText { get { return GameObjectToInstantiate != null ? GameObjectToInstantiate.name : "";  } }
		public override string RequiresSetupText { get { return "This feedback requires that a GameObjectToInstantiate be set to be able to work properly. You can set one below."; } }
		#endif

		[MMFInspectorGroup("Instantiate Object", true, 37, true)]
		/// the object to instantiate
		[Tooltip("the object to instantiate")]
		[FormerlySerializedAs("VfxToInstantiate")]
		public GameObject GameObjectToInstantiate;

		[MMFInspectorGroup("Position", true, 39)]
		/// the chosen way to position the object 
		[Tooltip("물체를 배치하기 위해 선택한 방법")]
		public PositionModes PositionMode = PositionModes.FeedbackPosition;
		/// the chosen way to position the object 
		[Tooltip("물체를 배치하기 위해 선택한 방법")]
		public bool AlsoApplyRotation = false;
		/// the chosen way to position the object 
		[Tooltip("물체를 배치하기 위해 선택한 방법")]
		public bool AlsoApplyScale = false;
		/// the transform at which to instantiate the object
		[Tooltip("객체를 인스턴스화하는 변환")]
		[MMFEnumCondition("PositionMode", (int)PositionModes.Transform)]
		public Transform TargetTransform;
		/// the transform at which to instantiate the object
		[Tooltip("객체를 인스턴스화하는 변환")]
		[MMFEnumCondition("PositionMode", (int)PositionModes.WorldPosition)]
		public Vector3 TargetPosition;
		/// the position offset at which to instantiate the object
		[Tooltip("객체를 인스턴스화할 위치 오프셋")]
		[FormerlySerializedAs("VfxPositionOffset")]
		public Vector3 PositionOffset;

		/// if this is true, instantiation position will be randomized between RandomizeMin and RandomizeMax 
		[Tooltip("이것이 사실이라면 인스턴스화 위치는 RandomizeMin과 RandomizeMax 사이에서 무작위화됩니다.")]
		public bool RandomizePosition = false;
		/// the minimum value we'll randomize our position with
		[Tooltip("위치를 무작위로 지정할 최소값")]
		[MMFCondition("RandomizePosition", true)]
		public Vector3 RandomizedPositionMin = Vector3.zero;
		/// the maximum value we'll randomize our position with
		[Tooltip("위치를 무작위로 지정할 최대값")]
		[MMFCondition("RandomizePosition", true)]
		public Vector3 RandomizedPositionMax = Vector3.one;

		[MMFInspectorGroup("Parent", true, 47)]
		/// if specified, the instantiated object (or the pool of objects) will be parented to this transform 
		[Tooltip("지정된 경우 인스턴스화된 객체(또는 객체 풀)는 이 변환의 상위 항목이 됩니다.")]
		public Transform ParentTransform;

		[MMFInspectorGroup("Object Pool", true, 40)]
		/// whether or not we should create automatically an object pool for this object
		[Tooltip("이 개체에 대한 개체 풀을 자동으로 생성해야 하는지 여부")]
		[FormerlySerializedAs("VfxCreateObjectPool")]
		public bool CreateObjectPool;
		/// the initial and planned size of this object pool
		[Tooltip("이 개체 풀의 초기 및 계획 크기")]
		[MMFCondition("CreateObjectPool", true)]
		[FormerlySerializedAs("VfxObjectPoolSize")]
		public int ObjectPoolSize = 5;
		/// whether or not to create a new pool even if one already exists for that same prefab
		[Tooltip("동일한 프리팹에 대해 이미 풀이 존재하는 경우에도 새 풀을 생성할지 여부")]
		[MMFCondition("CreateObjectPool", true)] 
		public bool MutualizePools = false;

		protected MMMiniObjectPooler _objectPooler; 
		protected GameObject _newGameObject;
		protected bool _poolCreatedOrFound = false;
		protected Vector3 _randomizedPosition = Vector3.zero;

		/// <summary>
		/// On init we create an object pool if needed
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);

			if (Active && CreateObjectPool && !_poolCreatedOrFound)
			{
				if (_objectPooler != null)
				{
					_objectPooler.DestroyObjectPool();
					owner.ProxyDestroy(_objectPooler.gameObject);
				}

				GameObject objectPoolGo = new GameObject();
				objectPoolGo.name = Owner.name+"_ObjectPooler";
				_objectPooler = objectPoolGo.AddComponent<MMMiniObjectPooler>();
				_objectPooler.GameObjectToPool = GameObjectToInstantiate;
				_objectPooler.PoolSize = ObjectPoolSize;
				if (ParentTransform != null)
				{
					_objectPooler.transform.SetParent(ParentTransform);
				}
				else
				{
					_objectPooler.transform.SetParent(Owner.transform);
				}
				_objectPooler.MutualizeWaitingPools = MutualizePools;
				_objectPooler.FillObjectPool();
				if ((Owner != null) && (objectPoolGo.transform.parent == null))
				{
					SceneManager.MoveGameObjectToScene(objectPoolGo, Owner.gameObject.scene);    
				}
				_poolCreatedOrFound = true;
			}
		}

		/// <summary>
		/// On Play we instantiate the specified object, either from the object pool or from scratch
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || (GameObjectToInstantiate == null))
			{
				return;
			}
            
			if (_objectPooler != null)
			{
				_newGameObject = _objectPooler.GetPooledGameObject();
				if (_newGameObject != null)
				{
					PositionObject(position);
					_newGameObject.SetActive(true);
				}
			}
			else
			{
				_newGameObject = GameObject.Instantiate(GameObjectToInstantiate) as GameObject;
				if (_newGameObject != null)
				{
					SceneManager.MoveGameObjectToScene(_newGameObject, Owner.gameObject.scene);
					PositionObject(position);    
				}
			}
		}

		protected virtual void PositionObject(Vector3 position)
		{
			_newGameObject.transform.position = GetPosition(position);
			if (AlsoApplyRotation)
			{
				_newGameObject.transform.rotation = GetRotation();    
			}
			if (AlsoApplyScale)
			{
				_newGameObject.transform.localScale = GetScale();    
			}
			if (!CreateObjectPool && (ParentTransform != null))
			{
				_newGameObject.transform.SetParent(ParentTransform);
			}
		}

		/// <summary>
		/// Gets the desired position of that particle system
		/// </summary>
		/// <param name="position"></param>
		/// <returns></returns>
		protected virtual Vector3 GetPosition(Vector3 position)
		{
			if (RandomizePosition)
			{
				_randomizedPosition.x = UnityEngine.Random.Range(RandomizedPositionMin.x, RandomizedPositionMax.x);
				_randomizedPosition.y = UnityEngine.Random.Range(RandomizedPositionMin.y, RandomizedPositionMax.y);
				_randomizedPosition.z = UnityEngine.Random.Range(RandomizedPositionMin.z, RandomizedPositionMax.z);
			}
	        
			switch (PositionMode)
			{
				case PositionModes.FeedbackPosition:
					return Owner.transform.position + PositionOffset + _randomizedPosition;
				case PositionModes.Transform:
					return TargetTransform.position + PositionOffset + _randomizedPosition;
				case PositionModes.WorldPosition:
					return TargetPosition + PositionOffset + _randomizedPosition;
				case PositionModes.Script:
					return position + PositionOffset + _randomizedPosition;
				default:
					return position + PositionOffset + _randomizedPosition;
			}
		}

        
		/// <summary>
		/// Gets the desired rotation of that particle system
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		protected virtual Quaternion GetRotation()
		{
			switch (PositionMode)
			{
				case PositionModes.FeedbackPosition:
					return Owner.transform.rotation;
				case PositionModes.Transform:
					return TargetTransform.rotation;
				case PositionModes.WorldPosition:
					return Quaternion.identity;
				case PositionModes.Script:
					return Owner.transform.rotation;
				default:
					return Owner.transform.rotation;
			}
		}

		/// <summary>
		/// Gets the desired scale of that particle system
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		protected virtual Vector3 GetScale()
		{
			switch (PositionMode)
			{
				case PositionModes.FeedbackPosition:
					return Owner.transform.localScale;
				case PositionModes.Transform:
					return TargetTransform.localScale;
				case PositionModes.WorldPosition:
					return Owner.transform.localScale;
				case PositionModes.Script:
					return Owner.transform.localScale;
				default:
					return Owner.transform.localScale;
			}
		}
	}
}