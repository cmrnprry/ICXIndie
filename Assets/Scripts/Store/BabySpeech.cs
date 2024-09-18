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
        private bool canMove = true;

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
            int rand = Random.Range(0, 10);
            if (corners <= 2 && rand < 5)
                StoreManager.Instance.SpawnParentMessage();

            if (corners <= 2)
            {
                StoreManager.Instance.BabyMeter -= 5;
                Destroy(this.gameObject);
            }
            else
                coroutine = StartCoroutine(ForceTimer());

            canMove = true;
            s.Kill();
            s = null;
            AddForce();
        }

        public override void OnPointerDown(PointerEventData pointerEventData)
        {
            base.OnPointerDown(pointerEventData);

            StopCoroutine(coroutine);
            canMove = false;
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

        public override void OnDrag(PointerEventData eventData)
        {
            var screenPoint = Input.mousePosition;

            screenPoint.z = 10.0f; //distance of the plane from the camera
            transform.position = Camera.main.ScreenToWorldPoint(screenPoint);
        }

        int CheckIfOnScreen()
        {
            Vector3[] Corners = new Vector3[4];
            rect_transform.GetWorldCorners(Corners);
            int isObjectOverflowing = 4;

            Corners[0].z = 10.0f;
            Corners[1].z = 10.0f;
            Corners[2].z = 10.0f;
            Corners[3].z = 10.0f;


            var one = Camera.main.WorldToScreenPoint(Corners[0]);
            var two = Camera.main.WorldToScreenPoint(Corners[1]);
            var three = Camera.main.WorldToScreenPoint(Corners[2]);
            var four = Camera.main.WorldToScreenPoint(Corners[3]);

            if (Corners != null)
            {
                if (one.x <= 0 || one.x >= Screen.width || one.y <= 0 || one.y >= Screen.height)
                    isObjectOverflowing -= 1;

                if (two.x <= 0 || two.x >= Screen.width || two.y <= 0 || two.y >= Screen.height)
                    isObjectOverflowing -= 1;

                if (three.x <= 0 || three.x >= Screen.width || three.y <= 0 || three.y >= Screen.height)
                    isObjectOverflowing -= 1;

                if (four.x <= 0 || four.x >= Screen.width || four.y <= 0 || four.y >= Screen.height)
                    isObjectOverflowing -= 1;
            }

            return isObjectOverflowing;
        }

        private IEnumerator ForceTimer()
        {
            float time = Random.Range(0.5f, 2.5f);
            yield return new WaitForSeconds(time);

            if (canMove)
            {
                AddForce();
                CyclesAlive += 1;
            }

        }

        private void AddForce()
        {
            int rand = Random.Range(0, 4);
            float time = Random.Range(0.25f, 1.5f);
            var ToPosition = StoreManager.Instance.GetRandomPositionInBounds();

            if (!canMove)
                rand = 3;

            if (CyclesAlive == 4)
            {
                float scale = Random.Range(1.5f, 2);
                t = transform.DOScale(new Vector3(scale, scale, 0), time);

                rand = 5;
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
                    Debug.LogWarning("Could not perform Movement");
                    break;
            }

            t.OnComplete(() =>
            {
                if (canMove)
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