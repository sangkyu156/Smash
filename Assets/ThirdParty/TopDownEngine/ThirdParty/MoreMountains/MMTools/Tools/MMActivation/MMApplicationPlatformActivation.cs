using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 바인딩을 저장하는 데 사용되는 클래스
    /// </summary>
    [Serializable]
	public class PlatformBindings
	{
		public enum PlatformActions { DoNothing, Disable }
		public RuntimePlatform Platform = RuntimePlatform.WindowsPlayer;
		public PlatformActions PlatformAction = PlatformActions.DoNothing;
	}

    /// <summary>
    /// 이 클래스를 게임 개체에 추가하면 플랫폼을 감지하기 위해 Application.platform을 사용하여 플랫폼 컨텍스트에 따라 활성화/비활성화됩니다.
    /// </summary>
    [AddComponentMenu("More Mountains/Tools/Activation/MMApplicationPlatformActivation")]
	public class MMApplicationPlatformActivation : MonoBehaviour
	{
		/// the possible times at which this script can run
		public enum ExecutionTimes { Awake, Start, OnEnable }
        
		[Header("Settings")]
		/// the selected execution time
		public ExecutionTimes ExecutionTime = ExecutionTimes.Awake;
		/// whether or not this should output a debug line in the console
		public bool DebugToTheConsole = false;

		[Header("Platforms")]
		public List<PlatformBindings> Platforms;

		/// <summary>
		/// On Enable, processes the state if needed
		/// </summary>
		protected virtual void OnEnable()
		{
			if (ExecutionTime == ExecutionTimes.OnEnable)
			{
				Process();
			}
		}

		/// <summary>
		/// On Awake, processes the state if needed
		/// </summary>
		protected virtual void Awake()
		{
			if (ExecutionTime == ExecutionTimes.Awake)
			{
				Process();
			}            
		}

		/// <summary>
		/// On Start, processes the state if needed
		/// </summary>
		protected virtual void Start()
		{
			if (ExecutionTime == ExecutionTimes.Start)
			{
				Process();
			}            
		}

		/// <summary>
		/// Enables or disables the object based on current platform
		/// </summary>
		protected virtual void Process()
		{
			foreach (PlatformBindings platform in Platforms)
			{
				if (platform.Platform == Application.platform)
				{
					DisableIfNeeded(platform.PlatformAction, platform.Platform.ToString());
				}
			}

			if (Application.platform == RuntimePlatform.Android)
			{

			}
		}

		/// <summary>
		/// Disables the object if needed, and outputs a debug log if requested
		/// </summary>
		/// <param name="platform"></param>
		/// <param name="platformName"></param>
		protected virtual void DisableIfNeeded(PlatformBindings.PlatformActions platform, string platformName)
		{
			if (this.gameObject.activeInHierarchy && (platform == PlatformBindings.PlatformActions.Disable))
			{
				this.gameObject.SetActive(false);
				if (DebugToTheConsole)
				{
					Debug.LogFormat(this.gameObject.name + " got disabled via MMPlatformActivation, platform : " + platformName + ".");
				}
			}
		}
	}
}