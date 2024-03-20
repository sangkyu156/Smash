using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using MoreMountains.Tools;
using MoreMountains.MMInterface;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 간단한 시작 화면 클래스.
    /// </summary>
    [AddComponentMenu("TopDown Engine/GUI/StartScreen")]
	public class StartScreen : TopDownMonoBehaviour
	{
		/// the level to load after the start screen
		[Tooltip("시작 화면 이후에 로드할 레벨")]
		public string NextLevel;
		/// the name of the MMSceneLoadingManager scene you want to use
		[Tooltip("사용하려는 MMSceneLoadingManager 장면의 이름")]
		public string LoadingSceneName = "";
		/// the delay after which the level should auto skip (if less than 1s, won't autoskip)
		[Tooltip("레벨이 자동으로 건너뛰어야 하는 지연 시간(1초 미만이면 자동 건너뛰지 않음)")]
		public float AutoSkipDelay = 0f;

		[Header("Fades")]
		/// the duration of the fade from black at the start of the level
		[Tooltip("레벨 시작 시 검정색에서 페이드가 지속되는 시간")]
		public float FadeInDuration = 1f;
		/// the duration of the fade to black at the end of the level
		[Tooltip("레벨 끝에서 검은색으로 페이드되는 지속 시간")]
		public float FadeOutDuration = 1f;
		/// the tween type to use to fade the startscreen in and out 
		[Tooltip("시작 화면을 페이드 인 및 페이드 아웃하는 데 사용하는 트윈 유형")]
		public MMTweenType Tween = new MMTweenType(MMTween.MMTweenCurve.EaseInOutCubic);

		[Header("Sound Settings Bindings")]
		/// the switch used to turn the music on or off
		[Tooltip("음악을 켜거나 끄는 데 사용되는 스위치")]
		public MMSwitch MusicSwitch;
		/// the switch used to turn the SFX on or off
		[Tooltip("SFX를 켜거나 끄는 데 사용되는 스위치")]
		public MMSwitch SfxSwitch;

		/// <summary>
		/// Initialization
		/// </summary>
		protected virtual void Awake()
		{	
			GUIManager.Instance.SetHUDActive (false);
			MMFadeOutEvent.Trigger(FadeInDuration, Tween);
			Cursor.visible = true;
			if (AutoSkipDelay > 1f)
			{
				FadeOutDuration = AutoSkipDelay;
				StartCoroutine (LoadFirstLevel ());
			}
		}

		/// <summary>
		/// On Start, initializes the music and sfx switches
		/// </summary>
		protected async void Start()
		{
			await Task.Delay(1);
			
			if (MusicSwitch != null)
			{
				MusicSwitch.CurrentSwitchState = MMSoundManager.Instance.settingsSo.Settings.MusicOn ? MMSwitch.SwitchStates.Right : MMSwitch.SwitchStates.Left;
				MusicSwitch.InitializeState ();
			}

			if (SfxSwitch != null)
			{
				SfxSwitch.CurrentSwitchState = MMSoundManager.Instance.settingsSo.Settings.SfxOn ? MMSwitch.SwitchStates.Right : MMSwitch.SwitchStates.Left;
				SfxSwitch.InitializeState ();
			}
		}

		/// <summary>
		/// During update we simply wait for the user to press the "jump" button.
		/// </summary>
		protected virtual void Update()
		{
			if (!Input.GetButtonDown ("Player1_Jump"))
				return;
			
			ButtonPressed ();
		}

		/// <summary>
		/// What happens when the main button is pressed
		/// </summary>
		public virtual void ButtonPressed()
		{
			MMFadeInEvent.Trigger(FadeOutDuration, Tween);
			// if the user presses the "Jump" button, we start the first level.
			StartCoroutine (LoadFirstLevel ());
		}

		/// <summary>
		/// Loads the next level.
		/// </summary>
		/// <returns>The first level.</returns>
		protected virtual IEnumerator LoadFirstLevel()
		{
			yield return new WaitForSeconds (FadeOutDuration);
			if (LoadingSceneName == "")
			{
				MMSceneLoadingManager.LoadScene (NextLevel);	
			}
			else
			{
				MMSceneLoadingManager.LoadScene (NextLevel, LoadingSceneName);
			}
			
		}
	}
}