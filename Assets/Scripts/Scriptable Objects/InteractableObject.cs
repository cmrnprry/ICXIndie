using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Interactable", menuName = "ScriptableObjects/InteractableObject", order = 1)]
public class InteractableObject : ScriptableObject
{
    public int Time_to_do;
    public bool completed;
    public bool Need_to_Buy = false;
    public bool Need_to_Prepare = false;
    public List<PrepareableObjects> preparable_info;

    [TextAreaAttribute(1, 2)]
    public string InteractionDescription; //what the intereaction does

    public List<Option> options; //if the interaction has options of what to do
    public List<Item> items; //if the interaction has items needed to get 
    public AfterText CompletionText;
}

[System.Serializable]
public struct Option
{
    [TextAreaAttribute(1, 2)]
    public string name;

    [TextAreaAttribute(2, 3)]
    public string value;

    public List<BuyableItems> need; //items needed to preform

    public int index; //used to grab which option used if needed
}

[System.Serializable]
public struct Item
{
    public BuyableItems item;

    public bool bought;
    public bool prepared;
}