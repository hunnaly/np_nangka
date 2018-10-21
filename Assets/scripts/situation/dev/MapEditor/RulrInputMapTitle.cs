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
                // RuleInputMapTitleToMapEditor
                //------------------------------------------------------------------
                public class RuleInputMapTitleToMapEditor : RuleBase, INpRule
                {
                    public bool CheckRule()
                    {
                        IEntityMapTitleSetting iSetting = Utility.GetIEntityMapTitleSetting();
                        return (iSetting.GetResult() == EntityMapTitleSetting.RESULT.OK);
                    }

                    public void ReadyNextSituation()
                    {
                        Debug.Log("RuleInputMapTitleToMapEditor.ReadyNextSituation()");

                        IEntityCommonInputDialog iInputDialog = Utility.GetIEntityCommonInputDialog();
                        iInputDialog.Terminate();

                        IEntityMapTitleSetting iSetting = Utility.GetIEntityMapTitleSetting();
                        iSetting.Terminate();

                        IEntityMapEditorConsole iMEConsole = Utility.GetIEntityMapEditorConsole();
                        iMEConsole.Cancel(true);

                        // Dungeon 処理を復帰
                        IEntityDungeon iDungeon = Utility.GetIEntityDungeon();
                        iDungeon.Pause(false);

                        // 次の Situation を登録
                        this.nextSituation = NpSituation.Create<SituationMapEditor>();
                    }

                    public void CleanUpForce() { }

                } //class RuleInputMapFileToMapEditor


                //------------------------------------------------------------------
                // RuleInputMapFileToMEConsole
                //------------------------------------------------------------------
                public class RuleInputMapTitleToMEConsole : RuleBase, INpRule
                {
                    public bool CheckRule()
                    {
                        IEntityMapTitleSetting iSetting = Utility.GetIEntityMapTitleSetting();
                        return (iSetting.GetResult() == EntityMapTitleSetting.RESULT.CANCEL);
                    }

                    public void ReadyNextSituation()
                    {
                        Debug.Log("RuleInputMapTitleToMEConsole.ReadyNextSituation()");

                        IEntityCommonInputDialog iInputDialog = Utility.GetIEntityCommonInputDialog();
                        iInputDialog.Terminate();

                        IEntityMapTitleSetting iSetting = Utility.GetIEntityMapTitleSetting();
                        iSetting.Terminate();

                        IEntityMapEditorConsole iMEConsole = Utility.GetIEntityMapEditorConsole();
                        iMEConsole.Cancel(false);

                        // 次の Situation を登録
                        this.nextSituation = NpSituation.Create<SituationMEConsole>();
                    }

                    public void CleanUpForce() { }

                } //class RuleInputMapTitleToMEConsole

            } //namespace mapeditor
        } //namespace dev
    } //namespace situation
} //namespace nangka
