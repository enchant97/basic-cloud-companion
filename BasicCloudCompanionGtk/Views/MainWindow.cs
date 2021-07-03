using Gtk;
using System;
using System.Linq;
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
        private readonly Button toParentDirBnt;
        private readonly Button uploadFileBnt;
        private readonly Grid navigationGrid;

        private string Username;
        private string CurrPath;
        private readonly BasicCloudApi.Communication cloudApi;
        private string[] RootPaths;
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
            sharesBnt.Clicked += NavigateToRootsOnClick;
            controlBox.PackStart(sharesBnt, false, false, 0);
            createDirBnt = new("Create Directory");
            createDirBnt.Clicked += CreateDirOnClick;
            controlBox.PackStart(createDirBnt, false, false, 0);
            uploadFileBnt = new("Upload File");
            uploadFileBnt.Clicked += UploadFileOnClick;
            controlBox.PackStart(uploadFileBnt, false, false, 0);
            toParentDirBnt = new("Back");
            toParentDirBnt.Clicked += ToParentDirOnClick;
            controlBox.PackStart(toParentDirBnt, false, false, 0);

            navigationGrid = new();
            navigationGrid.ColumnHomogeneous = true;
            mainBox.PackStart(navigationGrid, true, true, 10);

            Label statusLabel = new();
            mainBox.PackEnd(statusLabel, false, false, 0);

            LoggedOut();
            ShowAll();
        }
        #region Misc Action Handling
        private void LoggedOut()
        {
            loginBnt.Sensitive = true;

            sharesBnt.Sensitive = false;
            createDirBnt.Sensitive = false;
            uploadFileBnt.Sensitive = false;
            toParentDirBnt.Sensitive = false;
        }
        private void JustLoggedIn()
        {
            loginBnt.Sensitive = false;
            ToggleControlBoxButtons();
            _ = LoadRoots();
        }
        /// <summary>
        /// handling enabling/disabling the control box buttons
        /// </summary>
        private void ToggleControlBoxButtons()
        {
            if (string.IsNullOrEmpty(CurrPath))
            {
                // at root
                sharesBnt.Sensitive = false;
                createDirBnt.Sensitive = false;
                uploadFileBnt.Sensitive = false;
                toParentDirBnt.Sensitive = false;
            }
            else
            {
                // inside a directory
                sharesBnt.Sensitive = true;
                createDirBnt.Sensitive = true;
                uploadFileBnt.Sensitive = true;
                toParentDirBnt.Sensitive = true;
            }
        }
        private void OnDelete(object obj, DeleteEventArgs args)
        {
            Application.Quit();
        }
        #endregion
        #region Utility
        /// <summary>
        /// Handle known HTTP errors
        /// </summary>
        /// <param name="exception">the HTTP exception</param>
        /// <returns>returns if the exception was handled</returns>
        protected bool HandleHttpExceptions(HttpRequestException exception)
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
        /// <summary>
        /// allows smart joining of a path with the current path
        /// </summary>
        /// <param name="pathPart">the path to join</param>
        /// <returns>the joined path</returns>
        protected string JoinWithCurrentPath(string pathPart)
        {
            if (!string.IsNullOrEmpty(CurrPath))
            {
                return CurrPath + "/" + pathPart;
            }
            return pathPart;
        }
        /// <summary>
        /// Gets the parent directory from the CurrPath,
        /// if the current path is a root will return null.
        /// </summary>
        /// <returns>the parent directory or null</returns>
        protected string GetCurrentParentDir()
        {
            if (RootPaths.Contains(CurrPath)) { return null; }
            return BasicCloudApi.Helpers.GetParentDir(CurrPath);
        }
        #endregion
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
                Sensitive = isDir,
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

            if (isDir)
            {
                // setup dir click handlers
                chdirBnt.Clicked += delegate (object sender, EventArgs e) { ChangeDirOnClick(sender, e, path); };
                rmBnt.Clicked += delegate (object sender, EventArgs e) { DeleteDirOnClick(sender, e, path); };
                downloadBnt.Clicked += delegate (object sender, EventArgs e) { DownloadDirOnClick(sender, e, path); };
            }
            else
            {
                // setup file click handlers
                rmBnt.Clicked += delegate (object sender, EventArgs e) { DeleteFileOnClick(sender, e, path); };
                downloadBnt.Clicked += delegate (object sender, EventArgs e) { DownloadFileOnClick(sender, e, path); };
            }

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
        /// <summary>
        /// Add directory content to the navigation grid
        /// </summary>
        /// <param name="contents"></param>
        private void AddDirContentToNavigation(BasicCloudApi.Types.Content[] contents)
        {
            foreach (var dirDontent in contents)
            {
                AddNavigationRow(
                    dirDontent.name.Replace("\\", "/"),
                    dirDontent.meta.is_directory
                );
            }
        }
        #endregion
        #region BasicCloudApi Usage
        /// <summary>
        /// Load the root directories (home and shared) and add to screen
        /// </summary>
        private async Task LoadRoots()
        {
            try
            {
                ClearNavigation();

                var directoryRoots = await cloudApi.GetDirectoryRoots();

                RootPaths = new string[]
                {
                    directoryRoots.home,
                    directoryRoots.shared
                };

                AddNavigationRow(RootPaths[0], true, false);
                AddNavigationRow(RootPaths[1], true, false);
            }
            catch (HttpRequestException err)
            {
                if (!HandleHttpExceptions(err)) { throw; }
            }
        }
        /// <summary>
        /// load the current directory path's contents
        /// </summary>
        private async Task LoadCurrentDirContents()
        {
            try
            {
                ClearNavigation();
                var directoryContents = await cloudApi.PostDirectoryContents(CurrPath);
                AddDirContentToNavigation(directoryContents);
            }
            catch (HttpRequestException err)
            {
                if (!HandleHttpExceptions(err)) { throw; }
            }
        }
        #endregion
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
        private async void NavigateToRootsOnClick(object obj, EventArgs args)
        {
            System.Diagnostics.Debug.WriteLine("navigate to roots button clicked");
            CurrPath = null;
            ToggleControlBoxButtons();
            await LoadRoots();
        }
        private async void ToParentDirOnClick(object obj, EventArgs args)
        {
            System.Diagnostics.Debug.WriteLine("navigate to parent directory button clicked");
            CurrPath = GetCurrentParentDir();
            ToggleControlBoxButtons();
            if (string.IsNullOrEmpty(CurrPath)) { await LoadRoots(); }
            else { await LoadCurrentDirContents(); }
        }
        private async void UploadFileOnClick(object obj, EventArgs args)
        {
            System.Diagnostics.Debug.WriteLine("upload file button clicked");
            try
            {
                Helpers.InputDialogs.ResponseAndString response = Helpers.InputDialogs.ShowFileOpen(
                    this,
                    "Select A File To Upload"
                );
                if (response.ResponseType == ResponseType.Ok)
                {
                    string fileName = System.IO.Path.GetFileName(response.Content);
                    var fileContent = await BasicCloudApi.Helpers.ReadFileByteContent(response.Content);
                    await cloudApi.PostUploadFile(fileContent, fileName, CurrPath);
                }
            }
            catch (HttpRequestException err)
            {
                if (!HandleHttpExceptions(err)) { throw; }
            }
        }
        private async void ChangeDirOnClick(object obj, EventArgs args, string path)
        {
            System.Diagnostics.Debug.WriteLine("change directory button clicked");
            CurrPath = JoinWithCurrentPath(path);
            ToggleControlBoxButtons();
            await LoadCurrentDirContents();
        }
        private async void CreateDirOnClick(object obj, EventArgs args)
        {
            System.Diagnostics.Debug.WriteLine("make directory button clicked");
            InputWindow dialog = new(
                this,
                "Create Directory",
                "Please Enter New Directory Name",
                "Input Here..."
            );
            var response = dialog.Run();
            if (response == (int)ResponseType.Ok)
            {
                string newDirName = dialog.Input;
                try
                {
                    await cloudApi.PostCreateDirectory(CurrPath, newDirName);
                }
                catch (HttpRequestException err)
                {
                    if (!HandleHttpExceptions(err)) { throw; }
                }
            }
            dialog.Destroy();
        }
        private async void DeleteDirOnClick(object obj, EventArgs args, string path)
        {
            System.Diagnostics.Debug.WriteLine("delete directory button clicked");
            var response = Helpers.Alerts.ShowQuestion(this, "are you sure you want to delete the directory?");
            if (response == ResponseType.Yes)
            {
                var fullPath = JoinWithCurrentPath(path);
                try
                {
                    await cloudApi.DeleteDirectory(fullPath);
                }
                catch (HttpRequestException err)
                {
                    if (!HandleHttpExceptions(err)) { throw; }
                }
            }
        }
        private async void DownloadDirOnClick(object obj, EventArgs args, string folderName)
        {
            System.Diagnostics.Debug.WriteLine("download directory button clicked");
            try
            {
                string downloadPath = JoinWithCurrentPath(folderName);
                Helpers.InputDialogs.ResponseAndString response = Helpers.InputDialogs.ShowSelectFolder(
                    this,
                    "Select Folder To Save Zip File"
                );
                if (response.ResponseType == ResponseType.Ok)
                {
                    var httpContent = await cloudApi.DownloadDirectoryAsZip(downloadPath);
                    string savePath = response.Content + "/" + folderName.Replace("/", "_") + ".zip";
                    await BasicCloudApi.Helpers.WriteHttpContentToFile(savePath, httpContent);
                }
            }
            catch (HttpRequestException err)
            {
                if (!HandleHttpExceptions(err)) { throw; }
            }
        }
        private async void DeleteFileOnClick(object obj, EventArgs args, string path)
        {
            System.Diagnostics.Debug.WriteLine("delete file button clicked");
            var response = Helpers.Alerts.ShowQuestion(this, "are you sure you want to delete the file?");
            if (response == ResponseType.Yes)
            {
                var fullPath = JoinWithCurrentPath(path);
                try
                {
                    await cloudApi.DeleteFile(fullPath);
                }
                catch (HttpRequestException err)
                {
                    if (!HandleHttpExceptions(err)) { throw; }
                }
            }
        }
        private async void DownloadFileOnClick(object obj, EventArgs args, string filename)
        {
            System.Diagnostics.Debug.WriteLine("download file button clicked");
            try
            {
                string downloadPath = JoinWithCurrentPath(filename);
                Helpers.InputDialogs.ResponseAndString response = Helpers.InputDialogs.ShowSelectFolder(
                    this,
                    "Select Folder To Save File"
                );
                if (response.ResponseType == ResponseType.Ok)
                {
                    var httpContent = await cloudApi.DownloadFile(downloadPath);
                    string savePath = response.Content + "/" + filename;
                    await BasicCloudApi.Helpers.WriteHttpContentToFile(savePath, httpContent);
                }
            }
            catch (HttpRequestException err)
            {
                if (!HandleHttpExceptions(err)) { throw; }
            }
        }
        #endregion
    }
}
