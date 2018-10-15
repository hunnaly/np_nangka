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

                        // 次の Situation を登録
                        this.nextSituation = NpSituation.Create<SituationMapEditor>();
                    }

                    public void CleanUpForce()
                    {
                        Debug.Log("RuleMEConsoleToMapEditor.CleanUpForce()");
                    }

                } //class RuleMEConsoleToMapEditor

            } //namespace mapeditor
        } //namespace dev
    } //namespace situation
} //namespace nangka
