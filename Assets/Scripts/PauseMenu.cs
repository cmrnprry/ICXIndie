using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AYellowpaper.SerializedCollections
{
    public class PauseMenu : MonoBehaviour
    {
        public GameObject pause;
        private static bool isPaused = false;

        public void OpenClosePause()
        {
            pause.SetActive(!pause.activeSelf);
            isPaused = pause.activeSelf;
        }

        public static bool Pause()
        {
            return isPaused;
        }

        public static void SetPause(bool p)
        {
            isPaused = p;
        }


        // Update is called once per frame
        void Update()
        {
            if ((Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P)) && GameManager.CanPause)
            {
                OpenClosePause();
            }
        }
    }
}