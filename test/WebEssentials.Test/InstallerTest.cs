using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.VisualStudio.ExtensionManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using VSIXBundler.Core;
using VSIXBundler.Core.Installer;

namespace WebEssentials.Test
{
    /// <summary>
    /// Summary description for InstallerTest
    /// </summary>
    [TestClass,]
    public class InstallerTest
    {
        private Installer _installer;
        private string _cachePath;
        private ISettings _settings;

        [TestInitialize]
        public void TestSetup()
        {
            _cachePath = Path.Combine(Path.GetTempPath(), "cache.json");

            var logFile = Path.Combine(Path.GetTempPath(), "logfile.json");
            _settings = new SettingsFactory().Create();
            _settings.LogFilePath = logFile;
            var logger = new LoggerFactory().Create(_settings);
            var store = new DataStore(new StaticRegistryKey(), _settings);
            var feed = new LiveFeed(Constants.LiveFeedUrl, _cachePath, logger);

            _installer = new Installer(feed, store, _settings, logger);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            File.Delete(_cachePath);
        }

        [TestMethod]
        public async Task CheckForUpdatesNoCacheAsync()
        {
            File.Delete(_cachePath);

            var v = new Mock<Version>();
            var mgr = new Mock<IVsExtensionManager>();
            bool hasUpdates = await _installer.CheckForUpdatesAsync(v.Object, mgr.Object);

            Assert.IsTrue(hasUpdates);
        }

        [TestMethod]
        public async Task CheckForUpdatesValidCacheAsync()
        {
            File.WriteAllText(_cachePath, "{}");
            var v = new Mock<Version>();
            var mgr = new Mock<IVsExtensionManager>();
            bool hasUpdates = await _installer.CheckForUpdatesAsync(v.Object, mgr.Object);

            Assert.IsFalse(hasUpdates);
        }

        [TestMethod]
        public async Task GetExtensionsMarkedForDeletionAsync()
        {
            string content = @"{
            ""Add New File"": {
                ""id"": ""2E78AA18-E864-4FBB-B8C8-6186FC865DB3"",
                ""minVersion"": ""15.0"",
                ""maxVersion"": ""15.2""
                }
            }";

            File.WriteAllText(_cachePath, content);
            await _installer.LiveFeed.ParseAsync();

            IEnumerable<ExtensionEntry> tooLow = _installer.GetExtensionsMarkedForDeletion(new Version(14, 0));
            Assert.AreEqual(1, tooLow.Count());
            Assert.AreEqual("2E78AA18-E864-4FBB-B8C8-6186FC865DB3", tooLow.ElementAt(0).Id);

            IEnumerable<ExtensionEntry> tooHigh = _installer.GetExtensionsMarkedForDeletion(new Version(16, 0));
            Assert.AreEqual(1, tooHigh.Count());
            Assert.AreEqual("2E78AA18-E864-4FBB-B8C8-6186FC865DB3", tooHigh.ElementAt(0).Id);

            IEnumerable<ExtensionEntry> lowerBounce = _installer.GetExtensionsMarkedForDeletion(new Version(15, 0));
            Assert.IsFalse(lowerBounce.Any());

            IEnumerable<ExtensionEntry> upperBounce = _installer.GetExtensionsMarkedForDeletion(new Version(15, 2));
            Assert.IsFalse(upperBounce.Any());

            IEnumerable<ExtensionEntry> middle = _installer.GetExtensionsMarkedForDeletion(new Version(15, 1));
            Assert.IsFalse(middle.Any());
        }
    }
}