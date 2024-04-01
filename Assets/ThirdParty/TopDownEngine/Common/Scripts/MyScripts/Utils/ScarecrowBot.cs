using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ES3AutoSaveMgr;

public class ScarecrowBot : MonoBehaviour, MMEventListener<MMGameEvent>
{
    SphereCollider sphereCollider;

    private void Awake()
    {
        sphereCollider = GetComponent<SphereCollider>();
    }

    void OnEnable()
    {
        this.MMEventStartListening<MMGameEvent>();
    }

    void OnDisable()
    {
        this.MMEventStopListening<MMGameEvent>();
    }

    public void OnMMEvent(MMGameEvent gameEvent)
    {
        if (gameEvent.EventName == "Installing")
        {
            sphereCollider.isTrigger = false;
            Debug.Log("콜라이더 isTrigger = false");
        }

        if (gameEvent.EventName == "Installed")
        {
            sphereCollider.isTrigger = true;
            Debug.Log("콜라이더 isTrigger = true");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //if (other.gameObject.layer == LayerMask.NameToLayer("Enemies") || other.gameObject.layer == LayerMask.NameToLayer("MidairEnemies"))
        //{
        //    other.GetComponent<Character>().Provoked(this.gameObject);
        //}
    }

    private void OnTriggerExit(Collider other)
    {
        //if (other.gameObject.layer == LayerMask.NameToLayer("Enemies") || other.gameObject.layer == LayerMask.NameToLayer("MidairEnemies"))
        //{
        //    other.GetComponent<Character>().ProvocationEnd();
        //}
    }
}
