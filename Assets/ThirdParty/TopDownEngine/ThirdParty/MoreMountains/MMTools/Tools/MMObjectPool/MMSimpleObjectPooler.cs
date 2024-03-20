using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 단일 유형의 객체를 출력하는 간단한 객체 풀
    /// </summary>
    [AddComponentMenu("More Mountains/Tools/Object Pool/MMSimpleObjectPooler")]
	public class MMSimpleObjectPooler : MMObjectPooler 
	{
		/// the game object we'll instantiate 
		public GameObject GameObjectToPool;
		/// the number of objects we'll add to the pool
		public int PoolSize = 20;
		/// if true, the pool will automatically add objects to the itself if needed
		public bool PoolCanExpand = true;
	    
		public List<MMSimpleObjectPooler> Owner { get; set; }
		private void OnDestroy() { Owner?.Remove(this); }

        /// <summary>
        /// 인스펙터에서 지정한 게임 객체 유형으로 객체 풀을 채웁니다.
        /// </summary>
        public override void FillObjectPool()
		{
			if (GameObjectToPool == null)
			{
				return;
			}

			// if we've already created a pool, we exit
			if ((_objectPool != null) && (_objectPool.PooledGameObjects.Count > PoolSize))
			{
				return;
			}

			CreateWaitingPool ();

			int objectsToSpawn = PoolSize;

			if (_objectPool != null)
			{
				objectsToSpawn -= _objectPool.PooledGameObjects.Count;
			}

			// we add to the pool the specified number of objects
			for (int i = 0; i < objectsToSpawn; i++)
			{
				AddOneObjectToThePool ();
			}
		}

		/// <summary>
		/// Determines the name of the object pool.
		/// </summary>
		/// <returns>The object pool name.</returns>
		protected override string DetermineObjectPoolName()
		{
			return ("[SimpleObjectPooler] " + GameObjectToPool.name);	
		}
	    	
		/// <summary>
		/// This method returns one inactive object from the pool
		/// </summary>
		/// <returns>The pooled game object.</returns>
		public override GameObject GetPooledGameObject()
		{
            // we go through the pool looking for an inactive object
            for (int i=0; i< _objectPool.PooledGameObjects.Count; i++)
			{
				if (!_objectPool.PooledGameObjects[i].gameObject.activeInHierarchy)
				{
                    // if we find one, we return it
                    return _objectPool.PooledGameObjects[i];
				}
			}
			// if we haven't found an inactive object (the pool is empty), and if we can extend it, we add one new object to the pool, and return it		
			if (PoolCanExpand)
			{
				return AddOneObjectToThePool();
			}
			// if the pool is empty and can't grow, we return nothing.
			return null;
		}
		
		/// <summary>
		/// Adds one object of the specified type (in the inspector) to the pool.
		/// </summary>
		/// <returns>The one object to the pool.</returns>
		protected virtual GameObject AddOneObjectToThePool()
		{
            if (GameObjectToPool == null)
			{
				Debug.LogWarning("The "+gameObject.name+" ObjectPooler doesn't have any GameObjectToPool defined.", gameObject);
				return null;
			}

			bool initialStatus = GameObjectToPool.activeSelf;
			GameObjectToPool.SetActive(false);
			GameObject newGameObject = (GameObject)Instantiate(GameObjectToPool);
			GameObjectToPool.SetActive(initialStatus);
			SceneManager.MoveGameObjectToScene(newGameObject, this.gameObject.scene);
			if (NestWaitingPool)
			{
				newGameObject.transform.SetParent(_waitingPool.transform);	
			}
			newGameObject.name = GameObjectToPool.name + "-" + _objectPool.PooledGameObjects.Count;

			_objectPool.PooledGameObjects.Add(newGameObject);

			return newGameObject;
		}
	}
}