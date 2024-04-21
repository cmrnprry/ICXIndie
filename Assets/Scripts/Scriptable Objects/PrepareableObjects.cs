using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Interactable", menuName = "ScriptableObjects/PrepareableObject", order = 2)]
public class PrepareableObjects : ScriptableObject
{
    public int Time_to_do;
    public List<BuyableItems> preparedItem;
    public bool completed;

    [TextAreaAttribute(1, 2)]
    public string PreparationDescription; //what the intereaction does on prepare
}
