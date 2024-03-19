using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 기능을 캐릭터에 추가하면 TimeControl 버튼을 누를 때 시간을 제어할 수 있습니다.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/Abilities/Character Time Control")]
	public class CharacterTimeControl : CharacterAbility
	{
		public enum Modes { OneTime, Continuous }
        
		/// the chosen mode for this ability : one time will stop time for the specified duration on button press, even if you release it, while continuous will stop time while the button is pressed, until cooldown consumption duration expiration
		[Tooltip("이 능력에 대해 선택한 모드: 버튼을 눌렀을 때 버튼을 눌렀을 때 지정된 지속 시간 동안 1회 시간이 정지되고, 버튼을 눌렀을 때 재사용 대기시간이 만료될 때까지 지속 시간이 멈춥니다.")]
		public Modes Mode = Modes.Continuous;
		/// the time scale to switch to when the time control button gets pressed
		[Tooltip("시간 제어 버튼을 눌렀을 때 전환할 시간 척도")]
		public float TimeScale = 0.5f;
		/// the duration for which to keep the timescale changed
		[Tooltip("변경된 기간을 유지하는 기간")]
		[MMEnumCondition("Mode", (int)Modes.OneTime)]
		public float OneTimeDuration = 1f;
		/// whether or not the timescale should get lerped
		[Tooltip("시간 척도가 잘못되어야 하는지 여부")]
		public bool LerpTimeScale = true;
		/// the speed at which to lerp the timescale
		[Tooltip("시간 척도를 조정하는 속도")]
		public float LerpSpeed = 5f;
		/// the cooldown for this ability
		[Tooltip("이 능력의 재사용 대기시간")]
		public MMCooldown Cooldown;

		protected bool _timeControlled = false;

		/// <summary>
		/// Watches for input press
		/// </summary>
		protected override void HandleInput()
		{
			base.HandleInput();
			if (!AbilityAuthorized)
			{
				return;
			}
			if (_inputManager.TimeControlButton.State.CurrentState == MMInput.ButtonStates.ButtonDown)
			{
				TimeControlStart();
			}
			if (_inputManager.TimeControlButton.State.CurrentState == MMInput.ButtonStates.ButtonUp)
			{
				TimeControlStop();
			}
		}

		/// <summary>
		/// On initialization, we init our cooldown
		/// </summary>
		protected override void Initialization()
		{
			base.Initialization();
			Cooldown.Initialization();
		}

		/// <summary>
		/// Starts the time scale modification
		/// </summary>
		public virtual void TimeControlStart()
		{
			if (Cooldown.Ready())
			{
				PlayAbilityStartFeedbacks();
				if (Mode == Modes.Continuous)
				{
					MMTimeScaleEvent.Trigger(MMTimeScaleMethods.For, TimeScale, Cooldown.ConsumptionDuration, LerpTimeScale, LerpSpeed, true);
					Cooldown.Start();
					_timeControlled = true;    
				}
				else
				{
					MMTimeScaleEvent.Trigger(MMTimeScaleMethods.For, TimeScale, OneTimeDuration, LerpTimeScale, LerpSpeed, false);
					Cooldown.Start();
				}
			}            
		}

		/// <summary>
		/// Stops the time control
		/// </summary>
		public virtual void TimeControlStop()
		{
			Cooldown.Stop();
		}

		/// <summary>
		/// On update, we unfreeze time if needed
		/// </summary>
		public override void ProcessAbility()
		{
			base.ProcessAbility();
			Cooldown.Update();

			if ((Cooldown.CooldownState != MMCooldown.CooldownStates.Consuming) && _timeControlled)
			{
				if (Mode == Modes.Continuous)
				{
					_timeControlled = false;
					MMTimeScaleEvent.Trigger(MMTimeScaleMethods.Unfreeze, 1f, 0f, false, 0f, false);    
				}
			}
		}

		protected virtual void OnCooldownStateChange(MMCooldown.CooldownStates newState)
		{
			if (newState == MMCooldown.CooldownStates.Stopped)
			{
				StopStartFeedbacks();
				PlayAbilityStopFeedbacks();
			}
		}
		
		protected override void OnEnable()
		{
			base.OnEnable();
			Cooldown.OnStateChange += OnCooldownStateChange;
		}
		
		protected override void OnDisable()
		{
			base.OnDisable();
			Cooldown.OnStateChange -= OnCooldownStateChange;
		}
	}
}