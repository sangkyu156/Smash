using UnityEngine;
using UnityEngine.Events;

namespace MoreMountains.Tools
{
    /// <summary>
    /// objectPooler에서 풀링할 것으로 예상되는 개체에 이 클래스를 추가합니다.
    /// 이러한 객체는 Destroy()를 호출하여 파괴할 수 없으며 단지 비활성화 상태로 설정된다는 점에 유의하십시오(그게 요점입니다).
    /// </summary>
    [AddComponentMenu("More Mountains/Tools/Object Pool/MMPoolableObject")]
	public class MMPoolableObject : MMObjectBounds
	{
		[Header("Events")]
		public UnityEvent ExecuteOnEnable;
		public UnityEvent ExecuteOnDisable;
		
		public delegate void Events();
		public event Events OnSpawnComplete;

		[Header("Poolable Object")]
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
		/// Called every frame
		/// </summary>
		protected virtual void Update()
		{

		}

		/// <summary>
		/// When the objects get enabled (usually after having been pooled from an ObjectPooler, we initiate its death countdown.
		/// </summary>
		protected virtual void OnEnable()
		{
			Size = GetBounds().extents * 2;
			if (LifeTime > 0f)
			{
				Invoke("Destroy", LifeTime);	
			}
			ExecuteOnEnable?.Invoke();
		}

		/// <summary>
		/// When the object gets disabled (maybe it got out of bounds), we cancel its programmed death
		/// </summary>
		protected virtual void OnDisable()
		{
			ExecuteOnDisable?.Invoke();
			CancelInvoke();
		}

		/// <summary>
		/// Triggers the on spawn complete event
		/// </summary>
		public virtual void TriggerOnSpawnComplete()
		{
			OnSpawnComplete?.Invoke();
		}
	}
}