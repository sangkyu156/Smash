using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text;
using System.Linq;
using System;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 메뉴 항목을 통해 프로젝트에서 모든 빈 디렉토리를 제거하는 유지 관리 클래스
    /// </summary>
    public class MMCleanEmptyFolders : MonoBehaviour
	{
		static string _consoleLog = "";
		static List<DirectoryInfo> _listOfEmptyDirectories = new List<DirectoryInfo>();

		/// <summary>
		/// Parses the project for empty directories and removes them, as well as their associated meta file
		/// </summary>
		[MenuItem("Tools/More Mountains/Cleanup empty folders", false, 504)]
		protected static void CleanupMissingScripts()
		{
			_listOfEmptyDirectories.Clear();
			var assetsDir = Application.dataPath + Path.DirectorySeparatorChar;
			GetEmptyDirectories(new DirectoryInfo(assetsDir), _listOfEmptyDirectories);

			if (0 < _listOfEmptyDirectories.Count)
			{
				_consoleLog = "[MMCleanEmptyFolders] Removed "+ _listOfEmptyDirectories.Count + " empty directories:\n";
				foreach (var d in _listOfEmptyDirectories)
				{
					_consoleLog += "· "+ d.FullName.Replace(assetsDir, "") + "\n";
					FileUtil.DeleteFileOrDirectory(d.FullName);
					FileUtil.DeleteFileOrDirectory(d.FullName+".meta");
				}

				Debug.Log(_consoleLog);
				_consoleLog = "";

				AssetDatabase.Refresh();
			}
		}

		/// <summary>
		/// Returns true if a directory is empty and updates a list of empty directories
		/// </summary>
		/// <param name="directory"></param>
		/// <param name="listOfEmptyDirectories"></param>
		/// <returns></returns>
		static bool GetEmptyDirectories(DirectoryInfo directory, List<DirectoryInfo> listOfEmptyDirectories)
		{
			bool directoryIsEmpty = true;
			directoryIsEmpty = (directory.GetDirectories().Count(x => !GetEmptyDirectories(x, listOfEmptyDirectories)) == 0) && (directory.GetFiles("*.*").All(x => x.Extension == ".meta"));

			if (directoryIsEmpty)
			{
				listOfEmptyDirectories.Add(directory);
			}      
            
			return directoryIsEmpty;
		}
	}
}