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
                // RuleMEConsoleToMapEditor
                //------------------------------------------------------------------
                public class RuleMEConsoleToMapEditor : RuleBase, INpRule
                {
                    public bool CheckRule()
                    {
                        IEntityMapEditorConsole iMeConsole = Utility.GetIEntityMapEditorConsole();
                        return (iMeConsole.GetDetectedMode() == CONSOLE_MODE.HIDDEN);
                    }

                    public void ReadyNextSituation()
                    {
                        Debug.Log("RuleMEConsoleToMapEditor.ReadyNextSituation()");

                        IEntityMapEditorConsole iMeConsole = Utility.GetIEntityMapEditorConsole();
                        iMeConsole.ChangeMode();

                        // Dungeon 処理を復帰
                        IEntityDungeon iDungeon = Utility.GetIEntityDungeon();
                        iDungeon.Pause(false);

                        // 次の Situation を登録
                        this.nextSituation = NpSituation.Create<SituationMapEditor>();
                    }

                    public void CleanUpForce() { }

                } //class RuleMEConsoleToMapEditor


                //------------------------------------------------------------------
                // RuleMEConsoleToMapEditor
                //------------------------------------------------------------------
                public class RuleMEConsoleToNewMap : RuleBase, INpRule
                {
                    public bool CheckRule()
                    {
                        IEntityMapEditorConsole iMeConsole = Utility.GetIEntityMapEditorConsole();
                        bool bDecided = (iMeConsole.GetDetectedMode() == CONSOLE_MODE.DECIDED);
                        bool bSelectNewMap = (iMeConsole.GetDecidedItem() == MAIN_CONSOLE_ITEM.MAP_NEW);

                        return (bDecided && bSelectNewMap);
                    }

                    public void ReadyNextSituation()
                    {
                        Debug.Log("RuleMEConsoleToNewMap.ReadyNextSituation()");
                        Utility.StartCoroutine(this.Ready());
                    }

                    public void CleanUpForce() { }

                    private IEnumerator Ready()
                    {
                        IEntityMapEditorConsole iMeConsole = Utility.GetIEntityMapEditorConsole();
                        iMeConsole.ChangeMode();

                        yield return Utility.RegistEntityNewMap();

                        // 次の Situation を登録
                        this.nextSituation = NpSituation.Create<SituationNewMap>();
                    }

                } //class RuleMEConsoleToMapEditor

            } //namespace mapeditor
        } //namespace dev
    } //namespace situation
} //namespace nangka
