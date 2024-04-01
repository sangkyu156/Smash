using UnityEngine;
using System.Collections;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 이 구성 요소를 개체에 추가하면 Start() 후 X초 후에 자동으로 삭제됩니다.
    /// </summary>
    [AddComponentMenu("More Mountains/Tools/Activation/MMTimedDestruction")]
	public class MMTimedDestruction : MonoBehaviour
	{
        /// 가능한 파괴 모드
        public enum TimedDestructionModes { Destroy, Disable }

        /// 이 객체의 파괴 모드: 파괴 또는 비활성화
        public TimedDestructionModes TimeDestructionMode = TimedDestructionModes.Destroy;
        /// 객체를 파괴하기 전의 시간(초)
        public float TimeBeforeDestruction=2;

		/// <summary>
		/// On Start(), we schedule the object's destruction
		/// </summary>
		protected virtual void Start ()
		{
			StartCoroutine(Destruction());
		}

        /// <summary>
        /// TimeBeforeDestruction 초 후에 객체를 파괴합니다.
        /// </summary>
        protected virtual IEnumerator Destruction()
		{
			yield return MMCoroutine.WaitFor(TimeBeforeDestruction);

			if (TimeDestructionMode == TimedDestructionModes.Destroy)
			{
				Destroy(gameObject);
			}
			else
			{
				gameObject.SetActive(false);
			}	        
		}
	}
}