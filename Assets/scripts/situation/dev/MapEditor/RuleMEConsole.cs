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
                // RuleMEConsoleToNewMap
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
                        Utility.StartCoroutine(this.ReadyNewMap());
                    }

                    public void CleanUpForce() { }

                    private IEnumerator ReadyNewMap()
                    {
                        IEntityMapEditorConsole iMeConsole = Utility.GetIEntityMapEditorConsole();
                        iMeConsole.ChangeMode();

                        yield return Utility.RegistEntityNewMap();

                        // 次の Situation を登録
                        this.nextSituation = NpSituation.Create<SituationNewMap>();
                    }

                } //class RuleMEConsoleToNewMap


                //------------------------------------------------------------------
                // RuleMEConsoleToSaveMap
                //------------------------------------------------------------------
                public class RuleMEConsoleToSaveMap : RuleBase, INpRule
                {
                    public bool CheckRule()
                    {
                        IEntityMapEditorConsole iMeConsole = Utility.GetIEntityMapEditorConsole();
                        bool bDecided = (iMeConsole.GetDetectedMode() == CONSOLE_MODE.DECIDED);
                        bool bSelectSaveMap = (iMeConsole.GetDecidedItem() == MAIN_CONSOLE_ITEM.MAP_SAVE);

                        return (bDecided && bSelectSaveMap);
                    }

                    public void ReadyNextSituation()
                    {
                        Debug.Log("RuleMEConsoleToSaveMap.ReadyNextSituation()");
                        Utility.StartCoroutine(this.ReadySaveMap());
                    }

                    public void CleanUpForce() { }

                    private IEnumerator ReadySaveMap()
                    {
                        IEntityMapEditorConsole iMeConsole = Utility.GetIEntityMapEditorConsole();
                        iMeConsole.ChangeMode();

                        yield return Utility.RegistEntitySaveMap();

                        // 次の Situation を登録
                        this.nextSituation = NpSituation.Create<SituationSaveMap>();
                    }

                } //class RuleMEConsoleToSaveMap


                //------------------------------------------------------------------
                // RuleMEConsoleToLoadMap
                //------------------------------------------------------------------
                public class RuleMEConsoleToLoadMap : RuleBase, INpRule
                {
                    public bool CheckRule()
                    {
                        IEntityMapEditorConsole iMeConsole = Utility.GetIEntityMapEditorConsole();
                        bool bDecided = (iMeConsole.GetDetectedMode() == CONSOLE_MODE.DECIDED);
                        bool bSelectLoadMap = (iMeConsole.GetDecidedItem() == MAIN_CONSOLE_ITEM.MAP_LOAD);

                        return (bDecided && bSelectLoadMap);
                    }

                    public void ReadyNextSituation()
                    {
                        Debug.Log("RuleMEConsoleToLoadMap.ReadyNextSituation()");
                        Utility.StartCoroutine(this.ReadyLoadMap());
                    }

                    public void CleanUpForce() { }

                    private IEnumerator ReadyLoadMap()
                    {
                        IEntityMapEditorConsole iMeConsole = Utility.GetIEntityMapEditorConsole();
                        iMeConsole.ChangeMode();

                        yield return Utility.RegistEntityLoadMap();

                        // 次の Situation を登録
                        this.nextSituation = NpSituation.Create<SituationLoadMap>();
                    }

                } //class RuleMEConsoleToLoadMap

            } //namespace mapeditor
        } //namespace dev
    } //namespace situation
} //namespace nangka
