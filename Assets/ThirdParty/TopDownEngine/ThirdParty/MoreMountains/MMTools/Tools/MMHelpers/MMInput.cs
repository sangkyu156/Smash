using UnityEngine;
using System.Collections;

namespace MoreMountains.Tools
{	
	/// <summary>
	/// Input helpers
	/// </summary>

	public class MMInput : MonoBehaviour 
	{
		/// <summary>
		/// All possible states for a button. Can be used in a state machine.
		/// </summary>
		public enum ButtonStates { Off, ButtonDown, ButtonPressed, ButtonUp }

		public enum AxisTypes { Positive, Negative }

        /// <summary>
        /// 축을 가져와서 축을 눌렀는지 여부(예를 들어 xbox 트리거에 유용함)와 축/트리거를 바이너리로 사용해야 하는 경우에 따라 ButtonState를 반환합니다.
        /// </summary>
        /// <returns>The axis as button.</returns>
        /// <param name="axisName">Axis name.</param>
        /// <param name="threshold">Threshold value below which the button is off or released.</param>
        /// <param name="currentState">Current state of the axis.</param>
        public static ButtonStates ProcessAxisAsButton (string axisName, float threshold, ButtonStates currentState, AxisTypes AxisType = AxisTypes.Positive)
		{
			float axisValue = Input.GetAxis (axisName);
			ButtonStates returnState;

			bool comparison = (AxisType == AxisTypes.Positive) ? (axisValue < threshold) : (axisValue > threshold);
			
			if (comparison)
			{
				if (currentState == ButtonStates.ButtonPressed)
				{
					returnState = ButtonStates.ButtonUp;
				}
				else
				{
					returnState = ButtonStates.Off;
				}
			}
			else
			{
				if (currentState == ButtonStates.Off)
				{
					returnState = ButtonStates.ButtonDown;
				}
				else
				{
					returnState = ButtonStates.ButtonPressed;
				}
			}
			return returnState;
		}

        /// <summary>
        /// IM 버튼(InputManager 버튼의 약자), 모바일 키이든 실제 키이든 버튼 상태를 처리하는 데 사용되는 클래스
        /// </summary>
        public class IMButton
		{
            /// 버튼 상태를 저장하는 데 사용되는 상태 머신
            public MMStateMachine<MMInput.ButtonStates> State {get;protected set;}
			/// the unique ID of this button
			public string ButtonID;

			public delegate void ButtonDownMethodDelegate();
			public delegate void ButtonPressedMethodDelegate();
			public delegate void ButtonUpMethodDelegate();

			public ButtonDownMethodDelegate ButtonDownMethod;
			public ButtonPressedMethodDelegate ButtonPressedMethod;
			public ButtonUpMethodDelegate ButtonUpMethod;

			/// returns the time (in unscaled seconds) since the last time the button was pressed down
			public float TimeSinceLastButtonDown { get { return Time.unscaledTime - _lastButtonDownAt; } }
			/// returns the time (in unscaled seconds) since the last time the button was released
			public float TimeSinceLastButtonUp { get { return Time.unscaledTime - _lastButtonUpAt; } }
			/// returns true if this button was pressed down within the time (in unscaled seconds) passed in parameters
			public bool ButtonDownRecently(float time) { return (TimeSinceLastButtonDown <= time); }
			/// returns true if this button was released within the time (in unscaled seconds) passed in parameters
			public bool ButtonUpRecently(float time) { return (TimeSinceLastButtonUp <= time); }

			protected float _lastButtonDownAt;
			protected float _lastButtonUpAt;

			public IMButton(string playerID, string buttonID, ButtonDownMethodDelegate btnDown = null, ButtonPressedMethodDelegate btnPressed = null, ButtonUpMethodDelegate btnUp = null) 
			{
				ButtonID = playerID + "_" + buttonID;
				ButtonDownMethod = btnDown;
				ButtonUpMethod = btnUp;
				ButtonPressedMethod = btnPressed;
				State = new MMStateMachine<MMInput.ButtonStates> (null, false);
				State.ChangeState (MMInput.ButtonStates.Off);
			}

			public virtual void TriggerButtonDown()
			{
				_lastButtonDownAt = Time.unscaledTime;
				if (ButtonDownMethod == null)
				{
					State.ChangeState(MMInput.ButtonStates.ButtonDown);
				}
				else
				{
					ButtonDownMethod();
				}
			}

			public virtual void TriggerButtonPressed()
			{
				if (ButtonPressedMethod == null)
				{
					State.ChangeState(MMInput.ButtonStates.ButtonPressed);
				}
				else
				{
					ButtonPressedMethod();
				}
			}

			public virtual void TriggerButtonUp()
			{
				_lastButtonUpAt = Time.unscaledTime;
				if (ButtonUpMethod == null)
				{
					State.ChangeState(MMInput.ButtonStates.ButtonUp);
				}
				else
				{
					ButtonUpMethod();
				}
			}
		}
	}


}