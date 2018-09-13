using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace np {

    public class NpEntityConductor : INpEntityController
    {
        private bool bValidate = false;
        private bool bTerminate = false;

        private Dictionary<string, NpEntity> dicRegistTable
            = new Dictionary<string, NpEntity>();

        private Dictionary<string, NpEntity> dicEntityTable
            = new Dictionary<string, NpEntity>();

        private List<string> listUnregistTable
            = new List<string>();


        private bool Regist(string key, NpEntity entity)
        {
            this.dicRegistTable.Add(key, entity);
            return true;
        }

        private void Unregist(string key)
        {
            this.dicEntityTable.Remove(key);
        }

        public void Start()
        {
            if (this.IsInvalidate())
            {
                this.bTerminate = false;
                this.bValidate = true;
            }
        }

        public void Update()
        {
            if (this.IsInvalidate()) return;

            // 登録予約された Entity を本登録
            foreach (KeyValuePair<string, NpEntity> item in this.dicRegistTable)
            {
                this.dicEntityTable.Add(item.Key, item.Value);
            }
            this.dicRegistTable.Clear();

            // 各 Entity の Update 処理
            foreach (KeyValuePair<string, NpEntity> item in this.dicEntityTable)
            {
                item.Value.Update();
                if (item.Value.IsInvalidate() == true)
                {
                    this.listUnregistTable.Add(item.Key);
                    item.Value.CleanUpForce();
                }
            }

            // 無効になった Entity があればテーブルから削除する
            if (this.listUnregistTable.Count > 0)
            {
                foreach (string key in this.listUnregistTable)
                {
                    this.Unregist(key);
                }
                this.listUnregistTable.Clear();
            }

            if (this.IsTerminating())
            {
                if (this.dicEntityTable.Count == 0)
                {
                    this.bTerminate = false;
                    this.bValidate = false;
                }
            }
        }

        public void LastUpdate()
        {
            if (this.IsInvalidate() || this.IsTerminating()) return;
            foreach (NpEntity entity in this.dicEntityTable.Values)
            {
                entity.LastUpdate();
            }
        }

        public void OnGUI()
        {
            if (this.IsInvalidate() || this.IsTerminating()) return;
            foreach (NpEntity entity in this.dicEntityTable.Values)
            {
                entity.OnGUI();
            }
        }


        // INpTaskController の実装

        public void CreateAndRegist<T>()
            where T : NpEntity, new()
        {
            T entity = new T();
            if (entity != null)
            {
                if (this.Regist(entity.GetType().FullName, entity) == true)
                {
                    entity.Start();
                }
            }
        }

        public void Terminate()
        {
            if (this.IsInvalidate() || this.IsTerminating()) return;

            foreach (NpEntity entity in this.dicEntityTable.Values)
            {
                entity.Terminate();
            }
            foreach (NpEntity entity in this.dicRegistTable.Values)
            {
                entity.Terminate();
            }
            this.bTerminate = true;
        }

        public bool IsInvalidate() { return (this.bValidate == false); }
        public bool IsTerminating() { return this.bTerminate; }

        public NpEntity Exist(string key)
        {
            NpEntity retEntity = null;
            if (this.dicEntityTable.ContainsKey(key))
            {
                retEntity = this.dicEntityTable[key];
            }
            return retEntity;
        }
        public NpEntity Exist(NpEntity entity)
        {
            NpEntity retEntity = null;
            if (this.dicEntityTable.ContainsValue(entity))
            {
                retEntity = entity;
            }
            return retEntity;
        }
    }

}// namespace np
