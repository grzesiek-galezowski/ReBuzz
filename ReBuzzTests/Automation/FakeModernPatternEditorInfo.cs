using AtmaFileSystem;
using Buzz.MachineInterface;
using BuzzGUI.Interfaces;
using ReBuzz.Core;

namespace ReBuzzTests.Automation
{
    public static class FakeModernPatternEditorInfo
    {
        internal static MachineDLL GetMachineDll(ReBuzzCore buzz, AbsoluteFilePath location)
        {
            MachineDecl? decl = FakeModernPatternEditor.GetMachineDecl();
            return new MachineDLL
            {
                Name = decl.Name,
                Buzz = buzz,
                Path = location.ToString(),
                Is64Bit = true,
                IsCrashed = false,
                IsManaged = true,
                IsLoaded = false,
                IsMissing = false,
                IsOutOfProcess = false,
                ManagedDLL = null,
                MachineInfo = new MachineInfo()
                {
                    Flags =
                        MachineInfoFlags.NO_OUTPUT | MachineInfoFlags.CONTROL_MACHINE |
                        MachineInfoFlags.PATTERN_EDITOR | MachineInfoFlags.LOAD_DATA_RUNTIME,
                    Author = decl.Author,
                    InternalVersion = 0,
                    MaxTracks = decl.MaxTracks,
                    MinTracks = 1,
                    Name = decl.Name,
                    ShortName = decl.ShortName,
                    Type = MachineType.Effect,
                    Version = 66
                },
                Presets = null,
                SHA1Hash = "258A3DE5BA33E71D69271E36557EA8E4E582298E",
                GUIFactoryDecl =
                    new MachineGUIFactoryDecl
                        {
                            IsGUIResizable = true, PreferWindowedGUI = true, UseThemeStyles = false
                        },
                ModuleHandle = 0
            };
        }

        public static string DllName => "FakeModernPatternEditor.dll";
    }
}