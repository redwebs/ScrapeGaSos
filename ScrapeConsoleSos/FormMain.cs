﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using log4net.Config;
using PageScrapeSos;


namespace ScrapeConsoleSos
{
    // GA Secretary of State Web Scrape
    // Search form at https://elections.sos.ga.gov/GAElection/CandidateDetails
    public partial class FormMain : Form
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private const string DnsNotResolved = "The remote name could not be resolved";
        private static string _lastStat = string.Empty;
        private static string _currentElection = string.Empty;
        private List<CandidateSos> _candidateList;
        private List<Election> _electionList;

        #region Form Init

        // ++++++++++++++++++++++ Form declaration and load ++++++++++++++++++++++

        public FormMain()
        {
            InitializeComponent();
            XmlConfigurator.Configure();
            Log.Debug("-------------- Started program ScrapeConsole"); 

        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            // Tell backgroundWorker to support cancellations
            backgroundWorkerScrape.WorkerSupportsCancellation = true;

            // Tell backgroundWorker to report progress
            backgroundWorkerScrape.WorkerReportsProgress = true;

            // Fill combo with years
            var thisYear = DateTime.Now.Year;

            for (var year = thisYear - 1; year < thisYear + 3; year++)
            {
                cboYear.Items.Add(year);
            }

            cboYear.SelectedItem = thisYear;

        }

        #endregion Form Init

        #region Background Worker Events

        // +++++++++++++++++++++++ Background worker events +++++++++++++++++++++++

        private void BackgroundWorkerScrape_DoWork(object sender, DoWorkEventArgs e)
        {
            // Array = {0-Operation enum, 1-Year string, 2-Election string}

            var sendingWorker = (BackgroundWorker)sender;   // Capture the BackgroundWorker that fired the event
            var arrObjects = (object[])e.Argument;          // Collect the array of objects the we received from the main thread

            // Get the input values from inside the objects array, don't forget to cast
            var scrapeOp = (ScrapeOp) arrObjects[0];
            var year = (string)arrObjects[1];
            var electionId = (string)arrObjects[2];
            
            var timer = Stopwatch.StartNew();

            var scrapeResult = new ScrapeResult
            {
                Operation = scrapeOp
            };

            switch (scrapeOp)
            {
                case ScrapeOp.Elections:
                    
                    var formSrchSos = new FormSearchSos(year.ToString());
                    scrapeResult.ScrapeStat = UpdateCandidates.GetElections(formSrchSos);  // returns ScrapeStatus

                    break;

                case ScrapeOp.Candidates:
                    var formSrchSosCand = new FormSearchSos(year, electionId );
                    scrapeResult.ScrapeStat = UpdateCandidates.GetCandidates(formSrchSosCand);
                    break;

                default:
                    break;
            }

//            scrapeResult.SequenceStat = seqStatus;
            scrapeResult.ElapsedTime = timer.Elapsed.ToString();
            e.Result = scrapeResult;
        }

        private void BackgroundWorkerScrape_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // Update UI - Not currently used for Sec of State
            var status = (ScrapeUserStatus) e.UserState;
            progressBar1.Maximum = status.OfficeCount;
            progressBar1.Increment(status.OfficesSearched);
            tbStatus.Text = $"Candidate: {status.Candidate}";

            var thisStat = $"{status.ElapsedTime} Candidate: {status.Candidate}";

            if (thisStat != _lastStat)
            {
                AppendLogBox(thisStat);
                _lastStat = thisStat;
            }

            if (!string.IsNullOrEmpty(status.Message))
            {
                AppendLogBox(status.Message);
            }

