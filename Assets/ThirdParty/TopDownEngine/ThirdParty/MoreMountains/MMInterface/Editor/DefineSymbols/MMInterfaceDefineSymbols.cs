using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MoreMountains.MMInterface
{
#if UNITY_EDITOR
    /// <summary>
    /// 이 클래스를 사용하면 빌드 설정의 기호 정의 목록에 자동으로 추가될 기호를 코드에서 편집하여 지정할 수 있습니다.
    /// </summary>
    [InitializeOnLoad]
	public class MMInterfaceDefineSymbols
	{
		/// <summary>
		/// A list of all the symbols you want added to the build settings
		/// </summary>
		public static readonly string[] Symbols = new string[] 
		{
			"MOREMOUNTAINS_INTERFACE"
		};

		/// <summary>
		/// As soon as this class has finished compiling, adds the specified define symbols to the build settings
		/// </summary>
		static MMInterfaceDefineSymbols()
		{
			string scriptingDefinesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
			List<string> scriptingDefinesStringList = scriptingDefinesString.Split(';').ToList();
			scriptingDefinesStringList.AddRange(Symbols.Except(scriptingDefinesStringList));
			PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, string.Join(";", scriptingDefinesStringList.ToArray()));
		}
	}
	#endif
}