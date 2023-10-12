using Fisobs.Core;
using RWCustom;
using UnityEngine;

namespace Chyz;

internal sealed class ChyzIcon : Icon {
    public override int Data(AbstractPhysicalObject apo) {
        return apo is AbstractChyz chyz ? (int)(chyz.hue * 1000f) : 0;
    }

    public override Color SpriteColor(int data) {
        return Custom.HSL2RGB(data / 1000f, 1f, 0.5f);
    }

    public override string SpriteName(int data) {
        return "icon_Chyz";
    }
}