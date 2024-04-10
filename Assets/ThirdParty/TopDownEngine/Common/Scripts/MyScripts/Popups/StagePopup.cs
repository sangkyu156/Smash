using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Codice.CM.SEIDInfo;
using MoreMountains.TopDownEngine;
using MoreMountains.Tools;

public class StagePopup : MonoBehaviour
{
    public TextMeshProUGUI titleText;

    void Start()
    {
        Time.timeScale = 0f;

        //SetText();
    }

    public void PopupClose()
    {
        Time.timeScale = 1f;
        gameObject.SetActive(false);
    }

    public void SetText()
    {
        switch (GameManager.Instance.stage)
        {
            case Define.Stage.Stage01: titleText.text = TextUtil.GetText("game:popup:stagetitle1"); break;
            case Define.Stage.Stage02: titleText.text = TextUtil.GetText("game:popup:stagetitle2"); break;
            case Define.Stage.Stage03: titleText.text = TextUtil.GetText("game:popup:stagetitle3"); break;
            case Define.Stage.Stage04: titleText.text = TextUtil.GetText("game:popup:stagetitle4"); break;
            case Define.Stage.Stage05: titleText.text = TextUtil.GetText("game:popup:stagetitle5"); break;
            case Define.Stage.Stage06: titleText.text = TextUtil.GetText("game:popup:stagetitle6"); break;
            case Define.Stage.Stage07: titleText.text = TextUtil.GetText("game:popup:stagetitle7"); break;
            case Define.Stage.Stage08: titleText.text = TextUtil.GetText("game:popup:stagetitle8"); break;
            case Define.Stage.Stage09: titleText.text = TextUtil.GetText("game:popup:stagetitle9"); break;
            case Define.Stage.Stage10: titleText.text = TextUtil.GetText("game:popup:stagetitle10"); break;
        }
    }

    public void GotoStage()
    {
        switch (GameManager.Instance.stage)
        {
            case Define.Stage.Stage01: 
            case Define.Stage.Stage02: 
            case Define.Stage.Stage03: 
            case Define.Stage.Stage04: 
            case Define.Stage.Stage05: MMSceneLoadingManager.LoadScene("Battlefield01"); break;
            case Define.Stage.Stage06: break;
            case Define.Stage.Stage07: break;
            case Define.Stage.Stage08: break;
            case Define.Stage.Stage09: break;
            case Define.Stage.Stage10: break;
        }
    }
}
