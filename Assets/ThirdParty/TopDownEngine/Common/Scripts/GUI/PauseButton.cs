using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
	/// <summary>
	/// A simple component meant to be added to the pause button
	/// </summary>
	[AddComponentMenu("TopDown Engine/GUI/PauseButton")]
	public class PauseButton : TopDownMonoBehaviour
	{
		/// <summary>
		/// Triggers a pause event
		/// </summary>
		public virtual void PauseButtonAction()
		{
            // GameManager 및 이를 수신할 수 있는 다른 클래스에 대해 Pause 이벤트를 트리거합니다.
            StartCoroutine(PauseButtonCo());

		}

        /// <summary>
        /// UnPause 이벤트를 통해 게임 일시정지를 해제합니다.
        /// </summary>
        public virtual void UnPause()
		{
			StartCoroutine(PauseButtonCo());
		}

        /// <summary>
        /// 일시 중지 이벤트를 트리거하는 데 사용되는 코루틴
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator PauseButtonCo()
		{
			yield return null;
            // GameManager 및 이를 수신할 수 있는 다른 클래스에 대해 Pause 이벤트를 트리거합니다.
            TopDownEngineEvent.Trigger(TopDownEngineEventTypes.TogglePause, null);
		}

	}
}