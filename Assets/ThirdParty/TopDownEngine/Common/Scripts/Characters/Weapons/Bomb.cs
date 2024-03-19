using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 무기를 사용할 때 "상처 구역"을 활성화하는 기본 근접 무기 클래스
    /// </summary>
    [AddComponentMenu("TopDown Engine/Weapons/Bomb")]
	public class Bomb : TopDownMonoBehaviour 
	{
		/// the shape of the bomb's damage area
		public enum DamageAreaShapes { Rectangle, Circle }

		[Header("Explosion")]
        /// 폭탄이 터지기 전의 지연
        [Tooltip("폭탄이 터지기 전의 지연")]
		public float TimeBeforeExplosion = 2f;
        /// 폭탄이 폭발할 때 인스턴스화할 VFX
        [Tooltip("폭탄이 폭발할 때 인스턴스화할 VFX")]
		public GameObject ExplosionEffect;
        /// 폭탄이 터질 때 재생되는 소리
        [Tooltip("폭탄이 터질 때 재생되는 소리")]
		public AudioClip ExplosionSfx;

		[Header("Flicker")]
        /// 폭발하기 전에 스프라이트가 깜박여야 하는지 여부
        [Tooltip("폭발하기 전에 스프라이트가 깜박여야 하는지 여부")]
		public bool FlickerSprite = true;
        /// 깜박임이 시작되기 전까지의 시간
        [Tooltip("깜박임이 시작되기 전까지의 시간")]
		public float TimeBeforeFlicker = 1f;
        /// 깜박여야 하는 속성의 이름
        [Tooltip("깜박여야 하는 속성의 이름")]
		public string MaterialPropertyName = "_Color";

		[Header("Damage Area")]
        /// 피해 지역의 충돌체
        [Tooltip("피해 지역의 충돌체")]
		public Collider2D DamageAreaCollider;
        /// 피해 지역의 지속 시간
        [Tooltip("피해 지역의 지속 시간")]
		public float DamageAreaActiveDuration = 1f;

		protected float _timeSinceStart;
		protected Renderer _renderer;
		protected MMPoolableObject _poolableObject;
		protected bool _flickering;
		protected bool _damageAreaActive;
		protected Color _initialColor;
		protected Color _flickerColor = new Color32(255, 20, 20, 255);
		protected MaterialPropertyBlock _propertyBlock;
		
		/// <summary>
		/// On enable, we initialize our bomb
		/// </summary>
		protected virtual void OnEnable()
		{
			Initialization ();
		}

		/// <summary>
		/// Initializes the bomb
		/// </summary>
		protected virtual void Initialization()
		{
			if (DamageAreaCollider == null)
			{
				Debug.LogWarning ("There's no damage area associated to this bomb : " + this.name + ". You should set one via its inspector.");
				return;
			}
			DamageAreaCollider.isTrigger = true;
			DisableDamageArea ();

			_propertyBlock = new MaterialPropertyBlock();
			_renderer = gameObject.MMGetComponentNoAlloc<Renderer> ();
			if (_renderer != null)
			{
				if (_renderer.sharedMaterial.HasProperty(MaterialPropertyName))
				{
					_initialColor = _renderer.sharedMaterial.GetColor(MaterialPropertyName);    
				}
			}

			_poolableObject = gameObject.MMGetComponentNoAlloc<MMPoolableObject> ();
			if (_poolableObject != null)
			{
				_poolableObject.LifeTime = 0;
			}

			_timeSinceStart = 0;
			_flickering = false;
			_damageAreaActive = false;
		}

		/// <summary>
		/// On update, makes our bomb flicker, activates the damage area and destroys the bomb if needed
		/// </summary>
		protected virtual void Update()
		{
			_timeSinceStart += Time.deltaTime;
			// flickering
			if (_timeSinceStart >= TimeBeforeFlicker)
			{
				if (!_flickering && FlickerSprite)
				{
					// We make the bomb's sprite flicker
					if (_renderer != null)
					{
						StartCoroutine(MMImage.Flicker(_renderer,_initialColor,_flickerColor,0.05f,(TimeBeforeExplosion - TimeBeforeFlicker)));	
					}
				}
			}

			// activate damage area
			if (_timeSinceStart >= TimeBeforeExplosion && !_damageAreaActive)
			{
				EnableDamageArea ();
				_renderer.enabled = false;
				InstantiateExplosionEffect ();
				PlayExplosionSound ();
				_damageAreaActive = true;
			}

			if (_timeSinceStart >= TimeBeforeExplosion + DamageAreaActiveDuration)
			{
				DestroyBomb ();
			}
		}

		/// <summary>
		/// Destroys the bomb
		/// </summary>
		protected virtual void DestroyBomb()
		{
			_renderer.enabled = true;
			_renderer.GetPropertyBlock(_propertyBlock);
			_propertyBlock.SetColor(MaterialPropertyName, _initialColor);
			_renderer.SetPropertyBlock(_propertyBlock);
			if (_poolableObject != null)
			{
				_poolableObject.Destroy ();	
			}
			else
			{
				Destroy (gameObject);
			}

		}

		/// <summary>
		/// Instantiates a VFX at the bomb's position
		/// </summary>
		protected virtual void InstantiateExplosionEffect()
		{
			// instantiates the destroy effect
			if (ExplosionEffect!=null)
			{
				GameObject instantiatedEffect=(GameObject)Instantiate(ExplosionEffect,transform.position,transform.rotation);
				instantiatedEffect.transform.localScale = transform.localScale;
			}
		}

		/// <summary>
		/// Plays a sound on explosion
		/// </summary>
		protected virtual void PlayExplosionSound()
		{
			if (ExplosionSfx!=null)
			{
				MMSoundManagerSoundPlayEvent.Trigger(ExplosionSfx, MMSoundManager.MMSoundManagerTracks.Sfx, this.transform.position);
			}
		}

		/// <summary>
		/// Enables the damage area.
		/// </summary>
		protected virtual void EnableDamageArea()
		{
			DamageAreaCollider.enabled = true;
		}

		/// <summary>
		/// Disables the damage area.
		/// </summary>
		protected virtual void DisableDamageArea()
		{
			DamageAreaCollider.enabled = false;
		}
	}
}