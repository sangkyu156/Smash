using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// �� Ŭ������ ��ΰ� ������ �߻�ü�� �������� ó���մϴ�.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Automation/PathedProjectile")]
	public class PathedProjectile : TopDownMonoBehaviour
	{
		[MMInformation("�� ���� ��Ұ� ���Ե� GameObject�� ����� ���� �̵��ϰ� ��� �����ϸ� �ı��˴ϴ�. ���⿡�� ������ ���� �� �ν��Ͻ�ȭ�� ��ü�� ������ �� �ֽ��ϴ�. ���� �ӵ��� �����Ϸ��� �ʱ�ȭ �޼��带 ����ϼ���.", MoreMountains.Tools.MMInformationAttribute.InformationType.Info,false)]
		/// The effect to instantiate when the object gets destroyed
		[Tooltip("��ü�� �ı��� �� �ν��Ͻ�ȭ�ϴ� ȿ��")]
		public GameObject DestroyEffect;
		/// the destination of the projectile
		[Tooltip("�߻�ü�� ������")]
		protected Transform _destination;
		/// the movement speed
		[Tooltip("�̵� �ӵ�")]
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