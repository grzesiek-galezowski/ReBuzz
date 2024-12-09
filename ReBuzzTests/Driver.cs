using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Threading;
using AtmaFileSystem;
using AtmaFileSystem.IO;
using BuzzGUI.Common;
using BuzzGUI.Interfaces;
using FluentAssertions;
using ReBuzz;
using ReBuzz.AppViews;
using ReBuzz.Core;
using ReBuzz.FileOps;
using ReBuzz.MachineManagement;

namespace ReBuzzTests;

public class Driver : IDisposable, IInitializationObserver
{
    private static readonly AbsoluteDirectoryPath TestDataRootPath = AbsoluteDirectoryPath.Value(Path.GetTempPath()).AddDirectoryName("ReBuzzTestData");
    private readonly AbsoluteDirectoryPath gearDir;
    private readonly AbsoluteDirectoryPath _themesDir;
    private ReBuzzCore reBuzzCore;
    private readonly AbsoluteDirectoryPath _gearEffectsDir;
    private readonly AbsoluteDirectoryPath _gearGeneratorsDir; //bug eliminate all underscores
    private readonly AbsoluteDirectoryPath _reBuzzRootDir =
      TestDataRootPath.AddDirectoryName($"{Guid.NewGuid()}__{DateTime.UtcNow.Ticks}");

    static Driver()
    {
        AssertionOptions.FormattingOptions.MaxLines = 10000;
        AttemptToCleanupTestRootDirs();
    }

    public Driver()
    {
        gearDir = _reBuzzRootDir.AddDirectoryName("Gear"); //bug delete the dir after test (if possible)
        _gearEffectsDir = gearDir.AddDirectoryName("Effects");
        _gearGeneratorsDir = gearDir.AddDirectoryName("Generators");
        _themesDir = _reBuzzRootDir.AddDirectoryName("Themes");
    }

    public void AssertGearMachinesConsistOf(ImmutableList<string> expectedMachineNames)
    {
        reBuzzCore.Gear.Machine.Select(m => m.Name).Should().Equal(expectedMachineNames);
    }

    public void Start()
    {
        SetupDirectoryStructure();

        var engineSettings = Global.EngineSettings;
        var buzzPath = _reBuzzRootDir.ToString();
        var generalSettings = Global.GeneralSettings;

        var dispatcher = new GuiLessDispatcher();
        var registryExInstance = new ProductionDataProtectingRegistryEx(new RegistryExInstance());
        reBuzzCore = new ReBuzzCore(generalSettings, engineSettings, buzzPath, Global.RegistryRoot, new FakeMachineDLLScanner(gearDir), dispatcher, registryExInstance);
        reBuzzCore.SelectedTheme = "<default>"; //bug this is actually written to registry!

        var initialization = new ReBuzzCoreInitialization(reBuzzCore, buzzPath, dispatcher, registryExInstance);
        initialization.StartReBuzzEngineStep1((_, args) =>
        {
            TestContext.Out.WriteLine("PropertyChanged: " + args.PropertyName);
        });
        initialization.StartReBuzzEngineStep2(IntPtr.MaxValue);
        initialization.StartReBuzzEngineStep3(engineSettings, this);
        initialization.StartReBuzzEngineStep4(
          machineDb: new FakeMachineDb(),
          machineDbDatabaseEvent: s => { TestContext.Out.WriteLine("DatabaseEvent: " + s); },
          onPatternEditorActivated: () => { TestContext.Out.WriteLine("OnPatternEditorActivated"); },
          onSequenceEditorActivated: () => { TestContext.Out.WriteLine("OnSequenceEditorActivated"); },
          onShowSettings: s => { TestContext.Out.WriteLine("OnShowSettings: " + s); },
          onSetPatternEditorControl: control => { TestContext.Out.WriteLine("SetPatternEditorControl: " + control); },
          onFullScreenChanged: b => { TestContext.Out.WriteLine("OnFullScreenChanged: " + b); },
          onThemeChanged: s => { TestContext.Out.WriteLine("OnThemeChanged: " + s); }
        );

        initialization.StartReBuzzEngineStep5(s => { TestContext.Out.WriteLine("OpenFile: " + s); });
        initialization.StartReBuzzEngineStep6();

        reBuzzCore.BuzzCommandRaised += command =>
        {
            TestContext.Out.WriteLine("Command: " + command);
            if (command == BuzzCommand.Exit)
            {
                //bug
                reBuzzCore.Playing = false;
                reBuzzCore.Release();
            }
        };
    }

