using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 결정은 캐릭터가 이 프레임에 히트했거나 지정된 히트 수에 도달한 후에 true를 반환합니다.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Decisions/AIDecisionHit")]
	//[RequireComponent(typeof(Health))]
	public class AIDecisionHit : AIDecision
	{
        /// true를 반환하는 데 필요한 적중 횟수
        [Tooltip("true를 반환하는 데 필요한 적중 횟수")]
		public int NumberOfHits = 1;

		protected int _hitCounter;
		protected Health _health;

		/// <summary>
		/// On init we grab our Health component
		/// </summary>
		public override void Initialization()
		{
			_health = _brain.gameObject.GetComponentInParent<Health>();
			_hitCounter = 0;
		}

		/// <summary>
		/// On Decide we check whether we've been hit
		/// </summary>
		/// <returns></returns>
		public override bool Decide()
		{
			return EvaluateHits();
		}

		/// <summary>
		/// Checks whether we've been hit enough times
		/// </summary>
		/// <returns></returns>
		protected virtual bool EvaluateHits()
		{
			return (_hitCounter >= NumberOfHits);
		}

		/// <summary>
		/// On EnterState, resets the hit counter
		/// </summary>
		public override void OnEnterState()
		{
			base.OnEnterState();
			_hitCounter = 0;
		}

		/// <summary>
		/// On exit state, resets the hit counter
		/// </summary>
		public override void OnExitState()
		{
			base.OnExitState();
			_hitCounter = 0;
		}

		/// <summary>
		/// When we get hit we increase our hit counter
		/// </summary>
		protected virtual void OnHit()
		{
			_hitCounter++;
		}

		/// <summary>
		/// Grabs our health component and starts listening for OnHit events
		/// </summary>
		protected virtual void OnEnable()
		{
			if (_health == null)
			{
				_health = _brain.gameObject.GetComponentInParent<Health>();
			}

			if (_health != null)
			{
				_health.OnHit += OnHit;
			}
		}

		/// <summary>
		/// Stops listening for OnHit events
		/// </summary>
		protected virtual void OnDisable()
		{
			if (_health != null)
			{
				_health.OnHit -= OnHit;
			}
		}
	}
}