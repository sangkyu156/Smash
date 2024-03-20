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
	public class MMFeedbackParticlesInstantiation : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.ParticlesColor; } }
#endif
        /// 인스턴스화된 객체를 배치하는 다양한 방법:
        /// - FeedbackPosition : 객체는 피드백 위치와 선택적 오프셋에서 인스턴스화됩니다.
        /// - Transform : 객체는 지정된 변환 위치와 선택적 오프셋에서 인스턴스화됩니다.
        /// - WorldPosition : 객체는 지정된 월드 위치 벡터와 선택적 오프셋에서 인스턴스화됩니다.
        /// - Script : 피드백을 호출할 때 매개변수에 전달된 위치
        public enum PositionModes { FeedbackPosition, Transform, WorldPosition, Script }
        /// 가능한 배달 모드
        /// - cached : 파티클 시스템의 복사본을 캐시하고 재사용합니다.
        /// - on demand : 모든 플레이에 대해 새로운 입자 시스템을 인스턴스화합니다.
        public enum Modes { Cached, OnDemand }

		[Header("Particles Instantiation")]
		/// whether the particle system should be cached or created on demand the first time
		[Tooltip("파티클 시스템을 처음으로 캐시해야 하는지 아니면 요청 시 생성해야 하는지 여부")]
		public Modes Mode = Modes.Cached;
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

		[Header("Position")]
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

		protected ParticleSystem _instantiatedParticleSystem;
		protected List<ParticleSystem> _instantiatedRandomParticleSystems;

		/// <summary>
		/// On init, instantiates the particle system, positions it and nests it if needed
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(GameObject owner)
		{
			if (!Active)
			{
				return;
			}
			if (Mode == Modes.Cached)
			{
				InstantiateParticleSystem();
			}
		}

		/// <summary>
		/// Instantiates the particle system
		/// </summary>
		protected virtual void InstantiateParticleSystem()
		{
			if (CachedRecycle)
			{
				if (_instantiatedParticleSystem != null)
				{
					PositionParticleSystem(_instantiatedParticleSystem);
					return;
				}
			}

			Transform newParent = null;
            
			if (NestParticles)
			{
				if (PositionMode == PositionModes.FeedbackPosition)
				{
					newParent = this.transform;
				}
				if (PositionMode == PositionModes.Transform)
				{
					newParent = InstantiateParticlesPosition;
				}
			}
            

			if (RandomParticlePrefabs.Count > 0)
			{
				if (Mode == Modes.Cached)
				{
					_instantiatedRandomParticleSystems = new List<ParticleSystem>();
					foreach(ParticleSystem system in RandomParticlePrefabs)
					{
						ParticleSystem newSystem = GameObject.Instantiate(system, newParent) as ParticleSystem;
						if (newParent == null)
						{
							SceneManager.MoveGameObjectToScene(newSystem.gameObject, this.gameObject.scene);    
						}
						_instantiatedRandomParticleSystems.Add(newSystem);
					}
				}
				else
				{
					int random = Random.Range(0, RandomParticlePrefabs.Count);
					_instantiatedParticleSystem = GameObject.Instantiate(RandomParticlePrefabs[random], newParent) as ParticleSystem;
					if (newParent == null)
					{
						SceneManager.MoveGameObjectToScene(_instantiatedParticleSystem.gameObject, this.gameObject.scene);    
					}
				}
			}
			else
			{
				_instantiatedParticleSystem = GameObject.Instantiate(ParticlesPrefab, newParent) as ParticleSystem;
				if (newParent == null)
				{
					SceneManager.MoveGameObjectToScene(_instantiatedParticleSystem.gameObject, this.gameObject.scene);    
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

				system.transform.position = GetPosition(this.transform.position);
				if (ApplyRotation)
				{
					system.transform.rotation = GetRotation(this.transform);    
				}

				if (ApplyScale)
				{
					system.transform.localScale = GetScale(this.transform);    
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
					return this.transform.rotation;
				case PositionModes.Transform:
					return InstantiateParticlesPosition.rotation;
				case PositionModes.WorldPosition:
					return Quaternion.identity;
				case PositionModes.Script:
					return this.transform.rotation;
				default:
					return this.transform.rotation;
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
					return this.transform.localScale;
				case PositionModes.Transform:
					return InstantiateParticlesPosition.localScale;
				case PositionModes.WorldPosition:
					return this.transform.localScale;
				case PositionModes.Script:
					return this.transform.localScale;
				default:
					return this.transform.localScale;
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
					return this.transform.position + Offset;
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

			if (Mode == Modes.OnDemand)
			{
				InstantiateParticleSystem();
			}

			if (_instantiatedParticleSystem != null)
			{
				if (ForceSetActiveOnPlay)
				{
					_instantiatedParticleSystem.gameObject.SetActive(true);
				}
				_instantiatedParticleSystem.Stop();
				_instantiatedParticleSystem.transform.position = GetPosition(position);
				_instantiatedParticleSystem.Play();
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
				_instantiatedRandomParticleSystems[random].Play();
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

			if (_instantiatedParticleSystem != null)
			{
				_instantiatedParticleSystem?.Stop();
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