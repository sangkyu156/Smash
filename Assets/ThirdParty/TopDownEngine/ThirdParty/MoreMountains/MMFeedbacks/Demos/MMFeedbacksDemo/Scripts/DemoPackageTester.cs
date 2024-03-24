using MoreMountains.Tools;
using UnityEngine;
using UnityEditor;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// MMFeedbacks 데모에 사용하기 위한 이 클래스는 요구 사항을 확인하고
    /// 필요한 경우 오류 메시지.
    /// </summary>
    public class DemoPackageTester : MonoBehaviour
	{
		[MMFInformation("이 구성 요소는 이 데모에 대한 종속성이 설치되지 않은 경우 콘솔에 오류를 표시하는 데만 사용됩니다. 원하는 경우 안전하게 제거할 수 있으며 일반적으로 자신의 게임에 해당 항목을 유지하고 싶지 않을 것입니다.", MMFInformationAttribute.InformationType.Warning, false)]
		/// does the scene require post processing to be installed?
		public bool RequiresPostProcessing;
		/// does the scene require TextMesh Pro to be installed?
		public bool RequiresTMP;
		/// does the scene require Cinemachine to be installed?
		public bool RequiresCinemachine;

		/// <summary>
		/// On Awake we test for dependencies
		/// </summary>
		protected virtual void Awake()
		{
			#if  UNITY_EDITOR
			if (Application.isPlaying)
			{
				TestForDependencies();    
			}
			#endif
		}

		/// <summary>
		/// Checks whether or not dependencies have been correctly installed
		/// </summary>
		protected virtual void TestForDependencies()
		{
			bool missingDependencies = false;
			string missingString = "";
			bool cinemachineFound = false;
			bool tmpFound = false;
			bool postProcessingFound = false;
            
			#if MM_CINEMACHINE
			cinemachineFound = true;
			#endif
                        
			#if MM_TEXTMESHPRO
			tmpFound = true;
			#endif
                        
			#if MM_POSTPROCESSING
			postProcessingFound = true;
			#endif

			if (missingDependencies)
			{
				// we do nothing but without that we get an annoying warning so here we are.
			}

			if (RequiresCinemachine && !cinemachineFound)
			{
				missingDependencies = true;
				missingString += "Cinemachine";
			}

			if (RequiresTMP && !tmpFound)
			{
				missingDependencies = true;
				if (missingString != "") { missingString += ", "; }
				missingString += "TextMeshPro";
			}
            
			if (RequiresPostProcessing && !postProcessingFound)
			{
				missingDependencies = true;
				if (missingString != "") { missingString += ", "; }
				missingString += "PostProcessing";
			}
            
			#if UNITY_EDITOR
			if (missingDependencies)
			{
				Debug.LogError("[DemoPackageTester] It looks like you're missing some dependencies required by this demo ("+missingString+")." +
				               " You'll have to install them to run this demo. You can learn more about how to do so in the documentation, at http://feel-docs.moremountains.com/how-to-install.html" +
				               "\n\n");
                
				if (EditorUtility.DisplayDialog("Missing dependencies!",
					    "This demo requires a few dependencies to be installed first (Cinemachine, TextMesh Pro, PostProcessing).\n\n" +
					    "You can use Feel without them of course, but this demo needs them to work (check out the documentation to learn more!).\n\n" +
					    "Would you like to automatically install them?", "Yes, install dependencies", "No"))
				{
					MMFDependencyInstaller.InstallFromPlay();
				}
			}
			#endif
		}
	}    
}