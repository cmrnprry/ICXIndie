using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ActionsPreformed
{
    public static List<AfterText> actions = new List<AfterText>();


    public static void ActionPerformed(InteractableObject obj, int index)
    {
        AfterText temp = obj.CompletionText;
        if (index != -1)
            temp.option = obj.options[index];
        temp.intObject = obj;
        actions.Add(temp);
    }

    public static void ActionPerformed(PrepareableObjects obj)
    {
        string option = "prepared";
    }

    public static bool CheckAction(string name)
    {
        bool found = false;
        foreach (var action in actions)
        {
            if (action.intObject.name.Contains(name))
            {
                found = true;
                break;
            }
        }

        return found;
    }

    //find the buyable object from the chosen object
    public static BuyableItems FindNeed(string name, int index = 0)
    {
        foreach (var action in actions)
        {
            if (action.intObject.name.Contains(name))
            {
                if (action.option.name != "")
                    return action.option.need[index];
            }
        }

        return BuyableItems.Null;
    }

    public static string FindOptionName(string name)
    {
        foreach (var action in actions)
        {
            if (action.intObject.name.Contains(name))
            {
                return action.option.name;
            }
        }

        return "";
    }

    public static int FindOptionIndex(string name)
    {
        foreach (var action in actions)
        {
            if (action.intObject.name.Contains(name))
            {
                return action.option.index;
            }
        }

        return -1;
    }
}