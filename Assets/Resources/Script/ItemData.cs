using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New Item", menuName = "Blacksmith/Item")]

public class ItemData : ScriptableObject
{
    public JenisBarangEnum itemType;
    public int quantity;

}
