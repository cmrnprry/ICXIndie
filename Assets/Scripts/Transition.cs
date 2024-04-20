using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Transition : MonoBehaviour
{
    public Image TransitionScreen;
    public List<GameObject> Screens;

    public void ChangeScreen(GameObject obj)
    {
        TransitionScreen.gameObject.SetActive(true);
        TransitionScreen.DOFade(1, 1f).OnComplete(() =>
        {
            Screens.ForEach(s => s.gameObject.SetActive(false));
            obj.SetActive(true);
            TransitionScreen.DOFade(0, 1f).OnComplete(() =>
            {
                TransitionScreen.gameObject.SetActive(false);
            });
        });
    }
}
