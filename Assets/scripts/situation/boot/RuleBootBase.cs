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
                    // Fade 制御 Entity を登録
                    Global.Instance.EntityCtrl.CreateAndRegist<EntityFade>();

                    // 有効な状態になるまで待つ
                    IEntityFade iEntityFade = null;
                    while ((iEntityFade = Utility.GetIEntityFade()) == null) yield return null;
                    while (iEntityFade.IsValid() == false) yield return null;

                    // ノータイムでフェードアウト状態にしておく
                    if (bFadeOut) iEntityFade.FadeOut(0.0f);
                }
            }

        } //namespace boot
    } //namespace situation
} //namespace nangka
