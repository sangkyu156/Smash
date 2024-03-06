using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if MM_POSTPROCESSING
using UnityEngine.Rendering.PostProcessing;
#endif
using MoreMountains.Feedbacks;

namespace MoreMountains.FeedbacksForThirdParty
{
    /// <summary>
    /// 이 클래스는 검사기에 지정된 대상 집합에 초점을 맞추기 위해 피사계 심도를 설정합니다.
    /// </summary>
    [AddComponentMenu("More Mountains/Feedbacks/Shakers/PostProcessing/MMAutoFocus")]
	#if MM_POSTPROCESSING
	[RequireComponent(typeof(PostProcessVolume))]
	#endif
	public class MMAutoFocus : MonoBehaviour
	{
		[Header("Bindings")]
        /// 카메라의 위치
        [Tooltip("카메라의 위치")]
		public Transform CameraTransform;
        /// 가능한 모든 대상 목록
        [Tooltip("가능한 모든 대상 목록")]
		public Transform[] FocusTargets;
        /// 포커스 대상에 적용할 오프셋
        [Tooltip("포커스 대상에 적용할 오프셋")]
		public Vector3 Offset;

		[Header("Setup")]
        /// 이 자동 초점의 현재 목표
        [Tooltip("이 자동 초점의 현재 목표")]
		public float FocusTargetID;
        
		[Header("Desired Aperture")]
        /// 작업할 구멍
        [Tooltip("작업할 구멍")]
		[Range(0.1f, 20f)]
		public float Aperture = 0.1f;

        
		#if MM_POSTPROCESSING
		protected PostProcessVolume _volume;
		protected PostProcessProfile _profile;
		protected DepthOfField _depthOfField;

        /// <summary>
        /// 처음에는 볼륨과 프로필을 가져옵니다.
        /// </summary>
        void Start()
		{
			_volume = GetComponent<PostProcessVolume>();
			_profile = _volume.profile;
			_profile.TryGetSettings<DepthOfField>(out _depthOfField);
		}

        /// <summary>
        /// 대상에 DoF를 적용합니다.
        /// </summary>
        void Update()
		{
			int focusTargetID = Mathf.FloorToInt(FocusTargetID);
			if (focusTargetID < FocusTargets.Length)
			{
				float distance = Vector3.Distance(CameraTransform.position, FocusTargets[focusTargetID].position + Offset);
				_depthOfField.focusDistance.Override(distance);
				_depthOfField.aperture.Override(Aperture);    
			}
		}
		#endif
	}
}