using ClosedXML.Excel;

using FinancialBoardsFetch.Modules;

using MartinCostello.SqlLocalDb;

using Microsoft.Win32;

using Nager.Date;
using Nager.Date.PublicHolidays;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace FinancialBoardsFetch.Forms
{
    public partial class MainForm : Form
    {
        private readonly Canvas Canvas;

#if !DEBUG
        private bool Restarting = false;
#endif
        private readonly string NL = Environment.NewLine;
        private readonly string DL = Environment.NewLine + Environment.NewLine;

        private readonly Mouse Mouse = new();

        public string NotifyIconText
        {
            get { return notifyIcon.Text; }
            set
            {
                notifyIcon.Text = value;
            }
        }

        public string AutoUpdateText
        {
            set { cmiToggleAutoUpdate.Text = value; }
        }

        private DataGridView StatusGridView { get => Canvas.statusGrid; }
        private DataGridView FDataGridView { get => Canvas.fetchDataGrid; }

        public MainForm()
        {
            InitializeComponent();

#if !DEBUG
            Restarting = false;
#endif

            Canvas = new Canvas(this);
        }

        private void MDIParent_Shown(object sender, EventArgs e)
        {
            Canvas.Show();
            ShowOnSecondaryMonitor();

            if (Properties.Settings.Default.StartMinimized)
                MinimizeApp();

            if (StartLocalDB())
                Program.ShowMessage(this, "LocalDB is running...", "LocalDB State", 10000, MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                MessageBox.Show("Failed to launch LocalDB", "LocalDB State", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        internal bool StartLocalDB()
        {
            using SqlLocalDbApi localDB = new();

            ISqlLocalDbInstanceInfo instance = localDB.GetOrCreateInstance("MSSQLLocalDB");
            ISqlLocalDbInstanceManager manager = instance.Manage();

            try
            {
                if (!instance.IsRunning)
                {
                    Program.ShowMessage(this, "Starting LocalDB instance", "LocalDB State", 3000, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    manager.Start();
                }

                using Microsoft.Data.SqlClient.SqlConnection connection = instance.CreateConnection();
                connection.Open();
                connection.Close();
                //manager.Stop();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error Starting LocalDB", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return false;
        }

        void ShowOnSecondaryMonitor()
        {
            Screen[] screens = Screen.AllScreens;
            SetFormLocation(this, screens[^1]);
            WindowState = FormWindowState.Maximized;
        }

        public void UpdateProgressBar(ProgressBarStyle pbs)
        {
            if (statusStrip.InvokeRequired)
                statusStrip.Invoke(new MethodInvoker(delegate { tsProgressBar.Style = pbs; }));
            else
                tsProgressBar.Style = pbs;
        }

        public void UpdateStatus(string status)
        {
            if (statusStrip.InvokeRequired)
                statusStrip.Invoke(new MethodInvoker(delegate { tsStatusLabel.Text = status; }));
            else
                tsStatusLabel.Text = status;
        }

        private static void SetFormLocation(Form form, Screen screen)
        {
            form.StartPosition = FormStartPosition.Manual;
            Rectangle bounds = screen.Bounds;
            form.SetBounds(bounds.X, bounds.Y, bounds.Width, bounds.Height);
        }

        private void ExitToolsStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void MniFetchUpdateNow_Click(object sender, EventArgs e)
        {
            bool AfterReset()
            {
                TimeSpan start = new(04, 50, 0);
                TimeSpan end = new(09, 0, 0);
                TimeSpan now = Properties.Settings.Default.SimulateTime ? Canvas.dtpSimTime.Value.TimeOfDay : DateTime.Now.TimeOfDay;
                return now >= start && now <= end;
            }
            if (AfterReset())
            {
                Program.ShowMessage(this, "The financial boards have been reset by Iress by now. Updating manually will reset their values to zero.", "Warning!", 30000, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult result = MessageBox.Show("Do you want to manually update the selected fetches, even though it could reset their values to zero?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                    Canvas.UpdateAll(true);
                else
                    Canvas.UpdateAll();
            }
            else
                Canvas.UpdateAll();
        }

        private void MniCancelUpdate_Click(object sender, EventArgs e)
        {
            Canvas.CancelAll();
            mniCancelUpdate.Enabled = false;
        }

        public static bool MarketsOpen()
        {
            int hourOpen = int.Parse(Properties.Settings.Default.JSEUpdateStart[..2]);
            int minuteOpen = int.Parse(Properties.Settings.Default.JSEUpdateStart.Substring(3, 2));
            int hourClose = int.Parse(Properties.Settings.Default.JSEUpdateEnd[..2]);
            int minuteClose = int.Parse(Properties.Settings.Default.JSEUpdateEnd.Substring(3, 2));

            TimeSpan marketsOpen = new(hourOpen, minuteOpen, 0);
            TimeSpan marketsClosed = new(hourClose, minuteClose, 0);
            TimeSpan now = DateTime.Today.TimeOfDay;
            return ((now >= marketsOpen && now <= marketsClosed) && !(DateSystem.IsWeekend(DateTime.Today.Date, CountryCode.ZA)) && !(DateSystem.IsPublicHoliday(DateTime.Today.Date, CountryCode.ZA)));
        }

        private void MniResetSettings_Click(object sender, EventArgs e)
        {
            DialogResult result;
            result = MessageBox.Show("Are you sure you want to reset to default settings?", "Reset Settings?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
                Properties.Settings.Default.Reset();
            else
                return;
            result = MessageBox.Show("Application settings have been reset to defaults. You might have to restart the application for some changes to take effect.\n\nWould you like to restart now?", "Restart Now?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
#if !DEBUG
                Restarting = true;
#endif
                Application.Restart();
            }
        }

        private void MDIParent_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized && Properties.Settings.Default.MinimizeToTray)
                MinimizeApp();
        }

        private void MinimizeApp()
        {
            if (InvokeRequired)
                Invoke((MethodInvoker)(() => WindowState = FormWindowState.Minimized));
            else
                WindowState = FormWindowState.Minimized;
            if (InvokeRequired)
                Invoke((MethodInvoker)(() => Hide()));
            else
                Hide();
            notifyIcon.Visible = true;
        }

        public void RestoreFromTray()
        {
            if (InvokeRequired)
                Invoke((MethodInvoker)(() => WindowState = FormWindowState.Maximized));
            else
                WindowState = FormWindowState.Maximized;
            if (InvokeRequired)
                Invoke((MethodInvoker)(() => Show()));
            else
                Show();
            notifyIcon.Visible = false;

            SetForegroundWindow(Process.GetCurrentProcess().MainWindowHandle);
        }

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        private void NotifyIcon_MouseDoubleClick(object sender, MouseEventArgs e) => RestoreFromTray();

        private void MniAbout_Click(object sender, EventArgs e)
        {
            AboutBox about = new();
            about.ShowDialog();
        }

        private void CmiRestoreApp_Click(object sender, EventArgs e) => RestoreFromTray();

        private void CmiFetchUpdatesNow_Click(object sender, EventArgs e) => Canvas.UpdateAll();

        private void CmiToggleAutoUpdate_Click(object sender, EventArgs e)
        {
            if (Canvas.AutoFetch)
                Canvas.StopTimer();
            else
                Canvas.StartTimer();
        }

        private void CmiExit_Click(object sender, EventArgs e) => Application.Exit();

        private void MDIParent_FormClosing(object sender, FormClosingEventArgs e)
        {
#if !DEBUG
            if (!Restarting)
            {
                MessageBox.Show("You are about to close the Financials Fetch!\n\nAll Financials data will not be updated until you run this app again. Rather minimize it to the system tray if it's obstructing your view.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                e.Cancel = (DialogResult.No == MessageBox.Show("Are you sure you want to close the Financials run fetch?\n\n", "Quit Financials Fetch?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2));
            }
#endif
            notifyIcon.Visible = false;
        }

        private void MniResetFetchLogDB_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete all fetch status logs?", "Confirm Delete", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question) == DialogResult.Yes)
                ResetTable("FetchStatusTable");
        }

        private void MniResetResultLogDB_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete all fetch data logs?", "Confirm Delete", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question) == DialogResult.Yes)
                ResetTable("FetchDataTable");
        }

        private void ResetTable(string tableName)
        {
            using SqlConnection connection = new(ConnString());
            string query = $@"DELETE FROM {tableName};";

            using SqlCommand command = new(query, connection);

            try
            {
                connection.Open();
                command.ExecuteNonQuery();
                string readableTableName = tableName == "FetchStatusTable" ? "Fetch Status Table" : "Fetch Result Table";
                AutoClosingMessageBox.Show($"All records were removed successfully from {readableTableName}.", "", 5000);

                if (tableName == "FetchDataTable")
                    Canvas.FilterDataLogsToDate();
                else
                    Canvas.FilterStatusLogsToDateTime();
            }
            catch (SqlException ex)
            {
                AutoClosingMessageBox.Show($"Error removing table records.{Environment.NewLine}Details: " + ex.ToString(), "Error!", 10000);
            }
            finally
            {
                connection.Close();
            }
        }

        private static void RefreshDataGridView(DataGridView dgv, DataTable dt, string sc, ListSortDirection lsd = ListSortDirection.Descending)
        {
            if (dgv.InvokeRequired)
            {
                dgv.Invoke(new MethodInvoker(delegate
                    {
                        dgv.DataSource = dt;
                        dgv.Sort(dgv.Columns[sc], lsd);
                        if (sc.Contains("Date"))
                            dgv.Columns[sc].DefaultCellStyle.Format = "yyyy-MM-dd HH:mm:ss";
                    }));
            }
            else
            {
                dgv.DataSource = dt;
                dgv.Sort(dgv.Columns[sc], lsd);
                if (sc.Contains("Date"))
                    dgv.Columns[sc].DefaultCellStyle.Format = "yyyy-MM-dd HH:mm:ss";
            }
        }

        internal void LogFetchData(Guid guid, string date, List<Resource> resources)
        {
            Mouse.EnableMouse();
            using SqlConnection connection = new(ConnString());
            string query = $@"INSERT INTO FetchDataTable (Fk_Id, ResourceType, Code, ShortName, Time, Date, Trade, Points, Perc) VALUES (@Fk_Id, @ResourceType, @Code, @ShortName, @Time, @Date, @Trade, @Points, @Perc)";

            using SqlCommand command = new(query, connection);
            foreach (Resource resource in resources)
            {
                command.Parameters.AddWithValue("@Fk_Id", guid);
                command.Parameters.AddWithValue("@ResourceType", Resource.MyResourceType.ToString());
                command.Parameters.AddWithValue("@Code", resource.Code);
                command.Parameters.AddWithValue("@ShortName", resource.Sname);
                command.Parameters.AddWithValue("@Time", resource.Time);
                command.Parameters.AddWithValue("@Date", date);
                command.Parameters.AddWithValue("@Trade", resource.Trade);
                command.Parameters.AddWithValue("@Points", resource.Points);
                command.Parameters.AddWithValue("@Perc", resource.Perc);

                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    Program.ShowMessage(this, $"Error logging {Resource.MyResourceType} fetch data.{NL}Reason:{NL}{ex.Message}{DL}{ex.StackTrace}", "Error!", 30000, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    connection.Close();
                    command.Parameters.Clear();
                    Canvas.FilterDataLogsToDate();
                    Mouse.EnableMouse();
                }
            }
        }

        internal void LogFetchStatus(Guid statusId, FetchStatus fetchStatus, string msg)
        {
            Mouse.DisableMouse();
            using SqlConnection connection = new(ConnString());
            string query = $@"INSERT INTO FetchStatusTable (StatusId, FetchStatus, Message, DateLogged) VALUES (@StatusId, @FetchStatus, @Message, @DateLogged)";

            using SqlCommand command = new(query, connection);
            command.Parameters.AddWithValue("@StatusId", statusId);
            command.Parameters.AddWithValue("@FetchStatus", fetchStatus.ToString());
            command.Parameters.AddWithValue("@Message", msg);
            command.Parameters.AddWithValue("@DateLogged", DateTime.Now);

            try
            {
                connection.Open();
                command.ExecuteNonQuery();
                //MessageBox.Show($"Success logging msg => {msg}", "Success");
            }
            catch (SqlException ex)
            {
                //Program.ShowMessage(this, $"Error logging fetch status.{Environment.NewLine}Details:{ex.Message}{NL}{ex.StackTrace}", "Error!", 10000, MessageBoxButtons.OK, MessageBoxIcon.Error);
                MessageBox.Show($"Error logging fetch status.{Environment.NewLine}Details:{ex.Message}{NL}{ex.StackTrace}", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                connection.Close();

                Canvas.FilterStatusLogsToDateTime();
                Mouse.EnableMouse();
            }
        }

        //public string ConnString() => $@"Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename={Path.Combine(Directory.GetCurrentDirectory(), @"AppDB\FetchResultDB.mdf")};Integrated Security=True;";

        public static string ConnString()
        {
            string sCurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string sFile = System.IO.Path.Combine(sCurrentDirectory, @"AppDB\FetchResultDB.mdf");
            string sFilePath = Path.GetFullPath(sFile);
            return $"Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename={sFilePath};Integrated Security=True";
        }

        internal void ShowRelatedStatus(Guid statusKey)
        {
            Mouse.DisableMouse();
            using SqlConnection connection = new(ConnString());

            string query = "SELECT FetchStatus [Fetch Status], Message [Message], DateLogged [Date Logged], [StatusId] FROM FetchStatusTable WHERE StatusId = @StatusId";

            using SqlCommand cmdSelect = new()
            {
                Connection = connection,
                CommandText = query
            };
            cmdSelect.Parameters.AddWithValue("@StatusId", statusKey);
            try
            {
                cmdSelect.Connection.Open();

                using SqlDataAdapter sda = new(cmdSelect);

                using DataTable dt = new();
                sda.Fill(dt);

                RefreshDataGridView(StatusGridView, dt, "Date Logged", ListSortDirection.Descending);
            }
            catch (Exception ex)
            {
                Program.ShowMessage(this, $"Error querying related status log.{NL}Details: {ex.Message}{DL}Stacktrace: {ex.StackTrace}", "Error Querying Status Log", 30000, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                cmdSelect.Connection.Close();
                Mouse.EnableMouse();
            }
        }

        internal void ShowRelatedData(Guid foreignKey)
        {
            Mouse.DisableMouse();
            using SqlConnection connection = new(ConnString());

            string query = "SELECT ResourceType [Resource Type], [Code], ShortName [Short Name], [Time], [Date], [Trade], [Points], [Perc], Fk_Id [Foreign Key], DataItemId [Id] FROM FetchDataTable WHERE Fk_Id = @ForeignKey";

            using SqlCommand cmdSelect = new()
            {
                Connection = connection,
                CommandText = query
            };
            cmdSelect.Parameters.AddWithValue("@ForeignKey", foreignKey);
            try
            {
                cmdSelect.Connection.Open();

                using SqlDataAdapter sda = new(cmdSelect);

                using DataTable dt = new();
                sda.Fill(dt);

                RefreshDataGridView(FDataGridView, dt, "Id", ListSortDirection.Descending);
            }
            catch (Exception ex)
            {
                Program.ShowMessage(this, $"Error querying related data log.{Environment.NewLine}Details: {ex}", "Error Querying Data Log", 30000, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                cmdSelect.Connection.Close();
                Mouse.EnableMouse();
            }
        }

        internal void ShowAllStatusLogs()
        {
            Mouse.DisableMouse();
            using SqlConnection connection = new(ConnString());

            using SqlCommand cmdSelect = new("SELECT FetchStatus [Fetch Status], Message [Message], DateLogged [Date Logged], StatusId [Id] FROM FetchStatusTable", connection);

            try
            {
                cmdSelect.Connection.Open();

                using SqlDataAdapter sda = new(cmdSelect);

                using DataTable dt = new();
                sda.Fill(dt);

                RefreshDataGridView(StatusGridView, dt, "Date Logged", ListSortDirection.Descending);
            }
            catch (Exception ex)
            {
                Program.ShowMessage(this, $"Error querying status logs.{Environment.NewLine}Details: {ex}", "Error Querying status Logs", 30000, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                cmdSelect.Connection.Close();
                Mouse.EnableMouse();
            }
        }

        internal void ShowAllDataLogs()
        {
            Mouse.DisableMouse();

            using SqlConnection connection = new(ConnString());

            using SqlCommand cmdSelect = new("SELECT ResourceType [Resource Type], [Code], ShortName [Short Name], [Time], [Date], [Trade], [Points], [Perc], Fk_Id [Foreign Key], DataItemId [Id] FROM FetchDataTable", connection);

            try
            {
                cmdSelect.Connection.Open();

                using SqlDataAdapter sda = new(cmdSelect);

                using DataTable dt = new();
                sda.Fill(dt);

                RefreshDataGridView(FDataGridView, dt, "Id", ListSortDirection.Descending);
            }
            catch (Exception ex)
            {
                Program.ShowMessage(this, $"Error querying data logs.{Environment.NewLine}Details: {ex}", "Error Querying Data Logs", 30000, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                cmdSelect.Connection.Close();
                Mouse.EnableMouse();
            }
        }

        internal void FilterStatusLogs(DateTime startTime, DateTime endTime)
        {
            Mouse.DisableMouse();
            using SqlConnection connection = new(ConnString());

            string query = "SELECT FetchStatus [Fetch Status], Message [Message], DateLogged [Date Logged], StatusId [Status Id] FROM FetchStatusTable WHERE \"DateLogged\" BETWEEN @StartTime AND @EndTime";

            using SqlCommand cmdSelect = new()
            {
                Connection = connection,
                CommandText = query
            };

            cmdSelect.Parameters.AddWithValue("@StartTime", ToDateTime2(startTime));
            cmdSelect.Parameters.AddWithValue("@EndTime", ToDateTime2(endTime));

            try
            {
                cmdSelect.Connection.Open();

                using SqlDataAdapter sda = new(cmdSelect);

                using DataTable dt = new();
                sda.Fill(dt);

                RefreshDataGridView(StatusGridView, dt, "Date Logged", ListSortDirection.Descending);
            }
            catch (Exception ex)
            {
                Program.ShowMessage(this, $"Error querying fetch log.{Environment.NewLine}Details: {ex}", "Error Querying Fetch Log", 30000, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                cmdSelect.Connection.Close();
            }
            Mouse.EnableMouse();
        }

        internal void FilterDataLogs(DateTime startDate, DateTime endDate)
        {
            using SqlConnection connection = new(ConnString());

            string query = "SELECT ResourceType [Resource Type], [Code], ShortName [Short Name], [Time], [Date], [Trade], [Points], [Perc], Fk_Id [Foreign Key], DataItemId [Id] FROM FetchDataTable WHERE TRY_PARSE(Date as date) BETWEEN @StartDate AND @EndDate";

            using SqlCommand cmdSelect = new()
            {
                Connection = connection,
                CommandText = query
            };
            cmdSelect.Parameters.AddWithValue("@StartDate", ToDateTime2(startDate));
            cmdSelect.Parameters.AddWithValue("@EndDate", ToDateTime2(endDate));

            try
            {
                cmdSelect.Connection.Open();

                using SqlDataAdapter sda = new(cmdSelect);

                using DataTable dt = new();
                sda.Fill(dt);

                RefreshDataGridView(FDataGridView, dt, "Id", ListSortDirection.Descending);
            }
            catch (Exception ex)
            {
                Program.ShowMessage(this, $"Error querying fetch log.{Environment.NewLine}Details: {ex}", "Error Querying Fetch Log", 30000, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                cmdSelect.Connection.Close();
            }
        }

        internal void FilterDataLogs(TimeSpan startTime, TimeSpan endTime)
        {
            using SqlConnection connection = new(ConnString());

            string query = "SELECT ResourceType [Resource Type], [Code], ShortName [Short Name], [Time], [Date], [Trade], [Points], [Perc], Fk_Id [Foreign Key], DataItemId [Id] FROM FetchDataTable WHERE TRY_PARSE(Time as time) BETWEEN @StartTime AND @EndTime";

            using SqlCommand cmdSelect = new()
            {
                Connection = connection,
                CommandText = query
            };
            cmdSelect.Parameters.AddWithValue("@StartTime", startTime);
            cmdSelect.Parameters.AddWithValue("@EndTime", endTime);
            try
            {
                cmdSelect.Connection.Open();

                using SqlDataAdapter sda = new(cmdSelect);

                using DataTable dt = new();
                sda.Fill(dt);

                RefreshDataGridView(FDataGridView, dt, "Date Logged", ListSortDirection.Descending);
            }
            catch (Exception ex)
            {
                Program.ShowMessage(this, $"Error querying fetch log.{Environment.NewLine}Details: {ex}", "Error Querying Fetch Log", 30000, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                cmdSelect.Connection.Close();
            }
        }

        private static string ToDateTime2(DateTime dateTime) => $"{dateTime.ToShortDateString().Replace("/", "-")}T{dateTime.ToLongTimeString()}.0000";

        private void ShowAllStatusLogsToolStripMenuItem_Click(object sender, EventArgs e) => ShowAllStatusLogs();

        private void ShowAllDataLogsToolStripMenuItem_Click(object sender, EventArgs e) => ShowAllDataLogs();

        private void FilterToDate_Click(object sender, EventArgs e)
        {
            Mouse.DisableMouse();
            ToolStripMenuItem menuItem = sender as ToolStripMenuItem;

            switch (menuItem.Name)
            {
                case "cmiFilterStatusToDate":
                    {
                        Canvas.mcStatusFrom.SetDate(DateTime.Now.Date);
                        Canvas.mcStatusTo.SetDate(DateTime.Now.Date);
                        Canvas.FilterStatusLogsToDateTime();
                        break;
                    }
                case "cmiFilterDataToDate":
                    {
                        Canvas.mcDataLogsFrom.SetDate(DateTime.Now.Date);
                        Canvas.mcDataLogsTo.SetDate(DateTime.Now.Date);
                        Canvas.FilterDataLogsToDate();
                        break;
                    }
            }
            Mouse.EnableMouse();
        }

        private void ExportLogs_Click(object sender, EventArgs e)
        {
            Mouse.DisableMouse();
            ToolStripMenuItem menuItem = ((ToolStripMenuItem)sender);

            switch (menuItem.Name)
            {
                case "cmiExportStatusLogs":
                    {
                        ExportLogs(StatusGridView);
                        break;
                    }
                case "cmiExportDataLogs":
                    {
                        ExportLogs(FDataGridView);
                        break;
                    }
            }
            Mouse.EnableMouse();
        }

        private void ExportLogs(DataGridView dgv)
        {
            Mouse.DisableMouse();

            string typeOfLogs = dgv.Name == "dataGridView1" ? "Fetch Status logs" : "Fetch Data logs";

            //Creating DataTable
            using DataTable dt = new()
            {
                TableName = typeOfLogs
            };

            //Adding the Columns
            foreach (DataGridViewColumn column in dgv.Columns)
            {
                dt.Columns.Add(column.HeaderText, column.ValueType);
            }

            //Adding the Rows
            foreach (DataGridViewRow row in dgv.Rows)
            {
                if (dt.Rows.Count == dgv.Rows.Count - 1)
                    break;
                dt.Rows.Add();
                foreach (DataGridViewCell cell in row.Cells)
                {
                    dt.Rows[^1][cell.ColumnIndex] = cell.Value.ToString();
                }
            }

            using SaveFileDialog sfd = new()
            {
                Filter = "XML file (*.xml)|*.xml|Excel Spreadsheet (*.xlsx)|*.xlsx",
                AddExtension = true,
                AutoUpgradeEnabled = true,
                CheckPathExists = true,
                CreatePrompt = false,
                OverwritePrompt = true,
                SupportMultiDottedExtensions = true,
                InitialDirectory = Properties.Settings.Default.SaveFolderPath,
                Title = $"Export {typeOfLogs}"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                if (sfd.FilterIndex == 1)
                {
                    try
                    {
                        dt.WriteXml(sfd.FileName, XmlWriteMode.WriteSchema, true);

                        Program.ShowMessage(this, text: $"Successfully exported {dgv.RowCount} {typeOfLogs} to {sfd.FileName}", caption: "Export XML Succesful", timeout: 30000, buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error saving workbook.{Environment.NewLine}{ex.Message}", "Export Spreadsheet Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        Mouse.EnableMouse();
                    }
                }
                else
                {
                    using XLWorkbook wb = new();
                    try
                    {
                        wb.Worksheets.Add(dt, "Customers");
                        wb.SaveAs(sfd.FileName);

                        Program.ShowMessage(this, text: $"Successfully exported {dgv.RowCount} {typeOfLogs} to {sfd.FileName}", caption: "Export Query Succesful", timeout: 30000, buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error saving workbook. Details:{Environment.NewLine}{ex.Message}", "Export Query Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        Mouse.EnableMouse();
                    }
                }
            }

            
        }
    }
}
