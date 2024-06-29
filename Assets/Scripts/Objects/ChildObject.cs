using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace AYellowpaper.SerializedCollections
{
    public class ChildObject : ObjectAbstract
    {
        public static bool FedChocolate, FedWatermelon, WatermelonPrepared;
        public static bool isMess;
        private int BoredMeter = 0;
        private GameObject CurrentImage, NextImage;
        public GameObject Options;
        public List<Image> Outlines;
        public List<Image> Mess;
        public List<Image> Eat;
        public List<Image> Clickable;

        public List<AudioClip> blender, watermelon;
        public List<AudioClip> bored_sounds;

        [Header("Buttons")]
        public Button Watermelon;
        public TextMeshProUGUI WatermelonText;

        private void OnEnable()
        {
            PrepareObject.OnPrepareItem += ChildBoredMeter;
            PrepareObject.OnWaterMelonPrepared += WaterMelonPrepared;
        }

        private void OnDisable()
        {
            PrepareObject.OnPrepareItem -= ChildBoredMeter;
            PrepareObject.OnWaterMelonPrepared -= WaterMelonPrepared;
        }


        private void Start()
        {
            Description = Description_Text.gameObject.transform.parent.gameObject;
            outline = Outlines[0];

            var newcolor = Color.yellow;
            ColorUtility.TryParseHtmlString("#CFC55D", out newcolor);
            next_color = newcolor;

            next_color.a = 1;
            outline.gameObject.transform.parent.gameObject.GetComponent<Image>().alphaHitTestMinimumThreshold = 0.5f;

            CurrentImage = Outlines[BoredMeter].gameObject.transform.parent.gameObject;
        }

        public override void OnClick()
        {
            if (FedChocolate && FedWatermelon)
                return;

            Options.SetActive(true);

            base.OnClick();
        }

        /// <summary>
        /// Hover mouse over interactable object
        /// </summary>
        public override void OnHoverEnter()
        {
            if (FedChocolate && FedWatermelon)
                return;

            if (isMess)
                Description_Text.text = "I have to have something to occupy @ while I clean.";
            else
                Description_Text.text = "Can chat for a bit or give @ a treat.";

            Description.gameObject.SetActive(true);

            if (outline.color == Color.black)
                SetObjectColor();
        }

        /// <summary>
        /// mouse leavesinteractable object hover area
        /// </summary>
        public override void OnHoverExit()
        {
            if ((FedChocolate && FedWatermelon) || Description.gameObject.activeSelf)
                return;

            base.OnHoverExit();
        }

        public void FlipInteration(bool isOn)
        {
            foreach (Image item in Clickable)
            {
                item.raycastTarget = !isOn;
            }
        }

        private void CauseMess()
        {
            //check what mess to make
            isMess = true;
            var mess = 1;
            source.clip = blender[0];

            //if watermelon is NOT prepared, mess with it
            if (!WatermelonPrepared)
            {
                mess = 0;
                source.clip = watermelon[0];
            }

            //Set temp outline
            var temp_outline = Mess[mess];

            if (mess == 1)
                Mess[2].gameObject.SetActive(false);

            source.PlayDelayed(0.15f);
            FadeChildImage(temp_outline, true);
        }

        public void FeedChild(int isChocolate)
        {
            if (isChocolate == 0)
            {
                FedChocolate = true;
                GameManager.FedChocolate = true;
                source.clip = blender[1];
            }
            else
            {
                FedWatermelon = true;
                source.clip = watermelon[1];
            }

            isMess = false;
            BoredMeter = 0;

            //Set temp outline
            var temp_outline = Eat[isChocolate];

            source.PlayDelayed(0.15f);
            FadeChildImage(temp_outline, true);
        }

        public void ChildBoredMeter()
        {
            if (BoredMeter == 2)
            {
                CauseMess();
                return;
            }

            //Iincrease bored meter
            BoredMeter += 1;

            //Set temp outline
            var temp_outline = Outlines[BoredMeter];

            FadeChildImage(temp_outline, false);
        }

        private void FadeChildImage(Image temp_outline, bool isMess)
        {
            RaycastDetection(false, temp_outline);
            Sequence mySequence = DOTween.Sequence();

            //Set next image
            NextImage = temp_outline.gameObject.transform.parent.gameObject;
            NextImage.SetActive(true);

            //Turn off outline of current image
            CurrentImage.transform.GetChild(0).gameObject.SetActive(false);

            //Fade current image out and next image in

            if (!isMess)
            {
                int index = (Random.Range(0, 3));
                if (index <= 1)
                    source.PlayOneShot(bored_sounds[index]);
            }
            mySequence.Append(CurrentImage.GetComponent<Image>().DOFade(0, .5f)).Insert(0.1f, NextImage.GetComponent<Image>().DOFade(1, .5f)).OnComplete(() =>
            {
                //turn off current (last) image
                CurrentImage.SetActive(false);

                //Set the temp outline to current and turn it on
                outline = temp_outline;
                outline.gameObject.SetActive(true);

                var newcolor = Color.yellow;
                ColorUtility.TryParseHtmlString("#CFC55D", out newcolor);

                //Ensure the outline colors are correct
                next_color = newcolor;
                outline.color = Color.black;

                //Set next to bve current
                CurrentImage = NextImage;

                //Turn Raycast back on
                RaycastDetection(true);

                //play sound

            });
        }

        private void RaycastDetection(bool isOn, Image second = null)
        {
            outline.raycastTarget = isOn;

            if (second != null)
                second.raycastTarget = isOn;

            print("raycast off");
        }

        public void WaterMelonPrepared()
        {
            var parent = Options.transform;
            Watermelon.interactable = true;
            WatermelonText.text = "Feed Watermelon";
            WatermelonPrepared = true;
        }

        public void CloseInteraction()
        {
            Options.SetActive(false);
            GameManager.Instance.CancelAction();
            OnHoverExit();
        }
    }
}