            if (status.Cancelled)
            {
                AppendLogBox($"User cancelled the scrape.");
            }
        }

        private void BackgroundWorkerScrape_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var result = (ScrapeResult)e.Result;

            if (e.Error != null)
            {
                // An error occurred
                tbStatus.Text = $"An error occurred: {e.Error.Message}";
            }
            else if (e.Cancelled)
            {
                // The process was cancelled
                tbStatus.Text = " Job cancelled.";
            }
            else
            {
                if (!result.ScrapeStat.ScrapeSuccess)
                {
                    var lastMsg = result.ErrorMessage;

                    if (lastMsg.Contains(DnsNotResolved))
                    {
                        tbStatus.Text = $" Job aborted, DNS lookup failed for site, check Internet connection. Elapsed Time: {result.ElapsedTime}";
                    }
                    else
                    {
                        if (result.ScrapeStat.Candidates == null)
                        {
                            tbStatus.Text = $" No candidates found for this election, Elapsed Time: {result.ElapsedTime}";
                            btnStart.Enabled = true;
                            AppendLogBox($" Candidates Job finished, no Candidates, Elapsed Time: {result.ElapsedTime}");
                        }
                        else
                        {
                            // Some other fatal error
                            tbStatus.Text = $" Job aborted, {lastMsg}, Elapsed Time: {result.ElapsedTime}";
                        }
                    }
                }
                else
                {
                    // The process finished
                    switch (result.Operation)
                    {
                        case ScrapeOp.Elections:
                            
                            _electionList = result.ScrapeStat.Elections;
                            cboElections.Items.Clear();

                            foreach (Election election in _electionList)
                            {
                                if (election.ElectionId != "0")
                                {
                                    cboElections.Items.Add(new ComboBoxItem(election.ElectionName, election.ElectionId));
                                }
                            }

                            if (cboElections.Items.Count > 0)
                            {
                                cboElections.SelectedIndex = cboElections.Items.Count - 1;
                            }

                            btnLoadElections.Enabled = true;
                            AppendLogBox($"Elections Job finished, {_electionList.Count - 1} Elections, Elapsed Time: {result.ElapsedTime}");

                            break;

                        case ScrapeOp.Candidates:

                            _candidateList = result.ScrapeStat.Candidates;
                            btnStart.Enabled = true;
                            AppendLogBox($" Candidates Job finished, {_candidateList.Count} Candidates, Elapsed Time: {result.ElapsedTime}");

                            break;

                        default:
                            break;
                    }
                }
            }
        }

        #endregion Background Worker Events

        // +++++++++++++++++++++++ UI Events +++++++++++++++++++++++++++++++++++

        #region UI Events

        private void BtnStart_Click(object sender, EventArgs e)
        {
            var operation = ScrapeOp.Candidates;
            var year = cboYear.SelectedItem.ToString();

            if (cboElections.Items.Count == 0)
            {
                AppendLogBox("Cannot get candidates, no Election selected.");
                return;
            }
            btnStart.Enabled = false;

            var election = ((ComboBoxItem)cboElections.SelectedItem).Value;
            _currentElection = cboElections.Text.Substring(0,5).Replace("/","-");

            // Array = {0-Operation enum, 1-Year string, 2-Election string}
            var arrObjects = new object[] { operation, year, election};        // Declare the array of objects

            if (backgroundWorkerScrape.IsBusy)
            {
                AppendLogBox("Cannot get candidates, Background worker is busy.");
                return;
            }

            btnLoadElections.Enabled = false;                           // Disable the Start button
            AppendLogBox("Starting get candidates.");

            backgroundWorkerScrape.RunWorkerAsync(arrObjects);  // Call the background worker, process on a separate thread
        }

        private void BtnStop_Click(object sender, EventArgs e)
        {
            // Flag the process to be stopped. This will not stop the process. 
            // It will only set the backgroundWorker.CancellationPending property.
            backgroundWorkerScrape.CancelAsync();
            btnStart.Enabled = true;
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtLog.Text = string.Empty;
        }

        private void btnCopyLog_Click(object sender, EventArgs e)
        {
            Clipboard.SetDataObject(txtLog.Text, true);
        }

        private void BtnSaveCsv_Click(object sender, EventArgs e)
        {
            if (_candidateList == null)
            {
                AppendLogBox("Empty candidate list");
                return;
            }
            if (_candidateList.Count == 0)
            {
                AppendLogBox("No candidates in list");
                return;
            }

            var dummy = new CandidateSos();

            var sb = new StringBuilder();

            sb.AppendLine(dummy.CsvHeader());

            foreach (var candidate in _candidateList)
            {
                sb.AppendLine(candidate.ToCsv());
            }

            var path = $"{tbCsvFilePath.Text}\\{Utils.FilenameWithDateTime($"CandSos{_currentElection}_", "csv")}";
            FileHelper.StringToFile(sb, path);

            AppendLogBox($"CSV file written to {path}");
        }

        private void BtnSetPath_Click(object sender, EventArgs e)
        {
            var browser = new FolderBrowserDialog();
            string error;
            tbCsvFilePath.Text = FileHelper.GetFolderName(browser, "", "Select CSV save folder", out error);
            if (string.IsNullOrEmpty(tbCsvFilePath.Text))
            {
                tbCsvFilePath.Text = "C:\\Temp";
            }
        }

        private void btnLoadElections_Click(object sender, EventArgs e)
        {
            var operation = ScrapeOp.Elections;
            var year = (string) cboYear.SelectedItem.ToString();

            // Array = {0-Operation enum, 1-Year string, 2-Election string}
            var arrObjects = new object[] {operation, year, "0"};        // Declare the array of objects

            if (backgroundWorkerScrape.IsBusy)
            {
                AppendLogBox("Cannot get elections, Background worker is busy.");
                return;
            }

            btnLoadElections.Enabled = false;                           // Disable the Start button
            txtLog.Text = "Starting get elections.";

            backgroundWorkerScrape.RunWorkerAsync(arrObjects);  // Call the background worker, process on a separate thread
        }

        #endregion UI Events


        private void AppendLogBox(string str)
        {
            txtLog.Text = $"{str}{Environment.NewLine}{txtLog.Text}";
        }

        private void btnTest_Click(object sender, EventArgs e)
        {

            var sb =ScrapHelp.TestAgility(tbStatus.Text);

            AppendLogBox(sb.ToString());

            FileHelper.StringToFile(sb, @"c:\temp\scrape.txt");
        }
    }
}
