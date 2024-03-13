using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;
using Random = UnityEngine.Random;

namespace  MoreMountains.TopDownEngine
{
    /// <summary>
    /// 개체를 생성하기 위한 클래스(일반적으로 항목 선택기이지만 반드시 그런 것은 아님) 
	/// 생성은 언제든지 모든 스크립트에 의해 트리거될 수 있으며 손상이나 사망 시 전리품을 트리거하는 자동 후크가 함께 제공됩니다.
    /// </summary>
    public class Loot : TopDownMonoBehaviour
	{
        /// 전리품을 정의할 수 있는 가능한 모드
        public enum LootModes { Unique, LootTable, LootTableScriptableObject }

		[Header("Loot Mode")]
        /// 선택한 전리품 모드 :
        /// - unique : 단순한 물건
        /// - loot table : 이 Loot 객체에 특정한 LootTable
        /// - loot definition : LootTable 스크립트 가능 객체 (created by right click > Create > MoreMountains > TopDown Engine > Loot Definition
        /// 그런 다음 이 loot definition를 다른 전리품 개체에서 재사용할 수 있습니다.
        [Tooltip("선택한 전리품 모드 : - unique : 단순한 물건  - loot table : 이 Loot 객체에 특정한 LootTable - loot definition : LootTable 스크립트 가능 객체 (created by right click > Create > MoreMountains > TopDown Engine > Loot Definition. 그런 다음 이 loot definition를 다른 전리품 개체에서 재사용할 수 있습니다.")]
		public LootModes LootMode = LootModes.Unique;

        /// LootMode에 있을 때 약탈할 개체
        [Tooltip("LootMode에 있을 때 약탈할 개체")]
		[MMEnumCondition("LootMode", (int) LootModes.Unique)]
		public GameObject GameObjectToLoot;

        /// 스폰할 객체를 정의하는 전리품 테이블
        [Tooltip("스폰할 객체를 정의하는 전리품 테이블")]
		[MMEnumCondition("LootMode", (int) LootModes.LootTable)]
		public MMLootTableGameObject LootTable;

        /// 스폰할 객체를 정의하는 전리품 테이블 스크립트 가능 객체
        [Tooltip("스폰할 객체를 정의하는 전리품 테이블 스크립트 가능 객체")]
		[MMEnumCondition("LootMode", (int) LootModes.LootTableScriptableObject)]
		public MMLootTableGameObjectSO LootTableSO;

		[Header("Conditions")]
        /// 이것이 사실이라면 이 개체가 죽을 때 전리품이 발생합니다.
        [Tooltip("이것이 사실이라면 이 개체가 죽을 때 전리품이 발생합니다.")]
		public bool SpawnLootOnDeath = true;
        /// 이것이 사실이라면 이 물체가 피해를 입을 때 전리품이 발생합니다.
        [Tooltip("이것이 사실이라면 이 물체가 피해를 입을 때 전리품이 발생합니다.")]
		public bool SpawnLootOnDamage = false;
        
		[Header("Pooling")]
        /// 이것이 사실이라면 전리품이 모아질 것입니다
        [Tooltip("이것이 사실이라면 전리품이 모아질 것입니다")]
		public bool PoolLoot = false;
        /// 전리품 테이블의 각 개체에 대한 풀 크기를 결정합니다.
        [Tooltip("전리품 테이블의 각 개체에 대한 풀 크기를 결정합니다.")]
		[MMCondition("PoolLoot", true)]
		public int PoolSize = 20;
        /// 이 풀의 고유한 이름은 풀을 상호화하려면 동일한 전리품 테이블을 공유하는 모든 전리품 개체 간에 공통적이어야 합니다.
        [Tooltip("이 풀의 고유한 이름은 풀을 상호화하려면 동일한 전리품 테이블을 공유하는 모든 전리품 개체 간에 공통적이어야 합니다.")]
		[MMCondition("PoolLoot", true)]
		public string MutualizedPoolName = "";
        
