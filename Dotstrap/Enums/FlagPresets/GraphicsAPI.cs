namespace Dotstrap.Enums.FlagPresets
{
    public enum GraphicsAPI
    {
        [EnumName(FromTranslation = "Common.Automatic")]
        Default,
        [EnumName(StaticName = "Direct3D 11")]
        Direct3D11,
        [EnumName(StaticName = "Vulkan")]
        Vulkan,
        [EnumName(StaticName = "OpenGL")]
        OpenGL
    }
}
