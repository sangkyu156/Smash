using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 풀 개체에 대한 인터페이스입니다. <br/>
    /// 풀의 객체에 대한 추가 처리가 필요한 경우 클래스에서 이 인터페이스를 구현할 수 있습니다.
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