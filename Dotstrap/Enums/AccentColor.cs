namespace Dotstrap.Enums
{
    // display names come from the Enums.AccentColor.* translation strings
    public enum AccentColor
    {
        Green,
        Blue,
        Purple,
        Red,
        Orange,
        Teal,
        Pink,
        [EnumName(FromTranslation = "Common.Custom")]
        Custom
    }
}
