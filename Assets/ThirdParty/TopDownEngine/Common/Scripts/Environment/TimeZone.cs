using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 클래스를 트리거에 추가하면 입력 시 지정된 기간 및 설정에 대해 시간 척도를 수정할 수 있습니다.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Environment/Time Zone")]
	public class TimeZone : ButtonActivated
	{
		/// the possible modes for this zone
		public enum Modes { DurationBased, ExitBased }

		[Header("Time Zone")]

		/// whether this zone will modify time on entry for a certain duration, or until it is exited
		[Tooltip("이 구역이 특정 기간 동안 입장 시간을 수정할지, 아니면 나갈 때까지 시간을 수정할지 여부")]
		public Modes Mode = Modes.DurationBased;

		/// the new timescale to apply
		[Tooltip("적용할 새로운 기간")]
		public float TimeScale = 0.5f;
		/// the duration to apply the new timescale for
		[Tooltip("새로운 기간을 적용할 기간")]
		public float Duration = 1f;
		/// whether or not the timescale should be lerped
		[Tooltip("시간 척도를 위반해야 하는지 여부")]
		public bool LerpTimeScale = true;
		/// the speed at which to lerp the timescale
		[Tooltip("시간 척도를 조정하는 속도")]
		public float LerpSpeed = 5f;

		/// <summary>
		/// When the button is pressed we start modifying the timescale
		/// </summary>
		public override void TriggerButtonAction()
		{
			if (!CheckNumberOfUses())
			{
				return;
			}
			base.TriggerButtonAction();
			ControlTime();
		}

		/// <summary>
		/// When exiting, and if needed, we reset the time scale
		/// </summary>
		/// <param name="collider"></param>
		public override void TriggerExitAction(GameObject collider)
		{
			if (Mode == Modes.ExitBased)
			{
				if (!CheckConditions(collider))
				{
					return;
				}

				if (!TestForLastObject(collider))
				{
					return;
				}

				MMTimeScaleEvent.Trigger(MMTimeScaleMethods.Unfreeze, 1f, 0f, false, 0f, false);
			}
		}

		/// <summary>
		/// Modifies the timescale
		/// </summary>
		public virtual void ControlTime()
		{
			if (Mode == Modes.ExitBased)
			{
				MMTimeScaleEvent.Trigger(MMTimeScaleMethods.For, TimeScale, Duration, LerpTimeScale, LerpSpeed, true);
			}
			else
			{
				MMTimeScaleEvent.Trigger(MMTimeScaleMethods.For, TimeScale, Duration, LerpTimeScale, LerpSpeed, false);
			}
		}
	}
}