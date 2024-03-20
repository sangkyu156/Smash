using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 피드백의 다양한 단계에서 객체를 활성화 또는 비활성화합니다.
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("이 피드백을 사용하면 초기화, 재생, 중지 또는 재설정 시 대상 게임 개체의 상태를 활성에서 비활성(또는 그 반대로)으로 변경할 수 있습니다. 이들 각각에 대해 상태를 강제로 적용할지(활성 또는 비활성) 또는 토글할지(활성이 비활성화되고 비활성이 활성이 됨)를 지정할 수 있습니다.")]
	[FeedbackPath("GameObject/Set Active")]
	public class MMFeedbackSetActive : MMFeedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.GameObjectColor; } }
		#endif

		/// the possible effects the feedback can have on the target object's status 
		public enum PossibleStates { Active, Inactive, Toggle }
        
		[Header("Set Active")]
		/// the gameobject we want to change the active state of
		[Tooltip("활성 상태를 변경하려는 게임 개체")]
		public GameObject TargetGameObject;
        
		[Header("States")]
		/// whether or not we should alter the state of the target object on init
		[Tooltip("초기화 시 대상 객체의 상태를 변경해야 하는지 여부")]
		public bool SetStateOnInit = false;
		[MMFCondition("SetStateOnInit", true)]
		/// how to change the state on init
		[Tooltip("how to change the state on init")]
		public PossibleStates StateOnInit = PossibleStates.Inactive;
		/// whether or not we should alter the state of the target object on play
		[Tooltip("플레이 중인 대상 객체의 상태를 변경해야 하는지 여부")]
		public bool SetStateOnPlay = false;
		/// how to change the state on play
		[Tooltip("플레이 중 상태를 변경하는 방법")]
		[MMFCondition("SetStateOnPlay", true)]
		public PossibleStates StateOnPlay = PossibleStates.Inactive;
		/// whether or not we should alter the state of the target object on stop
		[Tooltip("정지 시 대상 객체의 상태를 변경해야 하는지 여부")]
		public bool SetStateOnStop = false;
		/// how to change the state on stop
		[Tooltip("중지 시 상태를 변경하는 방법")]
		[MMFCondition("SetStateOnStop", true)]
		public PossibleStates StateOnStop = PossibleStates.Inactive;
		/// whether or not we should alter the state of the target object on reset
		[Tooltip("재설정 시 대상 객체의 상태를 변경해야 하는지 여부")]
		public bool SetStateOnReset = false;
		/// how to change the state on reset
		[Tooltip("재설정 시 상태를 변경하는 방법")]
		[MMFCondition("SetStateOnReset", true)]
		public PossibleStates StateOnReset = PossibleStates.Inactive;
        
		/// <summary>
		/// On init we change the state of our object if needed
		/// </summary>
		/// <param name="owner"></param>
		protected override void CustomInitialization(GameObject owner)
		{
			base.CustomInitialization(owner);
			if (Active && (TargetGameObject != null))
			{
				if (SetStateOnInit)
				{
					SetStatus(StateOnInit);
				}
			}
		}

		/// <summary>
		/// On Play we change the state of our object if needed
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || (TargetGameObject == null))
			{
				return;
			}
            
			if (SetStateOnPlay)
			{
				SetStatus(StateOnPlay);
			}
		}

		/// <summary>
		/// On Stop we change the state of our object if needed
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
		{
			base.CustomStopFeedback(position, feedbacksIntensity);

			if (Active && FeedbackTypeAuthorized && (TargetGameObject != null))
			{
				if (SetStateOnStop)
				{
					SetStatus(StateOnStop);
				}
			}
		}

		/// <summary>
		/// On Reset we change the state of our object if needed
		/// </summary>
		protected override void CustomReset()
		{
			base.CustomReset();

			if (InCooldown)
			{
				return;
			}

			if (Active && FeedbackTypeAuthorized && (TargetGameObject != null))
			{
				if (SetStateOnReset)
				{
					SetStatus(StateOnReset);
				}
			}
		}

		/// <summary>
		/// Changes the status of the object
		/// </summary>
		/// <param name="state"></param>
		protected virtual void SetStatus(PossibleStates state)
		{
			bool newState = false;
			switch (state)
			{
				case PossibleStates.Active:
					newState = NormalPlayDirection ? true : false;
					break;
				case PossibleStates.Inactive:
					newState = NormalPlayDirection ? false : true;
					break;
				case PossibleStates.Toggle:
					newState = !TargetGameObject.activeInHierarchy;
					break;
			}
			TargetGameObject.SetActive(newState);
		}
	}
}