using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using np;
using nangka.entity;

namespace nangka {
    namespace utility
    {

        public static class Utility
        {
            //----------------------------------------------------------------
            // コルーチン
            //----------------------------------------------------------------

            // MonoBehaviour を継承していないクラスに提供するための StartCoroutine をラップしたもの
            public static Coroutine StartCoroutine(IEnumerator coroutine)
            {
                return NpAppBase.StartStaticCoroutine(coroutine);
            }


            //----------------------------------------------------------------
            // フェード処理簡易版
            // 注意：EntityFade が動作している必要がある
            //----------------------------------------------------------------

            public static IEnumerator FadeIn()
            {
                IEntityFade iEntityFade = GetIEntityFade();
                iEntityFade.FadeIn();

                // フェードイン完了待ち
                while (iEntityFade.IsDoing()) yield return null;
                yield return null;
            }

            public static IEnumerator FadeOut(float target)
            {
                IEntityFade iEntityFade = Utility.GetIEntityFade();
                iEntityFade.FadeOut(target);

                // フェードアウト完了待ち
                while (iEntityFade.IsDoing()) yield return null;
                yield return null;
            }


            //----------------------------------------------------------------
            // 登録中の各種 Entity を操作するためのインタフェースを取得
            //----------------------------------------------------------------

            // インタフェースを取得する（非公開）
            private static U GetInterface<T, U>(string className)
                where T : class, U
                where U : class
            {
                NpEntity entity = Global.Instance.EntityCtrl.Exist(className);
                U iEntity = (entity == null) ? null : entity.GetInterface<T, U>();
                return iEntity;
            }

            // 各Entityインタフェースを取得する
            public static IEntityFade GetIEntityFade() { return GetInterface<EntityFade, IEntityFade>(Define.ENTITY_CNAME_FADE); }
            public static IEntityDungeon GetIEntityDungeon() { return GetInterface<EntityDungeon, IEntityDungeon>(Define.ENTITY_CNAME_DUNGEON); }

        }

    } //namespace utility
} //namespace nangka
