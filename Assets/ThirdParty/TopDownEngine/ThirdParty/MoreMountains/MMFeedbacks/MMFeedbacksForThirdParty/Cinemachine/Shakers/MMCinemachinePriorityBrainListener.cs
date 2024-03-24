using System.Collections;
using UnityEngine;
#if MM_CINEMACHINE
using Cinemachine;
#endif
using MoreMountains.Feedbacks;

namespace MoreMountains.FeedbacksForThirdParty
{
    /// <summary>
    /// 이를 Cinemachine 브레인에 추가하면 사용자 정의 블렌드 전환(MMFeedbackCinemachineTransition과 함께 사용)을 허용할 수 있습니다.
    /// </summary>
    [AddComponentMenu("More Mountains/Feedbacks/Shakers/Cinemachine/MMCinemachinePriorityBrainListener")]
	#if MM_CINEMACHINE
	[RequireComponent(typeof(CinemachineBrain))]
	#endif
	public class MMCinemachinePriorityBrainListener : MonoBehaviour
	{
        
		[HideInInspector] 
		public TimescaleModes TimescaleMode = TimescaleModes.Scaled;
        
        
		public virtual float GetTime() { return (TimescaleMode == TimescaleModes.Scaled) ? Time.time : Time.unscaledTime; }
		public virtual float GetDeltaTime() { return (TimescaleMode == TimescaleModes.Scaled) ? Time.deltaTime : Time.unscaledDeltaTime; }
    
		#if MM_CINEMACHINE    
		protected CinemachineBrain _brain;
		protected CinemachineBlendDefinition _initialDefinition;
		protected Coroutine _coroutine;

		/// <summary>
		/// On Awake we grab our brain
		/// </summary>
		protected virtual void Awake()
		{
			_brain = this.gameObject.GetComponent<CinemachineBrain>();
		}

        /// <summary>
        /// 이벤트를 받을 때 필요한 경우 기본 전환을 변경합니다.
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="forceMaxPriority"></param>
        /// <param name="newPriority"></param>
        /// <param name="forceTransition"></param>
        /// <param name="blendDefinition"></param>
        /// <param name="resetValuesAfterTransition"></param>
        public virtual void OnMMCinemachinePriorityEvent(MMChannelData channelData, bool forceMaxPriority, int newPriority, bool forceTransition, CinemachineBlendDefinition blendDefinition, bool resetValuesAfterTransition, TimescaleModes timescaleMode, bool restore = false)
		{
			if (forceTransition)
			{
				if (_coroutine != null)
				{
					StopCoroutine(_coroutine);
				}
				else
				{
					_initialDefinition = _brain.m_DefaultBlend;
				}
				_brain.m_DefaultBlend = blendDefinition;
				TimescaleMode = timescaleMode;
				_coroutine = StartCoroutine(ResetBlendDefinition(blendDefinition.m_Time));                
			}
		}

        /// <summary>
        /// 기본 전환을 초기 값으로 재설정하는 데 사용되는 코루틴
        /// </summary>
        /// <param name="delay"></param>
        /// <returns></returns>
        protected virtual IEnumerator ResetBlendDefinition(float delay)
		{
			for (float timer = 0; timer < delay; timer += GetDeltaTime())
			{
				yield return null;
			}
			_brain.m_DefaultBlend = _initialDefinition;
			_coroutine = null;
		}

		/// <summary>
		/// On enable we start listening for events
		/// </summary>
		protected virtual void OnEnable()
		{
			_coroutine = null;
			MMCinemachinePriorityEvent.Register(OnMMCinemachinePriorityEvent);
		}

		/// <summary>
		/// Stops listening for events
		/// </summary>
		protected virtual void OnDisable()
		{
			if (_coroutine != null)
			{
				StopCoroutine(_coroutine);
			}
			_coroutine = null;
			MMCinemachinePriorityEvent.Unregister(OnMMCinemachinePriorityEvent);
		}
		#endif
	}
}