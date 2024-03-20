using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 이 구성 요소는 MMF_Broadcast 피드백에 의해 자동으로 추가됩니다.
    /// </summary>
    public class MMF_BroadcastProxy : MonoBehaviour
	{
		/// the channel on which to broadcast
		[Tooltip("방송할 채널")]
		[MMReadOnly]
		public int Channel;
		/// a debug view of the current level being broadcasted
		[Tooltip("브로드캐스팅 중인 현재 레벨의 디버그 뷰")]
		[MMReadOnly]
		public float DebugLevel;
		/// whether or not a broadcast is in progress (will be false while the value is not changing, and thus not broadcasting)
		[Tooltip("방송 진행 여부(값이 변하지 않는 동안은 false가 되어 방송되지 않음)")]
		[MMReadOnly]
		public bool BroadcastInProgress = false;

		public float ThisLevel { get; set; }
		protected float _levelLastFrame;

		/// <summary>
		/// On Update we process our broadcast
		/// </summary>
		protected virtual void Update()
		{
			ProcessBroadcast();
		}

		/// <summary>
		/// Broadcasts the value if needed
		/// </summary>
		protected virtual void ProcessBroadcast()
		{
			BroadcastInProgress = false;
			if (ThisLevel != _levelLastFrame)
			{
				MMRadioLevelEvent.Trigger(Channel, ThisLevel);
				BroadcastInProgress = true;
			}
			DebugLevel = ThisLevel;
			_levelLastFrame = ThisLevel;
		}
	}    
}