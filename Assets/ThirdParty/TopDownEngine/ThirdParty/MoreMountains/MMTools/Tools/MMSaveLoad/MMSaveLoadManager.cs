using UnityEngine;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MoreMountains.Tools
{
    /// <summary>
    /// 특정 폴더 및 파일에 개체를 저장하고 로드할 수 있습니다.
    /// 
    /// 사용방법(최소) :
    /// 
    /// Save : MMSaveLoadManager.Save(TestObject, FileName+SaveFileExtension, FolderName);
    /// 
    /// Load : TestObject = (YourObjectClass)MMSaveLoadManager.Load(typeof(YourObjectClass), FileName + SaveFileExtension, FolderName);
    /// 
    /// Delete save : MMSaveLoadManager.DeleteSave(FileName+SaveFileExtension, FolderName);
    /// 
    /// Delete save folder : MMSaveLoadManager.DeleteSaveFolder(FolderName);
    /// 
    /// 시스템이 사용해야 하는 IMMSaveLoadManagerMethod를 지정할 수도 있습니다. 기본적으로 바이너리이지만 암호화된 바이너리, json 또는 json 암호화를 선택할 수도 있습니다. 
    /// MMSaveLoadTester 클래스에서 이들 각각을 설정하는 방법에 대한 예를 찾을 수 있습니다.
    /// 
    /// </summary>
    public static class MMSaveLoadManager
	{
        /// 파일을 저장하고 불러올 때 사용하는 방법(물론 두 번 모두 동일해야 함)
        public static IMMSaveLoadManagerMethod SaveLoadMethod = new MMSaveLoadManagerMethodBinary();
        /// 시스템이 파일을 저장하는 데 사용할 기본 최상위 폴더
        private const string _baseFolderName = "/MMData/";
        /// 아무것도 제공되지 않은 경우 저장 폴더의 이름
        private const string _defaultFolderName = "MMSaveLoadManager";

        /// <summary>
        /// 폴더 이름을 기준으로 파일을 로드하고 저장할 때 사용할 저장 경로를 결정합니다.
        /// </summary>
        /// <returns>저장 경로.</returns>
        /// <param name="folderName">Folder name.</param>
        public static string DetermineSavePath(string folderName = _defaultFolderName)
		{
			string savePath;
            // 사용 중인 장치에 따라 경로를 조합합니다.
            if (Application.platform == RuntimePlatform.IPhonePlayer) 
			{
				savePath = Application.persistentDataPath + _baseFolderName;
			} 
			else 
			{
				savePath = Application.persistentDataPath + _baseFolderName;
			}
			#if UNITY_EDITOR
			savePath = Application.dataPath + _baseFolderName;
			#endif

			savePath = savePath + folderName + "/";
			return savePath;
		}

        /// <summary>
        /// 저장할 파일의 이름을 결정합니다
        /// </summary>
        /// <returns>The save file name.</returns>
        /// <param name="fileName">File name.</param>
        static string DetermineSaveFileName(string fileName)
		{
			return fileName;
		}

        /// <summary>
        /// 지정된 saveObject, fileName 및 폴더 이름을 디스크의 파일에 저장합니다.
        /// </summary>
        /// <param name="saveObject">Save object.</param>
        /// <param name="fileName">File name.</param>
        /// <param name="foldername">Foldername.</param>
        public static void Save(object saveObject, string fileName, string foldername = _defaultFolderName)
		{
			string savePath = DetermineSavePath(foldername);
			string saveFileName = DetermineSaveFileName(fileName);

            // 디렉토리가 아직 존재하지 않으면 디렉토리를 생성합니다.
            if (!Directory.Exists(savePath))
			{
				Directory.CreateDirectory(savePath);
            }

            // 객체를 직렬화하고 디스크의 파일에 씁니다.
            FileStream saveFile = File.Create(savePath + saveFileName);

			SaveLoadMethod.Save(saveObject, saveFile);
            Debug.Log($"세이브경로 = {savePath}{saveFileName}");
            saveFile.Close();
		}

        /// <summary>
        /// 파일 이름을 기준으로 지정된 파일을 지정된 폴더에 로드합니다.
        /// </summary>
        /// <param name="fileName">File name.</param>
        /// <param name="foldername">Foldername.</param>
        public static object Load(System.Type objectType, string fileName, string foldername = _defaultFolderName)
		{
			string savePath = DetermineSavePath(foldername);
			string saveFileName = savePath + DetermineSaveFileName(fileName);

			object returnObject;

            // MMSaves 디렉터리나 저장 파일이 존재하지 않으면 로드할 것이 없으므로 아무것도 하지 않고 종료합니다.
            if (!Directory.Exists(savePath) || !File.Exists(saveFileName))
			{
                return null;
			}

			FileStream saveFile = File.Open(saveFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
			returnObject = SaveLoadMethod.Load(objectType, saveFile);
			Debug.Log($"로드경로 = {saveFileName}");
			saveFile.Close();

			return returnObject;
		}

        /// <summary>
        /// 디스크에서 저장 내용을 제거합니다.
        /// </summary>
        /// <param name="fileName">File name.</param>
        /// <param name="folderName">Folder name.</param>
        public static void DeleteSave(string fileName, string folderName = _defaultFolderName)
		{
			string savePath = DetermineSavePath(folderName);
			string saveFileName = DetermineSaveFileName(fileName);
			if (File.Exists(savePath + saveFileName))
			{
				File.Delete(savePath + saveFileName);
			}	
			if (File.Exists(savePath + saveFileName + ".meta"))
			{
				File.Delete(savePath + saveFileName + ".meta");
			}			
		}

        /// <summary>
        /// 저장 폴더 전체를 삭제합니다.
        /// </summary>
        /// <param name="folderName"></param>
        public static void DeleteSaveFolder(string folderName = _defaultFolderName)
		{
			string savePath = DetermineSavePath(folderName);
			if (Directory.Exists(savePath))
			{
				DeleteDirectory(savePath);
			}
		}

        /// <summary>
        /// 이 MMSaveLoadManager에 의해 저장된 모든 저장 파일을 삭제합니다.
        /// </summary>
        public static void DeleteAllSaveFiles()
		{
			string savePath = DetermineSavePath("");

			savePath = savePath.Substring(0, savePath.Length - 1);
			if (savePath.EndsWith("/"))
			{
				savePath = savePath.Substring(0, savePath.Length - 1);
			}

			if (Directory.Exists(savePath))
			{
				DeleteDirectory(savePath);
			}
		}

        /// <summary>
        /// 지정된 디렉터리를 삭제합니다.
        /// </summary>
        /// <param name="target_dir"></param>
        public static void DeleteDirectory(string target_dir)
		{
			string[] files = Directory.GetFiles(target_dir);
			string[] dirs = Directory.GetDirectories(target_dir);

			foreach (string file in files)
			{
				File.SetAttributes(file, FileAttributes.Normal);
				File.Delete(file);
			}

			foreach (string dir in dirs)
			{
				DeleteDirectory(dir);
			}

			Directory.Delete(target_dir, false);

			if (File.Exists(target_dir + ".meta"))
			{
				File.Delete(target_dir + ".meta");
			}
		}
	}
}