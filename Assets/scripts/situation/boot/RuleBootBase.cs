using System.Collections;
using nangka.entity;
using nangka.utility;


namespace nangka {
    namespace situation {
        namespace boot
        {

            public abstract class RuleBootBase : RuleBase
            {
                public virtual bool CheckRule()
                {
                    return true;
                }

                public virtual void ReadyNextSituation()
                {
                    Utility.StartCoroutine(this.Ready());
                }

                public virtual void CleanUpForce() { }


                protected virtual IEnumerator Ready()
                {
                    yield return null;
                }

                // フェードの準備処理を派生クラスへ提供する
                protected IEnumerator ReadyEntityFade(bool bFadeOut=true)
                {
                    // Fade 制御 Entity を登録し、有効な状態になるまで待つ
                    yield return Utility.RegistEntityFade();

                    // ノータイムでフェードアウト状態にしておく
                    IEntityFade iEntityFade = Utility.GetIEntityFade();
                    if (bFadeOut) iEntityFade.FadeOut(0.0f);
                }
            }

        } //namespace boot
    } //namespace situation
} //namespace nangka
