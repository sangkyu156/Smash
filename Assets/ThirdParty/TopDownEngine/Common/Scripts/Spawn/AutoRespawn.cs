using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 스크립트를 개체에 추가하면 플레이어가 다시 생성될 때 자동으로 다시 활성화되고 부활됩니다.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Spawn/Auto Respawn")]
	public class AutoRespawn : TopDownMonoBehaviour, Respawnable 
	{
		[Header("플레이어가 리스폰되면 리스폰됩니다.")]
        /// 이것이 사실이라면 이 개체는 플레이어가 부활할 때 마지막 위치에서 다시 생성됩니다.
        [Tooltip("이것이 사실이라면 이 개체는 플레이어가 부활할 때 마지막 위치에서 다시 생성됩니다.")]
		public bool RespawnOnPlayerRespawn = true;
        /// 이것이 사실이라면 플레이어가 부활할 때 이 개체는 초기 위치로 재배치됩니다.
        [Tooltip("이것이 사실이라면 플레이어가 부활할 때 이 개체는 초기 위치로 재배치됩니다.")]
		public bool RepositionToInitOnPlayerRespawn = false;
        /// 이것이 사실이라면 이 객체의 모든 구성요소는 종료 시 비활성화됩니다.
        [Tooltip("이것이 사실이라면 이 객체의 모든 구성요소는 종료 시 비활성화됩니다.")]
		public bool DisableAllComponentsOnKill = false;
        /// 이것이 사실이라면 이 게임오브젝트는 종료 시 비활성화됩니다.
        [Tooltip("이것이 사실이라면 이 게임오브젝트는 종료 시 비활성화됩니다.")]
		public bool DisableGameObjectOnKill = true;        

		[Header("Checkpoints")]
        /// 이것이 사실이라면 개체는 체크포인트에 연결되어 있는지 여부에 관계없이 항상 다시 생성됩니다.
        [Tooltip("이것이 사실이라면 개체는 체크포인트에 연결되어 있는지 여부에 관계없이 항상 다시 생성됩니다.")]
		public bool IgnoreCheckpointsAlwaysRespawn = true;
        /// 플레이어가 이 체크포인트에서 다시 생성되면 개체가 다시 생성됩니다.
        [Tooltip("플레이어가 이 체크포인트에서 다시 생성되면 개체가 다시 생성됩니다.")]
		public List<CheckPoint> AssociatedCheckpoints;

		[Header("Auto respawn after X seconds")]
        /// 값이 0보다 높으면 이 개체는 죽은 지 X초 후에 마지막 위치에서 다시 생성됩니다.
        [Tooltip("값이 0보다 높으면 이 개체는 죽은 지 X초 후에 마지막 위치에서 다시 생성됩니다.")]
		public float AutoRespawnDuration = 0f;
        /// 이 개체가 자동으로 다시 생성될 수 있는 횟수
        [Tooltip("이 개체가 자동으로 다시 생성될 수 있는 횟수")]
		public int AutoRespawnAmount = 3;
        /// 남은 리스폰 양(읽기 전용, 런타임 시 클래스에 의해 제어됨)
        [Tooltip("남은 리스폰 양(읽기 전용, 런타임 시 클래스에 의해 제어됨)")]
		[MMReadOnly]
		public int AutoRespawnRemainingAmount = 3;
        /// 플레이어가 다시 생성될 때 인스턴스화할 효과
        [Tooltip("플레이어가 다시 생성될 때 인스턴스화할 효과")]
		public GameObject RespawnEffect;
        /// 플레이어가 부활할 때 재생할 SFX
        [Tooltip("플레이어가 부활할 때 재생할 SFX")]
		public AudioClip RespawnSfx;

		// respawn
		public delegate void OnReviveDelegate();
		public OnReviveDelegate OnRevive;

		protected MonoBehaviour[] _otherComponents;
		protected Collider2D _collider2D;
		protected Renderer _renderer;
		protected Character _character;
		protected Health _health;
		protected bool _reviving = false;
		protected float _timeOfDeath = 0f;
		protected bool _firstRespawn = true;
		protected Vector3 _initialPosition;
		protected AIBrain _aiBrain;

		/// <summary>
		/// On Start we grab our various components
		/// </summary>
		protected virtual void Start()
		{
			AutoRespawnRemainingAmount = AutoRespawnAmount;
			_otherComponents = this.gameObject.GetComponents<MonoBehaviour>() ;
			_collider2D = this.gameObject.GetComponent<Collider2D> ();
			_renderer = this.gameObject.GetComponent<Renderer> ();
			_character = this.gameObject.GetComponent<Character>();
			_health = this.gameObject.GetComponent<Health>();
			_aiBrain = this.gameObject.GetComponent<AIBrain>();
			if ((_aiBrain == null) && (_character != null))
			{
				_aiBrain = _character.CharacterBrain;
			}
			_initialPosition = this.transform.position;
		}

		/// <summary>
		/// When the player respawns, we reinstate this agent.
		/// </summary>
		/// <param name="checkpoint">Checkpoint.</param>
		/// <param name="player">Player.</param>
		public virtual void OnPlayerRespawn (CheckPoint checkpoint, Character player)
		{
			if (RepositionToInitOnPlayerRespawn)
			{
				this.transform.position = _initialPosition;				
			}

			if (RespawnOnPlayerRespawn)
			{
				if (_health != null)
				{
					_health.Revive();
				}
				Revive ();
			}
			AutoRespawnRemainingAmount = AutoRespawnAmount;
		}

		/// <summary>
		/// On Update we check whether we should be reviving this agent
		/// </summary>
		protected virtual void Update()
		{
			if (_reviving)
			{
				if (_timeOfDeath + AutoRespawnDuration < Time.time)
				{
					if (AutoRespawnAmount == 0)
					{
						return;
					}
					if (AutoRespawnAmount > 0)
					{
						if (AutoRespawnRemainingAmount <= 0)
						{
							return;
						}
						AutoRespawnRemainingAmount -= 1;
					}
					Revive ();
					_reviving = false;
				}
			}
		}

		/// <summary>
		/// Kills this object, turning its parts off based on the settings set in the inspector
		/// </summary>
		public virtual void Kill()
		{
			if (AutoRespawnDuration <= 0f)
			{
				// object is turned inactive to be able to reinstate it at respawn
				if (DisableGameObjectOnKill)
				{
					gameObject.SetActive(false);	
				}
			}
			else
			{
				if (DisableAllComponentsOnKill)
				{
					foreach (MonoBehaviour component in _otherComponents)
					{
						if (component != this)
						{
							component.enabled = false;
						}
					}
				}
				
				if (_collider2D != null) { _collider2D.enabled = false;	}
				if (_renderer != null)	{ _renderer.enabled = false; }
				_reviving = true;
				_timeOfDeath = Time.time;
			}
		}

		/// <summary>
		/// Revives this object, turning its parts back on again
		/// </summary>
		public virtual void Revive()
		{
			if (AutoRespawnDuration <= 0f)
			{
				// object is turned inactive to be able to reinstate it at respawn
				gameObject.SetActive(true);
			}
			else
			{
				if (DisableAllComponentsOnKill)
				{
					foreach (MonoBehaviour component in _otherComponents)
					{
						component.enabled = true;
					}
				}
				
				if (_collider2D != null) { _collider2D.enabled = true;	}
				if (_renderer != null)	{ _renderer.enabled = true; }
				InstantiateRespawnEffect ();
				PlayRespawnSound ();
			}
			if (_health != null)
			{
				_health.Revive();
			}
			if (_aiBrain != null)
			{
				_aiBrain.ResetBrain();
			}
			OnRevive?.Invoke();
		}

		/// <summary>
		/// Instantiates the respawn effect at the object's position
		/// </summary>
		protected virtual void InstantiateRespawnEffect()
		{
			// instantiates the destroy effect
			if (RespawnEffect != null)
			{
				GameObject instantiatedEffect=(GameObject)Instantiate(RespawnEffect,transform.position,transform.rotation);
				instantiatedEffect.transform.localScale = transform.localScale;
			}
		}

		/// <summary>
		/// Plays the respawn sound.
		/// </summary>
		protected virtual void PlayRespawnSound()
		{
			if (RespawnSfx != null)
			{
				MMSoundManagerSoundPlayEvent.Trigger(RespawnSfx, MMSoundManager.MMSoundManagerTracks.Sfx, this.transform.position);
			}
		}
	}
}