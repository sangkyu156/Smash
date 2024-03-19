using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 결정은 캐릭터가 근거가 있는 경우 true를 반환하고, 그렇지 않으면 false를 반환합니다.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Decisions/AIDecisionGrounded")]
	public class AIDecisionGrounded : AIDecision
	{
        /// 이 결정 상태에 진입한 후 접지 상태를 무시하는 기간(초)입니다.
        [Tooltip("이 결정 상태에 진입한 후 접지 상태를 무시하는 기간(초)입니다.")]
		public float GroundedBufferDelay = 0.2f;

		protected TopDownController _topDownController;
		protected float _startTime = 0f;

		/// <summary>
		/// On init we grab our TopDownController component
		/// </summary>
		public override void Initialization()
		{
			_topDownController = this.gameObject.GetComponentInParent<TopDownController>();
		}

		/// <summary>
		/// On Decide we check if we're grounded
		/// </summary>
		/// <returns></returns>
		public override bool Decide()
		{
			return EvaluateGrounded();
		}

		/// <summary>
		/// Checks whether the character is grounded
		/// </summary>
		/// <returns></returns>
		protected virtual bool EvaluateGrounded()
		{
			if (Time.time - _startTime < GroundedBufferDelay)
			{
				return false;
			}
			return (_topDownController.Grounded);
		}

		/// <summary>
		/// On Enter State we reset our start time
		/// </summary>
		public override void OnEnterState()
		{
			base.OnEnterState();
			_startTime = Time.time;
		}
	}
}