using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InteractionObject : ObjectAbstract
{
    [Header("Object Info")]
    public List<GameObject> ItemsBought;

    [Header("On Completed")]
    [SerializeField]
    private Sprite added;
    [SerializeField]
    private GameObject image;
    [SerializeField]
    private bool useImageOverSprite = false;
    [SerializeField]
    private bool useAlphaOverImage = false;

    [Header("Options")]
    [SerializeField]
    private GameObject Option;
    [SerializeField]
    private Transform Option_Parent;




    private void Start()
    {
        if (Description_Text != null)
        {
            Description_Text.text = obj.InteractionDescription;
            Description = Description_Text.gameObject.transform.parent.gameObject;
        }
        
        outline = GetComponent<Image>();
        next_color = Color.yellow;
        next_color.a = 1;

        if (useImageOverSprite)
            outline.gameObject.transform.parent.gameObject.GetComponent<Image>().alphaHitTestMinimumThreshold = 0.25f;

        if (useAlphaOverImage)
            outline.alphaHitTestMinimumThreshold = 0.8f;

    }

    /// <summary>
    /// Click interactable Object
    /// </summary>
    public override void OnClick()
    {
        if (Option_Parent.gameObject.activeSelf || obj.completed)
            return;

        //if this action requaires you to buy/prepare something
        if (obj.Need_to_Buy || obj.Need_to_Prepare && !obj.name.Contains("Treats"))
        {
            bool buy = obj.Need_to_Buy;
            bool prepare = obj.Need_to_Prepare;

            foreach (var item in obj.items)
            {
                if (obj.Need_to_Buy && item.bought)
                    buy = false;

                if (obj.Need_to_Prepare && item.prepared)
                    prepare = false;
            }

            if (buy && prepare)
            {
                Description_Text.text = "You need to buy and prepare supplies before doing this.";
                return;
            }
            else if (buy)
            {
                Description_Text.text = "You need to buy supplies before doing this.";
                return;
            }
            else if (prepare)
            {
                Description_Text.text = "You need to prepare supplies before doing this.";
                return;
            }
        }

        if (obj.options != null && obj.options.Count > 0)
        {
            DisplayOptions();
        }
        else
        {
            GameManager.Instance.TimeReminder(obj, this);
        }

        base.OnClick();
    }

    /// <summary>
    /// Hover mouse over interactable object
    /// </summary>
    public override void OnHoverEnter()
    {
        if (obj.completed)
            return;

        base.OnHoverEnter();
    }

    /// <summary>
    /// mouse leavesinteractable object hover area
    /// </summary>
    public override void OnHoverExit()
    {
        if (Option_Parent.gameObject.activeSelf || obj.completed || !outline.raycastTarget)
            return;

        foreach (Transform child in Option_Parent)
        {
            Destroy(child.gameObject);
        }

        base.OnHoverExit();
    }


    public override void SetInteraction()
    {
        if (obj.name.Contains("Bear"))
        {
            if (GameManager.Instance.Index == 0)
            {
                outline.gameObject.SetActive(false);
                image.gameObject.SetActive(true);
            }
            else
            {
                outline.sprite = added;
                outline.color = Color.white;
            }
            obj.completed = true;
            Description.gameObject.SetActive(false);

            return;
        }

        if (!useImageOverSprite)
        {
            outline.sprite = added;
            outline.color = Color.white;
        }
        else
        {
            outline.gameObject.SetActive(false);
            image.gameObject.SetActive(true);
            this.transform.parent.gameObject.GetComponent<Image>().enabled = false;
        }

        obj.completed = true;
        Description.gameObject.SetActive(false);
    }


    /// <summary>
    /// Display options if the interaction has any
    /// </summary>
    private void DisplayOptions()
    {
        Description.gameObject.SetActive(true);
        bool can_preform = true;

        foreach (Option option in obj.options)
        {
            can_preform = true;

            if (option.need != null)
            {
                foreach (BuyableItems n in option.need)
                {
                    if ((!HasBoughtObject(n)) || (!HasPreparedObject(n)))
                        can_preform = false;
                }
            }

            var op = Instantiate(Option, Option_Parent, false);
            var button = op.transform.GetChild(0).GetComponent<Button>();
            var text = button.gameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            var desc = op.transform.GetChild(1).transform.GetChild(0).GetComponent<TextMeshProUGUI>();

            if (can_preform)
                button.onClick.AddListener(delegate
                { ClickOptionPossible(option.index); });
            else
                button.interactable = false;

            text.text = option.name;

            var str = (can_preform) ? option.value : "You need to buy/prepare supplies before doing this.";
            desc.text = str;
        }

        if (Option_Parent.childCount > 0)
            Option_Parent.gameObject.SetActive(true);
    }

    private void ClickOptionPossible(int index)
    {
        GameManager.Instance.TimeReminder(obj, this, index);
        Option_Parent.gameObject.SetActive(false);
    }

    public void RaycastDetection(bool isOn)
    {
        if (outline != null)
            outline.raycastTarget = isOn;
    }


    //////////////////////////////////////// HELPERS ////////////////////////////////////////////

    public void CloseInteraction()
    {
        Option_Parent.gameObject.SetActive(false);
        GameManager.Instance.CancelAction();
        OnHoverExit();
    }

    /// <summary>
    /// Checks if an object was prepared
    /// </summary>
    /// <param name="item">item that we are checking against</param>
    /// <returns></returns>
    private bool HasPreparedObject(BuyableItems item)
    {
        bool isPossible = false;

        foreach (var i in obj.items)
        {
            if (i.item.ToString() == item.ToString())
            {
                isPossible = i.prepared;
            }
        }

        return isPossible;
    }

    /// <summary>
    /// Checks if an object was bought
    /// </summary>
    /// <param name="item">item that we are checking against</param>
    /// <returns></returns>
    private bool HasBoughtObject(BuyableItems item)
    {
        bool isPossible = false;

        foreach (var i in obj.items)
        {
            if (i.item.ToString() == item.ToString())
            {
                isPossible = i.bought;
            }
        }

        return isPossible;
    }

    public void SetBuy(BuyableItems item)
    {
        int index = 0;
        bool all_bought = true;
        for (index = 0; index < obj.items.Count; index++)
        {
            if (obj.items[index].item.ToString() == item.ToString())
            {
                var ii = obj.items[index];
                ii.bought = true;
                obj.items[index] = ii;
            }

            if (!obj.items[index].bought)
                all_bought = false;
        }

        //ONLY treats have individual buying needs, others need you to buy everything
        if (obj.name.Contains("Treats"))
            ItemsBought[index].SetActive(true);
        else if (all_bought)
            ItemsBought[0].SetActive(true);
    }
}
