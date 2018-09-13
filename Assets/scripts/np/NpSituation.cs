using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace np {

    public class NpSituation
    {
        private Rules rules = null;

        // Situation インスタンス生成メソッドを提供
        public static NpSituation Create<T>()
            where T : NpSituation, new()
        {
            T p = new T();
            if (p != null)
            {
                if (p.Initialize() == false)
                {
                    p = null;
                }
            }
            return p;
        }

        // 強制的に CleanUp() を行う
        public void CleanUpForce()
        {
            this.CleanUp();
        }

        // 更新処理
        public NpSituation Update()
        {
            if (this.rules == null) return this;
            if (this.rules.IsInvalidate()) return this;

            NpSituation next = this.rules.RuleProc(this);

            if (this.rules.IsInvalidate())
            {
                this.CleanUp();
            }

            return next;
        }

        // 終了処理
        public void Terminate()
        {
            if (this.rules != null) this.rules.Terminate();
        }

        // コンストラクタは Create() から呼び出される
        // ユーザーは呼び出せない
        protected NpSituation() { }

        // どのような Rule を作成するかは派生クラスに委ねる
        protected virtual bool CreateRules() { return true; }

        // Rule 生成処理を派生クラスに提供する
        protected INpRule CreateRule<T>()
            where T : INpRule, new()
        {
            T p = new T();
            if (p != null)
            {
                this.AddRule(p);
            }
            return p;
        }

        // Rule を追加する
        private void AddRule(INpRule rule)
        {
            if (this.rules != null) this.rules.AddRule(rule);
        }

        // インスタンス生成時に一度だけ呼び出される初期化処理
        private bool Initialize()
        {
            this.rules = new Rules();
            this.rules.Start();
            return (this.rules == null) ? false : this.CreateRules();
        }

        // リソースの破棄など後片付けを行う処理
        // 複数回呼び出されることに注意して実装する必要あり
        private void CleanUp()
        {
            if (this.rules != null)
            {
                this.rules.CleanUpForce();
                this.rules = null;
            }
        }

        // デストラクタ
        // リソース破棄漏れがないように念のために CleanUp() を呼び出す
        ~NpSituation()
        {
            this.CleanUp();
        }

        public bool IsInvalidate() { return ((this.rules == null) || this.rules.IsInvalidate()); }


        class Rules
        {
            private bool bValidate = false;
            private bool bTerminate = false;
            private INpRule ruleReady = null;
            private List<INpRule> listRuleTable = new List<INpRule>();

            public Rules() { }

            public void Start()
            {
                this.ruleReady = null;
                this.bTerminate = false;
                this.bValidate = true;
            }

            public void AddRule(INpRule rule)
            {
                if (rule == null) return;
                this.listRuleTable.Add(rule);
            }

            public void ClearRules()
            {
                for (int i=this.listRuleTable.Count-1; i>=0; i--)
                {
                    this.listRuleTable[i].CleanUpForce();
                    this.listRuleTable.RemoveAt(i);
                }
            }

            private INpRule CheckRules()
            {
                INpRule rule = null;
                foreach(INpRule r in this.listRuleTable)
                {
                    if (r.CheckRule())
                    {
                        rule = r;
                        break;
                    }
                }
                return rule;
            }

            public NpSituation RuleProc(NpSituation pCur)
            {
                NpSituation pNext = null;
                do
                {
                    if (this.IsInvalidate()) break;

                    if (this.bTerminate)
                    {
                        this.CleanUp();
                        this.bTerminate = false;
                        this.bValidate = false;
                        break;
                    }

                    pNext = pCur;
                    if (this.ruleReady == null)
                    {
                        INpRule p = this.CheckRules();
                        if (p != null)
                        {
                            p.ReadyNextSituation();
                            this.ruleReady = p;
                        }
                    }

                    if (this.ruleReady != null)
                    {
                        NpSituation temp = this.ruleReady.CompletedReadyNextSituation();
                        if (temp != null)
                        {
                            this.bTerminate = true;
                            this.ruleReady = null;
                            pNext = temp;
                        }
                    }
                }
                while (false);

                return pNext;
            }

            public void Terminate()
            {
                this.bTerminate = true;
            }

            // デストラクタ
            // リソース破棄漏れがないように念のために CleanUp() を呼び出す
            ~Rules()
            {
                this.CleanUp();
            }

            // 強制的に CleanUp() を行う
            public void CleanUpForce()
            {
                this.CleanUp();
            }

            private void CleanUp()
            {
                if (this.listRuleTable != null)
                {
                    Debug.Log("NpSituation.CleanUp()");
                    this.ClearRules();
                    this.listRuleTable = null;
                }
            }

            public bool IsInvalidate() { return (this.bValidate == false); }
            public bool IsTerminating() { return this.bTerminate; }
        }
    }

} //namespace np
