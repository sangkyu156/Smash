using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 코인매니저
    /// </summary>
    [AddComponentMenu("TopDown Engine/Items/Coin")]
	public class Coin : PickableItem
	{
		/// The amount of points to add when collected
		[Tooltip("적립 시 추가되는 포인트 금액")]
		public int PointsToAdd = 10;

        /// <summary>
        /// 무언가가 동전과 충돌할 때 트리거됩니다.
        /// </summary>
        /// <param name="collider">Other.</param>
        protected override void Pick(GameObject picker) 
		{
			// we send a new points event for the GameManager to catch (and other classes that may listen to it too)
			TopDownEnginePointEvent.Trigger(PointsMethods.Add, PointsToAdd);
		}
	}
}