using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 용도(단순, 다중 개체 풀러)에 따라 확장되고 생성자에 의해 인터페이스로 사용되는 기본 클래스입니다.
    /// 여전히 싱글톤 및 start() 초기화와 같은 일반적인 작업을 처리합니다.
    /// 이 클래스를 프리팹에 추가하지 마세요. 아무 일도 일어나지 않습니다. 대신 SimpleObjectPooler 또는 MultipleObjectPooler를 추가하세요.
    /// </summary>
    public abstract class MMObjectPooler : MonoBehaviour
	{
		/// singleton pattern
		public static MMObjectPooler Instance;
        /// 이것이 사실인 경우 풀은 동일한 이름을 가진 대기 풀을 찾으면 새 대기 풀을 만들지 않으려고 시도합니다.
        public bool MutualizeWaitingPools = false;
        /// 이것이 사실이라면 대기 중인 모든 활성 개체는 빈 게임 개체 아래에 다시 그룹화됩니다. 그렇지 않으면 계층 구조의 최상위 수준에 있게 됩니다.
        public bool NestWaitingPool = true;
        /// 이것이 사실이라면 대기 풀은 이 개체 아래에 중첩됩니다.
        [MMCondition("NestWaitingPool", true)] 
		public bool NestUnderThis = false;

        /// 이 개체는 풀링된 개체를 그룹화하는 데 사용됩니다.
        protected GameObject _waitingPool = null;
		protected MMObjectPool _objectPool;
		protected const int _initialPoolsListCapacity = 5;
		protected bool _onSceneLoadedRegistered = false;

        public static List<MMObjectPool> _pools = new List<MMObjectPool>(_initialPoolsListCapacity);

		/// <summary>
		/// Adds a pooler to the static list if needed
		/// </summary>
		/// <param name="pool"></param>
		public static void AddPool(MMObjectPool pool)
		{
			if (_pools == null)
			{
				_pools = new List<MMObjectPool>(_initialPoolsListCapacity);    
			}
			if (!_pools.Contains(pool))
			{
				_pools.Add(pool);
			}
		}

		/// <summary>
		/// Removes a pooler from the static list
		/// </summary>
		/// <param name="pool"></param>
		public static void RemovePool(MMObjectPool pool)
		{
			_pools?.Remove(pool);
		}

		/// <summary>
		/// On awake we fill our object pool
		/// </summary>
		protected virtual void Awake()
		{
			Instance = this;
            FillObjectPool();			
		}

        /// <summary>
        /// 대기 풀을 생성하거나 이미 사용 가능한 풀이 있는 경우 재사용을 시도합니다.
        /// </summary>
        protected virtual bool CreateWaitingPool()
		{
            if (!MutualizeWaitingPools)
			{
				// we create a container that will hold all the instances we create
				_waitingPool = new GameObject(DetermineObjectPoolName());
				SceneManager.MoveGameObjectToScene(_waitingPool, this.gameObject.scene);
				_objectPool = _waitingPool.AddComponent<MMObjectPool>();
				_objectPool.PooledGameObjects = new List<GameObject>();
				ApplyNesting();
				return true;
			}
			else
			{
				MMObjectPool objectPool = ExistingPool(DetermineObjectPoolName());
				if (objectPool != null)
				{
					_objectPool = objectPool;
					_waitingPool = objectPool.gameObject;
					return false;
				}
				else
				{
					_waitingPool = new GameObject(DetermineObjectPoolName());
					SceneManager.MoveGameObjectToScene(_waitingPool, this.gameObject.scene);
					_objectPool = _waitingPool.AddComponent<MMObjectPool>();
					_objectPool.PooledGameObjects = new List<GameObject>();
					ApplyNesting();
					AddPool(_objectPool);
					return true;
				}
			}
		}
        
		/// <summary>
		/// Looks for an existing pooler for the same object, returns it if found, returns null otherwise
		/// </summary>
		/// <param name="objectToPool"></param>
		/// <returns></returns>
		public virtual MMObjectPool ExistingPool(string poolName)
		{
			if (_pools == null)
			{
				_pools = new List<MMObjectPool>(_initialPoolsListCapacity);    
			}
			if (_pools.Count == 0)
			{
				var pools = FindObjectsOfType<MMObjectPool>();
				if (pools.Length > 0)
				{
					_pools.AddRange(pools);
				}
			}
			foreach (MMObjectPool pool in _pools)
			{
				if ((pool != null) && (pool.name == poolName)/* && (pool.gameObject.scene == this.gameObject.scene)*/)
				{
					return pool;
				}
			}
			return null;
		}

		/// <summary>
		/// If needed, nests the waiting pool under this object
		/// </summary>
		protected virtual void ApplyNesting()
		{
			if (NestWaitingPool && NestUnderThis && (_waitingPool != null))
			{
				_waitingPool.transform.SetParent(this.transform);
			}
		}

		/// <summary>
		/// Determines the name of the object pool.
		/// </summary>
		/// <returns>The object pool name.</returns>
		protected virtual string DetermineObjectPoolName()
		{
			return ("[ObjectPooler] " + this.name);	
		}

		/// <summary>
		/// Implement this method to fill the pool with objects
		/// </summary>
		public virtual void FillObjectPool()
		{
			return ;
		}

        /// <summary>
        /// 게임 객체를 반환하려면 이 메서드를 구현하세요.
        /// </summary>
        /// <returns>풀링된 게임 개체입니다.</returns>
        public virtual GameObject GetPooledGameObject()
		{
			return null;
		}

		/// <summary>
		/// Destroys the object pool
		/// </summary>
		public virtual void DestroyObjectPool()
		{
			if (_waitingPool != null)
			{
				Destroy(_waitingPool.gameObject);
			}
		}

		/// <summary>
		/// On enable we register to the scene loaded hook
		/// </summary>
		protected virtual void OnEnable()
		{
			if (!_onSceneLoadedRegistered)
			{
				SceneManager.sceneLoaded += OnSceneLoaded;    
			}
		}

		/// <summary>
		/// OnSceneLoaded we recreate 
		/// </summary>
		/// <param name="scene"></param>
		/// <param name="loadSceneMode"></param>
		private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
		{
			if (this == null)
			{
				return;
			}
			if ((_objectPool == null) || (_waitingPool == null))
			{
				if (this != null)
				{
					FillObjectPool();    
				}
			}
		}
        
		/// <summary>
		/// On Destroy we remove ourselves from the list of poolers 
		/// </summary>
		private void OnDestroy()
		{
			if ((_objectPool != null) && NestUnderThis)
			{
				RemovePool(_objectPool);    
			}

			if (_onSceneLoadedRegistered)
			{
				SceneManager.sceneLoaded -= OnSceneLoaded;
				_onSceneLoadedRegistered = false;
			}
		}
	}
}