		[Header("Spawn")]
        /// 이것이 거짓이면 생성이 발생하지 않습니다.
        [Tooltip("이것이 거짓이면 생성이 발생하지 않습니다.")]
		public bool CanSpawn = true;
        /// 전리품을 생성하기 전에 기다리는 지연(초)
        [Tooltip("전리품을 생성하기 전에 기다리는 지연(초)")]
		public float Delay = 0f;
        /// 생성할 객체의 최소 및 최대 수량
        [Tooltip("생성할 객체의 최소 및 최대 수량")]
		[MMVector("Min","Max")]
		public Vector2 Quantity = Vector2.one;
        /// 위치, 회전 및 크기 조절 개체는 다음 위치에 생성되어야 합니다.
        [Tooltip("위치, 회전 및 크기 조절 개체는 다음 위치에 생성되어야 합니다.")]
		public MMSpawnAroundProperties SpawnProperties;
        /// 이것이 사실이라면 전리품은 최대 수량으로 제한되며, 그 이상의 새로운 전리품 시도는 결과가 없습니다. 이것이 거짓이라면 전리품은 무제한이며 영원히 발생할 수 있습니다.
        [Tooltip("이것이 사실이라면 전리품은 최대 수량으로 제한되며, 그 이상의 새로운 전리품 시도는 결과가 없습니다. 이것이 거짓이라면 전리품은 무제한이며 영원히 발생할 수 있습니다.")]
		public bool LimitedLootQuantity = true;
        /// 이 전리품 개체에서 약탈할 수 있는 개체의 최대 수량
        [Tooltip("이 전리품 개체에서 약탈할 수 있는 개체의 최대 수량")]
		[MMCondition("LimitedLootQuantity", true)]
		public int MaximumQuantity = 100;
        /// 디버그 목적으로 표시되는 이 전리품 개체에서 약탈할 수 있는 개체의 남은 수량입니다.
        [Tooltip("디버그 목적으로 표시되는 이 전리품 개체에서 약탈할 수 있는 개체의 남은 수량입니다.")]
		[MMReadOnly]
		public int RemainingQuantity = 100;

		[Header("Collisions")]
        /// 생성된 객체가 장애물을 피하려고 시도해야 하는지 여부
        [Tooltip("생성된 객체가 장애물을 피하려고 시도해야 하는지 여부")]
		public bool AvoidObstacles = false;
        /// 충돌 감지가 작동할 수 있는 가능한 모드
        public enum DimensionModes { TwoD, ThreeD}
        /// 충돌 감지가 2D에서 발생해야 하는지 아니면 3D에서 발생해야 하는지 여부
        [Tooltip("충돌 감지가 2D에서 발생해야 하는지 아니면 3D에서 발생해야 하는지 여부")]
		[MMCondition("AvoidObstacles", true)]
		public DimensionModes DimensionMode = DimensionModes.TwoD;
        /// 생성된 객체가 충돌해서는 안 되는 레이어가 포함된 레이어 마스크
        [Tooltip("생성된 객체가 충돌해서는 안 되는 레이어가 포함된 레이어 마스크")]
		[MMCondition("AvoidObstacles", true)]
		public LayerMask AvoidObstaclesLayerMask = LayerManager.ObstaclesLayerMask;
        /// 장애물이 발견되어서는 안 되는 물체 주위의 반경
        [Tooltip("장애물이 발견되어서는 안 되는 물체 주위의 반경")]
		[MMCondition("AvoidObstacles", true)]
		public float AvoidRadius = 0.25f;
        /// 마지막 전리품이 장애물 내에 있는 경우 스크립트가 전리품을 위한 다른 위치를 찾으려고 시도해야 하는 횟수입니다. 더 많은 시도: 더 나은 결과, 더 높은 비용
        [Tooltip("마지막 전리품이 장애물 내에 있는 경우 스크립트가 전리품을 위한 다른 위치를 찾으려고 시도해야 하는 횟수입니다. 더 많은 시도: 더 나은 결과, 더 높은 비용")]
		[MMCondition("AvoidObstacles", true)]
		public int MaxAvoidAttempts = 5;
        
