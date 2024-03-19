using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 인스턴스화될 때 배경 음악을 재생하도록 하려면 이 클래스를 GameObject에 추가하세요.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Sound/BackgroundMusic")]
	public class BackgroundMusic : TopDownMonoBehaviour
	{
		/// the background music
		[Tooltip("배경 음악으로 사용할 오디오 클립")]
		public AudioClip SoundClip;
        /// 음악이 반복되어야 하는지 여부
        [Tooltip("음악이 반복되어야 하는지 여부")]
		public bool Loop = true;
        /// 이 배경음악을 만들 때 사용하는 ID
        [Tooltip("이 배경음악을 만들 때 사용하는 ID")]
		public int ID = 255;


		/// <summary>
		/// Gets the AudioSource associated to that GameObject, and asks the GameManager to play it.
		/// </summary>
		protected virtual void Start()
		{
			MMSoundManagerPlayOptions options = MMSoundManagerPlayOptions.Default;
			options.ID = ID;
			options.Loop = Loop;
			options.Location = Vector3.zero;
			options.MmSoundManagerTrack = MMSoundManager.MMSoundManagerTracks.Music;
            
			MMSoundManagerSoundPlayEvent.Trigger(SoundClip, options);
		}
	}
}