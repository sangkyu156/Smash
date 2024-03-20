using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 표적 획득 설정을 수집하는 클래스
    /// </summary>
    [System.Serializable]
	public class MMFeedbackTargetAcquisition
	{
		public enum Modes { None, Self, AnyChild, ChildAtIndex, Parent, FirstReferenceHolder, PreviousReferenceHolder, ClosestReferenceHolder, NextReferenceHolder, LastReferenceHolder }

        /// 표적 획득을 위해 선택된 모드
        /// None : 아무 일도 일어나지 않습니다.
        /// Self : MMF 플레이어의 게임 개체에서 대상이 선택됩니다.
        /// AnyChild : 대상은 MMF 플레이어의 하위 개체 중 하나에서 선택됩니다.
        /// ChildAtIndex : 대상은 MMF 플레이어의 인덱스 X에 있는 자식에서 선택됩니다.
        /// Parent : 일치하는 대상이 발견된 첫 번째 부모에서 대상이 선택됩니다.
        /// Various reference holders : the target will be picked on the specified reference holder in the list (either the first one, previous : first one found before this feedback in the list, closest in any direction from this feedback, the next one found, or the last one in the list)   
        [Tooltip("the selected mode for target acquisition\n"+
            "None : 아무 일도 일어나지 않습니다.\n" +
            "Self : MMF 플레이어의 게임 개체에서 대상이 선택됩니다.\n" +
            "AnyChild : 대상은 MMF 플레이어의 하위 개체 중 하나에서 선택됩니다.\n" +
            "ChildAtIndex : 대상은 MMF 플레이어의 인덱스 X에 있는 자식에서 선택됩니다.\n" +
            "Parent : 일치하는 대상이 발견된 첫 번째 부모에서 대상이 선택됩니다.\n" +
            "Various reference holders : 대상은 목록의 지정된 참조 홀더에서 선택됩니다. " +
            "(either the first one, previous : 목록에서 이 피드백 이전에 발견된 첫 번째 것, 이 피드백에서 어떤 방향으로든 가장 가까운 것, 발견된 다음 것 또는 목록의 마지막 것)")]
		public Modes Mode = Modes.None;

		[MMFEnumCondition("Mode", (int)Modes.ChildAtIndex)]
		public int ChildIndex = 0;

		private static MMF_ReferenceHolder _referenceHolder;

		public static MMF_ReferenceHolder GetReferenceHolder(MMFeedbackTargetAcquisition settings, MMF_Player owner, int currentFeedbackIndex)
		{
			switch (settings.Mode)
			{
				case Modes.FirstReferenceHolder:
					return owner.GetFeedbackOfType<MMF_ReferenceHolder>(MMF_Player.AccessMethods.First, currentFeedbackIndex);
				case Modes.PreviousReferenceHolder:
					return owner.GetFeedbackOfType<MMF_ReferenceHolder>(MMF_Player.AccessMethods.Previous, currentFeedbackIndex);
				case Modes.ClosestReferenceHolder:
					return owner.GetFeedbackOfType<MMF_ReferenceHolder>(MMF_Player.AccessMethods.Closest, currentFeedbackIndex);
				case Modes.NextReferenceHolder:
					return owner.GetFeedbackOfType<MMF_ReferenceHolder>(MMF_Player.AccessMethods.Next, currentFeedbackIndex);
				case Modes.LastReferenceHolder:
					return owner.GetFeedbackOfType<MMF_ReferenceHolder>(MMF_Player.AccessMethods.Last, currentFeedbackIndex);
			}
			return null;
		}

		public static GameObject FindAutomatedTargetGameObject(MMFeedbackTargetAcquisition settings, MMF_Player owner, int currentFeedbackIndex)
		{
			if (owner.FeedbacksList[currentFeedbackIndex].ForcedReferenceHolder != null)
			{
				return owner.FeedbacksList[currentFeedbackIndex].ForcedReferenceHolder.GameObjectReference;
			}
			
			_referenceHolder = GetReferenceHolder(settings, owner, currentFeedbackIndex);
			switch (settings.Mode)
			{
				case Modes.Self:
					return owner.gameObject;
				case Modes.ChildAtIndex:
					return owner.transform.GetChild(settings.ChildIndex).gameObject;
				case Modes.AnyChild:
					return owner.transform.GetChild(0).gameObject;
				case Modes.Parent:
					return owner.transform.parent.gameObject;
				case Modes.FirstReferenceHolder: 
				case Modes.PreviousReferenceHolder:
				case Modes.ClosestReferenceHolder:
				case Modes.NextReferenceHolder:
				case Modes.LastReferenceHolder:
					return _referenceHolder?.GameObjectReference;
			}
			return null;
		}

		public static T FindAutomatedTarget<T>(MMFeedbackTargetAcquisition settings, MMF_Player owner, int currentFeedbackIndex)
		{
			if (owner.FeedbacksList[currentFeedbackIndex].ForcedReferenceHolder != null)
			{
				return owner.FeedbacksList[currentFeedbackIndex].ForcedReferenceHolder.GameObjectReference.GetComponent<T>();
			}
			_referenceHolder = GetReferenceHolder(settings, owner, currentFeedbackIndex);
			switch (settings.Mode)
			{
				case Modes.Self:
					return owner.GetComponent<T>();
				case Modes.ChildAtIndex:
					return owner.transform.GetChild(settings.ChildIndex).gameObject.GetComponent<T>();
				case Modes.AnyChild:
					for (int i = 0; i < owner.transform.childCount; i++) 
					{
						if (owner.transform.GetChild(i).GetComponent<T>() != null)
						{
							return owner.transform.GetChild(i).GetComponent<T>();
						}
					}
					return owner.GetComponentInChildren<T>();
				case Modes.Parent:
					return owner.transform.parent.GetComponentInParent<T>();
				case Modes.FirstReferenceHolder: 
				case Modes.PreviousReferenceHolder:
				case Modes.ClosestReferenceHolder:
				case Modes.NextReferenceHolder:
				case Modes.LastReferenceHolder:
					return (_referenceHolder != null)
						? _referenceHolder.GameObjectReference.GetComponent<T>()
						: default(T);
			}
			return default(T);
		}
		
		
		
	}
}