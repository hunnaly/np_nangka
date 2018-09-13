using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using np;
using nangka.utility;

namespace nangka {
    namespace entity
    {

        public interface IEntityFade
        {
            bool IsValid();
            void Activate(bool enabled);
            void FadeOut(float time = 1.0f, float target = 1.0f);
            void FadeIn(float time = 1.0f, bool bAutoActivateOff = true);
            bool IsDoing();
            void Terminate();
        }

        public class EntityFade : NpEntity, IEntityFade
        {
            private bool bValid = false;
            private bool bUnloading = false;

            private bool bDoingFade = false;
            private bool bAutoActivateOff = false;
            private float fTime = 0.0f;
            private float fElapsed = 0.0f;
            private float fAlphaOrg, fAlphaTarget;
            private float fRed, fGreen, fBlue, fAlpha;
            private Image fadePanel;


            protected override bool StartProc()
            {
                Debug.Log("EntityFade.StartProc()");
                Utility.StartCoroutine(this.LoadSceneUIFade());
                return true;
            }

            protected override bool UpdateProc()
            {
                // ui_fade のロードが完了していなければ何もせず完了を待つ
                if (this.bValid == false) return false;

                // フェード処理が設定されているときに処理を行う
                if (this.IsDoing())
                {
                    this.Activate(true);

                    this.fElapsed += Time.deltaTime;
                    if (this.fElapsed >= this.fTime)
                    {
                        this.fElapsed = this.fTime;
                        this.bDoingFade = false;
                    }

                    if (this.bDoingFade == false)
                    {
                        this.fAlpha = this.fAlphaTarget;
                        if (this.bAutoActivateOff) this.Activate(false);
                    }
                    else
                    {
                        float delta = (this.fAlphaTarget - this.fAlphaOrg) * this.fElapsed / this.fTime;
                        this.fAlpha = this.fAlphaOrg + delta;
                    }
                    this.SetColor();
                }

                return false;
            }

            protected override bool TerminateProc()
            {
                if (this.bValid)
                {
                    this.Activate(false);
                    this.bValid = false;
                    this.bUnloading = true;
                    Utility.StartCoroutine(this.UnloadSceneUIFade());
                }

                return (this.bUnloading == false);
            }

            protected override void CleanUp()
            {
                this.fadePanel = null;
                // MEMO:
                // Terminate せずに強制的に CleanUp したときは
                // ui_fade シーンが残ってしまうことに注意！！
            }


            public bool IsValid() { return this.bValid; }

            public void Activate(bool enabled)
            {
                if (this.fadePanel.enabled == enabled) return;
                this.fadePanel.enabled = enabled;
            }

            public void FadeOut(float time = 1.0f, float target = 1.0f)
            {
                this.bAutoActivateOff = false;
                this.FadeInit(time, target);
            }

            public void FadeIn(float time = 1.0f, bool bAutoActivateOff = true)
            {
                this.bAutoActivateOff = bAutoActivateOff;
                this.FadeInit(time, 0.0f);
            }

            private void FadeInit(float time, float target)
            {
                this.fTime = (time <= 0.0f) ? 0.0f : time;
                this.fAlphaOrg = this.fAlpha;
                this.fAlphaTarget = target;
                this.fElapsed = 0.0f;
                this.bDoingFade = true;

                // ノータイムで反映させるときはこのタイミングでフェード処理を完了させるが
                // シーンのロードが完了していないときは Update に処理を委ねる
                if (this.bValid)
                {
                    this.Activate(true);

                    if (time <= 0.0f)
                    {
                        this.fAlpha = this.fAlphaTarget;
                        this.SetColor();
                        this.bDoingFade = false;

                        if (this.bAutoActivateOff) this.Activate(false);
                    }
                }
            }


            private IEnumerator LoadSceneUIFade()
            {
                yield return SceneManager.LoadSceneAsync(Utility.SCENE_NAME_FADE, LoadSceneMode.Additive);

                var scene = SceneManager.GetSceneByName(Utility.SCENE_NAME_FADE);
                var canvas = scene.GetRootGameObjects().First(obj => obj.GetComponent<Canvas>() != null).GetComponent<Canvas>();
                var component = canvas.GetComponent<ui>();
                var objPanel = component.objectTable[0];

                if (objPanel != null)
                {
                    this.fadePanel = objPanel.GetComponent<Image>();
                    this.fadePanel.enabled = false;
                    this.bValid = (this.fadePanel != null);

                    this.fRed = this.fadePanel.color.r;
                    this.fGreen = this.fadePanel.color.g;
                    this.fBlue = this.fadePanel.color.b;
                    this.fAlpha = this.fadePanel.color.a;
                }
            }

            private IEnumerator UnloadSceneUIFade()
            {
                yield return SceneManager.UnloadSceneAsync("ui_fade");

                this.fadePanel = null;
                this.bUnloading = false;
            }

            public bool IsDoing()
            {
                return this.bDoingFade;
            }

            private void SetColor()
            {
                this.fadePanel.color = new Color(this.fRed, this.fGreen, this.fBlue, this.fAlpha);
            }
        }

    } //namespace entity
} //namespace nangka
