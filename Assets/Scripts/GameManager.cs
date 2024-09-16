using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public enum BuyableItems
{
    //junkfood is to appease baby at the store
    Pumpkin, Watermelon, Meat, Peanutbutter, Chocolate, Rope, Wood, Nails, Needle, Thread, Banana, Null = -1, JunkFood = -2, Default = -3
}

public enum EndingActions
{
    //junkfood is to appease baby at the store
    Bitten = 0, Teddy = 1, TV = 2, Bed = 3, Trap = 4, Vomit = 5, Happy = 6, None = -1, Run_away = 108, Bone_crunching = 100, Scratching = 101, Toy_Playing = 102, Jump = 103, Sniffs = 104, TV_Break = 105, Munch = 106, Ripping = 107
}

public enum Pronouns
{
    plural = 0, singular = 1
}


namespace AYellowpaper.SerializedCollections
{
    public class GameManager : MonoBehaviour
    {
        private int Time = 350;
        public static bool CanPause = false;

        [HideInInspector] public string ChildName = "Chris"; private string previous = "";
        [HideInInspector] public List<string> ChildNoun = new List<string> { "they", "them", "their" };
        private Pronouns current = Pronouns.plural;
        [HideInInspector] public string ParentName = "Mom";
        [HideInInspector] public float childTired = 0;
        [HideInInspector] public bool wearingGloves = false;

        [Header("Preform Action?")]
        [SerializeField]
        private GameObject ReminderParent;
        [SerializeField]
        private TextMeshProUGUI Reminder_Text;
        private ObjectAbstract Current_Interaction = null;
        private PrepareableObjects Current_Preperation = null;
        [HideInInspector] public int Index = -1;

        [Header("Total Time")]
        [SerializeField]
        private TextMeshProUGUI TotalTime_Text;

        [Header("Phase Objects")]
        [SerializeField]
        private GameObject NightTime;
        [SerializeField]
        private GameObject DayTime;
        [SerializeField]
        private GameObject EndGame;
        [SerializeField]
        private Animator Transition;

        [Header("Audio")]
        public AudioSource NightSource;
        public List<AudioClip> nightclips;
        public AudioSource BGMSource, ScarySource;
        public AudioClip BGM, BGM_Scary;

        [Header("Ending")]
        public List<Sprite> Ending_Sprites;
        public Image Ending_Image;
        public Animator Ending_Continue;
        public TextMeshProUGUI Ending_TextBox;
        private bool CanContinue = false;

        private List<BuyableItems> BoughtItems = new List<BuyableItems>(); // list of everythign we have bought
        public List<InteractionObject> InteractionObjectList = new List<InteractionObject>(); //list of all the objects we can interact with at home
        private InteractableObject CurrentAction;

        public delegate void SettingsChange();
        public static event SettingsChange OnChangeParent;
        public static event SettingsChange OnChangeChildName;
        public static event SettingsChange OnChangeChildNouns;

        public static bool FedChocolate;

        public static GameManager Instance { get; private set; }

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

            Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
            Screen.SetResolution(1920, 1080, true, Screen.currentResolution.refreshRate);
        }

        private void Start()
        {
            ChildName = "Chris";
            TotalTime_Text.text = $"Time Left: {Time} min";
            StartCoroutine(FinishAudioIntro());
        }

        //Checks to see what is bought or completed on start
        private void CheckSave()
        {
            foreach (var item in InteractionObjectList)
            {
                if (item.interacton.completed)
                {
                    item.SetInteraction();
                }
            }
        }


        //sets string
        public void SetParent(string change)
        {
            ParentName = change;

            OnChangeParent.Invoke();
        }

        //sets string and invokes an action
        public void SetChild(string change)
        {
            previous = ChildName;
            ChildName = change;

            OnChangeChildName.Invoke();
        }

        public void SetChildPronoun(string noun)
        {
            var split = noun.Split(" /");
            ChildNoun[0] = split[0];
            ChildNoun[1] = split[1];
            ChildNoun[2] = split[2];

            current = (ChildNoun[0] == "they") ? Pronouns.plural : Pronouns.singular;

            Debug.Log(ChildNoun[0]);
            Debug.Log(ChildNoun[1]);
            Debug.Log(ChildNoun[2]);
        }

        //takes in old child text name and replaces it wiith th ecorrect version
        public string SetChildName(string oldName)
        {
            string changeTo = oldName;
            if (oldName.Contains("@"))
                changeTo = oldName.Replace("@", ChildName);
            else if (previous != "")
                changeTo = oldName.Replace(previous, ChildName);

            return changeTo;
        }

        public string SetParentName(string text)
        {
            string change = text;
            if (text.Contains("@"))
                change = text.Replace("@", ParentName);
            else if (previous != "")
                change = text.Replace(previous, ParentName);

            return change;
        }

