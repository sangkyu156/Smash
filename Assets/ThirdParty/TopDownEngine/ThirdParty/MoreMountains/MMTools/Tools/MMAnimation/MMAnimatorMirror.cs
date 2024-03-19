using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 이 클래스를 사용하면 소스 애니메이터의 애니메이터 매개변수 동작을 대상 애니메이터의 매개변수 동작에 미러링할 수 있습니다.
    /// 대상은 소스를 미러링합니다.
    /// 대상과 소스 모두에 존재하는 매개변수만 고려되며, 런타임에 들어가기 전에 두 매개변수가 동일해야 합니다.
    /// </summary>
    public class MMAnimatorMirror : MonoBehaviour
	{
		/// a struct used to store bindings
		public struct MMAnimatorMirrorBind
		{
			public int ParameterHash;
			public AnimatorControllerParameterType ParameterType;
		}

		[Header("Bindings")]
		/// the animator to mirror
		public Animator SourceAnimator;
		/// the animator to mirror to
		public Animator TargetAnimator;

		protected AnimatorControllerParameter[] _sourceParameters;
		protected AnimatorControllerParameter[] _targetParameters;
		protected List<MMAnimatorMirrorBind> _updateParameters;

		/// <summary>
		/// On Awake we initialize
		/// </summary>
		protected virtual void Awake()
		{
			Initialization();
		}

		/// <summary>
		/// Stores animation parameters hashes
		/// </summary>
		public virtual void Initialization()
		{
			if (TargetAnimator == null)
			{
				TargetAnimator = this.gameObject.GetComponent<Animator>();
			}

			if ((TargetAnimator == null) || (SourceAnimator == null))
			{
				return;
			}

			// we store our source parameters
			int numberOfParameters = SourceAnimator.parameterCount;
			_sourceParameters = new AnimatorControllerParameter[numberOfParameters];
			for (int i = 0; i < numberOfParameters; i++)
			{
				_sourceParameters[i] = SourceAnimator.GetParameter(i);
			}

			// we store our target parameters
			numberOfParameters = TargetAnimator.parameterCount;
			_targetParameters = new AnimatorControllerParameter[numberOfParameters];
			for (int i = 0; i < numberOfParameters; i++)
			{
				_targetParameters[i] = TargetAnimator.GetParameter(i);
			}

			// we store our matching parameters
			_updateParameters = new List<MMAnimatorMirrorBind>();

			foreach (AnimatorControllerParameter sourceParam in _sourceParameters)
			{
				foreach (AnimatorControllerParameter targetParam in _targetParameters)
				{
					if (sourceParam.name == targetParam.name)
					{
						MMAnimatorMirrorBind bind = new MMAnimatorMirrorBind();
						bind.ParameterHash = sourceParam.nameHash;
						bind.ParameterType = sourceParam.type;
						_updateParameters.Add(bind);
					}
				}
			}
		}

		/// <summary>
		/// On Update we mirror our behaviours
		/// </summary>
		protected virtual void Update()
		{
			Mirror();
		}

		/// <summary>
		/// Copies animation parameter states from one animator to the other
		/// </summary>
		protected virtual void Mirror()
		{
			if ((TargetAnimator == null) || (SourceAnimator == null))
			{
				return;
			}

			foreach (MMAnimatorMirrorBind bind in _updateParameters)
			{
				switch (bind.ParameterType)
				{
					case AnimatorControllerParameterType.Bool:
						TargetAnimator.SetBool(bind.ParameterHash, SourceAnimator.GetBool(bind.ParameterHash));
						break;
					case AnimatorControllerParameterType.Float:
						TargetAnimator.SetFloat(bind.ParameterHash, SourceAnimator.GetFloat(bind.ParameterHash));
						break;
					case AnimatorControllerParameterType.Int:
						TargetAnimator.SetInteger(bind.ParameterHash, SourceAnimator.GetInteger(bind.ParameterHash));
						break;
					case AnimatorControllerParameterType.Trigger:
						if (SourceAnimator.GetBool(bind.ParameterHash))
						{
							TargetAnimator.SetTrigger(bind.ParameterHash);
						}
						else
						{
							TargetAnimator.ResetTrigger(bind.ParameterHash);
						}
						break;
				}
			}
		}
	}
}