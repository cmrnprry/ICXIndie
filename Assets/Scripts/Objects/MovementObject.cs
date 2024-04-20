using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MovementObject : ObjectAbstract
{

    private void Start()
    {
        outline = GetComponent<Image>();
        next_color = Color.yellow;
        next_color.a = 1;
    }
}
