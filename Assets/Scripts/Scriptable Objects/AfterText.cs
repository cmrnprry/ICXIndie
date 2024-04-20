using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Interactable", menuName = "ScriptableObjects/AfterText", order = 1)]
public class AfterText : ScriptableObject
{
    public InteractableObject intObject;

    //if the interactable had an option, store which they chose here
    public Option option;
}
