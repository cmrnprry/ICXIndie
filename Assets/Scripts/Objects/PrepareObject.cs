using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PrepareObject : ObjectAbstract
{
    private PrepareableObjects prepare;
    //public GameObject isMessy;
    public int index = 0;
    public List<GameObject> Objects; //Objects to turn on/off

    public delegate void ClickAction();
    public static event ClickAction OnPrepareItem;
    public static event ClickAction OnWaterMelonPrepared;

    private void Start()
    {
        prepare = obj.preparable_info[index];

        Description_Text.text = prepare.PreparationDescription;
        Description = Description_Text.gameObject.transform.parent.gameObject;

        outline = GetComponent<Image>();
        next_color = Color.yellow;
        next_color.a = 1;
        outline.gameObject.transform.parent.gameObject.GetComponent<Image>().alphaHitTestMinimumThreshold = 0.25f;
    }

    public override void OnClick()
    {
        if (prepare.completed || ChildObject.isMess)
            return;

        GameManager.Instance.TimeReminder(obj, this, index);
        base.OnClick();
    }

    /// <summary>
    /// Hover mouse over interactable object
    /// </summary>
    public override void OnHoverEnter()
    {
        if (prepare.completed)
            return;

        if (ChildObject.isMess)
            Description_Text.text = "I have clean this up before he makes a bigger mess";
        else
            Description_Text.text = prepare.PreparationDescription;

        base.OnHoverEnter();
    }

    /// <summary>
    /// mouse leavesinteractable object hover area
    /// </summary>
    public override void OnHoverExit()
    {
        if (prepare.completed)
            return;

        base.OnHoverExit();
    }

    public override void SetInteraction()
    {
        //set prepare to true
        prepare.completed = true;
        
        if (obj.name.ToLower().Contains("treats"))
        {
            var temp = obj.items[index];
            temp.prepared = true;
            obj.items[index] = temp;
        }
        else
            obj.Need_to_Prepare = false;

        if (prepare.name.Contains("Watermelon"))
            OnWaterMelonPrepared.Invoke();

        OnPrepareItem.Invoke();

        foreach (GameObject o in Objects)
        {
            o.SetActive(!o.activeSelf);
        }

        outline.gameObject.transform.parent.gameObject.SetActive(false);
    }

    //////////////////////////////////////// HELPERS ////////////////////////////////////////////
    public override bool IsPreperation()
    {
        return true;
    }

    public void CloseInteraction()
    {
        GameManager.Instance.CancelAction();
        OnHoverExit();
    }
}
