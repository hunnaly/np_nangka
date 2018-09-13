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
            // シーン名
            //----------------------------------------------------------------

            public static string SCENE_NAME_FADE = "ui_fade";


            //----------------------------------------------------------------
            // 登録中の各種 Entity を操作するためのインタフェースを取得
            //----------------------------------------------------------------

            // Fadeインタフェースを取得する
            public static string ENTITY_CNAME_FADE = "nangka.entity.EntityFade";
            public static IEntityFade GetIEntityFade() { return GetInterface<EntityFade, IEntityFade>(ENTITY_CNAME_FADE); }

            // インタフェースを取得する（非公開）
            private static U GetInterface<T, U>(string className)
                where T : class, U
                where U : class
            {
                NpEntity entity = Global.Instance.EntityCtrl.Exist(className);
                U iEntity = (entity == null) ? null : entity.GetInterface<T, U>();
                return iEntity;
            }
        }

    } //namespace utility
} //namespace nangka