        /// <summary>
        /// Checks to see if player has bought an item
        /// </summary>
        /// <param name="item">the item to check</param>
        /// <returns></returns>
        public bool HasBought(BuyableItems item)
        {
            if (BoughtItems != null && BoughtItems.Contains(item))
                return true;

            return false;
        }

        public bool HasBought(string item)
        {
            bool Bought = false;
            if (BoughtItems != null)
            {
                BoughtItems.ForEach(bought => { if (item.Contains(bought.ToString())) { Bought = true; } });
            }

            return Bought;
        }

        public bool HasPrepared(BuyableItems item)
        {
            if (BoughtItems != null && BoughtItems.Contains(item))
                return true;

            return false;
        }

        public void BuyItem(BuyableItems item)
        {
            BoughtItems.Add(item);

            if (item == BuyableItems.JunkFood || item == BuyableItems.Null || item == BuyableItems.Banana)
                return;

            for (int ii = 0; ii < InteractionObjectList.Count; ii++)
            {
                InteractionObjectList[ii].SetBuy(item);
            }
        }

        public int NumberBought()
        {
            return BoughtItems.Count;
        }

        public void RemoveTime(int time)
        {
            Sequence scaleTime = DOTween.Sequence();
            RectTransform rect = TotalTime_Text.gameObject.GetComponent<RectTransform>();
            Time -= time;
            if (Time <= 100)
            {
                Color color = Color.red;
                ColorUtility.TryParseHtmlString("#A42707", out color);
                TotalTime_Text.color = color;
            }

            scaleTime.Append(rect.DOScale(1.5f, 0.35f)).Insert(0, rect.DOAnchorPos(new Vector2(465, -75), 0.35f)).OnStart(() =>
            {

                TotalTime_Text.text = $"Time Left: {Time} min";

            }).Insert(0.65f, rect.DOScale(1, 1.5f)).Insert(0.65f, rect.DOAnchorPos(new Vector2(315, -50), 1.5f)).OnComplete(() =>
            {
                Color color = Color.red;
                ColorUtility.TryParseHtmlString("#D9D2D1", out color);

                TotalTime_Text.color = color;
            });


            if (Time <= 0)
            {
                EndDayTimer();
                return;
            }

            if (Time <= 100 || (StoreManager.Instance != null && !StoreManager.Instance.InStore))
                CheckTimeLeft();
        }

        public int GetTime()
        {
            return Time;
        }

        public void CheckTimeLeft()
        {
            float BGM_To = 1f;
            float Scary_To = 1f;

            if (Time >= 200)
            {
                BGM_To = 1;
                Scary_To = 0;
            }
            else if (Time >= 100)
            {
                BGM_To = 0.85f;
                Scary_To = 0.25f;
            }
            else if (Time >= 75)
            {
                BGM_To = .5f;
                Scary_To = .5f;
            }
            else if (Time >= 50)
            {
                BGM_To = .35f;
                Scary_To = .65f;
            }
            else
            {
                BGM_To = .25f;
                Scary_To = 1;
            }


            AdjustVolume(BGM_To, 1, 0);
            AdjustVolume(Scary_To, 1, 1);
        }

        public void TimeReminder(InteractableObject obj, InteractionObject io, int index = -1)
        {
            var time = obj.Time_to_do;

            CurrentAction = obj;
            ReminderParent.SetActive(true);

            if (Time < time)
            {
                Reminder_Text.text = $"This action will cost {time.ToString()}, but you only have {Time.ToString()} minutes left.\n The day will end if you do this. Continue?";
            }
            else
                Reminder_Text.text = $"This action will cost {time.ToString()}. You have {Time.ToString()} minutes left.\n Continue?";
            Current_Interaction = io;
            Index = index;
        }

        public void TimeReminder(InteractableObject obj, PrepareObject io, int index = 0)
        {
            var time = io.prepare.Time_to_do;

            CurrentAction = obj;
            ReminderParent.SetActive(true);
            Reminder_Text.text = $"This action will cost {time.ToString()}. You have {Time.ToString()} minutes left.\n Continue?";
            Current_Interaction = io;
            Current_Preperation = io.prepare;
            Index = index;
        }

        public void PauseInteraction(bool on)
        {
            foreach (var item in InteractionObjectList)
            {
                item.RaycastDetection(on);

                if (on && item == Current_Interaction)
                    item.OnHoverExit();
            }
        }

        public void PerformAction()
        {
            PauseInteraction(true);

            Current_Interaction.SetInteraction();

            if (Current_Interaction.IsPreperation())
            {
                //ActionsPreformed.ActionPerformed(CurrentAction.preparable_info[Index]);
                foreach (var interaction in InteractionObjectList)
                {
                    interaction.SetPrepare(Current_Preperation);
                }
                RemoveTime(Current_Preperation.Time_to_do);
            }
            else
            {
                CheckDoubles(CurrentAction);
                ActionsPreformed.ActionPerformed(CurrentAction, Index);
                RemoveTime(CurrentAction.Time_to_do);
            }
        }

