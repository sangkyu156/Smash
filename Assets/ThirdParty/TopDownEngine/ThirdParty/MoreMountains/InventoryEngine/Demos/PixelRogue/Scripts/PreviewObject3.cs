using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static NPOI.HSSF.Util.HSSFColor;

public class PreviewObject3 : MonoBehaviour
{
    [SerializeField]
    private int layerGround;
    public bool isInstallable;

    private void Awake()
    {
        layerGround = 9;
        isInstallable = true;
    }

    private void Update()
    {
        HandleCollision();
    }

    // �浹 ó���� ����ϴ� �Լ�
    void HandleCollision()
    {
        isInstallable = true;
    }
}