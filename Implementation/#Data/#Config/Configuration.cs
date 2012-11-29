﻿// This file is provided unter the terms of the 
// Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License.
// To view a copy of this license, visit http://creativecommons.org/licenses/by-nc-sa/3.0/.
// 
// Written by CoderCow

using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Schema;

namespace Terraria.Plugins.CoderCow.AdvancedCircuits {
  public class Configuration {
    #region [Constants]
    public const string CurrentVersion = "1.2";
    #endregion

    #region [Property: OverrideVanillaCircuits]
    private bool overrideVanillaCircuits;

    public bool OverrideVanillaCircuits {
      get { return this.overrideVanillaCircuits; }
      set { this.overrideVanillaCircuits = value; }
    }
    #endregion

    #region [Property: AdvancedCircuitsEnabled]
    private bool advancedCircuitsEnabled;

    public bool AdvancedCircuitsEnabled {
      get { return this.advancedCircuitsEnabled; }
      set { this.advancedCircuitsEnabled = value; }
    }
    #endregion

    #region [Property: MaxDartTrapsPerCircuit]
    private int maxDartTrapsPerCircuit;

    public int MaxDartTrapsPerCircuit {
      get { return this.maxDartTrapsPerCircuit; }
      set { this.maxDartTrapsPerCircuit = value; }
    }
    #endregion

    #region [Property: MaxStatuesPerCircuit]
    private int maxStatuesPerCircuit;

    public int MaxStatuesPerCircuit {
      get { return this.maxStatuesPerCircuit; }
      set { this.maxStatuesPerCircuit = value; }
    }
    #endregion

    #region [Property: MaxPumpsPerCircuit]
    private int maxPumpsPerCircuit;

    public int MaxPumpsPerCircuit {
      get { return this.maxPumpsPerCircuit; }
      set { this.maxPumpsPerCircuit = value; }
    }
    #endregion

    #region [Property: MaxCircuitLength]
    private int maxCircuitLength;

    public int MaxCircuitLength {
      get { return this.maxCircuitLength; }
      set { this.maxCircuitLength = value; }
    }
    #endregion

    #region [Property: BoulderWirePermission]
    private string boulderWirePermission;

    public string BoulderWirePermission {
      get { return this.boulderWirePermission; }
      set { this.boulderWirePermission = value; }
    }
    #endregion

    #region [Property: BlockActivatorConfig]
    private BlockActivatorConfig blockActivatorConfig;

    public BlockActivatorConfig BlockActivatorConfig {
      get { return this.blockActivatorConfig; }
      set { this.blockActivatorConfig = value; }
    }
    #endregion

    #region [Property: PumpConfigs]
    private Dictionary<ComponentConfigProfile,PumpConfig> pumpConfigs;

    public Dictionary<ComponentConfigProfile,PumpConfig> PumpConfigs {
      get { return this.pumpConfigs; }
      set { this.pumpConfigs = value; }
    }
    #endregion

    #region [Property: DartTrapConfigs]
    private Dictionary<ComponentConfigProfile,DartTrapConfig> dartTrapConfigs;

    public Dictionary<ComponentConfigProfile,DartTrapConfig> DartTrapConfigs {
      get { return this.dartTrapConfigs; }
      set { this.dartTrapConfigs = value; }
    }
    #endregion

    #region [Property: StatueConfigs]
    private Dictionary<StatueType,StatueConfig> statueConfigs;

    public Dictionary<StatueType,StatueConfig> StatueConfigs {
      get { return this.statueConfigs; }
      set { this.statueConfigs = value; }
    }
    #endregion


    #region [Methods: Constructor, Static Read]
    public Configuration(): this(true) {}

    protected Configuration(bool fillDictionaries) {
      this.overrideVanillaCircuits = false;
      this.advancedCircuitsEnabled = true;
      this.maxDartTrapsPerCircuit = 10;
      this.maxStatuesPerCircuit = 10;
      this.maxPumpsPerCircuit = 4;
      this.maxCircuitLength = 400;

      this.blockActivatorConfig = new BlockActivatorConfig();

      this.pumpConfigs = new Dictionary<ComponentConfigProfile,PumpConfig>();
      if (fillDictionaries)
        this.pumpConfigs.Add(ComponentConfigProfile.Default, new PumpConfig());

      this.dartTrapConfigs = new Dictionary<ComponentConfigProfile,DartTrapConfig>();
      if (fillDictionaries)
        this.dartTrapConfigs.Add(ComponentConfigProfile.Default, new DartTrapConfig());

      this.statueConfigs = new Dictionary<StatueType,StatueConfig>();
    }