		[Header("Feedback")]
        /// 전리품을 생성할 때 재생할 MMFeedback입니다. 피드백은 하나만 재생됩니다. 항목별로 하나씩 원하는 경우 항목 자체에 배치하고 개체가 인스턴스화될 때 재생되도록 하는 것이 가장 좋습니다. 
        [Tooltip("전리품을 생성할 때 재생할 MMFeedback입니다. 피드백은 하나만 재생됩니다. 항목별로 하나씩 원하는 경우 항목 자체에 배치하고 개체가 인스턴스화될 때 재생되도록 하는 것이 가장 좋습니다.")]
		public MMFeedbacks LootFeedback;

		[Header("Debug")]
        /// 이것이 사실이라면, 전리품이 생성되는 모양을 보여주기 위해 기즈모가 그려질 것입니다.
        [Tooltip("이것이 사실이라면, 전리품이 생성되는 모양을 보여주기 위해 기즈모가 그려질 것입니다.")]
		public bool DrawGizmos = false;
        /// 그릴 기즈모의 양
        [Tooltip("그릴 기즈모의 양")]
		public int GizmosQuantity = 1000;
        /// 기즈모를 그려야 하는 색상
        [Tooltip("기즈모를 그려야 하는 색상")]
		public Color GizmosColor = MMColors.LightGray;
        /// 기즈모를 그릴 크기
        [Tooltip("기즈모를 그릴 크기")]
		public float GimosSize = 1f;
        /// 전리품을 실행하는 데 사용되는 디버그 버튼
        [Tooltip("전리품을 실행하는 데 사용되는 디버그 버튼")]
		[MMInspectorButton("SpawnLootDebug")] 
		public bool SpawnLootButton;
        
		public static List<MMSimpleObjectPooler> SimplePoolers = new List<MMSimpleObjectPooler>();
		public static List<MMMultipleObjectPooler> MultiplePoolers = new List<MMMultipleObjectPooler>();

		protected Health _health;
		protected GameObject _objectToSpawn;
		protected GameObject _spawnedObject;
		protected Vector3 _raycastOrigin;
		protected RaycastHit2D _raycastHit2D;
		protected Collider[] _overlapBox;
		protected MMSimpleObjectPooler _simplePooler;
		protected MMMultipleObjectPooler _multipleObjectPooler;

        /// <summary>
        /// Awake에서는 건강 구성 요소가 있으면 가져오고 전리품 테이블을 초기화합니다.
        /// </summary>
        protected virtual void Awake()
		{
			_health = this.gameObject.GetComponentInParent<Health>();
			InitializeLootTable();
			InitializePools();
			ResetRemainingQuantity();
		}

        /// <summary>
        /// 남은 수량을 최대 수량으로 재설정합니다.
        /// </summary>
        public virtual void ResetRemainingQuantity()
		{
			RemainingQuantity = MaximumQuantity;
		}

        /// <summary>
        /// 연관된 전리품 테이블의 가중치를 계산합니다.
        /// </summary>
        public virtual void InitializeLootTable()
		{
			switch (LootMode)
			{
				case LootModes.LootTableScriptableObject:
					if (LootTableSO != null)
					{
						LootTableSO.ComputeWeights();
					}
					break;
				case LootModes.LootTable:
					LootTable.ComputeWeights();
					break;
			}
		}

		protected virtual void InitializePools()
		{
			if (!PoolLoot)
			{
				return;
			}

			switch (LootMode)
			{
				case LootModes.Unique:
					_simplePooler = FindSimplePooler();
					break;
				case LootModes.LootTable:
					_multipleObjectPooler = FindMultiplePooler();
					break;
				case LootModes.LootTableScriptableObject:
					_multipleObjectPooler = FindMultiplePooler();
					break;
			}
		}

		protected virtual MMSimpleObjectPooler FindSimplePooler()
		{
			foreach (MMSimpleObjectPooler simplePooler in SimplePoolers)
			{
				if (simplePooler.GameObjectToPool == GameObjectToLoot)
				{
					return simplePooler;
				}
			}
            // 찾지 못한 경우 새로 만듭니다.
            GameObject newObject = new GameObject("[MMSimpleObjectPooler] "+GameObjectToLoot.name);
			MMSimpleObjectPooler pooler = newObject.AddComponent<MMSimpleObjectPooler>();
			pooler.GameObjectToPool = GameObjectToLoot;
			pooler.PoolSize = PoolSize;
			pooler.NestUnderThis = true;
			pooler.FillObjectPool();            
			pooler.Owner = SimplePoolers;
			SimplePoolers.Add(pooler);
			return pooler;
		}
        