    public void NewFile()
    {
        reBuzzCore.ExecuteCommand(BuzzCommand.NewFile);
    }

    private void SetupDirectoryStructure()
    {
        gearDir.Create();
        _gearEffectsDir.Create();
        _gearGeneratorsDir.Create();
        _themesDir.Create();
        foreach (var gearFile in AbsoluteDirectoryPath.OfThisFile().AddDirectoryName("Gear").EnumerateFiles())
        {
            gearFile.Copy(gearDir + gearFile.FileName(), overwrite: true);
        }
    }

    public void Dispose()
    {
        reBuzzCore?.ExecuteCommand(BuzzCommand.Exit); //bug logs
    }

    public void AssertRequiredPropertiesAreInitialized()
    {
        //bug is this really needed?
        reBuzzCore.Should().NotBeNull();
        reBuzzCore.Gear.Should().NotBeNull();
        reBuzzCore.Gear.Machine.Should().NotBeEmpty();
        reBuzzCore.AudioEngine.Should().NotBeNull();
        reBuzzCore.SongCore.Should().NotBeNull();
        reBuzzCore.SongCore.BuzzCore.Should().Be(reBuzzCore);
        reBuzzCore.SongCore.WavetableCore.Should().NotBeNull();
        reBuzzCore.MachineManager.Should().NotBeNull();
    }

    void IInitializationObserver.NotifyMachineManagerCreated(MachineManager machineManager)
    {
        TestContext.Out.WriteLine("MachineManager created");
    }

    public void AssertInitialState()
    {
        InitialStateAssertions.AssertInitialState(gearDir, this.reBuzzCore);
    }

    private static void AttemptToCleanupTestRootDirs()
    {
        foreach (var singleTestRoot in TestDataRootPath.EnumerateDirectories())
        {
            try
            {
                singleTestRoot.Delete(true);
                TestContext.Out.WriteLine("Deleted " + singleTestRoot);
            }
            catch
            {
                TestContext.Out.WriteLine("Could not delete " + singleTestRoot);
            }
        }
    }
}

public class ProductionDataProtectingRegistryEx(IRegistryEx registryExInstance) : IRegistryEx //bug move
{
    private readonly Lock lockObject = new();
    private readonly Dictionary<(string Path, string Key), object> registry = new();
    public void Write<T>(string key, T x, string path = "BuzzGUI")
    {
        lock (lockObject)
        {
            Console.WriteLine("Write to memory: " + key + " " + x + " " + path); //bug
            registry[(path, key)] = x;
        }
    }

    public T Read<T>(string key, T def, string path = "BuzzGUI")
    {
        lock (lockObject)
        {
            T result;
            if (registry.TryGetValue((path, key), out var cachedResult))
            {
                result = (T)cachedResult;
            }
            else
            {
                result = registryExInstance.Read(key, def, path);
            }

            Console.WriteLine("Read: " + key + " " + result + " " + path);
            return result;
        }
    }

    public IEnumerable<T> ReadNumberedList<T>(string key, string path = "BuzzGUI")
    {
        lock (lockObject)
        {
            IEnumerable<T> numberedList;
            if (registry.TryGetValue((path, key), out var cachedResult))
            {
                numberedList = (IEnumerable<T>)cachedResult;
            }
            else
            {
                numberedList = registryExInstance.ReadNumberedList<T>(key, path);
            }

            Console.WriteLine("ReadNumberedList: " + key + " [" + string.Join(", ", numberedList) + "] " + path);
            return numberedList;
        }
    }

}

public class GuiLessDispatcher : IUiDispatcher //bug move
{
    public void Invoke(Action action)
    {
        action();
    }

    public void BeginInvoke(Action action)
    {
        action();
    }

    public void BeginInvoke(Action action, DispatcherPriority priority)
    {
        action();
    }
}

internal class FakeMachineDb : IMachineDatabase //bug move
{
    public event Action<string>? DatabaseEvent;

    public Dictionary<int, MachineDatabase.InstrumentInfo> DictLibRef { get; set; } = new();
    public void CreateDB()
    {

    }

    public string GetLibName(int id)
    {
        return "NOT_IMPLEMENTED";
    }

    public MenuItemCore IndexMenu { get; } = new MenuItemCore();
}