using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 결정은 레벨이 로드된 후 지정된 기간(초)이 지나면 true를 반환합니다.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Decisions/AIDecisionTimeSinceStart")]
	public class AIDecisionTimeSinceStart : AIDecision
	{
        /// true를 반환할 때까지의 기간(초)입니다.
        [Tooltip("true를 반환할 때까지의 기간(초)입니다.")]
		public float AfterTime;

		protected float _startTime;

		/// <summary>
		/// On init we store our current time
		/// </summary>
		public override void Initialization()
		{
			_startTime = Time.time;
		}

		/// <summary>
		/// On Decide we evaluate our time since the level has started
		/// </summary>
		/// <returns></returns>
		public override bool Decide()
		{
			return EvaluateTime();
		}

		/// <summary>
		/// Returns true if the time since the level has started has exceeded our requirements
		/// </summary>
		/// <returns></returns>
		protected virtual bool EvaluateTime()
		{
			return (Time.time - _startTime >= AfterTime);
		}
	}
}