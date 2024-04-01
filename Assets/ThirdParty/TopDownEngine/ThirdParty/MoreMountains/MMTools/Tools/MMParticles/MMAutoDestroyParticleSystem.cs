using UnityEngine;
using System.Collections;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 이 클래스를 ParticleSystem에 추가하면 방출이 중지되면 자동으로 소멸됩니다.
    /// ParticleSystem이 반복되지 않는지 확인하십시오. 그렇지 않으면 이 스크립트는 쓸모가 없습니다.
    /// </summary>
    [AddComponentMenu("More Mountains/Tools/Particles/MMAutoDestroyParticleSystem")]
	public class MMAutoDestroyParticleSystem : MonoBehaviour 
	{
        /// ParticleSystem이 상위 요소도 파괴해야 하는 경우 True입니다.
        public bool DestroyParent = false;

        /// 어떤 이유로 방출이 끝날 때 입자가 자동으로 파괴되지 않는 경우 지연 후 강제로 파괴할 수 있습니다. 그렇지 않으면 0으로 두십시오.
        public float DestroyDelay = 0f;
		
		protected ParticleSystem _particleSystem;
		protected float _startTime;
		protected bool _started = false;

        /// <summary>
        /// 초기화하면 ParticleSystem 구성 요소를 얻습니다.
        /// </summary>
        protected virtual void Start()
		{
			_started = false;
			_particleSystem = GetComponent<ParticleSystem>();
			if (DestroyDelay != 0)
			{
				_startTime = Time.time;
			}
		}

        /// <summary>
        /// ParticleSystem이 재생을 멈추면 이를 파괴합니다.
        /// </summary>
        protected virtual void Update()
		{	
			if ( (DestroyDelay != 0) && (Time.time - _startTime > DestroyDelay) )
			{
				DestroyParticleSystem();
			}	

			if (_particleSystem.isPlaying)
			{
				_started = true;
				return;
			}

			DestroyParticleSystem();
		}

        /// <summary>
        /// 파티클 시스템을 파괴합니다.
        /// </summary>
        protected virtual void DestroyParticleSystem()
		{
			if (!_started)
			{
				return;
			}
			if (transform.parent!=null)
			{
				if(DestroyParent)
				{
                    transform.parent.gameObject.SetActive(false);
                }
			}
            gameObject.SetActive(false);
        }
	}
}