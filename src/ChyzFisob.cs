using Fisobs.Core;
using Fisobs.Items;
using Fisobs.Properties;
using Fisobs.Sandbox;

namespace Chyz;

public class ChyzFisob : Fisob {
    public static readonly AbstractPhysicalObject.AbstractObjectType Chyz = new("Chyz", true);
    public static readonly MultiplayerUnlocks.SandboxUnlockID ChyzId = new("Chyz", true);
    private static readonly ChyzProperties properties = new();

    public ChyzFisob() : base(Chyz) {
        Icon = new ChyzIcon();
        SandboxPerformanceCost = new SandboxPerformanceCost(0.2f, 0f);
        RegisterUnlock(ChyzId, MultiplayerUnlocks.SandboxUnlockID.Slugcat, data: 150);
    }

    public override AbstractPhysicalObject Parse(World world, EntitySaveData saveData, SandboxUnlock unlock) {
        string[] array = saveData.CustomData.Split(new char[] {
            ';'
        });
        if (array.Length < 8) {
            array = new string[8];
        }
        float num;
        float num2;
        float num3;
        float num4;
        AbstractChyz abstractChyz = new AbstractChyz(world, saveData.Pos, saveData.ID) {
            hue = (float.TryParse(array[0], out num) ? num : 0f),
            saturation = (float.TryParse(array[1], out num2) ? num2 : 1f),
            scaleX = (float.TryParse(array[2], out num3) ? num3 : 1f),
            scaleY = (float.TryParse(array[3], out num4) ? num4 : 1f)
        };
        bool flag3 = unlock != null;
        if (flag3) {
            abstractChyz.hue = unlock.Data / 1000f;
            bool flag4 = unlock.Data == 0;
            if (flag4) {
                abstractChyz.scaleX += 0.2f;
                abstractChyz.scaleY += 0.2f;
            }
        }
        return abstractChyz;
    }

    public override ItemProperties Properties(PhysicalObject forObject) {
        return properties;
    }
}