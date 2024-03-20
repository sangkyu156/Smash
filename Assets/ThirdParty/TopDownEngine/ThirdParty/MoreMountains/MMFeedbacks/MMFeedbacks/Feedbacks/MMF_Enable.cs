using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 피드백의 다양한 단계에서 객체를 활성화 또는 비활성화합니다.
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("이 피드백을 사용하면 대상 게임 개체의 동작 상태를 활성에서 비활성(또는 그 반대로), 초기화, 재생, 중지 또는 재설정 시 변경할 수 있습니다. " +
"각각에 대해 상태를 강제로 적용할지(활성화 또는 비활성화) 또는 토글할지(활성화는 비활성화, 비활성화는 활성화) 지정할 수 있습니다.")]
	[FeedbackPath("GameObject/Enable Behaviour")]
	public class MMF_Enable : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.GameObjectColor; } }
		public override bool EvaluateRequiresSetup() { return (TargetBehaviour == null); }
		public override string RequiredTargetText { get { return TargetBehaviour != null ? TargetBehaviour.name : "";  } }
		public override string RequiresSetupText { get { return "This feedback requires that a TargetBehaviour be set to be able to work properly. You can set one below."; } }
		#endif

		/// the possible effects the feedback can have on the target object's status 
		public enum PossibleStates { Enabled, Disabled, Toggle }

		[MMFInspectorGroup("Enable Target Monobehaviour", true, 86, true)]
		/// the gameobject we want to change the active state of
		[Tooltip("활성 상태를 변경하려는 게임 개체")]
		public Behaviour TargetBehaviour;
		/// a list of extra gameobjects we want to change the active state of
		[Tooltip("활성 상태를 변경하려는 추가 게임 개체 목록")]
		public List<Behaviour> ExtraTargetBehaviours;
		/// whether or not we should alter the state of the target object on init
		[Tooltip("초기화 시 대상 객체의 상태를 변경해야 하는지 여부")]
		public bool SetStateOnInit = false;
		/// how to change the state on init
		[MMFCondition("SetStateOnInit", true)]
		[Tooltip("how to change the state on init")]
		public PossibleStates StateOnInit = PossibleStates.Disabled;
		/// whether or not we should alter the state of the target object on play
		[Tooltip("whether or not we should alter the state of the target object on play")]
		public bool SetStateOnPlay = false;
		/// how to change the state on play
		[MMFCondition("SetStateOnPlay", true)]
		[Tooltip("how to change the state on play")]
		public PossibleStates StateOnPlay = PossibleStates.Disabled;
		/// whether or not we should alter the state of the target object on stop
		[Tooltip("whether or not we should alter the state of the target object on stop")]
		public bool SetStateOnStop = false;
		/// how to change the state on stop
		[Tooltip("how to change the state on stop")]
		[MMFCondition("SetStateOnStop", true)]
		public PossibleStates StateOnStop = PossibleStates.Disabled;
		/// whether or not we should alter the state of the target object on reset
		[Tooltip("whether or not we should alter the state of the target object on reset")]
		public bool SetStateOnReset = false;
		/// how to change the state on reset
		[Tooltip("재설정 시 상태를 변경하는 방법")]
		[MMFCondition("SetStateOnReset", true)]
		public PossibleStates StateOnReset = PossibleStates.Disabled;

		protected bool _initialState;
		
		/// <summary>
		/// On init we change the state of our Behaviour if needed
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);
			if (Active && (TargetBehaviour != null))
			{
				if (SetStateOnInit)
				{
					SetStatus(StateOnInit);
				}
			}
		}

		/// <summary>
		/// On Play we change the state of our Behaviour if needed
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || (TargetBehaviour == null))
			{
				return;
			}
			if (SetStateOnPlay)
			{
				SetStatus(StateOnPlay);
			}
		}

		/// <summary>
		/// On Stop we change the state of our Behaviour if needed
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
		{
			if (!Active || !FeedbackTypeAuthorized || (TargetBehaviour == null))
			{
				return;
			}
			base.CustomStopFeedback(position, feedbacksIntensity);

			if (SetStateOnStop)
			{
				SetStatus(StateOnStop);
			}
		}

		/// <summary>
		/// On Reset we change the state of our Behaviour if needed
		/// </summary>
		protected override void CustomReset()
		{
			base.CustomReset();

			if (InCooldown)
			{
				return;
			}
            
			if (!Active || !FeedbackTypeAuthorized || (TargetBehaviour == null))
			{
				return;
			}
            
			if (SetStateOnReset)
			{
				SetStatus(StateOnReset);
			}
		}

		/// <summary>
		/// Changes the status of the Behaviour
		/// </summary>
		/// <param name="state"></param>
		protected virtual void SetStatus(PossibleStates state)
		{
			SetStatus(state, TargetBehaviour);
			foreach (Behaviour extra in ExtraTargetBehaviours)
			{
				SetStatus(state, extra);
			}
		}

		/// <summary>
		/// Sets the specified status on the target Behaviour
		/// </summary>
		/// <param name="state"></param>
		/// <param name="target"></param>
		protected virtual void SetStatus(PossibleStates state, Behaviour target)
		{
			_initialState = target.enabled;
			switch (state)
			{
				case PossibleStates.Enabled:
					target.enabled = NormalPlayDirection ? true : false;
					break;
				case PossibleStates.Disabled:
					target.enabled = NormalPlayDirection ? false : true;
					break;
				case PossibleStates.Toggle:
					target.enabled = !target.enabled;
					break;
			}
		}
		
		/// <summary>
		/// On restore, we put our object back at its initial position
		/// </summary>
		protected override void CustomRestoreInitialValues()
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			
			TargetBehaviour.enabled = _initialState;
			foreach (Behaviour extra in ExtraTargetBehaviours)
			{
				extra.enabled = _initialState;
			}
		}
	}
}