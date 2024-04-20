using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using DG.Tweening;
using Random = UnityEngine.Random;


namespace AYellowpaper.SerializedCollections
{
    public enum StoreActions
    {
        Any, Jump, Cry, Climb, Hang, Reach
    }
    public class StoreManager : MonoBehaviour
    {
        private Sequence mySequence;

        [SerializeField]
        public int BabyMeter = 0; //will track how annoying baby is being

        [Header("Baby")]
        [SerializedDictionary("Description", "Child Actions")]
        public SerializedDictionary<string, StoreActions> BabyWords;
        public RectTransform BabySpeech_Parent;
        public GameObject Baby_Bubbles;
        private float SpawnTime = 15f;
        private StoreActions CurrentAction = StoreActions.Reach;


        [Header("Parent")]
        [TextAreaAttribute(1, 2)]
        public List<string> AdultWords;
        public TextMeshProUGUI ParentTextBox;
        private float time = 3f;

        [Header("Shopping List")]
        public GameObject List_Item;
        public Transform ListParent;

        [Header("Switch Asiles")]
        public TextMeshProUGUI Asile_Type, Switch_Left, Switch_Right;
        public Image Produce, Grocery, Hardware;
        private List<TextMeshProUGUI> ShoppingList;

        public static StoreManager Instance { get; private set; }

        private void Awake()
        {
            // If there is an instance, and it's not me, delete myself.

            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }
        }

        private void Start()
        {
            mySequence = DOTween.Sequence();
        }

        private void OnDisable()
        {
            DepopulateList();
        }

        private void OnEnable()
        {
            ShoppingList = new List<TextMeshProUGUI>();
            PopulateList();
            StartCoroutine(SpawnTimer());
        }

        private void Update()
        {
            if (BabyMeter >= 100)
                BabyMeter = 100;

            if (BabyMeter <= 0)
                BabyMeter = 0;

            if (BabyMeter >= 100)
            {
                SpawnTime = 0.5f;
            }
            else if (BabyMeter >= 75)
            {
                SpawnTime = 1f;
            }
            else if (BabyMeter >= 50)
            {
                SpawnTime = 2.5f;
            }
            else if (BabyMeter >= 15)
            {
                SpawnTime = 4f;
            }
        }

        public Vector2 GetRandomPositionInBounds()
        {
            Vector3[] bounds = new Vector3[4];
            BabySpeech_Parent.GetWorldCorners(bounds);

            var randPosX = Random.Range(bounds[0].x, bounds[2].x);
            var randPosY = Random.Range(bounds[0].y, bounds[1].y);

            return new Vector2(randPosX, randPosY);
        }

        private IEnumerator SpawnTimer()
        {
            yield return new WaitForSeconds(SpawnTime);

            int rand = Random.Range(0, BabyWords.Count - 1);
            //string text = BabyWords[rand];

            GameObject obj = Instantiate(Baby_Bubbles, GetRandomPositionInBounds(), Quaternion.identity, BabySpeech_Parent);
            //obj.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = text;

            if (BabySpeech_Parent.childCount >= 12)
                BabyMeter += 15;
            else if (BabySpeech_Parent.childCount >= 7)
                BabyMeter += 10;
            else if (BabySpeech_Parent.childCount >= 5)
                BabyMeter += 5;

            StartCoroutine(SpawnTimer());
        }

        public void SpawnParentMessage(string message = "")
        {
            int rand = Random.Range(0, AdultWords.Count - 1);
            ParentTextBox.text = (message == "") ? AdultWords[rand] : message;

            ParentTextBox.gameObject.transform.parent.gameObject.SetActive(true);

            BabyMeter -= 10;

            StartCoroutine(CloseParentMessage());
        }

        private IEnumerator CloseParentMessage()
        {
            yield return new WaitForSeconds(time);
            ParentTextBox.gameObject.transform.parent.gameObject.SetActive(false);
            time = 3f;
        }

