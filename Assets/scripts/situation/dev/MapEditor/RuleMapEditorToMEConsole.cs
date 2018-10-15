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
                public class RuleMapEditorToMEConsole : RuleBase, INpRule
                {
                    public bool CheckRule()
                    {
                        IEntityMapEditorConsole iMeConsole = Utility.GetIEntityMapEditorConsole();
                        return (iMeConsole.GetDetectedMode() == CONSOLE_MODE.MAIN);
                    }

                    public void ReadyNextSituation()
                    {
                        Debug.Log("RuleMapEditorToMEConsole.ReadyNextSituation()");

                        IEntityMapEditorConsole iMeConsole = Utility.GetIEntityMapEditorConsole();
                        iMeConsole.ChangeMode();

                        // 次の Situation を登録
                        this.nextSituation = NpSituation.Create<SituationMEConsole>();
                    }

                    public void CleanUpForce()
                    {
                        Debug.Log("RuleMapEditorToMEConsole.CleanUpForce()");
                    }

                } //class RuleMapEditorToMEConsole

            } //namespace mapeditor
        } //namespace dev
    } //namespace situation
} //namespace nangka
