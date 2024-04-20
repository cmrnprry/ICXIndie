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
            next_color = Color.yellow;
            next_color.a = 1;
            mySequence = DOTween.Sequence();
            //outline.gameObject.transform.parent.gameObject.GetComponent<Image>().alphaHitTestMinimumThreshold = 0.25f;
        }

        public override void OnClick()
        {
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