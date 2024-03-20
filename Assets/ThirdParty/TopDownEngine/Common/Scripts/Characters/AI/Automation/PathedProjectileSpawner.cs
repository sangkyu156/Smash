using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// Spawns pathed 발사체
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Automation/PathedProjectileSpawner")]
	public class PathedProjectileSpawner : TopDownMonoBehaviour 
	{
		[MMInformation("이 구성 요소가 포함된 GameObject는 지정된 발사 속도로 발사체를 생성합니다.", MoreMountains.Tools.MMInformationAttribute.InformationType.Info,false)]
		/// the pathed projectile's destination
		[Tooltip("경로가 지정된 발사체의 목적지")]
		public Transform Destination;
		/// the projectiles to spawn
		[Tooltip("스폰되는 발사체")]
		public PathedProjectile Projectile;
		/// the effect to instantiate at each spawn
		[Tooltip("각 생성 시 인스턴스화할 효과")]
		public GameObject SpawnEffect;
		/// the speed of the projectiles
		[Tooltip("발사체의 속도")]
		public float Speed;
		/// the frequency of the spawns
		[Tooltip("스폰의 빈도")]
		public float FireRate;
		
		protected float _nextShotInSeconds;

		/// <summary>
		/// Initialization
		/// </summary>
		protected virtual void Start () 
		{
			_nextShotInSeconds=FireRate;
		}

		/// <summary>
		/// Every frame, we check if we need to instantiate a new projectile
		/// </summary>
		protected virtual void Update () 
		{
			if((_nextShotInSeconds -= Time.deltaTime)>0)
				return;
				
			_nextShotInSeconds = FireRate;
			var projectile = (PathedProjectile) Instantiate(Projectile, transform.position,transform.rotation);
			projectile.Initialize(Destination,Speed);
			
			if (SpawnEffect!=null)
			{
				Instantiate(SpawnEffect,transform.position,transform.rotation);
			}
		}

		/// <summary>
		/// Debug mode
		/// </summary>
		public virtual void OnDrawGizmos()
		{
			if (Destination==null)
				return;
			
			Gizmos.color=Color.gray;
			Gizmos.DrawLine(transform.position,Destination.position);
		}
	}
}