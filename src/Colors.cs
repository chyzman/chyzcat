namespace Chyz;

public class Colors {
    public static void Init() {
        On.ExplosiveSpear.ApplyPalette += (orig, self, leaser, cam, palette) => {
            orig(self, leaser, cam, palette);
            if (!Plugin.chyzTouched.TryGetValue(self, out ChyzTouched touched)) return;
            touched.UpdateColor();
        };
    }
}