using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using YSGame;

namespace ChatingRoom
{
    public class LunDaoAmrMagOnline
    {
        private LunDaoAmrMagOnline.LunDaoAnimationController controller;
        private Queue<Transform> heChengQueue = new Queue<Transform>();
        private Queue<Image> useCardQueue = new Queue<Image>();
        private Queue<List<Image>> completeLunTiQueue = new Queue<List<Image>>();
        private bool heChengLock = true;
        private bool useCardLock = true;
        private bool completeLunTiLock = true;
        public LunDaoAmrMagOnline.LunDaoAnimationController PlayStartLunDao()
        {
            GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.LoadAnimation("StartLunDao"), LunDaoManagerOnline.inst.transform);
            Animator component = gameObject.GetComponent<Animator>();
            AnimatorUtils component2 = gameObject.GetComponent<AnimatorUtils>();
            this.controller = new LunDaoAmrMagOnline.LunDaoAnimationController(component, component2);
            return this.controller;
        }


        public void AddHeCheng(Transform transform)
        {
            this.heChengQueue.Enqueue(transform);
            this.PlayHeCheng();
        }


        public void AddChuPai(int id)
        {
            GameObject animationPrefab = this.LoadAnimation("NpcUseCard");
            Image animation = UnityEngine.Object.Instantiate<GameObject>(animationPrefab, LunDaoManagerOnline.inst.AnimatorPanel).GetComponent<Image>();
            //if (LunDaoManagerOnline.inst.cardSprites == null)
            //{
            //    Console.WriteLine("LunDaoManagerOnline.inst.cardSprites空");
            //}
            animation.sprite = ChatingRoomManager.lunTiNameSpriteList[id];
            this.useCardQueue.Enqueue(animation);
            this.PlayChuPai();
        }


        public void AddCompleteLunTi(Image bg, Image img)
        {
            if (img != null && bg != null)
            {
                this.completeLunTiQueue.Enqueue(new List<Image> { bg, img });
                this.PlayCompleteLunTi();
                return;
            }
            img.gameObject.SetActive(true);
        }


        private void PlayCompleteLunTi()
        {
            if (this.completeLunTiLock && this.completeLunTiQueue.Count > 0)
            {
                this.completeLunTiLock = false;
                List<Image> list = this.completeLunTiQueue.Dequeue();
                Image bg = list[0];
                Image img = list[1];
                bg.DOColor(new Color(1f, 1f, 1f, 1f), 0.5f).Play<TweenerCore<Color, Color, ColorOptions>>();
                img.DOColor(new Color(1f, 1f, 1f, 1f), 0.5f).Play<TweenerCore<Color, Color, ColorOptions>>();
                img.transform.DOScale(Vector3.one, 0.5f).OnComplete(delegate
                {
                    this.completeLunTiLock = true;
                    this.PlayCompleteLunTi();
                }).Play<TweenerCore<Vector3, Vector3, VectorOptions>>();
            }
        }


        private void PlayChuPai()
        {
            if (this.useCardLock && this.useCardQueue.Count > 0)
            {
                this.useCardLock = false;
                Image image = this.useCardQueue.Dequeue();
                image.gameObject.SetActive(true);
                image.DOColor(new Color(1f, 1f, 1f, 1f), 0.4f).OnComplete(delegate
                {
                    image.DOColor(new Color(1f, 1f, 1f, 0f), 0.1f).Play<TweenerCore<Color, Color, ColorOptions>>();
                }).Play<TweenerCore<Color, Color, ColorOptions>>();
                image.transform.DOLocalMoveY(37f, 0.5f, false).OnComplete(delegate
                {
                    this.useCardLock = true;
                    UnityEngine.Object.Destroy(image.gameObject);
                    this.PlayChuPai();
                }).SetEase(Ease.InCirc).Play<TweenerCore<Vector3, Vector3, VectorOptions>>();
            }
        }


        private void PlayHeCheng()
        {
            if (this.heChengLock && this.heChengQueue.Count > 0)
            {
                this.heChengLock = false;
                MusicMag.instance.PlayEffectMusic(17, 1f);
                Transform transform = this.heChengQueue.Dequeue();
                GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.LoadAnimation("HeCheng"), LunDaoManagerOnline.inst.AnimatorPanel);
                gameObject.transform.position = new Vector3(transform.position.x + 10f, transform.position.y, transform.position.z);
                gameObject.SetActive(false);
                LunDaoQiuOnline component = transform.GetComponent<LunDaoQiuOnline>();
                GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(component.lunDaoQiuImage.gameObject, LunDaoManagerOnline.inst.AnimatorPanel);
                gameObject2.SetActive(false);
                Image component2 = gameObject2.GetComponent<Image>();
                component2.color = new Color(1f, 1f, 1f, 0f);
                Text componentInChildren = gameObject2.GetComponentInChildren<Text>();
                componentInChildren.text = (int.Parse(componentInChildren.text) - 1).ToString();
                gameObject2.transform.position = new Vector3(transform.position.x + 321f, transform.position.y, 0f);
                componentInChildren.color = new Color(1f, 1f, 1f, 0f);
                Animator component3 = gameObject.GetComponent<Animator>();
                gameObject.GetComponent<AnimatorUtils>().completeCallBack = delegate
                {
                    this.heChengLock = true;
                    this.PlayHeCheng();
                };
                gameObject.SetActive(true);
                gameObject2.SetActive(true);
                component3.Play("Play");
                component2.DOColor(new Color(1f, 1f, 1f, 1f), 0.2f).Play<TweenerCore<Color, Color, ColorOptions>>();
                componentInChildren.DOColor(new Color(1f, 1f, 1f, 1f), 0.2f).Play<TweenerCore<Color, Color, ColorOptions>>();
                DOTween.To(() => gameObject2.transform.position, delegate (Vector3 x)
                {
                    gameObject2.transform.position = x;
                }, new Vector3(transform.position.x, transform.position.y, 0f), 0.2f).OnComplete(delegate
                {
                    UnityEngine.Object.Destroy(gameObject2);
                }).Play<TweenerCore<Vector3, Vector3, VectorOptions>>();
            }
        }


        private GameObject LoadAnimation(string name)
        {
            return Resources.Load<GameObject>("NewUI/LunDao/DongHua/Prefab/" + name);
        }




        public class LunDaoAnimationController
        {

            private UnityAction playingAction;

            private UnityAction completeCallBack;

            private AnimatorUtils animatorUtils;

            private Animator animator;

            public LunDaoAnimationController(Animator animator, AnimatorUtils animatorUtils)
            {
                this.animator = animator;
                this.animatorUtils = animatorUtils;
            }

            public LunDaoAmrMagOnline.LunDaoAnimationController PlayingAction(UnityAction playingAction)
            {
                this.playingAction = playingAction;
                return this;
            }

            public LunDaoAmrMagOnline.LunDaoAnimationController CompleteAction(UnityAction completeCallBack)
            {
                this.completeCallBack = completeCallBack;
                return this;
            }

            public void Run()
            {
                this.animatorUtils.completeCallBack = this.completeCallBack;
                this.animator.Play("Start");
                this.playingAction();
            }


        }


        public enum AnimationName
        {
            StartLunDao = 1
        }


    }
}
