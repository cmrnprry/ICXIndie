using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace AYellowpaper.SerializedCollections
{
    public class MovementObject : ObjectAbstract
    {

        private void Start()
        {
            outline = GetComponent<Image>();
            var newcolor = Color.yellow;
            ColorUtility.TryParseHtmlString("#CFC55D", out newcolor);
            next_color = newcolor;
            next_color.a = 1;
        }
    }
}
