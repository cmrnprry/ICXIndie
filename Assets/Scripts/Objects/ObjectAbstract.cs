using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public abstract class ObjectAbstract : MonoBehaviour
{
    [Header("Object Info")]
    [SerializeField]
    public InteractableObject interacton;

    [SerializeField]
    protected TextMeshProUGUI Description_Text;
    protected GameObject Description;

    [Header("Audio")]
    public AudioSource source;

    protected Image outline;
    protected Color next_color;
    protected string OptionChoice;

    public virtual void OnClick()
    {
        GameManager.Instance.PauseInteraction(false);
    }

    /// <summary>
    /// Hover mouse over interactable object
    /// </summary>
    public virtual void OnHoverEnter()
    {
        if (Description != null)
            Description.gameObject.SetActive(true);

        SetObjectColor();
    }

    /// <summary>
    /// mouse leavesinteractable object hover area
    /// </summary>
    public virtual void OnHoverExit()
    {
        if (Description != null)
            Description.gameObject.SetActive(false);

        if (outline.color == Color.yellow)
            SetObjectColor();
    }

    public virtual void SetInteraction()
    {
        Description.gameObject.SetActive(false);
        source.Play();
    }

    //////////////////////////////////////// HELPERS ////////////////////////////////////////////
    public virtual bool IsPreperation()
    {
        return false;
    }
    protected void SetObjectColor()
    {
        var temp_color = outline.color;
        outline.color = next_color;
        next_color = temp_color;
    }
}
