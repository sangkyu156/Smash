using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    /// <summary>
    /// (게임의 모든 오디오 리스너에 배치된 경우) 보장하는 간단한 구성 요소
    /// "장면에 두 명의 오디오 청취자가 있습니다"라는 경고가 다시는 표시되지 않습니다.
    /// </summary>
    [RequireComponent(typeof(AudioListener))]
	public class MMAudioListener : MonoBehaviour
	{
		protected AudioListener _audioListener;
		protected AudioListener[] _otherListeners;
        
		/// <summary>
		/// On enable, disables other listeners if found
		/// </summary>
		protected virtual void OnEnable()
		{
			_audioListener = this.gameObject.GetComponent<AudioListener>();
			_otherListeners = FindObjectsOfType(typeof(AudioListener)) as AudioListener[];

			foreach (AudioListener audioListener in _otherListeners)
			{
				if ((audioListener != null) && (audioListener != _audioListener) )
				{
					audioListener.enabled = false;
				}    
			}
		}
	}    
}