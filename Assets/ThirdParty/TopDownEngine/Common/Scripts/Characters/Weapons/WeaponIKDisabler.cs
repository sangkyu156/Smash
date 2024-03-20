using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 클래스를 WeaponIK 구성 요소(일반적으로 캐릭터 애니메이터) 옆에 추가할 수 있으며 다음을 수행할 수 있습니다.
    /// IK를 비활성화하고 선택적으로 특정 애니메이션 중에 WeaponAttachment의 부모를 다시 지정합니다.
    /// </summary>
    public class WeaponIKDisabler : TopDownMonoBehaviour
	{
		[Header("Animation Parameter Names")] 
		/// a list of animation parameter names which, if true, should cause IK to be disabled
		[Tooltip("true인 경우 IK를 비활성화해야 하는 애니메이션 매개변수 이름 목록")]
		public List<string> AnimationParametersPreventingIK;
        
		[Header("Attachments")]
		/// the WeaponAttachment transform to reparent
		[Tooltip("WeaponAttachment를 상위로 변환")]
		public Transform WeaponAttachment;
		/// the transform the WeaponAttachment will be reparented to when certain animation parameters are true
		[Tooltip("특정 애니메이션 매개변수가 true일 때 WeaponAttachment의 상위가 변경되는 변환입니다.")]
		public Transform WeaponAttachmentParentNoIK;

		[Header("Settings")]
		/// whether or not to match parent position when disabling IK
		[Tooltip("IK를 비활성화할 때 상위 위치와 일치할지 여부")]
		public bool FollowParentPosition = false;
		/// whether or not to disable weapon aim when disabling IK
		[Tooltip("IK를 비활성화할 때 무기 조준을 비활성화할지 여부")]
		public bool ControlWeaponAim = false;

		protected Transform _initialParent;
		protected Vector3 _initialLocalPosition;
		protected Vector3 _initialLocalScale;
		protected Quaternion _initialRotation;
		protected WeaponIK _weaponIK;
		protected WeaponAim _weaponAim;
		protected Animator _animator;
		protected List<int> _animationParametersHashes;
		protected bool _shouldSetIKLast = true;

		/// <summary>
		/// On Start we initialize our component
		/// </summary>
		protected virtual void Start()
		{
			Initialization();
		}

		/// <summary>
		/// Grabs animator, weaponIK, hashes the animation parameter names, and stores initial positions
		/// </summary>
		protected virtual void Initialization()
		{
			_weaponAim = this.gameObject.GetComponent<WeaponAim>();
			_weaponIK = this.gameObject.GetComponent<WeaponIK>();
			_animator = this.gameObject.GetComponent<Animator>();
			_animationParametersHashes = new List<int>();
            
			foreach (string _animationParameterName in AnimationParametersPreventingIK)
			{
				int newHash = Animator.StringToHash(_animationParameterName);
				_animationParametersHashes.Add(newHash);
			}

			if (WeaponAttachment != null)
			{
				_initialParent = WeaponAttachment.parent;
				_initialLocalPosition = WeaponAttachment.transform.localPosition;
				_initialLocalScale = WeaponAttachment.transform.localScale;
				_initialRotation = WeaponAttachment.transform.localRotation;
			}
		}
        
		/// <summary>
		/// On animator IK, we turn IK on or off if needed
		/// </summary>
		/// <param name="layerIndex"></param>
		protected virtual void OnAnimatorIK(int layerIndex)
		{
			if ((_animator == null) || (_weaponIK == null) || (WeaponAttachment == null))
			{
				return;
			}

			if (_animationParametersHashes.Count <= 0)
			{
				return;
			}

			bool shouldPreventIK = false;
			foreach (int hash in _animationParametersHashes)
			{
				if (_animator.GetBool(hash))
				{
					shouldPreventIK = true;
				}
			}

			if (shouldPreventIK != _shouldSetIKLast)
			{
				PreventIK(shouldPreventIK);
			}

			_shouldSetIKLast = shouldPreventIK;
		}

		/// <summary>
		/// Enables or disables IK
		/// </summary>
		/// <param name="status"></param>
		protected virtual void PreventIK(bool status)
		{
			if (status)
			{
				_weaponIK.AttachLeftHand = false;
				_weaponIK.AttachRightHand = false;
				WeaponAttachment.transform.SetParent(WeaponAttachmentParentNoIK);

				if (FollowParentPosition)
				{
					WeaponAttachment.transform.localPosition = Vector3.zero;
					WeaponAttachment.transform.localScale = WeaponAttachmentParentNoIK.transform.localScale;
					WeaponAttachment.transform.localRotation = Quaternion.identity;	
				}
				
				EnableWeaponAim(false);
			}
			else
			{
				_weaponIK.AttachLeftHand = true;
				_weaponIK.AttachRightHand = true;
				WeaponAttachment.transform.SetParent(_initialParent);
                
				WeaponAttachment.transform.localPosition = _initialLocalPosition;
				WeaponAttachment.transform.localScale = _initialLocalScale;
				WeaponAttachment.transform.localRotation = _initialRotation;

				EnableWeaponAim(true);
			}
		}

		/// <summary>
		/// Enables or disables weapon aim based on the specified status
		/// </summary>
		/// <param name="status"></param>
		protected virtual void EnableWeaponAim(bool status)
		{
			if (!ControlWeaponAim)
			{
				return;
			}

			_weaponAim.enabled = status;
		}
	}
}