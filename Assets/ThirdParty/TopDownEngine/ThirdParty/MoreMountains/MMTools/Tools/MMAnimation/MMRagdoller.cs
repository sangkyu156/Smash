using UnityEngine;
using System.Collections.Generic;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 봉제인형 신체 부위 정보를 저장하는 데 사용되는 클래스
    /// </summary>
    public class RagdollBodyPart
	{
		public Transform BodyPartTransform;
		public Vector3 StoredPosition;
		public Quaternion StoredRotation;
	}

    /// <summary>
    /// 일반적으로 애니메이터가 조종하는 캐릭터에 봉제 인형을 조종하고 우아하게 떨어지게 하려면 이 클래스를 사용합니다.
    /// 이 스크립트의 영향을 받지 않으려는 봉제인형 부분(예: 무기)이 있는 경우 해당 부분에 MMRagdollerIgnore 구성 요소를 추가하기만 하면 됩니다.
    /// </summary>
    [AddComponentMenu("More Mountains/Tools/Animation/MMRagdoller")]
	public class MMRagdoller : MonoBehaviour
	{
        /// <summary>
        /// 봉제 인형의 가능한 상태:
        /// - Animated: 애니메이터 컨트롤러에 의해 구동되고 강체는 잠자기 상태입니다.
        /// - Ragdolling: 완전한 래그돌 모드, 순수하게 물리학 기반
        /// - Blending : 래그돌링과 애니메이션 간 전환
        /// </summary>
        public enum RagdollStates
		{
			Animated,
			Ragdolling,
			Blending
		}

		[Header("Ragdoll")]
		/// the current state of the ragdoll
		public RagdollStates CurrentState = RagdollStates.Animated;
		/// the duration in seconds it takes to blend from Ragdolling to Animated
		public float RagdollToMecanimBlendDuration = 0.5f;

		[Header("Rigidbodies")]
		/// The rigidbody attached to the main body part of the ragdoll (usually the Pelvis) 
		public Rigidbody MainRigidbody;
		/// if this is true, all rigidbodies will be forced to sleep every frame
		public bool ForceSleep = true;
		/// whether or not blending will occur when going from ragdolling to animated
		public bool AllowBlending = true;

		protected float _mecanimToGetUpTransitionTime = 0.05f;
		protected float _ragdollingEndTimestamp = -float.MaxValue;
		protected Vector3 _ragdolledHipPosition;
		protected Vector3 _ragdolledHeadPosition;
		protected Vector3 _ragdolledFeetPosition;
		protected List<RagdollBodyPart> _bodyparts = new List<RagdollBodyPart>();
		protected Animator _animator;
		protected List<Component> _rigidbodiesTempList;
		protected Component[] _rigidbodies;
		protected HashSet<int> _animatorParameters;

		protected const string _getUpFromBackAnimationParameterName = "GetUpFromBack";
		protected int _getUpFromBackAnimationParameter;
		protected const string _getUpFromBellyAnimationParameterName = "GetUpFromBelly";
		protected int _getUpFromBellyAnimationParameter;
		protected bool _initialized = false;

		/// <summary>
		/// Use this to get the current state of the ragdoll or to set a new one
		/// </summary>
		public bool Ragdolling
		{
			get
			{
				// if we're not animated, we're ragdolling
				return CurrentState != RagdollStates.Animated;
			}
			set
			{
				if (value == true)
				{
					// if we're 
					if (CurrentState == RagdollStates.Animated)
					{
						SetIsKinematic(false);
						_animator.enabled = false;
						CurrentState = RagdollStates.Ragdolling;
						MMAnimatorExtensions.UpdateAnimatorBool(_animator, _getUpFromBackAnimationParameter, false, _animatorParameters);
						MMAnimatorExtensions.UpdateAnimatorBool(_animator, _getUpFromBellyAnimationParameter, false, _animatorParameters);
					}
				}
				else
				{
					if (CurrentState == RagdollStates.Ragdolling)
					{
						SetIsKinematic(true);
						_ragdollingEndTimestamp = Time.time;
						_animator.enabled = true;

						CurrentState = AllowBlending ? RagdollStates.Blending: RagdollStates.Animated;
						

						foreach (RagdollBodyPart bodypart in _bodyparts)
						{
							bodypart.StoredRotation = bodypart.BodyPartTransform.rotation;
							bodypart.StoredPosition = bodypart.BodyPartTransform.position;
						}

						_ragdolledFeetPosition = 0.5f * (_animator.GetBoneTransform(HumanBodyBones.LeftToes).position + _animator.GetBoneTransform(HumanBodyBones.RightToes).position);
						_ragdolledHeadPosition = _animator.GetBoneTransform(HumanBodyBones.Head).position;
						_ragdolledHipPosition = _animator.GetBoneTransform(HumanBodyBones.Hips).position;

						if (_animator.GetBoneTransform(HumanBodyBones.Hips).forward.y > 0)
						{
							MMAnimatorExtensions.UpdateAnimatorBool(_animator, _getUpFromBackAnimationParameter, true, _animatorParameters);
						}
						else
						{
							MMAnimatorExtensions.UpdateAnimatorBool(_animator, _getUpFromBellyAnimationParameter, true, _animatorParameters);
						}
					}
				}
			}
		}

		/// <summary>
		/// On start we initialize our ragdoller
		/// </summary>
		protected virtual void Start()
		{
			Initialization();
		}

		/// <summary>
		/// Grabs rigidbodies, adds body parts and stores the animator
		/// </summary>
		protected virtual void Initialization()
		{
			// we grab all rigidbodies and set them to kinematic
			_rigidbodies = GetComponentsInChildren(typeof(Rigidbody));

			_rigidbodiesTempList = new List<Component>();
			foreach (Component rigidbody in _rigidbodies)
			{
				if (rigidbody.gameObject.MMGetComponentNoAlloc<MMRagdollerIgnore>() == null)
				{
					_rigidbodiesTempList.Add(rigidbody);
				}
			}

			_rigidbodies = null;
			_rigidbodies = _rigidbodiesTempList.ToArray();


			if (CurrentState == RagdollStates.Animated)
			{
				SetIsKinematic(true);
			}
			else
			{
				SetIsKinematic(false);
			}

			// we grab all transforms and add a RagdollBodyPart to them
			Component[] transforms = GetComponentsInChildren(typeof(Transform));
			foreach (Component component in transforms)
			{
				if (component.transform != this.transform)
				{
					RagdollBodyPart bodyPart = new RagdollBodyPart { BodyPartTransform = component as Transform };
					_bodyparts.Add(bodyPart);
				}
			}

			// we store our animator
			_animator = this.gameObject.GetComponent<Animator>();
			RegisterAnimatorParameters();
			
			_initialized = true;
		}

		/// <summary>
		/// Registers our animation parameters
		/// </summary>
		protected virtual void RegisterAnimatorParameters()
		{
			_animatorParameters = new HashSet<int>();

			_getUpFromBackAnimationParameter = Animator.StringToHash(_getUpFromBackAnimationParameterName);
			_getUpFromBellyAnimationParameter = Animator.StringToHash(_getUpFromBellyAnimationParameterName);

			if (_animator == null)
			{
				return;
			}
			if (_animator.MMHasParameterOfType(_getUpFromBackAnimationParameterName, AnimatorControllerParameterType.Bool))
			{
				_animatorParameters.Add(_getUpFromBackAnimationParameter);
			}
			if (_animator.MMHasParameterOfType(_getUpFromBellyAnimationParameterName, AnimatorControllerParameterType.Bool))
			{
				_animatorParameters.Add(_getUpFromBellyAnimationParameter);
			}
		}

		/// <summary>
		/// Sets all rigidbodies in the ragdoll to kinematic and stops them from detecting collisions (or the other way around)
		/// </summary>
		/// <param name="isKinematic"></param>
		protected virtual void SetIsKinematic(bool isKinematic)
		{
			foreach (Component rigidbody in _rigidbodies)
			{
				if (rigidbody.transform != this.transform)
				{
					(rigidbody as Rigidbody).detectCollisions = !isKinematic;
					(rigidbody as Rigidbody).isKinematic = isKinematic;
				}
			}
		}

		/// <summary>
		/// Forces all rigidbodies in the ragdoll to sleep
		/// </summary>
		public virtual void ForceRigidbodiesToSleep()
		{
			foreach (Component rigidbody in _rigidbodies)
			{
				if (rigidbody.transform != this.transform)
				{
					(rigidbody as Rigidbody).Sleep();
				}
			}
		}

		/// <summary>
		/// On late update, we force our ragdoll elements to sleep and handle blending
		/// </summary>
		protected virtual void LateUpdate()
		{
			if ((CurrentState == RagdollStates.Animated) && ForceSleep)
			{
				ForceRigidbodiesToSleep();
			}

			HandleBlending();
		}

		/// <summary>
		/// Blends between ragdolling and animated and switches to Animated at the end
		/// </summary>
		protected virtual void HandleBlending()
		{
			if (CurrentState == RagdollStates.Blending)
			{
				if (Time.time <= _ragdollingEndTimestamp + _mecanimToGetUpTransitionTime)
				{
					transform.position = GetRootPosition();

					Vector3 ragdollingDirection = _ragdolledHeadPosition - _ragdolledFeetPosition;
					ragdollingDirection.y = 0;

					Vector3 meanFeetPosition = 0.5f * (_animator.GetBoneTransform(HumanBodyBones.LeftFoot).position + _animator.GetBoneTransform(HumanBodyBones.RightFoot).position);
					Vector3 animatedDirection = _animator.GetBoneTransform(HumanBodyBones.Head).position - meanFeetPosition;
					animatedDirection.y = 0;

					transform.rotation *= Quaternion.FromToRotation(animatedDirection.normalized, ragdollingDirection.normalized);
				}
				float ragdollBlendAmount = 1.0f - (Time.time - _ragdollingEndTimestamp - _mecanimToGetUpTransitionTime) / RagdollToMecanimBlendDuration;
				ragdollBlendAmount = Mathf.Clamp01(ragdollBlendAmount);

				foreach (RagdollBodyPart bodypart in _bodyparts)
				{
					if (bodypart.BodyPartTransform != transform)
					{
						if (bodypart.BodyPartTransform == _animator.GetBoneTransform(HumanBodyBones.Hips))
						{
							bodypart.BodyPartTransform.position = Vector3.Lerp(bodypart.BodyPartTransform.position, bodypart.StoredPosition, ragdollBlendAmount);
						}
						bodypart.BodyPartTransform.rotation = Quaternion.Slerp(bodypart.BodyPartTransform.rotation, bodypart.StoredRotation, ragdollBlendAmount);
					}
				}

				if (ragdollBlendAmount == 0)
				{
					CurrentState = RagdollStates.Animated;
					return;
				}
			}
		}

		/// <summary>
		/// Returns the current position of the ragdoll (technically the hips position)
		/// </summary>
		/// <returns></returns>
		public Vector3 GetPosition()
		{
			if (!_initialized)
			{
				Initialization();
			}
			Vector3 newPosition = (_animator.GetBoneTransform(HumanBodyBones.Hips) == null) ? MainRigidbody.position : _animator.GetBoneTransform(HumanBodyBones.Hips).position; 
			return newPosition;
		}

		/// <summary>
		/// Returns the offset root position
		/// </summary>
		/// <returns></returns>
		protected Vector3 GetRootPosition()
		{
			Vector3 ragdollPosition = (_animator.GetBoneTransform(HumanBodyBones.Hips) == null) ? MainRigidbody.position : _animator.GetBoneTransform(HumanBodyBones.Hips).position; 
			Vector3 animatedToRagdolling = _ragdolledHipPosition - ragdollPosition;
			Vector3 newRootPosition = transform.position + animatedToRagdolling;
			RaycastHit[] hits = Physics.RaycastAll(new Ray(newRootPosition, Vector3.down));
			newRootPosition.y = 0;
			foreach (RaycastHit hit in hits)
			{
				if (!hit.transform.IsChildOf(transform))
				{
					newRootPosition.y = Mathf.Max(newRootPosition.y, hit.point.y);
				}
			}
			return newRootPosition;
		}
	}
}