namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// �÷��̾� ��Ȱ�� ���� �������̽�
    /// </summary>
    public interface Respawnable
	{
		void OnPlayerRespawn(CheckPoint checkpoint, Character player);
	}
}