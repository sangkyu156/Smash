using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
#if MM_URP
using UnityEngine.Rendering.Universal;
#endif
using MoreMountains.Feedbacks;

namespace MoreMountains.FeedbacksForThirdParty
{
    /// <summary>
    /// 이 클래스는 인스펙터에 지정된 대상 세트에 초점을 맞추기 위해 URP 피사계 심도를 설정합니다.
    /// </summary>
#if MM_URP
    [RequireComponent(typeof(Volume))]
	#endif
	[AddComponentMenu("More Mountains/Feedbacks/Shakers/PostProcessing/MMAutoFocus_URP")]
	public class MMAutoFocus_URP : MonoBehaviour
	{
		[Header("Bindings")]
		/// the position of the camera
		[Tooltip("카메라의 위치")]
		public Transform CameraTransform;
		/// a list of all possible targets
		[Tooltip("가능한 모든 대상 목록")]
		public Transform[] FocusTargets;
		[Header("Setup")]
		/// the current target of this auto focus
		[Tooltip("이 자동 초점의 현재 목표")]
		public float FocusTargetID;
		[Header("Desired Aperture")]
		/// the aperture to work with
		[Tooltip("작업할 조리개")]
		[Range(0.1f, 20f)]
		public float Aperture = 0.1f;

		#if MM_URP
		protected Volume _volume;
		protected VolumeProfile _profile;
		protected DepthOfField _depthOfField;

		/// <summary>
		/// On Start, stores volume, profile and DoF
		/// </summary>
		void Start()
		{
			_volume = GetComponent<Volume>();
			_profile = _volume.profile;
			_profile.TryGet<DepthOfField>(out _depthOfField);
		}

		/// <summary>
		/// On update we set our focus distance and aperture
		/// </summary>
		void Update()
		{
			float distance = Vector3.Distance(CameraTransform.position, FocusTargets[Mathf.FloorToInt(FocusTargetID)].position);
			_depthOfField.focusDistance.Override(distance);
			_depthOfField.aperture.Override(Aperture);
		}
		#endif
	}
}