        public void CancelAction()
        {
            PauseInteraction(true);
            CurrentAction = null;
            Current_Interaction = null;
        }

        private void CheckDoubles(InteractableObject action)
        {
            foreach (var item in InteractionObjectList)
            {
                if (action.name == item.interacton.name && item.GetComponent<Image>().color.a < 1)
                {
                    item.SetInteraction();
                    break;
                }
            }
        }

        ///////////////////////////////////////////////////////// AUDIO /////////////////////////////////////////////////////////

        public void AdjustVolume(float to, float duration, int flag)
        {
            //0 = bgm, 1 = scary, 2 = both
            if (flag == 0 || flag == 2)
            {
                if (BGMSource.volume != to)
                    BGMSource.DOFade(to, duration);
            }

            if (flag == 1 || flag == 2)
            {
                if (ScarySource.volume != to)
                    ScarySource.DOFade(to, duration);
            }

        }

        private IEnumerator FinishAudioIntro()
        {
            // Cache the WaitForSeconds call, this is done because
            // like any other class WaitForSeconds causes garbage because it is heap allocated.
            // Therefore we cache it to simply reuse the same class instance.
            var waitForClipRemainingTime = new WaitForSeconds(GetClipRemainingTime(BGMSource));
            yield return waitForClipRemainingTime;
            BGMSource.clip = BGM;
            BGMSource.Play();
            ScarySource.Play();
            BGMSource.loop = true;
        }

        private bool IsReversePitch(AudioSource source)
        {
            return source.pitch < 0f;
        }

        private float GetClipRemainingTime(AudioSource source)
        {
            // Calculate the remainingTime of the given AudioSource,
            // if we keep playing with the same pitch.
            float remainingTime = (source.clip.length - source.time) / source.pitch;
            return IsReversePitch(source) ?
                (source.clip.length + remainingTime) :
                remainingTime;
        }

        private string ToUpperCase(string str)
        {
            if (string.IsNullOrEmpty(str))
                return string.Empty;

            char[] a = str.ToCharArray();
            a[0] = char.ToUpper(a[0]);

            return new string(a);
        }

        ///////////////////////////////////////////////////////// SECOND PHASE /////////////////////////////////////////////////////////
        private Coroutine routine = null;
        [SerializedDictionary("End Text", "Action")]
        private Dictionary<string, EndingActions> Ending_Text = new Dictionary<string, EndingActions>();
        [HideInInspector] public bool Finished = false;

        public void EndDayButton()
        {
            StartCoroutine(EndDayTransition());
        }

        private void EndDayTimer()
        {
            StartCoroutine(EndDayTransition());
        }

        IEnumerator EndDayTransition()
        {
            Transition.gameObject.SetActive(true);
            yield return new WaitForSecondsRealtime(0.5f);
            DayTime.SetActive(false);

            bool window = ActionsPreformed.CheckAction("Window");

            NightTime.SetActive(true);
            EndGame.SetActive(true);
            AddUpEndings();
            AdjustVolume(0.65f, 0.5f, 0);
            AdjustVolume(0.3f, 0.5f, 1);

            yield return new WaitForSecondsRealtime(0.5f);
            Transition.SetTrigger("Toggle");
        }

