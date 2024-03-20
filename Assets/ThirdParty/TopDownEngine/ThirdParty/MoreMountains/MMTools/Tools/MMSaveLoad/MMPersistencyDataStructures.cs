using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace MoreMountains.Tools
{
    /// <summary>
    /// ��� �����͸� �����ϴ� �� ���Ǵ� ����ȭ ���� Ŭ����, Ű�� ���ڿ�(��� �̸�), ���� MMPersistencySceneData
    /// </summary>
    [Serializable]
	public class DictionaryStringSceneData : MMSerializableDictionary<string, MMPersistenceSceneData>
	{
		public DictionaryStringSceneData() : base() { }
		protected DictionaryStringSceneData(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}

    /// <summary>
    /// ��ü �����͸� �����ϴ� �� ���Ǵ� ����ȭ ���� Ŭ����. Ű�� ���ڿ�(��ü �̸�), ���� ���ڿ�(��ü ������)
    /// </summary>
    [Serializable]
	public class DictionaryStringString : MMSerializableDictionary<string, string>
	{
		public DictionaryStringString() : base() { }
		protected DictionaryStringString(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}

    /// <summary>
    /// ��� ������ ������ ���Ӽ� �������� ��� �����͸� �����ϴ� �� ���Ǵ� ����ȭ ���� Ŭ����
    /// </summary>
    [Serializable]
	public class MMPersistenceManagerData
	{
		public string PersistenceID;
		public string SaveDate;
		public DictionaryStringSceneData SceneDatas;
	}

    /// <summary>
    /// ����� ��� ������, ��ü ������ ������ �����ϴ� �� ���Ǵ� ����ȭ ���� Ŭ����
    /// </summary>
    [Serializable]
	public class MMPersistenceSceneData
	{
		public DictionaryStringString ObjectDatas;
	}

    /// <summary>
    /// MMPersistencyManager�� ���� Ʈ���ŵ� �� �ִ� �پ��� ������ ���Ӽ� �̺�Ʈ
    /// </summary>
    public enum MMPersistenceEventType { DataSavedToMemory, DataLoadedFromMemory, DataSavedFromMemoryToFile, DataLoadedFromFileToMemory }

    /// <summary>
    /// ���Ӽ� �̺�Ʈ �����͸� �����ϴ� �� ���Ǵ� ������ �����Դϴ�.
    /// ��� :
    /// MMPersistencyEvent.Trigger(MMPersistencyEventType.DataLoadedFromFileToMemory, "yourPersistencyID");
    /// </summary>
    public struct MMPersistenceEvent
	{
		public MMPersistenceEventType PersistenceEventType;
		public string PersistenceID;

		public MMPersistenceEvent(MMPersistenceEventType eventType, string persistenceID)
		{
			PersistenceEventType = eventType;
			PersistenceID = persistenceID;
		}

		static MMPersistenceEvent e;
		public static void Trigger(MMPersistenceEventType eventType, string persistencyID)
		{
			e.PersistenceEventType = eventType;
			e.PersistenceID = persistencyID;
		}
	}
}
