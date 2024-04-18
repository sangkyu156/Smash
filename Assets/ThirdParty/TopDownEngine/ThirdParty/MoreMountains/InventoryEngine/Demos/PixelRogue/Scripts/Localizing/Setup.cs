using UnityEngine;

public class Setup : MonoBehaviour
{
    public enum Language
    {
        Korean, Japanese, English
    }

    static public Language language = Language.English;

    public void Korean()
    {
        language = Language.Korean;
    }

    public void English()
    {
        language = Language.English;
    }
}
