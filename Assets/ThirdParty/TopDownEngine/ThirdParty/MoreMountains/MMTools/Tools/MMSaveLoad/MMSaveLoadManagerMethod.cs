using UnityEngine;

namespace MoreMountains.Tools
{
    /// <summary>
    /// Awake 또는 요청 시 이 구성 요소는 MMSaveLoadManager에서 SaveLoadMethod를 강제 실행하여 데이터를 파일에 저장하는 방식을 변경합니다.
    /// 이는 MMSaveLoadManager를 사용하는 모든 클래스에 영향을 미칩니다(저장하거나 로드하기 전에 해당 메서드를 변경하지 않는 한).
    /// 방법을 변경하면 기존 데이터 파일이 호환되지 않으므로 해당 파일을 삭제하고 새 파일부터 시작해야 합니다.
    /// </summary>
    public class MMSaveLoadManagerMethod : MonoBehaviour
	{
		[Header("Save and load method")]
		[MMInformation("Awake 또는 요청 시 이 구성 요소는 MMSaveLoadManager에서 SaveLoadMethod를 강제 실행하여 데이터를 파일에 저장하는 방식을 변경합니다. " +
"이것은 MMSaveLoadManager를 사용하는 모든 클래스에 영향을 미칩니다(저장하거나 로드하기 전에 해당 메서드를 변경하지 않는 한)." +
"방법을 변경하면 기존 데이터 파일이 호환되지 않으므로 해당 파일을 삭제하고 새 파일부터 시작해야 합니다.", 
						MMInformationAttribute.InformationType.Info,false)]

		/// the method to use to save to file
		[Tooltip("파일에 저장하는 데 사용하는 방법")]
		public MMSaveLoadManagerMethods SaveLoadMethod = MMSaveLoadManagerMethods.Binary;
		/// the key to use to encrypt the file (if using an encryption method)
		[Tooltip("파일을 암호화하는 데 사용할 키(암호화 방법을 사용하는 경우)")]
		public string EncryptionKey = "ThisIsTheKey";

		protected IMMSaveLoadManagerMethod _saveLoadManagerMethod;

		/// <summary>
		/// On Awake, we set the MMSaveLoadManager's method to the chosen one
		/// </summary>
		protected virtual void Awake()
		{
			SetSaveLoadMethod();
		}
		
		/// <summary>
		/// Creates a new MMSaveLoadManagerMethod and passes it to the MMSaveLoadManager
		/// </summary>
		public virtual void SetSaveLoadMethod()
		{
			switch(SaveLoadMethod)
			{
				case MMSaveLoadManagerMethods.Binary:
					_saveLoadManagerMethod = new MMSaveLoadManagerMethodBinary();
					break;
				case MMSaveLoadManagerMethods.BinaryEncrypted:
					_saveLoadManagerMethod = new MMSaveLoadManagerMethodBinaryEncrypted();
					((MMSaveLoadManagerEncrypter)_saveLoadManagerMethod).Key = EncryptionKey;
					break;
				case MMSaveLoadManagerMethods.Json:
					_saveLoadManagerMethod = new MMSaveLoadManagerMethodJson();
					break;
				case MMSaveLoadManagerMethods.JsonEncrypted:
					_saveLoadManagerMethod = new MMSaveLoadManagerMethodJsonEncrypted();
					((MMSaveLoadManagerEncrypter)_saveLoadManagerMethod).Key = EncryptionKey;
					break;
			}
			MMSaveLoadManager.SaveLoadMethod = _saveLoadManagerMethod;
		}
	}    
}

