using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Jayrock.Json;

namespace TransmissionRemoteDotnet
{
    public partial class StatsDialog : Form
    {
        private static StatsDialog instance = null;
        private static readonly object padlock = new object();

        public static StatsDialog Instance
        {
            get
            {
                lock (padlock)
                {
                    if (!IsActive())
                    {
                        instance = new StatsDialog();
                    }
                }
                return instance;
            }
        }

        private StatsDialog()
        {
            InitializeComponent();
        }

        private void CloseFormButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        public static void CloseIfOpen()
        {
            if (IsActive())
            {
                instance.Close();
            }
        }

        private static bool IsActive()
        {
            return instance != null && !instance.IsDisposed;
        }

        public static void StaticUpdateStats(JsonObject stats)
        {
            if (IsActive())
            {
                instance.UpdateStats(stats);
            }
        }

        public void UpdateStats(JsonObject stats)
        {
            try
            {
                JsonObject sessionstats = (JsonObject)stats["current-stats"];
                JsonObject cumulativestats = (JsonObject)stats["cumulative-stats"];
                TimeSpan ts = TimeSpan.FromSeconds(Toolbox.ToInt(sessionstats["secondsActive"]));
                downloadedBytesValue1.Text = Toolbox.GetFileSize(Toolbox.ToLong(sessionstats["downloadedBytes"]));
                uploadedBytesValue1.Text = Toolbox.GetFileSize(Toolbox.ToLong(sessionstats["uploadedBytes"]));
                filesAddedValue1.Text = ((JsonNumber)sessionstats["filesAdded"]).ToString();
                sessionCountValue1.Text = ((JsonNumber)sessionstats["sessionCount"]).ToString();
                secondsActiveValue1.Text = Toolbox.FormatTimespanLong(ts);
                ts = TimeSpan.FromSeconds(Toolbox.ToInt(cumulativestats["secondsActive"]));
                downloadedBytesValue2.Text = Toolbox.GetFileSize(Toolbox.ToLong(cumulativestats["downloadedBytes"]));
                uploadedBytesValue2.Text = Toolbox.GetFileSize(Toolbox.ToLong(cumulativestats["uploadedBytes"]));
                filesAddedValue2.Text = ((JsonNumber)cumulativestats["filesAdded"]).ToString();
                sessionCountValue2.Text = ((JsonNumber)cumulativestats["sessionCount"]).ToString();
                secondsActiveValue2.Text = ts.Ticks < 0 ? OtherStrings.UnknownNegativeResult : Toolbox.FormatTimespanLong(ts);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Unable to load stats data", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }

        private void StatsWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = CommandFactory.Request((JsonObject)e.Argument);
        }

        private void StatsWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ((ICommand)e.Result).Execute();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!statsWorker.IsBusy)
            {
                statsWorker.RunWorkerAsync(Requests.SessionStats());
            }
        }

        private void StatsDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            timer1.Enabled = false;
        }

        private void StatsDialog_Load(object sender, EventArgs e)
        {
            statsWorker.RunWorkerAsync(Requests.SessionStats());
            timer1.Enabled = true;
        }
    }
}