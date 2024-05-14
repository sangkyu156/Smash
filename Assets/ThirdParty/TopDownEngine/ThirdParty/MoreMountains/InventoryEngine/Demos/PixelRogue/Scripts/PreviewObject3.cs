using UnityEngine;

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

    // 충돌 처리를 담당하는 함수
    void HandleCollision()
    {
        isInstallable = true;
    }
}