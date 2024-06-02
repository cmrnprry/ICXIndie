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
            next_color = Color.yellow;
            next_color.a = 1;
        }
    }
}
