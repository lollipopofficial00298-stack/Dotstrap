namespace Dotstrap.Enums.FlagPresets
{
    // Roblox's internal graphics quality levels (Enum.QualityLevel) go from 1 to 21
    public enum RenderQualityLevel
    {
        [EnumName(FromTranslation = "Common.Automatic")]
        Default,
        [EnumName(StaticName = "1")]
        Level1,
        [EnumName(StaticName = "3")]
        Level3,
        [EnumName(StaticName = "5")]
        Level5,
        [EnumName(StaticName = "10")]
        Level10,
        [EnumName(StaticName = "15")]
        Level15,
        [EnumName(StaticName = "21")]
        Level21
    }
}
