using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 인스턴스화될 때 배경 음악을 재생하도록 하려면 이 클래스를 GameObject에 추가하세요.
    /// 주의: 한 번에 하나의 배경 음악만 재생됩니다.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Sound/PersistentBackgroundMusic")]
	public class PersistentBackgroundMusic : MMPersistentSingleton<PersistentBackgroundMusic>
	{
		/// the background music clip to use as persistent background music
		[Tooltip("지속적인 배경 음악으로 사용할 배경 음악 클립")]
		public AudioClip SoundClip;
		/// whether or not the music should loop
		[Tooltip("음악이 반복되어야 하는지 여부")]
		public bool Loop = true;
        
		protected AudioSource _source;
		protected PersistentBackgroundMusic _otherBackgroundMusic;

		protected virtual void OnEnable()
		{
			_otherBackgroundMusic = (PersistentBackgroundMusic)FindObjectOfType(typeof(PersistentBackgroundMusic));
			if ((_otherBackgroundMusic != null) && (_otherBackgroundMusic != this) )
			{
				_otherBackgroundMusic.enabled = false;
			}
		}

		/// <summary>
		/// Gets the AudioSource associated to that GameObject, and asks the GameManager to play it.
		/// </summary>
		protected virtual void Start()
		{
			MMSoundManagerPlayOptions options = MMSoundManagerPlayOptions.Default;
			options.Loop = Loop;
			options.Location = Vector3.zero;
			options.MmSoundManagerTrack = MMSoundManager.MMSoundManagerTracks.Music;
			options.Persistent = true;
            
			MMSoundManagerSoundPlayEvent.Trigger(SoundClip, options);
		}
	}
}