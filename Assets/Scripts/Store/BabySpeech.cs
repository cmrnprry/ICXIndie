using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

namespace AYellowpaper.SerializedCollections
{
    public class BabySpeech : DragAndDrop
    {
        private Rect screenRect;
        private Rigidbody2D body;
        private Tween t = null;
        private Sequence s = null;
        private Coroutine coroutine = null;
        private int CyclesAlive = 0;

        // Start is called before the first frame update
        void Start()
        {
            screenRect = new Rect(0, 0, Screen.width, Screen.height);
            body = GetComponent<Rigidbody2D>();

            coroutine = StartCoroutine(ForceTimer());
        }

        public override void OnPointerUp(PointerEventData pointerEventData)
        {
            base.OnPointerUp(pointerEventData);

            int corners = CheckIfOnScreen();
            if (corners >= 2)
                StoreManager.Instance.SpawnParentMessage();
            if (corners >= 3)
            {
                Destroy(this.gameObject);
                StoreManager.Instance.BabyMeter -= 15;
            }


            s.Kill();
            s = null;
            AddForce();
        }

        public override void OnPointerDown(PointerEventData pointerEventData)
        {
            base.OnPointerDown(pointerEventData);

            StopCoroutine(coroutine);
            t.Kill();
            t = null;
            var time = 1.5f;

            if (StoreManager.Instance.BabyMeter >= 100)
            {
                time = 0.25f;
            }
            else if (StoreManager.Instance.BabyMeter >= 75)
            {
                time = 0.5f;
            }
            else if (StoreManager.Instance.BabyMeter >= 50)
            {
                time = 1f;
            }

            s = DOTween.Sequence();
            s.Append(body.DORotate(10, time)).Append(body.DORotate(-10, time)).SetLoops(-1, LoopType.Yoyo);
        }

        int CheckIfOnScreen()
        {
            Vector3[] objectCorners = new Vector3[4];
            rect_transform.GetWorldCorners(objectCorners);
            int isObjectOverflowing = 0;

            foreach (Vector3 corner in objectCorners)
            {
                if (!screenRect.Contains(corner))
                {
                    isObjectOverflowing += 1;
                }
            }

            return isObjectOverflowing;
        }

        private IEnumerator ForceTimer()
        {
            float time = Random.Range(0.5f, 2.5f);
            yield return new WaitForSeconds(time);
            AddForce();
            CyclesAlive += 1;
        }

        private void AddForce()
        {
            int rand = Random.Range(0, 4);
            float time = Random.Range(0.25f, 1.5f);
            var ToPosition = StoreManager.Instance.GetRandomPositionInBounds();

            if (CyclesAlive == 4)
            {
                float scale = Random.Range(1.5f, 2);
                t = transform.DOScale(new Vector3(scale, scale, 0), time);
            }


            switch (rand)
            {
                case 0: //Move x&y
                    t = this.transform.DOMove(ToPosition, time);
                    break;
                case 1: //Move X
                    t = this.transform.DOMoveX(ToPosition.x, time);
                    break;
                case 2: //Move Y
                    t = this.transform.DOMoveY(ToPosition.y, time);
                    break;
                case 3: // rotate
                    float rotateZ = Random.Range(90f, 270f);

                    t = transform.DORotate(new Vector3(0, 0, rotateZ), time);
                    break;
                default:
                    Debug.LogError("Could not perform Movement");
                    break;
            }

            t.OnComplete(() =>
            {
                coroutine = StartCoroutine(ForceTimer());
            });
        }

        void OnCollision2DEnter(Collision collision)
        {
            if (collision.gameObject.tag == "Speech")
            {
                Physics2D.IgnoreCollision(this.GetComponent<BoxCollider2D>(), collision.gameObject.GetComponent<BoxCollider2D>());
            }
        }

        void OnCollision2DStay(Collision collision)
        {
            if (collision.gameObject.tag == "Speech")
            {
                Physics2D.IgnoreCollision(this.GetComponent<BoxCollider2D>(), collision.gameObject.GetComponent<BoxCollider2D>());
            }
        }

    }
}