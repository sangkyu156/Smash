using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace MoreMountains.Tools
{	
	[Serializable]
    /// <summary>
    /// 다중 개체 풀러 개체입니다.
    /// </summary>
    public class MMMultipleObjectPoolerObject
	{
		public GameObject GameObjectToPool;
		public int PoolSize;
		public bool PoolCanExpand = true;
		public bool Enabled = true;
	}

    /// <summary>
    /// 풀에서 개체를 가져올 수 있는 다양한 방법
    /// </summary>
    public enum MMPoolingMethods { OriginalOrder, OriginalOrderSequential, RandomBetweenObjects, RandomPoolSizeBased }

    /// <summary>
    /// 이 클래스를 사용하면 풀링할 다양한 개체의 풀을 가질 수 있습니다.
    /// </summary>
    [AddComponentMenu("More Mountains/Tools/Object Pool/MMMultipleObjectPooler")]
	public class MMMultipleObjectPooler : MMObjectPooler
	{
        /// 풀링할 개체 목록
        public List<MMMultipleObjectPoolerObject> Pool;
		[MMInformation("MultipleObjectPooler는 Spawner가 사용할 객체의 예비입니다. 요청을 받으면 선택한 풀링 방법에 따라 선택된 풀(이상적으로는 비활성 개체)에서 개체를 반환합니다.\n- OriginalOrder는 검사기에서 설정한 순서대로(위에서 위쪽으로) 개체를 생성합니다. 하단)\n- OriginalOrderSequential은 동일한 작업을 수행하지만 다음 객체로 이동하기 전에 각 풀을 비웁니다.\n- RandomBetweenObjects는 풀에서 객체 하나를 무작위로 선택하지만 풀 크기를 무시하고 각 객체는 객체를 얻을 수 있는 동일한 기회를 갖습니다. 선택됨\n- PoolSizeBased는 풀 크기 확률에 따라 풀에서 객체 하나를 무작위로 선택합니다(풀 크기가 클수록 선택될 확률이 높아집니다)'...", MoreMountains.Tools.MMInformationAttribute.InformationType.Info,false)]
        /// 선택한 풀링 방법
        public MMPoolingMethods PoolingMethod = MMPoolingMethods.RandomPoolSizeBased;
		[MMInformation("CanPoolSameObjectTwice를 false로 설정하면 풀러는 반복을 피하기 위해 동일한 개체가 두 번 풀링되는 것을 방지하려고 합니다. 이는 순서 풀링이 아닌 무작위 풀링 방법에만 영향을 미칩니다.", MoreMountains.Tools.MMInformationAttribute.InformationType.Info,false)]
        /// 동일한 객체를 연속으로 두 번 풀링할 수 있는지 여부입니다. CanPoolSameObjectTwice를 false로 설정하면 풀러는 반복을 피하기 위해 동일한 개체가 두 번 풀링되는 것을 방지하려고 합니다. 이는 순서 풀링이 아닌 무작위 풀링 방법에만 영향을 미칩니다.
        public bool CanPoolSameObjectTwice=true;
        /// 함께 사용하려는 모든 MMMultipleObjectPoolers에서 일치해야 하는 고유한 이름
        [MMCondition("MutualizeWaitingPools", true)]
		public string MutualizedPoolName = "";
		
		public List<MMMultipleObjectPooler> Owner { get; set; }
		private void OnDestroy() { Owner?.Remove(this); }

        /// 실제 개체 풀
        protected GameObject _lastPooledObject;
		protected int _currentIndex = 0;
		protected int _currentIndexCounter = 0;

        /// <summary>
        /// 개체 풀의 이름을 결정합니다.
        /// </summary>
        /// <returns>개체 풀 이름입니다.</returns>
        protected override string DetermineObjectPoolName()
		{
			if ((MutualizedPoolName == null) || (MutualizedPoolName == ""))
			{
				return ("[MultipleObjectPooler] " + this.name);	
			}
			else
			{
				return ("[MultipleObjectPooler] " + MutualizedPoolName);	
			}
		}

        /// <summary>
        /// 검사기에서 지정한 개체 수로 개체 풀을 채웁니다.
        /// </summary>
        public override void FillObjectPool()
		{
			if ((Pool == null) || (Pool.Count == 0))
			{
				return;
			}

            // 대기 풀을 만듭니다. 이미 존재하는 경우 아무것도 채울 필요가 없습니다.
            if (!CreateWaitingPool())
			{
				return;
			}

            // 풀에 항목이 하나만 있으면 CanPoolSameObjectTwice를 true로 설정합니다.
            if (Pool.Count <= 1)
			{
				CanPoolSameObjectTwice=true;
			}

			bool stillObjectsToPool;
			int[] poolSizes;

            // 우리가 원래 검사관 명령에 따라 합산한다면
            switch (PoolingMethod)
			{
				case MMPoolingMethods.OriginalOrder:
					stillObjectsToPool = true;
                    // 검사기에 영향을 주지 않도록 풀 크기를 임시 배열에 저장합니다.
                    poolSizes = new int[Pool.Count];
					for (int i = 0; i < Pool.Count; i++)
					{
						poolSizes[i] = Pool[i].PoolSize;
					}

                    // 인스펙터에 있는 순서대로 개체를 살펴보고 추가할 개체를 찾는 동안 풀을 채웁니다.
                    while (stillObjectsToPool)
					{
						stillObjectsToPool = false;
						for (int i = 0; i < Pool.Count; i++)
						{
							if (poolSizes[i] > 0)
							{
								AddOneObjectToThePool(Pool[i].GameObjectToPool);
								poolSizes[i]--;
								stillObjectsToPool = true;
							}			            
						}
					}
					break;
				case MMPoolingMethods.OriginalOrderSequential:
                    // 검사기에 영향을 주지 않도록 풀 크기를 임시 배열에 저장합니다.
                    foreach (MMMultipleObjectPoolerObject pooledGameObject in Pool)
					{
						for (int i = 0; i < pooledGameObject.PoolSize ; i++ )
						{
							AddOneObjectToThePool(pooledGameObject.GameObjectToPool);								
						}
					}				
					break;
				default:
					int k = 0;
                    // 인스펙터에 지정된 각 유형의 객체에 대해
                    foreach (MMMultipleObjectPoolerObject pooledGameObject in Pool)
					{
                        // 해당 유형의 개체에 대해 풀링할 개체 수가 지정되지 않은 경우 아무 작업도 수행하지 않고 종료합니다.
                        if (k > Pool.Count) { return; }

                        // 검사기에 지정된 대로 해당 유형의 객체 수를 하나씩 추가합니다.
                        for (int j = 0; j < Pool[k].PoolSize; j++)
						{
							AddOneObjectToThePool(pooledGameObject.GameObjectToPool);
						}
						k++;
					}
					break;
			}
		}

        /// <summary>
        /// 지정된 유형의 개체 하나를 개체 풀에 추가합니다.
        /// </summary>
        /// <returns>방금 추가된 개체입니다.</returns>
        /// <param name="typeOfObject">The type of object to add to the pool.</param>
        protected virtual GameObject AddOneObjectToThePool(GameObject typeOfObject)
		{
			if (typeOfObject == null)
			{
				return null;
			}

			bool initialStatus = typeOfObject.activeSelf;
			typeOfObject.SetActive(false);
			GameObject newGameObject = (GameObject)Instantiate(typeOfObject);
			typeOfObject.SetActive(initialStatus);
			SceneManager.MoveGameObjectToScene(newGameObject, this.gameObject.scene);
			if (NestWaitingPool)
			{
				newGameObject.transform.SetParent(_waitingPool.transform);	
			}
			newGameObject.name = typeOfObject.name;
			_objectPool.PooledGameObjects.Add(newGameObject);
			return newGameObject;
		}

        /// <summary>
        /// 풀에서 임의의 개체를 가져옵니다.
        /// </summary>
        /// <returns>풀링된 게임 개체입니다.</returns>
        public override GameObject GetPooledGameObject()
		{
			GameObject pooledGameObject;
			switch (PoolingMethod)
			{
				case MMPoolingMethods.OriginalOrder:
					pooledGameObject = GetPooledGameObjectOriginalOrder();
					break;
				case MMPoolingMethods.RandomPoolSizeBased:
					pooledGameObject =  GetPooledGameObjectPoolSizeBased();
					break;
				case MMPoolingMethods.RandomBetweenObjects:
					pooledGameObject =  GetPooledGameObjectRandomBetweenObjects();
					break;
				case MMPoolingMethods.OriginalOrderSequential:
					pooledGameObject =  GetPooledGameObjectOriginalOrderSequential();
					break;
				default:
					pooledGameObject = null;
					break;
			}
			if (pooledGameObject!=null)
			{
				_lastPooledObject = pooledGameObject;
			}
			else
			{	
				_lastPooledObject = null;
			}
			return pooledGameObject;
		}

        /// <summary>
        /// 목록이 설정된 순서에 따라 풀에서 게임 개체를 찾으려고 시도합니다(각각의 풀 크기에 관계없이 하나씩).
        /// </summary>
        /// <returns>풀링된 게임 개체의 원래 순서.</returns>
        protected virtual GameObject GetPooledGameObjectOriginalOrder()
		{
			int newIndex;
            // 목록의 끝에 도달하면 처음부터 다시 시작합니다.
            if (_currentIndex >= Pool.Count)
			{
				ResetCurrentIndex ();
			}

			MMMultipleObjectPoolerObject searchedObject = GetPoolObject(Pool[_currentIndex].GameObjectToPool);

			if (_currentIndex >= _objectPool.PooledGameObjects.Count) { return null; }
			if (!searchedObject.Enabled) { _currentIndex++; return null; }

            // 개체가 이미 활성화되어 있으면 다른 개체를 찾아야 합니다.
            if (_objectPool.PooledGameObjects[_currentIndex].gameObject.activeInHierarchy)
			{
				GameObject findObject = FindInactiveObject(_objectPool.PooledGameObjects[_currentIndex].gameObject.name,_objectPool.PooledGameObjects);
				if (findObject != null)
				{
					_currentIndex++;
					return findObject;
				}

                // 풀을 확장할 수 있으면 새 풀을 만듭니다.
                if (searchedObject.PoolCanExpand)
				{
					_currentIndex++;
					return AddOneObjectToThePool(searchedObject.GameObjectToPool);	
				}
				else
				{
                    // 확장할 수 없으면 아무것도 반환하지 않습니다.
                    return null;					
				}
			}
			else
			{
                // 객체가 비활성 상태이면 반환합니다.
                newIndex = _currentIndex;
				_currentIndex++;
				return _objectPool.PooledGameObjects[newIndex]; 
			}
		}

		protected int _currentCount = 0;

        /// <summary>
        /// 목록이 설정된 순서에 따라 풀에서 게임 개체를 찾으려고 시도합니다(각각의 풀 크기에 관계없이 하나씩).
        /// </summary>
        /// <returns>풀링된 게임 개체의 원래 순서입니다.</returns>
        protected virtual GameObject GetPooledGameObjectOriginalOrderSequential()
		{
            // 목록의 끝에 도달하면 처음부터 다시 시작합니다.
            if (_currentIndex >= Pool.Count)
			{
				_currentCount = 0;
				ResetCurrentIndex ();
			}

			MMMultipleObjectPoolerObject searchedObject = GetPoolObject(Pool[_currentIndex].GameObjectToPool);

			if (_currentIndex >= _objectPool.PooledGameObjects.Count) { return null; }
			if (!searchedObject.Enabled) { _currentIndex++; _currentCount = 0; return null; }


            // 개체가 이미 활성화되어 있으면 다른 개체를 찾아야 합니다.
            if (_objectPool.PooledGameObjects[_currentIndex].gameObject.activeInHierarchy)
			{
				GameObject findObject = FindInactiveObject(Pool[_currentIndex].GameObjectToPool.name, _objectPool.PooledGameObjects);
				if (findObject != null)
				{
					_currentCount++;
					OrderSequentialResetCounter(searchedObject);
					return findObject;
				}

                // 풀을 확장할 수 있으면 새 풀을 만듭니다.
                if (searchedObject.PoolCanExpand)
				{
					_currentCount++;
					OrderSequentialResetCounter(searchedObject);
					return AddOneObjectToThePool(searchedObject.GameObjectToPool);	
				}
				else
				{
                    // 확장할 수 없으면 아무것도 반환하지 않습니다.
                    _currentIndex++;
					_currentCount = 0;
					return null;					
				}
			}
			else
			{
                // 객체가 비활성 상태이면 반환합니다.
                _currentCount++;
				OrderSequentialResetCounter(searchedObject);
				return _objectPool.PooledGameObjects[_currentIndex]; 
			}
		}

		protected virtual void OrderSequentialResetCounter(MMMultipleObjectPoolerObject searchedObject)
		{
			if (_currentCount >= searchedObject.PoolSize)
			{
				_currentIndex++;
				_currentCount = 0;
			}
		}

        /// <summary>
        /// 풀 크기 확률에 따라 풀에서 개체 하나를 무작위로 선택합니다(풀 크기가 클수록 선택될 확률이 높아짐).
        /// </summary>
        /// <returns>풀링된 게임 개체 풀 크기는 기반입니다.</returns>
        protected virtual GameObject GetPooledGameObjectPoolSizeBased()
		{
            // 우리는 무작위 인덱스를 얻습니다
            int randomIndex = UnityEngine.Random.Range(0, _objectPool.PooledGameObjects.Count);

			int overflowCounter=0;

            // 해당 객체가 활성화되어 있는지 확인하고 그렇지 않은 경우 루프를 반복합니다.
            while (!PoolObjectEnabled(_objectPool.PooledGameObjects[randomIndex]) && overflowCounter < _objectPool.PooledGameObjects.Count)
			{
				randomIndex = UnityEngine.Random.Range(0, _objectPool.PooledGameObjects.Count);
				overflowCounter++;
			}
			if (!PoolObjectEnabled(_objectPool.PooledGameObjects[randomIndex]))
			{ 
				return null; 
			}

            // 동일한 객체를 두 번 풀링할 수 없으면 잠시 동안 루프를 반복하여 다른 객체를 얻으려고 합니다.
            overflowCounter = 0;
			while (!CanPoolSameObjectTwice 
			       && _objectPool.PooledGameObjects[randomIndex] == _lastPooledObject 
			       && overflowCounter < _objectPool.PooledGameObjects.Count)
			{
				randomIndex = UnityEngine.Random.Range(0, _objectPool.PooledGameObjects.Count);
				overflowCounter++;
			}

            //  우리가 선택한 항목이 활성화된 경우
            if (_objectPool.PooledGameObjects[randomIndex].gameObject.activeInHierarchy)
			{
                // 동일한 유형의 다른 비활성 개체를 찾으려고 합니다.
                GameObject pulledObject = FindInactiveObject(_objectPool.PooledGameObjects[randomIndex].gameObject.name,_objectPool.PooledGameObjects);
				if (pulledObject!=null)
				{
					return pulledObject;
				}
				else
				{
                    // 이 유형의 비활성 개체를 찾을 수 없으면 확장할 수 있는지 확인합니다.
                    MMMultipleObjectPoolerObject searchedObject = GetPoolObject(_objectPool.PooledGameObjects[randomIndex].gameObject);
					if (searchedObject==null)
					{
						return null; 
					}
                    // 이 개체에 대한 풀의 확장이 허용되는지 여부(궁금하신 경우 검사기에서 설정됩니다)
                    if (searchedObject.PoolCanExpand)
					{						
						return AddOneObjectToThePool(searchedObject.GameObjectToPool);						 	
					}
					else
					{
                        // 성장이 허용되지 않으면 아무것도 반환하지 않습니다.
                        return null;
					}
				}
			}
			else
			{
                // 풀이 비어 있지 않으면 찾은 임의의 개체를 반환합니다.
                return _objectPool.PooledGameObjects[randomIndex];   
			}
		}

        /// <summary>
        /// 풀에서 하나의 개체를 무작위로 가져오지만 풀 크기를 무시하면 각 개체가 선택될 확률이 동일합니다.
        /// </summary>
        /// <returns>개체 간에 무작위로 풀링된 게임 개체입니다.</returns>
        protected virtual GameObject GetPooledGameObjectRandomBetweenObjects()
		{
            // 원래 풀에 있는 개체 중 하나를 무작위로 선택합니다.
            int randomIndex = UnityEngine.Random.Range(0, Pool.Count);
			
			int overflowCounter=0;

            // 동일한 객체를 두 번 풀링할 수 없으면 잠시 동안 루프를 반복하여 다른 객체를 얻으려고 합니다.
            while (!CanPoolSameObjectTwice && Pool[randomIndex].GameObjectToPool == _lastPooledObject && overflowCounter < _objectPool.PooledGameObjects.Count )
			{
				randomIndex = UnityEngine.Random.Range(0, Pool.Count);
				overflowCounter++;
			}
			int originalRandomIndex = randomIndex+1;

			bool objectFound = false;

            // 반환할 개체를 찾지 못했고 다양한 개체 유형을 모두 살펴보지 않았지만 계속 진행합니다.
            overflowCounter = 0;
			while (!objectFound 
			       && randomIndex != originalRandomIndex 
			       && overflowCounter < _objectPool.PooledGameObjects.Count)
			{
                // 인덱스가 끝에 있으면 재설정합니다.
                if (randomIndex >= Pool.Count)
				{
					randomIndex=0;
				}

				if (!Pool[randomIndex].Enabled)
				{
					randomIndex++;
					overflowCounter++;
					continue;
				}

                // 풀에서 해당 유형의 비활성 개체를 찾으려고 합니다.
                GameObject newGameObject = FindInactiveObject(Pool[randomIndex].GameObjectToPool.name, _objectPool.PooledGameObjects);
				if (newGameObject!=null)
				{
					objectFound=true;
					return newGameObject;
				}
				else
				{
                    // 아무것도 없고 확장할 수 있으면 확장합니다.
                    if (Pool[randomIndex].PoolCanExpand)
					{
						return AddOneObjectToThePool(Pool[randomIndex].GameObjectToPool);	
					}
				}
				randomIndex++;
				overflowCounter++;
			}
			return null;
		}

		protected string _tempSearchedName;

        /// <summary>
        ///풀의 지정된 인덱스에 있는 유형의 개체를 가져옵니다.
        /// 이 다중 객체 풀러의 요점은 다양한 풀을 추상화하고 처리하는 것입니다.
        /// 선택한 모드에 따라 선택합니다. 다양한 풀 중에서 직접 선택할 계획이라면,
        /// 단순히 여러 개의 단일 객체 풀러를 갖는 것을 고려해보세요.
        /// </summary>
        /// <param name="index"></param>
        public virtual GameObject GetPooledGamObjectAtIndex(int index)
		{
			if ((index < 0) || (index >= Pool.Count))
			{
				return null;
			}

			_tempSearchedName = Pool[index].GameObjectToPool.name;
			return GetPooledGameObjectOfType(_tempSearchedName);
		}

        /// <summary>
        ///풀에서 지정된 이름의 개체를 가져옵니다.
        /// 이 다중 객체 풀러의 요점은 다양한 풀을 추상화하고 처리하는 것입니다.
        /// 선택한 모드에 따라 선택합니다. 다양한 풀 중에서 직접 선택할 계획이라면,
        /// 단순히 여러 개의 단일 객체 풀러를 갖는 것을 고려해보세요.
        /// </summary>
        /// <returns>유형의 풀링된 게임 개체입니다.</returns>
        /// <param name="type">Type.</param>
        public virtual GameObject GetPooledGameObjectOfType(string searchedName)
		{
			GameObject newObject = FindInactiveObject(searchedName,_objectPool.PooledGameObjects);

			if (newObject!=null)
			{
				return newObject;
			}
			else
			{
                // 객체를 반환하지 않았다면 풀이 비어 있다는 의미입니다(적어도 해당 특정 유형의 객체가 포함되어 있지 않다는 의미입니다).
                // 풀 확장이 허용되는 경우
                GameObject searchedObject = FindObject(searchedName,_objectPool.PooledGameObjects);
				if (searchedObject == null) 
				{
					return null;
				}

				if (GetPoolObject(FindObject(searchedName,_objectPool.PooledGameObjects)).PoolCanExpand)
				{
					return AddOneObjectToThePool(searchedObject);
				}
			}

            // 해당 객체에 대한 풀이 비어 있고 확장이 허용되지 않으면 아무것도 반환하지 않습니다.
            return null;
		}

        /// <summary>
        /// 이름을 기준으로 풀에서 비활성 개체를 찾습니다.
        /// 해당 이름의 비활성 객체가 풀에서 발견되지 않은 경우 null을 반환합니다.
        /// </summary>
        /// <returns>The inactive object.</returns>
        /// <param name="searchedName">Searched name.</param>
        protected virtual GameObject FindInactiveObject(string searchedName, List<GameObject> list)
		{
			for (int i = 0; i < list.Count; i++)
			{
                // 풀 안에서 요청된 유형과 일치하는 객체를 찾은 경우
                if (list[i].name.Equals(searchedName))
				{
                    // 해당 객체가 지금 비활성 상태인 경우
                    if (!list[i].gameObject.activeInHierarchy)
					{
                        // 우리는 그것을 반환
                        return list[i];
					}
				}            
			}
			return null;
		}

		protected virtual GameObject FindAnyInactiveObject(List<GameObject> list)
		{
			for (int i = 0; i < list.Count; i++)
			{
                // 해당 객체가 지금 비활성 상태인 경우
                if (!list[i].gameObject.activeInHierarchy)
				{
                    // 우리는 그것을 반환
                    return list[i];
				}                        
			}
			return null;
		}

        /// <summary>
        /// 활성 또는 비활성 이름을 기준으로 풀에서 개체를 찾습니다.
        /// 풀에 해당 이름의 개체가 없으면 null을 반환합니다.
        /// </summary>
        /// <returns>The object.</returns>
        /// <param name="searchedName">Searched name.</param>
        protected virtual GameObject FindObject(string searchedName,List<GameObject> list)
		{
			for (int i = 0; i < list.Count; i++)
			{
                // 풀 안에서 요청된 유형과 일치하는 객체를 찾은 경우
                if (list[i].name.Equals(searchedName))
				{
                    // 해당 객체가 지금 비활성 상태인 경우
                    return list[i];
				}            
			}
			return null;
		}

        /// <summary>
        /// GameObject를 기반으로 원래 풀에서 MultipleObjectPoolerObject를 반환합니다(존재하는 경우).
        /// 이는 이름 기반이라는 점에 유의하세요.
        /// </summary>
        /// <returns>The pool object.</returns>
        /// <param name="testedObject">Tested object.</param>
        protected virtual MMMultipleObjectPoolerObject GetPoolObject(GameObject testedObject)
		{
			if (testedObject==null)
			{
				return null;
			}
			int i=0;
			foreach(MMMultipleObjectPoolerObject poolerObject in Pool)
			{
				if (testedObject.name.Equals(poolerObject.GameObjectToPool.name))
				{
					return (poolerObject);
				}
				i++;
			}
			return null;
		}

		protected virtual bool PoolObjectEnabled(GameObject testedObject)
		{
			MMMultipleObjectPoolerObject searchedObject = GetPoolObject(testedObject);
			if (searchedObject != null)
			{
				return searchedObject.Enabled;
			}
			else
			{
				return false;
			}
		}

		public virtual void EnableObjects(string name,bool newStatus)
		{
			foreach(MMMultipleObjectPoolerObject poolerObject in Pool)
			{
				if (name.Equals(poolerObject.GameObjectToPool.name))
				{
					poolerObject.Enabled = newStatus;
				}
			}
		}

		public virtual void ResetCurrentIndex()
		{
			_currentIndex = 0;
			_currentIndexCounter = 0;
		}
	}
}