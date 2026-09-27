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
        Unlimited
    }
}
