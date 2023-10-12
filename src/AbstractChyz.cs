using Fisobs.Core;
using UnityEngine;

namespace Chyz;

sealed class AbstractChyz : AbstractPhysicalObject
{
    public AbstractChyz(World world, WorldCoordinate pos, EntityID ID) : base(world, ChyzFisob.Chyz, null, pos, ID)
    {
        hue = 1f;
        saturation = 0.5f;
        scaleX = 1;
        scaleY = 1;
    }

    public override void Realize()
    {
        base.Realize();
        realizedObject ??= new Chyz(this, world);
    }

    public float hue;
    public float saturation;
    public float scaleX;
    public float scaleY;

    public override string ToString()
    {
        return this.SaveToString($"{hue};{saturation};{scaleX};{scaleY}");
    }
}