using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ObjectSwitcher : MonoBehaviour
{
    public List<Transform> objects; // 3 objek anak (child dari satu parent)
    public Vector3 leftPos, centerPos, rightPos; // localPositions
    public Vector3 normalScale = Vector3.one;
    public Vector3 bigScale = new Vector3(1.5f, 1.5f, 1.5f);

    public Button btnLeft;
    public Button btnRight;

    private int centerIndex = 1; // objek yang di tengah
    private bool inputHeld = false; // Untuk mencegah switch berulang saat tombol ditekan terus

    void Start()
    {
        btnLeft.onClick.AddListener(SwitchLeft);
        btnRight.onClick.AddListener(SwitchRight);
        UpdatePositionAndScale();
    }

    void SwitchLeft()
    {
        centerIndex = (centerIndex + 2) % 3; 
        UpdatePositionAndScale();
    }

    void SwitchRight()
    {
        centerIndex = (centerIndex + 1) % 3; 
        UpdatePositionAndScale();
    }

   void UpdatePositionAndScale()
{
    for (int i = 0; i < 3; i++)
    {
        int posIndex = (i - centerIndex + 3) % 3;

        Vector3 targetPos = centerPos;
        Vector3 targetScale = normalScale;

        if (posIndex == 0) targetPos = leftPos;
        else if (posIndex == 1)
        {
            targetPos = centerPos;
            targetScale = bigScale;
        }
        else if (posIndex == 2) targetPos = rightPos;

        LeanTween.moveLocal(objects[i].gameObject, targetPos, 0.5f).setEase(LeanTweenType.easeInOutSine);
        LeanTween.scale(objects[i].gameObject, targetScale, 0.5f).setEase(LeanTweenType.easeInOutSine);

      
    }
}


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            SwitchRight();
            inputHeld = true;
        }
        else if (Input.GetKeyDown(KeyCode.Q))
        {
            SwitchLeft();
            inputHeld = true;
        }
        else if (Input.GetKeyUp(KeyCode.E) || Input.GetKeyUp(KeyCode.Q))
        {
            inputHeld = false;
        }
    }
}
