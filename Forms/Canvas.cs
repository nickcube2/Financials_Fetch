using FinancialBoardsFetch.Modules;

using Nager.Date;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Windows.Forms;
using System.Xml;

using static System.Windows.Forms.ListView;

namespace FinancialBoardsFetch.Forms
{
    enum Tab
    {
        Settings,
        Commodities,
        Currencies,
        JSEIndice,
        Top40
    }

    enum FetchStatus
    {
        Success,
        Loading,
        Caution,
        Error
    }

    public partial class Canvas : Form
    {
        private const bool LOG_FETCH_YES = true;
        //private const bool LOG_FETCH_NO = false;

        BackgroundWorker BW_Commodities;
        BackgroundWorker BW_Currencies;
        BackgroundWorker BW_JSEIndice;
        BackgroundWorker BW_Top40;

        string RequestCommodities, RequestCurrencies, RequestJSEIndice, RequestTop40;

        Queue<MethodInvoker> UpdateQueue;

        Timer NextUpdateTimer;
        readonly Stopwatch Stopwatch = new();

        int Interval;

        DateTime NextUpdateAt;

        public bool AutoFetch = false;
        readonly string DL = Environment.NewLine + Environment.NewLine;
        readonly Mouse Mouse = new();

        public Canvas(MainForm mainForm)
        {
            InitializeComponent();

            MdiParent = mainForm;

            InitCanvas();
        }

        void InitCanvas()
        {
            InitBackgroundWorkers();

            InitSettingsOptions();

            InitWarnings();

            InitRenamingList();

            InitTimer();

            statusGrid.ContextMenuStrip = ((MainForm)MdiParent).cmsStatusLogs;
            fetchDataGrid.ContextMenuStrip = ((MainForm)MdiParent).cmsDataLogs;
        }

        private void InitRenamingList()
        {
            string resource_data = GetResourceData();
            List<string> companies = resource_data.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList();

            companies = companies.Select(co => co.Replace(",", " > ")).ToList();

            ListViewItemCollection items = new(lvNames);
            items.AddRange(companies.Select(company => new ListViewItem(company)).ToArray());
        }

        private static string GetResourceData() => Properties.Resources.All_Codes;

        void CheckInternetConnection()
        {
            SetFetchStatus(Tab.Settings, FetchStatus.Loading, "Checking internet...", LOG_FETCH_YES);
            string url = "google.com";
            if (CheckConnection.CanConnect(url))
                SetFetchStatus(Tab.Settings, FetchStatus.Success, $"Internet connection established.", LOG_FETCH_YES);
            else
                SetFetchStatus(Tab.Settings, FetchStatus.Error, $"Could not establish connection to internet.", LOG_FETCH_YES);
        }

        private void InitTimer()
        {
            NextUpdateTimer = new System.Windows.Forms.Timer()
            {
                Interval = 1000,
            };
            NextUpdateTimer.Tick += Timer_Tick;
        }

        private void LoadLastSaves()
        {
            PopulateCommoditiesGrid();
            PopulateCurrenciesGrid();
            PopulateJSEIndiceGrid();
            PopulateTop40Grid();
        }

        private void InitSettingsOptions()
        {
            txtSaveFolderPath.Text = Properties.Settings.Default.SaveFolderPath;

            cbUpdateOnStart.Checked = Properties.Settings.Default.UpdateOnStart;
            cbStartAutoUpdateOnUpdate.Checked = Properties.Settings.Default.StartAutoUpdateOnStart;
            cbStartMinimized.Checked = Properties.Settings.Default.StartMinimized;
            cbMinimizeToTray.Checked = Properties.Settings.Default.MinimizeToTray;

            cbSwitchToActiveFetchTab.Checked = Properties.Settings.Default.SwitchToActiveFetchTab;
            cbSwitchBackToSettingsTab.Checked = Properties.Settings.Default.SwitchBackToSettingsTab;
            cbBringToForefront.Checked = true; // Properties.Settings.Default.BringToForefront;

            dtpJSEUpdateStart.Value = DateTime.Parse(Properties.Settings.Default.JSEUpdateStart);
            dtpJSEUpdateEnd.Value = DateTime.Parse(Properties.Settings.Default.JSEUpdateEnd);

            cbSimulateTime.Checked = Properties.Settings.Default.SimulateTime;
            Properties.Settings.Default.LastSimulatedTime = Properties.Settings.Default.LastSimulatedTime == DateTime.MinValue ? DateTime.Now : Properties.Settings.Default.LastSimulatedTime;
            dtpSimTime.Value = Properties.Settings.Default.SimulateTime ? Properties.Settings.Default.LastSimulatedTime : DateTime.Now;

#if DEBUG
            grpTimeSimulator.Enabled = true;
#else
            grpTimeSimulator.Enabled = false;
#endif

            cbSaveLogs.Checked = Properties.Settings.Default.SaveLogs;

            cbUpdateCommodities.Checked = Properties.Settings.Default.UpdateCommodities;
            cbUpdateCurrencies.Checked = Properties.Settings.Default.UpdateCurrencies;
            cbUpdateJSEIndice.Checked = Properties.Settings.Default.UpdateJSEIndice;
            cbUpdateTop40.Checked = Properties.Settings.Default.UpdateTop40;

            rbCommoditiesUseNewFeed.Checked = Properties.Settings.Default.UseNewCommodities;
            rbCommoditiesUseOldFeed.Checked = !Properties.Settings.Default.UseNewCommodities;
            rbCurrenciesUseNewFeed.Checked = Properties.Settings.Default.UseNewCurrencies;
            rbCurrenciesUseOldFeed.Checked = !Properties.Settings.Default.UseNewCurrencies;
            rbJSEIndiceUseNewFeed.Checked = Properties.Settings.Default.UseNewJSEIndice;
            rbJSEIndiceUseOldFeed.Checked = !Properties.Settings.Default.UseNewJSEIndice;
            rbTop40UseNewFeed.Checked = Properties.Settings.Default.UseNewTop40;
            rbTop40UseOldFeed.Checked = !Properties.Settings.Default.UseNewTop40;

            nudInterval.Value = Properties.Settings.Default.NudInterval;
            cmbInterval.SelectedIndex = Properties.Settings.Default.IntervalPeriodIndex;

            if (!(Properties.Settings.Default.LastUpdateAt.Year == 0001))
                lblLastUpdateAt.Text = $"Last Update At: {Properties.Settings.Default.LastUpdateAt:HH: mm: ss dddd, dd MMMM yyyy}";
            else
                lblLastUpdateAt.Text = "Last Update At: ...";

            cbLogEvents.Checked = Properties.Settings.Default.LogEvents;
        }

        void InitWarnings()
        {
            lblFetchStatusGeneral.Text = lblFetchStatusCommodities.Text = lblFetchStatusCurrencies.Text = lblFetchStatusJSEIndice.Text = lblFetchStatusTop40.Text = "";
        }

