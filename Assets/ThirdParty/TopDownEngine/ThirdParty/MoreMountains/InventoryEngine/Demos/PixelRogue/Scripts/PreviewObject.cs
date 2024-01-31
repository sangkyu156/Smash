using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreviewObject : MonoBehaviour
{
    [SerializeField]
    private int layerGround; 
    public MeshRenderer myMaterial;

    [SerializeField]
    private Material green;
    [SerializeField]
    private Material red;

    public float colliderSize;
    public bool isInstallable;

    private void Awake()
    {
        myMaterial = GetComponent<MeshRenderer>();

        colliderSize = GetCapsuleColliderSizeFloat(this.gameObject.GetComponent<CapsuleCollider>());
        layerGround = 9;
    }

    void Update()
    {
        OnTriggerEnter2();
    }

    private void OnTriggerEnter2()
    {
        // 현재 GameObject 주위에 반경 colliderSize 안에 있는 Collider들을 모두 찾습니다.
        Collider[] colliders = Physics.OverlapSphere(transform.position, colliderSize);

        // 모든 찾은 Collider들을 순회하면서 처리합니다.
        foreach (Collider collider in colliders)
        {
            // 여기에서 원하는 동작을 수행
            HandleCollision(collider);
        }
    }

    // 충돌 처리를 담당하는 함수
    void HandleCollision(Collider otherCollider)
    {
        if (otherCollider.gameObject.layer != layerGround)
        {
            myMaterial.material = red;
            isInstallable = false;
        }
        else
        {
            myMaterial.material = green;
            isInstallable = true;
        }
    }

    //콜라이더 사이즈 씬화면에 그리기
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        // 현재 객체의 위치에 구체를 그립니다.
        Gizmos.DrawWireSphere(transform.position, colliderSize);
    }

    //캡슐콜라이더 크기를 float형으로 반환 하는 함수
    float GetCapsuleColliderSizeFloat(CapsuleCollider capsuleCollider)
    {
        // CapsuleCollider의 크기는 height와 radius를 적절히 조합하여 계산
        float size = capsuleCollider.radius * 2;

        return size;
    }
}