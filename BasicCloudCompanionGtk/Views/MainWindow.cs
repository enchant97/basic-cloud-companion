using Gtk;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace BasicCloudCompanionGtk.Views
{
    class MainWindow : Window
    {
        private readonly Button loginBnt;
        private readonly Button sharesBnt;
        private readonly Button createDirBnt;
        private readonly Grid navigationGrid;

        private string Username;
        private string CurrPath;
        private readonly BasicCloudApi.Communication cloudApi;
        private BasicCloudApi.Types.DirectoryRoots DirRoots;

        public MainWindow() : base("Basic Cloud Companion - Gtk Edition")
        {
            // TODO: load this from config
            cloudApi = new("http://127.0.0.1:8000");

            SetDefaultSize(250, 200);
            SetPosition(WindowPosition.Center);
            DeleteEvent += OnDelete;

            VBox mainBox = new();
            Add(mainBox);

            Label titleLabel = new("Basic Cloud Companion");
            mainBox.PackStart(titleLabel, false, false, 10);

            HBox controlBox = new();
            mainBox.PackStart(controlBox, false, false, 0);
            loginBnt = new("Login");
            loginBnt.Clicked += LoginOnClick;
            controlBox.PackStart(loginBnt, false, false, 0);
            sharesBnt = new("Shares");
            controlBox.PackStart(sharesBnt, false, false, 0);
            createDirBnt = new("Create Directory");
            controlBox.PackStart(createDirBnt, false, false, 0);

            navigationGrid = new();
            navigationGrid.ColumnHomogeneous = true;
            mainBox.PackStart(navigationGrid, true, true, 10);

            Label statusLabel = new();
            mainBox.PackEnd(statusLabel, false, false, 0);

            LoggedOut();
            ShowAll();
        }
        private void LoggedOut()
        {
            loginBnt.Sensitive = true;
            sharesBnt.Sensitive = false;
            createDirBnt.Sensitive = false;
        }
        private void JustLoggedIn()
        {
            loginBnt.Sensitive = false;
            // TODO: do other stuff?
            _ = LoadRoots();
        }
        private void OnDelete(object obj, DeleteEventArgs args)
        {
            Application.Quit();
        }
        /// <summary>
        /// Handle known HTTP errors
        /// </summary>
        /// <param name="exception">the HTTP exception</param>
        /// <returns>returns if the exception was handled</returns>
        private bool HandleHttpExceptions(HttpRequestException exception)
        {
            if (exception.InnerException is System.Net.Sockets.SocketException)
            {
                Helpers.Alerts.ShowError(this, "No server connection");
            }
            else if (exception.StatusCode == HttpStatusCode.Unauthorized)
            {
                Helpers.Alerts.ShowError(this, "Not authorised");
            }
            else { return false;  }
            return true;
        }
        #region Navigation Control
        /// <summary>
        /// Add a row to the navigation grid
        /// </summary>
        /// <param name="path">the path/filename part to use</param>
        /// <param name="isDir">whether path is a directory</param>
        /// <param name="isEditible">whether path is editible</param>
        /// <param name="isDownloadable">whether path is downloadable</param>
        private void AddNavigationRow(string path, bool isDir, bool isEditible = true, bool isDownloadable = true)
        {
            Label nameLabel = new(path);
            Button chdirBnt = new()
            {
                Label = "Navigate",
                Sensitive = isDir
            };
            Button rmBnt = new()
            {
                Label = "Delete",
                Sensitive = isEditible
            };
            Button downloadBnt = new()
            {
                Label = "Download",
                Sensitive = isDownloadable
            };

            // TODO: implement click handlers

            navigationGrid.Attach(nameLabel, 0, navigationGrid.Children.Length, 1, 1);
            navigationGrid.AttachNextTo(chdirBnt, nameLabel, PositionType.Right, 1, 1);
            navigationGrid.AttachNextTo(rmBnt, chdirBnt, PositionType.Right, 1, 1);
            navigationGrid.AttachNextTo(downloadBnt, rmBnt, PositionType.Right, 1, 1);

            navigationGrid.ShowAll();
        }
        /// <summary>
        /// Remove all navigation rows
        /// </summary>
        private void ClearNavigation()
        {
            for (int i = navigationGrid.Children.Length - 1; i >= 0; i--)
            {
                navigationGrid.RemoveRow(i);
            }
        }
        #endregion
        private async Task LoadRoots()
        {
            try
            {
                createDirBnt.Sensitive = false;
                sharesBnt.Sensitive = false;

                DirRoots = await cloudApi.GetDirectoryRoots();

                ClearNavigation();

                AddNavigationRow(DirRoots.home, true, false);
                AddNavigationRow(DirRoots.shared, true, false);

                ClearNavigation();

                AddNavigationRow(DirRoots.home, true, false);
                AddNavigationRow(DirRoots.shared, true, false);
            }
            catch (HttpRequestException err)
            {
                if (!HandleHttpExceptions(err)) { throw; }
            }
        }
        #region Button Click Handlers
        private async void LoginOnClick(object obj, EventArgs args)
        {
            var dialog = new LoginWindow(this);
            var response = dialog.Run();
            if (response == ((int)ResponseType.Ok))
            {
                string username = dialog.Username;
                string password = dialog.Password;

                if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                {
                    try
                    {
                    // username and password was not blank
                    await cloudApi.PostLoginToken(username, password, true);
                    Username = username;
                    JustLoggedIn();
                    }
                    catch (HttpRequestException err)
                    {
                        if (!HandleHttpExceptions(err)) { throw; }
                    }
                }
                else
                {
                    // username and password was blank
                    Helpers.Alerts.ShowWarning(this, "username or password was blank");
                }
            }
            dialog.Destroy();
        }
        #endregion
    }
}
