using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
	/// 대상 레벨에 진입점을 지정하면서 한 레벨에서 다음 레벨로 이동하는 데 사용되는 클래스입니다.
	/// 진입점은 각 레벨의 LevelManager 구성 요소에 정의됩니다. 이는 단순히 목록의 변환일 뿐입니다.
	/// 목록의 인덱스는 진입점의 식별자입니다. 
    /// </summary>
        [AddComponentMenu("TopDown Engine/Spawn/GoToLevelEntryPoint")]
	public class GoToLevelEntryPoint : FinishLevel 
	{
		[Space(10)]
		[Header("Points of Entry")]

        /// 진입점을 사용할지 여부입니다. 그렇지 않으면 다음 레벨로 넘어가게 됩니다.
        [Tooltip("진입점을 사용할지 여부입니다. 그렇지 않으면 다음 레벨로 넘어가게 됩니다.")]
		public bool UseEntryPoints = false;
        /// 다음 레벨로 이동할 진입점 인덱스
        [Tooltip("다음 레벨로 이동할 진입점 인덱스")]
		public int PointOfEntryIndex;
        /// 다음 레벨로 이동할 때 향하는 방향
        [Tooltip("다음 레벨로 이동할 때 향하는 방향")]
		public Character.FacingDirections FacingDirection;

        /// <summary>
        /// 다음 레벨을 로드하고 게임 관리자에 대상 진입점 인덱스를 저장합니다.
        /// </summary>
        public override void GoToNextLevel()
		{
			if (UseEntryPoints)
			{
                PointOfEntryIndex = SetPointOfEntryIndex();

                GameManager.Instance.StorePointsOfEntry(LevelName, PointOfEntryIndex, FacingDirection);
			}
			
			base.GoToNextLevel ();
		}

        //스테이지 클리어 상태확인후 스폰지점인덱스 반환 하는 함수
        int SetPointOfEntryIndex()
        {
            int index = 0;
            bool[] stage = new bool[51];
            stage[0] = DataManager.Instance.datas.stage0;
            stage[1] = DataManager.Instance.datas.stage1;
            stage[2] = DataManager.Instance.datas.stage2;
            stage[3] = DataManager.Instance.datas.stage3;
            stage[4] = DataManager.Instance.datas.stage4;
            stage[5] = DataManager.Instance.datas.stage5;
            stage[6] = DataManager.Instance.datas.stage6;
            stage[7] = DataManager.Instance.datas.stage7;
            stage[8] = DataManager.Instance.datas.stage8;
            stage[9] = DataManager.Instance.datas.stage9;
            stage[10] = DataManager.Instance.datas.stage10;
            stage[11] = DataManager.Instance.datas.stage11;
            stage[12] = DataManager.Instance.datas.stage12;
            stage[13] = DataManager.Instance.datas.stage13;
            stage[14] = DataManager.Instance.datas.stage14;
            stage[15] = DataManager.Instance.datas.stage15;
            stage[16] = DataManager.Instance.datas.stage16;
            stage[17] = DataManager.Instance.datas.stage17;
            stage[18] = DataManager.Instance.datas.stage18;
            stage[19] = DataManager.Instance.datas.stage19;
            stage[20] = DataManager.Instance.datas.stage20;
            stage[21] = DataManager.Instance.datas.stage21;
            stage[22] = DataManager.Instance.datas.stage22;
            stage[23] = DataManager.Instance.datas.stage23;
            stage[24] = DataManager.Instance.datas.stage24;
            stage[25] = DataManager.Instance.datas.stage25;
            stage[26] = DataManager.Instance.datas.stage26;
            stage[27] = DataManager.Instance.datas.stage27;
            stage[28] = DataManager.Instance.datas.stage28;
            stage[29] = DataManager.Instance.datas.stage29;
            stage[30] = DataManager.Instance.datas.stage30;
            stage[31] = DataManager.Instance.datas.stage31;
            stage[32] = DataManager.Instance.datas.stage32;
            stage[33] = DataManager.Instance.datas.stage33;
            stage[34] = DataManager.Instance.datas.stage34;
            stage[35] = DataManager.Instance.datas.stage35;
            stage[36] = DataManager.Instance.datas.stage36;
            stage[37] = DataManager.Instance.datas.stage37;
            stage[38] = DataManager.Instance.datas.stage38;
            stage[39] = DataManager.Instance.datas.stage39;
            stage[40] = DataManager.Instance.datas.stage40;
            stage[41] = DataManager.Instance.datas.stage41;
            stage[42] = DataManager.Instance.datas.stage42;
            stage[43] = DataManager.Instance.datas.stage43;
            stage[44] = DataManager.Instance.datas.stage44;
            stage[45] = DataManager.Instance.datas.stage45;
            stage[46] = DataManager.Instance.datas.stage46;
            stage[47] = DataManager.Instance.datas.stage47;
            stage[48] = DataManager.Instance.datas.stage48;
            stage[49] = DataManager.Instance.datas.stage49;
            stage[50] = DataManager.Instance.datas.stage50;

            for (int i = 1; i <= stage.Length; i++)
            {
                if (stage[i] == true)
                    index = i;
                else
                    return index;
            }

            return index;
        }
    }
}