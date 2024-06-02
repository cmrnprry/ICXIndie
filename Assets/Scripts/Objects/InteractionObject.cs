using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace AYellowpaper.SerializedCollections
{
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
                Description_Text.text = interacton.InteractionDescription;
                Description = Description_Text.gameObject.transform.parent.gameObject;
            }

            outline = GetComponent<Image>();
            next_color = Color.yellow;
            next_color.a = 1;

            if (useImageOverSprite)
                outline.gameObject.transform.parent.gameObject.GetComponent<Image>().alphaHitTestMinimumThreshold = 0.25f;

            if (useAlphaOverImage)
                outline.alphaHitTestMinimumThreshold = 0.8f;

            if (interacton.completed)
                SetInteraction();

        }

        /// <summary>
        /// Click interactable Object
        /// </summary>
        public override void OnClick()
        {
            if (Option_Parent.gameObject.activeSelf || interacton.completed)
                return;

            //if this action requaires you to buy/prepare something
            if (interacton.Need_to_Buy || interacton.Need_to_Prepare && !interacton.name.Contains("Treats"))
            {
                bool buy = interacton.Need_to_Buy;
                bool prepare = interacton.Need_to_Prepare;

                foreach (var item in interacton.items)
                {
                    if (interacton.Need_to_Buy && item.bought)
                        buy = false;

                    if (interacton.Need_to_Prepare && item.prepared)
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

            if (interacton.options != null && interacton.options.Count > 0)
            {
                DisplayOptions();
            }
            else
            {
                GameManager.Instance.TimeReminder(interacton, this);
            }

            base.OnClick();
        }

        /// <summary>
        /// Hover mouse over interactable object
        /// </summary>
        public override void OnHoverEnter()
        {
            if (interacton.completed)
                return;

            base.OnHoverEnter();
        }

        /// <summary>
        /// mouse leavesinteractable object hover area
        /// </summary>
        public override void OnHoverExit()
        {
            if (Option_Parent.gameObject.activeSelf || interacton.completed || !outline.raycastTarget)
                return;

            foreach (Transform child in Option_Parent)
            {
                Destroy(child.gameObject);
            }

            base.OnHoverExit();
        }


        public override void SetInteraction()
        {
            if (interacton.name.Contains("Bear"))
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
                interacton.completed = true;
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

            interacton.completed = true;
            Description.gameObject.SetActive(false);
        }


        /// <summary>
        /// Display options if the interaction has any
        /// </summary>
        private void DisplayOptions()
        {
            Description.gameObject.SetActive(true);
            bool can_preform = true;

            foreach (Option option in interacton.options)
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
            GameManager.Instance.TimeReminder(interacton, this, index);
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

            foreach (var i in interacton.items)
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

            foreach (var i in interacton.items)
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
            if (interacton.items.Count <= 0)
                return;

            for (index = 0; index < interacton.items.Count; index++)
            {
                if (interacton.items[index].item.ToString() == item.ToString())
                {
                    var ii = interacton.items[index];
                    ii.bought = true;
                    interacton.items[index] = ii;

                    //ONLY treats have individual buying needs, others need you to buy everything
                    if (interacton.name.Contains("Treats"))
                        ItemsBought[index].SetActive(true);
                }

                if (!interacton.items[index].bought)
                    all_bought = false;
            }


            if (all_bought)
                ItemsBought[0].SetActive(true);
        }

        public void SetPrepare(PrepareableObjects prepared)
        {
            int index = 0;
            if (interacton.items.Count <= 0)
                return;

            for (index = 0; index < interacton.items.Count; index++)
            {
                if (prepared.preparedItem.Contains(interacton.items[index].item))
                {
                    var ii = interacton.items[index];
                    ii.prepared = true;
                    interacton.items[index] = ii;
                }
            }
        }

    }
}