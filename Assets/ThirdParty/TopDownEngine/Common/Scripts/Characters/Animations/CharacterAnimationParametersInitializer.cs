using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
#if UNITY_EDITOR
using UnityEditor.Animations;
#endif

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// CharacterAnimationParametersInitializer 클래스에서 사용할 캐릭터 애니메이션 매개변수 정의를 저장하는 데 사용되는 구조체입니다.
    /// </summary>
    public struct TopDownCharacterAnimationParameter
	{
		/// the name of the parameter
		public string ParameterName;
		/// the type of the parameter
		public AnimatorControllerParameterType ParameterType;

		public TopDownCharacterAnimationParameter(string name, AnimatorControllerParameterType type)
		{
			ParameterName = name;
			ParameterType = type;
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public class CharacterAnimationParametersInitializer : TopDownMonoBehaviour
	{
		[Header("Initialization")]
        /// 이것이 사실이라면 이 구성요소는 문자 매개변수를 추가한 후 자체적으로 제거됩니다.
        [Tooltip("이것이 사실이라면 이 구성요소는 문자 매개변수를 추가한 후 자체적으로 제거됩니다.")]
		public bool AutoRemoveAfterInitialization = true;
		[MMInspectorButton("AddAnimationParameters")]
		public bool AddAnimationParametersButton;

		protected TopDownCharacterAnimationParameter[] ParametersArray = new TopDownCharacterAnimationParameter[]
		{
			new TopDownCharacterAnimationParameter("Alive", AnimatorControllerParameterType.Bool),
			new TopDownCharacterAnimationParameter("Grounded", AnimatorControllerParameterType.Bool),
			new TopDownCharacterAnimationParameter("Idle", AnimatorControllerParameterType.Bool),
			new TopDownCharacterAnimationParameter("Walking", AnimatorControllerParameterType.Bool),
			new TopDownCharacterAnimationParameter("Running", AnimatorControllerParameterType.Bool),
			new TopDownCharacterAnimationParameter("Activating", AnimatorControllerParameterType.Bool),
			new TopDownCharacterAnimationParameter("Crouching", AnimatorControllerParameterType.Bool),
			new TopDownCharacterAnimationParameter("Crawling", AnimatorControllerParameterType.Bool),
			new TopDownCharacterAnimationParameter("Damage", AnimatorControllerParameterType.Trigger),
			new TopDownCharacterAnimationParameter("Dashing", AnimatorControllerParameterType.Bool),
			new TopDownCharacterAnimationParameter("DamageDashing", AnimatorControllerParameterType.Bool),
			new TopDownCharacterAnimationParameter("DashingDirectionX", AnimatorControllerParameterType.Float),
			new TopDownCharacterAnimationParameter("DashingDirectionY", AnimatorControllerParameterType.Float),
			new TopDownCharacterAnimationParameter("DashingDirectionZ", AnimatorControllerParameterType.Float),
			new TopDownCharacterAnimationParameter("DashStarted", AnimatorControllerParameterType.Bool),
			new TopDownCharacterAnimationParameter("Death", AnimatorControllerParameterType.Trigger),
			new TopDownCharacterAnimationParameter("FacingDirection2D", AnimatorControllerParameterType.Float),
			new TopDownCharacterAnimationParameter("FallingDownHole", AnimatorControllerParameterType.Bool),
			new TopDownCharacterAnimationParameter("WeaponEquipped", AnimatorControllerParameterType.Bool),
			new TopDownCharacterAnimationParameter("WeaponEquippedID", AnimatorControllerParameterType.Int),
			new TopDownCharacterAnimationParameter("Jumping", AnimatorControllerParameterType.Bool),
			new TopDownCharacterAnimationParameter("DoubleJumping", AnimatorControllerParameterType.Bool),
			new TopDownCharacterAnimationParameter("HitTheGround", AnimatorControllerParameterType.Bool),
			new TopDownCharacterAnimationParameter("Random", AnimatorControllerParameterType.Float),
			new TopDownCharacterAnimationParameter("RandomConstant", AnimatorControllerParameterType.Int),
			new TopDownCharacterAnimationParameter("Direction", AnimatorControllerParameterType.Float),
			new TopDownCharacterAnimationParameter("Speed", AnimatorControllerParameterType.Float),
			new TopDownCharacterAnimationParameter("xSpeed", AnimatorControllerParameterType.Float),
			new TopDownCharacterAnimationParameter("ySpeed", AnimatorControllerParameterType.Float),
			new TopDownCharacterAnimationParameter("zSpeed", AnimatorControllerParameterType.Float),
			new TopDownCharacterAnimationParameter("HorizontalDirection", AnimatorControllerParameterType.Float),
			new TopDownCharacterAnimationParameter("VerticalDirection", AnimatorControllerParameterType.Float),
			new TopDownCharacterAnimationParameter("RelativeForwardSpeed", AnimatorControllerParameterType.Float),
			new TopDownCharacterAnimationParameter("RelativeLateralSpeed", AnimatorControllerParameterType.Float),
			new TopDownCharacterAnimationParameter("RelativeForwardSpeedNormalized", AnimatorControllerParameterType.Float),
			new TopDownCharacterAnimationParameter("RelativeLateralSpeedNormalized", AnimatorControllerParameterType.Float),
			new TopDownCharacterAnimationParameter("RemappedForwardSpeedNormalized", AnimatorControllerParameterType.Float),
			new TopDownCharacterAnimationParameter("RemappedLateralSpeedNormalized", AnimatorControllerParameterType.Float),
			new TopDownCharacterAnimationParameter("RemappedSpeedNormalized", AnimatorControllerParameterType.Float),
			new TopDownCharacterAnimationParameter("Stunned", AnimatorControllerParameterType.Bool),
			new TopDownCharacterAnimationParameter("Pushing", AnimatorControllerParameterType.Bool),
			new TopDownCharacterAnimationParameter("YRotationSpeed", AnimatorControllerParameterType.Float),
			new TopDownCharacterAnimationParameter("YRotationOffset", AnimatorControllerParameterType.Float),
		};

		protected Animator _animator;
		#if UNITY_EDITOR
		protected AnimatorController _controller;
		#endif
		protected List<string> _parameters = new List<string>();

		/// <summary>
		/// Adds all the default animation parameters on your character's animator
		/// </summary>
		public virtual void AddAnimationParameters()
		{
			// we grab the animator
			_animator = this.gameObject.GetComponent<Animator>();
			if (_animator == null)
			{
				Debug.LogError("You need to add the AnimationParameterInitializer class to a gameobject with an Animator.");
			}

			// we grab the controller
			#if UNITY_EDITOR
			_controller = _animator.runtimeAnimatorController as AnimatorController;
			if (_controller == null)
			{
				Debug.LogError("You need an animator controller on this Animator.");
			}
			#endif

			// we store its parameters
			_parameters.Clear();
			foreach (AnimatorControllerParameter param in _animator.parameters)
			{
				_parameters.Add(param.name);
			}

			// we add all the listed parameters
			foreach (TopDownCharacterAnimationParameter parameter in ParametersArray)
			{
				if (!_parameters.Contains(parameter.ParameterName))
				{
					#if UNITY_EDITOR
					_controller.AddParameter(parameter.ParameterName, parameter.ParameterType);
					#endif
				}
			}

			// we remove this component if needed
			if (AutoRemoveAfterInitialization)
			{
				DestroyImmediate(this);
			}
		}
	}
}