        //how tired/excited is the baby
        private void AddUpEndings()
        {
            Ending_Text.Clear();
            //check how much chocolate the baby had
            int chocolateAmount = 0;


            //objects that have no major implimications, and just affects tiredness
            bool cat_tower = ActionsPreformed.CheckAction("Cat_Tower"); //tired += 0.5 | runs around but doesn't really care
            bool pumpkin = ActionsPreformed.CheckAction("Pumpkin"); //tired += 1.5 | loves it
            bool watermelon = ActionsPreformed.CheckAction("Watermelon"); //tired -= 0.5 | likes it but has opposite effect

            //usually good, but can be bad
            bool bear = ActionsPreformed.CheckAction("Fix_Bear"); //if fixed: tired += 1 | plays with it, but breaks it again else: tired -= 0.5 | makes {ChildNoun[1]} frustrated
            bool playmat = ActionsPreformed.CheckAction("Treats"); // if pb: tired += 1  if candy: tired -= 2 || makes {ChildNoun[1]} hyper, but will crash and puke soon  else: tired -= 1
            bool tv = ActionsPreformed.CheckAction("TV"); //if cartoon: tired += 1   if nature: tired += 2 | neighbors annoyed by barking / howling ?

            //Mothing bad, but depends on other things
            bool bed = ActionsPreformed.CheckAction("Bed"); // tired += 0.5 | digs around but doesn't reall care   if you give {ChildNoun[1]} a lot of chocolate or tire {ChildNoun[1]} out, {ChildNoun[0]} will nest in there

            //prevents escape
            bool windows = ActionsPreformed.CheckAction("Window"); //tired += 0.5 \ tires an escape

            string plural = current == Pronouns.singular ? "s" : "";

            //START TEXT
            string chocolateString = $"As the full moon rises, {ChildName} excitedly runs around the basement while you finish any last minute preparations.";
            if (ChildObject.FedChocolate)
            {
                chocolateAmount += 1;
                chocolateString = $"As the full moon rises, {ChildName} is pulling on your clothes, asking for more sweets. You tell {ChildNoun[1]} to go sit down while you try to finish any last minute preparations.";
            }
            EndingText(chocolateString);

            string next = (windows) ? $"It isn't long before the moon light shines into the basement through cracks in the wooden boards, and your little {ChildName} fully transforms." : $"It isn't long before the moon light shines into the basement through the windows, and your little {ChildName} fully transforms.";

            EndingText(next + $"\nIt never sounds or looks pleasent, but {ChildNoun[0]} say{plural} it doesn't hurt when you ask.", EndingActions.Bone_crunching);

            next = $"It isn't long before {ChildName} is running around the room, sniffing around all the things left out.";
            if (ChildObject.FedChocolate)
                next += $" The chocolate you fed {ChildNoun[1]} earlier seemed to make {ChildNoun[1]} more hyper than normal.";

            EndingText(next);



            //CHECK WHAT PLAYER DID IN A SET ORDER
            BuyableItems treatTypes = ActionsPreformed.FindNeed("Treats");

            //START WITH BEAR TO HAVE IMAGE
            bool temp_bool = false;
            if (bear)
            {
                var index = ActionsPreformed.FindOptionIndex("Fix_Bear");

                if (index == 0)
                {
                    temp_bool = true;
                    if (chocolateAmount > 0)
                    {
                        EndingText($"While running in circles, {ChildName} locks eyes with {ChildNoun[2]} bear stuffie, grabs it in {ChildNoun[2]} jaws and violently shakes it.\nThe bear never stood a chance.", EndingActions.Teddy);
                    }

                    else
                    {
                        EndingText($"While running in circles, {ChildName} sees {ChildNoun[2]} bear, and starts playing with. {ChildName} throws it around in the air, attempting to catch it before it hits the ground.\nBy the time {ChildName}'s done with it, {ChildNoun[2]} sharp teeth have ripped your stitches, and the bear, apart.", EndingActions.Teddy);
                        EndingText($"");
                    }


                    childTired += 1;
                }
                else if (index == 1)
                {
                    temp_bool = false;
                    if (chocolateAmount > 0)
                        EndingText($"While running in circles, {ChildName} sees {ChildNoun[2]} broken bear, and grabs it in {ChildNoun[2]} jaws, violently shaking it. The already ruined bear is ripped to pieces. You worry you won't be able to fix it later.", EndingActions.Teddy);
                    else
                        EndingText($"While running in circles, {ChildName} sees {ChildNoun[2]} broken bear, and noses it before whining at it. {ChildName} tries to play with it, but the stuffed toy falls into further disrepair.", EndingActions.Teddy);

                    childTired -= 0.5f;
                }
            }
            if (cat_tower)
            {
                string tower_text = "";
                if (bear && temp_bool)
                    tower_text = ($"Finished playing with {ChildNoun[2]} bear, {ChildName} turns to {ChildNoun[2]} tower. {ChildName} runs up the ladder and perches on top before something in the room catches {ChildNoun[2]} attention.");
                else if (bear && !temp_bool)
                    tower_text = ($"Tired of {ChildNoun[2]} broken toy, {ChildName} turns to {ChildNoun[2]} tower. {ChildName} runs up the ladder and perches on top before something in the room catches {ChildNoun[2]} attention.");
                else
                    tower_text = ($"{ChildName} turns to {ChildNoun[2]} tower. {ChildName} runs up the ladder and perches on top before something in the room catches {ChildNoun[2]} attention.");

                if (ChildObject.FedChocolate)
                    EndingText(tower_text + $"\n{ChildName} leaps from {ChildNoun[2]} perch, and sprints to {ChildNoun[2]} new fixation.", EndingActions.Jump);
                else
                    EndingText(tower_text + $"\n{ChildName} whines and readies himself before jumping from the top of {ChildNoun[2]} perch, and prances to {ChildNoun[2]} new fixation.", EndingActions.Jump);

                childTired += 0.5f;
            }
            if (playmat)
            {
                string temp = "";
                switch (treatTypes)
                {
                    case BuyableItems.Chocolate:
                        childTired += 1;
                        chocolateAmount += 1;

                        EndingText($"{ChildName} pounces on the mat, sniffs out all the chocolate treats hidden in it, and wolfs down everything {ChildNoun[0]} could find. Not too long after {ChildName} zooms around the room.", EndingActions.Sniffs);
                        break;
                    case BuyableItems.Peanutbutter:
                        childTired += 2;

                        EndingText($"{ChildName} pounces on the mat and sniffs out all the peanut butter treats hidden in it.\n{ToUpperCase(ChildNoun[2])} tail goes a mile a minute, and once {ChildName} notices, begins to chase it.", EndingActions.Sniffs);
                        break;
                    default: // didn't put anything
                        childTired -= 1f;

                        EndingText($"{ChildName} sniffs and digs at the mat, but after realizing there's nothing there, moves on. {ToUpperCase(ChildNoun[2])} tail droops.", EndingActions.Sniffs);
                        break;
                }
            }

            if (tv)
            {
                string temp = "";
                if (ActionsPreformed.FindOptionIndex("TV") == 0)
                {
                    temp = $"The intro song of kid's cartoon starts playing. {ChildNoun[2]} ears instantly perk up, and {ChildName} runs over to the TV. Sitting and wagging {ChildNoun[2]} tail, {ChildNoun[0]} cock{plural} {ChildNoun[2]} head from side to side as the characters on screen ask questions.";
                    childTired += 1;
                }
                else
                {
                    temp = $"The wolf pack on the nature show playing start howling. {ChildNoun[2]} ears instantly perk up, and {ChildName} runs over to the TV. {ToUpperCase(ChildNoun[0])} bark{plural} and howl{plural} with the wolves on screen, tail going a mile a minute.";
                    childTired += 2;
                }

                EndingText(temp + $"\n{ChildName} watches the show happily, until {ChildNoun[0]} jump{plural} up and presses a button, turning it off. {ChildName} whines and paws at the TV, trying to turn it back on. When {ChildNoun[0]} can't, {ChildName} grows frustrated, biting and pulling at the bars surrounding the TV.", EndingActions.TV_Break);

                EndingText($"\n{ChildName} easily pulls and bends the metal. It isn't long before the the TV lays in a wrecked pile on the floor. Satisfied with {ChildNoun[2]} destruction, {ChildName} trots away.", EndingActions.TV);
            }
            if (watermelon)
            {
                string tv_text = "";
                if (tv)
                {
                    tv_text = ($"The taste of metal and electronics seemed to leave a bad taste in {ChildNoun[2]} mouth, and {ChildName} trots over to the watermelon on the floor.");
                }

                if (treatTypes == BuyableItems.Chocolate)
                    EndingText(tv_text + $"\n{ChildName} gives the watermelon halves a few tentative licks, but instead of chewing or eating, {ChildNoun[0]} dig into them. It makes a huge mess, but {ChildNoun[0]} seem{plural} to be having fun.");
                else
                    EndingText(tv_text + $"\n{ChildName} gives the watermelon halves a few tentative licks before ripping it to bits. You're not sure how much {ChildNoun[0]}'s actually eating vesus how much is mush on the floor, but {ChildNoun[0]} seem{plural} to be having fun.");

                childTired += 1.5f;
            }
            if (pumpkin)
            {
                string temp = "";
                if (watermelon)
                {
                    temp = "Covered in watermelon bits, ";
                }

                temp += (chocolateAmount > 0) ? $"{ChildName} sniffs out the meat pumpkin and bats it around for a while. {ToUpperCase(ChildNoun[0])} gnaw{plural} on the pumpkin, but quickly moves on. The sugar high seems to be wearing off." : $"{ChildName} sniffs out the meat pumpkin and bats it around, gnawing on the pumpkin and scarfing down any meat pieces that fall out. ";

                float tired = (chocolateAmount > 0) ? 1f : 1.5f;
                EndingText(temp, EndingActions.Munch);
                childTired += tired;
            }

            if (bed)
            {
                childTired += 0.5f;

                string temp = "";
                if (chocolateAmount >= 2)
                {
                    string bad_end = "";
                    EndingText($"{ChildName} is slowly walking around the room, and stops next {ChildNoun[2]} bed, heaving. Before you can do anything, {ChildNoun[0]} vomit{plural} everything all over {ChildNoun[2]}. {ChildNoun[0]} wimper{plural} and curl{plural} up next to it.", EndingActions.Vomit);

                    bad_end = ($"The rest of the night passes quickly. You stay by {ChildNoun[2]} side the entire time, rubbing {ChildNoun[2]} back and giving {ChildNoun[1]} water.  {ChildName} vomits a few more times before {ChildNoun[0]} transform{plural} back.");
                    EndingText(bad_end + $"You and {ChildName} spend the rest of the day curled up together. You make a mental note to not give {ChildNoun[1]} chocolate again, no matter how much {ChildNoun[0]} beg{plural}.");
                    routine = StartCoroutine(ShowEndText());
                    Finished = true;
                    return;
                }
                else if (chocolateAmount > 0)
                {
                    temp = $"{ChildName} hops on {ChildNoun[2]} bed and begins to dig in it. {ToUpperCase(ChildNoun[0])} stop{plural}, heaves for a moment, then continues to dig.";
                }
                else
                    temp = $"{ChildName} hops on {ChildNoun[2]} bed and begins to dig in it.";


                //TODO: FIGURE OUT THIS NUMBER
                if (childTired >= 5)
                {
                    temp += $" {ChildNoun[0]} turn{plural} in circles before plopping down and falling asleep.";
                    EndingText(temp, EndingActions.Happy);
                    EndingText($"The rest of the night passes quickly and quietly. {ChildName} stays in {ChildNoun[2]} bed all night, allowing you to get some sleep of your own. You wake in the morning to a messy basement, and a well slept child. You pat yourself on the back, and wake {ChildName} to get ready for the day.");
                    routine = StartCoroutine(ShowEndText());
                    Finished = true;
                    return;
                }


                temp += $"\n{ChildName} continues to dig in {ChildNoun[2]} bed, and even starts biting and tearing at it.";
                EndingText(temp, EndingActions.Bed);
                EndingText($"{ChildName} rips the bed open, pulling all the stuffing out before eyeing the basement windows.", EndingActions.Ripping);
            }
            if (!windows)
            {
                EndingText($"{ChildName} paces around, looking up at the unboarded windows. {ToUpperCase(ChildNoun[0])} whine{plural} and paw{plural} at the walls before standing up on {ChildNoun[2]} hind legs.\n{ChildName} backs up and jumps, squeezing out of the basement window and into the backyard.", EndingActions.Run_away);
                CheckNightTime(chocolateAmount);
            }
            else
            {
                EndingText($"{ChildNoun[0]} pace{plural} around, and attempts to escape, but the wooden boards hold firm. Unable to escape through the window, {ChildName} scratches at the basement door.", EndingActions.Scratching);


                EndingText($"When you try to stop {ChildNoun[1]}, {ChildNoun[0]} turn{plural} around and bites your hand. You suppress a swear, and wrap your hand in your shirt.\n{ ChildName} almost immediately realizes { ChildNoun[2]} error, and licks your leg in apology.You tell { ChildNoun[1]} to go lay down, and { ChildNoun[0]} slink{plural} to { ChildNoun[2]} destroyed bed.", EndingActions.Bitten);

                EndingText($"The rest of the night passes quickly and quietly. {ChildName} stayed next to {ChildNoun[2]} bed the rest of the night, allowing you to leave the basement and clean your wound. It heals completely before the sun is fully up, and you curse yourself for you carelessness.\nThe next day consisted of explaining to {ChildName} how being a wolf does not mean {ChildNoun[0]} can act how {ChildNoun[0]} want{plural}, and calling around to other family members to figure out how future full moons will go.");

                Finished = true;
            }

            routine = StartCoroutine(ShowEndText());
        }

