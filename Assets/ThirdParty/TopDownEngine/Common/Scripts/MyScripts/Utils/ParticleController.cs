using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ParticleController : MonoBehaviour
{
    private new ParticleSystem particleSystem;
    CapsuleCollider capsuleCollider;
    public Slider slider;
    float totalDuration;
    float usageTime;
    public Define.Grade grade;

    private void Start()
    {
        // 파티클 시스템의 재생 시간을 가져옵니다.
        totalDuration = particleSystem.main.duration;
        slider.maxValue = totalDuration;
    }

    private void Update()
    {
        if (particleSystem.isPlaying && slider.enabled)
        {
            // 현재 파티클 시스템이 재생된 시간을 더해줍니다.
            usageTime += Time.deltaTime;

            // 남은 시간을 계산합니다.
            float remainingTime = Mathf.Max(totalDuration - usageTime, 0f);

            // 남은 시간을 슬라이더의 value로 설정합니다.
            slider.value = remainingTime;
        }
        else
        {
            // 파티클 시스템이 재생 중이 아닌 경우, 슬라이더의 value를 0으로 설정합니다.
            slider.value = 0f;
        }
    }

    private void OnEnable()
    {
        particleSystem = GetComponent<ParticleSystem>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        particleSystem.Play();
        // 파티클의 수명이 끝나면 해당 게임 오브젝트 삭제
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
        slider.gameObject.SetActive(false);
        MMGameEvent.Trigger("Destroyed");
    }
}
