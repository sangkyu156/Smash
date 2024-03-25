using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ParticleController : MonoBehaviour
{
    private new ParticleSystem particleSystem;
    public ParticleSystem subParticle; //�ݶ��̴� �������� ���� ���������ϴ� ��ƼŬ�ý���
    CapsuleCollider capsuleCollider;
    public Slider slider;
    float totalDuration;
    float usageTime;
    float particleCycle = 1f; //������ƼŬ�� ������ �ƴ� ��ȸ���̸� ��ȸ���� ����Ǵ� �ð�
    public Define.Grade grade;

    private void Start()
    {
        // ��ƼŬ �ý����� ��� �ð��� �����ɴϴ�.
        totalDuration = particleSystem.main.duration;
        slider.maxValue = totalDuration;
    }

    private void Update()
    {
        if(!subParticle.main.loop && subParticle != null)
        {
            particleCycle += Time.deltaTime;

            if (particleCycle >= 1)
            {
                subParticle.Play();
                particleCycle = 0;
            }
        }

        if (particleSystem.isPlaying && slider.enabled)
        {
            // ���� ��ƼŬ �ý����� ����� �ð��� �����ݴϴ�.
            usageTime += Time.deltaTime;

            // ���� �ð��� ����մϴ�.
            float remainingTime = Mathf.Max(totalDuration - usageTime, 0f);

            // ���� �ð��� �����̴��� value�� �����մϴ�.
            slider.value = remainingTime;
        }
        else
        {
            // ��ƼŬ �ý����� ��� ���� �ƴ� ���, �����̴��� value�� 0���� �����մϴ�.
            slider.value = 0f;
        }
    }

    private void OnEnable()
    {
        particleSystem = GetComponent<ParticleSystem>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        particleSystem.Play();
        // ��ƼŬ�� ������ ������ �ش� ���� ������Ʈ ����
        Invoke("DestroyParticleObject", particleSystem.main.duration + 2f);
        Invoke("ParticleColliderOff", particleSystem.main.duration);
    }

    private void DestroyParticleObject()
    {
        Destroy(gameObject);
    }

    void ParticleColliderOff()
    {
        capsuleCollider.enabled = false;
        if(subParticle != null)
            subParticle.gameObject.SetActive(false);
        slider.gameObject.SetActive(false);
        MMGameEvent.Trigger("Destroyed");
    }
}
