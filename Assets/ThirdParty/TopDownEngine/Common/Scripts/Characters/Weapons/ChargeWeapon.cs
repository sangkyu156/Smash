using System;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 돌격 무기를 구성하는 무기의 돌격 속성을 저장하는 데 사용되는 클래스 각 돌격 무기는 이들 중 여러 개로 구성되며 각각 돌격 시퀀스의 단계를 나타냅니다.
    /// </summary>
    [Serializable]
	public class ChargeWeaponStep
	{
		/// the weapon to cause an attack with at that step
		[Tooltip("그 단계에서 공격을 일으키는 무기")]
		public Weapon TargetWeapon;
		/// the duration (in seconds) it should take to keep the charge going to the next step
		[Tooltip("다음 단계로 요금이 계속 진행되는 데 걸리는 시간(초)")]
		public float ChargeDuration = 1f;
		/// if the charge is interrupted at this step, whether or not to trigger this weapon's attack
		[Tooltip("이 단계에서 충전이 중단되면 이 무기의 공격을 발동할지 여부")]
		public bool TriggerIfChargeInterrupted = true;
		/// if this is true, the weapon at this step will be flipped when the charge weapon flips 
		[Tooltip("이것이 사실이라면 이 단계의 무기는 충전 무기가 뒤집힐 때 뒤집힐 것입니다.")]
		public bool FlipWhenChargeWeaponFlips = true;
		/// a feedback to trigger when this step starts charging
		[Tooltip("이 단계에서 충전이 시작되면 피드백 트리거로 트리거됩니다.")]
		public MMFeedbacks ChargeStartFeedbacks;
		/// a feedback to trigger when this step gets interrupted (when the charge is dropped at this step)
		[Tooltip("이 단계가 중단될 때 트리거할 피드백(이 단계에서 요금이 삭제될 때)")]
		public MMFeedbacks ChargeInterruptedFeedbacks;
		/// a feedback to trigger when this step completes and the charge potentially moves on to the next step
		[Tooltip("이 단계가 완료되고 요금이 다음 단계로 넘어갈 때 트리거되는 피드백")]
		public MMFeedbacks ChargeCompleteFeedbacks;
		/// the total time (in seconds) from the complete start of the charge weapon to this weapon's charge being complete
		public float ChargeTotalDuration { get; set; }
		/// whether this step's charge has started or not
		public bool ChargeStarted { get; set; }
		/// whether this step's charge has completed or not
		public bool ChargeComplete { get; set; }
	}

    /// <summary>
    /// 이 구성 요소를 개체에 추가하면 각각 고유한 무기를 트리거하는 일련의 충전 단계를 정의할 수 있으며, 입력 모드 또는 조건부 릴리스, 모든 단계에 대한 후크 등과 같은 옵션이 완비됩니다.
	/// Megaman이나 Zelda와 같은 충전 무기에 유용합니다.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Weapons/Charge Weapon")]
	public class ChargeWeapon : Weapon
	{
		/// the possible timescales for this weapon
		public enum TimescaleModes { Scaled, Unscaled }
		/// whether the charge should be released on input release, or after the last charge duration
		public enum ReleaseModes { OnInputRelease, AfterLastChargeDuration }
		/// the current delta time value
		public virtual float DeltaTime => TimescaleMode == TimescaleModes.Scaled ? Time.deltaTime : Time.unscaledDeltaTime;
		/// the current time value
		public virtual float CurrentTime => TimescaleMode == TimescaleModes.Scaled ? Time.time : Time.unscaledTime;

		[MMInspectorGroup("Charge Weapon", true, 22)]
		[Header("List of Weapons in the Charge Sequence")]
		/// the list of weapons that make up this charge weapon's sequence of steps
		[Tooltip("이 돌격 무기의 일련의 단계를 구성하는 무기 목록")]
		public List<ChargeWeaponStep> Weapons;
		
		[Header("Settings")]
		/// whether this weapon should trigger its attack when all steps are done charging, or when input gets released
		[Tooltip("모든 단계의 충전이 완료되었을 때 또는 입력이 해제되었을 때 이 무기가 공격을 트리거해야 하는지 여부")]
		public ReleaseModes ReleaseMode = ReleaseModes.OnInputRelease;
		/// whether this weapon's input should run on scaled or unscaled time
		[Tooltip("이 무기의 입력이 조정된 시간에 실행되어야 하는지, 조정되지 않은 시간에 실행되어야 하는지 여부")]
		public TimescaleModes TimescaleMode = TimescaleModes.Scaled;
		/// whether or not the start of the charge should trigger the first step's weapon's attack or not
		[Tooltip("돌격 시작이 첫 번째 단계의 무기 공격을 트리거할지 여부")]
		public bool AllowInitialShot = true;
		
		[Header("Debug")]
		/// the current charge index in the Weapons step list
		[Tooltip("무기 단계 목록의 현재 충전 지수")]
		[MMReadOnly]
		public int CurrentChargeIndex = 0;
		/// whether this weapon is currently charging or not
		[Tooltip("이 무기가 현재 충전 중인지 여부")]
		[MMReadOnly] 
		public bool Charging = false;

		protected float _chargingStartedAt = 0f;
		protected int _chargeIndexLastFrame;
		protected int _initialWeaponIndex = 0;
		
		/// <summary>
		/// On init, we initialize our durations, weapons and reset the charge
		/// </summary>
		public override void Initialization()
		{
			base.Initialization();
			InitializeTotalDurations();
			InitializeWeapons();
			ResetCharge();
		}

		/// <summary>
		/// goes through all weapons to setup their total duration (the time from start after which their step is complete)
		/// </summary>
		public virtual void InitializeTotalDurations()
		{
			float total = 0f;
			if (DelayBeforeUse > 0)
			{
				total += DelayBeforeUse;
				CurrentChargeIndex = -1;
			}
			foreach (ChargeWeaponStep item in Weapons)
			{
				total += item.ChargeDuration;
				item.ChargeTotalDuration = total;
			}
			_chargeIndexLastFrame = CurrentChargeIndex;
			_initialWeaponIndex = CurrentChargeIndex;
		}

		/// <summary>
		/// resets the charge, reinitializing all counters 
		/// </summary>
		public virtual void ResetCharge()
		{
			Charging = false;
			CurrentChargeIndex = _initialWeaponIndex;
			foreach (ChargeWeaponStep item in Weapons)
			{
				item.ChargeStarted = false;
				item.ChargeComplete = false;
			}
		}

		/// <summary>
		/// Initializes all weapons for all steps
		/// </summary>
		protected virtual void InitializeWeapons()
		{
			foreach (ChargeWeaponStep item in Weapons)
			{
				item.TargetWeapon.SetOwner(Owner, CharacterHandleWeapon);
				item.TargetWeapon.Initialization();
				item.TargetWeapon.InitializeAnimatorParameters();
			}
		}
		
		/// <summary>
		/// On update, if we're charging, we process our charge to evaluate the current step
		/// </summary>
		protected override void Update()
		{
			base.Update();
			ProcessCharge();
		}

		/// <summary>
		/// Determines the current step, and if it's different from the last frame, starts the new step
		/// </summary>
		protected virtual void ProcessCharge()
		{
			if (!Charging)
			{
				return;
			}
			
			CurrentChargeIndex = FindCurrentWeaponIndex();

			if (CurrentChargeIndex != _chargeIndexLastFrame)
			{
				CompleteStepCharge(_chargeIndexLastFrame);
				StartStepCharge(CurrentChargeIndex);
			}

			if ((ReleaseMode == ReleaseModes.AfterLastChargeDuration) && (CurrentChargeIndex == Weapons.Count - 1))
			{
				StopChargeSequence();
			}
			
			_chargeIndexLastFrame = CurrentChargeIndex;
		}

		/// <summary>
		/// Initializes the charge sequence
		/// </summary>
		protected virtual void StartChargeSequence()
		{
			Charging = true;
			_chargingStartedAt = CurrentTime;
			if (WeaponExists(CurrentChargeIndex))
			{
				StartStepCharge(CurrentChargeIndex);
				if (AllowInitialShot)
				{
					ForceWeaponAttack(0);
				}
			}
		}

		/// <summary>
		/// Causes a step to start charging
		/// </summary>
		/// <param name="index"></param>
		protected virtual void StartStepCharge(int index)
		{
			if (!WeaponExists(index))
			{
				return;
			}
			
			Weapons[index].ChargeStarted = true;
			Weapons[index].ChargeStartFeedbacks?.PlayFeedbacks();
		}

		/// <summary>
		/// Stops a step charge
		/// </summary>
		/// <param name="index"></param>
		protected virtual void InterruptStepCharge(int index)
		{
			if (!WeaponExists(index))
			{
				return;
			}
			Weapons[index].ChargeStartFeedbacks?.StopFeedbacks();
			Weapons[index].ChargeInterruptedFeedbacks?.PlayFeedbacks();
		}

		/// <summary>
		/// Completes a step charge
		/// </summary>
		/// <param name="index"></param>
		protected virtual void CompleteStepCharge(int index)
		{
			if (!WeaponExists(index))
			{
				return;
			}
			
			Weapons[index].ChargeStartFeedbacks?.StopFeedbacks();
			Weapons[index].ChargeComplete = true;
			Weapons[index].ChargeCompleteFeedbacks?.PlayFeedbacks();
		}
		
		/// <summary>
		/// Stops the entire charge sequence, triggering the appropriate feedbacks
		/// </summary>
		protected virtual void StopChargeSequence()
		{
			if (!Charging)
			{
				return;
			}
			
			if ((CurrentChargeIndex >= 0) || !AllowInitialShot)
			{
				bool shouldAttack = true;
				if (CurrentChargeIndex < Weapons.Count - 1 && !Weapons[CurrentChargeIndex].ChargeComplete)
				{
					if (!Weapons[CurrentChargeIndex].TriggerIfChargeInterrupted)
					{
						shouldAttack = false;
					}
				}

				if (shouldAttack)
				{
					Weapons[CurrentChargeIndex].ChargeStartFeedbacks?.StopFeedbacks();
					Weapons[CurrentChargeIndex].ChargeCompleteFeedbacks?.StopFeedbacks();
					if (WeaponExists(CurrentChargeIndex - 1))
					{
						Weapons[CurrentChargeIndex - 1].ChargeStartFeedbacks?.StopFeedbacks();
						Weapons[CurrentChargeIndex - 1].ChargeCompleteFeedbacks?.StopFeedbacks();	
					}
					ForceWeaponAttack(CurrentChargeIndex);	
				}
			}

			if (!Weapons[CurrentChargeIndex].ChargeComplete)
			{
				InterruptStepCharge(CurrentChargeIndex);
			}

			ResetCharge();
		}

		/// <summary>
		/// Forces the weapon at the specified step to turn on
		/// </summary>
		/// <param name="index"></param>
		protected virtual void ForceWeaponAttack(int index)
		{
			Weapons[index].TargetWeapon.TurnWeaponOn();
		}

		/// <summary>
		/// Returns the index of the current weapon in the charge sequence
		/// </summary>
		/// <returns></returns>
		protected virtual int FindCurrentWeaponIndex()
		{
			float elapsedTime = CurrentTime - _chargingStartedAt;

			if (elapsedTime < DelayBeforeUse)
			{
				return -1;
			}
			
			for (int i = 0; i < Weapons.Count; i++)
			{
				if (Weapons[i].ChargeTotalDuration > elapsedTime)
				{
					return i;
				}
			}
			return Weapons.Count - 1;
		}
		
		/// <summary>
		/// Returns true if the weapon at the specified index exists
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		protected virtual bool WeaponExists(int index)
		{
			return (index >= 0) && (index < Weapons.Count);
		}
		
		/// <summary>
		/// When the charge weapon gets activated, we start charging
		/// </summary>
		public override void TurnWeaponOn()
		{
			base.TurnWeaponOn();
			StartChargeSequence();
		}

		/// <summary>
		/// When the charge weapon's input gets released, we stop charging
		/// </summary>
		public override void WeaponInputReleased()
		{
			base.WeaponInputReleased();
			StopChargeSequence();
		}

		public override void FlipWeapon()
		{
			base.FlipWeapon();
			for (int i = 0; i < Weapons.Count; i++)
			{
				if (Weapons[i].FlipWhenChargeWeaponFlips)
				{
					Weapons[i].TargetWeapon.Flipped = Flipped;
				}
			}
		}
	}
}
