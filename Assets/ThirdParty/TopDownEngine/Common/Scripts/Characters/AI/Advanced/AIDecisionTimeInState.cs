using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 결정은 두뇌가 이 결정의 상태에 있었던 이후 최소값과 최대값(초) 사이에서 무작위로 선택된 지정된 기간 후에 true를 반환합니다. 다른 작업을 수행한 후 X초 후에 특정 작업을 수행하는 데 사용합니다.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Decisions/AIDecisionTimeInState")]
	public class AIDecisionTimeInState : AIDecision
	{
        /// true를 반환할 때까지의 최소 기간(초)입니다.
        [Tooltip("true를 반환할 때까지의 최소 기간(초)입니다.")]
		public float AfterTimeMin = 2f;
        /// true를 반환할 때까지의 최대 기간(초)입니다.
        [Tooltip("true를 반환할 때까지의 최대 기간(초)입니다.")]
		public float AfterTimeMax = 2f;

		protected float _randomTime;

		/// <summary>
		/// On Decide we evaluate our time
		/// </summary>
		/// <returns></returns>
		public override bool Decide()
		{
			return EvaluateTime();
		}

		/// <summary>
		/// Returns true if enough time has passed since we entered the current state
		/// </summary>
		/// <returns></returns>
		protected virtual bool EvaluateTime()
		{
			if (_brain == null) { return false; }
			return (_brain.TimeInThisState >= _randomTime);
		}

		/// <summary>
		/// On init we randomize our next delay
		/// </summary>
		public override void Initialization()
		{
			base.Initialization();
			RandomizeTime();
		}

		/// <summary>
		/// On enter state we randomize our next delay
		/// </summary>
		public override void OnEnterState()
		{
			base.OnEnterState();
			RandomizeTime();
		}

		/// <summary>
		/// On randomize time we randomize our next delay
		/// </summary>
		protected virtual void RandomizeTime()
		{
			_randomTime = Random.Range(AfterTimeMin, AfterTimeMax);
		}
	}
}