using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Myriad;
using On.MoreSlugcats;
using UnityEngine;
using ElectricSpear = MoreSlugcats.ElectricSpear;
using LillyPuck = IL.MoreSlugcats.LillyPuck;

namespace Chyz;

public class ChyzTouched {
    public PhysicalObject Self;
    public List<Color> CurrentColors;
    public Player Toucher;

    public ChyzTouched(PhysicalObject self, Player toucher) {
        Self = self;
        Toucher = toucher;
        CurrentColors = GetCurrentColors(self);
    }

    public void UpdateColor() {
        for (int i = 0; i < CurrentColors.Count; i++) {
            CurrentColors[i] = Color.Lerp(CurrentColors[i], Toucher.ShortCutColor(), 0.01f);
        }
        ColorHelper<PhysicalObject>.INSTANCE.setColors(Self, CurrentColors);
    }

    public static List<Color> GetCurrentColors(PhysicalObject physicalObject) {
        return ColorHelper<PhysicalObject>.INSTANCE.getColors(physicalObject);

    }

    public static void Touch(PhysicalObject physicalObject, Player player) {
        if (Plugin.chyzTouched.TryGetValue(physicalObject, out ChyzTouched touched)) {
            touched.Toucher = player;
        }
        else {
            Plugin.chyzTouched.Add(physicalObject, new ChyzTouched(physicalObject, player));
        }
        Debug.Log($"{player} Touched {physicalObject}");
    }
}