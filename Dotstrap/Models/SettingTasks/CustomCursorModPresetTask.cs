using Dotstrap.Models.SettingTasks.Base;

namespace Dotstrap.Models.SettingTasks
{
    // lets the user browse their own cursor image, applied to both the regular and "far" arrow
    // cursor content paths - the same overlay mechanism the built-in cursor presets already use
    public class CustomCursorModPresetTask : StringBaseTask
    {
        private static readonly string[] TargetPaths = new[]
        {
            @"content\textures\Cursors\KeyboardMouse\ArrowCursor.png",
            @"content\textures\Cursors\KeyboardMouse\ArrowFarCursor.png"
        };

        private static string FullPath(string relative) => Path.Combine(Paths.Modifications, relative);

        public CustomCursorModPresetTask() : base("ModPreset", "CustomCursor")
        {
            if (TargetPaths.All(x => File.Exists(FullPath(x))))
                OriginalState = FullPath(TargetPaths[0]);
        }

        public override void Execute()
        {
            if (!String.IsNullOrEmpty(NewState) && File.Exists(NewState))
            {
                foreach (var relative in TargetPaths)
                {
                    string full = FullPath(relative);

                    Directory.CreateDirectory(Path.GetDirectoryName(full)!);
                    Filesystem.AssertReadOnly(full);
                    File.Copy(NewState, full, true);
                }
            }
            else
            {
                foreach (var relative in TargetPaths)
                {
                    string full = FullPath(relative);

                    if (File.Exists(full))
                    {
                        Filesystem.AssertReadOnly(full);
                        File.Delete(full);
                    }
                }
            }

            OriginalState = NewState;
        }
    }
}
