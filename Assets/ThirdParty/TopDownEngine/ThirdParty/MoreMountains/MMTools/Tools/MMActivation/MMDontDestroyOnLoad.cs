using UnityEngine;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 이 구성 요소를 개체에 추가하면 장면 전체에 걸쳐 지속됩니다. 
    /// </summary>
    public class MMDontDestroyOnLoad : MonoBehaviour
	{
		/// <summary>
		/// On Awake we make sure our object will not destroy on the next scene load
		/// </summary>
		protected void Awake()
		{
			DontDestroyOnLoad(this.gameObject);
		}
	}    
}