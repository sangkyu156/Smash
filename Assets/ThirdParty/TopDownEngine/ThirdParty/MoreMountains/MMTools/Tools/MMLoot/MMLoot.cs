using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace  MoreMountains.Tools
{
    /// <summary>
    /// MMLootTable의 내용을 정의하는 클래스
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MMLoot<T>
	{
		/// the object to return
		public T Loot;
        /// 테이블에 있는 특정 개체에 부여된 무게
        public float Weight = 1f;
        /// 이 개체가 약탈될 확률이 표시됩니다. ChancePercentages는 MMLootTable 클래스에 의해 계산됩니다.
        [MMReadOnly] 
		public float ChancePercentage;
        
		/// the computed low bound of this object's range
		public float RangeFrom { get; set; }
		/// the computed high bound of this object's range
		public float RangeTo { get; set; }
	}
    
    
	/// <summary>
	/// a MMLoot implementation for gameobjects
	/// </summary>
	[System.Serializable]
	public class MMLootGameObject : MMLoot<GameObject> { }
    
	/// <summary>
	/// a MMLoot implementation for strings
	/// </summary>
	[System.Serializable]
	public class MMLootString : MMLoot<string> { }
    
	/// <summary>
	/// a MMLoot implementation for floats
	/// </summary>
	[System.Serializable]
	public class MMLootFloat : MMLoot<float> { }
    
}