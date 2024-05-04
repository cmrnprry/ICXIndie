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
        Any = -1, Jump = 1, Cry = 0, Climb = 4, Hang = 2, Reach = 3
    }
    public class StoreManager : MonoBehaviour
    {
        private Sequence BackgroundSequence, ChildSequence;

        [SerializeField]
        public float BabyMeter = 0; //will track how annoying baby is being

        [Header("Baby")]
        [SerializedDictionary("Description", "Child Actions")]
        public SerializedDictionary<string, StoreActions> BabyWords;
        public RectTransform BabySpeech_Parent;
        public GameObject Baby_Bubbles;
        private float SpawnTime = 5f;
        private StoreActions CurrentAction = StoreActions.Reach;
        public List<Image> BabyImages;


        [Header("Parent")]
        [SerializedDictionary("Description", "Child Actions")]
        public SerializedDictionary<string, StoreActions> AdultWords;
        public TextMeshProUGUI ParentTextBox;
        private float time = 3f;

        [Header("Shopping List")]
        public GameObject List_Item;
        public Transform ListParent;

        [Header("Switch Asiles")]
        public TextMeshProUGUI Asile_Type, Switch_Left, Switch_Right;
        public Image Produce, Grocery, Hardware;
        private List<TextMeshProUGUI> ShoppingList;

        private Coroutine ParentCoroutine;
        private Coroutine SpawnCoroutine;
        private Coroutine TantrumCoroutine;
        private Coroutine BabyMessageCoroutine;

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
            BackgroundSequence = DOTween.Sequence();
            ChildSequence = DOTween.Sequence();
        }

        private void OnDisable()
        {
            DepopulateList();
            StopCoroutine(ParentCoroutine);
            StopCoroutine(SpawnCoroutine);
            StopCoroutine(TantrumCoroutine);
            StopCoroutine(BabyMessageCoroutine);
        }

        private void OnEnable()
        {
            ShoppingList = new List<TextMeshProUGUI>();
            PopulateList();

            SpawnTime = 1.5f;
            SpawnCoroutine = StartCoroutine(SpawnTimer());
            BabyMessageCoroutine = StartCoroutine(IncreaseBaby());
            UpdateTimer();
        }

        private void Update()
        {
            if (BabyMeter >= 100)
                BabyMeter = 100;

            if (BabyMeter <= 0)
                BabyMeter = 0;
        }

        public Vector2 GetRandomPositionInBounds()
        {
            Vector3[] bounds = new Vector3[4];
            BabySpeech_Parent.GetWorldCorners(bounds);

            var randPosX = Random.Range(bounds[0].x, bounds[2].x);
            var randPosY = Random.Range(bounds[0].y, bounds[1].y);

            return new Vector2(randPosX, randPosY);
        }

        private void UpdateTimer()
        {
            SpawnTime = 6.5f + ((1f - 6.5f) * (BabyMeter / 100));
        }

        private IEnumerator SpawnTimer()
        {
            yield return new WaitForSeconds(SpawnTime);

            if (BabySpeech_Parent.childCount >= 15)
                yield return new WaitUntil(() => BabySpeech_Parent.childCount < 15);

            string text = ChooseMessage(BabyWords);

            if (text.Contains("@"))
            {
                text = text.Replace("@", GameManager.Instance.ParentName);
            }

            Vector2 pos = GetRandomPositionInBounds();

            if (CurrentAction == StoreActions.Cry || CurrentAction == StoreActions.Jump || CurrentAction == StoreActions.Hang)
            {
                var screenPoint = Input.mousePosition;
                screenPoint.z = 10.0f; //distance of the plane from the camera
                pos = Camera.main.ScreenToWorldPoint(screenPoint);
            }

            GameObject obj = Instantiate(Baby_Bubbles, pos, Quaternion.identity, BabySpeech_Parent);
            obj.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = text;

            if (BabySpeech_Parent.childCount >= 10)
                BabyMeter += 15;
            else if (BabySpeech_Parent.childCount >= 5)
                BabyMeter += 10;
            else if (BabySpeech_Parent.childCount >= 3)
                BabyMeter += 5;

            CheckBaby();
            SpawnCoroutine = StartCoroutine(SpawnTimer());
        }

        private string ChooseMessage(SerializedDictionary<string, StoreActions> words)
        {
            List<string> list = new List<string>();
            foreach (KeyValuePair<string, StoreActions> entry in words)
            {
                if (entry.Value == CurrentAction)
                    list.Add(entry.Key);
            }

            int rand = Random.Range(0, list.Count);
            return list[rand];
        }

        public void SpawnParentMessage(string message = "")
        {
            ParentTextBox.text = (message == "") ? ChooseMessage(AdultWords) : message;

            if (ParentTextBox.text.Contains("@"))
            {
                ParentTextBox.text = ParentTextBox.text.Replace("@", GameManager.Instance.ChildName);
            }

            ParentTextBox.gameObject.transform.parent.gameObject.SetActive(true);

            int rand = Random.Range(1, 10);
            if (rand < 5)
                BabyMeter -= 10;

            if (ParentCoroutine != null)
                StopCoroutine(ParentCoroutine);

            ParentCoroutine = StartCoroutine(CloseParentMessage());
        }

        private IEnumerator CloseParentMessage()
        {
            yield return new WaitForSeconds(time);
            ParentTextBox.gameObject.transform.parent.gameObject.SetActive(false);
            time = 3f;

            ParentCoroutine = null;
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
            GameManager.Instance.BuyItem(item);

            BabyMeter += (item != BuyableItems.JunkFood) ? 15 : -10;

            if (GameManager.Instance.NumberBought() >= 6)
                BabyMeter += 7;
            else if (GameManager.Instance.NumberBought() >= 4)
                BabyMeter += 5;
            else if (GameManager.Instance.NumberBought() >= 2)
                BabyMeter += 3;

            //SpawnParentMessage();
            CheckBaby();
        }

        private void CheckBaby()
        {
            //everytime we change the babymeter, update teh timer
            UpdateTimer();

            var next = StoreActions.Reach;
            if (BabyMeter <= 25) // he is reaching an dhappy
            {
                next = StoreActions.Reach;
            }
            else if (BabyMeter <= 40) //he is jumping and trying to get your attention
            {
                next = StoreActions.Jump;
            }
            else if (BabyMeter <= 60) // he is trying to help by climbing
            {
                next = StoreActions.Climb;
            }
            else if (BabyMeter <= 75) // he is pouting
            {
                next = StoreActions.Hang;
            }
            else //he is throwing a tantrum
            {
                next = StoreActions.Cry;
                TantrumCoroutine = StartCoroutine(Tantrum(1.5f));
            }

            if (TantrumCoroutine != null && CurrentAction == StoreActions.Cry && next == StoreActions.Cry)
            {
                StopCoroutine(TantrumCoroutine);
                TantrumCoroutine = null;
            }

            if (next != CurrentAction)
                SwitchChildImage(next);
        }

        private IEnumerator Tantrum(float wait = 2.5f)
        {
            yield return new WaitForSeconds(wait);

            if (CurrentAction != StoreActions.Cry)
            {
                TantrumCoroutine = null;
                yield break;
            }

            SpawnParentMessage();
            GameManager.Instance.RemoveTime(5);

            TantrumCoroutine = StartCoroutine(Tantrum());
        }

        private IEnumerator IncreaseBaby(float wait = 2.5f)
        {
            yield return new WaitForSeconds(wait);

            var removeTiem = false;
            if (BabySpeech_Parent.childCount >= 10)
            {
                BabyMeter += 7;
                removeTiem = true;
            }
            else if (BabySpeech_Parent.childCount >= 5)
            {
                BabyMeter += 3;
                removeTiem = true;
            }
            else if (BabySpeech_Parent.childCount >= 3)
            {
                BabyMeter += 1;
                removeTiem = true;
            }

            if (removeTiem)
                GameManager.Instance.RemoveTime(3);

            BabyMessageCoroutine = StartCoroutine(IncreaseBaby());
        }

        private void SwitchChildImage(StoreActions next)
        {
            ChildSequence = DOTween.Sequence();
            var Current = BabyImages[(int)CurrentAction];
            var Next = BabyImages[(int)next];

            Next.gameObject.SetActive(true);
            CurrentAction = next;

            ChildSequence.Append(Current.DOFade(0, 0.85f)).Insert(0.1f, Next.DOFade(1, 0.85f)).OnComplete(() =>
            {
                Current.gameObject.SetActive(false);
                ChildSequence = null;
            });
        }

        //produce = 0
        //hardware = 1
        //other = 2
        public void SwitchAisleTo(bool IsLeft)
        {
            if (BackgroundSequence != null && BackgroundSequence.active)
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
            BackgroundSequence = DOTween.Sequence();
            BackgroundSequence.Append(current.DOFade(0, 1.25f));
            next.gameObject.SetActive(true);
            current.raycastTarget = false;
            next.raycastTarget = false;

            foreach (Transform child in current.gameObject.transform)
            {
                child.GetChild(0).gameObject.SetActive(false);
                child.gameObject.GetComponent<Image>().raycastTarget = false;
                BackgroundSequence.Insert(0f, child.gameObject.GetComponent<Image>().DOFade(0, 1.25f));
            }

            foreach (Transform child in next.gameObject.transform)
            {
                list.Add(child);
                BackgroundSequence.Insert(0.1f, child.gameObject.GetComponent<Image>().DOFade(1, 1.25f));
            }

            BackgroundSequence.Insert(0.1f, next.DOFade(1, 1.25f)).OnComplete(() =>
            {
                //turn off current (last) image
                current.gameObject.SetActive(false);
                next.raycastTarget = true;

                list.ForEach(child =>
                {
                    child.GetChild(0).gameObject.SetActive(true);
                    child.gameObject.GetComponent<Image>().raycastTarget = true;
                });

                BackgroundSequence = null;
            });
        }

        private void DepopulateList()
        {
            foreach (Transform child in ListParent)
            {
                Destroy(child.gameObject);
            }
        }

        public void ReturnHome()
        {
            int Time = 30;
            if (CurrentAction == StoreActions.Cry)
                Time += 15;
            else if (CurrentAction == StoreActions.Hang)
                Time += 5;

            GameManager.Instance.RemoveTime(Time);
        }

        public void UpdateHomeMessage(TextMeshProUGUI box)
        {
            string text = "It will take 30 minutes to get home.";
            if (CurrentAction == StoreActions.Cry)
                text = "It will take 45 minutes to get home. I should calm him down first.";
            else if (CurrentAction == StoreActions.Hang)
                text = "It will take 35 minutes to get home when he's pouting like this.";

            box.text = text;
        }
    }
}