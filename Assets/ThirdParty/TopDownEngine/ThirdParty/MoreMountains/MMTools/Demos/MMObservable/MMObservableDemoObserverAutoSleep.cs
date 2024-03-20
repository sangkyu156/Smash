using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    /// <summary>
    /// MMObservableDemo 장면에서 MMObservable 패턴을 시연하는 데 사용되는 테스트 클래스
    /// 이것은 Awake에서 자체적으로 비활성화되고 비활성화된 경우에도 변경 사항을 수동적으로 수신합니다.
    /// </summary>
    public class MMObservableDemoObserverAutoSleep : MonoBehaviour
	{
		public MMObservableDemoSubject TargetSubject;

		protected virtual void OnSpeedChange()
		{
			this.transform.position = this.transform.position.MMSetY(TargetSubject.PositionX.Value);
		}

		/// <summary>
		/// On awake we start listening for changes
		/// </summary>
		protected virtual void Awake()
		{
			TargetSubject.PositionX.OnValueChanged += OnSpeedChange;
			this.enabled = false;
		}

		/// <summary>
		/// On destroy we stop listening for changes
		/// </summary>
		protected virtual void OnDestroy()
		{
			TargetSubject.PositionX.OnValueChanged -= OnSpeedChange;
		}

		/// <summary>
		/// On enable we do nothing
		/// </summary>
		protected virtual void OnEnable()
		{

		}

		/// <summary>
		/// On disable we do nothing
		/// </summary>
		protected virtual void OnDisable()
		{

		}
	}
}