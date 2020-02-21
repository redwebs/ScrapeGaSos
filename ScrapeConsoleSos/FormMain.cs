using System;
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
        private static string LastStat = string.Empty;
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
            // Update UI
            var status = (ScrapeUserStatus) e.UserState;
            progressBar1.Maximum = status.OfficeCount;
            progressBar1.Increment(status.OfficesSearched);
            tbStatus.Text = $"Candidate: {status.Candidate}";

            var thisStat = $"{status.ElapsedTime} Candidate: {status.Candidate}";

            if (thisStat != LastStat)
            {
                AppendLogBox(thisStat);
                LastStat = thisStat;
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
                        // Some other fatal error
                        tbStatus.Text = $" Job aborted, {lastMsg}, Elapsed Time: {result.ElapsedTime}";
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
                            break;

                        case ScrapeOp.Candidates:

                            _candidateList = (List<CandidateSos>)result.Candidates;
                            break;

                        default:
                            break;
                    }

             //       tbStatus.Text = $" Job finished, {_candidateList.Count} Candidates, Elapsed Time: {result.ElapsedTime}";
               //     tbStatus.Text = $" Job finished, {_electionList.Count} Elections, Elapsed Time: {result.ElapsedTime}";
                }
            }
            //AppendLogBox($"Total Bytes Read: {result.SequenceStat.BytesReceived:###,###}");
            AppendLogBox(tbStatus.Text);
        }

        #endregion Background Worker Events

        // ++++++++++++++++++++++ Background Worker Functions +++++++++++++++++++++

        #region Scrape Testing and Development

        private void TestAdditionalInfo()
        {
            // Test function to get additional info for a single office

            var candidate = new Candidate
            {
                NameId = "11068",
                FilerId = "C2018001171",
                OfficeTypeId = "130",
                OfficeName = "Madison Fain Barton"
            };

            backgroundWorkerScrape.ReportProgress(1, "Begin TestAdditionalInfo : " + Environment.NewLine );

            if (!AdditionalInfo.ReadThePage(candidate))
            {
                backgroundWorkerScrape.ReportProgress(1, AdditionalInfo.AddInfoStatus.LastOpMessage);
                return;
            }
            
            backgroundWorkerScrape.ReportProgress(2, AdditionalInfo.AddInfoStatus.LastOpMessage + Environment.NewLine);
        }

        private void RunSingleQuery()
        {
            // Test function to scrape a single office

            var search = new FormSearch
            {
                ElectionYear = "2018",
                OfficeTypeId = "120",
                OfficeName = "State%20Senate",
                District = "",
                Circuit = "",
                Division = "",
                County = "",
                City = "",
                FilerId = ""
            };

            backgroundWorkerScrape.ReportProgress(1, "Begin new search with FormSearch : " + Environment.NewLine + search);

            if (!UpdateCandidates.ReadFirstPage(search))
            {
                backgroundWorkerScrape.ReportProgress(1, UpdateCandidates.CurrentStatus.LastOpMessage);
                return;
            }

            var candidates = UpdateCandidates.Candidates;

            backgroundWorkerScrape.ReportProgress(2, UpdateCandidates.CurrentStatus.LastOpMessage + Environment.NewLine);

            while (UpdateCandidates.CurrentStatus.LastPageCompleted < UpdateCandidates.CurrentStatus.TotalPages)
            {
                var finished = UpdateCandidates.ReadSubsequentPage(search);
                backgroundWorkerScrape.ReportProgress(2, UpdateCandidates.CurrentStatus.LastOpMessage + Environment.NewLine);
            }
        }

        private static SequenceStatus RunAllQueries(int year, BackgroundWorker bgWorker)
        {
            var path = $"{Utils.GetExecutingDirectory()}\\Data\\OfficeNames-Ids.json";

            var program = new ScrapeSequence(path);

            program.RunAllQueries(year, bgWorker);

            return program.SeqStatus;
        }

        #endregion Scrape Testing and development


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

            var election = ((ComboBoxItem)cboElections.SelectedItem).Value;

            // Array = {0-Operation enum, 1-Year string, 2-Election string}
            var arrObjects = new object[] { operation, year, election};        // Declare the array of objects

            if (backgroundWorkerScrape.IsBusy)
            {
                AppendLogBox("Cannot get candidates, Background worker is busy.");
                return;
            }

            btnLoadElections.Enabled = false;                           // Disable the Start button
            txtLog.Text = "Starting get candidates.";

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

            var path = $"{tbCsvFilePath.Text}\\{Utils.FilenameWithDateTime("CandidatesSos", "csv")}";
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
