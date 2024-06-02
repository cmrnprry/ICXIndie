using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum BuyableItems
{
    //junkfood is to appease baby at the store
    Pumpkin, Watermelon, Meat, Peanutbutter, Chocolate, Rope, Wood, Nails, Needle, Thread, Banana, Null = -1, JunkFood = -2, Default = -3
}

public enum EndingActions
{
    //junkfood is to appease baby at the store
    Bitten = 0, Teddy = 1, TV = 2, Bed = 3, Trap = 4, Vomit = 5, Happy = 6, None = -1
}


namespace AYellowpaper.SerializedCollections
{
    public class GameManager : MonoBehaviour
    {
        private int Time = 300;

        [HideInInspector] public string ChildName = "Vick";
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
        public Transform Ending_Parent;
        public TextMeshProUGUI Example_text;

        private List<BuyableItems> BoughtItems = new List<BuyableItems>(); // list of everythign we have bought
        public List<InteractionObject> InteractionObjectList = new List<InteractionObject>(); //list of all the objects we can interact with at home
        private InteractableObject CurrentAction;

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
        }

        private void Start()
        {
            TotalTime_Text.text = $"Time Left: {Time}";
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

        public void SetParent(string change)
        {
            ParentName = change;
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

            foreach (var interaction in InteractionObjectList)
            {
                interaction.SetBuy(item);
            }
        }

        public int NumberBought()
        {
            return BoughtItems.Count;
        }

        public void RemoveTime(int time)
        {
            Time -= time;

            if (Time <= 0)
            {
                EndDayTimer();
                print("Next Phase");
                return;
            }

            TotalTime_Text.text = $"Time Left: {Time.ToString()}";
        }

        public void TimeReminder(InteractableObject obj, InteractionObject io, int index = -1)
        {
            var time = obj.Time_to_do;

            CurrentAction = obj;
            ReminderParent.SetActive(true);
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

        ///////////////////////////////////////////////////////// SECOND PHASE /////////////////////////////////////////////////////////
        private Coroutine routine = null;
        [SerializedDictionary("End Text", "Action")]
        private SerializedDictionary<string, EndingActions> Ending_Text = new SerializedDictionary<string, EndingActions>();
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
            bool bear = ActionsPreformed.CheckAction("Fix_Bear"); //if fixed: tired += 1 | plays with it, but breaks it again else: tired -= 0.5 | makes him frustrated
            bool playmat = ActionsPreformed.CheckAction("Treats"); // if pb: tired += 1  if candy: tired -= 2 || makes him hyper, but will crash and puke soon  else: tired -= 1
            bool tv = ActionsPreformed.CheckAction("TV"); //if cartoon: tired += 1   if nature: tired += 2 | neighbors annoyed by barking / howling ?

            //Mothing bad, but depends on other things
            bool bed = ActionsPreformed.CheckAction("Bed"); // tired += 0.5 | digs around but doesn't reall care   if you give him a lot of chocolate or tire him out, he will nest in there

            //prevents escape
            bool windows = ActionsPreformed.CheckAction("Window"); //tired += 0.5 \ tires an escape


            //START TEXT
            string chocolateString = $"As the full moon rises, {ChildName} excitedly runs around the basement while you finish any last minute preparations.";
            if (ChildObject.FedChocolate)
            {
                chocolateAmount += 1;
                chocolateString = $"As the full moon rises, {ChildName} is pulling on your clothes, asking for more sweets. You tell him to go sit down while you try to finish any last minute preparations.";
            }
            EndingText(chocolateString);

            string next = (windows) ? $"It isn't long before the moon light shines into the basement through cracks in the wooden boards, and your little {ChildName} fully transforms." : $"It isn't long before the moon light shines into the basement through the windows, and your little {ChildName} fully transforms.";
            EndingText(next);

            EndingText($"It never sounds or looks pleasent, but he says he doesn't hurt when you ask.");

            next = $"It isn't long before he is running around the room, sniffing around all the things you left out for him.";
            if (ChildObject.FedChocolate)
                next += $" The chocolate you fed him earlier seemed to make him more hyper than normal.";

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
                        EndingText($"While running in circles, {ChildName} locks eyes with his bear stuffie, grabs it in his jaws and violently shakes it.", EndingActions.Teddy);
                        EndingText($"The bear never stood a chance.");
                    }

                    else
                    {
                        EndingText($"While running in circles, {ChildName} sees his bear, and starts playing with. He throws it around in the air, attempting to catch it before it hits the ground.", EndingActions.Teddy);
                        EndingText($"By the time he's done with it, his sharp teeth have caused your stitches to rip and the bear to fall apart.");
                    }


                    childTired += 1;
                }
                else if (index == 1)
                {
                    temp_bool = false;
                    if (chocolateAmount > 0)
                        EndingText($"While running in circles, {ChildName} sees his broken bear, and grabs it in his jaws, violently shaking it. The already ruined bear is ripped to pieces, and you worry you won't be able to fix it later.", EndingActions.Teddy);
                    else
                        EndingText($"While running in circles, {ChildName} sees his broken bear, and noses it before whining at it. He tries to play with it, but the stuffed toy falls into further disrepair.", EndingActions.Teddy);

                    childTired -= 0.5f;
                }
            }
            if (cat_tower)
            {
                if (bear && temp_bool)
                    EndingText($"Finished playing with his bear, {ChildName} turns to his tower. He runs up the ladder and perches on top before something in the room catches his attention.");
                else if (bear && !temp_bool)
                    EndingText($"Tired of his broken toy, {ChildName} turns to his tower. He runs up the ladder and perches on top before something in the room catches his attention.");
                else
                    EndingText($"{ChildName} turns to his tower. He runs up the ladder and perches on top before something in the room catches his attention.");

                if (ChildObject.FedChocolate)
                    EndingText($"He leaps from the top of his perch, and sprints to his new fixation.");
                else
                    EndingText($"{ChildName} whines and readies himself before jumping from the top of his perch, and prances to his new fixation.");

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

                        EndingText($"{ChildName} pounces on the mat and sniffs out all the treats hidden in it. He wolfs down all the chocolate treats he could find, and then begins to zoom around the room.");
                        break;
                    case BuyableItems.Peanutbutter:
                        childTired += 2;

                        EndingText($"{ChildName} pounces on the mat and sniffs out all the treats hidden in it. His tail goes a mile a minute, and once he notices, begins to chase it.");
                        break;
                    default: // didn't put anything
                        childTired -= 0.5f;

                        EndingText($"{ChildName} sniffs and digs at the mat, but once he realizes there's nothing there, moves on.");
                        break;
                }
            }

            if (tv)
            {
                string temp = "";
                if (ActionsPreformed.FindOptionIndex("TV") == 0)
                {
                    temp = $"The intro song of cartoon starts playing. {ChildName}'s ears instantly perk up, and he runs over. He sits and wags his tail, cocking his head from side to side as the characters on screen ask questions.";
                    childTired += 1;
                }
                else
                {
                    temp = $"The wolf pack on the nature show playing start howling.  {ChildName}'s ears instantly perk up, and he runs over. He wags his tail and begins to bark and howl with the wolves on screen.";
                    childTired += 2;
                }

                EndingText(temp, EndingActions.TV);
                EndingText($"He watches the show happily, until he tries to jump up and presses a button, turning it off. He whines and paws at the TV, trying to turn it back on. When he can't, {ChildName} grwos frustrated and bites and pulls at the bars surrounding the TV.");
                EndingText($"{ChildName} easily pulls and bends the metal. It isn't long before the the TV lays in a wrecked pile on the floor. Satisfied with his destruction, he trots away.");
            }
            if (watermelon)
            {
                if (tv)
                {
                    EndingText("The taste of metal and electronics seemed to leave a bad taste in his mouth, and he makes his way to the watermelon on the floor.");
                }

                if (treatTypes == BuyableItems.Chocolate)
                    EndingText($"{ChildName} gives the watermelon halves a few tentative licks, but instead of chewing or eating, he begins to dig into them. It makes a huge mess, but he seems to be having fun.");
                else
                    EndingText($"{ChildName} gives the watermelon halves a few tentative licks before ripping it to bits. You're not sure how much he's actually eating vesus how much is mush on the floor, but he seems to be having fun.");

                childTired += 1.5f;
            }
            if (pumpkin)
            {
                string temp = "";
                if (watermelon)
                {
                    temp = "Covered in watermelon bits, ";
                }

                temp += (chocolateAmount > 0) ? $"{ChildName} sniffs out the meat pumpkin and bats it around for a while. He knaws on the pumpkin, but quickly moves on. The sugar high seems to be wearing off." : $"{ChildName} sniffs out the meat pumpkin and bats it around, knawing on the pumpkin and scarfing down any meat pieces that fall out. ";

                float tired = (chocolateAmount > 0) ? 1f : 1.5f;
                EndingText(temp);
                childTired += tired;
            }

            if (bed)
            {
                childTired += 0.5f;

                string temp = "";
                if (chocolateAmount >= 2)
                {
                    EndingText($"{ChildName} is slowly walking around the room, and stops next his bed, heaving. Before you can do anything, he vomits everything all over his. He wimpers and curls up next to it.", EndingActions.Vomit);
                    EndingText($"The rest of the night passes quickly. You stay by his side the entire time, rubbing his back and giving him water.  {ChildName} vomits a few more times before he transforms back.");
                    EndingText($"You and {ChildName} spend the rest of the day curled up together. You make a mental note to not give him chocolate again, no matter how much he begs.");
                    routine = StartCoroutine(ShowEndText());
                    Finished = true;
                    return;
                }
                else if (chocolateAmount > 0)
                {
                    temp = $"{ChildName} hops on his bed and begins to dig in it. He stops, heaves for a moment, then continues to dig.";
                }
                else
                    temp = $"{ChildName} hops on his bed and begins to dig in it.";


                //TODO: FIGURE OUT THIS NUMBER
                if (childTired >= 5)
                {
                    temp += " He turns in circles before plopping down and falling asleep.";
                    EndingText(temp, EndingActions.Happy);
                    EndingText($"The rest of the night passes quickly and quietly. {ChildName} stays in his bed all night, allowing you to get some sleep of your own.");
                    EndingText($"You wake in the morning to a messy basement, and a well slept child. You pat yourself on the back, and wake {ChildName} to get ready for the day.");
                    routine = StartCoroutine(ShowEndText());
                    Finished = true;
                    return;
                }


                temp += " He continues to dig in his bed, and even starts biting and tearing at it";
                EndingText(temp, EndingActions.Bed);
                EndingText($"{ChildName} rips the bed open, pulling all the stuffing out before eyeing the basement windows.");
            }
            if (!windows)
            {
                EndingText($"{ChildName} paces around, looking up at the unboarded windows. He whines and paws at the walls before standing up on his hind legs. {ChildName} backs up and jumps, squeezing out of the basement window and into the backyard.");
                CheckNightTime(chocolateAmount);
            }
            else
            {
                EndingText($"{ChildName} paces around, and attempts to escape, but the wooden boards hold firm. Unable to escape through the window, he scratches at the basement door.", EndingActions.Bitten);
                EndingText($"When you try to stop him, he turns around and bites your hand. You supress a swear, and wrap your hand in your shirt.");
                EndingText($"{ChildName} almost immediately realizes his error, and licks your leg in apology. You tell him to go lay down, and he slinks to his destroyed bed.");
                EndingText($"The rest of the night passes quickly and quietly. {ChildName} stayed next to his bed the rest of the night night, allowing you to leave the basement and clean your wound. It heals completely before the sun is fully up, and yoiu surse yourself for you carelessness.");
                EndingText($"The next day consisted of explaining to {ChildName} how being a wolf does not mean he can act how he wants, and calling around to other family members to figure out how future full moons will go.");
            }

            routine = StartCoroutine(ShowEndText());
        }

        public void Continue()
        {
            EndGame.SetActive(false);
        }

        //addending text
        private void EndingText(string text, EndingActions action = EndingActions.None)
        {
            Ending_Text.Add(text, action);
        }

        private IEnumerator ShowEndText()
        {
            //For each text chunk in Ending_Text, lets create a chunk of text
            foreach (var pair in Ending_Text)
            {
                TextMeshProUGUI Text_Object = Instantiate(Example_text, Ending_Parent);
                Text_Object.alpha = 0;
                Text_Object.text = pair.Key;
                Text_Object.gameObject.SetActive(true);
                Text_Object.ForceMeshUpdate();
            }

            yield return new WaitForFixedUpdate();

            for (int kk = 1; kk < Ending_Parent.childCount; kk++)
            {
                TextMeshProUGUI Text = Ending_Parent.GetChild(kk).gameObject.GetComponent<TextMeshProUGUI>();
                string contents = Text.text;

                //Get teh Text Info
                TMP_TextInfo textInfo = Text.textInfo;

                if (Ending_Text[contents] != EndingActions.None)
                {
                    var Action = Ending_Text[contents];
                    Ending_Image.gameObject.SetActive(true);

                    Ending_Image.sprite = Ending_Sprites[(int)Action];
                }

                for (int ii = 0; ii <= contents.Length; ii++)
                {
                    if (ii == contents.Length)
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

                    Text.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

                    yield return new WaitForSeconds(0.03f);
                }

                Text.alpha = 225;
            }
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

        public void SetName(string name)
        {
            ChildName = name;
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

            foreach (Transform child in Ending_Parent)
            {
                if (child.gameObject.activeSelf)
                    Destroy(child.gameObject);
            }

            EndGame.SetActive(true);
            bool wiggle = (childTired >= 5) ? false : true;


            if (wearingGloves)
            {
                EndingText($"{ChildName} snaps at you, but the thick gloves prevent him from breaking the skin.");
                EndingText("You tell him a firm \"No!\", and he puts his ears backs, pouting.");

                if (!wiggle || flag == 4)
                {
                    EndingText($"You manage to wrangle {ChildName} back into the house, dispite his attempts to wiggle away.");
                    EndingText("He gives in quickly, and settles into your arms, quickly falling asleep.");
                    EndingText($"You double check all the doors and windows, and wait out the rest of the nigth with {ChildName}.");
                }
                else
                {
                    EndingText($"You try to wrangle {ChildName} back into the house, but he fights you every step of the way.");
                    EndingText("He whines and pants, continously nipping at your hands and arms, trying to break free.");
                    EndingText("He suddenly stops, looks at you and snaps at your face.");
                    EndingText("You stumble back, losing your grip and he leaps from your arms, and over the fence.");
                    EndingText("You say a string of curses before running after him.");
                }
            }
            else
            {
                EndingText($"{ChildName} snaps at you, sinking his teeth into your hand.", EndingActions.Bitten);


                if (!wiggle || flag == 4)
                {
                    EndingText($"You supress a curse, but keep your grip on {ChildName}.");
                    EndingText("He continues to try wriggle out of your grip, but you give him one sharp glare and he settles down.");
                    EndingText($"You carry {ChildName} back into the house, check the windows and doors, and place him down next to you while you clean your wound.");
                    EndingText($"{ChildName}, sits next to you, ears back, and purposely doesn't look at you. He eventually noses your arm, and licks your hand in an apology.");
                    EndingText("You pat his head with your bandaged hand, and the two of you spend the rest of the night curled up on the couch together.");
                }
                else
                {
                    EndingText($"You supress a curse, but lose your grip on {ChildName}.");
                    EndingText("He takes the chance, leaps out of your grasp and over the fence.");
                    EndingText($"You rush into the house, hastily wrap your wound, and run after him.");
                }
            }


            StartCoroutine(ShowEndText());
            Transition.SetTrigger("Toggle");

        }
    }
}