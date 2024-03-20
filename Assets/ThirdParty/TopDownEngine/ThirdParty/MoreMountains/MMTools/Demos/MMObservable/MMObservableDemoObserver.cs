using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    /// <summary>
    /// MMObservableTest 데모 장면에서 MMObservable을 시연하는 데 사용되는 테스트 클래스
    /// </summary>
    public class MMObservableDemoObserver : MonoBehaviour
	{
		/// the subject to look at
		public MMObservableDemoSubject TargetSubject;    

		/// <summary>
		/// When the position changes, we move our object accordingly on the y axis
		/// </summary>
		protected virtual void OnPositionChange()
		{
			this.transform.position = this.transform.position.MMSetY(TargetSubject.PositionX.Value);
		}
        
		/// <summary>
		/// On enable we start listening for changes
		/// </summary>
		protected virtual void OnEnable()
		{
			TargetSubject.PositionX.OnValueChanged += OnPositionChange;
		}

		/// <summary>
		/// On enable we stop listening for changes
		/// </summary>
		protected virtual void OnDisable()
		{
			TargetSubject.PositionX.OnValueChanged -= OnPositionChange;
		}
	}
}