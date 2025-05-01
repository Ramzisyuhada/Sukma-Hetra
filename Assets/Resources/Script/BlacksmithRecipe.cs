using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;





[CreateAssetMenu(fileName = "PandaiBesi", menuName = "Recipe/PandaiBesi")]
public class BlacksmithRecipe : ScriptableObject
{
    public List<MaterialRequirement> requiredMaterials;
    public ItemData resultItem;
    public GameObject resultPrefab;
}
