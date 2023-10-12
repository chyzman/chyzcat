namespace Chyz; 

using Menu.Remix.MixedUI;
using RWCustom;
using UnityEngine;

public class ChyzOption : OptionInterface
  {
    public ConfigurableInfo configInfo;
    private UIelement[] UI1;
    internal readonly Plugin instance;

    public ChyzOption()
    {
      configInfo = null;
    }

    public override void Initialize()
    {
      base.Initialize();
      InGameTranslator inGameTranslator = Custom.rainWorld.inGameTranslator;
      OpTab configTab = new OpTab( this, inGameTranslator.Translate("Config"));
      Tabs = new OpTab[1]{configTab};

      configTab.AddItems(new UIelement[]
      {
        new OpLabel(60f, 440f, inGameTranslator.Translate("Hi"), true),
        new OpLabel(90f, 410f, inGameTranslator.Translate("Idk what to put here"), false),
      });
    }
  }