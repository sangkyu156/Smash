using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Rendering;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 이 구성 요소를 개체에 추가하면 대상 프레임 속도와 vsync 수가 설정됩니다. 대상 FPS가 작동하려면 vsync 수가 0이어야 합니다.
    /// </summary>
    [AddComponentMenu("More Mountains/Tools/Performance/MMFPSUnlock")]
	public class MMFPSUnlock : MonoBehaviour
	{
		/// the target FPS you want the game to run at, that's up to how many times Update will run every second
		[Tooltip("게임을 실행하려는 목표 FPS는 매초마다 업데이트가 실행되는 횟수에 달려 있습니다.")]
		public int TargetFPS;
		/// the number of frames to wait before rendering the next one. 0 will render every frame, 1 will render every 2 frames, 5 will render every 5 frames, etc
		[Tooltip("다음 프레임을 렌더링하기 전에 대기할 프레임 수입니다. 0은 모든 프레임을 렌더링하고, 1은 2프레임마다 렌더링하고, 5는 5프레임마다 렌더링하는 등입니다.")]
		public int RenderFrameInterval = 0;
		[Range(0,2)]
		/// whether vsync should be enabled or not (on a 60Hz screen, 1 : 60fps, 2 : 30fps, 0 : don't wait for vsync)
		[Tooltip("vsync를 활성화해야 하는지 여부(60Hz 화면에서 1: 60fps, 2: 30fps, 0: vsync를 기다리지 않음)")]
		public int VSyncCount = 0;

		/// <summary>
		/// On start we change our target fps and vsync settings
		/// </summary>
		protected virtual void Start()
		{
			UpdateSettings();
		}	
        
		/// <summary>
		/// When a value gets changed in the editor, we update our settings
		/// </summary>
		protected virtual void OnValidate()
		{
			UpdateSettings();
		}

		/// <summary>
		/// Updates the target frame rate value and vsync count setting
		/// </summary>
		protected virtual void UpdateSettings()
		{
			QualitySettings.vSyncCount = VSyncCount;
			Application.targetFrameRate = TargetFPS;
			OnDemandRendering.renderFrameInterval = RenderFrameInterval;
		}
	}
}