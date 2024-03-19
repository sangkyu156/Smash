using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 구성요소를 개체에 추가하면 충돌하는 개체에 손상을 입힐 수 있습니다.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/Damage/KillZone")]
	public class KillZone : TopDownMonoBehaviour
	{
		[Header("Targets")]
		[MMInformation("이 구성 요소는 개체가 충돌하는 개체를 죽이도록 만듭니다. 여기에서 어떤 레이어를 삭제할지 정의할 수 있습니다.", MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]
		// the layers containing the objects that will be damaged by this object
		[Tooltip("이 개체에 의해 손상될 개체를 포함하는 레이어")]
		public LayerMask TargetLayerMask = LayerManager.PlayerLayerMask;

		protected Health _colliderHealth;

		/// <summary>
		/// Initialization
		/// </summary>
		protected virtual void Awake()
		{

		}

		/// <summary>
		/// OnEnable we set the start time to the current timestamp
		/// </summary>
		protected virtual void OnEnable()
		{

		}
        
		/// <summary>
		/// When a collision with the player is triggered, we give damage to the player and knock it back
		/// </summary>
		/// <param name="collider">what's colliding with the object.</param>
		public virtual void OnTriggerStay2D(Collider2D collider)
		{
			Colliding(collider.gameObject);
		}

		/// <summary>
		/// when something enters our zone, we call our colliding endpoint
		/// </summary>
		/// <param name="collider"></param>
		public virtual void OnTriggerEnter2D(Collider2D collider)
		{
			Colliding(collider.gameObject);
		}

		/// <summary>
		/// when something stays in the zone, we call our colliding endpoint
		/// </summary>
		/// <param name="collider"></param>
		public virtual void OnTriggerStay(Collider collider)
		{
			Colliding(collider.gameObject);
		}

		/// <summary>
		/// When something enters our zone, we call our colliding endpoint
		/// </summary>
		/// <param name="collider"></param>
		public virtual void OnTriggerEnter(Collider collider)
		{
			Colliding(collider.gameObject);
		}

		/// <summary>
		/// When colliding, we kill our collider if it's a Health equipped object
		/// </summary>
		/// <param name="collider"></param>
		protected virtual void Colliding(GameObject collider)
		{
			if (!this.isActiveAndEnabled)
			{
				return;
			}

			// if what we're colliding with isn't part of the target layers, we do nothing and exit
			if (!MMLayers.LayerInLayerMask(collider.layer, TargetLayerMask))
			{
				return;
			}

			_colliderHealth = collider.gameObject.MMGetComponentNoAlloc<Health>();

			// if what we're colliding with is damageable
			if (_colliderHealth != null)
			{
				if (_colliderHealth.CurrentHealth > 0)
				{
					_colliderHealth.Kill();
				}                
			}
		}
	}
}