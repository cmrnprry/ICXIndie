using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace AYellowpaper.SerializedCollections
{
    public class PrepareObject : ObjectAbstract
    {
        public PrepareableObjects prepare;
        //public GameObject isMessy;
        public int index = 0;
        public List<GameObject> Objects; //Objects to turn on/off

        public delegate void ClickAction();
        public static event ClickAction OnPrepareItem;
        public static event ClickAction OnWaterMelonPrepared;

        private void Start()
        {
            Description_Text.text = prepare.PreparationDescription;
            Description = Description_Text.gameObject.transform.parent.gameObject;

            outline = GetComponent<Image>();

            var newcolor = Color.yellow;
            ColorUtility.TryParseHtmlString("#CFC55D", out newcolor);
            next_color = newcolor;

            next_color.a = 1;
            outline.gameObject.transform.parent.gameObject.GetComponent<Image>().alphaHitTestMinimumThreshold = 0.25f;
        }

        public override void OnClick()
        {
            if (prepare.completed || ChildObject.isMess)
                return;

            GameManager.Instance.TimeReminder(interacton, this, index);
            base.OnClick();
        }

        /// <summary>
        /// Hover mouse over interactable object
        /// </summary>
        public override void OnHoverEnter()
        {
            if (prepare.completed)
                return;

            if (ChildObject.isMess)
                Description_Text.text = "I have clean this up before he makes a bigger mess";
            else
                Description_Text.text = prepare.PreparationDescription;

            base.OnHoverEnter();
        }

        /// <summary>
        /// mouse leavesinteractable object hover area
        /// </summary>
        public override void OnHoverExit()
        {
            if (prepare.completed)
                return;

            base.OnHoverExit();
        }

        public override void SetInteraction()
        {
            //set prepare to true
            prepare.completed = true;
            if (source.clip != null)
                source.Play();

            if (prepare.name.Contains("Watermelon"))
                OnWaterMelonPrepared.Invoke();

            OnPrepareItem.Invoke();
            outline.enabled = false;

            Sequence mySequence = DOTween.Sequence();
            mySequence.Append(this.gameObject.transform.parent.gameObject.GetComponent<Image>().DOFade(0, 0.55f)).OnComplete(() =>
            {
                outline.gameObject.transform.parent.gameObject.SetActive(false);
                foreach (GameObject o in Objects)
                {
                    o.SetActive(!o.activeSelf);
                }
            });
        }

        //////////////////////////////////////// HELPERS ////////////////////////////////////////////
        public override bool IsPreperation()
        {
            return true;
        }

        public void CloseInteraction()
        {
            GameManager.Instance.CancelAction();
            OnHoverExit();
        }
    }
}
