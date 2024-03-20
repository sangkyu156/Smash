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
	public class MMFeedbackParticles : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.ParticlesColor; } }
		#endif
        
		public enum Modes { Play, Stop, Pause }

		[Header("Bound Particles")]
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

		/// <summary>
		/// On init we stop our particle system
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(GameObject owner)
		{
			base.CustomInitialization(owner);
			StopParticles();
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
				switch (Mode)
				{
					case Modes.Play:
						RandomParticleSystems[random].Play();
						break;
					case Modes.Stop:
						RandomParticleSystems[random].Stop();
						break;
					case Modes.Pause:
						RandomParticleSystems[random].Pause();
						break;
				}
				return;
			}
			else if (BoundParticleSystem != null)
			{
				switch (Mode)
				{
					case Modes.Play:
						BoundParticleSystem?.Play();
						break;
					case Modes.Stop:
						BoundParticleSystem?.Stop();
						break;
					case Modes.Pause:
						BoundParticleSystem?.Pause();
						break;
				}
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