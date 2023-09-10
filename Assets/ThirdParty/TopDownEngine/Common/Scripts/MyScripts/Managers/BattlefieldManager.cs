using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BattlefieldManager : MonoBehaviour
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
            Debug.Log("j����");
        }
    }

    public void NavMeshBake()
    {
        nav.GetComponent<NavMeshSurface>().BuildNavMesh();
    }
}
