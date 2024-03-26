using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 이 구성 요소를 캐릭터에 추가하면 기절할 수 있습니다. 캐릭터를 기절시키려면 Stun 또는 StunFor 메서드를 호출하기만 하면 됩니다. 이 구성요소 검사기 하단에 테스트 버튼이 있습니다. StunZones를 사용하여 캐릭터를 기절시킬 수도 있습니다.
    /// Animator parameters : Stunned (bool)
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/Abilities/Character Stun")] 
	public class CharacterStun : CharacterAbility
	{
		/// This method is only used to display a helpbox text at the beginning of the ability's inspector
		public override string HelpBoxText() { return "Add this component to a character and it'll be able to be stunned. To stun a character, simply call its Stun or StunFor methods. You'll find test buttons at the bottom of this component's inspector. You can also use StunZones to stun your characters."; }
        
		[Header("IK")]
		/// a weapon IK to pilot when stunned
		[Tooltip("기절했을 때 조종할 무기 IK")]
		public WeaponIK BoundWeaponIK;
		/// whether or not to detach the left hand of the character from IK when stunned
		[Tooltip("기절 시 캐릭터의 왼손을 IK에서 분리할지 여부")]
		public bool DetachLeftHand = false;
		/// whether or not to detach the right hand of the character from IK when stunned
		[Tooltip("기절 시 캐릭터의 오른손을 IK에서 분리할지 여부")]
		public bool DetachRightHand = false;
        
		[Header("Weapon Models")]
		/// whether or not to disable the weapon model when stunned
		[Tooltip("기절 시 무기 모델을 비활성화할지 여부")]
		public bool DisableAimWeaponModelAtTargetDuringStun = false;
		/// the list of weapon models to disable when stunned
		[Tooltip("기절 시 비활성화할 무기 모델 목록")]
		public List<WeaponModel> WeaponModels;
        
		[Header("Tests")]
		/// a test button to stun this character
		[MMInspectorButton("Stun")]
		public bool StunButton;
		/// a test button to exit stun on this character
		[MMInspectorButton("ExitStun")]
		public bool ExitStunButton;
        
		protected const string _stunnedAnimationParameterName = "Stunned";
		protected int _stunnedAnimationParameter;
		protected Coroutine _stunCoroutine;
		protected CharacterStates.CharacterConditions _previousCondition;

		/// <summary>
		/// Stuns the character
		/// </summary>
		public virtual void Stun()
		{
			if ((_previousCondition != CharacterStates.CharacterConditions.Stunned) && (_condition.CurrentState != CharacterStates.CharacterConditions.Stunned))
			{
				_previousCondition = _condition.CurrentState;
			} 		
			_condition.ChangeState(CharacterStates.CharacterConditions.Stunned);
			_controller.SetMovement(Vector3.zero);
			AbilityStartFeedbacks?.PlayFeedbacks();//스턴능력이 시작될때 = 스털걸렸을때 그래서 스타트피드백 실행하는거
            DetachIK();
		}
        
		/// <summary>
		/// Stuns the character for the specified duration
		/// </summary>
		/// <param name="duration"></param>
		public virtual void StunFor(float duration)
		{
			if (_stunCoroutine != null)
			{
				StopCoroutine(_stunCoroutine);
			}
			_stunCoroutine = StartCoroutine(StunCoroutine(duration));
		}

		/// <summary>
		/// Exits stun, resetting condition to the previous one
		/// </summary>
		public virtual void ExitStun()
		{
			if (_condition.CurrentState != CharacterStates.CharacterConditions.Stunned)
			{
				return;
			}
			
			AbilityStopFeedbacks?.PlayFeedbacks();
			_condition.ChangeState(_previousCondition);
			AttachIK();
		}

		/// <summary>
		/// Stuns the character, waits for the specified duration, then exits stun
		/// </summary>
		/// <param name="duration"></param>
		/// <returns></returns>
		protected virtual IEnumerator StunCoroutine(float duration)
		{
			Stun();
			yield return MMCoroutine.WaitFor(duration);
			ExitStun();
		}

		/// <summary>
		/// Detaches IK
		/// </summary>
		protected virtual void DetachIK()
		{
			if (DetachLeftHand) { BoundWeaponIK.AttachLeftHand = false; }
			if (DetachRightHand) { BoundWeaponIK.AttachRightHand = false; }
			if (DisableAimWeaponModelAtTargetDuringStun)
			{
				foreach(WeaponModel model in WeaponModels)
				{
					model.AimWeaponModelAtTarget = false;
				}
			}
		}

		/// <summary>
		/// Attaches IK
		/// </summary>
		protected virtual void AttachIK()
		{
			if (DetachLeftHand) { BoundWeaponIK.AttachLeftHand = true; }
			if (DetachRightHand) { BoundWeaponIK.AttachRightHand = true; }
			if (DisableAimWeaponModelAtTargetDuringStun)
			{
				foreach (WeaponModel model in WeaponModels)
				{
					model.AimWeaponModelAtTarget = true;
				}
			}
		}

		/// <summary>
		/// Adds required animator parameters to the animator parameters list if they exist
		/// </summary>
		protected override void InitializeAnimatorParameters()
		{
			RegisterAnimatorParameter (_stunnedAnimationParameterName, AnimatorControllerParameterType.Bool, out _stunnedAnimationParameter);
		}

		/// <summary>
		/// At the end of each cycle, we send our Running status to the character's animator
		/// </summary>
		public override void UpdateAnimator()
		{
			MMAnimatorExtensions.UpdateAnimatorBool(_animator, _stunnedAnimationParameter, (_condition.CurrentState == CharacterStates.CharacterConditions.Stunned),_character._animatorParameters, _character.RunAnimatorSanityChecks);
		}
	}
}