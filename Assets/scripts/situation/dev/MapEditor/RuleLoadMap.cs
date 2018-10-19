using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using np;
using nangka.entity;
using nangka.situation;
using nangka.situation.dungeon;
using nangka.utility;

namespace nangka
{
    namespace situation
    {
        namespace dev
        {
            namespace mapeditor
            {

                //------------------------------------------------------------------
                // RuleLoadMapToMapEditor
                //------------------------------------------------------------------
                public class RuleLoadMapToMapEditor : RuleBase, INpRule
                {
                    public bool CheckRule()
                    {
                        IEntityLoadMap iLoadMap = Utility.GetIEntityLoadMap();
                        return (iLoadMap.GetResult() == EntityLoadMap.RESULT.SUCCESS);
                    }

                    public void ReadyNextSituation()
                    {
                        Debug.Log("RuleLoadMapToMapEditor.ReadyNextSituation()");

                        IEntityLoadMap iLoadMap = Utility.GetIEntityLoadMap();
                        iLoadMap.Terminate();

                        IEntityMapEditorConsole iMEConsole = Utility.GetIEntityMapEditorConsole();
                        iMEConsole.Cancel(true);

                        // Dungeon 処理を復帰
                        IEntityDungeon iDungeon = Utility.GetIEntityDungeon();
                        iDungeon.Pause(false);

                        // 次の Situation を登録
                        this.nextSituation = NpSituation.Create<SituationMapEditor>();
                    }

                    public void CleanUpForce() { }

                } //class RuleLoadMapToMapEditor


                //------------------------------------------------------------------
                // RuleLoadMapToMEConsole
                //------------------------------------------------------------------
                public class RuleLoadMapToMEConsole : RuleBase, INpRule
                {
                    public bool CheckRule()
                    {
                        IEntityLoadMap iLoadMap = Utility.GetIEntityLoadMap();
                        return (iLoadMap.GetResult() == EntityLoadMap.RESULT.CANCEL);
                    }

                    public void ReadyNextSituation()
                    {
                        Debug.Log("RuleLoadMapToMEConsole.ReadyNextSituation()");

                        IEntityLoadMap iLoadMap = Utility.GetIEntityLoadMap();
                        iLoadMap.Terminate();

                        IEntityMapEditorConsole iMEConsole = Utility.GetIEntityMapEditorConsole();
                        iMEConsole.Cancel(false);

                        // 次の Situation を登録
                        this.nextSituation = NpSituation.Create<SituationMEConsole>();
                    }

                    public void CleanUpForce() { }

                } //class RuleLoadMapToMEConsole

            } //namespace mapeditor
        } //namespace dev
    } //namespace situation
} //namespace nangka
