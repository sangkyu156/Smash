using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
	[AddComponentMenu("TopDown Engine/Character/Abilities/Character Damage Dash 3D")]
	public class CharacterDamageDash3D : CharacterDash3D
	{
		[Header("Damage Dash")]
        /// 돌진할 때 활성화할 DamageOnTouch 개체(보통 캐릭터 모델 아래에 배치되며, 트리거하도록 설정된 어떤 형태의 Collider2D가 필요함)
        [Tooltip("돌진할 때 활성화할 DamageOnTouch 개체(보통 캐릭터 모델 아래에 배치되며, 트리거하도록 설정된 어떤 형태의 Collider2D가 필요함)")]
		public DamageOnTouch TargetDamageOnTouch;
        
		protected const string _damageDashingAnimationParameterName = "DamageDashing";
		protected int _damageDashingAnimationParameter;
        
		/// <summary>
		/// On initialization, we disable our damage on touch object
		/// </summary>
		protected override void Initialization()
		{
			base.Initialization();
			TargetDamageOnTouch?.gameObject.SetActive(false);
		}

		/// <summary>
		/// When we start to dash, we activate our damage object
		/// </summary>
		public override void DashStart()
		{
			base.DashStart();
			TargetDamageOnTouch?.gameObject.SetActive(true);
		}

		/// <summary>
		/// When we stop dashing, we disable our damage object
		/// </summary>
		public override void DashStop()
		{
			base.DashStop();
			TargetDamageOnTouch?.gameObject.SetActive(false);
		}
        
		/// <summary>
		/// Adds required animator parameters to the animator parameters list if they exist
		/// </summary>
		protected override void InitializeAnimatorParameters()
		{
			base.InitializeAnimatorParameters();
			RegisterAnimatorParameter(_damageDashingAnimationParameterName, AnimatorControllerParameterType.Bool, out _damageDashingAnimationParameter);
		}

		/// <summary>
		/// At the end of each cycle, we send our Running status to the character's animator
		/// </summary>
		public override void UpdateAnimator()
		{
			base.UpdateAnimator();
			MMAnimatorExtensions.UpdateAnimatorBool(_animator, _damageDashingAnimationParameter, (_movement.CurrentState == CharacterStates.MovementStates.Dashing), _character._animatorParameters, _character.RunAnimatorSanityChecks);
			_dashStartedThisFrame = false;
		}
	}
}