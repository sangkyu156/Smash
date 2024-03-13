using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace  MoreMountains.Tools
{
    /// <summary>
    /// 가중치 목록에서 개체를 무작위로 선택하는 데 사용할 수 있는 전리품 테이블 도우미
    /// 이 디자인 패턴은 2014년 Daniel Cook의 블로그에서 더 자세히 설명되었습니다.
    /// https://lostgarden.home.blog/2014/12/08/loot-drop-tables/
    ///
    /// 이 일반 LootTable은 각각 가중치가 부여된 약탈할 개체 목록을 정의합니다.
    /// 가중치는 특정 숫자에 추가될 필요가 없으며 서로 상대적입니다.
    /// ComputeWeights 메서드는 이러한 가중치를 기반으로 각 개체가 선택될 확률을 결정합니다.
    /// GetLoot 메서드는 테이블에서 무작위로 선택한 하나의 개체를 반환합니다.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="V"></typeparam>
    public class MMLootTable<T,V> where T:MMLoot<V>
	{
        /// 테이블에 의해 반환될 가능성이 있는 개체 목록
        [SerializeField]
		public List<T> ObjectsToLoot;

        /// 디버그 목적으로만 사용되는 가중치의 총량
        [Header("Debug")]
		[MMReadOnly]
		public float WeightsTotal;
        
		protected float _maximumWeightSoFar = 0f;
		protected bool _weightsComputed = false;

        /// <summary>
        /// 지정된 가중치를 기준으로 테이블의 각 개체에 대한 확률 백분율을 결정합니다.
        /// </summary>
        public virtual void ComputeWeights()
		{
			if (ObjectsToLoot == null)
			{
				return;
			}

			if (ObjectsToLoot.Count == 0)
			{
				return;
			}

			_maximumWeightSoFar = 0f;

			foreach(T lootDropItem in ObjectsToLoot)
			{
				if(lootDropItem.Weight >= 0f)
				{
					lootDropItem.RangeFrom = _maximumWeightSoFar;
					_maximumWeightSoFar += lootDropItem.Weight;	
					lootDropItem.RangeTo = _maximumWeightSoFar;
				} 
				else 
				{
					lootDropItem.Weight =  0f;						
				}
			}

			WeightsTotal = _maximumWeightSoFar;

			foreach(T lootDropItem in ObjectsToLoot)
			{
				lootDropItem.ChancePercentage = ((lootDropItem.Weight) / WeightsTotal) * 100;
			}

			_weightsComputed = true;
		}

        /// <summary>
        /// 테이블에서 무작위로 선택된 객체 하나를 반환합니다.
        /// </summary>
        /// <returns></returns>
        public virtual T GetLoot()
		{	
			if (ObjectsToLoot == null)
			{
				return null;
			}

			if (ObjectsToLoot.Count == 0)
			{
				return null;
			}

			if (!_weightsComputed)
			{
				ComputeWeights();
			}
            
			float index = Random.Range(0, WeightsTotal);
 
			foreach (T lootDropItem in ObjectsToLoot)
			{
				if ((index > lootDropItem.RangeFrom) && (index < lootDropItem.RangeTo))
				{
					return lootDropItem;
				}
			}	
            
			return null;
		}
	}

    /// <summary>
    /// GameObject에 대한 MMLootTable 구현
    /// </summary>
    [System.Serializable]
	public class MMLootTableGameObject : MMLootTable<MMLootGameObject, GameObject> { }

    /// <summary>
    /// 부동 소수점에 대한 MMLootTable 구현
    /// </summary>
    [System.Serializable]
	public class MMLootTableFloat : MMLootTable<MMLootFloat, float> { }

    /// <summary>
    /// 문자열에 대한 MMLootTable 구현
    /// </summary>
    [System.Serializable]
	public class MMLootTableString : MMLootTable<MMLootString, string> { } 
}