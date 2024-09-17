using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AYellowpaper.SerializedCollections
{
    [System.Serializable]
    public struct Convo
    {
        public int topic;
        public string topic_title;
        public string parent;
        public string kid;
        public bool spoken;
    }

    public class TalktoKid : MonoBehaviour
    {
        public List<Convo> Convo;
        public List<TextMeshProUGUI> ButtonText;
        public ChildObject childObject;
        public Transform Option_Parent;
        public Button Button_Prefab;
        public Image cover;
        public TextMeshProUGUI parent, child;

        private void Start()
        {
            CreateTalking();
        }

        private void OnEnable()
        {
            SetNames();
            GameManager.OnChangeChildName += SetNames;
        }

        private void OnDisable()
        {
            GameManager.OnChangeChildName -= SetNames;
        }

        private void SetNames()
        {
            foreach (TextMeshProUGUI box in ButtonText)
            {
                string change = GameManager.Instance.SetChildName(box.text);
                box.text = change;
                box.ForceMeshUpdate();
            }
        }

        private void CreateTalking()
        {
            int index = -1;
            foreach(Convo item in Convo)
            {
                if (index < item.topic)
                {
                    Button button = Instantiate(Button_Prefab, Option_Parent);
                    button.onClick.AddListener(() =>
                    {
                        OnClick(item.topic);
                        GameManager.Instance.RemoveTime(2);
                    });

                    button.gameObject.SetActive(true);
                    button.gameObject.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = item.topic_title;
                    index++;
                }
            }

        }

        //when player clicks on the button
        public void OnClick(int topic)
        {
            //find number of options within the topic
            int start = -1, total = 0, choose = -1;
            for (int ii = 0; ii < Convo.Count; ii++)
            {
                if (Convo[ii].topic == topic)
                {
                    if (start == -1)
                        start = ii;

                    total++;
                }
            }

            total += start;
            choose = start;

            while (Convo[choose].spoken)
            {
                if (choose >= total - 1)
                {
                    for(int kk = start; kk < total; kk++)
                    {
                        var t = Convo[kk];
                        t.spoken = false;
                        Convo[kk] = t;
                    }

                    choose = start;
                }
                else
                {
                    choose ++;
                }
            }

            parent.text = GameManager.Instance.SetChildName(Convo[choose].parent);
            child.text = GameManager.Instance.SetParentName(Convo[choose].kid);

            var temp = Convo[choose];
            temp.spoken = true;
            Convo[choose] = temp;

            StartCoroutine(Talking());
        }

        private IEnumerator Talking()
        {
            parent.gameObject.transform.parent.gameObject.SetActive(true);

            yield return new WaitForSeconds(2.5f);

            child.gameObject.transform.parent.gameObject.SetActive(true);

            yield return new WaitForSeconds(2.5f);
            child.gameObject.transform.parent.gameObject.SetActive(false);
            parent.gameObject.transform.parent.gameObject.SetActive(false);

            if (childObject != null)
                childObject.FlipInteration(false);

            if (cover != null)
                cover.enabled = false;
        }
    }
}