        private void InitBackgroundWorkers()
        {
            BW_Commodities = new BackgroundWorker
            {
                WorkerSupportsCancellation = true,
                WorkerReportsProgress = true
            };
            BW_Commodities.DoWork += BW_Commodities_DoWork;
            BW_Commodities.RunWorkerCompleted += BW_Commodities_RunWorkerCompleted;

            BW_Currencies = new BackgroundWorker
            {
                WorkerSupportsCancellation = true
            };
            BW_Currencies.DoWork += BW_Currencies_DoWork;
            BW_Currencies.RunWorkerCompleted += BW_Currencies_RunWorkerCompleted;

            BW_JSEIndice = new BackgroundWorker
            {
                WorkerSupportsCancellation = true
            };
            BW_JSEIndice.DoWork += BW_JSEIndices_DoWork;
            BW_JSEIndice.RunWorkerCompleted += BW_JSEIndices_RunWorkerCompleted;

            BW_Top40 = new BackgroundWorker
            {
                WorkerSupportsCancellation = true
            };
            BW_Top40.DoWork += BW_Top40_DoWork;
            BW_Top40.RunWorkerCompleted += BW_Top40_RunWorkerCompleted;

            UpdateQueue = new Queue<MethodInvoker>();
        }

        private void BW_Commodities_DoWork(object sender, DoWorkEventArgs e)
        {
            Stopwatch.Restart();
            DoCommoditiesFetch();
        }

        private void BW_Currencies_DoWork(object sender, DoWorkEventArgs e)
        {
            Stopwatch.Restart();
            DoCurrenciesFetch();
        }

        private void BW_JSEIndices_DoWork(object sender, DoWorkEventArgs e)
        {
            Stopwatch.Restart();
            DoJSEIndiceFetch();
        }

        private void BW_Top40_DoWork(object sender, DoWorkEventArgs e)
        {
            Stopwatch.Restart();
            DoTop40Fetch();
        }

        private void BW_Commodities_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Stopwatch.Stop();
            PopulateCommoditiesGrid();
            StopWaitVisual(Tab.Commodities);
            CallNextUpdateFetchTask();
        }