		protected virtual MMMultipleObjectPooler FindMultiplePooler()
		{
			foreach (MMMultipleObjectPooler multiplePooler in MultiplePoolers)
			{
				if ((multiplePooler != null) && (multiplePooler.MutualizedPoolName == MutualizedPoolName)) 
				{
					return multiplePooler;
				}
			}
            // 찾지 못한 경우 새로 만듭니다.
            GameObject newObject = new GameObject("[MMMultipleObjectPooler] "+MutualizedPoolName);
			MMMultipleObjectPooler pooler = newObject.AddComponent<MMMultipleObjectPooler>();
			pooler.MutualizeWaitingPools = true;
			pooler.MutualizedPoolName = MutualizedPoolName;
			pooler.NestUnderThis = true;
			pooler.Pool = new List<MMMultipleObjectPoolerObject>();
			if (LootMode == LootModes.LootTable)
			{
				foreach (MMLootGameObject loot in LootTable.ObjectsToLoot)
				{
					MMMultipleObjectPoolerObject objectToPool = new MMMultipleObjectPoolerObject();
					objectToPool.PoolSize = PoolSize * (int)loot.Weight;
					objectToPool.GameObjectToPool = loot.Loot;
					pooler.Pool.Add(objectToPool);
				}
			}
			else if (LootMode == LootModes.LootTableScriptableObject)
			{
				foreach (MMLootGameObject loot in LootTableSO.LootTable.ObjectsToLoot)
				{
					MMMultipleObjectPoolerObject objectToPool = new MMMultipleObjectPoolerObject
					{
						PoolSize = PoolSize * (int)loot.Weight,
						GameObjectToPool = loot.Loot
					};
					pooler.Pool.Add(objectToPool);
				}
			}
			pooler.FillObjectPool();
			pooler.Owner = MultiplePoolers;
			MultiplePoolers.Add(pooler);
			return pooler;
		}

        /// <summary>
        /// 이 방법은 지연을 적용한 후 지정된 전리품을 생성합니다(있는 경우).
        /// </summary>
        public virtual void SpawnLoot()
		{
			if (!CanSpawn)
			{
				return;
			}
			StartCoroutine(SpawnLootCo());
		}

        /// <summary>
        /// 검사기 버튼에 의해 호출되는 디버그 메서드
        /// </summary>
        protected virtual void SpawnLootDebug()
		{
			if (!Application.isPlaying)
			{
				Debug.LogWarning("이 디버그 버튼은 플레이 모드에서만 사용할 수 있습니다.");
				return;
			}

			SpawnLoot();
		}

        /// <summary>
        /// 지연 후 전리품을 생성하는 데 사용되는 코루틴
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator SpawnLootCo()
		{
			yield return MMCoroutine.WaitFor(Delay);
			int randomQuantity = Random.Range((int)Quantity.x, (int)Quantity.y + 1);
			for (int i = 0; i < randomQuantity; i++)
			{
				SpawnOneLoot();
			}
			LootFeedback?.PlayFeedbacks();
		}

		protected virtual void Spawn(GameObject gameObjectToSpawn)
		{
			if (PoolLoot)
			{
				switch (LootMode)
				{
					case LootModes.Unique:
						_spawnedObject = _simplePooler.GetPooledGameObject();
						break;
					case LootModes.LootTable: case LootModes.LootTableScriptableObject:
						_spawnedObject = _multipleObjectPooler.GetPooledGameObject();
						break;
				}
			}
			else
			{
				_spawnedObject = Instantiate(_objectToSpawn);    
			}
		}

