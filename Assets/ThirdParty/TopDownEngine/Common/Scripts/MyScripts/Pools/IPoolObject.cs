using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// Ǯ ��ü�� ���� �������̽��Դϴ�. <br/>
    /// Ǯ�� ��ü�� ���� �߰� ó���� �ʿ��� ��� Ŭ�������� �� �������̽��� ������ �� �ֽ��ϴ�.
    /// </summary>
    public interface IPoolObject
    {
        /// <summary>
        /// Called by pool when object instantiated.
        /// </summary>
        void OnCreatedInPool();

        /// <summary>
        /// Called by pool before gets you pool's object. Useful for state resetting.
        /// </summary>
        void OnGettingFromPool();
    }
}