using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace MoreMountains.Tools
{	
	[RequireComponent(typeof(CanvasGroup))]
	[AddComponentMenu("More Mountains/Tools/Controls/MMTouchControls")]
	public class MMTouchControls : MonoBehaviour 
	{
		public enum InputForcedMode { None, Mobile, Desktop }
		[MMInformation("자동 모바일 감지를 선택하면 빌드 타겟이 Android 또는 iOS일 때 엔진이 자동으로 모바일 컨트롤로 전환됩니다. 아래 드롭다운을 사용하여 모바일 또는 데스크톱(키보드, 게임 패드) 컨트롤을 강제할 수도 있습니다.\n모바일 컨트롤 및/또는 GUI가 필요하지 않은 경우 이 구성 요소는 자체적으로 작동할 수도 있습니다. 대신 빈 게임 개체에 넣기만 하면 됩니다. ", MMInformationAttribute.InformationType.Info,false)]
        /// 자동 모바일 감지를 선택하면 빌드 타겟이 Android 또는 iOS일 때 엔진이 자동으로 모바일 컨트롤로 전환됩니다.
        /// 아래 드롭다운을 사용하여 모바일 또는 데스크톱(키보드, 게임패드) 제어를 강제할 수도 있습니다. 모바일 제어가 필요하지 않은 경우 참고하세요.
        /// 및/또는 GUI 이 구성 요소는 자체적으로 작동할 수도 있습니다. 대신 빈 GameObject에 배치하면 됩니다.
        [Tooltip("자동 모바일 감지를 선택하면 빌드 타겟이 Android 또는 iOS일 때 엔진이 자동으로 모바일 컨트롤로 전환됩니다." +
"아래 드롭다운을 사용하여 모바일 또는 데스크톱(키보드, 게임패드) 제어를 강제할 수도 있습니다. 모바일 제어가 필요하지 않은 경우 참고하세요." +
"및/또는 GUI 이 구성 요소는 자체적으로 작동할 수도 있으며 대신 빈 GameObject에 배치하기만 하면 됩니다.")]
		public bool AutoMobileDetection = true;
		/// Force desktop mode (gamepad, keyboard...) or mobile (touch controls) 
		[Tooltip("Force desktop mode (gamepad, keyboard...) or mobile (touch controls)")]
		public InputForcedMode ForcedMode;
		public bool IsMobile { get; protected set; }

		protected CanvasGroup _canvasGroup;
		protected float _initialMobileControlsAlpha;

		/// <summary>
		/// We get the player from its tag.
		/// </summary>
		protected virtual void Start()
		{
			_canvasGroup = GetComponent<CanvasGroup>();

			_initialMobileControlsAlpha = _canvasGroup.alpha;
			SetMobileControlsActive(false);
			IsMobile=false;
			if (AutoMobileDetection)
			{
				#if UNITY_ANDROID || UNITY_IPHONE
					SetMobileControlsActive(true);
					IsMobile = true;
				#endif
			}
			if (ForcedMode==InputForcedMode.Mobile)
			{
				SetMobileControlsActive(true);
				IsMobile = true;
			}
			if (ForcedMode==InputForcedMode.Desktop)
			{
				SetMobileControlsActive(false);
				IsMobile = false;		
			}
		}
		
		/// <summary>
		/// Use this method to enable or disable mobile controls
		/// </summary>
		/// <param name="state"></param>
		public virtual void SetMobileControlsActive(bool state)
		{
			if (_canvasGroup!=null)
			{
				_canvasGroup.gameObject.SetActive(state);
				if (state)
				{
					_canvasGroup.alpha=_initialMobileControlsAlpha;
				}
				else
				{
					_canvasGroup.alpha=0;
				}
			}
		}
	}
}