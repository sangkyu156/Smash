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
        if (ES3.FileExists(FileName))//저장한 파일이 있는지 확인해서 있다면
        {
            button.interactable = true;
        }
        else//저장한 파일이 있는지 확인해서 없다면
        {
            button.interactable = false;

            // hexadecimal 색상 코드
            string hexColor = "#7D7D7D"; // 회색

            // hexadecimal 색상 코드를 Color32로 변환
            Color32 color = HexToColor(hexColor);
            color.a = 210;

            // 텍스트 색상 변경
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
