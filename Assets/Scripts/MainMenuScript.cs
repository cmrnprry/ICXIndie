using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.SceneManagement;
using System;

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

        public TMP_Dropdown Name_Drop, Noun_Drop, ScreenType_Drop, ScreenSize_Drop;

        public void ShowScreen(GameObject Menu)
        {
            TransitionScreen.gameObject.SetActive(true);
            TransitionScreen.DOFade(1, 0.15f).OnComplete(() =>
            {
                Menu.SetActive(!Menu.activeSelf);
                TransitionScreen.DOFade(0, 0.15f).OnComplete(() =>
                {
                    TransitionScreen.gameObject.SetActive(false);
                    GameManager.CanPause = CanPause();
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
            GameManager.Instance.SetName(Name_Drop.options[name].text);
        }

        public void SetPronouns(int noun)
        {
            GameManager.Instance.SetChildPronoun(Noun_Drop.options[noun].text);
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
                    GameManager.CanPause = CanPause();
                });
            });
        }

        public void QuitGame()
        {
            Application.Quit();
        }

        public bool CanPause()
        {
            if (!mm.activeSelf)
                return true;

            return false;
        }

        public void SetScreenSize(int size)
        {
            string screen = ScreenSize_Drop.options[size].text;
            var array = screen.Split(' ');
            int width = Int32.Parse(array[0]);
            int height = Int32.Parse(array[2]);
            bool isFull = (Screen.fullScreenMode == FullScreenMode.FullScreenWindow) ? true : false;
            Screen.SetResolution(width, height, isFull, Screen.currentResolution.refreshRate);
        }

        public void SetScreenType(int type)
        {
            string screen = ScreenType_Drop.options[type].text;
            switch (screen)
            {
                case "Full Screen":
                    Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                    break;
                case "Full Screen Windowed":
                    Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                    break;
                case "Windowed":
                    Screen.fullScreenMode = FullScreenMode.Windowed;
                    break;
            }

            
        }
    }
}
