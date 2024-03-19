using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MoreMountains.Tools
{
    /// <summary>
    /// 결정은 모든 프레임의 전환에 의해 평가되고 true 또는 false를 반환하는 구성 요소입니다. 예를 들어 특정 상태에서 소요된 시간, 대상까지의 거리, 영역 내의 물체 감지 등이 있습니다.
    /// </summary>
    public abstract class AIDecision : MonoBehaviour
	{
		/// Decide will be performed every frame while the Brain is in a state this Decision is in. Should return true or false, which will then determine the transition's outcome.
		public abstract bool Decide();

		public string Label;
		public bool DecisionInProgress { get; set; }
		protected AIBrain _brain;
        
		/// <summary>
		/// On Awake we grab our Brain
		/// </summary>
		protected virtual void Awake()
		{
			_brain = this.gameObject.GetComponentInParent<AIBrain>();
		}

		/// <summary>
		/// Meant to be overridden, called when the game starts
		/// </summary>
		public virtual void Initialization()
		{

		}

		/// <summary>
		/// Meant to be overridden, called when the Brain enters a State this Decision is in
		/// </summary>
		public virtual void OnEnterState()
		{
			DecisionInProgress = true;
		}

		/// <summary>
		/// Meant to be overridden, called when the Brain exits a State this Decision is in
		/// </summary>
		public virtual void OnExitState()
		{
			DecisionInProgress = false;
		}
	}
}