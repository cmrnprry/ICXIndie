using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum BuyableItems
{
    Pumpkin, Watermelon, Meat, Peanutbutter, Chocolate, Rope, Wood, Nails, Sewing, Banana, Null = -1
}

public class GameManager : MonoBehaviour
{
    private int Time = 1000;

    private string ChildName = "Vick";
    private string ParentName = "Mom";
    [HideInInspector] public float childTired = 0;

    [Header("Preform Action?")]
    [SerializeField]
    private GameObject ReminderParent;
    [SerializeField]
    private TextMeshProUGUI Reminder_Text;
    private ObjectAbstract Current_Interaction = null;
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
    private TextMeshProUGUI EndText;
    [SerializeField]
    private Animator Transition;

    private List<BuyableItems> BoughtItems = new List<BuyableItems>();
    public List<InteractionObject> Objects = new List<InteractionObject>();
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
        TotalTime_Text.text = $"Time Left: {Time.ToString()}";
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
            BoughtItems.ForEach(bought => { if (item.Contains(bought.ToString())) { Bought =  true; } });
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
        foreach (var obj in Objects)
        {
            obj.SetBuy(item);
        }

    }

    public int NumberBought()
    {
        return BoughtItems.Count;
    }

    public void RemoveTime(int time)
    {
        Time -= time;

        if (Time < 0)
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
        var time = obj.preparable_info[index].Time_to_do;

        CurrentAction = obj;
        ReminderParent.SetActive(true);
        Reminder_Text.text = $"This action will cost {time.ToString()}. You have {Time.ToString()} minutes left.\n Continue?";
        Current_Interaction = io;
        Index = index;
    }

    public void PauseInteraction(bool on)
    {
        foreach (var item in Objects)
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
            ActionsPreformed.ActionPerformed(CurrentAction.preparable_info[Index]);
            RemoveTime(CurrentAction.preparable_info[Index].Time_to_do);
        }
        else
        {
            CheckDoubles();
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


    private void CheckDoubles()
    {
        foreach (var item in Objects)
        {
            if (CurrentAction.name == item.obj.name && item.GetComponent<Image>().color.a < 1)
            {
                item.SetInteraction();
                break;
            }
        }
    }


    ///////////////////////////////////////////////////////// SECOND PHASE /////////////////////////////////////////////////////////
    private Coroutine routine = null;
    private string end = "";

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
        EndText.text = "";
        //check how much chocolate the baby had
        int chocolateAmount = 0;


        //objects that have no major implimications, and just affects tiredness
        bool cat_tower = ActionsPreformed.CheckAction("Cat_Tower"); //tired += 0.5 | runs around but doesn't really care
        bool pumpkin = ActionsPreformed.CheckAction("Pumpkin"); //tired += 1.5 | loves it
        bool watermelon = ActionsPreformed.CheckAction("Watermelon"); //tired -= 0.5 | likes it but has opposite effect

        //usually good, but can be bad
        bool bear = ActionsPreformed.CheckAction("Watermelon"); //if fixed: tired += 1 | plays with it, but breaks it again else: tired -= 0.5 | makes him frustrated
        bool playmat = ActionsPreformed.CheckAction("Treats"); // if pb: tired += 1  if candy: tired -= 2 || makes him hyper, but will crash and puke soon  else: tired -= 1
        bool tv = ActionsPreformed.CheckAction("TV"); //if cartoon: tired += 1   if nature: tired += 2 | neighbors annoyed by barking / howling ?

        //Mothing bad, but depends on other things
        bool bed = ActionsPreformed.CheckAction("Bed"); // tired += 0.5 | digs around but doesn't reall care   if you give him a lot of chocolate or tire him out, he will nest in there

        //prevents escape
        bool windows = ActionsPreformed.CheckAction("Window"); //tired += 0.5 \ tires an escape


        //START TEXT
        string chocolateString = $"As the moon rises, {ChildName} is excited, but not too hyper.";
        if (ChildObject.FedChocolate)
        { chocolateAmount += 1; chocolateString = $"As the moon rises, {ChildName} is pulling on your clothes, asking for more sweets."; }
        EndingText(chocolateString);

        BuyableItems treatTypes = ActionsPreformed.FindNeed("Treats");

        //CHECK WHAT PLAYER DID IN A SET ORDER
        if (cat_tower)
        {
            EndingText($"{ChildName} runs up the ladder and perches on top before growing bored and moving on. ");
            childTired += 0.5f;
        }
        if (playmat)
        {
            string temp = "";
            switch (treatTypes)
            {
                case BuyableItems.Chocolate:
                    temp = $"{ChildName} pounces on the mat and sniffs out all the treats hidden in it. After eating all the chocolate treats, he begins to run in circles around the room.";
                    childTired += 1;
                    chocolateAmount += 1;
                    break;
                case BuyableItems.Peanutbutter:
                    temp = $"{ChildName} pounces on the mat and sniffs out all the treats hidden in it. His tail goes a mile a minute, and after eating all the peanutbutter treats, he notices and begins to chase it.";
                    childTired -= 2;
                    break;
                default: // didn't put anything
                    temp = $"{ChildName} sniffs and digs at the mat, but once he realizes there's nothing there, moves on.";
                    childTired -= 0.5f;
                    break;
            }

            EndingText(temp);
        }
        if (bear)
        {
            if (ActionsPreformed.FindOptionName("Bear").Contains("Sew"))
            {
                if (chocolateAmount >= 2)
                    EndingText($"While running in circles, {ChildName} locks eyes with his bear stuffie, grabs it in his jaws and violently shakes it. The bear never stood a chance.");
                else if (chocolateAmount > 0)
                    EndingText($"After capturing his tail, {ChildName} sees his bear, and starts playing with. He throws it around in the air before getting bored and moving on.");
                else
                    EndingText($"{ChildName} sees his bear, and starts playing with. He throws it around in the air before getting bored and moving on.");

                childTired += 1;
            }
            else
            {
                if (treatTypes == BuyableItems.Chocolate)
                    EndingText($"While running in circles, {ChildName} sees his broken bear, and grabs it in his jaws, violently shaking it. The already ruined bear is ripped to pieces.");
                else if (treatTypes == BuyableItems.Peanutbutter)
                    EndingText($"After capturing his tail, {ChildName} sees his broken bear, and noses it before whining at it.");
                else
                    EndingText($"{ChildName} sees his broken bear, and noses it before whining at it.");
                childTired -= 0.5f;
            }
        }

        if (watermelon)
        {
            if (treatTypes == BuyableItems.Chocolate)
            {
                EndingText($"The watermelon halves peak his interest, but instead of chewing or eating, he begins to dig into them. {ChildName} makes a huge mess, but seems to have had fun.");
                childTired += 1f;
            }
            else
                EndingText($"The watermelon halves peak his interest, and rips it to bits. You're not sure how much he ate, {ChildName} but seems to have had fun.");

            childTired -= 0.5f;
        }
        if (pumpkin)
        {
            string temp = "";
            if (watermelon)
            {
                temp = "Covered in watermelon bits, ";
                temp += (chocolateAmount > 0) ? $"{ChildName} sniffs out the meat pumpkin and bats it around for a while. He knaws on the pumpkin, but quickly moves on. The sugar high seems to be wearing off." : $"{ChildName} sniffs out the meat pumpkin and bats it around, knawing on the pumpkin and scarfing down any meat piece he finds. ";
            }
            else
                temp = (chocolateAmount > 0) ? $"{ChildName} sniffs out the meat pumpkin and bats it around for a while. He knaws on the pumpkin, but quickly moves on. The sugar high seems to be wearing off." : $"{ChildName} sniffs out the meat pumpkin and bats it around, knawing on the pumpkin and scarfing down any meat piece he finds. ";

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
                temp = $"{ChildName} stands next his bed and begins to heave. Before you can do anything, he vomits everything all over his. He wimpers and curls up next to it. ";
                //TODO: jump to baby is real sick
            }
            else if (chocolateAmount > 0)
            {
                temp = $"{ChildName} hops on his bed and begins to dig in it. He stops, heaves for a moment, but doesn't throw anything up.";
            }
            else
                temp = $"{ChildName} hops on his bed and begins to dig in it.";


            //TODO: FIGURE OUT THIS NUMBER
            if (childTired >= 10 && tv)
                temp += " He turns in circles, preparing for sleep, but something on the tv catches his attention. He plops into his bed, watching the TV.";
            else if (childTired >= 10)
                temp += " Hes turns in circles before plopping down and falling asleep.";
            else if (tv)
                temp += " Something on the TV catches his interest and he runs over.";
            else
                temp += " He continues to dig in his bed before eyeing the basement windows.";


            EndingText(temp);
        }
        if (tv)
        {
            string temp = "";
            if (ActionsPreformed.FindOptionName("TV") == "Cartoons")
            {
                temp = $"The cartoon playing is one of his favourites in human form. He sits and wags his tail, cocking his head from side to side as the characters on screen ask questions.";
                childTired -= 1;

                //TODO: FIGURE OUT THIS VARIABLE
                if (childTired >= 10)
                {
                    temp += $"\n\n {ChildName} lays down watching the characters, trying to keep his eyes open before eventually falling asleep.";
                }

            }
            else
            {
                temp = $"The nature show is playing a wolf pack playing. {ChildName} wags his tail and begins to bark and howl with the wolves on screen.";
                childTired += 1;

                //TODO: FIGURE OUT THIS VARIABLE
                if (childTired >= 10)
                {
                    temp += $"\n\n {ChildName} lays down watching the wolves, trying to keep his eyes open before eventually falling asleep.";
                }
            }

            EndingText(temp);
        }
        if (!windows)
        {
            EndingText($"{ChildName} paces around, looking up at the unboarded windows. He whines and paws at the walls before standing up on his hind legs. {ChildName} backs up and jumps, squeezing out of the basement window and into the backyard.");
            CheckNightTime(chocolateAmount);
        }

        //TODO: check if he can't get out, but is also still alil hyper

        routine = StartCoroutine(ShowEndText());
    }

    public void Continue()
    {
        if (childTired >= 10)
        {
            //load main menu?
        }
        else
        {
            EndGame.SetActive(false);
        }
    }

    //addending text
    private void EndingText(string text)
    {
        end += (text + "\n\n");
    }

    private IEnumerator ShowEndText()
    {
        var array = end.Split("\n\n");
        var index = 0;
        var current = array[0].Length + 1;

        EndText.alpha = 0;
        EndText.text = end;
        EndText.ForceMeshUpdate();
        TMP_TextInfo textInfo = EndText.textInfo;

        while (index < array.Length)
        {
            for (int ii = 0; ii < end.Length; ii++)
            {
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

                EndText.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

                yield return new WaitForSeconds(0.03f);

                if (ii >= current && index < array.Length)
                {
                    index++;
                    current += array[index].Length + 2;
                    yield return new WaitForSeconds(1.5f);
                }
            }
        }

        EndText.alpha = 225;
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
            }
            else if (action.intObject.name.Contains("Traps"))
            {
                if (action.option.need.Count > 0)
                    trap = 1;
                else
                    trap = -1;
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

        }
        else
        {
            objTire.GetChild(2).gameObject.SetActive(true);

            if (trap <= 0)
                objTire.GetChild(3).gameObject.SetActive(true);
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

}
