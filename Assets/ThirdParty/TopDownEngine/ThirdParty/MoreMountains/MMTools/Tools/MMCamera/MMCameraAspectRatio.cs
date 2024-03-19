#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace MoreMountains.Tools
{
	[RequireComponent(typeof(Camera))]
    /// <summary>
    /// 카메라에 종횡비를 강제로 적용합니다.
    /// </summary>
    [AddComponentMenu("More Mountains/Tools/Camera/MMCameraAspectRatio")]
	public class MMCameraAspectRatio : MonoBehaviour 
	{
		public enum Modes { Fixed, ScreenRatio }

		[Header("Camera")]
		/// the camera to change the aspect ratio on
		[Tooltip("화면 비율을 변경하는 카메라")]
		public Camera TargetCamera;
		/// the mode of choice, fixed will force a specified ratio, while ScreenRatio will adapt the camera's aspect to the current screen ratio
		[Tooltip("선택한 모드인 고정은 지정된 비율을 강제로 적용하고, ScreenRatio는 카메라의 측면을 현재 화면 비율에 맞게 조정합니다.")]
		public Modes Mode = Modes.Fixed;
		/// in fixed mode, the ratio to apply to the camera
		[Tooltip("고정 모드에서 카메라에 적용할 비율")]
		[MMEnumCondition("Mode", (int)Modes.Fixed)]
		public Vector2 FixedAspectRatio = Vector2.zero;

		[Header("Automation")]
		/// whether or not to apply the ratio automatically on Start
		[Tooltip("시작시 비율을 자동으로 적용할지 여부")]
		public bool ApplyAspectRatioOnStart = true;
		/// whether or not to apply the ratio automatically on enable
		[Tooltip("활성화 시 비율을 자동으로 적용할지 여부")]
		public bool ApplyAspectRatioOnEnable = false;

		[Header("Debug")] 
		[MMInspectorButton("ApplyAspectRatio")]
		public bool ApplyAspectRatioButton;
		
		protected float _defaultAspect = 16f / 9f;

		/// <summary>
		/// On enable we apply our aspect ratio if needed
		/// </summary>
		protected virtual void OnEnable()
		{
			if (ApplyAspectRatioOnEnable) { ApplyAspectRatio(); }
		}

		/// <summary>
		/// On start we apply our aspect ratio if needed
		/// </summary>
		protected virtual void Start()
		{
			if (ApplyAspectRatioOnStart) { ApplyAspectRatio(); }
		}

		/// <summary>
		/// Applies the specified aspect ratio
		/// </summary>
		public virtual void ApplyAspectRatio()
		{
			if (TargetCamera == null)
			{
				return;
			}

			float newAspectRatio = _defaultAspect;
			float ratioX = 1f;
			float ratioY = 1f;
			switch (Mode)
			{
				case Modes.Fixed:
					ratioX = FixedAspectRatio.x;
					ratioY = FixedAspectRatio.y;
					break;
				case Modes.ScreenRatio:
					#if UNITY_EDITOR
					string[] res = UnityStats.screenRes.Split('x');
					ratioX = int.Parse(res[0]);
					ratioY = int.Parse(res[1]);
					#else
						ratioX = Screen.width;
						ratioY = Screen.height;
					#endif
					
					break;
			}
			newAspectRatio = ratioY != 0f ? ratioX / ratioY : _defaultAspect;
			TargetCamera.aspect = newAspectRatio;
		}
		
	}
}