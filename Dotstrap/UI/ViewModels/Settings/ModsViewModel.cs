using System.IO.Compression;
using System.Windows;
using System.Windows.Input;

using Microsoft.Win32;

using Windows.Win32;
using Windows.Win32.UI.Shell;
using Windows.Win32.Foundation;

using CommunityToolkit.Mvvm.Input;

using Dotstrap.Models.SettingTasks;
using Dotstrap.AppData;

namespace Dotstrap.UI.ViewModels.Settings
{
    public class ModsViewModel : NotifyPropertyChangedViewModel
    {
        private void OpenModsFolder() => Process.Start("explorer.exe", Paths.Modifications);

        private readonly Dictionary<string, byte[]> FontHeaders = new()
        {
            { "ttf", new byte[4] { 0x00, 0x01, 0x00, 0x00 } },
            { "otf", new byte[4] { 0x4F, 0x54, 0x54, 0x4F } },
            { "ttc", new byte[4] { 0x74, 0x74, 0x63, 0x66 } } 
        };

        private void ManageCustomFont()
        {
            if (!String.IsNullOrEmpty(TextFontTask.NewState))
            {
                TextFontTask.NewState = "";
            }
            else
            {
                var dialog = new OpenFileDialog
                {
                    Filter = $"{Strings.Menu_FontFiles}|*.ttf;*.otf;*.ttc"
                };

                if (dialog.ShowDialog() != true)
                    return;

                string type = dialog.FileName.Substring(dialog.FileName.Length-3, 3).ToLowerInvariant();

                if (!FontHeaders.ContainsKey(type) 
                    || !FontHeaders.Any(x => File.ReadAllBytes(dialog.FileName).Take(4).SequenceEqual(x.Value)))
                {
                    Frontend.ShowMessageBox(Strings.Menu_Mods_Misc_CustomFont_Invalid, MessageBoxImage.Error);
                    return;
                }

                TextFontTask.NewState = dialog.FileName;
            }

            OnPropertyChanged(nameof(ChooseCustomFontVisibility));
            OnPropertyChanged(nameof(DeleteCustomFontVisibility));
        }

        private void ManageCustomCursor()
        {
            if (!String.IsNullOrEmpty(CustomCursorTask.NewState))
            {
                CustomCursorTask.NewState = "";
            }
            else
            {
                var dialog = new OpenFileDialog
                {
                    Filter = $"{Strings.FileTypes_ImageFiles}|*.png;*.bmp"
                };

                if (dialog.ShowDialog() != true)
                    return;

                CustomCursorTask.NewState = dialog.FileName;
            }

            OnPropertyChanged(nameof(ChooseCustomCursorVisibility));
            OnPropertyChanged(nameof(DeleteCustomCursorVisibility));
        }

        private void ExportMods()
        {
            var dialog = new SaveFileDialog
            {
                FileName = $"{App.ProjectName}-Mods.zip",
                Filter = $"{Strings.FileTypes_ZipArchive}|*.zip"
            };

            if (dialog.ShowDialog() != true)
                return;

            if (!Directory.Exists(Paths.Modifications) || !Directory.EnumerateFileSystemEntries(Paths.Modifications).Any())
            {
                Frontend.ShowMessageBox(Strings.Menu_Mods_Misc_Backup_NothingToExport, MessageBoxImage.Information);
                return;
            }

            if (File.Exists(dialog.FileName))
                File.Delete(dialog.FileName);

            ZipFile.CreateFromDirectory(Paths.Modifications, dialog.FileName, CompressionLevel.Optimal, false);

            Frontend.ShowMessageBox(Strings.Menu_Mods_Misc_Backup_ExportDone, MessageBoxImage.Information);
        }

        private void ImportMods()
        {
            var dialog = new OpenFileDialog
            {
                Filter = $"{Strings.FileTypes_ZipArchive}|*.zip"
            };

            if (dialog.ShowDialog() != true)
                return;

            var result = Frontend.ShowMessageBox(Strings.Menu_Mods_Misc_Backup_ImportConfirm, MessageBoxImage.Warning, MessageBoxButton.YesNo);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                Directory.CreateDirectory(Paths.Modifications);
                ZipFile.ExtractToDirectory(dialog.FileName, Paths.Modifications, true);
            }
            catch (Exception ex)
            {
                Frontend.ShowMessageBox($"{Strings.Menu_Mods_Misc_Backup_ImportFailed}\n\n{ex.Message}", MessageBoxImage.Error);
                return;
            }

            Frontend.ShowMessageBox(Strings.Menu_Mods_Misc_Backup_ImportDone, MessageBoxImage.Information);
        }

