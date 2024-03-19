using UnityEngine;
using System.Collections;
using System.Text;
using MoreMountains.Tools;
using UnityEngine.UI;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 진행률 표시줄과 텍스트 표시를 결합하고 무기의 현재 탄약 수준을 표시하는 데 사용할 수 있는 클래스입니다.
    /// </summary>
    [AddComponentMenu("TopDown Engine/GUI/AmmoDisplay")]
	public class AmmoDisplay : MMProgressBar 
	{
        /// AmmoDisplay의 ID
        [Tooltip("AmmoDisplay의 ID")]
		public int AmmoDisplayID = 0;
        /// 현재 탄약 번호를 표시하는 데 사용되는 Text 객체
        [Tooltip("현재 탄약 번호를 표시하는 데 사용되는 Text 객체")]
		public Text TextDisplay;

		protected int _totalAmmoLastTime, _maxAmmoLastTime, _ammoInMagazineLastTime, _magazineSizeLastTime;
		protected StringBuilder _stringBuilder;

		/// <summary>
		/// On init we initialize our string builder
		/// </summary>
		public override void Initialization()
		{
			base.Initialization();
			_stringBuilder = new StringBuilder();
		}
		
		/// <summary>
		/// Updates the text display with the parameter string
		/// </summary>
		/// <param name="newText">New text.</param>
		public virtual void UpdateTextDisplay(string newText)
		{
			if (TextDisplay != null)
			{
				TextDisplay.text = newText;
			}
		}

		/// <summary>
		/// Updates the ammo display's text and progress bar
		/// </summary>
		/// <param name="magazineBased">If set to <c>true</c> magazine based.</param>
		/// <param name="totalAmmo">Total ammo.</param>
		/// <param name="maxAmmo">Max ammo.</param>
		/// <param name="ammoInMagazine">Ammo in magazine.</param>
		/// <param name="magazineSize">Magazine size.</param>
		/// <param name="displayTotal">If set to <c>true</c> display total.</param>
		public virtual void UpdateAmmoDisplays(bool magazineBased, int totalAmmo, int maxAmmo, int ammoInMagazine, int magazineSize, bool displayTotal)
		{
			// we make sure there's actually something to update
			if ((_totalAmmoLastTime == totalAmmo)
			    && (_maxAmmoLastTime == maxAmmo)
			    && (_ammoInMagazineLastTime == ammoInMagazine)
			    && (_magazineSizeLastTime == magazineSize))
			{
				return;
			}

			_stringBuilder.Clear();
			
			if (magazineBased)
			{
				this.UpdateBar(ammoInMagazine,0,magazineSize);	
				if (displayTotal)
				{
					_stringBuilder.Append(ammoInMagazine.ToString());
					_stringBuilder.Append("/");
					_stringBuilder.Append(magazineSize.ToString());
					_stringBuilder.Append(" - ");
					_stringBuilder.Append((totalAmmo - ammoInMagazine).ToString());
					this.UpdateTextDisplay (_stringBuilder.ToString());					
				}
				else
				{
					_stringBuilder.Append(ammoInMagazine.ToString());
					_stringBuilder.Append("/");
					_stringBuilder.Append(magazineSize.ToString());
					this.UpdateTextDisplay (_stringBuilder.ToString());
				}
			}
			else
			{
				_stringBuilder.Append(totalAmmo.ToString());
				_stringBuilder.Append("/");
				_stringBuilder.Append(maxAmmo.ToString());
				this.UpdateBar(totalAmmo,0,maxAmmo);	
				this.UpdateTextDisplay (_stringBuilder.ToString());
			}

			_totalAmmoLastTime = totalAmmo;
			_maxAmmoLastTime = maxAmmo;
			_ammoInMagazineLastTime = ammoInMagazine;
			_magazineSizeLastTime = magazineSize;
		}
	}
}