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
        public bool isFirstCreated = false; // false 이면 처음 생성된거임

        [Header("Poolable Object")]
        /// 객체의 수명(초)입니다. 0으로 설정하면 영원히 유지되고, 양수로 설정하면 해당 시간 이후에는 비활성화됩니다.
        public float LifeTime = 0f;

        private void Awake()
        {
            isFirstCreated = false;
        }

        protected virtual void Update()
        {
            
        }

        /// <summary>
        /// 결국 재사용하기 위해 인스턴스를 비활성화합니다.
        /// </summary>
        public virtual void Destroy()
		{
			gameObject.SetActive(false);
		}

        /// <summary>
        /// 객체가 활성화되면(보통 ObjectPooler에서 풀링된 후) 종료 카운트다운이 시작됩니다.
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
        /// 객체가 비활성화되면(아마도 범위를 벗어났을 수도 있음) 프로그래밍된 종료를 취소합니다.
        /// </summary>
        protected virtual void OnDisable()
		{
			ExecuteOnDisable?.Invoke();
			CancelInvoke();
		}

        /// <summary>
        /// 생성 완료 이벤트를 트리거합니다.
        /// </summary>
        public virtual void TriggerOnSpawnComplete()
		{
			OnSpawnComplete?.Invoke();
		}
	}
}