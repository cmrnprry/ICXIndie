using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioScript : MonoBehaviour
{
    public AudioMixer mixer;
   public void AdjustMaster(float value)
    {
        mixer.SetFloat("master", Mathf.Log10(value) * 20);
    }

    public void AdjustBGM(float value)
    {
        mixer.SetFloat("bgm", Mathf.Log10(value) * 20);
    }

    public void AdjustSFX(float value)
    {
        mixer.SetFloat("sfx", Mathf.Log10(value) * 20);
    }
}
