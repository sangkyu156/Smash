using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 클래스는 경로가 지정된 발사체의 움직임을 처리합니다.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Automation/PathedProjectile")]
	public class PathedProjectile : TopDownMonoBehaviour
	{
		[MMInformation("이 구성 요소가 포함된 GameObject는 대상을 향해 이동하고 대상에 도달하면 파괴됩니다. 여기에서 영향을 받을 때 인스턴스화할 개체를 정의할 수 있습니다. 대상과 속도를 설정하려면 초기화 메서드를 사용하세요.", MoreMountains.Tools.MMInformationAttribute.InformationType.Info,false)]
		/// The effect to instantiate when the object gets destroyed
		[Tooltip("객체가 파괴될 때 인스턴스화하는 효과")]
		public GameObject DestroyEffect;
		/// the destination of the projectile
		[Tooltip("발사체의 목적지")]
		protected Transform _destination;
		/// the movement speed
		[Tooltip("이동 속도")]
		protected float _speed;

		/// <summary>
		/// Initializes the specified destination and speed.
		/// </summary>
		/// <param name="destination">Destination.</param>
		/// <param name="speed">Speed.</param>
		public virtual void Initialize(Transform destination, float speed)
		{
			_destination=destination;
			_speed=speed;
		}

		/// <summary>
		/// Every frame, me move the projectile's position to its destination
		/// </summary>
		protected virtual void Update () 
		{
			transform.position=Vector3.MoveTowards(transform.position,_destination.position,Time.deltaTime * _speed);
			var distanceSquared = (_destination.transform.position - transform.position).sqrMagnitude;
			if(distanceSquared > .01f * .01f)
				return;
			
			if (DestroyEffect!=null)
			{
				Instantiate(DestroyEffect,transform.position,transform.rotation); 
			}
			
			Destroy(gameObject);
		}	
	}
}