using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using MoreMountains.Tools;
using UnityEngine.EventSystems;
using MoreMountains.Feedbacks;
using UnityEngine.SceneManagement;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 모든 GUI 효과 및 변경 사항을 처리합니다.
    /// </summary>
    [AddComponentMenu("TopDown Engine/Managers/GUIManager")]
	public class GUIManager : MMSingleton<GUIManager> 
	{
		/// the main canvas
		[Tooltip("메인 캔버스")]
		public Canvas MainCanvas;
		/// the game object that contains the heads up display (avatar, health, points...)
		[Tooltip("헤드업 디스플레이(아바타, 체력, 포인트...)가 포함된 게임 개체")]
		public GameObject HUD;
		/// the health bars to update
		[Tooltip("업데이트할 체력 바")]
		public MMProgressBar[] HealthBars; //스테미나Bar도 같이 들어있을 수 있음
		/// the dash bars to update
		[Tooltip("업데이트할 대시 바")]
		public MMRadialProgressBar[] DashBars;
		/// the panels and bars used to display current weapon ammo
		[Tooltip("현재 무기 탄약을 표시하는 데 사용되는 패널과 막대")]
		public AmmoDisplay[] AmmoDisplays;
		/// the pause screen game object
		[Tooltip("일시정지 화면 게임 객체")]
		public GameObject PauseScreen;
        [Tooltip("도움말 화면 객체")]
        public HelperButton HelperScreen;
        /// the death screen
        [Tooltip("죽음 화면")]
		public GameObject DeathScreen;
		/// The mobile buttons
		[Tooltip("모바일 버튼")]
		public CanvasGroup Buttons;
		/// The mobile arrows
		[Tooltip("모바일 화살표")]
		public CanvasGroup Arrows;
		/// The mobile movement joystick
		[Tooltip("모바일 이동 조이스틱")]
		public CanvasGroup Joystick;
		/// the points counter
		[Tooltip("포인트 카운터")]
		public Text PointsText;
		/// the pattern to apply to format the display of points
		[Tooltip("포인트 표시 형식을 지정하기 위해 적용할 패턴")]
		public string PointsTextPattern = "000000";


		protected float _initialJoystickAlpha;
		protected float _initialButtonsAlpha;
		protected bool _initialized = false;

		//내가 만든 변수
		public CanvasGroup PlayerInventoryCanvas;
		public CanvasGroup StoreInventoryCanvas;

        /// <summary>
        /// Initialization
        /// </summary>
        protected override void Awake()
		{
			base.Awake();

			Initialization();
		}

		protected virtual void Initialization()
		{
			if (_initialized)
			{
				return;
			}

			if (Joystick != null)
			{
				_initialJoystickAlpha = Joystick.alpha;
			}
			if (Buttons != null)
			{
				_initialButtonsAlpha = Buttons.alpha;
			}

			_initialized = true;
		}

		/// <summary>
		/// Initialization
		/// </summary>
		protected virtual void Start()
		{
			RefreshPoints();
			SetPauseScreen(false);
			SetDeathScreen(false);
		}

		/// <summary>
		/// Sets the HUD active or inactive
		/// </summary>
		/// <param name="state">If set to <c>true</c> turns the HUD active, turns it off otherwise.</param>
		public virtual void SetHUDActive(bool state)
		{
			if (HUD!= null)
			{ 
				HUD.SetActive(state);
			}
			if (PointsText!= null)
			{ 
				PointsText.enabled = state;
			}
		}

		/// <summary>
		/// Sets the avatar active or inactive
		/// </summary>
		/// <param name="state">If set to <c>true</c> turns the HUD active, turns it off otherwise.</param>
		public virtual void SetAvatarActive(bool state)
		{
			if (HUD != null)
			{
				HUD.SetActive(state);
			}
		}

		/// <summary>
		/// Called by the input manager, this method turns controls visible or not depending on what's been chosen
		/// </summary>
		/// <param name="state">If set to <c>true</c> state.</param>
		/// <param name="movementControl">Movement control.</param>
		public virtual void SetMobileControlsActive(bool state, InputManager.MovementControls movementControl = InputManager.MovementControls.Joystick)
		{
			Initialization();
            
			if (Joystick != null)
			{
				Joystick.gameObject.SetActive(state);
				if (state && movementControl == InputManager.MovementControls.Joystick)
				{
					Joystick.alpha=_initialJoystickAlpha;
				}
				else
				{
					Joystick.alpha=0;
					Joystick.gameObject.SetActive (false);
				}
			}

			if (Arrows != null)
			{
				Arrows.gameObject.SetActive(state);
				if (state && movementControl == InputManager.MovementControls.Arrows)
				{
					Arrows.alpha=_initialJoystickAlpha;
				}
				else
				{
					Arrows.alpha=0;
					Arrows.gameObject.SetActive (false);
				}
			}

			if (Buttons != null)
			{
				Buttons.gameObject.SetActive(state);
				if (state)
				{
					Buttons.alpha=_initialButtonsAlpha;
				}
				else
				{
					Buttons.alpha=0;
					Buttons.gameObject.SetActive (false);
				}
			}
		}

        /// <summary>
        /// 일시 정지 화면을 켜거나 끄도록 설정합니다.
        /// </summary>
        /// <param name="state">If set to <c>true</c>, sets the pause.</param>
        public virtual void SetPauseScreen(bool state)
		{
			//PauseScreen.SetActive(state);
			//if (PauseScreen != null && PlayerInventoryCanvas.alpha == 0 && StoreInventoryCanvas.alpha == 0)
			//{
			//    PauseScreen.SetActive(state);
			//    EventSystem.current.sendNavigationEvents = state;
			//}

			if (SceneManager.GetActiveScene().name == "Village")
			{
				PauseScreen.SetActive(state);
				if (PauseScreen != null && PlayerInventoryCanvas.alpha == 0 && StoreInventoryCanvas.alpha == 0)
				{
					PauseScreen.SetActive(state);
					EventSystem.current.sendNavigationEvents = state;
				}
			}
			else if (SceneManager.GetActiveScene().name == "LevelSelect" || SceneManager.GetActiveScene().name == "Battlefield01")
			{
				if (PauseScreen != null)
				{
					PauseScreen.SetActive(state);
					EventSystem.current.sendNavigationEvents = state;
				}
			}
		}

        /// <summary>
        /// 사망 화면을 켜거나 끕니다.
        /// </summary>
        /// <param name="state">If set to <c>true</c>, sets the pause.</param>
        public virtual void SetDeathScreen(bool state)
		{
			if (DeathScreen != null)
			{
				DeathScreen.SetActive(state);
				EventSystem.current.sendNavigationEvents = state;
			}
		}

		/// <summary>
		/// Sets the jetpackbar active or not.
		/// </summary>
		/// <param name="state">If set to <c>true</c>, sets the pause.</param>
		public virtual void SetDashBar(bool state, string playerID)
		{
			if (DashBars == null)
			{
				return;
			}

			foreach (MMRadialProgressBar jetpackBar in DashBars)
			{
				if (jetpackBar != null)
				{ 
					if (jetpackBar.PlayerID == playerID)
					{
						jetpackBar.gameObject.SetActive(state);
					}					
				}
			}	        
		}

		/// <summary>
		/// Sets the ammo displays active or not
		/// </summary>
		/// <param name="state">If set to <c>true</c> state.</param>
		/// <param name="playerID">Player I.</param>
		public virtual void SetAmmoDisplays(bool state, string playerID, int ammoDisplayID)
		{
			if (AmmoDisplays == null)
			{
				return;
			}

			foreach (AmmoDisplay ammoDisplay in AmmoDisplays)
			{
				if (ammoDisplay != null)
				{ 
					if ((ammoDisplay.PlayerID == playerID) && (ammoDisplayID == ammoDisplay.AmmoDisplayID))
					{
						ammoDisplay.gameObject.SetActive(state);
					}					
				}
			}
		}
        		
		/// <summary>
		/// Sets the text to the game manager's points.
		/// </summary>
		public virtual void RefreshPoints()
		{
			if (PointsText!= null)
			{ 
				PointsText.text = GameManager.Instance.Points.ToString(PointsTextPattern);
			}
		}

		/// <summary>
		/// Updates the health bar.
		/// </summary>
		/// <param name="currentHealth">Current health.</param>
		/// <param name="minHealth">Minimum health.</param>
		/// <param name="maxHealth">Max health.</param>
		/// <param name="playerID">Player I.</param>
		public virtual void UpdateHealthBar(float currentHealth,float minHealth,float maxHealth,string playerID)
		{
			if (HealthBars == null) { return; }
			if (HealthBars.Length <= 0)	{ return; }

			foreach (MMProgressBar healthBar in HealthBars)
			{
				if (healthBar == null) { continue; }
				if (healthBar.PlayerID == playerID)
				{
					healthBar.UpdateBar(currentHealth,minHealth,maxHealth);
				}
			}
		}

		/// <summary>
		/// 스테미나바 업데이트
		/// </summary>
		/// <param name="currentHealth"></param>
		/// <param name="minHealth"></param>
		/// <param name="maxHealth"></param>
		/// <param name="playerID"></param>
        public virtual void UpdateStaminaBar(float currenStamina, float minStamina, float maxStamina, string playerID)
        {
            //이름은 HealthBars 이지만 StaminaBar가 들어 있을 수 있음
            if (HealthBars == null) { return; }
            if (HealthBars.Length <= 0) { return; }

            foreach (MMProgressBar staminaBar in HealthBars)
            {
                if (staminaBar == null) { continue; }
                if (staminaBar.PlayerID == playerID)
                {
                    staminaBar.UpdateBar(currenStamina, minStamina, maxStamina);
                }
            }
        }

        public void UpdateHealthBarText(float currentHealth, float maxHealth)
		{
			string text = currentHealth.ToString() + " / " + maxHealth.ToString();

            foreach (MMProgressBar healthBar in HealthBars)
            {
                healthBar.UpdateText(text);
            }
        }

		/// <summary>
		/// Updates the dash bars.
		/// </summary>
		/// <param name="currentFuel">Current fuel.</param>
		/// <param name="minFuel">Minimum fuel.</param>
		/// <param name="maxFuel">Max fuel.</param>
		/// <param name="playerID">Player I.</param>
		public virtual void UpdateDashBars(float currentFuel, float minFuel, float maxFuel,string playerID)
		{
			if (DashBars == null)
			{
				return;
			}

			foreach (MMRadialProgressBar dashbar in DashBars)
			{
				if (dashbar == null) { return; }
				if (dashbar.PlayerID == playerID)
				{
					dashbar.UpdateBar(currentFuel,minFuel,maxFuel);	
				}    
			}
		}

		/// <summary>
		/// Updates the (optional) ammo displays.
		/// </summary>
		/// <param name="magazineBased">If set to <c>true</c> magazine based.</param>
		/// <param name="totalAmmo">Total ammo.</param>
		/// <param name="maxAmmo">Max ammo.</param>
		/// <param name="ammoInMagazine">Ammo in magazine.</param>
		/// <param name="magazineSize">Magazine size.</param>
		/// <param name="playerID">Player I.</param>
		/// <param name="displayTotal">If set to <c>true</c> display total.</param>
		public virtual void UpdateAmmoDisplays(bool magazineBased, int totalAmmo, int maxAmmo, int ammoInMagazine, int magazineSize, string playerID, int ammoDisplayID, bool displayTotal)
		{
			if (AmmoDisplays == null)
			{
				return;
			}

			foreach (AmmoDisplay ammoDisplay in AmmoDisplays)
			{
				if (ammoDisplay == null) { return; }
				if ((ammoDisplay.PlayerID == playerID) && (ammoDisplayID == ammoDisplay.AmmoDisplayID))
				{
					ammoDisplay.UpdateAmmoDisplays (magazineBased, totalAmmo, maxAmmo, ammoInMagazine, magazineSize, displayTotal);
				}    
			}
		}
	}
}