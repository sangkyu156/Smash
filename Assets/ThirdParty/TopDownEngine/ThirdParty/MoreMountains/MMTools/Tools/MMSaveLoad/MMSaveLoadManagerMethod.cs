using UnityEngine;

namespace MoreMountains.Tools
{
    /// <summary>
    /// Awake �Ǵ� ��û �� �� ���� ��Ҵ� MMSaveLoadManager���� SaveLoadMethod�� ���� �����Ͽ� �����͸� ���Ͽ� �����ϴ� ����� �����մϴ�.
    /// �̴� MMSaveLoadManager�� ����ϴ� ��� Ŭ������ ������ ��Ĩ�ϴ�(�����ϰų� �ε��ϱ� ���� �ش� �޼��带 �������� �ʴ� ��).
    /// ����� �����ϸ� ���� ������ ������ ȣȯ���� �����Ƿ� �ش� ������ �����ϰ� �� ���Ϻ��� �����ؾ� �մϴ�.
    /// </summary>
    public class MMSaveLoadManagerMethod : MonoBehaviour
	{
		[Header("Save and load method")]
		[MMInformation("Awake �Ǵ� ��û �� �� ���� ��Ҵ� MMSaveLoadManager���� SaveLoadMethod�� ���� �����Ͽ� �����͸� ���Ͽ� �����ϴ� ����� �����մϴ�. " +
"�̰��� MMSaveLoadManager�� ����ϴ� ��� Ŭ������ ������ ��Ĩ�ϴ�(�����ϰų� �ε��ϱ� ���� �ش� �޼��带 �������� �ʴ� ��)." +
"����� �����ϸ� ���� ������ ������ ȣȯ���� �����Ƿ� �ش� ������ �����ϰ� �� ���Ϻ��� �����ؾ� �մϴ�.", 
						MMInformationAttribute.InformationType.Info,false)]

		/// the method to use to save to file
		[Tooltip("���Ͽ� �����ϴ� �� ����ϴ� ���")]
		public MMSaveLoadManagerMethods SaveLoadMethod = MMSaveLoadManagerMethods.Binary;
		/// the key to use to encrypt the file (if using an encryption method)
		[Tooltip("������ ��ȣȭ�ϴ� �� ����� Ű(��ȣȭ ����� ����ϴ� ���)")]
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

