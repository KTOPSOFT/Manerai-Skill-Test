using UnityEngine;
using UnityEngine.UI;

using System;

using System.Collections;
using System.Collections.Generic;

namespace YukiOno.SkillTest
{
    public class RadialSlider : CustomSlider
    {
        protected override void OnValidate()
        {
            listCount = progressBars.Count;

            ValidateImages(Image.FillMethod.Radial360, 2, 2);

            UpdateValues(true);
        }
    }
}









