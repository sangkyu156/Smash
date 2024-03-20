using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    /// <summary>
    /// MMObservableTest 데모 장면에서 MMObservable이 어떻게 작동하는지 보여주기 위해 사용되는 테스트 클래스
    /// </summary>
    public class MMObservableDemoSubject : MonoBehaviour
	{
		/// a public float we expose, outputting the x position of our object
		public MMObservable<float> PositionX = new MMObservable<float>();

		/// <summary>
		/// On Update we update our x position
		/// </summary>
		protected virtual void Update()
		{
			PositionX.Value = this.transform.position.x;
		}
	}
}