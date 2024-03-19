using UnityEngine;
using System.Collections;

namespace MoreMountains.Tools
{
    /// <summary>
    /// Mecanim의 애니메이션에 이 스크립트를 추가하면 시작 위치와 속도를 제어할 수 있습니다.
    /// </summary>
    [AddComponentMenu("More Mountains/Tools/Animation/MMAnimationModifier")]
	public class MMAnimationModifier : StateMachineBehaviour
	{
		[MMVectorAttribute("Min", "Max")]
		/// the min and max values for the start position of the animation (between 0 and 1)
		public Vector2 StartPosition = new Vector2(0, 0);

		[MMVectorAttribute("Min", "Max")]
		/// the min and max values for the animation speed (1 is normal)
		public Vector2 AnimationSpeed = new Vector2(1, 1);

		protected bool _enteredState = false;
		protected float _initialSpeed;

        /// <summary>
        /// 상태 진입 시 속도와 시작 위치를 수정합니다.
        /// </summary>
        /// <param name="animator"></param>
        /// <param name="stateInfo"></param>
        /// <param name="layerIndex"></param>
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			base.OnStateEnter(animator, stateInfo, layerIndex);

			// handle speed
			_initialSpeed = animator.speed;
			animator.speed = Random.Range(AnimationSpeed.x, AnimationSpeed.y);

			// handle start position
			if (!_enteredState)
			{
				animator.Play(stateInfo.fullPathHash, layerIndex, Random.Range(StartPosition.x, StartPosition.y));
			}
			_enteredState = !_enteredState;
		}

		/// <summary>
		/// On state exit, we restore our speed
		/// </summary>
		/// <param name="animator"></param>
		/// <param name="stateInfo"></param>
		/// <param name="layerIndex"></param>
		public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			base.OnStateExit(animator, stateInfo, layerIndex);
			animator.speed = _initialSpeed;            
		}
	}
}