using UnityEngine;
using np;

namespace nangka {
    namespace situation {
        namespace boot
        {
            public class SituationBoot : NpSituation
            {
                protected override bool CreateRules()
                {
                    Debug.Log("SituationBoot.CreateRules()");
                    //INpRule rule = this.CreateRule<RuleBootToDungeon>();
                    INpRule rule = this.CreateRule<RuleBootToDevEntrance>();
                    return (rule != null);
                }

            }

        } //namespace boot
    } //namespace situation
} //namespace nangka
