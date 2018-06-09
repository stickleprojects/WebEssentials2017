using System;
using System.Runtime.InteropServices;
using System.Threading;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

using VSIXBundler.Core;
using VSIXBundler.Core.Helpers;
using VSIXBundler.Core.Installer;

using Tasks = System.Threading.Tasks;

namespace WebEssentials
{
    [Guid(PackageGuids.guidVSPackageString)]
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", Vsix.Version, IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    public sealed class ExperimantalFeaturesPackage : AsyncPackage
    {
        protected override async Tasks.Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await ShowModalCommand.InitializeAsync(this, getLogger());

            // Load installer package
            var shell = await GetServiceAsync(typeof(SVsShell)) as IVsShell;
            var guid = new Guid(InstallerPackage._packageGuid);
            ErrorHandler.ThrowOnFailure(shell.LoadPackage(guid, out IVsPackage ppPackage));
        }

        private ILogger getLogger()
        {
            var settings = new SettingsFactory().Create();
            return new LoggerFactory().Create(settings);
        }
    }

    internal class LoggerFactory
    {
        public ILogger Create(ISettings settings)
        {
            return new Logger(settings);
        }
    }

    internal class SettingsFactory
    {
        public string VSIXName = "Bundler";
        public Uri LiveFeedUrl = new Uri("http://www.bbc.co.cuk");
        public string RegistrySubKey => VSIXName;

        public ISettings Create()
        {
            var rp = new ResourceProviderFactory().Create();
            return new Settings(VSIXName, LiveFeedUrl.ToString(), RegistrySubKey, rp);
        }
    }

    [Guid(_packageGuid)]
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.ShellInitialized_string, PackageAutoLoadFlags.BackgroundLoad)]
    public sealed class InstallerPackage : AsyncPackage
    {
        public const string _packageGuid = "4f2f2873-be87-4716-a4d5-3f3f047942c4";

        protected override async Tasks.Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            var settings = new SettingsFactory().Create();
            InstallerService.Initialize(this, settings, getLogger(settings));
            await InstallerService.RunAsync().ConfigureAwait(false);
        }

        private ILogger getLogger(ISettings settings)
        {
            return new LoggerFactory().Create(settings);
        }
    }
}