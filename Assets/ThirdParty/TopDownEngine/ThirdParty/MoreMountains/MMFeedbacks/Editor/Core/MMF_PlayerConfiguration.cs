using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 사본 정보와 글로벌 피드백 설정을 저장하는 자산입니다.
    /// 단 하나의 MMFeedbacksConfiguration 자산을 생성하고 Resources 폴더에 저장해야 합니다.
    /// MMFeedbacks를 설치할 때 이미 완료되었습니다.
    /// </summary>
    [CreateAssetMenu(menuName = "MoreMountains/MMFeedbacks/Configuration", fileName = "MMFeedbacksConfiguration")]
	public class MMF_PlayerConfiguration : ScriptableObject
	{
		private static MMF_PlayerConfiguration _instance;
		private static bool _instantiated;
        
		/// <summary>
		/// Singleton pattern
		/// </summary>
		public static MMF_PlayerConfiguration Instance
		{
			get
			{
				if (_instantiated)
				{
					return _instance;
				}
                
				string assetName = typeof(MMF_PlayerConfiguration).Name;
                
				MMF_PlayerConfiguration loadedAsset = Resources.Load<MMF_PlayerConfiguration>("MMF_PlayerConfiguration");
				_instance = loadedAsset;    
				_instantiated = true;
                
				return _instance;
			}
		}
        
		[Header("Help settings")]
		/// if this is true, inspector tips will be shown for MMFeedbacks
		public bool ShowInspectorTips = true;
		/// if this is true, when exiting play mode when KeepPlaymodeChanges is active, it'll turn off automatically, otherwise it'll remain on
		public bool AutoDisableKeepPlaymodeChanges = true;
		/// if this is true, when exiting play mode when KeepPlaymodeChanges is active, it'll turn off automatically, otherwise it'll remain on
		public bool InspectorGroupsExpandedByDefault = true;


        
		private void OnDestroy(){ _instantiated = false; }
	}    
}