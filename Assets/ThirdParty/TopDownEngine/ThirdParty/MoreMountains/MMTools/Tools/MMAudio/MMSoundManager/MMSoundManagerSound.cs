using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    /// <summary>
    ///MMSoundManager가 재생하는 사운드에 대한 정보를 저장하는 데 사용되는 간단한 구조체입니다.
    /// </summary>
    [Serializable]
	public struct MMSoundManagerSound
	{
		/// the ID of the sound 
		public int ID;
		/// the track the sound is being played on
		public MMSoundManager.MMSoundManagerTracks Track;
		/// the associated audiosource
		public AudioSource Source;
		/// whether or not this sound will play over multiple scenes
		public bool Persistent;

		public float PlaybackTime;
		public float PlaybackDuration;
	}
}