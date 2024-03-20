using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 장면 데이터를 저장하는 데 사용되는 직렬화 가능 클래스, 키는 문자열(장면 이름), 값은 MMPersistencySceneData
    /// </summary>
    [Serializable]
	public class DictionaryStringSceneData : MMSerializableDictionary<string, MMPersistenceSceneData>
	{
		public DictionaryStringSceneData() : base() { }
		protected DictionaryStringSceneData(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}

    /// <summary>
    /// 객체 데이터를 저장하는 데 사용되는 직렬화 가능 클래스. 키는 문자열(객체 이름), 값은 문자열(객체 데이터)
    /// </summary>
    [Serializable]
	public class DictionaryStringString : MMSerializableDictionary<string, string>
	{
		public DictionaryStringString() : base() { }
		protected DictionaryStringString(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}

    /// <summary>
    /// 장면 데이터 모음인 지속성 관리자의 모든 데이터를 저장하는 데 사용되는 직렬화 가능 클래스
    /// </summary>
    [Serializable]
	public class MMPersistenceManagerData
	{
		public string PersistenceID;
		public string SaveDate;
		public DictionaryStringSceneData SceneDatas;
	}

    /// <summary>
    /// 장면의 모든 데이터, 객체 데이터 모음을 저장하는 데 사용되는 직렬화 가능 클래스
    /// </summary>
    [Serializable]
	public class MMPersistenceSceneData
	{
		public DictionaryStringString ObjectDatas;
	}

    /// <summary>
    /// MMPersistencyManager에 의해 트리거될 수 있는 다양한 유형의 지속성 이벤트
    /// </summary>
    public enum MMPersistenceEventType { DataSavedToMemory, DataLoadedFromMemory, DataSavedFromMemoryToFile, DataLoadedFromFileToMemory }

    /// <summary>
    /// 지속성 이벤트 데이터를 저장하는 데 사용되는 데이터 구조입니다.
    /// 사용 :
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
