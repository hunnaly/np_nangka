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
            // 方向関連
            //----------------------------------------------------------------

            // 指定方向の反対を取得
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

            // 指定方向を向いて右手側の方向を取得
            public static Direction DirectionRight(Direction dir)
            {
                Direction ret = dir;
                switch (dir)
                {
                    case Direction.NORTH: ret = Direction.EAST; break;
                    case Direction.SOUTH: ret = Direction.WEST; break;
                    case Direction.WEST: ret = Direction.NORTH; break;
                    case Direction.EAST: ret = Direction.SOUTH; break;
                    default: break;
                }
                return ret;
            }

            // 指定方向を向いて左手側の方向を取得
            public static Direction DirectionLeft(Direction dir)
            {
                Direction ret = dir;
                switch (dir)
                {
                    case Direction.NORTH: ret = Direction.WEST; break;
                    case Direction.SOUTH: ret = Direction.EAST; break;
                    case Direction.WEST: ret = Direction.SOUTH; break;
                    case Direction.EAST: ret = Direction.NORTH; break;
                    default: break;
                }
                return ret;
            }

            // Y軸を軸とした指定方向の角度を取得
            public static float DirectionToAngleY(Direction dir)
            {
                float angle = 0.0f;
                switch (dir)
                {
                    case Direction.SOUTH: angle = 180.0f; break;
                    case Direction.WEST: angle = -90.0f; break;
                    case Direction.EAST: angle = 90.0f; break;
                    default: break;
                }
                return angle;
            }


            //----------------------------------------------------------------
            // 各種 Entity を利用するための便利関数群
            //----------------------------------------------------------------

            // インタフェースを取得する（非公開）
            private static U GetInterface<T, U>(string className)
                where T : class, U
                where U : class, IEntity
            {
                NpEntity entity = Global.Instance.EntityCtrl.Exist(className);
                U iEntity = (entity == null) ? null : entity.GetInterface<T, U>();
                return iEntity;
            }

            // Entity を登録してインタフェースが利用可能な状態になるまで待つ（非公開）
            private static IEnumerator RegistEntity<T, U>(string className)
                where T : NpEntity, U, new()
                where U : class, IEntity
            {
                Global.Instance.EntityCtrl.CreateAndRegist<T>();

                U iEntity = null;
                while ((iEntity = Utility.GetInterface<T, U>(className)) == null) yield return null;
                while (iEntity.IsReadyLogic() == false) yield return null;
            }

            // 各 Entity 利用のための便利関数
            // MEMO: Entity を用意するたびに追記していくこと！
            public static IEnumerator RegistEntityFade() { yield return RegistEntity<EntityFade, IEntityFade>(Define.ENTITY_CNAME_FADE); }
            public static IEntityFade GetIEntityFade() { return GetInterface<EntityFade, IEntityFade>(Define.ENTITY_CNAME_FADE); }

            public static IEnumerator RegistEntityDungeon() { yield return RegistEntity<EntityDungeon, IEntityDungeon>(Define.ENTITY_CNAME_DUNGEON); }
            public static IEntityDungeon GetIEntityDungeon() { return GetInterface<EntityDungeon, IEntityDungeon>(Define.ENTITY_CNAME_DUNGEON); }

            public static IEnumerator RegistEntityTextureResources() { yield return RegistEntity<EntityTextureResources, IEntityTextureResources>(Define.ENTITY_CNAME_TEXTURE_RESOURCES); }
            public static IEntityTextureResources GetIEntityTextureResources() { return GetInterface<EntityTextureResources, IEntityTextureResources>(Define.ENTITY_CNAME_TEXTURE_RESOURCES); }

            public static IEnumerator RegistEntityFrame() { yield return RegistEntity<EntityFrame, IEntityFrame>(Define.ENTITY_CNAME_FRAME); }
            public static IEntityFrame GetIEntityFrame() { return GetInterface<EntityFrame, IEntityFrame>(Define.ENTITY_CNAME_FRAME); }

            public static IEnumerator RegistEntityPlayer() { yield return RegistEntity<EntityPlayer, IEntityPlayer>(Define.ENTITY_CNAME_PLAYER); }
            public static IEntityPlayer GetIEntityPlayer() { return GetInterface<EntityPlayer, IEntityPlayer>(Define.ENTITY_CNAME_PLAYER); }

            public static IEnumerator RegistEntityPlayerData() { yield return RegistEntity<EntityPlayerData, IEntityPlayerData>(Define.ENTITY_CNAME_PLAYER_DATA); }
            public static IEntityPlayerData GetIEntityPlayerData() { return GetInterface<EntityPlayerData, IEntityPlayerData>(Define.ENTITY_CNAME_PLAYER_DATA); }

            public static IEnumerator RegistEntityMapData() { yield return RegistEntity<EntityMapData, IEntityMapData>(Define.ENTITY_CNAME_MAP_DATA); }
            public static IEntityMapData GetIEntityMapData() { return GetInterface<EntityMapData, IEntityMapData>(Define.ENTITY_CNAME_MAP_DATA); }

            public static IEnumerator RegistEntityStructure() { yield return RegistEntity<EntityStructure, IEntityStructure>(Define.ENTITY_CNAME_STRUCTURE); }
            public static IEntityStructure GetIEntityStructure() { return GetInterface<EntityStructure, IEntityStructure>(Define.ENTITY_CNAME_STRUCTURE); }

            public static IEnumerator RegistEntityMiniMap() { yield return RegistEntity<EntityMiniMap, IEntityMiniMap>(Define.ENTITY_CNAME_MINIMAP); }
            public static IEntityMiniMap GetIEntityMiniMap() { return GetInterface<EntityMiniMap, IEntityMiniMap>(Define.ENTITY_CNAME_MINIMAP); }

            public static IEnumerator RegistEntityRecreator() { yield return RegistEntity<EntityRecreator, IEntityRecreator>(Define.ENTITY_CNAME_RECREATOR); }
            public static IEntityRecreator GetIEntityRecreator() { return GetInterface<EntityRecreator, IEntityRecreator>(Define.ENTITY_CNAME_RECREATOR); }

            public static IEnumerator RegistEntityDevEntrance() { yield return RegistEntity<EntityDevEntrance, IEntityDevEntrance>(Define.ENTITY_CNAME_DEV_ENTRANCE); }
            public static IEntityDevEntrance GetIEntityDevEntrance() { return GetInterface<EntityDevEntrance, IEntityDevEntrance>(Define.ENTITY_CNAME_DEV_ENTRANCE); }
        }

    } //namespace utility
} //namespace nangka
