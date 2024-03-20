using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using MoreMountains.Tools;
using System;
using System.Collections.Generic;

namespace MoreMountains.Tools
{
	public struct MMStateChangeEvent<T> where T: struct, IComparable, IConvertible, IFormattable
	{
		public GameObject Target;
		public MMStateMachine<T> TargetStateMachine;
		public T NewState;
		public T PreviousState;

		public MMStateChangeEvent(MMStateMachine<T> stateMachine)
		{
			Target = stateMachine.Target;
			TargetStateMachine = stateMachine;
			NewState = stateMachine.CurrentState;
			PreviousState = stateMachine.PreviousState;
		}
	}

    /// <summary>
    /// 상태 머신의 공개 인터페이스입니다.
    /// </summary>
    public interface MMIStateMachine
	{
		bool TriggerEvents { get; set; }
	}

    /// <summary>
    /// StateMachine 관리자는 단순성을 염두에 두고 설계되었습니다(어차피 상태 머신만큼 단순할 수 있음).
    /// 이를 사용하려면 열거형이 필요합니다. 예: public enum CharacterConditions { Normal, ControlledMovement, Frozen, Paused, Dead }
    /// 다음과 같이 선언합니다. public StateMachine<CharacterConditions> ConditionStateMachine;
    /// 다음과 같이 초기화합니다. ConditionStateMachine = new StateMachine<CharacterConditions>();
    /// 그런 다음 어디서든 필요할 때 상태를 업데이트하기만 하면 됩니다. 예를 들면 다음과 같습니다. ConditionStateMachine.ChangeState(CharacterConditions.Dead);
    /// 상태 머신은 현재 및 이전 상태를 저장하고 언제든지 액세스할 수 있으며 선택적으로 이러한 상태의 시작/종료 시 이벤트를 트리거합니다.
    /// </summary>
    public class MMStateMachine<T> : MMIStateMachine where T : struct, IComparable, IConvertible, IFormattable
	{
        /// TriggerEvents를 true로 설정하면 상태 시스템은 상태에 들어가고 나갈 때 이벤트를 트리거합니다.
        /// 또한 다음과 같이 대리자의 하드 바인딩 없이 모든 리스너에서 수신할 수 있는 상태 변경 시 이벤트를 트리거하는 옵션이 있습니다.
        /// 일부 클래스에 공개 MMStateMachine<CharacterStates.MovementStates> MovementState가 있고 이를 사용하여 움직이는 캐릭터의 상태(유휴, 걷기, 달리기 등)를 추적한다고 가정해 보겠습니다.
        /// 다른 클래스에서는 할 수 있습니다. :
        /// public class TestListener : MonoBehaviour, MMEventListener<MMStateChangeEvent<CharacterStates.MovementStates>>
        /// {
        /// 	// triggered every time a state change event occurs
        /// 	public void OnMMEvent(MMStateChangeEvent<CharacterStates.MovementStates> stateChangeEvent)
        /// 	{
        /// 		if (stateChangeEvent.NewState == CharacterStates.MovementStates.Crawling)
        /// 		{
        /// 			//do something - in a real life scenario you'd probably make sure you have the right target, etc.
        /// 		}
        /// 	}
        /// 
        /// 	private void OnEnable() // on enable we start listening for these events
        /// 	{
        /// 		MMEventManager.AddListener<MMStateChangeEvent<CharacterStates.MovementStates>>(this);
        /// 	}
        /// 
        /// 	private void OnDisable() // on disable we stop listening for these events
        /// 	{
        /// 		MMEventManager.RemoveListener<MMStateChangeEvent<CharacterStates.MovementStates>>(this);
        /// 	}
        /// }
        /// 이제 이 캐릭터의 이동 상태가 변경될 때마다 OnMMEvent 메서드가 호출되며 이를 통해 원하는 작업을 수행할 수 있습니다.
        ///
        /// 이 상태 머신이 이벤트를 브로드캐스트하는지 여부
        public bool TriggerEvents { get; set; }
		/// the name of the target gameobject
		public GameObject Target;
		/// the current character's movement state
		public T CurrentState { get; set; }
		/// the character's movement state before entering the current one
		public T PreviousState { get; protected set; }

		public delegate void OnStateChangeDelegate();
        ///해당 상태 머신의 변경 사항을 로컬에서 듣기 위해 들을 수 있는 이벤트
        /// 모든 수업에서 듣기 : 
        /// void OnEnable()
        /// {
        ///    yourReferenceToTheStateMachine.OnStateChange += OnStateChange;
        /// }
        /// void OnDisable()
        /// {
        ///    yourReferenceToTheStateMachine.OnStateChange -= OnStateChange;
        /// }
        /// void OnStateChange()
        /// {
        ///    // Do something
        /// }
        public OnStateChangeDelegate OnStateChange;

        /// <summary>
        /// targetName(이벤트에 사용되며 일반적으로 GetInstanceID() 사용)과 함께 이벤트를 사용할지 여부를 사용하여 새 StateMachine을 생성합니다.
        /// </summary>
        /// <param name="targetName">Target name.</param>
        /// <param name="triggerEvents">If set to <c>true</c> trigger events.</param>
        public MMStateMachine(GameObject target, bool triggerEvents)
		{
			this.Target = target;
			this.TriggerEvents = triggerEvents;
		} 

		/// <summary>
		/// Changes the current movement state to the one specified in the parameters, and triggers exit and enter events if needed
		/// </summary>
		/// <param name="newState">New state.</param>
		public virtual void ChangeState(T newState)
		{
			// if the "new state" is the current one, we do nothing and exit
			if (EqualityComparer<T>.Default.Equals(newState, CurrentState))
			{
				return;
			}

			// we store our previous character movement state
			PreviousState = CurrentState;
			CurrentState = newState;

			OnStateChange?.Invoke();

			if (TriggerEvents)
			{
				MMEventManager.TriggerEvent (new MMStateChangeEvent<T> (this));
			}
		}

		/// <summary>
		/// Returns the character to the state it was in before its current state
		/// </summary>
		public virtual void RestorePreviousState()
		{
			// we restore our previous state
			CurrentState = PreviousState;

			OnStateChange?.Invoke();

			if (TriggerEvents)
			{
				MMEventManager.TriggerEvent (new MMStateChangeEvent<T> (this));
			}
		}	
	}
}