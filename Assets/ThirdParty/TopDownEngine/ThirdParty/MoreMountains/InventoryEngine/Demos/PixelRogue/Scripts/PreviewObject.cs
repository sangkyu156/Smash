using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreviewObject : MonoBehaviour
{
    private List<Collider> colliderList = new List<Collider>();

    [SerializeField]
    private int layerGround; 
    public MeshRenderer myMaterial;

    [SerializeField]
    private Material green;
    [SerializeField]
    private Material red;

    public float colliderSize;

    private void Awake()
    {
        myMaterial = GetComponent<MeshRenderer>();

        colliderSize = GetCapsuleColliderSizeFloat(this.gameObject.GetComponent<CapsuleCollider>());
    }

    void Update()
    {
        OnTriggerEnter2();
        //ChangeColor();
    }

    private void ChangeColor()
    {
        //if (colliderList.Count > 0)
        //    SetColor(red);
        //else
        //    SetColor(green);

        if (colliderList.Count > 0)
        {
            myMaterial.material = red;
            for (int i = 0; i < colliderList.Count; i++)
            {
                Debug.Log($"현재 colliderList에 들어있는 오브젝트 = {colliderList[i].name}");
            }
        }
        else
            myMaterial.material = green;
    }

    private void SetColor(Material mat)
    {
        foreach (Transform tf_Child in this.transform)
        {
            Material[] newMaterials = new Material[tf_Child.GetComponent<Renderer>().materials.Length];

            for (int i = 0; i < newMaterials.Length; i++)
            {
                newMaterials[i] = mat;
            }

            tf_Child.GetComponent<Renderer>().materials = newMaterials;
        }
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    if (other.gameObject.layer != layerGround)
    //        colliderList.Add(other);
    //}

    private void OnTriggerEnter2()
    {
        // 현재 GameObject 주위에 반경 1.0f 안에 있는 Collider들을 모두 찾습니다.
        Collider[] colliders = Physics.OverlapSphere(transform.position, colliderSize);

        // 모든 찾은 Collider들을 순회하면서 처리합니다.
        foreach (Collider collider in colliders)
        {
            // 자기 자신과의 충돌은 무시합니다.
            if (collider == GetComponent<Collider>())
                continue;

            // 여기에서 원하는 동작을 수행
            HandleCollision(collider);
        }
    }

    // 충돌 처리를 담당하는 함수
    void HandleCollision(Collider otherCollider)
    {
        if (otherCollider.gameObject.layer != layerGround)
            myMaterial.material = red;
        else
            myMaterial.material = green;
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

    //private void OnTriggerExit(Collider other)
    //{
    //    if (other.gameObject.layer != layerGround)
    //        colliderList.Remove(other);
    //}

    public bool isBuildable()
    {
        return colliderList.Count == 0;
    }
}