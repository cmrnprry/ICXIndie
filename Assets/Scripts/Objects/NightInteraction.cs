using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NightInteraction : ObjectAbstract
{
    // Start is called before the first frame update
    void Start()
    {
        if (Description_Text != null)
        {
            Description_Text.text = interacton.InteractionDescription;
            Description = Description_Text.gameObject.transform.parent.gameObject;
        }
        outline = GetComponent<Image>();
        next_color = Color.yellow;
        next_color.a = 1;
    }

    public override void OnClick()
    {
        var parent = GameObject.Find("NightYard").transform;

        if (!parent.GetChild(0).gameObject.activeSelf)
        {
            //player gets bit and baby escapes
            Description_Text.text = "Ouch! He bit me!";
        }
        else
        {
            //if tired = baby captured
            if (GameManager.Instance.childTired >= 10)
                Description_Text.text = "Got him.";
            else
                Description_Text.text = "He wiggled out of my grip and escaped!";
            //if not == baby escaped
        }
    }
}
