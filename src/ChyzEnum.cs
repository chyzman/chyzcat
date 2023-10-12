namespace Chyz;

public static class ChyzEnum {
    public static Player.BodyModeIndex CeilingClimb;
    public static Player.BodyModeIndex BackWallClimb;
    public static bool isReg;

    public static void RegisterValues() {
        CeilingClimb = new Player.BodyModeIndex("CeilingClimb", true);
        BackWallClimb = new Player.BodyModeIndex("BackWallClimb", true);
        isReg = true;
    }

    public static void UnregisterValues() {
        if (!isReg)
            return;
        CeilingClimb.Unregister();
        BackWallClimb.Unregister();
        CeilingClimb = null;
        isReg = false;
    }
}