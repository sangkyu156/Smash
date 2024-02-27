using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using UnityEngine.UI;
using DG.DemiEditor;
using DG.DOTweenEditor.UI;
using DG.Tweening.Core;

public class ScaleAndSparkle : MonoBehaviour
{
    Image image;
    Color newColor;
    RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();
        newColor = image.color;
    }

    private void Start()
    {
        float randomValue = UnityEngine.Random.Range(0.3f, 1.2f);
        ScaleUPandDown(randomValue);
    }

    void ScaleUPandDown(float ranNum)
    {
        rectTransform.DOBlendableScaleBy(new Vector3(1, 1, 1), ranNum).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);
    }

    void SparkleUPandDown(float ranNum2)
    {
        //안됨(아직 필요없어서 수정안함)
        DOTween.To(() => newColor, x => newColor = x, new Color(1, 1, 1, 0), ranNum2).SetOptions(true).SetLoops(-1, LoopType.Yoyo).OnStart(() => {});
    }
}
