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
                // RuleSaveMapToMapEditor
                //------------------------------------------------------------------
                public class RuleSaveMapToMapEditor : RuleBase, INpRule
                {
                    public bool CheckRule()
                    {
                        IEntitySaveMap iSaveMap = Utility.GetIEntitySaveMap();
                        return (iSaveMap.GetResult() == EntitySaveMap.RESULT.SUCCESS);
                    }

                    public void ReadyNextSituation()
                    {
                        Debug.Log("RuleSaveMapToMapEditor.ReadyNextSituation()");

                        IEntitySaveMap iSaveMap = Utility.GetIEntitySaveMap();
                        iSaveMap.Terminate();

                        IEntityMapEditorConsole iMEConsole = Utility.GetIEntityMapEditorConsole();
                        iMEConsole.Cancel(true);

                        // Dungeon 処理を復帰
                        IEntityDungeon iDungeon = Utility.GetIEntityDungeon();
                        iDungeon.Pause(false);

                        // 次の Situation を登録
                        this.nextSituation = NpSituation.Create<SituationMapEditor>();
                    }

                    public void CleanUpForce() { }

                } //class RuleSaveMapToMapEditor


                //------------------------------------------------------------------
                // RuleSaveMapToMEConsole
                //------------------------------------------------------------------
                public class RuleSaveMapToMEConsole : RuleBase, INpRule
                {
                    public bool CheckRule()
                    {
                        IEntitySaveMap iSaveMap = Utility.GetIEntitySaveMap();
                        return (iSaveMap.GetResult() == EntitySaveMap.RESULT.CANCEL);
                    }

                    public void ReadyNextSituation()
                    {
                        Debug.Log("RuleSaveMapToMEConsole.ReadyNextSituation()");

                        IEntitySaveMap iSaveMap = Utility.GetIEntitySaveMap();
                        iSaveMap.Terminate();

                        IEntityMapEditorConsole iMEConsole = Utility.GetIEntityMapEditorConsole();
                        iMEConsole.Cancel(false);

                        // 次の Situation を登録
                        this.nextSituation = NpSituation.Create<SituationMEConsole>();
                    }

                    public void CleanUpForce() { }

                } //class RuleSaveMapToMEConsole

            } //namespace mapeditor
        } //namespace dev
    } //namespace situation
} //namespace nangka
