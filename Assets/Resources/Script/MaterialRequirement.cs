using System;
using UnityEngine;

[Serializable]
public class MaterialRequirement
{
    [Tooltip("Item yang dibutuhkan")]
    public ItemData item;

    [Min(1)]
    [Tooltip("Jumlah item yang dibutuhkan")]
    public int quantity = 1;
}
