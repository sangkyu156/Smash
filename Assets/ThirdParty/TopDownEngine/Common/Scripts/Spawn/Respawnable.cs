namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 플레이어 부활을 위한 인터페이스
    /// </summary>
    public interface Respawnable
	{
		void OnPlayerRespawn(CheckPoint checkpoint, Character player);
	}
}