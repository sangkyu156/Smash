using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;
using MoreMountains.Tools;

public class BattlefieldManager : MonoBehaviour, MMEventListener<MMGameEvent>
{
    public Transform nav;

    void Start()
    {
        
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.J))
        {
            NavMeshBake();
            Debug.Log("j´©¸§");
        }
    }

    public void NavMeshBake()
    {
        nav.GetComponent<NavMeshSurface>().BuildNavMesh();
    }

    public void OnMMEvent(MMGameEvent gameEvent)
    {
        if (gameEvent.EventName == "Installed")
        {
            NavMeshBake();
        }
    }

    void OnEnable()
    {
    	this.MMEventStartListening<MMGameEvent>();
    }

    void OnDisable()
    {
        this.MMEventStopListening<MMGameEvent>();
    }
}
