using System;
using System.IO;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using VSIXBundler.Core;
using VSIXBundler.Core.Helpers;
using VSIXBundler.Core.Installer;

namespace WebEssentials.Test
{
    [TestClass]
    public class LiveFeedTest
    {
        private string _localPath;
        private ILogger _logger;
        private Settings _settings;

        [TestInitialize]
        public void Setup()
        {
            _localPath = Path.Combine(Path.GetTempPath(), "feed.json");
            var resourceProvider = new ResourceProviderFactory().Create();
            _settings = new Settings(resourceProvider) { LiveFeedCachePath = _localPath };

            _logger = new LoggerFactory().Create(_settings);
            File.Delete(_localPath);
        }

        [TestMethod]
        public async Task UpdateAsync()
        {
            var file = new FileInfo("..\\..\\artifacts\\feed.json");
            var feed = new LiveFeed(file.FullName, _localPath, _logger);

            await feed.UpdateAsync();
            File.Delete(_localPath);

            Assert.IsTrue(feed.Extensions.Count == 2);
            Assert.IsTrue(feed.Extensions[0].Name == "Add New File");
            Assert.IsTrue(feed.Extensions[0].Id == "2E78AA18-E864-4FBB-B8C8-6186FC865DB3");
            Assert.IsTrue(feed.Extensions[1].MaxVersion == new Version("16.0"));
        }

        [TestMethod]
        public async Task UpdateInvalidJSONAsync()
        {
            var feed = new LiveFeed("http://example.com", _localPath, _logger);

            await feed.UpdateAsync();

            Assert.IsTrue(feed.Extensions.Count == 0);
            Assert.IsFalse(File.Exists(_localPath));
        }

        [TestMethod]
        public async Task Update404Async()
        {
            var feed = new LiveFeed("http://asdlfkhasdflijsdflisjdfjoi23498734so08s0d8f.dk", _localPath, _logger);

            await feed.UpdateAsync();

            Assert.IsTrue(feed.Extensions.Count == 0);
            Assert.IsFalse(File.Exists(_localPath));
        }

        [TestMethod]
        public async Task ParsingAsync()
        {
            var feed = new LiveFeed("", _localPath, _logger);

            string content = @"{
            ""Add New File"": {
                ""id"": ""2E78AA18-E864-4FBB-B8C8-6186FC865DB3"",
                ""minVersion"": ""15.0""
                }
            }";

            File.WriteAllText(_localPath, content);

            await feed.ParseAsync();
            File.Delete(_localPath);

            Assert.IsTrue(feed.Extensions.Count == 1);
            Assert.IsTrue(feed.Extensions[0].Name == "Add New File");
            Assert.IsTrue(feed.Extensions[0].Id == "2E78AA18-E864-4FBB-B8C8-6186FC865DB3");
        }

        [TestMethod]
        public async Task ParsingInvalidJsonAsync()
        {
            var feed = new LiveFeed("", _localPath, _logger);

            string content = "invalid json";

            File.WriteAllText(_localPath, content);

            await feed.ParseAsync();
            File.Delete(_localPath);

            Assert.IsTrue(feed.Extensions.Count == 0);
        }
    }
}