    public static Configuration Read(string filePath) {
      XmlReaderSettings configReaderSettings = new XmlReaderSettings {
        ValidationType = ValidationType.Schema,
        ValidationFlags = XmlSchemaValidationFlags.ProcessIdentityConstraints | XmlSchemaValidationFlags.ReportValidationWarnings
      };
      
      string configSchemaPath = Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath) + ".xsd");
      configReaderSettings.Schemas.Add(null, configSchemaPath);

      XmlDocument document = new XmlDocument();
      using (XmlReader configReader = XmlReader.Create(filePath, configReaderSettings)) {
        document.Load(configReader);
      }

      // Before validating using the schema, first check if the configuration file's version matches with the supported version.
      XmlElement rootElement = document.DocumentElement;
      string fileVersionRaw;
      if (rootElement.HasAttribute("Version"))
        fileVersionRaw = rootElement.GetAttribute("Version");
      else
        fileVersionRaw = "1.0";
      
      if (fileVersionRaw != Configuration.CurrentVersion) {
        throw new FormatException(string.Format(
          "The configuration file is either outdated or too new. Expected version was: {0}. File version is: {1}", 
          Configuration.CurrentVersion, fileVersionRaw
        ));
      }

      document.Validate((sender, args) => {
        if (args.Severity == XmlSeverityType.Warning)
          AdvancedCircuitsPlugin.Trace.WriteLineWarning("Configuration validation warning: " + args.Message);
      });
      
      Configuration resultingConfig = new Configuration(false);
      resultingConfig.overrideVanillaCircuits = BoolEx.ParseEx(rootElement["OverrideVanillaCircuits"].InnerXml);
      resultingConfig.advancedCircuitsEnabled = BoolEx.ParseEx(rootElement["AdvancedCircuitsEnabled"].InnerText);
      resultingConfig.maxDartTrapsPerCircuit  = int.Parse(rootElement["MaxDartTrapsPerCircuit"].InnerText);
      resultingConfig.maxStatuesPerCircuit    = int.Parse(rootElement["MaxStatuesPerCircuit"].InnerText);
      resultingConfig.maxPumpsPerCircuit      = int.Parse(rootElement["MaxPumpsPerCircuit"].InnerText);
      resultingConfig.maxCircuitLength        = int.Parse(rootElement["MaxCircuitLength"].InnerText);
      resultingConfig.boulderWirePermission   = rootElement["BoulderWirePermission"].InnerText;
      resultingConfig.blockActivatorConfig    = BlockActivatorConfig.FromXmlElement(rootElement["BlockActivatorConfig"]);

      XmlElement pumpConfigsNode = rootElement["PumpConfigs"];
      foreach (XmlNode pumpConfigNode in pumpConfigsNode.ChildNodes) {
        XmlElement pumpConfigElement = (pumpConfigNode as XmlElement);
        if (pumpConfigElement == null)
          continue;

        ComponentConfigProfile componentConfigProfile = (ComponentConfigProfile)Enum.Parse(typeof(ComponentConfigProfile), pumpConfigElement.Attributes["Profile"].Value);
        resultingConfig.pumpConfigs.Add(componentConfigProfile, PumpConfig.FromXmlElement(pumpConfigElement));
      }

      XmlElement dartTrapConfigsNode = rootElement["DartTrapConfigs"];
      foreach (XmlNode dartTrapConfigNode in dartTrapConfigsNode.ChildNodes) {
        XmlElement dartTrapConfigElement = (dartTrapConfigNode as XmlElement);
        if (dartTrapConfigElement == null)
          continue;

        ComponentConfigProfile componentConfigProfile = (ComponentConfigProfile)Enum.Parse(typeof(ComponentConfigProfile), dartTrapConfigElement.Attributes["Profile"].Value);
        resultingConfig.dartTrapConfigs.Add(componentConfigProfile, DartTrapConfig.FromXmlElement(dartTrapConfigElement));
      }

      XmlElement statueConfigsNode = rootElement["StatueConfigs"];
      foreach (XmlNode statueConfigNode in statueConfigsNode.ChildNodes) {
        XmlElement statueConfigElement = (statueConfigNode as XmlElement);
        if (statueConfigElement == null)
          continue;

        StatueType statueType = (StatueType)Enum.Parse(typeof(StatueType), statueConfigElement.Attributes["StatueType"].Value);
        resultingConfig.statueConfigs.Add(statueType, StatueConfig.FromXmlElement(statueConfigElement));
      }

      return resultingConfig;
    }
    #endregion
  }
}