        private void ResetAllMods()
        {
            var result = Frontend.ShowMessageBox(Strings.Menu_Mods_Misc_ResetAllMods_Confirm, MessageBoxImage.Warning, MessageBoxButton.YesNo);

            if (result != MessageBoxResult.Yes)
                return;

            if (Directory.Exists(Paths.Modifications))
            {
                foreach (var entry in Directory.GetFileSystemEntries(Paths.Modifications))
                {
                    // never touch ClientSettings - that's where FastFlags live, not mods
                    if (String.Equals(Path.GetFileName(entry), "ClientSettings", StringComparison.OrdinalIgnoreCase))
                        continue;

                    try
                    {
                        if (Directory.Exists(entry))
                            Directory.Delete(entry, true);
                        else
                            File.Delete(entry);
                    }
                    catch (Exception ex)
                    {
                        App.Logger.WriteException("ModsViewModel::ResetAllMods", ex);
                    }
                }
            }

            OldAvatarBackgroundTask.NewState = false;
            OldCharacterSoundsTask.NewState = false;
            CursorTypeTask.NewState = default;
            EmojiFontTask.NewState = default;
            TextFontTask.NewState = "";
            CustomCursorTask.NewState = "";

            OnPropertyChanged(nameof(ChooseCustomFontVisibility));
            OnPropertyChanged(nameof(DeleteCustomFontVisibility));
            OnPropertyChanged(nameof(ChooseCustomCursorVisibility));
            OnPropertyChanged(nameof(DeleteCustomCursorVisibility));

            Frontend.ShowMessageBox(Strings.Menu_Mods_Misc_ResetAllMods_Done, MessageBoxImage.Information);
        }

        public ICommand OpenModsFolderCommand => new RelayCommand(OpenModsFolder);

        public ICommand ManageCustomCursorCommand => new RelayCommand(ManageCustomCursor);

        public ICommand ExportModsCommand => new RelayCommand(ExportMods);

        public ICommand ImportModsCommand => new RelayCommand(ImportMods);

        public ICommand ResetAllModsCommand => new RelayCommand(ResetAllMods);

        public CustomCursorModPresetTask CustomCursorTask { get; } = new();

        public Visibility ChooseCustomCursorVisibility => !String.IsNullOrEmpty(CustomCursorTask.NewState) ? Visibility.Collapsed : Visibility.Visible;

        public Visibility DeleteCustomCursorVisibility => !String.IsNullOrEmpty(CustomCursorTask.NewState) ? Visibility.Visible : Visibility.Collapsed;

        public Visibility ChooseCustomFontVisibility => !String.IsNullOrEmpty(TextFontTask.NewState) ? Visibility.Collapsed : Visibility.Visible;

        public Visibility DeleteCustomFontVisibility => !String.IsNullOrEmpty(TextFontTask.NewState) ? Visibility.Visible : Visibility.Collapsed;

        public ICommand ManageCustomFontCommand => new RelayCommand(ManageCustomFont);

        public ICommand OpenCompatSettingsCommand => new RelayCommand(OpenCompatSettings);

        public ModPresetTask OldAvatarBackgroundTask { get; } = new("OldAvatarBackground", @"ExtraContent\places\Mobile.rbxl", "OldAvatarBackground.rbxl");

        public ModPresetTask OldCharacterSoundsTask { get; } = new("OldCharacterSounds", new()
        {
            { @"content\sounds\action_footsteps_plastic.mp3", "Sounds.OldWalk.mp3"  },
            { @"content\sounds\action_jump.mp3",              "Sounds.OldJump.mp3"  },
            { @"content\sounds\action_get_up.mp3",            "Sounds.OldGetUp.mp3" },
            { @"content\sounds\action_falling.mp3",           "Sounds.Empty.mp3"    },
            { @"content\sounds\action_jump_land.mp3",         "Sounds.Empty.mp3"    },
            { @"content\sounds\action_swim.mp3",              "Sounds.Empty.mp3"    },
            { @"content\sounds\impact_water.mp3",             "Sounds.Empty.mp3"    }
        });

        public EmojiModPresetTask EmojiFontTask { get; } = new();

        public EnumModPresetTask<Enums.CursorType> CursorTypeTask { get; } = new("CursorType", new()
        {
            {
                Enums.CursorType.From2006, new()
                {
                    { @"content\textures\Cursors\KeyboardMouse\ArrowCursor.png",    "Cursor.From2006.ArrowCursor.png"    },
                    { @"content\textures\Cursors\KeyboardMouse\ArrowFarCursor.png", "Cursor.From2006.ArrowFarCursor.png" }
                }
            },
            {
                Enums.CursorType.From2013, new()
                {
                    { @"content\textures\Cursors\KeyboardMouse\ArrowCursor.png",    "Cursor.From2013.ArrowCursor.png"    },
                    { @"content\textures\Cursors\KeyboardMouse\ArrowFarCursor.png", "Cursor.From2013.ArrowFarCursor.png" }
                }
            }
        });

        public FontModPresetTask TextFontTask { get; } = new();

        private void OpenCompatSettings()
        {
            string path = new RobloxPlayerData().ExecutablePath;

            if (File.Exists(path))
                PInvoke.SHObjectProperties(HWND.Null, SHOP_TYPE.SHOP_FILEPATH, path, "Compatibility");
            else
                Frontend.ShowMessageBox(Strings.Common_RobloxNotInstalled, MessageBoxImage.Error);

        }
    }
}
