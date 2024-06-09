using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace AYellowpaper.SerializedCollections
{
    public class BuyObject : ObjectAbstract
    {
        public Image BoughtItem; //Objects to turn on/off
        public Image Object; //Objects to turn on/off
        private Sequence mySequence;
        public BuyableItems Item;

        private void Start()
        {
            outline = GetComponent<Image>();

            var newcolor = Color.yellow;
            ColorUtility.TryParseHtmlString("#CFC55D", out newcolor);
            next_color = newcolor;

            next_color.a = 1;
            mySequence = DOTween.Sequence();
        }

        public override void OnClick()
        {
            if (StoreManager.Instance.BabySpeech_Parent.childCount >= 10)
            {
                StoreManager.Instance.SpawnParentMessage("I need to calm the baby first.");
                return;
            }

            if ((Item != BuyableItems.JunkFood && Item != BuyableItems.Default) && GameManager.Instance.HasBought(Item))
            {
                StoreManager.Instance.SpawnParentMessage("I already have this.");
                return;
            }

            mySequence = DOTween.Sequence();
            outline.gameObject.SetActive(false);
            mySequence.Append(Object.GetComponent<Image>().DOFade(0, .85f)).Insert(0.1f, BoughtItem.DOFade(1, .85f)).OnComplete(() =>
            {
                //turn off current (last) image
                Object.gameObject.SetActive(false);
                StoreManager.Instance.BuyItem(Item);
            });
        }

        /// <summary>
        /// Hover mouse over interactable object
        /// </summary>
        public override void OnHoverEnter()
        {
            base.OnHoverEnter();
        }

        /// <summary>
        /// mouse leavesinteractable object hover area
        /// </summary>
        public override void OnHoverExit()
        {
            base.OnHoverExit();
        }
    }
}