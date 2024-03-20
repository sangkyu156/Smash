using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 런타임에 이 클래스를 파티클 시스템에 추가하면 인스펙터에서 재생/일시 중지/중지할 수 있는 컨트롤이 노출됩니다.
    /// Unity의 내장 컨트롤에는 재생 모드에 있을 때 일시 중지 기능이 없기 때문입니다.
    /// </summary>
    [RequireComponent(typeof(ParticleSystem))]
	public class MMRuntimeParticleControl : MonoBehaviour
	{
        /// <summary>
        /// 추적기에 가능한 모드:
        /// Basic은 메인 모듈의 지속 시간과 함께 작동합니다.
        /// ForcedBounds를 사용하면 슬라이더가 이동해야 하는 범위를 지정할 수 있습니다.
        /// </summary>
        public enum TrackerModes { Basic, ForcedBounds }
        
		[Header("Base Controls")]
		/// a test button to play the associated particle system 
		[MMInspectorButton("Play")] public bool PlayButton;
		/// a test button to pause the associated particle system
		[MMInspectorButton("Pause")] public bool PauseButton;
		/// a test button to stop the associated particle system
		[MMInspectorButton("Stop")] public bool StopButton;
        
		[Header("Simulate")]
		/// the timestamp at which to go when pressing the Simulate button
		public float TargetTimestamp = 1f;
		/// a test button to move the associated particle system to the specified timestamp
		[MMInspectorButton("Simulate")] public bool FastForwardToTimeButton;

		[Header("Tracker")]
		/// the selected tracker mode
		public TrackerModes TrackerMode = TrackerModes.Basic;
		/// when in ForcedBounds mode, the value to which the slider's lowest bound should be remapped
		[MMEnumCondition("TrackerMode", (int)TrackerModes.ForcedBounds)]
		public float MinBound;
		/// when in ForcedBounds mode, the value to which the slider's highest bound should be remapped
		[MMEnumCondition("TrackerMode", (int)TrackerModes.ForcedBounds)]
		public float MaxBound;
		/// a slider used to move the particle system through time at runtime
		[Range(0f, 1f)]
		public float Tracker;
		[MMReadOnly] 
		public float Timestamp;

		protected ParticleSystem _particleSystem;
		protected ParticleSystem.MainModule _mainModule;
        
		/// <summary>
		/// On Awake we grab our components
		/// </summary>
		protected virtual void Awake()
		{
			_particleSystem = this.GetComponent<ParticleSystem>();
			_mainModule = _particleSystem.main;
		}

		/// <summary>
		/// Plays the particle system
		/// </summary>
		protected virtual void Play()
		{
			_particleSystem.Play();
		}

		/// <summary>
		/// Pauses the particle system
		/// </summary>
		protected virtual void Pause()
		{
			_particleSystem.Pause();
		}

		/// <summary>
		/// Stops the particle system
		/// </summary>
		protected virtual void Stop()
		{
			_particleSystem.Stop();
		}

		/// <summary>
		/// Moves the particle system to the specified timestamp
		/// </summary>
		protected virtual void Simulate()
		{
			_particleSystem.Simulate(TargetTimestamp, true, true);
		}

		/// <summary>
		/// On validate, moves the particle system to the chosen timestamp along the track
		/// </summary>
		protected void OnValidate()
		{
			float minBound = (TrackerMode == TrackerModes.Basic) ? 0f : MinBound;
			float maxBound = (TrackerMode == TrackerModes.Basic) ? _mainModule.duration : MaxBound;
			Timestamp = MMMaths.Remap(Tracker, 0f, 1f, minBound, maxBound);
			_particleSystem.Simulate(Timestamp, true, true);
		}
	}    
}