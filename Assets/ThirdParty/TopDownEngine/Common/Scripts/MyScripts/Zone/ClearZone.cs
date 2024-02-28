using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearZone : MonoBehaviour
{
    public GameObject player;

    private void Awake()
    {
        if (player == null)
            player = CreateManager.Instance.player;
    }
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            StageClear();
            StartCoroutine(ClearSplashOn());
        }
    }

    void StageClear()
    {
        Time.timeScale = 0;
        CreateManager.Instance.SetClearData();
        GameObject clearEffect = CreateManager.Instantiate("Battlefield/ClearEffectYellow");
        clearEffect.transform.position = player.transform.position;
    }

    IEnumerator ClearSplashOn()
    {
        yield return new WaitForSecondsRealtime(2.7f); // 3√  ¥Î±‚
        CreateManager.Instance.clearSplash.SetActive(true);
    }
}