        private void PopulateList()
        {
            foreach (Transform child in ListParent)
            {
                var text = child.gameObject.GetComponent<TextMeshProUGUI>();
                var button = child.gameObject.GetComponent<Button>();

                ShoppingList.Add(text);
                button.onClick.AddListener(delegate
                {
                    StrikeItem(text);
                });

                if (GameManager.Instance.HasBought(text.text))
                    text.fontStyle = FontStyles.Strikethrough;
            }
        }

        private void StrikeItem(TextMeshProUGUI text)
        {
            if (text.fontStyle == FontStyles.Strikethrough)
                text.fontStyle = FontStyles.Normal;
            else
                text.fontStyle = FontStyles.Strikethrough;
        }

        public void BuyItem(BuyableItems item)
        {
            if (BabySpeech_Parent.childCount >= 10)
            {
                SpawnParentMessage("I need to calm the baby first.");
                return;
            }

            if (GameManager.Instance.HasBought(item))
            {
                SpawnParentMessage("I already have this.");
                return;
            }

            GameManager.Instance.BuyItem(item);
            BabyMeter += 15;  //TODO: MAKE THIS A BETTER VALUE

            if (GameManager.Instance.NumberBought() >= 4)
                BabyMeter += 15;
            else if (GameManager.Instance.NumberBought() >= 3)
                BabyMeter += 10;
            else if (GameManager.Instance.NumberBought() >= 2)
                BabyMeter += 5;

            SpawnParentMessage();
        }

        //produce = 0
        //hardware = 1
        //other = 2
        public void SwitchAisleTo(bool IsLeft)
        {
            if (mySequence != null && mySequence.active)
                return;

            Image nextImage = Produce;
            string current = Asile_Type.text;
            Asile_Type.text = (IsLeft) ? Switch_Left.text : Switch_Right.text;
            nextImage = (Asile_Type.text == "Grocery Aisle") ? Grocery : ((Asile_Type.text == "Hardware Aisle") ? Hardware : Produce);

            if (!IsLeft)
                Switch_Right.text = current;
            else
                Switch_Left.text = current;

            switch (current)
            {
                case "Grocery Aisle":
                    SwitchBackgrounds(Grocery, nextImage);
                    break;

                case "Hardware Aisle":
                    SwitchBackgrounds(Hardware, nextImage);
                    break;

                case "Produce Aisle":
                    SwitchBackgrounds(Produce, nextImage);
                    break;

                default:
                    Debug.LogError("Index out of bounds");
                    return;
            }
        }

        private void SwitchBackgrounds(Image current, Image next)
        {
            var list = new List<Transform>();

            //force current image to be on top of the next
            current.gameObject.transform.SetAsFirstSibling();

            //Set up sequence
            mySequence = DOTween.Sequence();
            mySequence.Append(current.DOFade(0, 1.25f));
            next.gameObject.SetActive(true);
            current.raycastTarget = false;
            next.raycastTarget = false;

            foreach (Transform child in current.gameObject.transform)
            {
                child.GetChild(0).gameObject.SetActive(false);
                child.gameObject.GetComponent<Image>().raycastTarget = false;
                mySequence.Insert(0f, child.gameObject.GetComponent<Image>().DOFade(0, 1.25f));
            }

            foreach (Transform child in next.gameObject.transform)
            {
                list.Add(child);
                mySequence.Insert(0.1f, child.gameObject.GetComponent<Image>().DOFade(1, 1.25f));
            }

            mySequence.Insert(0.1f, next.DOFade(1, 1.25f)).OnComplete(() =>
            {
                //turn off current (last) image
                current.gameObject.SetActive(false);
                next.raycastTarget = true;

                list.ForEach(child =>
                {
                    child.GetChild(0).gameObject.SetActive(true);
                    child.gameObject.GetComponent<Image>().raycastTarget = true;
                });

                mySequence = null;
            });
        }

        private void DepopulateList()
        {
            foreach (Transform child in ListParent)
            {
                Destroy(child.gameObject);
            }
        }
    }
}