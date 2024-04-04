using NPOI.SS.Formula.Functions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skillbase : MonoBehaviour
{
    public string SkillName = string.Empty;
    public string ShortDescription = string.Empty;
    public string Description = string.Empty;
    public int    SkillLevel = 0;
    public Sprite SkillIcon = null;
    public int maxLevel = 0;
    public int[] price = new int[20];
    public int SkillNumber = 0;

    private void Awake()
    {
        price = new int[20];
    }
}
