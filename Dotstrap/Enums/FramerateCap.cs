namespace Dotstrap.Enums
{
    // written to FramerateCap in Roblox's GlobalBasicSettings before launch. Default leaves the in-game setting alone.
    public enum FramerateCap
    {
        Default,
        [EnumName(StaticName = "300")]
        Fps300,
        [EnumName(StaticName = "360")]
        Fps360,
        [EnumName(StaticName = "480")]
        Fps480,
        [EnumName(StaticName = "1000")]
        Fps1000,
        // no longer offered - roblox treats -1 as invalid and falls back to 60 fps. kept so saved settings still load, and applied as 1000
        Unlimited
    }
}
