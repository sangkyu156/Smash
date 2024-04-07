using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearZone : MonoBehaviour
{
    public GameObject player;
    GameObject clearZoneArrow;

    private void Awake()
    {
        clearZoneArrow = CreateManager.Instantiate("Battlefield/ClearZoneArrow");
    }

    private void Start()
    {
        Invoke("asdqwq", 1f);
    }

    void Update()
    {
        if(player != null)
            PlayerFollow();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            StageClear();
            StartCoroutine(ClearSplashOn());
        }
    }

    public void asdqwq()
    {
        player = LevelManager.Instance.Players[0].gameObject;
    }

    void StageClear()
    {
        Time.timeScale = 0;
        CreateManager.Instance.SetClearData();
        GameObject clearEffect = CreateManager.Instantiate("Battlefield/ClearEffectYellow");
        clearEffect.transform.position = player.transform.position;
    }

    void PlayerFollow()
    {
        //Ŭ������ �ٶ󺸵��� Rotation�� ����
        clearZoneArrow.transform.LookAt(transform.position);
        Vector3 currentRotation = clearZoneArrow.transform.rotation.eulerAngles;
        Vector3 newRotation = currentRotation + new Vector3(-90f, -90f, 0f);
        clearZoneArrow.transform.rotation = Quaternion.Euler(newRotation);

        //Ŭ������ �������� �������̵�
        clearZoneArrow.transform.position = player.transform.position + (transform.position - player.transform.position).normalized * 3;
    }

    IEnumerator ClearSplashOn()
    {
        yield return new WaitForSecondsRealtime(2.7f); // 3�� ���
        CreateManager.Instance.clearSplash.SetActive(true);
        CreateManager.Instance.quickSlotsDisplay.SetActive(false);
    }
}
