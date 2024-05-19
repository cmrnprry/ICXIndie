using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NightInteraction : ObjectAbstract
{
    public TextMeshProUGUI Parent_Text;
    // Start is called before the first frame update
    void Start()
    {
        if (Description_Text != null && interacton != null)
        {
            Description_Text.text = interacton.InteractionDescription;
            Description = Description_Text.gameObject.transform.parent.gameObject;
        }
        outline = GetComponent<Image>();
        next_color = Color.yellow;
        next_color.a = 1;
    }

    public void OnClick(int flag)
    {
        bool wiggle = (GameManager.Instance.childTired >= 10) ? true : false;
        string text = "";

        if (!GameManager.Instance.wearingGloves)
        {
            text = $"Sonuvah- {GameManager.Instance.ChildName}, that <i>hurt</i>!";

            if (!wiggle || flag == 4)
                text += " Ack, but least I got him.";
            else
                text += $" {GameManager.Instance.ChildName}- Hey! Stop- wiggling!";

        }
        else
        {
            if (!wiggle || flag == 4)
                text += "Got him!";
            else
                text += $"{GameManager.Instance.ChildName}- Hey! Stop- wiggling!";

        }

        Parent_Text.gameObject.transform.parent.gameObject.SetActive(true);
        Parent_Text.text = text;


        StartCoroutine(GameManager.Instance.LastBit(flag));
    }
}
