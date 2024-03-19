using UnityEngine;

namespace MoreMountains.TopDownEngine
{
	public enum DamageTypeModes { BaseDamage, TypedDamage }
    /// <summary>
    /// 손상 유형을 식별하기 위해 자산을 생성할 수 있는 스크립트 가능한 객체
    /// </summary>
    [CreateAssetMenu(menuName = "MoreMountains/TopDownEngine/DamageType", fileName = "DamageType")]
	public class DamageType : ScriptableObject
	{
	}    
}