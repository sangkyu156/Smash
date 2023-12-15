using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace MoreMountains.InventoryEngine
{	
	[RequireComponent(typeof(InventoryDisplay))]
    /// <summary>
    /// InventoryDisplay와 페어링될 때 노래 재생을 처리하는 구성 요소
    /// </summary>
    public class InventorySoundPlayer : MonoBehaviour, MMEventListener<MMInventoryEvent>
	{
		public enum Modes { Direct, Event }

		[Header("Settings")]
        /// 사운드 재생을 선택하는 모드입니다. Direct는 오디오 소스를 재생하고, 이벤트는 MMSoundManager에 의해 포착되는 MMSfxEvent를 호출합니다.
        public Modes Mode = Modes.Direct;
		
		[Header("Sounds")]
		[MMInformation("Here you can define the default sounds that will get played when interacting with this inventory.",MMInformationAttribute.InformationType.Info,false)]
		/// the audioclip to play when the inventory opens
		public AudioClip OpenFx;
		/// the audioclip to play when the inventory closes
		public AudioClip CloseFx;
		/// the audioclip to play when moving from one slot to another
		public AudioClip SelectionChangeFx;
		/// the audioclip to play when moving from one slot to another
		public AudioClip ClickFX;
		/// the audioclip to play when moving an object successfully
		public AudioClip MoveFX;
		/// the audioclip to play when an error occurs (selecting an empty slot, etc)
		public AudioClip ErrorFx;
		/// the audioclip to play when an item is used, if no other sound has been defined for it
		public AudioClip UseFx;
		/// the audioclip to play when an item is dropped, if no other sound has been defined for it
		public AudioClip DropFx;
		/// the audioclip to play when an item is equipped, if no other sound has been defined for it
		public AudioClip EquipFx;

		protected string _targetInventoryName;
		protected string _targetPlayerID;
		protected AudioSource _audioSource;

        /// <summary>
        /// 시작 시 플레이어를 설정하고 나중에 사용할 수 있도록 몇 가지 참고 자료를 가져옵니다.
        /// </summary>
        protected virtual void Start()
		{
			SetupInventorySoundPlayer ();
			_audioSource = GetComponent<AudioSource> ();
			_targetInventoryName = this.gameObject.MMGetComponentNoAlloc<InventoryDisplay> ().TargetInventoryName;
			_targetPlayerID = this.gameObject.MMGetComponentNoAlloc<InventoryDisplay> ().PlayerID;
		}

        /// <summary>
        /// 인벤토리 사운드 플레이어를 설정합니다.
        /// </summary>
        public virtual void SetupInventorySoundPlayer()
		{
			AddAudioSource ();			
		}

        /// <summary>
        /// 필요한 경우 오디오 소스 구성 요소를 추가합니다.
        /// </summary>
        protected virtual void AddAudioSource()
		{
			if (GetComponent<AudioSource>() == null)
			{
				this.gameObject.AddComponent<AudioSource>();
			}
		}

        /// <summary>
        /// 매개변수 문자열에 지정된 사운드를 재생합니다.
        /// </summary>
        /// <param name="soundFx">Sound fx.</param>
        public virtual void PlaySound(string soundFx)
		{
			if (soundFx==null || soundFx=="")
			{
				return;
			}

			AudioClip soundToPlay=null;
			float volume=1f;

			switch (soundFx)
			{
				case "error":
					soundToPlay=ErrorFx;
					volume=1f;
					break;
				case "select":
					soundToPlay=SelectionChangeFx;
					volume=0.5f;
					break;
				case "click":
					soundToPlay=ClickFX;
					volume=0.5f;
					break;
				case "open":
					soundToPlay=OpenFx;
					volume=1f;
					break;
				case "close":
					soundToPlay=CloseFx;
					volume=1f;
					break;
				case "move":
					soundToPlay=MoveFX;
					volume=1f;
					break;
				case "use":
					soundToPlay=UseFx;
					volume=1f;
					break;
				case "drop":
					soundToPlay=DropFx;
					volume=1f;
					break;
				case "equip":
					soundToPlay=EquipFx;
					volume=1f;
					break;
			}

			if (soundToPlay!=null)
			{
				if (Mode == Modes.Direct)
				{
					_audioSource.PlayOneShot(soundToPlay,volume);	
				}
				else
				{
					MMSfxEvent.Trigger(soundToPlay, null, volume, 1);	
				}
			}
		}

        /// <summary>
        /// 매개변수에 지정된 사운드 FX를 원하는 볼륨으로 재생합니다.
        /// </summary>
        /// <param name="soundFx">Sound fx.</param>
        /// <param name="volume">Volume.</param>
        public virtual void PlaySound(AudioClip soundFx,float volume)
		{
			if (soundFx != null)
			{
				if (Mode == Modes.Direct)
				{
					_audioSource.PlayOneShot(soundFx, volume);
				}
				else
				{
					MMSfxEvent.Trigger(soundFx, null, volume, 1);
				}
			}
		}

        /// <summary>
        /// MMInventoryEvents를 포착하고 그에 따라 작동하여 해당 사운드를 재생합니다.
        /// </summary>
        /// <param name="inventoryEvent">Inventory event.</param>
        public virtual void OnMMEvent(MMInventoryEvent inventoryEvent)
		{
			// if this event doesn't concern our inventory display, we do nothing and exit
			if (inventoryEvent.TargetInventoryName != _targetInventoryName)
			{
				return;
			}

			if (inventoryEvent.PlayerID != _targetPlayerID)
			{
				return;
			}

			switch (inventoryEvent.InventoryEventType)
			{
				case MMInventoryEventType.Select:
					this.PlaySound("select");
					break;
				case MMInventoryEventType.Click:
					this.PlaySound("click");
					break;
				case MMInventoryEventType.InventoryOpens:
					this.PlaySound("open");
					break;
				case MMInventoryEventType.InventoryCloses:
					this.PlaySound("close");
					break;
				case MMInventoryEventType.Error:
					this.PlaySound("error");
					break;
				case MMInventoryEventType.Move:
					if (inventoryEvent.EventItem.MovedSound == null)
					{
						if (inventoryEvent.EventItem.UseDefaultSoundsIfNull) { this.PlaySound ("move"); }
					} else
					{
						this.PlaySound (inventoryEvent.EventItem.MovedSound, 1f);
					}
					break;
				case MMInventoryEventType.ItemEquipped:
					if (inventoryEvent.EventItem.EquippedSound == null)
					{
						if (inventoryEvent.EventItem.UseDefaultSoundsIfNull) { this.PlaySound ("equip"); }
					} else
					{
						this.PlaySound (inventoryEvent.EventItem.EquippedSound, 1f);
					}
					break;
				case MMInventoryEventType.ItemUsed:
					if (inventoryEvent.EventItem.UsedSound == null)
					{
						if (inventoryEvent.EventItem.UseDefaultSoundsIfNull) { this.PlaySound ("use"); 	}
					} else
					{
						this.PlaySound (inventoryEvent.EventItem.UsedSound, 1f);
					}
					break;
				case MMInventoryEventType.Drop:
					if (inventoryEvent.EventItem.DroppedSound == null)
					{
						if (inventoryEvent.EventItem.UseDefaultSoundsIfNull) { this.PlaySound ("drop"); 	}
					} else
					{
						this.PlaySound (inventoryEvent.EventItem.DroppedSound, 1f);
					}
					break;
			}
		}

        /// <summary>
        /// OnEnable을 사용하면 MMInventoryEvents 수신이 시작됩니다.
        /// </summary>
        protected virtual void OnEnable()
		{
			this.MMEventStartListening<MMInventoryEvent>();
		}

        /// <summary>
        /// OnDisable을 사용하면 MMInventoryEvents 수신이 중지됩니다.
        /// </summary>
        protected virtual void OnDisable()
		{
			this.MMEventStopListening<MMInventoryEvent>();
		}
	}
}