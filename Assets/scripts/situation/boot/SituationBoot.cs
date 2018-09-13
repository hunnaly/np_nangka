using System.Collections;
using System.Collections.Generic;
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
                    INpRule rule = this.CreateRule<RuleBootToDungeon>();
                    return (rule != null);
                }

            }

        } //namespace boot
    } //namespace situation
} //namespace nangka
