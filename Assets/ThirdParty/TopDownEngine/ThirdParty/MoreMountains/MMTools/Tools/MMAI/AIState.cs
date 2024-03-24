using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
	[System.Serializable]
	public class AIActionsList : MMReorderableArray<AIAction>
	{
	}
	[System.Serializable]
	public class AITransitionsList : MMReorderableArray<AITransition>
	{
	}

    /// <summary>
    /// 상태는 하나 이상의 작업과 하나 이상의 전환의 조합입니다. 상태의 예로는 "_적이 범위 내에 들어올 때까지 순찰 중_"이 있습니다.
    /// </summary>
    [System.Serializable]
	public class AIState 
	{
		/// the name of the state (will be used as a reference in Transitions
		public string StateName;

		[MMReorderableAttribute(null, "Action", null)]
		public AIActionsList Actions;
		[MMReorderableAttribute(null, "Transition", null)]
		public AITransitionsList Transitions;/*

        /// a list of actions to perform in this state
        public List<AIAction> Actions;
        /// a list of transitions to evaluate to exit this state
        public List<AITransition> Transitions;*/

		protected AIBrain _brain;

        /// <summary>
        /// 이 상태의 브레인을 매개변수에 지정된 상태로 설정합니다.
        /// </summary>
        /// <param name="brain"></param>
        public virtual void SetBrain(AIBrain brain)
		{
			_brain = brain;
		}

        /// <summary>
        /// Enter 상태에서 해당 정보를 행동과 결정에 전달합니다.
        /// </summary>
        public virtual void EnterState()
		{
			foreach (AIAction action in Actions)
			{
				action.OnEnterState();
			}
			foreach (AITransition transition in Transitions)
			{
				if (transition.Decision != null)
				{
					transition.Decision.OnEnterState();
				}
			}
		}

		/// <summary>
		/// On exit state we pass that info to our actions and decisions
		/// </summary>
		public virtual void ExitState()
		{
			foreach (AIAction action in Actions)
			{
				action.OnExitState();
			}
			foreach (AITransition transition in Transitions)
			{
				if (transition.Decision != null)
				{
					transition.Decision.OnExitState();
				}
			}
		}

		/// <summary>
		/// Performs this state's actions
		/// </summary>
		public virtual void PerformActions()
		{
			if (Actions.Count == 0) { return; }
			for (int i=0; i<Actions.Count; i++) 
			{
				if (Actions[i] != null)
				{
					Actions[i].PerformAction();
				}
				else
				{
					Debug.LogError("An action in " + _brain.gameObject.name + " on state " + StateName + " is null.");
				}
			}
		}

        /// <summary>
        /// 이 상태의 전환을 테스트합니다.
        /// </summary>
        public virtual void EvaluateTransitions()
		{
			if (Transitions.Count == 0) { return; }
			for (int i = 0; i < Transitions.Count; i++) 
			{
				if (Transitions[i].Decision != null)
				{
					if (Transitions[i].Decision.Decide())
					{
						if (!string.IsNullOrEmpty(Transitions[i].TrueState))
						{
							_brain.TransitionToState(Transitions[i].TrueState);
							break;
						}
					}
					else
					{
						if (!string.IsNullOrEmpty(Transitions[i].FalseState))
						{
							_brain.TransitionToState(Transitions[i].FalseState);
							break;
						}
					}
				}                
			}
		}        
	}
}