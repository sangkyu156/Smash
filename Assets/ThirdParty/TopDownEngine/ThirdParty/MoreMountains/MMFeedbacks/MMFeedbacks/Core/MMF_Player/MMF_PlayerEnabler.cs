using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// AutoPlayOnEnable 모드에 있는 경우 MMFeedbacks에 의해 자동으로 추가되는 도우미 클래스
    /// 부모 게임 개체가 비활성화/활성화되면 다시 플레이할 수 있습니다.
    /// </summary>
    [AddComponentMenu("")]
	public class MMF_PlayerEnabler : MonoBehaviour
	{
		/// the MMFeedbacks to pilot
		public MMF_Player TargetMmfPlayer { get; set; }
        
		/// <summary>
		/// On enable, we re-enable (and thus play) our MMFeedbacks if needed
		/// </summary>
		protected virtual void OnEnable()
		{
			if ((TargetMmfPlayer != null) && !TargetMmfPlayer.enabled && TargetMmfPlayer.AutoPlayOnEnable)
			{
				TargetMmfPlayer.enabled = true;
			}
		}
	}    
}