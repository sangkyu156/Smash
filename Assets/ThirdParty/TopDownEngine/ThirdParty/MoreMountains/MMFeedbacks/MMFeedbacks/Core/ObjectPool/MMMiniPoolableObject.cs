using UnityEngine;
using System.Collections;
using MoreMountains.Feedbacks;
using System;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// objectPooler에서 풀링할 것으로 예상되는 개체에 이 클래스를 추가합니다.
    /// 이러한 객체는 Destroy()를 호출하여 파괴할 수 없으며 단지 비활성화 상태로 설정된다는 점에 유의하십시오(그게 요점입니다).
    /// </summary>
    public class MMMiniPoolableObject : MonoBehaviour 
	{
		public delegate void Events();
		public event Events OnSpawnComplete;

		/// The life time, in seconds, of the object. If set to 0 it'll live forever, if set to any positive value it'll be set inactive after that time.
		public float LifeTime = 0f;

		/// <summary>
		/// Turns the instance inactive, in order to eventually reuse it.
		/// </summary>
		public virtual void Destroy()
		{
			gameObject.SetActive(false);
		}

		/// <summary>
		/// When the objects get enabled (usually after having been pooled from an ObjectPooler, we initiate its death countdown.
		/// </summary>
		protected virtual void OnEnable()
		{
			if (LifeTime > 0)
			{
				Invoke("Destroy", LifeTime);	
			}
		}

		/// <summary>
		/// When the object gets disabled (maybe it got out of bounds), we cancel its programmed death
		/// </summary>
		protected virtual void OnDisable()
		{
			CancelInvoke();
		}

		/// <summary>
		/// Triggers the on spawn complete event
		/// </summary>
		public virtual void TriggerOnSpawnComplete()
		{
			if(OnSpawnComplete != null)
			{
				OnSpawnComplete();
			}
		}
	}
}