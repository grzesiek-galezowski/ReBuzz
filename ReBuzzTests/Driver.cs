using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using AtmaFileSystem;
using AtmaFileSystem.IO;
using BuzzGUI.Common;
using BuzzGUI.Interfaces;
using FluentAssertions;
using ReBuzz;
using ReBuzz.AppViews;
using ReBuzz.Audio;
using ReBuzz.Core;
using ReBuzz.FileOps;
using ReBuzz.MachineManagement;

namespace ReBuzzTests;

public class Driver : IDisposable, IInitializationObserver
{
  private readonly AbsoluteDirectoryPath _gearDir;
  private readonly AbsoluteDirectoryPath _themesDir;
  private ReBuzzCore reBuzzCore;
  private readonly AbsoluteDirectoryPath _gearEffectsDir;
  private readonly AbsoluteDirectoryPath _gearGeneratorsDir; //bug eliminate all underscores
  private readonly AbsoluteDirectoryPath _rebuzzRootDir = AbsoluteDirectoryPath.Value(Path.GetTempPath()).AddDirectoryName("ReBuzzTestData").AddDirectoryName(Guid.NewGuid().ToString());

  static Driver()
  {
    AssertionOptions.FormattingOptions.MaxLines = 10000;
  }

  public Driver()
  {
    _gearDir = _rebuzzRootDir.AddDirectoryName("Gear"); //bug delete the dir after test (if possible)
    _gearEffectsDir = _gearDir.AddDirectoryName("Effects");
    _gearGeneratorsDir = _gearDir.AddDirectoryName("Generators");
    _themesDir = _rebuzzRootDir.AddDirectoryName("Themes");
  }

  public ReBuzzCore ReBuzzCore => reBuzzCore; //bug hide

  public void AssertGearMachinesConsistOf(ImmutableList<string> expectedMachineNames)
  {
    reBuzzCore.Gear.Machine.Select(m => m.Name).Should().Equal(expectedMachineNames);
  }

  public void Start()
  {
    SetupDirectoryStructure();

    var engineSettings = Global.EngineSettings;
    var buzzPath = _rebuzzRootDir.ToString();
    var generalSettings = Global.GeneralSettings;
    var registryRoot = Global.RegistryRoot;

    var dispatcher = new GuiLessDispatcher();
    reBuzzCore = new ReBuzzCore(generalSettings, engineSettings, buzzPath, registryRoot, new FakeMachineDLLScanner(_gearDir), dispatcher);
    reBuzzCore.SelectedTheme = "<default>"; //bug this is actually written to registry!

    var initialization = new ReBuzzCoreInitialization(reBuzzCore, buzzPath, dispatcher);
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
    _gearDir.Create();
    _gearEffectsDir.Create();
    _gearGeneratorsDir.Create();
    _themesDir.Create();
    foreach (var gearFile in AbsoluteDirectoryPath.OfThisFile().AddDirectoryName("Gear").EnumerateFiles())
    {
      gearFile.Copy(_gearDir + gearFile.FileName(), overwrite: true);
    }
  }

  public void Dispose()
  {
    reBuzzCore?.ExecuteCommand(BuzzCommand.Exit); //bug logs
  }

  public void AssertRequiredPropertiesAreInitialized()
  {
    //bug is this really needed?
    ReBuzzCore.Should().NotBeNull();
    ReBuzzCore.Gear.Should().NotBeNull();
    ReBuzzCore.Gear.Machine.Should().NotBeEmpty();
    ReBuzzCore.AudioEngine.Should().NotBeNull();
    ReBuzzCore.SongCore.Should().NotBeNull();
    ReBuzzCore.SongCore.BuzzCore.Should().Be(ReBuzzCore);
    ReBuzzCore.SongCore.WavetableCore.Should().NotBeNull();
    ReBuzzCore.MachineManager.Should().NotBeNull();
  }

  void IInitializationObserver.NotifyMachineManagerCreated(MachineManager machineManager)
  {
    TestContext.Out.WriteLine("MachineManager created");
  }

  //bug void IInitializationObserver.NotifyMachineDbCreated(IMachineDatabase machineDb)
  //bug {
  //bug   TestContext.Out.WriteLine("MachineDb created");
  //bug }
  public void AssertInitialState()
  {
    InitialStateAssertions.AssertInitialState(_gearDir, this.ReBuzzCore);
  }
}

public class GuiLessDispatcher : IUiDispatcher //bug
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

internal class FakeMachineDb : IMachineDatabase
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