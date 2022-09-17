namespace FinancialBoardsFetch.Forms
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.mniMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.mniFetchUpdatesNow = new System.Windows.Forms.ToolStripMenuItem();
            this.mniCancelUpdate = new System.Windows.Forms.ToolStripMenuItem();
            this.mniResetFetchLogDB = new System.Windows.Forms.ToolStripMenuItem();
            this.mniResetResultLogDB = new System.Windows.Forms.ToolStripMenuItem();
            this.MniResetSettings = new System.Windows.Forms.ToolStripMenuItem();
            this.mniAbout = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.mniExit = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.tsProgressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.tsStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.cmiRestoreApp = new System.Windows.Forms.ToolStripMenuItem();
            this.cmiFetchUpdatesNow = new System.Windows.Forms.ToolStripMenuItem();
            this.cmiToggleAutoUpdate = new System.Windows.Forms.ToolStripMenuItem();
            this.cmiAbout = new System.Windows.Forms.ToolStripMenuItem();
            this.cmiExit = new System.Windows.Forms.ToolStripMenuItem();
            this.panel1 = new System.Windows.Forms.Panel();
            this.lblLogging = new System.Windows.Forms.Label();
            this.pbLogging = new System.Windows.Forms.PictureBox();
            this.cmsStatusLogs = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.showAllStatusLogsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cmiFilterStatusToDate = new System.Windows.Forms.ToolStripMenuItem();
            this.cmiExportStatusLogs = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsDataLogs = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mniShowAllDataLogs = new System.Windows.Forms.ToolStripMenuItem();
            this.cmiFilterDataToDate = new System.Windows.Forms.ToolStripMenuItem();
            this.cmiExportDataLogs = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.contextMenuStrip.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbLogging)).BeginInit();
            this.cmsStatusLogs.SuspendLayout();
            this.cmsDataLogs.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip
            // 
            this.menuStrip.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mniMenu});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.MdiWindowListItem = this.mniMenu;
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Padding = new System.Windows.Forms.Padding(10, 3, 0, 3);
            this.menuStrip.Size = new System.Drawing.Size(1084, 26);
            this.menuStrip.TabIndex = 0;
            this.menuStrip.Text = "MenuStrip";
            // 
            // mniMenu
            // 
            this.mniMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mniFetchUpdatesNow,
            this.mniCancelUpdate,
            this.mniResetFetchLogDB,
            this.mniResetResultLogDB,
            this.MniResetSettings,
            this.mniAbout,
            this.toolStripSeparator5,
            this.mniExit});
            this.mniMenu.Image = global::FinancialBoardsFetch.Properties.Resources.Hamburger_Menu;
            this.mniMenu.ImageTransparentColor = System.Drawing.SystemColors.ActiveBorder;
            this.mniMenu.Name = "mniMenu";
            this.mniMenu.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.M)));
            this.mniMenu.Size = new System.Drawing.Size(28, 20);
            // 
            // mniFetchUpdatesNow
            // 
            this.mniFetchUpdatesNow.Name = "mniFetchUpdatesNow";
            this.mniFetchUpdatesNow.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F)));
            this.mniFetchUpdatesNow.Size = new System.Drawing.Size(259, 24);
            this.mniFetchUpdatesNow.Text = "Update All Now";
            this.mniFetchUpdatesNow.Click += new System.EventHandler(this.MniFetchUpdateNow_Click);
            // 
            // mniCancelUpdate
            // 
            this.mniCancelUpdate.Enabled = false;
            this.mniCancelUpdate.Name = "mniCancelUpdate";
            this.mniCancelUpdate.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.mniCancelUpdate.Size = new System.Drawing.Size(259, 24);
            this.mniCancelUpdate.Text = "Cancel Update";
            this.mniCancelUpdate.Click += new System.EventHandler(this.MniCancelUpdate_Click);
            // 
            // mniResetFetchLogDB
            // 
            this.mniResetFetchLogDB.Name = "mniResetFetchLogDB";
            this.mniResetFetchLogDB.Size = new System.Drawing.Size(259, 24);
            this.mniResetFetchLogDB.Text = "Reset Fetch Log DB";
            this.mniResetFetchLogDB.Click += new System.EventHandler(this.MniResetFetchLogDB_Click);
            // 
            // mniResetResultLogDB
            // 
            this.mniResetResultLogDB.Name = "mniResetResultLogDB";
            this.mniResetResultLogDB.Size = new System.Drawing.Size(259, 24);
            this.mniResetResultLogDB.Text = "Reset Result Log DB";
            this.mniResetResultLogDB.Click += new System.EventHandler(this.MniResetResultLogDB_Click);
            // 
            // MniResetSettings
            // 
            this.MniResetSettings.Name = "MniResetSettings";
            this.MniResetSettings.Size = new System.Drawing.Size(259, 24);
            this.MniResetSettings.Text = "Reset Settings";
            this.MniResetSettings.Click += new System.EventHandler(this.MniResetSettings_Click);
            // 
            // mniAbout
            // 
            this.mniAbout.Name = "mniAbout";
            this.mniAbout.Size = new System.Drawing.Size(259, 24);
            this.mniAbout.Text = "About";
            this.mniAbout.Click += new System.EventHandler(this.MniAbout_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(256, 6);
            // 
            // mniExit
            // 
            this.mniExit.Name = "mniExit";
            this.mniExit.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
            this.mniExit.Size = new System.Drawing.Size(259, 24);
            this.mniExit.Text = "E&xit";
            this.mniExit.Click += new System.EventHandler(this.ExitToolsStripMenuItem_Click);
            // 
            // statusStrip
            // 
            this.statusStrip.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsProgressBar,
            this.tsStatusLabel});
            this.statusStrip.Location = new System.Drawing.Point(0, 602);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Padding = new System.Windows.Forms.Padding(1, 0, 15, 0);
            this.statusStrip.Size = new System.Drawing.Size(1084, 25);
            this.statusStrip.TabIndex = 2;
            this.statusStrip.Text = "statusStrip";
            // 
            // tsProgressBar
            // 
            this.tsProgressBar.Name = "tsProgressBar";
            this.tsProgressBar.Size = new System.Drawing.Size(111, 19);
            // 
            // tsStatusLabel
            // 
            this.tsStatusLabel.Name = "tsStatusLabel";
            this.tsStatusLabel.Size = new System.Drawing.Size(75, 20);
            this.tsStatusLabel.Text = "Ready...";
            // 
            // notifyIcon
            // 
            this.notifyIcon.ContextMenuStrip = this.contextMenuStrip;
            this.notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon.Icon")));
            this.notifyIcon.Text = "Financials - Stopped";
            this.notifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.NotifyIcon_MouseDoubleClick);
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cmiRestoreApp,
            this.cmiFetchUpdatesNow,
            this.cmiToggleAutoUpdate,
            this.cmiAbout,
            this.cmiExit});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(237, 124);
            // 
            // cmiRestoreApp
            // 
            this.cmiRestoreApp.Name = "cmiRestoreApp";
            this.cmiRestoreApp.Size = new System.Drawing.Size(236, 24);
            this.cmiRestoreApp.Text = "Restore App";
            this.cmiRestoreApp.Click += new System.EventHandler(this.CmiRestoreApp_Click);
            // 
            // cmiFetchUpdatesNow
            // 
            this.cmiFetchUpdatesNow.Name = "cmiFetchUpdatesNow";
            this.cmiFetchUpdatesNow.Size = new System.Drawing.Size(236, 24);
            this.cmiFetchUpdatesNow.Text = "Fetch Updates Now";
            this.cmiFetchUpdatesNow.Click += new System.EventHandler(this.CmiFetchUpdatesNow_Click);
            // 
            // cmiToggleAutoUpdate
            // 
            this.cmiToggleAutoUpdate.Name = "cmiToggleAutoUpdate";
            this.cmiToggleAutoUpdate.Size = new System.Drawing.Size(236, 24);
            this.cmiToggleAutoUpdate.Text = "Start Auto-update";
            this.cmiToggleAutoUpdate.Click += new System.EventHandler(this.CmiToggleAutoUpdate_Click);
            // 
            // cmiAbout
            // 
            this.cmiAbout.Name = "cmiAbout";
            this.cmiAbout.Size = new System.Drawing.Size(236, 24);
            this.cmiAbout.Text = "About";
            // 
            // cmiExit
            // 
            this.cmiExit.Name = "cmiExit";
            this.cmiExit.Size = new System.Drawing.Size(236, 24);
            this.cmiExit.Text = "Exit";
            this.cmiExit.Click += new System.EventHandler(this.CmiExit_Click);
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Controls.Add(this.lblLogging);
            this.panel1.Controls.Add(this.pbLogging);
            this.panel1.Location = new System.Drawing.Point(884, 602);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(200, 25);
            this.panel1.TabIndex = 4;
            // 
            // lblLogging
            // 
            this.lblLogging.AutoSize = true;
            this.lblLogging.Location = new System.Drawing.Point(32, 3);
            this.lblLogging.Name = "lblLogging";
            this.lblLogging.Size = new System.Drawing.Size(106, 20);
            this.lblLogging.TabIndex = 1;
            this.lblLogging.Text = "Logging: On";
            // 
            // pbLogging
            // 
            this.pbLogging.Image = global::FinancialBoardsFetch.Properties.Resources.Log_Green;
            this.pbLogging.Location = new System.Drawing.Point(3, 4);
            this.pbLogging.Name = "pbLogging";
            this.pbLogging.Size = new System.Drawing.Size(23, 19);
            this.pbLogging.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pbLogging.TabIndex = 0;
            this.pbLogging.TabStop = false;
            // 
            // cmsStatusLogs
            // 
            this.cmsStatusLogs.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.cmsStatusLogs.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showAllStatusLogsToolStripMenuItem,
            this.cmiFilterStatusToDate,
            this.cmiExportStatusLogs});
            this.cmsStatusLogs.Name = "cmsStatusLogs";
            this.cmsStatusLogs.Size = new System.Drawing.Size(225, 82);
            // 
            // showAllStatusLogsToolStripMenuItem
            // 
            this.showAllStatusLogsToolStripMenuItem.Name = "showAllStatusLogsToolStripMenuItem";
            this.showAllStatusLogsToolStripMenuItem.Size = new System.Drawing.Size(224, 26);
            this.showAllStatusLogsToolStripMenuItem.Text = "Show All Status Logs";
            this.showAllStatusLogsToolStripMenuItem.Click += new System.EventHandler(this.ShowAllStatusLogsToolStripMenuItem_Click);
            // 
            // cmiFilterStatusToDate
            // 
            this.cmiFilterStatusToDate.Name = "cmiFilterStatusToDate";
            this.cmiFilterStatusToDate.Size = new System.Drawing.Size(224, 26);
            this.cmiFilterStatusToDate.Text = "Filter To Date";
            this.cmiFilterStatusToDate.Click += new System.EventHandler(this.FilterToDate_Click);
            // 
            // cmiExportStatusLogs
            // 
            this.cmiExportStatusLogs.Name = "cmiExportStatusLogs";
            this.cmiExportStatusLogs.Size = new System.Drawing.Size(224, 26);
            this.cmiExportStatusLogs.Text = "Export Status Logs";
            this.cmiExportStatusLogs.Click += new System.EventHandler(this.ExportLogs_Click);
            // 
            // cmsDataLogs
            // 
            this.cmsDataLogs.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.cmsDataLogs.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mniShowAllDataLogs,
            this.cmiFilterDataToDate,
            this.cmiExportDataLogs});
            this.cmsDataLogs.Name = "cmsResultLogs";
            this.cmsDataLogs.Size = new System.Drawing.Size(215, 104);
            // 
            // mniShowAllDataLogs
            // 
            this.mniShowAllDataLogs.Name = "mniShowAllDataLogs";
            this.mniShowAllDataLogs.Size = new System.Drawing.Size(214, 26);
            this.mniShowAllDataLogs.Text = "Show All Data Logs";
            this.mniShowAllDataLogs.Click += new System.EventHandler(this.ShowAllDataLogsToolStripMenuItem_Click);
            // 
            // cmiFilterDataToDate
            // 
            this.cmiFilterDataToDate.Name = "cmiFilterDataToDate";
            this.cmiFilterDataToDate.Size = new System.Drawing.Size(214, 26);
            this.cmiFilterDataToDate.Text = "Filter To Date";
            this.cmiFilterDataToDate.Click += new System.EventHandler(this.FilterToDate_Click);
            // 
            // cmiExportDataLogs
            // 
            this.cmiExportDataLogs.Name = "cmiExportDataLogs";
            this.cmiExportDataLogs.Size = new System.Drawing.Size(214, 26);
            this.cmiExportDataLogs.Text = "Export Data Logs";
            this.cmiExportDataLogs.Click += new System.EventHandler(this.ExportLogs_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1084, 627);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.menuStrip);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.IsMdiContainer = true;
            this.MainMenuStrip = this.menuStrip;
            this.Margin = new System.Windows.Forms.Padding(5);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "NRA Financials Fetch";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MDIParent_FormClosing);
            this.Shown += new System.EventHandler(this.MDIParent_Shown);
            this.Resize += new System.EventHandler(this.MDIParent_Resize);
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.contextMenuStrip.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbLogging)).EndInit();
            this.cmsStatusLogs.ResumeLayout(false);
            this.cmsDataLogs.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion


        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem mniMenu;
        private System.Windows.Forms.ToolStripMenuItem mniExit;
        private System.Windows.Forms.ToolStripMenuItem mniFetchUpdatesNow;
        public System.Windows.Forms.ToolStripMenuItem mniCancelUpdate;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripProgressBar tsProgressBar;
        private System.Windows.Forms.ToolStripStatusLabel tsStatusLabel;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem cmiRestoreApp;
        private System.Windows.Forms.ToolStripMenuItem cmiFetchUpdatesNow;
        private System.Windows.Forms.ToolStripMenuItem cmiToggleAutoUpdate;
        private System.Windows.Forms.ToolStripMenuItem cmiAbout;
        private System.Windows.Forms.ToolStripMenuItem cmiExit;
        private System.Windows.Forms.ToolStripMenuItem MniResetSettings;
        private System.Windows.Forms.ToolStripMenuItem mniAbout;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label lblLogging;
        private System.Windows.Forms.PictureBox pbLogging;
        private System.Windows.Forms.ToolStripMenuItem mniResetFetchLogDB;
        private System.Windows.Forms.ToolStripMenuItem mniResetResultLogDB;
        internal System.Windows.Forms.ContextMenuStrip cmsStatusLogs;
        internal System.Windows.Forms.ContextMenuStrip cmsDataLogs;
        private System.Windows.Forms.ToolStripMenuItem showAllStatusLogsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cmiFilterStatusToDate;
        private System.Windows.Forms.ToolStripMenuItem cmiExportStatusLogs;
        private System.Windows.Forms.ToolStripMenuItem mniShowAllDataLogs;
        private System.Windows.Forms.ToolStripMenuItem cmiFilterDataToDate;
        private System.Windows.Forms.ToolStripMenuItem cmiExportDataLogs;
    }
}
