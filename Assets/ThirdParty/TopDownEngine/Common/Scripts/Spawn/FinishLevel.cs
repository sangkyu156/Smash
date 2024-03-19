using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// �� Ŭ������ Ʈ���ſ� �߰��ϸ� �÷��̾ ���� ������ �̵��մϴ�.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Spawn/Finish Level")]
	public class FinishLevel : ButtonActivated
	{
		[Header("Finish Level")]
        /// ��ȯ�� ������ ��Ȯ�� �̸�
        [Tooltip("��ȯ�� ������ ��Ȯ�� �̸�")]
		public string LevelName;

        /// <summary>
        /// ��ư�� ������ ��ȭ�� ���۵˴ϴ�
        /// </summary>
        public override void TriggerButtonAction()
		{
			if (!CheckNumberOfUses())
			{
				return;
			}
			base.TriggerButtonAction ();
			GoToNextLevel();
		}

        /// <summary>
        /// ���� ������ �ε��մϴ�.
        /// </summary>
        public virtual void GoToNextLevel()
		{
			if (LevelManager.HasInstance)
			{
				LevelManager.Instance.GotoLevel(LevelName);
			}
			else
			{
				MMSceneLoadingManager.LoadScene(LevelName);
			}
		}
	}
}