        /// <summary>
        /// 정의된 수량에 관계없이 지연 없이 단일 전리품 개체를 생성합니다.
        /// </summary>
        public virtual void SpawnOneLoot()
		{
			_objectToSpawn = GetObject();

			if (_objectToSpawn == null)
			{
				return;
			}

			if (LimitedLootQuantity && (RemainingQuantity <= 0))
			{
				return;
			}

			Spawn(_objectToSpawn);

			if (AvoidObstacles)
			{
				bool placementOK = false;
				int amountOfAttempts = 0;
				while (!placementOK && (amountOfAttempts < MaxAvoidAttempts))
				{
					MMSpawnAround.ApplySpawnAroundProperties(_spawnedObject, SpawnProperties, this.transform.position);
                    
					if (DimensionMode == DimensionModes.TwoD)
					{
						_raycastOrigin = _spawnedObject.transform.position;
						_raycastHit2D = Physics2D.BoxCast(_raycastOrigin + Vector3.right * AvoidRadius, AvoidRadius * Vector2.one, 0f, Vector2.left, AvoidRadius, AvoidObstaclesLayerMask);
						if (_raycastHit2D.collider == null)
						{
							placementOK = true;
						}
						else
						{
							amountOfAttempts++;
						}
					}
					else
					{
						_raycastOrigin = _spawnedObject.transform.position;
						_overlapBox = Physics.OverlapBox(_raycastOrigin, Vector3.one * AvoidRadius, Quaternion.identity, AvoidObstaclesLayerMask);
                        
						if (_overlapBox.Length == 0)
						{
							placementOK = true;
						}
						else
						{
							amountOfAttempts++;
						}
					}
				}
			}
			else
			{
				MMSpawnAround.ApplySpawnAroundProperties(_spawnedObject, SpawnProperties, this.transform.position);    
			}
			if (_spawnedObject != null)
			{
				_spawnedObject.gameObject.SetActive(true);
			}
			_spawnedObject.SendMessage("OnInstantiate", SendMessageOptions.DontRequireReceiver);

			if (LimitedLootQuantity)
			{
				RemainingQuantity--;	
			}
		}

        /// <summary>
        /// 생성되어야 하는 개체를 가져옵니다.
        /// </summary>
        /// <returns></returns>
        protected virtual GameObject GetObject()
		{
			_objectToSpawn = null;
			switch (LootMode)
			{
				case LootModes.Unique:
					_objectToSpawn = GameObjectToLoot;
					break;
				case LootModes.LootTableScriptableObject:
					if (LootTableSO == null)
					{
						_objectToSpawn = null;
						break;
					}
					_objectToSpawn = LootTableSO.GetLoot();
					break;
				case LootModes.LootTable:
					_objectToSpawn = LootTable.GetLoot()?.Loot;
					break;
			}

			return _objectToSpawn;
		}

        /// <summary>
        /// 적중 시 필요한 경우 전리품을 생성합니다.
        /// </summary>
        protected virtual void OnHit()
		{
			if (!SpawnLootOnDamage)
			{
				return;
			}

			SpawnLoot();
		}

        /// <summary>
        /// 사망 시 필요한 경우 전리품을 생성합니다.
        /// </summary>
        protected virtual void OnDeath()
		{
			if (!SpawnLootOnDeath)
			{
				return;
			}

			SpawnLoot();
		}

        /// <summary>
        /// OnEnable 우리는 죽음을 듣기 시작하고 필요하면 공격합니다
        /// </summary>
        protected virtual void OnEnable()
		{
			if (_health != null)
			{
				_health.OnDeath += OnDeath;
				_health.OnHit += OnHit;
			}
		}

        /// <summary>
        /// OnDisable 우리는 죽음에 대한 듣기를 중단하고 필요한 경우 공격합니다.
        /// </summary>
        protected virtual void OnDisable()
		{
			if (_health != null)
			{
				_health.OnDeath -= OnDeath;
				_health.OnHit -= OnHit;
			}
		}

        /// <summary>
        /// OnDrawGizmos, 약탈 시 개체가 생성되는 모양을 표시합니다.
        /// </summary>
        protected virtual void OnDrawGizmos()
		{
			if (DrawGizmos)
			{
				MMSpawnAround.DrawGizmos(SpawnProperties, this.transform.position, GizmosQuantity, GimosSize, GizmosColor);    
			}
		}

	}
}