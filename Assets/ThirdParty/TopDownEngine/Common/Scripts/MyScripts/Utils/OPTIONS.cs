using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OPTIONS : MonoBehaviour
{
    public Setup setup;
    public GameObject OptionsPopup;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OptionsButton()
    {
        OptionsPopup.SetActive(true);
    }

    public void CloseButton()
    {
        OptionsPopup.SetActive(false);
    }

    public void KoreanSelect()
    {
        TextUtil.languageNumber = 0;
        //Setup.language = Setup.Language.Korean;
    }

    public void EnglishSelect()
    {
        TextUtil.languageNumber = 2;
        //Setup.language = Setup.Language.English;
    }
}
