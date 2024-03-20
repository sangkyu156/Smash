using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;
using UnityEngine.Events;
using MoreMountains.Feedbacks;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 스위치를 사용하면 현재 상태(켜짐 또는 꺼짐)에 따라 작업을 트리거할 수 있습니다. 문, 상자, 포털, 다리를 여는 데 유용합니다...
    /// </summary>
    [AddComponentMenu("TopDown Engine/Environment/Switch")]
	public class Switch : TopDownMonoBehaviour
	{
		[Header("Bindings")]
		/// a SpriteReplace to represent the switch knob when it's on
		[Tooltip("켜져 있을 때 스위치 손잡이를 나타내는 SpriteReplace")]
		public Animator SwitchAnimator;
     
		/// the possible states of the switch
		public enum SwitchStates { On, Off }
		/// the current state of the switch
		public SwitchStates CurrentSwitchState { get; set; }

		[Header("Switch")]

		/// the state the switch should start in
		[Tooltip("스위치가 시작되어야 하는 상태")]
		public SwitchStates InitialState = SwitchStates.Off;

		[Header("Events")]

		/// the methods to call when the switch is turned on
		[Tooltip("스위치가 켜졌을 때 호출할 메소드")]
		public UnityEvent SwitchOn;
		/// the methods to call when the switch is turned off
		[Tooltip("스위치가 꺼졌을 때 호출할 메서드")]
		public UnityEvent SwitchOff;
		/// the methods to call when the switch is toggled
		[Tooltip("스위치가 전환될 때 호출할 메서드")]
		public UnityEvent SwitchToggle;

		[Header("Feedbacks")]

		/// a feedback to play when the switch is toggled on
		[Tooltip("스위치가 켜졌을 때 재생할 피드백")]
		public MMFeedbacks SwitchOnFeedback;
		/// a feedback to play when the switch is turned off
		[Tooltip("스위치가 꺼졌을 때 재생할 피드백")]
		public MMFeedbacks SwitchOffFeedback;
		/// a feedback to play when the switch changes state
		[Tooltip("스위치 상태가 변경될 때 재생할 피드백")]
		public MMFeedbacks ToggleFeedback;

		[MMInspectorButton("TurnSwitchOn")]
		/// a test button to turn the switch on
		public bool SwitchOnButton;
		[MMInspectorButton("TurnSwitchOff")]
		/// a test button to turn the switch off
		public bool SwitchOffButton;
		[MMInspectorButton("ToggleSwitch")]
		/// a test button to change the switch's state
		public bool ToggleSwitchButton;

		/// <summary>
		/// On init, we set our current switch state
		/// </summary>
		protected virtual void Start()
		{
			CurrentSwitchState = InitialState;
			SwitchOffFeedback?.Initialization(this.gameObject);
			SwitchOnFeedback?.Initialization(this.gameObject);
			ToggleFeedback?.Initialization(this.gameObject);
		}

		/// <summary>
		/// Turns the switch on
		/// </summary>
		public virtual void TurnSwitchOn()
		{
			CurrentSwitchState = SwitchStates.On;
			if (SwitchOn != null) { SwitchOn.Invoke(); }
			if (SwitchToggle != null) { SwitchToggle.Invoke(); }
			SwitchOnFeedback?.PlayFeedbacks(this.transform.position);
		}

		/// <summary>
		/// Turns the switch off
		/// </summary>
		public virtual void TurnSwitchOff()
		{
			CurrentSwitchState = SwitchStates.Off;
			if (SwitchOff != null) { SwitchOff.Invoke(); }
			if (SwitchToggle != null) { SwitchToggle.Invoke(); }
			SwitchOffFeedback?.PlayFeedbacks(this.transform.position);
		}
        
		/// <summary>
		/// Use this method to go from one state to the other
		/// </summary>
		public virtual void ToggleSwitch()
		{
			if (CurrentSwitchState == SwitchStates.Off)
			{
				CurrentSwitchState = SwitchStates.On;
				if (SwitchOn != null) { SwitchOn.Invoke(); }
				if (SwitchToggle != null) { SwitchToggle.Invoke(); }
				SwitchOnFeedback?.PlayFeedbacks(this.transform.position);
			}
			else
			{
				CurrentSwitchState = SwitchStates.Off;
				if (SwitchOff != null) { SwitchOff.Invoke(); }
				if (SwitchToggle != null) { SwitchToggle.Invoke(); }
				SwitchOffFeedback?.PlayFeedbacks(this.transform.position);
			}
			ToggleFeedback?.PlayFeedbacks(this.transform.position);
		}

		/// <summary>
		/// On Update, we update our switch's animator
		/// </summary>
		protected virtual void Update()
		{
			if (SwitchAnimator != null)
			{
				SwitchAnimator.SetBool("SwitchOn", (CurrentSwitchState == SwitchStates.On));
			}            
		}
	}
}