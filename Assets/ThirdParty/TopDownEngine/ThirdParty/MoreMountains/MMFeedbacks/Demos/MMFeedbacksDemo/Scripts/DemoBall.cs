using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// MMFeedbacks 데모에 포함된 공의 수명 주기를 처리하는 클래스
    /// 공이 생성된 후 2초를 기다린 후 공을 파괴하고 그 동안 MMFeedbacks를 재생합니다.
    /// </summary>
    public class DemoBall : MonoBehaviour
	{
		/// the duration (in seconds) of the life of the ball
		public float LifeSpan = 2f;
		/// the feedback to play when the ball dies
		public MMFeedbacks DeathFeedback;


		/// <summary>
		/// On start, we trigger the programmed death of the ball
		/// </summary>
		protected virtual void Start()
		{
			StartCoroutine(ProgrammedDeath());
		}

		/// <summary>
		/// Waits for 2 seconds, then kills the ball object after having played the MMFeedbacks
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator ProgrammedDeath()
		{
			yield return MMCoroutine.WaitFor(LifeSpan);
			DeathFeedback?.PlayFeedbacks();
			this.gameObject.SetActive(false);
		}
	}
}