using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 일시정지 버튼에 추가할 간단한 구성요소
    /// </summary>
    [AddComponentMenu("TopDown Engine/GUI/PauseButton")]
	public class PauseButton : TopDownMonoBehaviour
	{
        /// <summary>
        /// 일시 중지 이벤트를 트리거합니다.
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