        public void Continue()
        {
            if (IsWriting)
            {
                CanContinue = true;
                return;
            }

            NightSource.Play();
            EndGame.SetActive(false);
        }

        //addending text
        private void EndingText(string text, EndingActions action = EndingActions.None)
        {
            Ending_Text.Add(text, action);
        }

        private bool IsWriting = true;
        private IEnumerator ShowEndText()
        {
            var index = 1;
            foreach(var pair in Ending_Text)
            {
                //reset text and make it invisible
                Ending_TextBox.alpha = 0;
                Ending_TextBox.text = pair.Key;
                Ending_TextBox.ForceMeshUpdate();

                //Get the Text Info
                TMP_TextInfo textInfo = Ending_TextBox.textInfo;

                if (pair.Value != EndingActions.None)
                {
                    var Action = pair.Value;

                    if ((int)Action <= 6) //images
                    {
                        Ending_Image.gameObject.SetActive(true);
                        Ending_Image.DOFade(0, 0.25f).OnComplete(() =>
                        {
                            Ending_Image.sprite = Ending_Sprites[(int)Action];
                            Ending_Image.DOFade(1, 0.25f);
                        });



                        if ((int)Action == 1)
                        {
                            NightSource.PlayOneShot(nightclips[7]); //play toy destruction sounds
                        }
                    }
                    else //audio
                    {
                        switch (Action)
                        {
                            case EndingActions.Run_away:
                                NightSource.PlayOneShot(nightclips[4]);
                                break;
                            case EndingActions.Ripping:
                                NightSource.PlayOneShot(nightclips[5]);
                                break;
                            case EndingActions.Bone_crunching:
                                NightSource.PlayOneShot(nightclips[6]);
                                break;
                            case EndingActions.TV_Break:
                                NightSource.PlayOneShot(nightclips[9]);
                                break;
                            case EndingActions.Jump:
                                NightSource.PlayOneShot(nightclips[8]);
                                break;
                            case EndingActions.Scratching:
                                NightSource.PlayOneShot(nightclips[2]);
                                break;
                            case EndingActions.Sniffs:
                                NightSource.PlayOneShot(nightclips[10]);
                                break;
                            case EndingActions.Munch:
                                NightSource.PlayOneShot(nightclips[11]);
                                break;
                            default:
                                break;
                        }
                    }

                }

                for (int ii = 0; ii <= Ending_TextBox.text.Length; ii++)
                {
                    if (ii == Ending_TextBox.text.Length)
                    {
                        yield return new WaitForSeconds(1.5f);
                        continue;
                    }

                    // Get the index of the material used by the current character.
                    int materialIndex = textInfo.characterInfo[ii].materialReferenceIndex;

                    // Get the vertex colors of the mesh used by this text element (character or sprite).
                    var newVertexColors = textInfo.meshInfo[materialIndex].colors32;

                    // Get the index of the first vertex used by this text element.
                    int vertexIndex = textInfo.characterInfo[ii].vertexIndex;

                    // Set all to full alpha
                    newVertexColors[vertexIndex + 0].a = 255;
                    newVertexColors[vertexIndex + 1].a = 255;
                    newVertexColors[vertexIndex + 2].a = 255;
                    newVertexColors[vertexIndex + 3].a = 255;

                    Ending_TextBox.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

                    yield return new WaitForSeconds(0.03f);
                }

                Ending_TextBox.alpha = 225;
                Ending_Continue.enabled = true;
                IsWriting = (index != Ending_Text.Count);

                yield return new WaitForFixedUpdate();
                yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space) || CanContinue);
                yield return new WaitForFixedUpdate();

