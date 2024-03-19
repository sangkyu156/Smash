using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 이 클래스를 캔버스에 추가하면 터치 위치에서 TouchPrefabs의 위치가 자동으로 변경됩니다.
    /// 게임이 기본 동시 터치 수(6)보다 많은 경우 더 높은 TouchProvision을 설정할 수 있습니다.
    /// 이 모노를 중지/작동하려면 비활성화/활성화합니다.
    /// </summary>
    public class MMDebugTouchDisplay : MonoBehaviour
	{
		[Header("Bindings")]
		/// the canvas to display the TouchPrefabs on
		public Canvas TargetCanvas;

		[Header("Touches")]
		/// the prefabs to instantiate to signify the position of the touches
		public RectTransform TouchPrefab;
		/// the amount of these prefabs to pool and provision
		public int TouchProvision = 6;

		protected List<RectTransform> _touchDisplays;

		/// <summary>
		/// On Start we initialize our pool
		/// </summary>
		protected virtual void Start()
		{
			Initialization();
		}

		/// <summary>
		/// Creates the pool of prefabs
		/// </summary>
		protected virtual void Initialization()
		{
			_touchDisplays = new List<RectTransform>();

			for (int i = 0; i < TouchProvision; i++)
			{
				RectTransform touchDisplay = Instantiate(TouchPrefab);
				touchDisplay.transform.SetParent(TargetCanvas.transform);
				touchDisplay.name = "MMDebugTouchDisplay_" + i;
				touchDisplay.gameObject.SetActive(false);
				_touchDisplays.Add(touchDisplay);
			}

			this.enabled = false;
		}

		/// <summary>
		/// On update we detect touches and move our prefabs at their position
		/// </summary>
		protected virtual void Update()
		{
			DisableAllDisplays();
			DetectTouches();
		}

		/// <summary>
		/// Acts on all touches
		/// </summary>
		protected virtual void DetectTouches()
		{
			for (int i = 0; i < Input.touchCount; ++i)
			{
				_touchDisplays[i].gameObject.SetActive(true);
				_touchDisplays[i].position = Input.GetTouch(i).position;
			}
		}

		/// <summary>
		/// Disables all touch prefabs
		/// </summary>
		protected virtual void DisableAllDisplays()
		{
			foreach(RectTransform display in _touchDisplays)
			{
				display.gameObject.SetActive(false);
			}
		}

		/// <summary>
		/// When this mono gets disabled we turn all our prefabs off
		/// </summary>
		protected virtual void OnDisable()
		{
			DisableAllDisplays();
		}
	}
}