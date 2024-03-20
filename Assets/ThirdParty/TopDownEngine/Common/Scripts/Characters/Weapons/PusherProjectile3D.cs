using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 구성 요소를 발사체(3D)에 추가하면 물건을 밀 수 있습니다(예: 문 열기).
    /// </summary>
    [AddComponentMenu("TopDown Engine/Weapons/Pusher Projectile 3D")]
	public class PusherProjectile3D : TopDownMonoBehaviour
	{
		/// the amount of force to apply when colliding
		[Tooltip("충돌할 때 적용되는 힘의 양")]
		public float PushPower = 10f;
		/// an offset to apply on the projectile's forward to account for super high speeds. This will affect the position the force is applied at. Usually 0 will be fine
		[Tooltip("초고속을 고려하여 발사체의 전방에 적용할 오프셋입니다. 이는 힘이 적용되는 위치에 영향을 미칩니다. 일반적으로 0이면 괜찮습니다.")]
		public float PositionOffset = 0f;

		protected Rigidbody _pushedRigidbody;
		protected Projectile _projectile;
		protected Vector3 _pushDirection;

		/// <summary>
		/// On Awake we grab our projectile component
		/// </summary>
		protected virtual void Awake()
		{
			_projectile = this.gameObject.GetComponent<Projectile>();
		}

		/// <summary>
		/// When our projectile collides with something, if it has a rigidbody, we apply the specified force to it
		/// </summary>
		/// <param name="collider"></param>
		protected virtual void OnTriggerEnter(Collider collider)
		{
			_pushedRigidbody = collider.attachedRigidbody;

			if ((_pushedRigidbody == null) || (_pushedRigidbody.isKinematic))
			{
				return;
			}

			_pushDirection.x = _projectile.Direction.x;
			_pushDirection.y = 0;
			_pushDirection.z = _projectile.Direction.z;

			_pushedRigidbody.AddForceAtPosition(_pushDirection.normalized * PushPower, this.transform.position + this.transform.forward * PositionOffset);
		}		
	}
}