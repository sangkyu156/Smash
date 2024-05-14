using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CONTINUE : MonoBehaviour
{
    public TextMeshProUGUI text;
    Button button;

    private string FileName = "SaveFile.es3";

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (ES3.FileExists(FileName))//������ ������ �ִ��� Ȯ���ؼ� �ִٸ�
        {
            button.interactable = true;
        }
        else//������ ������ �ִ��� Ȯ���ؼ� ���ٸ�
        {
            button.interactable = false;

            // hexadecimal ���� �ڵ�
            string hexColor = "#7D7D7D"; // ȸ��

            // hexadecimal ���� �ڵ带 Color32�� ��ȯ
            Color32 color = HexToColor(hexColor);
            color.a = 210;

            // �ؽ�Ʈ ���� ����
            text.color = color;
        }
    }

    Color32 HexToColor(string hex)
    {
        Color color = new Color();

        if (!ColorUtility.TryParseHtmlString(hex, out color))
        {
            Debug.LogError("Invalid hexadecimal color code: " + hex);
        }

        return color;
    }

    public void ContinueButton()
    {
        DataManager.Instance.DataLoad();
        LevelManager.Instance.GotoLevel("Village");
    }
}
