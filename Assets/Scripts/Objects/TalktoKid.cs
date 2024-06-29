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
        public string parent;
        public string kid;
    }

    public class TalktoKid : MonoBehaviour
    {
        public List<Convo> Convo;
        public List<TextMeshProUGUI> ButtonText;
        public ChildObject childObject;
        public Image cover;
        public TextMeshProUGUI parent, child;

        private void Start()
        {
            SetNames();
        }

        private void OnEnable()
        {
            GameManager.OnChangeChildName += SetNames;
        }

        private void SetNames()
        {
            foreach (TextMeshProUGUI text in ButtonText)
            {
                string change = GameManager.Instance.SetChildName(text.text);
                text.text = change;
                Debug.Log(text.text);
            }
        }

        //when player clicks on the button
        public void OnClick(int topic)
        {
            int rand =  Random.Range(0, Convo.Count);
            int picked = Convo[rand].topic;

            if (picked != topic)
            {
                OnClick(topic);
                return;
            }

            parent.text = GameManager.Instance.SetChildName(Convo[rand].parent);
            child.text = Convo[rand].kid;

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