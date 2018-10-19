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
                // RuleNewMapToMapEditor
                //------------------------------------------------------------------
                public class RuleNewMapToMapEditor : RuleBase, INpRule
                {
                    public bool CheckRule()
                    {
                        IEntityNewMap iNewMap = Utility.GetIEntityNewMap();
                        return (iNewMap.GetResult() == EntityNewMap.RESULT.SUCCESS);
                    }

                    public void ReadyNextSituation()
                    {
                        Debug.Log("RuleNewMapToMapEditor.ReadyNextSituation()");

                        IEntityNewMap iNewMap = Utility.GetIEntityNewMap();
                        iNewMap.Terminate();

                        IEntityMapEditorConsole iMEConsole = Utility.GetIEntityMapEditorConsole();
                        iMEConsole.Cancel(true);

                        // Dungeon 処理を復帰
                        IEntityDungeon iDungeon = Utility.GetIEntityDungeon();
                        iDungeon.Pause(false);

                        // 次の Situation を登録
                        this.nextSituation = NpSituation.Create<SituationMapEditor>();
                    }

                    public void CleanUpForce() { }

                } //class RuleNewMapToMapEditor


                //------------------------------------------------------------------
                // RuleNewMapToMEConsole
                //------------------------------------------------------------------
                public class RuleNewMapToMEConsole : RuleBase, INpRule
                {
                    public bool CheckRule()
                    {
                        IEntityNewMap iNewMap = Utility.GetIEntityNewMap();
                        return (iNewMap.GetResult() == EntityNewMap.RESULT.CANCEL);
                    }

                    public void ReadyNextSituation()
                    {
                        Debug.Log("RuleNewMapToMEConsole.ReadyNextSituation()");

                        IEntityNewMap iNewMap = Utility.GetIEntityNewMap();
                        iNewMap.Terminate();

                        IEntityMapEditorConsole iMEConsole = Utility.GetIEntityMapEditorConsole();
                        iMEConsole.Cancel(false);

                        // 次の Situation を登録
                        this.nextSituation = NpSituation.Create<SituationMEConsole>();
                    }

                    public void CleanUpForce() { }

                } //class RuleNewMapToMEConsole

            } //namespace mapeditor
        } //namespace dev
    } //namespace situation
} //namespace nangka
