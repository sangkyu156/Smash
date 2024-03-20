using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;
using MoreMountains.Tools;
using UnityEngine.SceneManagement;

namespace MoreMountains.Feedbacks
{
	[ExecuteAlways]
	[AddComponentMenu("")]
	[FeedbackPath("Audio/Sound")]
	[FeedbackHelp("이 피드백을 사용하면 이벤트(MMSoundManager와 같은 MMSfxEvent를 포착하려면 장면에 무언가가 필요함)를 통해 또는 캐시(AudioSource가 초기화 시 생성된 다음 재생할 준비가 됨)를 통해 지정된 AudioClip을 재생할 수 있습니다. 또는 주문형(Play에서 인스턴스화). 이러한 모든 방법에 대해 최소/최대 경계(임의성을 원하지 않는 경우 두 필드에 동일한 값을 설정), 임의 피치 및 선택적 AudioMixerGroup 사이에서 임의의 볼륨을 정의할 수 있습니다.")]
	public class MMFeedbackSound : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.SoundsColor; } }
#endif

        /// <summary>
        /// 사운드를 재생할 수 있는 방법.
        /// Event : MMSfxEvent를 보내는 경우 이 이벤트를 포착하고 사운드를 재생하려면 클래스가 필요합니다.
        /// Cached : 소유자를 부모로 삼아 사운드를 재생할 오디오 소스를 생성하고 저장합니다.
        /// OnDemand : 오디오 소스를 생성하고 사운드를 재생하려고 할 때마다 이를 파괴합니다.
        /// </summary>
        public enum PlayMethods { Event, Cached, OnDemand, Pool }

		[Header("Sound")]
		/// the sound clip to play
		[Tooltip("재생할 사운드 클립")]
		public AudioClip Sfx;

		[Header("Random Sound")]
		/// an array to pick a random sfx from
		[Tooltip("임의의 sfx를 선택하는 배열")]
		public AudioClip[] RandomSfx;

		[Header("Test")]
		[MMFInspectorButton("TestPlaySound")]
		public bool TestButton;
		[MMFInspectorButton("TestStopSound")]
		public bool TestStopButton;

		[Header("Method")]
		/// the play method to use when playing the sound (event, cached or on demand)
		[Tooltip("사운드를 재생할 때 사용할 재생 방법(이벤트, 캐시 또는 요청 시)")]
		public PlayMethods PlayMethod = PlayMethods.Event;
		/// the size of the pool when in Pool mode
		[Tooltip("풀 모드에 있을 때 풀의 크기")]
		[MMFEnumCondition("PlayMethod", (int)PlayMethods.Pool)]
		public int PoolSize = 10;

		[Header("Volume")]
		/// the minimum volume to play the sound at
		[Tooltip("the minimum volume to play the sound at")]
		public float MinVolume = 1f;
		/// the maximum volume to play the sound at
		[Tooltip("the maximum volume to play the sound at")]
		public float MaxVolume = 1f;

		[Header("Pitch")]
		/// the minimum pitch to play the sound at
		[Tooltip("the minimum pitch to play the sound at")]
		public float MinPitch = 1f;
		/// the maximum pitch to play the sound at
		[Tooltip("the maximum pitch to play the sound at")]
		public float MaxPitch = 1f;

		[Header("Mixer")]
		/// the audiomixer to play the sound with (optional)
		[Tooltip("사운드를 재생할 오디오 믹서(선택 사항)")]
		public AudioMixerGroup SfxAudioMixerGroup;
		[Tooltip("필요한 경우 0(가장 높음)에서 256 사이로 지정되는 오디오 소스 우선순위")] 
		public int Priority = 128;


		/// the duration of this feedback is the duration of the clip being played
		public override float FeedbackDuration { get { return GetDuration(); } }

		protected AudioClip _randomClip;
		protected AudioSource _cachedAudioSource;
		protected AudioSource[] _pool;
		protected AudioSource _tempAudioSource;
		protected float _duration;
		protected AudioSource _editorAudioSource;

		/// <summary>
		/// Custom init to cache the audiosource if required
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(GameObject owner)
		{
			base.CustomInitialization(owner);
			if (PlayMethod == PlayMethods.Cached)
			{
				_cachedAudioSource = CreateAudioSource(owner, "CachedFeedbackAudioSource");
			}
			if (PlayMethod == PlayMethods.Pool)
			{
				// create a pool
				_pool = new AudioSource[PoolSize];
				for (int i = 0; i < PoolSize; i++)
				{
					_pool[i] = CreateAudioSource(owner, "PooledAudioSource"+i);
				}
			}
		}

		protected virtual AudioSource CreateAudioSource(GameObject owner, string audioSourceName)
		{
			// we create a temporary game object to host our audio source
			GameObject temporaryAudioHost = new GameObject(audioSourceName);
			SceneManager.MoveGameObjectToScene(temporaryAudioHost.gameObject, this.gameObject.scene);
			// we set the temp audio's position
			temporaryAudioHost.transform.position = owner.transform.position;
			temporaryAudioHost.transform.SetParent(owner.transform);
			// we add an audio source to that host
			_tempAudioSource = temporaryAudioHost.AddComponent<AudioSource>() as AudioSource;
			_tempAudioSource.playOnAwake = false;
			return _tempAudioSource; 
		}

		/// <summary>
		/// Plays either a random sound or the specified sfx
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
            
			float intensityMultiplier = Timing.ConstantIntensity ? 1f : feedbacksIntensity;
            
			if (Sfx != null)
			{
				_duration = Sfx.length;
				PlaySound(Sfx, position, intensityMultiplier);
				return;
			}

			if (RandomSfx.Length > 0)
			{
				_randomClip = RandomSfx[Random.Range(0, RandomSfx.Length)];

				if (_randomClip != null)
				{
					_duration = _randomClip.length;
					PlaySound(_randomClip, position, intensityMultiplier);
				}
                
			}
		}

		protected virtual float GetDuration()
		{
			if (Sfx != null)
			{
				return Sfx.length;
			}

			float longest = 0f;
			if ((RandomSfx != null) && (RandomSfx.Length > 0))
			{
				foreach (AudioClip clip in RandomSfx)
				{
					if ((clip != null) && (clip.length > longest))
					{
						longest = clip.length;
					}
				}

				return longest;
			}

			return 0f;
		}

		/// <summary>
		/// Plays a sound differently based on the selected play method
		/// </summary>
		/// <param name="sfx"></param>
		/// <param name="position"></param>
		protected virtual void PlaySound(AudioClip sfx, Vector3 position, float intensity)
		{
			float volume = Random.Range(MinVolume, MaxVolume);
            
			if (!Timing.ConstantIntensity)
			{
				volume = volume * intensity;
			}
            
			float pitch = Random.Range(MinPitch, MaxPitch);

			int timeSamples = NormalPlayDirection ? 0 : sfx.samples - 1;
            
			if (!NormalPlayDirection)
			{
				pitch = -pitch;
			}
            
			if (PlayMethod == PlayMethods.Event)
			{
				MMSfxEvent.Trigger(sfx, SfxAudioMixerGroup, volume, pitch, Priority);
				return;
			}

			if (PlayMethod == PlayMethods.OnDemand)
			{
				// we create a temporary game object to host our audio source
				GameObject temporaryAudioHost = new GameObject("TempAudio");
				SceneManager.MoveGameObjectToScene(temporaryAudioHost.gameObject, this.gameObject.scene);
				// we set the temp audio's position
				temporaryAudioHost.transform.position = position;
				// we add an audio source to that host
				AudioSource audioSource = temporaryAudioHost.AddComponent<AudioSource>() as AudioSource;
				PlayAudioSource(audioSource, sfx, volume, pitch, timeSamples, SfxAudioMixerGroup, Priority);
				// we destroy the host after the clip has played
				Destroy(temporaryAudioHost, sfx.length);
			}

			if (PlayMethod == PlayMethods.Cached)
			{
				// we set that audio source clip to the one in paramaters
				PlayAudioSource(_cachedAudioSource, sfx, volume, pitch, timeSamples, SfxAudioMixerGroup, Priority);
			}

			if (PlayMethod == PlayMethods.Pool)
			{
				_tempAudioSource = GetAudioSourceFromPool();
				if (_tempAudioSource != null)
				{
					PlayAudioSource(_tempAudioSource, sfx, volume, pitch, timeSamples, SfxAudioMixerGroup, Priority);
				}
			}
		}

		/// <summary>
		/// Plays the audio source with the specified volume and pitch
		/// </summary>
		/// <param name="audioSource"></param>
		/// <param name="sfx"></param>
		/// <param name="volume"></param>
		/// <param name="pitch"></param>
		protected virtual void PlayAudioSource(AudioSource audioSource, AudioClip sfx, float volume, float pitch,
			int timeSamples, AudioMixerGroup audioMixerGroup = null, int priority = 128)
		{
			// we set that audio source clip to the one in paramaters
			audioSource.clip = sfx;
			audioSource.timeSamples = timeSamples;
			// we set the audio source volume to the one in parameters
			audioSource.volume = volume;
			audioSource.pitch = pitch;
			// we set our loop setting
			audioSource.loop = false;
			audioSource.priority = priority;
			if (audioMixerGroup != null)
			{
				audioSource.outputAudioMixerGroup = audioMixerGroup;
			}
			// we start playing the sound
			audioSource.Play(); 
		}

		/// <summary>
		/// Gets an audio source from the pool if possible
		/// </summary>
		/// <returns></returns>
		protected virtual AudioSource GetAudioSourceFromPool()
		{
			for (int i = 0; i < PoolSize; i++)
			{
				if (!_pool[i].isPlaying)
				{
					return _pool[i];
				}
			}
			return null;
		}

		/// <summary>
		/// A test method that creates an audiosource, plays it, and destroys itself after play
		/// </summary>
		protected virtual async void TestPlaySound()
		{
			AudioClip tmpAudioClip = null;

			if (Sfx != null)
			{
				tmpAudioClip = Sfx;
			}

			if (RandomSfx.Length > 0)
			{
				tmpAudioClip = RandomSfx[Random.Range(0, RandomSfx.Length)];
			}

			if (tmpAudioClip == null)
			{
				Debug.LogError(Label + " on " + this.gameObject.name + " can't play in editor mode, you haven't set its Sfx.");
				return;
			}

			float volume = Random.Range(MinVolume, MaxVolume);
			float pitch = Random.Range(MinPitch, MaxPitch);
			GameObject temporaryAudioHost = new GameObject("EditorTestAS_WillAutoDestroy");
			SceneManager.MoveGameObjectToScene(temporaryAudioHost.gameObject, this.gameObject.scene);
			temporaryAudioHost.transform.position = this.transform.position;
			_editorAudioSource = temporaryAudioHost.AddComponent<AudioSource>() as AudioSource;
			PlayAudioSource(_editorAudioSource, tmpAudioClip, volume, pitch, 0);
			float length = 1000 * tmpAudioClip.length;
			await Task.Delay((int)length);
			DestroyImmediate(temporaryAudioHost);
		}

		/// <summary>
		/// A test method that stops the test sound
		/// </summary>
		protected virtual void TestStopSound()
		{
			if (_editorAudioSource != null)
			{
				_editorAudioSource.Stop();
			}            
		}
	}
}