using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 객체 풀(단순 또는 다중)과 함께 사용되는 클래스
    /// 인스펙터에 설정된 최소값과 최대값 사이에서 무작위로 선택된 빈도로 객체를 정기적으로 생성합니다.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Automation/TimedSpawner")]
	public class TimedSpawner : TopDownMonoBehaviour 
	{
		/// the object pooler associated to this spawner
		public MMObjectPooler ObjectPooler { get; set; }
		
		[Header("Spawn")]
		/// whether or not this spawner can spawn
		[Tooltip("이 생성자가 생성될 수 있는지 여부")]
		public bool CanSpawn = true;
		/// the minimum frequency possible, in seconds
		[Tooltip("가능한 최소 빈도(초)")]
		public float MinFrequency = 1f;
		/// the maximum frequency possible, in seconds
		[Tooltip("가능한 최대 빈도(초)")]
		public float MaxFrequency = 1f;

		[Header("Debug")]
		[MMInspectorButton("ToggleSpawn")]
		/// a test button to spawn an object
		public bool CanSpawnButton;

		protected float _lastSpawnTimestamp = 0f;
		protected float _nextFrequency = 0f;

		/// <summary>
		/// On Start we initialize our spawner
		/// </summary>
		protected virtual void Start()
		{
			Initialization ();
		}

		/// <summary>
		/// Grabs the associated object pooler if there's one, and initalizes the frequency
		/// </summary>
		protected virtual void Initialization()
		{
			if (GetComponent<MMMultipleObjectPooler>() != null)
			{
				ObjectPooler = GetComponent<MMMultipleObjectPooler>();
			}
			if (GetComponent<MMSimpleObjectPooler>() != null)
			{
				ObjectPooler = GetComponent<MMSimpleObjectPooler>();
			}
			if (ObjectPooler == null)
			{
				Debug.LogWarning(this.name+" : no object pooler (simple or multiple) is attached to this Projectile Weapon, it won't be able to shoot anything.");
				return;
			}
			DetermineNextFrequency ();
		}

		/// <summary>
		/// Every frame we check whether or not we should spawn something
		/// </summary>
		protected virtual void Update()
		{
			if ((Time.time - _lastSpawnTimestamp > _nextFrequency)  && CanSpawn)
			{
				Spawn ();
			}
		}

		/// <summary>
		/// Spawns an object out of the pool if there's one available.
		/// If it's an object with Health, revives it too.
		/// </summary>
		protected virtual void Spawn()
		{
			GameObject nextGameObject = ObjectPooler.GetPooledGameObject();

			// mandatory checks
			if (nextGameObject==null) { return; }
			if (nextGameObject.GetComponent<MMPoolableObject>()==null)
			{
				throw new Exception(gameObject.name+" is trying to spawn objects that don't have a PoolableObject component.");		
			}	

			// we activate the object
			nextGameObject.gameObject.SetActive(true);
			nextGameObject.gameObject.MMGetComponentNoAlloc<MMPoolableObject>().TriggerOnSpawnComplete();

			// we check if our object has an Health component, and if yes, we revive our character
			Health objectHealth = nextGameObject.gameObject.MMGetComponentNoAlloc<Health> ();
			if (objectHealth != null) 
			{
				objectHealth.Revive ();
			}

			// we position the object
			nextGameObject.transform.position = this.transform.position;

			// we reset our timer and determine the next frequency
			_lastSpawnTimestamp = Time.time;
			DetermineNextFrequency ();
		}

		/// <summary>
		/// Determines the next frequency by randomizing a value between the two specified in the inspector.
		/// </summary>
		protected virtual void DetermineNextFrequency()
		{
			_nextFrequency = UnityEngine.Random.Range (MinFrequency, MaxFrequency);
		}

		/// <summary>
		/// Toggles spawn on and off
		/// </summary>
		public virtual void ToggleSpawn()
		{
			CanSpawn = !CanSpawn;
		}

		/// <summary>
		/// Turns spawning off
		/// </summary>
		public virtual void TurnSpawnOff()
		{
			CanSpawn = false;
		}

		/// <summary>
		/// Turns spawning on
		/// </summary>
		public virtual void TurnSpawnOn()
		{
			CanSpawn = true;
		}
	}
}