﻿using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.IO;
using ColossalFramework.Plugins;
using ColossalFramework.UI;
using ICities;
using Stats.Config;
using Stats.Localization;
using Stats.Ui;
using System.IO;
using UnityEngine;

namespace Stats
{
    public class Mod : LoadingExtensionBase, IUserMod
    {
        private readonly string _fallbackLanguageTwoLetterCode = "en";

        private LanguageResourceService<LanguageResourceDto> _languageResourceService;
        private LanguageResource _languageResource;

        private ConfigurationService<ConfigurationDto> _configurationService;
        private Configuration _configuration;
        private ConfigurationPanel _configurationPanel;

        private MainPanel _mainPanel;

        public string SystemName => "Stats";
        public string Name => "Stats";
        public string Description => "Adds a configurable panel to display all vital city stats at a glance.";
        public string Version => "1.3.2";
        public string WorkshopId => "1410077595";

        public void OnEnabled()
        {
            InitializeDependencies();

            if (LoadingManager.exists && LoadingManager.instance.m_loadingComplete)
            {
                InitializeMainPanel();
            }
        }

        public void OnDisabled()
        {
            if (LoadingManager.exists && LoadingManager.instance.m_loadingComplete)
            {
                DestroyMainPanel();
            }

            DestroyDependencies();
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            if (!(mode == LoadMode.LoadGame || mode == LoadMode.NewGame || mode == LoadMode.NewGameFromScenario))
            {
                return;
            }

            InitializeMainPanel();
        }

        public override void OnLevelUnloading()
        {
            DestroyMainPanel();
        }

        private void InitializeDependencies()
        {
            var configurationFileFullName = Path.Combine(DataLocation.localApplicationData, SystemName + ".xml");
            _configurationService = new ConfigurationService<ConfigurationDto>(configurationFileFullName);
            _languageResourceService = new LanguageResourceService<LanguageResourceDto>(
                SystemName,
                WorkshopId,
                PluginManager.instance
            );

            _configuration = File.Exists(_configurationService.ConfigurationFileFullName)
                ? new Configuration(_configurationService, _configurationService.Load())
                : new Configuration(_configurationService, new ConfigurationDto());

            var playerLanguage = new SavedString(Settings.localeID, Settings.gameSettingsFile, DefaultSettings.localeID);
            LocaleManager.defaultLanguage = playerLanguage; //necessary because LocaleManager.Constructor will use that value lol.
            LocaleManager.Ensure();
            _languageResource = LanguageResource.Create(_languageResourceService, playerLanguage, _fallbackLanguageTwoLetterCode);

            LocaleManager.eventUIComponentLocaleChanged += LocaleManager_eventUIComponentLocaleChanged;
        }

        private void DestroyDependencies()
        {
            LocaleManager.eventUIComponentLocaleChanged -= LocaleManager_eventUIComponentLocaleChanged;

            _configurationService = null;
            _languageResourceService = null;

            _configuration = null;
            _languageResource = null;

            _configurationPanel = null;
        }

        private void LocaleManager_eventUIComponentLocaleChanged()
        {
            var languageTwoLetterCode = LocaleManager.instance.language;
            _languageResource.LoadLanguage(languageTwoLetterCode);
            _mainPanel?.UpdateLocalization();
        }

        private void InitializeMainPanel()
        {
            var gameEngineService = new GameEngineService(
                DistrictManager.instance,
                BuildingManager.instance,
                EconomyManager.instance,
                ImmaterialResourceManager.instance,
                CitizenManager.instance,
                VehicleManager.instance
            );

            _mainPanel = UIView.GetAView().AddUIComponent(typeof(MainPanel)) as MainPanel;
            _mainPanel.Initialize(SystemName, _configuration, _languageResource, gameEngineService, InfoManager.instance);
            if (_configurationPanel != null)
            {
                _configurationPanel.MainPanel = _mainPanel;
            }
        }

        private void DestroyMainPanel()
        {
            if (_configurationPanel != null)
            {
                _configurationPanel.MainPanel = null;
            }
            GameObject.Destroy(_mainPanel.gameObject);
            _mainPanel = null;
        }

        public void OnSettingsUI(UIHelperBase helper)
        {
            var modFullTitle = new ModFullTitle(Name, Version);

            _configurationPanel = new ConfigurationPanel(
                helper,
                modFullTitle,
                _configuration,
                _languageResource
            );

            _configurationPanel.Initialize();
            if (_mainPanel != null)
            {
                _configurationPanel.MainPanel = _mainPanel;
            }
        }

        //TODO: postvans in use - to be tested, add translations
        //TODO: posttrucks in use - to be tested, add translations
        //TODO: disaster response vehicles - to be tested, add translations
        //TODO: disaster response helicopters - to be tested, add translation
        //TODO: mail itself
        //TODO: evacuation buses in use
        //TODO: rework garbage: split unpicked garbage from incinerator and landfill reserves
        //TODO: rework healthcare: split sick citizen from hospital beds
        //TODO: split item configuration from main panel 
        //TODO: color picker
        //TODO: add happiness values
        //TODO: maybe natural resources used
        //TODO: move itempanel logic out of mainpanel
        //TODO: unselect main menu if another service is selected but does not fit the click on the item
        //TODO: performance (for example GetBudget can be done once)
        //TODO: performance create only the ItemPanels that are enabled
        //TODO: refactoring
        //TODO: icons
        //TODO: values per building type
        //TODO: values per district
    }
}
