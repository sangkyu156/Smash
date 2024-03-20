using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 이 피드백은 파티클 시스템을 인스턴스화하고 피드백을 재생/중지할 때 이를 재생/중지합니다.
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("이 피드백은 시작 또는 재생의 지정된 위치에서 지정된 ParticleSystem을 인스턴스화하고 선택적으로 중첩합니다.")]
	[FeedbackPath("Particles/Particles Instantiation")]
	public class MMF_ParticlesInstantiation : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(DeclaredDuration); } set { DeclaredDuration = value;  } }
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.ParticlesColor; } }
		public override bool EvaluateRequiresSetup() { return (ParticlesPrefab == null); }
		public override string RequiredTargetText { get { return ParticlesPrefab != null ? ParticlesPrefab.name : "";  } }
		public override string RequiresSetupText { get { return "This feedback requires that a ParticlesPrefab be set to be able to work properly. You can set one below."; } }
#endif
        /// 인스턴스화된 객체를 배치하는 다양한 방법:
        /// - FeedbackPosition : 객체는 피드백 위치와 선택적 오프셋에서 인스턴스화됩니다.
        /// - Transform : 객체는 지정된 Transform 위치와 선택적 오프셋에서 인스턴스화됩니다.
        /// - WorldPosition: 객체는 지정된 월드 위치 벡터와 선택적 오프셋에서 인스턴스화됩니다.
        /// - 스크립트 : 피드백 호출 시 매개변수로 전달되는 위치
        [Tooltip("인스턴스화된 객체를 배치하는 다양한 방법:" +
			"- FeedbackPosition : 객체는 피드백 위치와 선택적 오프셋에서 인스턴스화됩니다." +
			"- Transform : 객체는 지정된 Transform 위치와 선택적 오프셋에서 인스턴스화됩니다." +
			"- WorldPosition: 객체는 지정된 월드 위치 벡터와 선택적 오프셋에서 인스턴스화됩니다." +
			"- 스크립트 : 피드백 호출 시 매개변수로 전달되는 위치")]
        public enum PositionModes { FeedbackPosition, Transform, WorldPosition, Script }
        /// 가능한 배달 모드
        /// - cached : 파티클 시스템의 복사본을 캐시하고 재사용합니다.
        /// - demand : 모든 플레이에 대해 새로운 입자 시스템을 인스턴스화합니다.
        [Tooltip("가능한 배달 모드" +
			"- cached : 파티클 시스템의 복사본을 캐시하고 재사용합니다." +
			"- demand : 모든 플레이에 대해 새로운 입자 시스템을 인스턴스화합니다.")]
        public enum Modes { Cached, OnDemand, Pool }

		[MMFInspectorGroup("Particles Instantiation", true, 37, true)]
		/// whether the particle system should be cached or created on demand the first time
		[Tooltip("파티클 시스템을 처음으로 캐시해야 하는지 아니면 요청 시 생성해야 하는지 여부")]
		public Modes Mode = Modes.Cached;
		
		/// the initial and planned size of this object pool
		[Tooltip("이 개체 풀의 초기 및 계획 크기")]
		[MMFEnumCondition("Mode", (int)Modes.Pool)]
		public int ObjectPoolSize = 5;
		/// whether or not to create a new pool even if one already exists for that same prefab
		[Tooltip("동일한 프리팹에 대해 이미 풀이 존재하는 경우에도 새 풀을 생성할지 여부")]
		[MMFEnumCondition("Mode", (int)Modes.Pool)]
		public bool MutualizePools = false;
		/// if specified, the instantiated object (or the pool of objects) will be parented to this transform 
		[Tooltip("지정된 경우 인스턴스화된 객체(또는 객체 풀)는 이 변환의 상위 항목이 됩니다. ")]
		[MMFEnumCondition("Mode", (int)Modes.Pool)]
		public Transform ParentTransform;
		
		/// if this is false, a brand new particle system will be created every time
		[Tooltip("이것이 거짓이면 매번 새로운 입자 시스템이 생성됩니다.")]
		[MMFEnumCondition("Mode", (int)Modes.OnDemand)]
		public bool CachedRecycle = true;
		/// the particle system to spawn
		[Tooltip("스폰할 파티클 시스템")]
		public ParticleSystem ParticlesPrefab;
		/// the possible random particle systems
		[Tooltip("가능한 무작위 입자 시스템")]
		public List<ParticleSystem> RandomParticlePrefabs;
		/// if this is true, the particle system game object will be activated on Play, useful if you've somehow disabled it in a past Play
		[Tooltip("이것이 사실이라면 입자 시스템 게임 개체가 Play에서 활성화됩니다. 과거 Play에서 어떻게든 비활성화한 경우 유용합니다.")]
		public bool ForceSetActiveOnPlay = false;
		/// if this is true, the particle system will be stopped every time the feedback is reset - usually before play
		[Tooltip("이것이 사실이라면 피드백이 재설정될 때마다 파티클 시스템이 중지됩니다. 일반적으로 플레이 전입니다.")]
		public bool StopOnReset = false;
		/// the duration for the player to consider. This won't impact your particle system, but is a way to communicate to the MMF Player the duration of this feedback. Usually you'll want it to match your actual particle system, and setting it can be useful to have this feedback work with holding pauses.
		[Tooltip("플레이어가 고려해야 할 기간. 이는 입자 시스템에 영향을 주지 않지만 이 피드백 기간을 MMF 플레이어에 전달하는 방법입니다. 일반적으로 실제 입자 시스템과 일치하도록 설정하고 일시 중지를 유지하면서 이 피드백이 작동하도록 설정하는 것이 유용할 수 있습니다.")]
		public float DeclaredDuration = 0f;

		[MMFInspectorGroup("Position", true, 29)]
		/// the selected position mode
		[Tooltip("선택한 위치 모드")]
		public PositionModes PositionMode = PositionModes.FeedbackPosition;
		/// the position at which to spawn this particle system
		[Tooltip("이 입자 시스템을 생성할 위치")]
		[MMFEnumCondition("PositionMode", (int)PositionModes.Transform)]
		public Transform InstantiateParticlesPosition;
		/// the world position to move to when in WorldPosition mode 
		[Tooltip("WorldPosition 모드에 있을 때 이동할 월드 위치")]
		[MMFEnumCondition("PositionMode", (int)PositionModes.WorldPosition)]
		public Vector3 TargetWorldPosition;
		/// an offset to apply to the instantiation position
		[Tooltip("인스턴스화 위치에 적용할 오프셋")]
		public Vector3 Offset;
		/// whether or not the particle system should be nested in hierarchy or floating on its own
		[Tooltip("입자 시스템이 계층 구조에 중첩되어야 하는지 아니면 자체적으로 떠다니는지 여부")]
		[MMFEnumCondition("PositionMode", (int)PositionModes.Transform, (int)PositionModes.FeedbackPosition)]
		public bool NestParticles = true;
		/// whether or not to also apply rotation
		[Tooltip("회전도 적용할지 여부")]
		public bool ApplyRotation = false;
		/// whether or not to also apply scale
		[Tooltip("스케일 적용 여부")]
		public bool ApplyScale = false;

		[MMFInspectorGroup("Simulation Speed", true, 43, false)]
		/// whether or not to force a specific simulation speed on the target particle system(s)
		[Tooltip("대상 입자 시스템에 특정 시뮬레이션 속도를 강제할지 여부")]
		public bool ForceSimulationSpeed = false;
		/// The min and max values at which to randomize the simulation speed, if ForceSimulationSpeed is true. A new value will be randomized every time this feedback plays
		[Tooltip("ForceSimulationSpeed가 true인 경우 시뮬레이션 속도를 무작위화할 최소 및 최대 값입니다. 이 피드백이 재생될 때마다 새로운 값이 무작위로 지정됩니다.\r\n")]
		[MMFCondition("ForceSimulationSpeed", true)]
		public Vector2 ForcedSimulationSpeed = new Vector2(0.1f,1f);

		protected ParticleSystem _instantiatedParticleSystem;
		protected List<ParticleSystem> _instantiatedRandomParticleSystems;

		protected MMMiniObjectPooler _objectPooler; 
		protected GameObject _newGameObject;
		protected bool _poolCreatedOrFound = false;
		
		/// <summary>
		/// On init, instantiates the particle system, positions it and nests it if needed
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(MMF_Player owner)
		{
			if (!Active)
			{
				return;
			}
			
			CacheParticleSystem();

			CreatePools(owner);
		}
		
		protected virtual bool ShouldCache => (Mode == Modes.OnDemand && CachedRecycle) || (Mode == Modes.Cached);

		protected virtual void CreatePools(MMF_Player owner)
		{
			if (Mode != Modes.Pool)
			{
				return;
			}

			if (!_poolCreatedOrFound)
			{
				if (_objectPooler != null)
				{
					_objectPooler.DestroyObjectPool();
					owner.ProxyDestroy(_objectPooler.gameObject);
				}

				GameObject objectPoolGo = new GameObject();
				objectPoolGo.name = Owner.name+"_ObjectPooler";
				_objectPooler = objectPoolGo.AddComponent<MMMiniObjectPooler>();
				_objectPooler.GameObjectToPool = ParticlesPrefab.gameObject;
				_objectPooler.PoolSize = ObjectPoolSize;
				if (ParentTransform != null)
				{
					_objectPooler.transform.SetParent(ParentTransform);
				}
				else
				{
					_objectPooler.transform.SetParent(Owner.transform);
				}
				_objectPooler.MutualizeWaitingPools = MutualizePools;
				_objectPooler.FillObjectPool();
				if ((Owner != null) && (objectPoolGo.transform.parent == null))
				{
					SceneManager.MoveGameObjectToScene(objectPoolGo, Owner.gameObject.scene);    
				}
				_poolCreatedOrFound = true;
			}
			
		}
		
		protected virtual void CacheParticleSystem()
		{
			if (!ShouldCache)
			{
				return;
			}

			InstantiateParticleSystem();
		}

		/// <summary>
		/// Instantiates the particle system
		/// </summary>
		protected virtual void InstantiateParticleSystem()
		{
			Transform newParent = null;
            
			if (NestParticles)
			{
				if (PositionMode == PositionModes.FeedbackPosition)
				{
					newParent = Owner.transform;
				}
				if (PositionMode == PositionModes.Transform)
				{
					newParent = InstantiateParticlesPosition;
				}
			}
			
			if (RandomParticlePrefabs.Count > 0)
			{
				if (ShouldCache)
				{
					_instantiatedRandomParticleSystems = new List<ParticleSystem>();
					foreach(ParticleSystem system in RandomParticlePrefabs)
					{
						ParticleSystem newSystem = GameObject.Instantiate(system, newParent) as ParticleSystem;
						if (newParent == null)
						{
							SceneManager.MoveGameObjectToScene(newSystem.gameObject, Owner.gameObject.scene);    
						}
						newSystem.Stop();
						_instantiatedRandomParticleSystems.Add(newSystem);
					}
				}
				else
				{
					int random = Random.Range(0, RandomParticlePrefabs.Count);
					_instantiatedParticleSystem = GameObject.Instantiate(RandomParticlePrefabs[random], newParent) as ParticleSystem;
					if (newParent == null)
					{
						SceneManager.MoveGameObjectToScene(_instantiatedParticleSystem.gameObject, Owner.gameObject.scene);    
					}
				}
			}
			else
			{
				if (ParticlesPrefab == null)
				{
					return;
				}
				_instantiatedParticleSystem = GameObject.Instantiate(ParticlesPrefab, newParent) as ParticleSystem;
				_instantiatedParticleSystem.Stop();
				if (newParent == null)
				{
					SceneManager.MoveGameObjectToScene(_instantiatedParticleSystem.gameObject, Owner.gameObject.scene);    
				}
			}
			
			if (_instantiatedParticleSystem != null)
			{
				PositionParticleSystem(_instantiatedParticleSystem);
			}

			if ((_instantiatedRandomParticleSystems != null) && (_instantiatedRandomParticleSystems.Count > 0))
			{
				foreach (ParticleSystem system in _instantiatedRandomParticleSystems)
				{
					PositionParticleSystem(system);
				}
			}
		}

		protected virtual void PositionParticleSystem(ParticleSystem system)
		{
			if (InstantiateParticlesPosition == null)
			{
				if (Owner != null)
				{
					InstantiateParticlesPosition = Owner.transform;
				}
			}

			if (system != null)
			{
				system.Stop();
				
				system.transform.position = GetPosition(Owner.transform.position);
				if (ApplyRotation)
				{
					system.transform.rotation = GetRotation(Owner.transform);    
				}

				if (ApplyScale)
				{
					system.transform.localScale = GetScale(Owner.transform);    
				}
            
				system.Clear();
			}
		}

		/// <summary>
		/// Gets the desired rotation of that particle system
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		protected virtual Quaternion GetRotation(Transform target)
		{
			switch (PositionMode)
			{
				case PositionModes.FeedbackPosition:
					return Owner.transform.rotation;
				case PositionModes.Transform:
					return InstantiateParticlesPosition.rotation;
				case PositionModes.WorldPosition:
					return Quaternion.identity;
				case PositionModes.Script:
					return Owner.transform.rotation;
				default:
					return Owner.transform.rotation;
			}
		}

		/// <summary>
		/// Gets the desired scale of that particle system
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		protected virtual Vector3 GetScale(Transform target)
		{
			switch (PositionMode)
			{
				case PositionModes.FeedbackPosition:
					return Owner.transform.localScale;
				case PositionModes.Transform:
					return InstantiateParticlesPosition.localScale;
				case PositionModes.WorldPosition:
					return Owner.transform.localScale;
				case PositionModes.Script:
					return Owner.transform.localScale;
				default:
					return Owner.transform.localScale;
			}
		}

		/// <summary>
		/// Gets the position 
		/// </summary>
		/// <param name="position"></param>
		/// <returns></returns>
		protected virtual Vector3 GetPosition(Vector3 position)
		{
			switch (PositionMode)
			{
				case PositionModes.FeedbackPosition:
					return Owner.transform.position + Offset;
				case PositionModes.Transform:
					return InstantiateParticlesPosition.position + Offset;
				case PositionModes.WorldPosition:
					return TargetWorldPosition + Offset;
				case PositionModes.Script:
					return position + Offset;
				default:
					return position + Offset;
			}
		}

		/// <summary>
		/// On Play, plays the feedback
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			if (Mode == Modes.Pool)
			{
				if (_objectPooler != null)
				{
					_newGameObject = _objectPooler.GetPooledGameObject();
					_instantiatedParticleSystem = _newGameObject.MMFGetComponentNoAlloc<ParticleSystem>();
					if (_instantiatedParticleSystem != null)
					{
						PositionParticleSystem(_instantiatedParticleSystem);
						_newGameObject.SetActive(true);
					}
				}
			}
			else
			{
				if (!ShouldCache)
				{
					InstantiateParticleSystem();
				}
				else
				{
					GrabCachedParticleSystem();
				}
			}
			
			if (_instantiatedParticleSystem != null)
			{
				if (ForceSetActiveOnPlay)
				{
					_instantiatedParticleSystem.gameObject.SetActive(true);
				}
				_instantiatedParticleSystem.Stop();
				_instantiatedParticleSystem.transform.position = GetPosition(position);
				PlayTargetParticleSystem(_instantiatedParticleSystem);
			}

			if ((_instantiatedRandomParticleSystems != null) && (_instantiatedRandomParticleSystems.Count > 0))
			{
				foreach (ParticleSystem system in _instantiatedRandomParticleSystems)
				{
                    
					if (ForceSetActiveOnPlay)
					{
						system.gameObject.SetActive(true);
					}
					system.Stop();
					system.transform.position = GetPosition(position);
				}
				int random = Random.Range(0, _instantiatedRandomParticleSystems.Count);
				PlayTargetParticleSystem(_instantiatedRandomParticleSystems[random]);
			}
		}

		/// <summary>
		/// Forces the sim speed if needed, then plays the target particle system
		/// </summary>
		/// <param name="targetParticleSystem"></param>
		protected virtual void PlayTargetParticleSystem(ParticleSystem targetParticleSystem)
		{
			if (ForceSimulationSpeed)
			{
				ParticleSystem.MainModule main = targetParticleSystem.main;
				main.simulationSpeed = Random.Range(ForcedSimulationSpeed.x, ForcedSimulationSpeed.y);
			}
			targetParticleSystem.Play();
		}

		/// <summary>
		/// Grabs and stores a random particle prefab
		/// </summary>
		protected virtual void GrabCachedParticleSystem()
		{
			if (RandomParticlePrefabs.Count > 0)
			{
				int random = Random.Range(0, RandomParticlePrefabs.Count);
				_instantiatedParticleSystem = _instantiatedRandomParticleSystems[random];
			}
		}

		/// <summary>
		/// On Stop, stops the feedback
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			if (_instantiatedParticleSystem != null)
			{
				_instantiatedParticleSystem?.Stop();
			}    
			if ((_instantiatedRandomParticleSystems != null) && (_instantiatedRandomParticleSystems.Count > 0))
			{
				foreach(ParticleSystem system in _instantiatedRandomParticleSystems)
				{
					system.Stop();
				}
			}
		}

		/// <summary>
		/// On Reset, stops the feedback
		/// </summary>
		protected override void CustomReset()
		{
			base.CustomReset();
            
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			if (InCooldown)
			{
				return;
			}

			if (StopOnReset && (_instantiatedParticleSystem != null))
			{
				_instantiatedParticleSystem.Stop();
			}
			if ((_instantiatedRandomParticleSystems != null) && (_instantiatedRandomParticleSystems.Count > 0))
			{
				foreach (ParticleSystem system in _instantiatedRandomParticleSystems)
				{
					system.Stop();
				}
			}
		}
	}
}