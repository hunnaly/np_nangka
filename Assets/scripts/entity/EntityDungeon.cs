using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using np;
using nangka.utility;

namespace nangka
{
    namespace entity
    {

        //------------------------------------------------------------------
        // IEntityDungeon
        //------------------------------------------------------------------
        public interface IEntityDungeon
        {
            void Pause(bool pause);
            void Terminate();

        } //interface IEntityDungeon


        //------------------------------------------------------------------
        // DungeonBuilder
        //------------------------------------------------------------------
        public class DungeonBuilder
        {
            private bool bValid = false;


            // 初期化
            public void Init()
            {
                this.bValid = true;
            }

            // 初期化情報も含めてクリア
            public void Clear()
            {
                this.Reset();
                this.bValid = false;
            }

            // 初期化情報を残した状態でリセット
            public void Reset()
            {

            }

            // 初期化情報に基づいてダンジョン生成
            public void Create()
            {

            }

        } //class DungeonBuilder


        //------------------------------------------------------------------
        // EntityDungeon
        //------------------------------------------------------------------
        public class EntityDungeon : NpEntity, IEntityDungeon
        {
            private bool bValid = false;

            private DungeonBuilder builder = null;


            //------------------------------------------------------------------
            // Entity メイン処理
            //------------------------------------------------------------------

            protected override bool StartProc()
            {
                Debug.Log("EntityDungeon.StartProc()");
                this.bValid = false;
                Utility.StartCoroutine(this.Ready());
                return true;
            }

            protected override bool UpdateProc()
            {
                if (this.bValid == false) return false;
                return false;
            }

            protected override bool TerminateProc()
            {
                return true;
            }

            protected override void CleanUp()
            {
                Debug.Log("EntityDungeon.CleanUp()");
            }


            //------------------------------------------------------------------
            // 準備処理
            //------------------------------------------------------------------

            private IEnumerator Ready()
            {
                yield return this.Build();

                yield return Utility.FadeIn();

                this.bValid = true;
            }

            // ダンジョン構築
            private IEnumerator Build()
            {
                do {
                    if (this.ReadyBuilder() == null) break;
                    this.builder.Create();

                } while (false);

                yield return null;
            }

            // ダンジョン構築オブジェクトの準備
            private DungeonBuilder ReadyBuilder()
            {
                do {
                    this.ReleaseBuilder();
                    if (this.CreateBuilder() == null) break;
                    this.InitBuilder();

                } while (false);

                return this.builder;
            }


            //------------------------------------------------------------------
            // ダンジョン構築オブジェクト制御
            //------------------------------------------------------------------

            // ダンジョン構築オブジェクトの生成
            private DungeonBuilder CreateBuilder()
            {
                this.builder = new DungeonBuilder();
                return this.builder;
            }

            // ダンジョン構築オブジェクトの解放
            private void ReleaseBuilder()
            {
                if (this.builder != null)
                {
                    this.builder.Clear();
                    this.builder = null;
                }
            }

            // ダンジョン構築オブジェクトの初期化
            private void InitBuilder()
            {
                // マップ情報の取得

                // マップ情報を渡してダンジョン構築オブジェクトを初期化
                this.builder.Init();
            }


        } //class EntityDungeon

    } //namespace entity
} //namespace nangka