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
                // RuleInputMapFileToMapEditor
                //------------------------------------------------------------------
                public class RuleInputMapFileToMapEditor : RuleBase, INpRule
                {
                    public bool CheckRule()
                    {
                        IEntityMapFileSetting iSetting = Utility.GetIEntityMapFileSetting();
                        return (iSetting.GetResult() == EntityMapFileSetting.RESULT.OK);
                    }

                    public void ReadyNextSituation()
                    {
                        Debug.Log("RuleInputMapFileToMapEditor.ReadyNextSituation()");

                        IEntityCommonInputDialog iInputDialog = Utility.GetIEntityCommonInputDialog();
                        iInputDialog.Terminate();

                        IEntityMapFileSetting iSetting = Utility.GetIEntityMapFileSetting();
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
                public class RuleInputMapFileToMEConsole : RuleBase, INpRule
                {
                    public bool CheckRule()
                    {
                        IEntityMapFileSetting iSetting = Utility.GetIEntityMapFileSetting();
                        return (iSetting.GetResult() == EntityMapFileSetting.RESULT.CANCEL);
                    }

                    public void ReadyNextSituation()
                    {
                        Debug.Log("RuleInputMapFileToMEConsole.ReadyNextSituation()");

                        IEntityCommonInputDialog iInputDialog = Utility.GetIEntityCommonInputDialog();
                        iInputDialog.Terminate();

                        IEntityMapFileSetting iSetting = Utility.GetIEntityMapFileSetting();
                        iSetting.Terminate();

                        IEntityMapEditorConsole iMEConsole = Utility.GetIEntityMapEditorConsole();
                        iMEConsole.Cancel(false);

                        // 次の Situation を登録
                        this.nextSituation = NpSituation.Create<SituationMEConsole>();
                    }

                    public void CleanUpForce() { }

                } //class RuleInputMapFileToMEConsole

            } //namespace mapeditor
        } //namespace dev
    } //namespace situation
} //namespace nangka
