using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;




[Serializable]

[CreateAssetMenu(fileName = "PandaiBesi", menuName = "Recipe/PandaiBesi")]
public class BlacksmithRecipe : ScriptableObject
{
    public string recipeName;
    public Ingredient[] requiredMaterials;
    public ItemData resultItem;
    public int resultAmount = 1;
}
