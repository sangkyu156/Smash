using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MoreMountains.TopDownEngine
{
#if UNITY_EDITOR
    /// <summary>
    /// 이 클래스를 사용하면 빌드 설정의 기호 정의 목록에 자동으로 추가될 기호를 코드에서 편집하여 지정할 수 있습니다.
    /// </summary>
    [InitializeOnLoad]
	public class TopDownEngineDefineSymbols
	{
        /// <summary>
        /// 빌드 설정에 추가하려는 모든 기호 목록
        /// </summary>
        public static readonly string[] Symbols = new string[]
		{
			"MOREMOUNTAINS_TOPDOWNENGINE"
		};

        /// <summary>
        /// 이 클래스의 컴파일이 완료되자마자 지정된 정의 기호를 빌드 설정에 추가합니다.
        /// </summary>
        static TopDownEngineDefineSymbols()
		{
			string scriptingDefinesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
			List<string> scriptingDefinesStringList = scriptingDefinesString.Split(';').ToList();
			scriptingDefinesStringList.AddRange(Symbols.Except(scriptingDefinesStringList));
			PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, string.Join(";", scriptingDefinesStringList.ToArray()));
		}
	}
	#endif
}