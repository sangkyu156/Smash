using UnityEngine;

namespace  MoreMountains.Tools
{
    /// <summary>
    /// 게임 객체에 대한 MMLootTable 정의를 포함하는 스크립트 가능한 객체
    /// </summary>
    [CreateAssetMenu(fileName="LootDefinition",menuName="MoreMountains/Loot Definition")]
	public class MMLootTableGameObjectSO : ScriptableObject
	{
        /// 전리품 테이블
        public MMLootTableGameObject LootTable;

        /// 전리품 테이블에서 객체를 반환합니다.
        public virtual GameObject GetLoot()
		{
			return LootTable.GetLoot()?.Loot;
		}

        /// <summary>
        /// 전리품 테이블의 가중치를 계산합니다.
        /// </summary>
        public virtual void ComputeWeights()
		{
			LootTable.ComputeWeights();
		}
		
		protected virtual void OnValidate()
		{
			ComputeWeights();
		}
	}
}