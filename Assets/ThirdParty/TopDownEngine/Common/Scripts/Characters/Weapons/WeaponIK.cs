using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 클래스를 사용하면 3D 캐릭터가 현재 무기의 핸들을 잡고 조준하는 곳을 볼 수 있습니다.
    /// 약간의 설정이 필요합니다. 캐릭터에 CharacterHandleWeapon 구성 요소가 있어야 하며, IKPass가 활성화된 애니메이터가 필요합니다(이는 애니메이터의 레이어 탭에서 설정됨).
    /// 애니메이터의 아바타는 반드시 휴머노이드로 설정되어야 합니다.
    /// 그리고 해당 스크립트를 애니메이터와 동일한 게임 개체에 배치해야 합니다(그렇지 않으면 작동하지 않습니다).
    /// 마지막으로 무기에 왼쪽 및 오른쪽 핸들(또는 둘 중 하나만)을 설정해야 합니다.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Weapons/Weapon IK")]
	public class WeaponIK : TopDownMonoBehaviour
	{
		[Header("Bindings")]
		/// The transform to use as a target for the left hand
		[Tooltip("왼손의 대상으로 사용할 변환")]
		public Transform LeftHandTarget = null;
		/// The transform to use as a target for the right hand
		[Tooltip("오른손의 대상으로 사용할 변환")]
		public Transform RightHandTarget = null;

		[Header("Attachments")]
		/// whether or not to attach the left hand to its target
		[Tooltip("왼손을 타겟에 붙일지 여부")]
		public bool AttachLeftHand = true;
		/// whether or not to attach the right hand to its target
		[Tooltip("오른손을 타겟에 붙일지 여부")]
		public bool AttachRightHand = true;
        
		[Header("Head")]
		/// the minimum and maximum weights to apply to the Head look at weights when using IK 
		[MMVector("Min","Max")]
		public Vector2 HeadWeights = new Vector2(0f, 1f);
        
		protected Animator _animator;

		protected virtual void Start()
		{
			_animator = GetComponent<Animator> ();
		}
        
		/// <summary>
		/// During the animator's IK pass, tries to attach the avatar's hands to the weapon
		/// </summary>
		protected virtual void OnAnimatorIK(int layerIndex)
		{
			if (_animator == null)
			{
				return;
			}

			//if the IK is active, set the position and rotation directly to the goal. 

			if (AttachLeftHand)
			{
				if (LeftHandTarget != null)
				{
					AttachHandToHandle(AvatarIKGoal.LeftHand, LeftHandTarget);

					_animator.SetLookAtWeight(HeadWeights.y);
					_animator.SetLookAtPosition(LeftHandTarget.position);
				}
				else
				{
					DetachHandFromHandle(AvatarIKGoal.LeftHand);
				}
			}
			
			if (AttachRightHand)
			{
				if (RightHandTarget != null)
				{
					AttachHandToHandle(AvatarIKGoal.RightHand, RightHandTarget);
				}
				else
				{
					DetachHandFromHandle(AvatarIKGoal.RightHand);
				}
			}
			

		}  

		/// <summary>
		/// Attaches the hands to the handles
		/// </summary>
		/// <param name="hand"></param>
		/// <param name="handle"></param>
		protected virtual void AttachHandToHandle(AvatarIKGoal hand, Transform handle)
		{
			_animator.SetIKPositionWeight(hand,1);
			_animator.SetIKRotationWeight(hand,1);  
			_animator.SetIKPosition(hand,handle.position);
			_animator.SetIKRotation(hand,handle.rotation);
		}

		/// <summary>
		/// Detachs the hand from handle, if the IK is not active, set the position and rotation of the hand and head back to the original position
		/// </summary>
		/// <param name="hand">Hand.</param>
		protected virtual void DetachHandFromHandle(AvatarIKGoal hand)
		{
			_animator.SetIKPositionWeight(hand,0);
			_animator.SetIKRotationWeight(hand,0); 
			_animator.SetLookAtWeight(HeadWeights.x);
		}

		/// <summary>
		/// Binds the character hands to the handles targets
		/// </summary>
		/// <param name="leftHand">Left hand.</param>
		/// <param name="rightHand">Right hand.</param>
		public virtual void SetHandles(Transform leftHand, Transform rightHand)
		{
			LeftHandTarget = leftHand;
			RightHandTarget = rightHand;
		}
	}
}