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
		/// True if the ParticleSystem should also destroy its parent
		public bool DestroyParent = false;

		/// If for some reason your particles don't get destroyed automatically at the end of the emission, you can force a destroy after a delay. Leave it at zero otherwise.
		public float DestroyDelay = 0f;
		
		protected ParticleSystem _particleSystem;
		protected float _startTime;
		protected bool _started = false;
		
		/// <summary>
		/// Initialization, we get the ParticleSystem component
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
		/// When the ParticleSystem stops playing, we destroy it.
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
		/// Destroys the particle system.
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
					Destroy(transform.parent.gameObject);	
				}
			}					
			Destroy (gameObject);
		}
	}
}