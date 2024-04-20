using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ScreenPanning : MonoBehaviour
{
    public RectTransform Background;
    private int index = 1;

    [SerializeField]
    private GameObject Left, Right;

    public void PanCamera(bool isNegative)
    {
        index += (isNegative) ? -1 : 1;

        switch (index)
        {
            case 0:
                Left.SetActive(false);
                Right.SetActive(true);

                Background.DOAnchorPosX(1920, 1.25f);

                break;
            case 1:
                Left.SetActive(true);
                Right.SetActive(false);

                Background.DOAnchorPosX(0, 1.25f);

                break;

            default:
                Debug.LogError($"Position {index} does not exist");
                break;
        }
    }
}
