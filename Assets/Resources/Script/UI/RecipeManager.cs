using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecipeManager : MonoBehaviour
{

    [SerializeField] private MeshRenderer _Mesh;

    [SerializeField] List<Sprite> _Sprites;

    private int _currentIndex = 0; // Simpan index sekarang

    private void Start()
    {
        if (_Sprites.Count > 0)
        {
            UpdateSprite();
        }
    }

    // Fungsi untuk Next
    public void Next()
    {
        if (_Sprites.Count == 0) return;

        _currentIndex++;
        if (_currentIndex >= _Sprites.Count)
        {
            _currentIndex = 0; // Kembali ke awal kalau sudah mentok
        }

        UpdateSprite();
    }

    // Fungsi untuk Prev
    public void Prev()
    {
        if (_Sprites.Count == 0) return;

        _currentIndex--;
        if (_currentIndex < 0)
        {
            _currentIndex = _Sprites.Count - 1; // Ke terakhir kalau sudah di awal
        }

        UpdateSprite();
    }

    private void UpdateSprite()
    {
        _Mesh.material.mainTexture = _Sprites[_currentIndex].texture;
    }
}
