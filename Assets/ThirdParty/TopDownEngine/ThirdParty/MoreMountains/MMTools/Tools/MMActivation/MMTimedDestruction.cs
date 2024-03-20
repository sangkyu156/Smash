using UnityEngine;
using System.Collections;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 이 구성 요소를 개체에 추가하면 Start() 후 X초 후에 자동으로 삭제됩니다.
    /// </summary>
    [AddComponentMenu("More Mountains/Tools/Activation/MMTimedDestruction")]
	public class MMTimedDestruction : MonoBehaviour
	{
		/// the possible destruction modes
		public enum TimedDestructionModes { Destroy, Disable }

		/// the destruction mode for this object : destroy or disable
		public TimedDestructionModes TimeDestructionMode = TimedDestructionModes.Destroy;
		/// The time (in seconds) before we destroy the object
		public float TimeBeforeDestruction=2;

		/// <summary>
		/// On Start(), we schedule the object's destruction
		/// </summary>
		protected virtual void Start ()
		{
			StartCoroutine(Destruction());
		}
		
		/// <summary>
		/// Destroys the object after TimeBeforeDestruction seconds
		/// </summary>
		protected virtual IEnumerator Destruction()
		{
			yield return MMCoroutine.WaitFor(TimeBeforeDestruction);

			if (TimeDestructionMode == TimedDestructionModes.Destroy)
			{
				Destroy(gameObject);
			}
			else
			{
				gameObject.SetActive(false);
			}	        
		}
	}
}