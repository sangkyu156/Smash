using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 이 피드백은 재생 시 관련 입자 시스템을 재생하고 중지 시 중지합니다.
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("이 피드백은 재생 시 지정된 ParticleSystem(장면에서)을 재생합니다.")]
	[FeedbackPath("Particles/Particles Play")]
	public class MMF_Particles : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(DeclaredDuration); } set { DeclaredDuration = value;  } }
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.ParticlesColor; } }
		public override bool EvaluateRequiresSetup() { return (BoundParticleSystem == null); }
		public override string RequiredTargetText { get { return BoundParticleSystem != null ? BoundParticleSystem.name : "";  } }
		public override string RequiresSetupText { get { return "This feedback requires that a BoundParticleSystem be set to be able to work properly. You can set one below."; } }
		#endif
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => BoundParticleSystem = FindAutomatedTarget<ParticleSystem>();
        
		public enum Modes { Play, Stop, Pause }

		[MMFInspectorGroup("Bound Particles", true, 41, true)]
		/// whether to Play, Stop or Pause the target particle system when that feedback is played
		[Tooltip("해당 피드백이 재생될 때 대상 입자 시스템을 재생, 중지 또는 일시 중지할지 여부")]
		public Modes Mode = Modes.Play;
		/// the particle system to play with this feedback
		[Tooltip("이 피드백을 다룰 파티클 시스템")]
		public ParticleSystem BoundParticleSystem;
		/// a list of (optional) particle systems 
		[Tooltip("(선택적) 입자 시스템 목록")]
		public List<ParticleSystem> RandomParticleSystems;
		/// if this is true, the particles will be moved to the position passed in parameters
		[Tooltip("이것이 사실이라면 입자는 매개변수에 전달된 위치로 이동됩니다.")]
		public bool MoveToPosition = false;
		/// if this is true, the particle system's object will be set active on play
		[Tooltip("이것이 사실이라면 입자 시스템의 객체는 플레이 시 활성 상태로 설정됩니다.")]
		public bool ActivateOnPlay = false;
		/// if this is true, the particle system will be stopped on initialization
		[Tooltip("이것이 사실이라면 초기화 시 파티클 시스템이 중지됩니다.")]
		public bool StopSystemOnInit = true;
		/// the duration for the player to consider. This won't impact your particle system, but is a way to communicate to the MMF Player the duration of this feedback. Usually you'll want it to match your actual particle system, and setting it can be useful to have this feedback work with holding pauses.
		[Tooltip("플레이어가 고려해야 할 기간. 이는 입자 시스템에 영향을 주지 않지만 이 피드백 기간을 MMF 플레이어에 전달하는 방법입니다. 일반적으로 실제 입자 시스템과 일치하도록 설정하고 일시 중지를 유지하면서 이 피드백이 작동하도록 설정하는 것이 유용할 수 있습니다.")]
		public float DeclaredDuration = 0f;

		[MMFInspectorGroup("Simulation Speed", true, 43, false)]
		/// whether or not to force a specific simulation speed on the target particle system(s)
		[Tooltip("대상 입자 시스템에 특정 시뮬레이션 속도를 강제할지 여부")]
		public bool ForceSimulationSpeed = false;
		/// The min and max values at which to randomize the simulation speed, if ForceSimulationSpeed is true. A new value will be randomized every time this feedback plays
		[Tooltip("ForceSimulationSpeed가 true인 경우 시뮬레이션 속도를 무작위화할 최소 및 최대 값입니다. 이 피드백이 재생될 때마다 새로운 값이 무작위로 지정됩니다.")]
		[MMFCondition("ForceSimulationSpeed", true)]
		public Vector2 ForcedSimulationSpeed = new Vector2(0.1f,1f);

		/// <summary>
		/// On init we stop our particle system
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);
			if (StopSystemOnInit)
			{
				StopParticles();
			}
		}

		/// <summary>
		/// On play we play our particle system
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			PlayParticles(position);
		}
        
		/// <summary>
		/// On Stop, stops the particle system
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			StopParticles();
		}

		/// <summary>
		/// On Reset, stops the particle system 
		/// </summary>
		protected override void CustomReset()
		{
			base.CustomReset();

			if (InCooldown)
			{
				return;
			}

			StopParticles();
		}

		/// <summary>
		/// Plays a particle system
		/// </summary>
		/// <param name="position"></param>
		protected virtual void PlayParticles(Vector3 position)
		{
			if (MoveToPosition)
			{
				BoundParticleSystem.transform.position = position;
				foreach (ParticleSystem system in RandomParticleSystems)
				{
					system.transform.position = position;
				}
			}

			if (ActivateOnPlay)
			{
				BoundParticleSystem.gameObject.SetActive(true);
				foreach (ParticleSystem system in RandomParticleSystems)
				{
					system.gameObject.SetActive(true);
				}
			}

			if (RandomParticleSystems.Count > 0)
			{
				int random = Random.Range(0, RandomParticleSystems.Count);
				HandleParticleSystemAction(RandomParticleSystems[random]);
			}
			else if (BoundParticleSystem != null)
			{
				HandleParticleSystemAction(BoundParticleSystem);
			}
		}

		/// <summary>
		/// Changes the target particle system's sim speed if needed, and calls the specified action on it
		/// </summary>
		/// <param name="targetParticleSystem"></param>
		protected virtual void HandleParticleSystemAction(ParticleSystem targetParticleSystem)
		{
			if (ForceSimulationSpeed)
			{
				ParticleSystem.MainModule main = targetParticleSystem.main;
				main.simulationSpeed = Random.Range(ForcedSimulationSpeed.x, ForcedSimulationSpeed.y);
			}
			
			switch (Mode)
			{
				case Modes.Play:
					targetParticleSystem?.Play();
					break;
				case Modes.Stop:
					targetParticleSystem?.Stop();
					break;
				case Modes.Pause:
					targetParticleSystem?.Pause();
					break;
			}
		}

		/// <summary>
		/// Stops all particle systems
		/// </summary>
		protected virtual void StopParticles()
		{
			foreach(ParticleSystem system in RandomParticleSystems)
			{
				system?.Stop();
			}
			if (BoundParticleSystem != null)
			{
				BoundParticleSystem.Stop();
			}            
		}
	}
}