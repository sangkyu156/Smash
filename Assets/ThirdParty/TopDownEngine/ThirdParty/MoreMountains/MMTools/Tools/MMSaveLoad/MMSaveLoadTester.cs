using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MoreMountains.Tools
{
    /// <summary>
    /// MMSaveLoadManager 클래스를 테스트하기 위해 데이터를 저장하는 테스트 개체
    /// </summary>
    [System.Serializable]
	public class MMSaveLoadTestObject
	{
		public string SavedText;
	}

    /// <summary>
    /// MMSaveLoadManager 클래스를 테스트하기 위해 MMSaveLoadTestScene에서 사용되는 간단한 클래스
    /// </summary>
    public class MMSaveLoadTester : MonoBehaviour
	{
		[Header("Bindings")]
		/// the text to save
		[Tooltip("저장할 텍스트")]
		public InputField TargetInputField;

		[Header("Save settings")]
		/// the chosen save method (json, encrypted json, binary, encrypted binary)
		[Tooltip("선택한 저장 방법(json, 암호화된 json, 바이너리, 암호화된 바이너리)")]
		public MMSaveLoadManagerMethods SaveLoadMethod = MMSaveLoadManagerMethods.Binary;
		/// the name of the file to save
		[Tooltip("저장할 파일 이름")]
		public string FileName = "TestObject";
		/// the name of the destination folder
		[Tooltip("대상 폴더의 이름")]
		public string FolderName = "MMTest/";
		/// the extension to use
		[Tooltip("사용할 확장")]
		public string SaveFileExtension = ".testObject";
		/// the key to use to encrypt the file (if needed)
		[Tooltip("파일을 암호화하는 데 사용할 키(필요한 경우)")]
		public string EncryptionKey = "ThisIsTheKey";

		/// Test button
		[MMInspectorButton("Save")]
		public bool TestSaveButton;
		/// Test button
		[MMInspectorButton("Load")]
		public bool TestLoadButton;
		/// Test button
		[MMInspectorButton("Reset")]
		public bool TestResetButton;

		protected IMMSaveLoadManagerMethod _saveLoadManagerMethod;

		/// <summary>
		/// Saves the contents of the TestObject into a file
		/// </summary>
		public virtual void Save()
		{
			InitializeSaveLoadMethod();
			MMSaveLoadTestObject testObject = new MMSaveLoadTestObject();
			testObject.SavedText = TargetInputField.text;
			MMSaveLoadManager.Save(testObject, FileName+SaveFileExtension, FolderName);
		}

		/// <summary>
		/// Loads the saved data
		/// </summary>
		public virtual void Load()
		{
			InitializeSaveLoadMethod();
			MMSaveLoadTestObject testObject = (MMSaveLoadTestObject)MMSaveLoadManager.Load(typeof(MMSaveLoadTestObject), FileName + SaveFileExtension, FolderName);
			TargetInputField.text = testObject.SavedText;
		}

		/// <summary>
		/// Resets all saves by deleting the whole folder
		/// </summary>
		protected virtual void Reset()
		{
			MMSaveLoadManager.DeleteSaveFolder(FolderName);
		}

		/// <summary>
		/// Creates a new MMSaveLoadManagerMethod and passes it to the MMSaveLoadManager
		/// </summary>
		protected virtual void InitializeSaveLoadMethod()
		{
			switch(SaveLoadMethod)
			{
				case MMSaveLoadManagerMethods.Binary:
					_saveLoadManagerMethod = new MMSaveLoadManagerMethodBinary();
					break;
				case MMSaveLoadManagerMethods.BinaryEncrypted:
					_saveLoadManagerMethod = new MMSaveLoadManagerMethodBinaryEncrypted();
					(_saveLoadManagerMethod as MMSaveLoadManagerEncrypter).Key = EncryptionKey;
					break;
				case MMSaveLoadManagerMethods.Json:
					_saveLoadManagerMethod = new MMSaveLoadManagerMethodJson();
					break;
				case MMSaveLoadManagerMethods.JsonEncrypted:
					_saveLoadManagerMethod = new MMSaveLoadManagerMethodJsonEncrypted();
					(_saveLoadManagerMethod as MMSaveLoadManagerEncrypter).Key = EncryptionKey;
					break;
			}
			MMSaveLoadManager.SaveLoadMethod = _saveLoadManagerMethod;
		}
	}
}