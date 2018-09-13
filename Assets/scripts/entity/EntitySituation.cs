using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using np;
using nangka.situation.boot;

namespace nangka {
    namespace entity
    {

        public class EntitySituation : NpEntity
        {
            private NpSituation situation = null;

            private delegate bool TProcFunc();
            private enum TPFUNCID
            {
                TPFUNCID_INVALID,
                TPFUNCID_TERMINATE_START,
                TPFUNCID_TERMINATE_RUN,
                TPFUNCID_TERMINATE_END
            }
            private Dictionary<TPFUNCID, TProcFunc> dicTProcFuncTable = null;
            private TPFUNCID curTPFuncId;


            protected override bool StartProc()
            {
                Debug.Log("EntitySituation.StartProc()");

                // ゲームロジックのスタート地点を設定
                this.situation = NpSituation.Create<SituationBoot>();

                this.ReadyTerminateProc();
                return true;
            }

            protected override bool UpdateProc()
            {
                NpSituation next = this.situation.Update();
                if (next != this.situation)
                {
                    this.situation.CleanUpForce();
                    this.situation = next;
                }
                return (next == null);
            }

            protected override bool TerminateProc()
            {
                bool bContinue = true;
                while (this.curTPFuncId != TPFUNCID.TPFUNCID_INVALID
                    && bContinue)
                {
                    bContinue = this.dicTProcFuncTable[this.curTPFuncId]();
                }
                return (this.curTPFuncId == TPFUNCID.TPFUNCID_INVALID);
            }

            protected override void CleanUp()
            {
                Debug.Log("EntitySituation.CleanUp()");
                if (dicTProcFuncTable != null)
                {
                    dicTProcFuncTable.Clear();
                    dicTProcFuncTable = null;
                }
                this.situation = null;
            }


            private void ReadyTerminateProc()
            {
                if (this.dicTProcFuncTable == null)
                {
                    this.dicTProcFuncTable = new Dictionary<TPFUNCID, TProcFunc>();
                    this.dicTProcFuncTable.Add(TPFUNCID.TPFUNCID_TERMINATE_START, TProc_TerminateStart);
                    this.dicTProcFuncTable.Add(TPFUNCID.TPFUNCID_TERMINATE_RUN, TProc_TerminateRun);
                    this.dicTProcFuncTable.Add(TPFUNCID.TPFUNCID_TERMINATE_END, TProc_TerminateEnd);
                    this.curTPFuncId = TPFUNCID.TPFUNCID_TERMINATE_START;
                }
            }
            private bool TProc_TerminateStart()
            {
                Debug.Log("EntitySituation.TProc_TerminateStart()");
                this.situation.Terminate();
                this.curTPFuncId = TPFUNCID.TPFUNCID_TERMINATE_RUN;
                return true;
            }
            private bool TProc_TerminateRun()
            {
                Debug.Log("EntitySituation.TProc_TerminateRun()");
                this.situation.Update();

                if (this.situation.IsInvalidate())
                {
                    this.curTPFuncId = TPFUNCID.TPFUNCID_TERMINATE_END;
                    return true;
                }
                return false;
            }
            private bool TProc_TerminateEnd()
            {
                Debug.Log("EntitySituation.TProc_TerminateEnd()");
                this.curTPFuncId = TPFUNCID.TPFUNCID_INVALID;
                return false;
            }
        }

    } //namespace entity
} //namespace nangka
