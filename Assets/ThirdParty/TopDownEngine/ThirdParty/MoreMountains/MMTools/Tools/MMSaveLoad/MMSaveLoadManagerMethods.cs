using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 다양한 방법(바이너리, json 등)을 사용하여 저장 및 로드를 구현하는 인터페이스
    /// </summary>
    public interface IMMSaveLoadManagerMethod
	{
		void Save(object objectToSave, FileStream saveFile);
		object Load(System.Type objectType, FileStream saveFile);
	}

    /// <summary>
    /// MMSaveLoadManager에서 사용할 수 있는 디스크에 파일을 저장하고 로드하는 가능한 방법
    /// </summary>
    public enum MMSaveLoadManagerMethods { Json, JsonEncrypted, Binary, BinaryEncrypted };

    /// <summary>
    /// 이 클래스는 스트림을 암호화하고 해독하는 메서드를 구현합니다.
    /// </summary>
    public abstract class MMSaveLoadManagerEncrypter
	{
		/// <summary>
		/// The Key to use to save and load the file
		/// </summary>
		public string Key { get; set; } = "yourDefaultKey";

		protected string _saltText = "SaltTextGoesHere";

		/// <summary>
		/// Encrypts the specified input stream into the specified output stream using the key passed in parameters
		/// </summary>
		/// <param name="inputStream"></param>
		/// <param name="outputStream"></param>
		/// <param name="sKey"></param>
		protected virtual void Encrypt(Stream inputStream, Stream outputStream, string sKey)
		{
			RijndaelManaged algorithm = new RijndaelManaged();
			Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(sKey, Encoding.ASCII.GetBytes(_saltText));

			algorithm.Key = key.GetBytes(algorithm.KeySize / 8);
			algorithm.IV = key.GetBytes(algorithm.BlockSize / 8);

			CryptoStream cryptostream = new CryptoStream(inputStream, algorithm.CreateEncryptor(), CryptoStreamMode.Read);
			cryptostream.CopyTo(outputStream);
		}

		/// <summary>
		/// Decrypts the input stream into the output stream using the key passed in parameters
		/// </summary>
		/// <param name="inputStream"></param>
		/// <param name="outputStream"></param>
		/// <param name="sKey"></param>
		protected virtual void Decrypt(Stream inputStream, Stream outputStream, string sKey)
		{
			RijndaelManaged algorithm = new RijndaelManaged();
			Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(sKey, Encoding.ASCII.GetBytes(_saltText));

			algorithm.Key = key.GetBytes(algorithm.KeySize / 8);
			algorithm.IV = key.GetBytes(algorithm.BlockSize / 8);

			CryptoStream cryptostream = new CryptoStream(inputStream, algorithm.CreateDecryptor(), CryptoStreamMode.Read);
			cryptostream.CopyTo(outputStream);
		}
	}
}