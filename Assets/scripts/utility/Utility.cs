using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using np;
using nangka.entity;

namespace nangka {
    namespace utility
    {
        //------------------------------------------------------------------
        // 方向
        //------------------------------------------------------------------
        public enum Direction : int
        {
            NORTH = 0,
            EAST = 1,
            SOUTH = 2,
            WEST = 3,
            PLANE_MAX = 4,
            UP = 4,
            DOWN = 5,
            SOLID_MAX = 6

        } //enum Direction


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
            // 指定方向の反対を取得
            //----------------------------------------------------------------
            public static Direction GetOppositeDirection(Direction dir)
            {
                Direction retDir = dir;
                switch (dir)
                {
                    case Direction.NORTH: retDir = Direction.SOUTH; break;
                    case Direction.SOUTH: retDir = Direction.NORTH; break;
                    case Direction.EAST: retDir = Direction.WEST; break;
                    case Direction.WEST: retDir = Direction.EAST; break;
                    case Direction.UP: retDir = Direction.DOWN; break;
                    case Direction.DOWN: retDir = Direction.UP; break;
                    default: break;
                }
                return retDir;
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
            public static IEntityTextureResources GetIEntityTextureResources() { return GetInterface<EntityTextureResources, IEntityTextureResources>(Define.ENTITY_CNAME_TEXTURE_RESOURCES); }
            public static IEntityFrame GetIEntityFrame() { return GetInterface<EntityFrame, IEntityFrame>(Define.ENTITY_CNAME_FRAME); }

        }

    } //namespace utility
} //namespace nangka
