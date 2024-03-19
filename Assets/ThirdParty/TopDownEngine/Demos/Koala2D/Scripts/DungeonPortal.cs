using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 생성기가 활성화된 경우 입자를 방출하고 그렇지 않은 경우 중지하기 위해 Koala 데모에서 사용되는 클래스
    /// </summary>
    public class DungeonPortal : TopDownMonoBehaviour
	{
		/// the particles to play while the portal is spawning
		[Tooltip("포털이 생성되는 동안 재생할 입자")]
		public ParticleSystem SpawnParticles;

		protected TimedSpawner _timedSpawner;
        
		/// <summary>
		/// On Awake we grab our timed spawner
		/// </summary>
		protected virtual void Awake()
		{
			_timedSpawner = this.gameObject.GetComponent<TimedSpawner>();
		}

		/// <summary>
		/// On Update, we stop or play our particles if needed
		/// </summary>
		protected virtual void Update()
		{
			if ((!_timedSpawner.CanSpawn) && (SpawnParticles.isPlaying))
			{
				SpawnParticles.Stop();
			}
			if ((_timedSpawner.CanSpawn) && (!SpawnParticles.isPlaying))
			{
				SpawnParticles.Play();
			}
		}
	}
}