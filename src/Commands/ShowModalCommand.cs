using System;
using System.ComponentModel.Design;
using System.Windows.Interop;

using EnvDTE;

using EnvDTE80;

using Microsoft.VisualStudio.Shell;

using VSIXBundler.Core.Helpers;

using WebEssentials.Commands;

namespace WebEssentials
{
    internal sealed class ShowModalCommand
    {
        private readonly AsyncPackage _package;
        private readonly ILogger _logger;

        private ShowModalCommand(AsyncPackage package, OleMenuCommandService commandService, ILogger logger)
        {
            _package = package;
            _logger = logger;

            var menuCommandID = new CommandID(PackageGuids.guidVSPackageCmdSet, PackageIds.ResetExtensions);
            var menuItem = new MenuCommand(ResetAsync, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        public static ShowModalCommand Instance
        {
            get;
            private set;
        }

        public async static System.Threading.Tasks.Task InitializeAsync(AsyncPackage package, ILogger logger)
        {
            var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new ShowModalCommand(package, commandService, logger);
        }

        private async void ResetAsync(object sender, EventArgs e)
        {
            var dte = await _package.GetServiceAsync(typeof(DTE)) as DTE2;
            var dialog = new LogWindow(_logger);

            var hwnd = new IntPtr(dte.MainWindow.HWnd);
            var window = (System.Windows.Window)HwndSource.FromHwnd(hwnd).RootVisual;
            dialog.Owner = window;
            dialog.ShowDialog();
        }
    }
}