                Ending_Continue.enabled = false;
                Ending_Continue.gameObject.GetComponent<TextMeshProUGUI>().alpha = 0;

                if (!IsWriting)
                    Ending_Continue.gameObject.GetComponent<Button>().onClick.Invoke();

                CanContinue = false;
                index++;
            }

            yield return new WaitForFixedUpdate();
        }

        public void SetContinue(bool val)
        {
            CanContinue = true;
        }


        //TO DO: SHOW BABY MOVE FROM PLACE TO PLACE
        //MEANING IF TRAP BAITED, DOTWEEN BETWEEN THEM
        // IF NOT BAITED FADE HIM TO TREE
        // IF NO TREE FADE HIM TO DIGGING
        private void CheckNightTime(int chocolate)
        {
            bool tire = false, patch = false;
            int trap = 0;
            foreach (var action in ActionsPreformed.actions)
            {
                if (action.intObject.name.Contains("Patch"))
                {
                    patch = true;
                    NightSource.clip = nightclips[0];
                }
                else if (action.intObject.name.Contains("Traps"))
                {
                    if (action.option.need.Count > 0)
                    {
                        trap = 1;
                        NightSource.clip = nightclips[1];
                    }
                    else
                    {
                        trap = -1;
                    }
                }
                else if (action.intObject.name.Contains("Tire"))
                {
                    tire = true;
                }
            }

            var objtrap = GameObject.Find("Trap").transform;
            if (trap > 0)
            {
                objtrap.GetChild(0).gameObject.SetActive(true);
                objtrap.GetChild(1).gameObject.SetActive(true);
            }
            else if (trap < 0)
            {
                objtrap.GetChild(1).gameObject.SetActive(true);
                objtrap.GetChild(2).gameObject.SetActive(true);
            }

            var objTire = GameObject.Find("Tire").transform;
            if (tire)
            {
                objTire.GetChild(0).gameObject.SetActive(true);

                if (trap <= 0 && patch)
                    objTire.GetChild(1).gameObject.SetActive(true);

                NightSource.clip = nightclips[2];
            }
            else
            {
                objTire.GetChild(2).gameObject.SetActive(true);

                if (trap <= 0)
                    objTire.GetChild(3).gameObject.SetActive(true);

                NightSource.clip = nightclips[3];
            }

            var objPatch = GameObject.Find("Patch").transform;
            if (patch)
            {
                objPatch.GetChild(0).gameObject.SetActive(true);

            }
            else if (tire && trap <= 0)
            {
                objPatch.GetChild(1).gameObject.SetActive(true);
            }
        }



        public void SetGloves(bool value)
        {
            wearingGloves = value;
        }

        public IEnumerator LastBit(int flag)
        {
            yield return new WaitForSecondsRealtime(3.5f);
            Transition.gameObject.SetActive(true);
            yield return new WaitForSecondsRealtime(0.5f);
            NightTime.SetActive(false);
            Ending_Text.Clear();
            Ending_Image.gameObject.SetActive(false);

            AdjustVolume(0.5f, 0.5f, 0);
            AdjustVolume(0.35f, 0.5f, 1);

            EndGame.SetActive(true);
            bool wiggle = (childTired >= 5) ? false : true;

            if (ActionsPreformed.CheckAction("Trap"))
            {
                EndingText($"{ChildName} stares at you from inside the traps you laid earlier. You open it up and grab {ChildName}.", EndingActions.Trap);
            }
            else
            {
                EndingText($"You grab {ChildName} before {ChildNoun[0]} can escape past the fence.");
            }
            if (wearingGloves)
            {
                EndingText($"{ChildName} snaps at you, but the thick gloves prevent {ChildNoun[1]} from breaking the skin.\nYou tell {ChildNoun[1]} a firm \"No!\", and {ChildName} puts {ChildNoun[2]} ears backs, pouting.");

                if (!wiggle || flag == 4)
                {
                    EndingText($"You manage to wrangle {ChildNoun[1]} back into the house, despite {ChildName} attempts to wriggle away. {ChildName} gives in quickly once you sit on the couch with him, and settles into your arms, quickly falling asleep.\nYou brethe out a deep sigh of relief, wait out the rest of the night with {ChildNoun[1]}.", EndingActions.Happy);

                    EndingText($"This full moon was eventful to say the least, but turned out fine in the end.");
                }
                else
                {
                    EndingText($"You try to wrangle {ChildNoun[1]} back into the house, but {ChildName} fights you every step of the way. {ChildName} whines and pants, continuously nipping at your hands and arms, trying to break free.\n{ChildName} suddenly stops, looks at you and snaps at your face.\nYou stumble back, losing your grip, allowing {ChildNoun[0]} to leap from your arms, and over the fence.", EndingActions.Run_away);

                    EndingText($"You say a string of curses before running after {ChildNoun[1]}, spending half the night searching for {ChildNoun[1]}.\nYou find {ChildName} rooting around a neighbors trash, covered in gross and eating something {ChildNoun[0]} shouldn't. Plugging your nose, you grab {ChildNoun[1]} and rush home.\nIt's nearly sunrise by the time you get back, and {ChildName} vomits the second you enter the house. You clean {ChildNoun[1]} up, put {ChildNoun[1]} to bed, and then tend to the rest of the mess.", EndingActions.Vomit);

                    EndingText($"This full moon was eventful to say the least, but at least cleaning up vomit was the worst of it in the end.");
                }
            }
            else
            {
                EndingText($"{ChildName} snaps at you the second you touch {ChildNoun[1]}, sinking {ChildNoun[2]} teeth into your hand.", EndingActions.Bitten);

                if (!wiggle || flag == 4)
                {
                    EndingText($"You suppress a curse, but keep your grip on {ChildNoun[1]}. {ChildName} continues to try wriggle out of your grip, but you give {ChildNoun[1]} one sharp glare and {ChildName} settles down.\nYou carry {ChildNoun[1]} back into the house, quickly check the windows and doors are locked, and place {ChildNoun[1]} down next to you while you clean your wound.");

                    EndingText($"{ChildName} sits next to you, ears back, purposely not looking at you. {ChildName} eventually noses your arm, and licks your hand in an apology.");

                    EndingText($"The rest of the night passes quickly and quietly. {ChildName} stayed next to {ChildNoun[2]} bed the rest of the night. It heals completely before the sun is fully up.\nThe next day was a long one of dealing with a massive tantrum from {ChildName} after being grounded, while calling around to other family members to figure out how future full moons will go.");
                }
                else
                {
                    EndingText($"You supress a curse, but lose your grip on {ChildNoun[1]}. {ChildName} takes the chance, leaps out of your grasp and over the fence.\nYou rush into the house, hastily wrap your wound, and run after {ChildNoun[1]}.\nYou find {ChildName} rooting around a neighbors trash, covered in gross and eating something {ChildNoun[0]} shouldn't. Plugging your nose, you grab {ChildNoun[1]} and rush home.", EndingActions.Run_away);

                    EndingText($"It's nearly sunrise by the time you get home, and {ChildName} vomits the second you enter the house.\nYou clean {ChildNoun[1]} up, put {ChildNoun[1]} to bed, and then tend to the rest of the mess.", EndingActions.Vomit);

                    EndingText($"The rest of the night passes quickly and quietly. {ChildName} stayed next to {ChildNoun[2]} bed the rest of the night, allowing you to leave the basement and clean your wound. It heals completely before the sun is fully up.\nThe next day was a long one of dealing with a massive tantrum from {ChildName} after being grounded, while calling around to other family members to figure out how future full moons will go.");
                }
            }


            StartCoroutine(ShowEndText());
            Finished = true;
            Transition.SetTrigger("Toggle");

        }
    }
}