        private void BW_Currencies_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Stopwatch.Stop();
            PopulateCurrenciesGrid();
            StopWaitVisual(Tab.Currencies);
            CallNextUpdateFetchTask();
        }

        private void BW_JSEIndices_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Stopwatch.Stop();
            PopulateJSEIndiceGrid();
            StopWaitVisual(Tab.JSEIndice);
            CallNextUpdateFetchTask();
        }

        private void BW_Top40_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Stopwatch.Stop();
            PopulateTop40Grid();
            StopWaitVisual(Tab.Top40);
            CallNextUpdateFetchTask();
        }

        void SetFetchStatus(Tab tab, FetchStatus fetchStatus, string msg, bool logFetch)
        {
            PictureBox pb = GetPictureBox();

            switch (fetchStatus)
            {
                case FetchStatus.Loading:
                    pb.Invoke((MethodInvoker)(() => pb.Image = Properties.Resources.Loading));
                    break;
                case FetchStatus.Success:
                    pb.Invoke((MethodInvoker)(() => pb.Image = Properties.Resources.Success));
                    break;
                case FetchStatus.Error:
                    pb.Invoke((MethodInvoker)(() => pb.Image = Properties.Resources.Error));
                    break;
                case FetchStatus.Caution:
                    pb.Invoke((MethodInvoker)(() => pb.Image = Properties.Resources.Caution));
                    break;
            }

            UpdateFetchStatusLabel(tab, msg);

            if (fetchStatus == FetchStatus.Error || fetchStatus == FetchStatus.Caution)
                ((MainForm)MdiParent).RestoreFromTray();

            PictureBox GetPictureBox()
            {
                return tab switch
                {
                    Tab.Settings => pbGeneral,
                    Tab.Commodities => pbCommodities,
                    Tab.Currencies => pbCurrencies,
                    Tab.JSEIndice => pbJSEIndice,
                    Tab.Top40 => pbTop40,
                    _ => new PictureBox(),
                };
            }

            if (logFetch)
            {
                Program.Guid = Guid.NewGuid();
                ((MainForm)MdiParent).LogFetchStatus(Program.Guid, fetchStatus, msg);
            }
        }

        void StopFlashingWarning(Tab tab)
        {
            switch (tab)
            {
                case Tab.Settings:
                    {
                        pbGeneral.Invoke((MethodInvoker)(() => pbGeneral.Image = Properties.Resources.Success));
                        lblFetchStatusGeneral.Invoke((MethodInvoker)(() => lblFetchStatusGeneral.Text = ""));
                        break;
                    }
                case Tab.Commodities:
                    {
                        pbCommodities.Invoke((MethodInvoker)(() => pbCommodities.Image = Properties.Resources.Success));
                        lblFetchStatusCommodities.Invoke((MethodInvoker)(() => lblFetchStatusCommodities.Text = ""));
                        break;
                    }
                case Tab.Currencies:
                    {
                        pbCurrencies.Invoke((MethodInvoker)(() => pbCurrencies.Image = Properties.Resources.Success));
                        lblFetchStatusCurrencies.Invoke((MethodInvoker)(() => lblFetchStatusCurrencies.Text = ""));
                        break;
                    }
                case Tab.JSEIndice:
                    {
                        pbJSEIndice.Invoke((MethodInvoker)(() => pbJSEIndice.Image = Properties.Resources.Success));
                        lblFetchStatusJSEIndice.Invoke((MethodInvoker)(() => lblFetchStatusJSEIndice.Text = ""));
                        break;
                    }
                case Tab.Top40:
                    {
                        pbTop40.Invoke((MethodInvoker)(() => pbTop40.Image = Properties.Resources.Success));
                        lblFetchStatusTop40.Invoke((MethodInvoker)(() => lblFetchStatusTop40.Text = ""));
                        break;
                    }

            }
        }

        void UpdateFetchStatusLabel(Tab tab, string updateMessage)
        {
            switch (tab)
            {
                case Tab.Settings:
                    {
                        lblFetchStatusGeneral.Invoke((MethodInvoker)(() => lblFetchStatusGeneral.Text = updateMessage));
                        break;
                    }
                case Tab.Commodities:
                    {
                        lblFetchStatusCommodities.Invoke((MethodInvoker)(() => lblFetchStatusCommodities.Text = updateMessage));
                        break;
                    }
                case Tab.Currencies:
                    {
                        lblFetchStatusCurrencies.Invoke((MethodInvoker)(() => lblFetchStatusCurrencies.Text = updateMessage));
                        break;
                    }
                case Tab.JSEIndice:
                    {
                        lblFetchStatusJSEIndice.Invoke((MethodInvoker)(() => lblFetchStatusJSEIndice.Text = updateMessage));
                        break;
                    }
                case Tab.Top40:
                    {
                        lblFetchStatusTop40.Invoke((MethodInvoker)(() => lblFetchStatusTop40.Text = updateMessage));
                        break;
                    }
            }
        }

        private void DoCommoditiesFetch()
        {
            StartWaitVisual(Tab.Commodities);

            string status = $"Loading Commodities Fetch...{DL}Fetch started at {DateTime.Now:hh:mm:ss}{DL}.";
            SetFetchStatus(Tab.Commodities, FetchStatus.Loading, status, LOG_FETCH_YES);
            XmlDocument iressDoc = new();

            try
            {
                iressDoc.LoadXml(Fetch.Update(RequestCommodities));
            }
            catch (XmlException ex)
            {
                string msg = ex.Message;
                if (ex.Message == "Root element is missing.")
                {
                    msg += $" This is either because the Iress feed server is down, or your internet connection is down.";
                    Program.ShowMessage(this, msg, "Error fetching commodities", 5000, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                SetFetchStatus(Tab.Commodities, FetchStatus.Error, msg, LOG_FETCH_YES);
                return;
            }

            if (iressDoc.DocumentElement.Name == "Error")
            {
                string msg = $"Error fetching Commodities.{Environment.NewLine}";
                msg += $"{iressDoc.DocumentElement["Message"].InnerText}.{Environment.NewLine}";
                SetFetchStatus(Tab.Commodities, FetchStatus.Error, msg, LOG_FETCH_YES);
                return;
            }

            XmlDocument maestroDoc = new();

            List<Resource> commodities = new();

            if (Properties.Settings.Default.UseNewCommodities)
            {
                foreach (XmlElement record in iressDoc.DocumentElement)
                {
                    Resource commodity = Resource.FromNewFeed(ResourceType.Commodity, record);
                    commodities.Add(commodity);
                }
            }
            else
            {
                foreach (XmlElement quote in iressDoc.DocumentElement)
                {
                    Resource commodity = Resource.FromOldFeed(ResourceType.Commodity, quote);
                    commodities.Add(commodity);
                }
            }

            string date = DateTime.Now.ToLongDateString();
            string maestroString = "<Commodities date='" + date + "'>";

            foreach (Resource commodity in commodities)
            {
                maestroString += commodity.ToXml();
            }

            maestroString += "</Commodities>";

            try
            {
                maestroDoc.LoadXml(maestroString);
            }
            catch (XmlException ex)
            {
                string msg = $"Error encountered while composing 'Commodities' xml string.{DL}{ex.Message}{DL}{ex.StackTrace}";
                SetFetchStatus(Tab.Commodities, FetchStatus.Error, msg, LOG_FETCH_YES);
                return;
            }

            try
            {
                maestroDoc.Save(Properties.Settings.Default.SaveFolderPath + @"\Commodities.xml");
            }
            catch (Exception ex)
            {
                string msg = $"Error encountered while saving 'Commodities' xml document.{DL}{ex.Message}{DL}{ex.StackTrace}";
                SetFetchStatus(Tab.Commodities, FetchStatus.Error, msg, LOG_FETCH_YES);
                return;
            }

            Program.Guid = Guid.NewGuid();
            ((MainForm)MdiParent).LogFetchData(Program.Guid, date, commodities);
            SetFetchStatus(Tab.Commodities, FetchStatus.Success, "Commodities fetch successful.", LOG_FETCH_YES);
        }

        void DoCurrenciesFetch()
        {
            StartWaitVisual(Tab.Currencies);

            string status = $"Loading Currencies Fetch...{DL}Fetch started at {DateTime.Now:hh:mm:ss}{DL}.";
            SetFetchStatus(Tab.Currencies, FetchStatus.Loading, status, LOG_FETCH_YES);
            XmlDocument iressDoc = new();

            try
            {
                iressDoc.LoadXml(Fetch.Update(RequestCurrencies));
            }
            catch (XmlException ex)
            {
                string msg = ex.Message;
                if (ex.Message == "Root element is missing.")
                {
                    msg += $" This is either because the Iress feed server is down, or your internet connection is down.";
                    Program.ShowMessage(this, msg, "Error fetching currencies", 5000, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                SetFetchStatus(Tab.Currencies, FetchStatus.Error, msg, LOG_FETCH_YES);
                return;
            }

            if (iressDoc.DocumentElement.Name == "Error")
            {
                string msg = $"Error fetching Currencies.{Environment.NewLine}";
                msg += $"{iressDoc.DocumentElement["Message"].InnerText}.";
                SetFetchStatus(Tab.Currencies, FetchStatus.Error, msg, LOG_FETCH_YES);
                return;
            }

            XmlDocument maestroDoc = new();

            List<Resource> currencies = new();

            if (Properties.Settings.Default.UseNewCurrencies)
            {
                foreach (XmlElement row in iressDoc.DocumentElement)
                {
                    Resource currency = Resource.FromNewFeed(ResourceType.Currency, row);
                    currencies.Add(currency);
                }
            }
            else
            {
                foreach (XmlElement row in iressDoc.DocumentElement)
                {
                    Resource currency = Resource.FromOldFeed(ResourceType.Currency, row);
                    currencies.Add(currency);
                }
            }

            string date = DateTime.Now.ToLongDateString();
            string maestroString = "<Currencies Date='" + date + "'>";

            foreach (Resource currency in currencies)
                maestroString += currency.ToXml();

            maestroString += "</Currencies>";
            try
            {
                maestroDoc.LoadXml(maestroString);
            }
            catch (XmlException ex)
            {
                string msg = $"Error encountered while composing 'Currencies' xml string.{DL}{ex.Message}{DL}{ex.StackTrace}";
                SetFetchStatus(Tab.Currencies, FetchStatus.Error, msg, LOG_FETCH_YES);
                return;
            }

            try
            {
                maestroDoc.Save(Properties.Settings.Default.SaveFolderPath + @"\Currencies.xml");
            }
            catch (XmlException ex)
            {
                string msg = $"Error encountered while saving 'Currencies' xml document.{DL}{ex.Message}{DL}{ex.StackTrace}";
                SetFetchStatus(Tab.Currencies, FetchStatus.Error, msg, LOG_FETCH_YES);
                return;
            }

            Program.Guid = Guid.NewGuid();
            ((MainForm)MdiParent).LogFetchData(Program.Guid, date, currencies);
            SetFetchStatus(Tab.Currencies, FetchStatus.Success, "Currencies fetch successful.", LOG_FETCH_YES);
        }

        void DoJSEIndiceFetch()
        {
            StartWaitVisual(Tab.JSEIndice);
            
            string status = $"Loading JSE-Indice Fetch...{DL}Fetch started at {DateTime.Now:hh:mm:ss}{DL}.";
            SetFetchStatus(Tab.JSEIndice, FetchStatus.Loading, status, LOG_FETCH_YES);
            XmlDocument iressDoc = new();

            try
            {
                iressDoc.LoadXml(Fetch.Update(RequestJSEIndice));
            }
            catch (XmlException ex)
            {
                string msg = ex.Message;
                if (ex.Message == "Root element is missing.")
                {
                    msg += $" This is either because the Iress feed server is down, or your internet connection is down.";
                    Program.ShowMessage(this, msg, "Error fetching Indice", 5000, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                SetFetchStatus(Tab.JSEIndice, FetchStatus.Error, msg, LOG_FETCH_YES);
                return;
            }

            if (iressDoc.DocumentElement.Name == "Error")
            {
                string msg = $"Error fetching JSE Indice.{Environment.NewLine}";
                msg += iressDoc.DocumentElement["Message"].InnerText;
                SetFetchStatus(Tab.JSEIndice, FetchStatus.Error, msg, LOG_FETCH_YES);
                return;
            }

            XmlDocument maestroDoc = new();

            List<Resource> indice = new();

            if (Properties.Settings.Default.UseNewJSEIndice)
            {
                foreach (XmlElement row in iressDoc.DocumentElement)
                {
                    Resource index = Resource.FromNewFeed(ResourceType.Index, row);
                    indice.Add(index);
                }
            }
            else
            {
                foreach (XmlElement row in iressDoc.DocumentElement)
                {
                    Resource index = Resource.FromOldFeed(ResourceType.Index, row);
                    indice.Add(index);
                }
            }

            string date = DateTime.Now.ToLongDateString();
            string maestroString = "<JSEIndice date='" + date + "'>";

            foreach (Resource index in indice)
            {
                maestroString += index.ToXml();
            }

            maestroString += "</JSEIndice>";

            try
            {
                maestroDoc.LoadXml(maestroString);
            }
            catch (XmlException ex)
            {
                string msg = $"Error encountered composing 'JSE Indice' xml string.{DL}{ex.Message}{DL}{ex.StackTrace}";
                SetFetchStatus(Tab.JSEIndice, FetchStatus.Error, msg, LOG_FETCH_YES);
                return;
            }
            try
            {
                maestroDoc.Save(Properties.Settings.Default.SaveFolderPath + @"\JSEIndice.xml");
            }
            catch (Exception ex)
            {
                string msg = $"Error encountered while saving xml document.{DL}{ex.Message}{DL}{ex.StackTrace}";
                SetFetchStatus(Tab.JSEIndice, FetchStatus.Error, msg, LOG_FETCH_YES);
                return;
            }

            Program.Guid = Guid.NewGuid();
            ((MainForm)MdiParent).LogFetchData(Program.Guid, date, indice);
            SetFetchStatus(Tab.JSEIndice, FetchStatus.Success, "JSE Indice fetch successful.", LOG_FETCH_YES);
        }

        private void DoTop40Fetch()
        {
            StartWaitVisual(Tab.Top40);

            string status = $"Loading JSE Top 40 Fetch...{DL}Fetch started at {DateTime.Now:hh:mm:ss}{DL}.";
            SetFetchStatus(Tab.Top40, FetchStatus.Loading, status, LOG_FETCH_YES);
            XmlDocument iressDoc = new();

            try
            {
                iressDoc.LoadXml(Fetch.Update(RequestTop40));
            }
            catch (XmlException ex)
            {
                string msg = ex.Message;
                if (ex.Message == "Root element is missing.")
                {
                    msg += $" This is either because the Iress feed server is down, or your internet connection is down.";
                    Program.ShowMessage(this, msg, "Error fetching commodities", 5000, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                SetFetchStatus(Tab.Top40, FetchStatus.Error, msg, LOG_FETCH_YES);
                return;
            }

            if (iressDoc.DocumentElement.Name == "Error")
            {
                string msg = $"Error fetching JSE Top 40 Index.{Environment.NewLine}";
                msg += iressDoc.DocumentElement["Message"].InnerText;
                SetFetchStatus(Tab.Top40, FetchStatus.Error, msg, LOG_FETCH_YES);
                return;
            }

            XmlDocument maestroDoc = new();

            List<Resource> shares = new();

            if (Properties.Settings.Default.UseNewTop40)
            {
                foreach (XmlElement record in iressDoc.DocumentElement)
                {
                    Resource share = Resource.FromNewFeed(ResourceType.Share, record);
                    shares.Add(share);
                }
                shares = shares.OrderBy(s => s.Code).ToList();
            }
            else
            {
                foreach (XmlElement quote in iressDoc.DocumentElement)
                {
                    Resource share = Resource.FromOldFeed(ResourceType.Share, quote);
                    shares.Add(share);
                }
            }            

            if (shares.Count != 40)
            {
                SetFetchStatus(Tab.Top40, FetchStatus.Error, $"Iress returned {shares.Count} companies for the Top 40. Contact Iress or try to request again. Update request aborted!", LOG_FETCH_YES);
                return;
            }

            string date = DateTime.Now.ToLongDateString();
            string maestroString = "<Top40 date='" + date + "'>";

            foreach (Resource share in shares)
            {
                maestroString += share.ToXml();
            }

            maestroString += "</Top40>";
            try
            {
                maestroDoc.LoadXml(maestroString);
            }
            catch (XmlException ex)
            {
                string msg = $"Error encountered while composing 'Top 40' xml string.{DL}{ex.Message}{DL}{ex.StackTrace}";
                SetFetchStatus(Tab.Top40, FetchStatus.Error, msg, LOG_FETCH_YES);
                return;
            }
            try
            {
                maestroDoc.Save(Properties.Settings.Default.SaveFolderPath + @"\Top40.xml");
            }
            catch (Exception ex)
            {
                string msg = $"Error encountered while saving xml document.{DL}{ex.Message}{DL}{ex.StackTrace}";
                SetFetchStatus(Tab.Top40, FetchStatus.Error, msg, LOG_FETCH_YES);
                return;
            }

            Program.Guid = Guid.NewGuid();
            ((MainForm)MdiParent).LogFetchData(Program.Guid, date, shares);
            SetFetchStatus(Tab.Top40, FetchStatus.Success, "Top 40 fetch successful.", LOG_FETCH_YES);
        }

        public void UpdateAll(bool manualOverride = false)
        {
            CheckInternetConnection();
            string drive = Path.GetPathRoot(Properties.Settings.Default.SaveFolderPath);
            if (!Directory.Exists(drive))
            {
                if (OperatingSystem.IsWindows())
                {
                    WindowsIdentity identity = WindowsIdentity.GetCurrent();
                    WindowsPrincipal principal = new(identity);

                    string message = $"{drive}: drive is inaccesible. Cancelling fetch operation?";

                    if (principal.IsInRole(WindowsBuiltInRole.Administrator))
                        message += "\n\nMake sure you are not running the application as Administrator";

                    SetFetchStatus(Tab.Settings, FetchStatus.Error, message, LOG_FETCH_YES);

                    Program.ShowMessage(this, message, "Incassible drive", 2000, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                RestartTimer();
                return;
            }

            string directory = Path.GetDirectoryName(Properties.Settings.Default.SaveFolderPath);
            if (!Directory.Exists(directory))
            {
                if (MessageBox.Show($"The directory {directory} was not found.\n\nPlease select a valid save path.", "Invalid Save Path", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) == DialogResult.OK)
                    NewSavePath();
                else
                {
                    string message = "Fetch cannot run unless you select a valid save path.";

                    MessageBox.Show(message, "Fetch Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                    SetFetchStatus(Tab.Settings, FetchStatus.Error, message, LOG_FETCH_YES);

                    return;
                }
            }

            QueueAll(manualOverride);

            CallNextUpdateFetchTask();
        }

        public void CancelAll()
        {
            BW_Commodities.CancelAsync();
            BW_Currencies.CancelAsync();
            BW_JSEIndice.CancelAsync();
            BW_Top40.CancelAsync();

            UpdateQueue.Clear();

            Application.DoEvents();
        }

        bool CanUpdateJSE()
        {
            TimeSpan timeToUse = Properties.Settings.Default.SimulateTime ? dtpSimTime.Value.TimeOfDay : DateTime.Now.TimeOfDay;
            DateTime dateToUse = Properties.Settings.Default.SimulateTime ? dtpSimTime.Value.Date : DateTime.Now.Date;
            TimeSpan updateJSEStart = TimeSpan.Parse(Properties.Settings.Default.JSEUpdateStart);
            TimeSpan updateJSEEnd = TimeSpan.Parse(Properties.Settings.Default.JSEUpdateEnd);
            return timeToUse >= updateJSEStart && timeToUse <= updateJSEEnd && !DateSystem.IsPublicHoliday(dateToUse, CountryCode.ZA) && !DateSystem.IsWeekend(dateToUse, CountryCode.ZA);
        }

        void QueueAll(bool manualOverride = false)
        {
            UpdateQueue.Clear();

            if (Properties.Settings.Default.UpdateCommodities)
                UpdateQueue.Enqueue(StartCommoditiesFetch);

            if (Properties.Settings.Default.UpdateCurrencies)
                UpdateQueue.Enqueue(StartCurrenciesFetch);

            if (CanUpdateJSE())
            {

                if (Properties.Settings.Default.UpdateJSEIndice)
                    UpdateQueue.Enqueue(StartJSEIndiceFetch);
                if (Properties.Settings.Default.UpdateTop40)
                    UpdateQueue.Enqueue(StartTop40Fetch);
            }
            else if (manualOverride)
            {
                int time = 5000;
                Program.ShowMessage(this, $"Cannot update at the moment because the markets are closed. Timer will restart in {((string)(time / 1000).ToString("D2"))} seconds.", "Markets closed.", time, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                RestartTimer();
            }
        }

        void CallNextUpdateFetchTask()
        {
            if (UpdateQueue.Count > 0)
            {
                MethodInvoker method = UpdateQueue.Dequeue();
                method();
                ((MainForm)MdiParent).mniCancelUpdate.Enabled = true;
            }
            else
            {
                ((MainForm)MdiParent).mniCancelUpdate.Enabled = false;

                Properties.Settings.Default.LastUpdateAt = DateTime.Now;
                Properties.Settings.Default.Save();

                ((MainForm)MdiParent).UpdateStatus("All fetches complete.");
                RestartTimer();

                if (Properties.Settings.Default.SwitchBackToSettingsTab)
                    SwitchToTab(Tab.Settings);
            }
        }

        void StartCommoditiesFetch()
        {
            if (Properties.Settings.Default.SwitchToActiveFetchTab)
                SwitchToTab(Tab.Commodities);
            if (!BW_Commodities.IsBusy)
                BW_Commodities.RunWorkerAsync();
        }

        void StartCurrenciesFetch()
        {
            if (Properties.Settings.Default.SwitchToActiveFetchTab)
                SwitchToTab(Tab.Currencies);
            if (!BW_Currencies.IsBusy)
                BW_Currencies.RunWorkerAsync();
        }

        void StartJSEIndiceFetch()
        {
            if (Properties.Settings.Default.SwitchToActiveFetchTab)
                SwitchToTab(Tab.JSEIndice);
            if (!BW_JSEIndice.IsBusy)
                BW_JSEIndice.RunWorkerAsync();
        }

        void StartTop40Fetch()
        {
            if (Properties.Settings.Default.SwitchToActiveFetchTab)
                SwitchToTab(Tab.Top40);
            if (!BW_Top40.IsBusy)
                BW_Top40.RunWorkerAsync();
        }

        void StartWaitVisual(Tab tab)
        {
            Mouse.DisableMouse();
            StopFlashingWarning(tab);
            switch (tab)
            {
                case Tab.Commodities:
                    {
                        TabCommodities.UseWaitCursor = true;
                        ((MainForm)MdiParent).UpdateProgressBar(ProgressBarStyle.Marquee);
                        ((MainForm)MdiParent).UpdateStatus("Fetching 'Commodities' update...");
                        break;
                    }
                case Tab.Currencies:
                    {
                        TabCurrencies.UseWaitCursor = true;
                        ((MainForm)MdiParent).UpdateProgressBar(ProgressBarStyle.Marquee);
                        ((MainForm)MdiParent).UpdateStatus("Fetching 'Currencies' update...");
                        break;
                    }
                case Tab.JSEIndice:
                    {
                        TabJSEIndice.UseWaitCursor = true;
                        ((MainForm)MdiParent).UpdateProgressBar(ProgressBarStyle.Marquee);
                        ((MainForm)MdiParent).UpdateStatus("Fetching 'JSE Indice' update...");
                        break;
                    }
                case Tab.Top40:
                    {
                        TabTop40.UseWaitCursor = true;
                        ((MainForm)MdiParent).UpdateProgressBar(ProgressBarStyle.Marquee);
                        ((MainForm)MdiParent).UpdateStatus("Fetching 'Top 40' update...");
                        break;
                    }
            }
        }

        void StopWaitVisual(Tab board)
        {
            switch (board)
            {
                case Tab.Commodities:
                    {
                        TabCommodities.UseWaitCursor = false;
                        ((MainForm)MdiParent).UpdateProgressBar(ProgressBarStyle.Continuous);
                        ((MainForm)MdiParent).UpdateStatus("Commodities update is complete.");
                        TimeSpan duration = Stopwatch.Elapsed;
                        lblFetchStatusCommodities.Text += $"{DL}Fetch completion time: {duration}.";
                        break;
                    }
                case Tab.Currencies:
                    {
                        TabCurrencies.UseWaitCursor = false;
                        ((MainForm)MdiParent).UpdateProgressBar(ProgressBarStyle.Continuous);
                        ((MainForm)MdiParent).UpdateStatus("Currencies update is complete.");
                        TimeSpan duration = Stopwatch.Elapsed;
                        lblFetchStatusCurrencies.Text += $"{DL}Fetch completion time: {duration}.";
                        break;
                    }
                case Tab.JSEIndice:
                    {
                        TabJSEIndice.UseWaitCursor = false;
                        ((MainForm)MdiParent).UpdateProgressBar(ProgressBarStyle.Continuous);
                        ((MainForm)MdiParent).UpdateStatus("JSE-Indice update is complete.");
                        TimeSpan duration = Stopwatch.Elapsed;
                        lblFetchStatusJSEIndice.Text += $"{DL}Fetch completion time: {duration}.";
                        break;
                    }
                case Tab.Top40:
                    {
                        TabTop40.UseWaitCursor = false;
                        ((MainForm)MdiParent).UpdateProgressBar(ProgressBarStyle.Continuous);
                        ((MainForm)MdiParent).UpdateStatus("Top-40 update is complete.");
                        TimeSpan duration = Stopwatch.Elapsed;
                        lblFetchStatusTop40.Text += $"{DL}Fetch completion time: {duration}.";
                        break;
                    }
            }
            Mouse.EnableMouse();
        }

        string GetFolderPath(string path)
        {
            if (!Directory.Exists(path))
            {
                NewSavePath();
            }

            if (path.EndsWith(@"\"))
                return path;
            else
                return path + @"\";
        }

        void PopulateCommoditiesGrid()
        {
            string sfp = GetFolderPath(Properties.Settings.Default.SaveFolderPath) + "Commodities.xml";

            if (!File.Exists(sfp)) return;

            ((MainForm)MdiParent).UpdateStatus("Populating Commodities grid...");
            if (!wbCommodities.IsBusy)
            {
                if (wbCommodities.Url != new Uri(sfp))
                    wbCommodities.Navigate(sfp);
                wbCommodities.Refresh();
            }
            Application.DoEvents();
            ClearGrid(dgCommodities);
            int k = 1;

            XmlDocument doc = new();
            doc.Load(sfp);

            string code, sname, time, points, trade, perc;
            foreach (XmlElement node in doc.DocumentElement)
            {
                code = node["Code"].InnerText;
                sname = node["ShortName"].InnerText;
                trade = node["Trade"].InnerText;
                time = node["Time"].InnerText;
                perc = node["Perc"].InnerText;
                points = node["Points"].InnerText;

                string[] row = { (k++).ToString("D2"), code, sname, time, trade, points, perc};
                AddRowToGrid(dgCommodities, row);
            }

            ResizeDGV(dgCommodities);
        }

        void PopulateCurrenciesGrid()
        {
            string sfp = GetFolderPath(Properties.Settings.Default.SaveFolderPath) + "Currencies.xml";

            if (!File.Exists(sfp)) return;

            ((MainForm)MdiParent).UpdateStatus("Populating Currencies grid...");

            if (!wbCurrencies.IsBusy)
            {
                if (wbCurrencies.Url != new Uri(sfp))
                    wbCurrencies.Navigate(sfp);
                wbCurrencies.Refresh();
            }
            Application.DoEvents();
            ClearGrid(dgCurrencies);
            int k = 1;

            XmlDocument doc = new();
            doc.Load(sfp);

            string code, sname, time, points, trade, perc;
            foreach (XmlElement node in doc.DocumentElement)
            {
                code = node["Code"].InnerText;
                sname = node["ShortName"].InnerText;
                trade = node["Trade"].InnerText;
                time = node["Time"].InnerText;
                perc = node["Perc"].InnerText;
                points = node["Points"].InnerText;

                string[] row = { (k++).ToString("D2"), code, sname, time, trade, points, perc };
                AddRowToGrid(dgCurrencies, row);
            }

            ResizeDGV(dgCurrencies);
        }

        void PopulateJSEIndiceGrid()
        {
            string sfp = GetFolderPath(Properties.Settings.Default.SaveFolderPath) + "JSEIndice.xml";

            if (!File.Exists(sfp)) return;

            ((MainForm)MdiParent).UpdateStatus("Populating JSE Indice grid...");
            if (!wbJSEIndices.IsBusy)
            {
                if (wbJSEIndices.Url != new Uri(sfp))
                    wbJSEIndices.Navigate(sfp);
                wbJSEIndices.Refresh();
            }
            Application.DoEvents();
            ClearGrid(dgJSEIndice);
            int k = 1;

            XmlDocument doc = new();
            doc.Load(sfp);

            string code, sname, time, points, trade, perc;
            foreach (XmlElement node in doc.DocumentElement)
            {
                code = node["Code"].InnerText;
                sname = node["ShortName"].InnerText;
                trade = node["Trade"].InnerText;
                time = node["Time"].InnerText;
                perc = node["Perc"].InnerText;
                points = node["Points"].InnerText;

                string[] row = { (k++).ToString("D2"), code, sname, time, trade, points, perc};
                AddRowToGrid(dgJSEIndice, row);
            }

            ResizeDGV(dgJSEIndice);
        }

        void PopulateTop40Grid()
        {
            string sfp = GetFolderPath(Properties.Settings.Default.SaveFolderPath) + "Top40.xml";

            if (!File.Exists(sfp)) return;

            ((MainForm)MdiParent).UpdateStatus("Populating Top-40 grid...");

            if (!wbTop40.IsBusy)
            {
                if (wbTop40.Url != new Uri(sfp))
                    wbTop40.Navigate(sfp);
                wbTop40.Refresh();
            }
            Application.DoEvents();
            ClearGrid(dgTop40);
            int k = 1;

            XmlDocument doc = new();
            doc.Load(sfp);

            string code, sname, time, points, perc, trade;
            foreach (XmlElement node in doc.DocumentElement)
            {
                code = node["Code"].InnerText;
                sname = node["ShortName"].InnerText;
                trade = node["Trade"].InnerText;
                time = node["Time"].InnerText;
                perc = node["Perc"].InnerText;
                points = node["Points"].InnerText;

                string[] row = { (k++).ToString("D2"), code, sname, time, trade, points, perc };
                AddRowToGrid(dgTop40, row);
            }

            ResizeDGV(dgTop40);
        }

        static void ResizeDGV(DataGridView dgv)
        {
            int k = 0;
            foreach (DataGridViewColumn column in dgv.Columns)
            {
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                k += column.Width;
            }

            dgv.Width = k + dgv.RowHeadersWidth + 10;
        }

        public static void AddRowToGrid(DataGridView dgv, string[] row)
        {
            if (dgv.InvokeRequired)
                dgv.Invoke(new MethodInvoker(delegate
                {
                    dgv.Rows.Add(row);
                }));
            else
                dgv.Rows.Add(row);

            ScrollDataGridToBottom(dgv);
        }

        void SwitchToTab(Tab board)
        {
            tabControl.SelectedTab = board == Tab.Commodities ? TabCommodities : board == Tab.Currencies ? TabCurrencies : board == Tab.JSEIndice ? TabJSEIndice : board == Tab.Top40 ? TabTop40 : TabSettings;
        }

        void SwitchToTab(TabPage tabPage) => tabControl.SelectedTab = tabPage;
        
        static void ScrollDataGridToBottom(DataGridView dgv)
        {

            if (dgv.InvokeRequired)
                dgv.Invoke((MethodInvoker)(() => dgv.FirstDisplayedScrollingRowIndex = dgv.RowCount - 1));
            else
                dgv.FirstDisplayedScrollingRowIndex = dgv.RowCount - 1;
        }

        public static void ClearGrid(DataGridView dataGridView)
        {
            if (dataGridView.InvokeRequired)
                dataGridView.Invoke(new MethodInvoker(delegate
                {
                    dataGridView.Rows.Clear();
                }));
            else
                dataGridView.Rows.Clear();
        }

        private void BtnChangePath_Click(object sender, EventArgs e)
        {
            NewSavePath();
        }

        void NewSavePath()
        {
            FolderBrowserDialog fbd = new()
            {
                Description = $"Save path for XML files",
                SelectedPath = Properties.Settings.Default.SaveFolderPath,
                ShowNewFolderButton = true,
            };

            FolderBrowserLauncher.ShowFolderBrowser(fbd);
            if (fbd.SelectedPath != "")
            {
                Properties.Settings.Default.SaveFolderPath = txtSaveFolderPath.Text = fbd.SelectedPath;
                Properties.Settings.Default.Save();
            }

            if (!Directory.Exists(Properties.Settings.Default.SaveFolderPath))
                Program.ShowMessage(this, $"{Properties.Settings.Default.SaveFolderPath} does not exist.", "Directory Does Not Exist", 10000, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void Canvas_Shown(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Maximized;

            LoadLastSaves();

            tabControl.SelectTab(Properties.Settings.Default.LastOpenTab);

            ((MainForm)MdiParent).UpdateStatus("Ready...");

            if (Properties.Settings.Default.UpdateOnStart)
                UpdateAll();

            if (Properties.Settings.Default.StartAutoUpdateOnStart)
                StartTimer();

            CheckInternetConnection();
        }

        void UpdateLinks()
        {
            lblLinkCommodities.Text = $"{RequestCommodities}";
            lblLinkCurrencies.Text = $"{RequestCurrencies}";
            lblLinkJSEIndices.Text = $"{RequestJSEIndice}";
            lblLinkJSETop40.Text = $"{RequestTop40}";
        }

        private void Canvas_FormClosed(object sender, FormClosedEventArgs e)
        {
            Properties.Settings.Default.LastOpenTab = tabControl.TabIndex;
        }

        private void Canvas_Resize(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Maximized;
        }

        private void CbUpdateOnStart_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.UpdateOnStart = cbUpdateOnStart.Checked;
            Properties.Settings.Default.Save();
        }

        private void CbStartAutoUpdateOnUpdate_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.StartAutoUpdateOnStart = cbStartAutoUpdateOnUpdate.Checked;
            Properties.Settings.Default.Save();
        }

        private void CbMinimizeToTray_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.MinimizeToTray = cbMinimizeToTray.Checked;
            Properties.Settings.Default.Save();
        }

        private void CbStartMinimized_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.StartMinimized = cbStartMinimized.Checked;
            Properties.Settings.Default.Save();
        }

        private void NudInterval_ValueChanged(object sender, EventArgs e)
        {
            SetInterval();
            Properties.Settings.Default.NudInterval = (int)nudInterval.Value;
            Properties.Settings.Default.Save();
        }

        private void CmbInterval_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetInterval();
            Properties.Settings.Default.IntervalPeriodIndex = (sbyte)cmbInterval.SelectedIndex;
            Properties.Settings.Default.Save();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            Interval--;
            if (Interval <= 0)
            {
                NextUpdateTimer.Stop();
                UpdateAll();
            }
            else
            {
                TimeSpan nextIn = TimeSpan.FromSeconds(Interval - 1);
                lblNextUpdateIn.Text = $"Next Update In: {nextIn:hh\\:mm\\:ss}";
            }
        }

        private void SetInterval()
        {
            switch (cmbInterval.SelectedIndex)
            {
                case (0):
                    {
#if DEBUG
                        nudInterval.Minimum = 10;
#else
                        nudInterval.Minimum = 30;
#endif
                        Interval = (int)nudInterval.Value;
                        break;
                    }
                case (1):
                    {
                        nudInterval.Minimum = 1;
                        Interval = (int)nudInterval.Value * 60;
                        break;
                    }
                case (2):
                    {
                        nudInterval.Minimum = 1;
                        Interval = (int)nudInterval.Value * 60 * 60;
                        break;
                    }
                case (3):
                    {
                        nudInterval.Minimum = 1;
                        Interval = (int)nudInterval.Value * 60 * 60 * 24;
                        break;
                    }
            }

            NextUpdateAt = DateTime.Now.AddSeconds(Interval);
        }

        public void RestartTimer()
        {
            SetInterval();
            lblLastUpdateAt.Text = $"Last Update At: {Properties.Settings.Default.LastUpdateAt:HH:mm:ss dddd, dd MMMM yyyy}";
            if (AutoFetch)
            {
                //SwitchToTab(Tab.Settings);
                NextUpdateTimer.Start();
                lblNextUpdateAt.Text = $"Next Update At: {NextUpdateAt:HH:mm:ss dddd, dd MMMM yyyy}";
            }
        }

        public void StartTimer()
        {
            btnStart.Enabled = false;
            btnStop.Enabled = true;
            AutoFetch = true;
            NextUpdateAt = DateTime.Now.AddSeconds(Interval - 2);
            lblNextUpdateAt.Text = $"Next Update At: {NextUpdateAt:HH:mm:ss dddd, dd MMMM yyyy}";
            NextUpdateTimer.Start();

            ((MainForm)MdiParent).NotifyIconText = "Financials -> Auto-Update Running!";
            ((MainForm)MdiParent).AutoUpdateText = "Stop Auto-Update";
        }

        public void StopTimer()
        {
            NextUpdateTimer.Stop();
            AutoFetch = false;
            btnStop.Enabled = false;
            btnStart.Enabled = true;

            lblNextUpdateIn.Text = "Next Update In: ...";
            lblNextUpdateAt.Text = "Next Update At: ...";
            if (Properties.Settings.Default.LastUpdateAt == DateTime.MinValue)
                lblLastUpdateAt.Text = "Last Update At: ...";
            else
                lblLastUpdateAt.Text = "Last Update At: " + Properties.Settings.Default.LastUpdateAt.ToString(@"HH:mm:ss dddd, dd MMMM yyyy");
            ((MainForm)MdiParent).NotifyIconText = "Financials -> Auto-Update Stopped!";
            ((MainForm)MdiParent).AutoUpdateText = "Start Auto-Update";
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            StartTimer();
        }

        private void BtnStop_Click(object sender, EventArgs e)
        {
            StopTimer();
        }

        private void CbUpdateResource_CheckedChanged(object sender, EventArgs e)
        {
            bool AtLeastOneUpdateSelected()
            {
                if (!cbUpdateCommodities.Checked && !cbUpdateCurrencies.Checked && !cbUpdateJSEIndice.Checked && !cbUpdateTop40.Checked)
                {
                    SwitchToTab(Tab.Settings);
                    Program.ShowMessage(this, "At least one financial resource needs to be checked for the update to run.", "Select One Option", 10000, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return false;
                }
                return true;
            }

            CheckBox target = ((CheckBox)sender);
            if (!AtLeastOneUpdateSelected())
            {
                target.Checked = true;
                return;
            }
            switch (target.Name)
            {
                case "cbUpdateCommodities":
                    {
                        Properties.Settings.Default.UpdateCommodities = cbUpdateCommodities.Checked;
                        break;
                    }
                case "cbUpdateCurrencies":
                    {
                        Properties.Settings.Default.UpdateCurrencies = cbUpdateCurrencies.Checked;
                        break;
                    }
                case "cbUpdateJSEIndice":
                    {
                        Properties.Settings.Default.UpdateJSEIndice = cbUpdateJSEIndice.Checked;
                        break;
                    }
                case "cbUpdateTop40":
                    {
                        Properties.Settings.Default.UpdateTop40 = cbUpdateTop40.Checked;
                        break;
                    }
            }
            Properties.Settings.Default.Save();
        }

        private void CbSwitchToActiveFetchTab_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.SwitchToActiveFetchTab = cbSwitchToActiveFetchTab.Checked;
            Properties.Settings.Default.Save();
        }

        private void CbSwitchBackToSettingsTab_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.SwitchBackToSettingsTab = cbSwitchBackToSettingsTab.Checked;
            Properties.Settings.Default.Save();
        }

        private void CbBringToForefront_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.BringToForefront = cbBringToForefront.Checked;
            Properties.Settings.Default.Save();
        }

        private void CbSaveLogs_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.SaveLogs = cbSaveLogs.Checked;
            Properties.Settings.Default.Save();
        }

        private void CbSimulateTime_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.SimulateTime = cbSimulateTime.Checked;
            Properties.Settings.Default.Save();
        }

        private void RbCommoditiesChangeFeed_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.UseNewCommodities = rbCommoditiesUseNewFeed.Checked;
            Properties.Settings.Default.Save();

            if (Properties.Settings.Default.UseNewCommodities)
                RequestCommodities = Properties.Settings.Default.RequestCommodities_New;
            else
                RequestCommodities = Properties.Settings.Default.RequestCommodities_Old;

            UpdateLinks();
        }

        private void RbCurrenciesChangeFeed_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.UseNewCurrencies = rbCurrenciesUseNewFeed.Checked;
            Properties.Settings.Default.Save();

            if (Properties.Settings.Default.UseNewCurrencies)
                RequestCurrencies = Properties.Settings.Default.RequestCurrencies_New;
            else
                RequestCurrencies = Properties.Settings.Default.RequestCurrencies_Old;

            UpdateLinks();
        }

        private void RbJSEIndiceChangeFeed_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.UseNewJSEIndice = rbJSEIndiceUseNewFeed.Checked;
            Properties.Settings.Default.Save();

            if (Properties.Settings.Default.UseNewJSEIndice)
                RequestJSEIndice = Properties.Settings.Default.RequestJSEIndice_New;
            else
                RequestJSEIndice = Properties.Settings.Default.RequestJSEIndice_Old;

            UpdateLinks();
        }

        private void RbTop40ChangeFeed_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.UseNewTop40 = rbTop40UseNewFeed.Checked;
            Properties.Settings.Default.Save();

            if (Properties.Settings.Default.UseNewTop40)
                RequestTop40 = Properties.Settings.Default.RequestTop40_New;
            else
                RequestTop40 = Properties.Settings.Default.RequestTop40_Old;

            UpdateLinks();
        }

        private void CbLogEvents_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.LogEvents = cbLogEvents.Checked;
            Properties.Settings.Default.Save();
        }

        private void PbBrowseCommditiesURL_Click(object sender, EventArgs e) => Process.Start(new ProcessStartInfo { FileName = $"{RequestCommodities}", UseShellExecute = true });

        private void PbBrowseCurrenciesURL_Click(object sender, EventArgs e) => Process.Start(new ProcessStartInfo { FileName = $"{RequestCurrencies}", UseShellExecute = true });

        private void PbBrowseJSEIndiceURL_Click(object sender, EventArgs e) => Process.Start(new ProcessStartInfo { FileName = $"{RequestJSEIndice}", UseShellExecute = true });

        private void PbBrowseTop40URL_Click(object sender, EventArgs e) => Process.Start(new ProcessStartInfo { FileName = $"{RequestTop40}", UseShellExecute = true });

        private void Data_Date_Filtered(object sender, DateRangeEventArgs e) => FilterDataLogsToDate();

        private void Data_Time_Filtered(object sender, EventArgs e)
        {
            FilterDataLogsToTime();
        }

        private void FetchStatus_Date_Filtered(object sender, DateRangeEventArgs e) => FilterStatusLogsToDateTime();

        private void FetchStatus_Time_Filtered(object sender, EventArgs e)
        {
            FilterStatusLogsToDateTime();
        }

        internal void FilterStatusLogsToDateTime()
        {
            DateTime startTime = mcStatusFrom.SelectionRange.Start.Date + dtpFetchStatusFrom.Value.TimeOfDay;
            DateTime endTime = mcStatusTo.SelectionRange.End.Date + dtpFetchStatusTo.Value.TimeOfDay;
            ((MainForm)MdiParent).FilterStatusLogs(startTime, endTime);
        }

        internal void FilterDataLogsToDate()
        {
            DateTime startDate = mcDataLogsFrom.SelectionRange.Start.Date;
            DateTime endDate = mcDataLogsTo.SelectionRange.End.Date;
            ((MainForm)MdiParent).FilterDataLogs(startDate, endDate);
        }

        internal void FilterDataLogsToTime()
        {
            TimeSpan startTime = dtpDataLogsFrom.Value.TimeOfDay;
            TimeSpan endTime = dtpDataLogsTo.Value.TimeOfDay;
            ((MainForm)MdiParent).FilterDataLogs(startTime, endTime);
        }

        private void DataGrid_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewRow fetchDataGridViewRow = fetchDataGrid.Rows[e.RowIndex];
            Guid foreignKey = ((Guid)fetchDataGridViewRow.Cells[8].Value);
            ((MainForm)MdiParent).ShowRelatedData(foreignKey);
        }

        private void StatusGrid_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewRow statusGridViewRow = statusGrid.Rows[e.RowIndex];
            string message = ((string)statusGridViewRow.Cells[1].Value);
            if (message.ToLower().Contains("internet") || message.ToLower().Contains("loading"))
            {
                string text = string.Empty;
                if (message.ToLower().Contains("internet"))
                    text = "No fetch data associated with internet connection statuses...";
                else
                    text = "No fetch data associated with fetch loading statuses...";
                Program.ShowMessage(this, text, "No Data Available", 5000, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            Guid foreignKey = ((Guid)statusGridViewRow.Cells[3].Value);
            ((MainForm)MdiParent).ShowRelatedData(foreignKey);
            EventHandler eventHandler = new(this.TabControl_SelectedIndexChanged);
            tabControl.SelectedIndexChanged -= eventHandler;
            SwitchToTab(TabDataLogs);
            tabControl.SelectedIndexChanged += eventHandler;
        }

        private void TabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            Mouse.DisableMouse();

            if (tabControl.SelectedTab == TabStatusLogs)
                FilterStatusLogsToDateTime();
            else if (tabControl.SelectedTab == TabDataLogs)
                FilterDataLogsToDate();

            Mouse.EnableMouse();
        }

        private void DtpSimTime_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.LastSimulatedTime = Properties.Settings.Default.SimulateTime ? dtpSimTime.Value : Properties.Settings.Default.LastSimulatedTime;
            Properties.Settings.Default.Save();
        }

        private void DtpJSEUpdateStart_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.JSEUpdateStart = dtpJSEUpdateStart.Value.ToLongTimeString();
            Properties.Settings.Default.Save();
        }

        private void DtpJSEUpdateEnd_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.JSEUpdateEnd = dtpJSEUpdateEnd.Value.ToLongTimeString();
            Properties.Settings.Default.Save();
        }
    }
}
