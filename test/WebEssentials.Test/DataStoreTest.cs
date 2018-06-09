using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json;

using VSIXBundler.Core;
using VSIXBundler.Core.Installer;

namespace WebEssentials.Test
{
    [TestClass]
    public class DataStoreTest
    {
        private string _logFile;
        private ExtensionEntry _entry;
        private IRegistryKey _registry;
        private ISettings _settings;

        [TestInitialize]
        public void Setup()
        {
            _logFile = Path.Combine(Path.GetTempPath(), "logfile.json");
            _settings = new Settings("", "", "", new ResourceProviderFactory().Create());
            _settings.LogFilePath = _logFile;

            _entry = new ExtensionEntry { Id = "id" };
            _registry = new StaticRegistryKey();

            Cleanup();
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (File.Exists(_logFile)) File.Delete(_logFile);
        }

        [TestMethod]
        public void ExtensionInstalledNoLogFile()
        {
            var store = new DataStore(_registry, _settings);
            store.MarkInstalled(_entry);

            Assert.AreEqual(1, store.Log.Count);
            Assert.IsFalse(File.Exists(_logFile));
            Assert.AreEqual(_entry.Id, store.Log[0].Id);
            Assert.AreEqual("Installed", store.Log[0].Action);
            Assert.IsTrue(store.HasBeenInstalled(_entry.Id));

            store.Save();
            Assert.IsTrue(File.Exists(_logFile));
        }

        [TestMethod]
        public void ExtensionUninstalledNoLogFile()
        {
            var store = new DataStore(_registry, _settings);
            store.MarkUninstalled(_entry);
            store.Save();

            Assert.AreEqual(1, store.Log.Count);
            Assert.AreEqual(_entry.Id, store.Log[0].Id);
            Assert.AreEqual("Uninstalled", store.Log[0].Action);
            Assert.AreEqual(_entry.Id, _registry.GetValue("disable"));
        }

        [TestMethod]
        public void LogFileExist()
        {
            var msg = new[] { new DataStore.LogMessage(_entry, "Installed") };

            var json = JsonConvert.SerializeObject(msg);
            File.WriteAllText(_logFile, json);

            var store = new DataStore(_registry, _settings);

            Assert.IsTrue(store.HasBeenInstalled(_entry.Id));
            Assert.AreEqual(1, store.Log.Count);
            Assert.AreEqual(_entry.Id, store.Log[0].Id);
        }

        [TestMethod]
        public void Reset()
        {
            var msg = new[] { new DataStore.LogMessage(_entry, "Installed") };

            var json = JsonConvert.SerializeObject(msg);
            File.WriteAllText(_logFile, json);

            var store = new DataStore(_registry, _settings);

            Assert.AreEqual(1, store.Log.Count);

            bool success = store.Reset();

            Assert.IsTrue(success);
            Assert.AreEqual(0, store.Log.Count);
            Assert.IsFalse(File.Exists(_logFile));
        }
    }
}