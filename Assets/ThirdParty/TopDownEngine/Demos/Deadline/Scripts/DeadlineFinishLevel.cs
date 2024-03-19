using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 다음 레벨을 로드할 Deadline 데모 전용 클래스
    /// </summary>
    public class DeadlineFinishLevel : FinishLevel 
	{
		/// <summary>
		/// Loads the next level
		/// </summary>
		public override void GoToNextLevel()
		{
			TopDownEngineEvent.Trigger(TopDownEngineEventTypes.LevelComplete, null);
			MMGameEvent.Trigger("Save");
			LevelManager.Instance.GotoLevel (LevelName);
		}	
	}
}