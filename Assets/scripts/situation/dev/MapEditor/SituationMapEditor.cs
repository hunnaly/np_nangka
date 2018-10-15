using UnityEngine;
using np;

namespace nangka
{
    namespace situation
    {
        namespace dev
        {
            namespace mapeditor
            {
                public class SituationMapEditor : NpSituation
                {
                    protected override bool CreateRules()
                    {
                        Debug.Log("SituationMapEditor.CreateRules()");
                        this.CreateRule<RuleMapEditorToMEConsole>();
                        return true;
                    }

                } // class SituaionMapEditor

            } //namespace mapeditor
        } //namespace dev
    } //namespace situation
} //namespace nangka
