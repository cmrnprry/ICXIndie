using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.SceneManagement;

namespace AYellowpaper.SerializedCollections
{
    public class MainMenuScript : MonoBehaviour
    {
        [Header("Loading Screen")]
        public GameObject mm;
        public GameObject pause;
        public GameObject game;

        [Header("Transition Screen")]
        public Image TransitionScreen;

        public TMP_Dropdown drop;

        public void ShowScreen(GameObject Menu)
        {
            TransitionScreen.gameObject.SetActive(true);
            TransitionScreen.DOFade(1, 0.25f).OnComplete(() =>
            {
                Menu.SetActive(!Menu.activeSelf);
                TransitionScreen.DOFade(0, 0.25f).OnComplete(() =>
                {
                    TransitionScreen.gameObject.SetActive(false);
                });
            });
        }

        public void Reload()
        {
            SceneManager.LoadScene(0);
        }

        public void FinishGame(GameObject Menu)
        {
            if (GameManager.Instance.Finished)
            {
                TransitionScreen.gameObject.SetActive(true);
                TransitionScreen.DOFade(1, 0.5f).OnComplete(() =>
                {
                    Menu.SetActive(!Menu.activeSelf);
                    TransitionScreen.DOFade(0, 0.5f).OnComplete(() =>
                    {
                        TransitionScreen.gameObject.SetActive(false);
                    });
                });
            }
            else
            {
                GameManager.Instance.Continue();
            }
        }

        public void SetName(int name)
        {
            GameManager.Instance.SetName(drop.options[name].text);
        }

        public void StartGame()
        {
            TransitionScreen.gameObject.SetActive(true);
            TransitionScreen.DOFade(1, 1f).OnComplete(() =>
            {
                mm.SetActive(false);
                game.SetActive(true);
                TransitionScreen.DOFade(0, 1f).OnComplete(() =>
                {
                    TransitionScreen.gameObject.SetActive(false);
                });
            });
        }

        public void QuitGame()
        {
            Application.Quit();
        }
    }
}
