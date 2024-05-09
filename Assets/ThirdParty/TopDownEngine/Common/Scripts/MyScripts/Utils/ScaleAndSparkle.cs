using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

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
        rectTransform.DOBlendableScaleBy(new Vector3(1, 1, 1), ranNum).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo).SetUpdate(true);
    }

    public void ScaleUPandDownOff()
    {
        rectTransform.DOPause();
    }

    void SparkleUPandDown(float ranNum2)
    {
        //�ȵ�(���� �ʿ��� ��������)
        DOTween.To(() => newColor, x => newColor = x, new Color(1, 1, 1, 0), ranNum2).SetOptions(true).SetLoops(-1, LoopType.Yoyo).OnStart(() => { });
    }
}
