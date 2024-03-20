using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 무기의 발사를 방지하기 위해 무기에 대한 추가 조건을 정의하는 데 사용되는 추상 클래스입니다.
    /// </summary>
    public abstract class WeaponPreventShooting : TopDownMonoBehaviour
	{
		/// <summary>
		/// Override this method to define shooting conditions
		/// </summary>
		/// <returns></returns>
		public abstract bool ShootingAllowed();
	}
}