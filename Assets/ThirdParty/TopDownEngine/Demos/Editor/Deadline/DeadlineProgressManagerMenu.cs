using UnityEditor;

namespace MoreMountains.TopDownEngine
{	
	public static class DeadlineProgressManagerMenu 
	{
		[MenuItem("Tools/More Mountains/Reset all Deadline progress",false,21)]
        /// <summary>
        /// 마감일 데모의 모든 진행 상황을 재설정하는 메뉴 항목을 추가합니다.
        /// </summary>
        private static void ResetProgress()
		{
			DeadlineProgressManager.Instance.ResetProgress ();
		}
	}
}