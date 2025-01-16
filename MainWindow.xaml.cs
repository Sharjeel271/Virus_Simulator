using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using System.ComponentModel;
using VirusSimulatorMethod;
using System.Runtime.CompilerServices;

namespace Virus_Simulator_Intepface
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        // Data for initialising the WriteableBitmap
        readonly int boardSize = 1500;
        readonly int dpiX = 100;
        readonly int dpiY = 100;
        readonly int width = 1;
        readonly int height = 1;
        readonly int stride = 5;
        readonly int offset = 0;

        // Colour data for the population
        readonly byte[] colourData = [ 64, 177, 0, 0 ];
        readonly byte[] infectedColour = [0, 0, 255, 0];
        readonly byte[] immuneColour = [255, 0, 0, 0];
        readonly byte[] deadColour = [0, 0, 0, 0];

        // Used to break out of the simulation when the user desires
        bool isSimulationRunning = false;

        // Binding properties
        private string bindingMessage;
        private Visibility submitButtonVisibility = Visibility.Visible;
        private Visibility stopButtonVisibility = Visibility.Hidden;
        private WriteableBitmap image;

        public string BindingMessage
        {
            get { return bindingMessage; } 
            set 
            {
                bindingMessage = value;
                OnPropertyChanged();
            } 
        }

        public Visibility SubmitButtonVisibility
        {
            get { return submitButtonVisibility; }
            set
            {
                submitButtonVisibility = value;
                OnPropertyChanged();
            }
        }

        public Visibility StopButtonVisibility
        {
            get { return stopButtonVisibility; }
            set
            {
                stopButtonVisibility = value;
                OnPropertyChanged();
            }
        }

        public WriteableBitmap Image
        {
            get { return image; }
            set
            {
                image = value;
                OnPropertyChanged();
            }
        }


        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();
            Image = CreateImage();
        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            if ((DataInputSection.InfectionRate.InputValue == null) || (DataInputSection.SurvivalRate.InputValue == null) || (DataInputSection.ImmunityChance.InputValue == null) ||
                (DataInputSection.InfectionRate.InputValue == "") || (DataInputSection.SurvivalRate.InputValue == "") || (DataInputSection.ImmunityChance.InputValue == ""))
            {
                BindingMessage = "Enter a value for all 3 input boxes";
                return;
            }

            UpdateProperties(SimulationStatus.Starting);

            var gameBoard = new Board(boardSize, int.Parse(DataInputSection.InfectionRate.InputValue), int.Parse(DataInputSection.SurvivalRate.InputValue), int.Parse(DataInputSection.ImmunityChance.InputValue), 3);

            var thread = new BackgroundWorker
            {
                WorkerReportsProgress = true
            };

            thread.DoWork += (send, args) =>
            {
                var bp = CreateImage();
                var worker = send as BackgroundWorker;
                worker.WorkerReportsProgress = true;

                while (true)
                {
                    // Ends the simulation if there are no more infected or the user has decided to stop it
                    if ((gameBoard.CurrentCycleInfected.Count == 0) || (!isSimulationRunning))
                    {
                        break;
                    }

                    gameBoard.RunSimulation();
                    var bpFrozen = bp.GetCurrentValueAsFrozen();

                    UpdateImage(gameBoard, bp);

                    worker.ReportProgress(1, bpFrozen);
                }
            };

            thread.ProgressChanged += new ProgressChangedEventHandler(Thread_ProgressChanged);
            thread.RunWorkerCompleted += Thread_WorkCompleted;

            thread.RunWorkerAsync();
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            UpdateProperties(SimulationStatus.Stopped);
        }

        private WriteableBitmap UpdateImage(Board gameBoard, WriteableBitmap bp)
        {
            foreach (var c in gameBoard.CurrentCycleInfected)
            {
                var rect = new Int32Rect(c.IndexX, c.IndexY, width, height);
                bp.WritePixels(rect, infectedColour, stride, offset);

                foreach (var n in c.NeighborsTiles)
                {
                    if (n.CellStatus == Status.Immune)
                    {
                        rect = new Int32Rect(n.IndexX, n.IndexY, width, height);
                        bp.WritePixels(rect, immuneColour, stride, offset);
                    }
                }
            }

            foreach (var c in gameBoard.CurrentCycleNoLongerInfected)
            {
                var rect = new Int32Rect(c.IndexX, c.IndexY, width, height);

                if (c.CellStatus == Status.Immune)
                {
                    bp.WritePixels(rect, immuneColour, stride, offset);
                }

                else
                {
                    bp.WritePixels(rect, deadColour, stride, offset);
                }
            }

            return bp;
        }

        private WriteableBitmap CreateImage()
        {
            var bp = new WriteableBitmap(boardSize, boardSize, dpiX, dpiY, PixelFormats.Bgr32, null);

            for (int i = 0; i < boardSize; i++)
            {
                for (int j = 0; j < boardSize; j++)
                {
                    var rect = new Int32Rect(i, j, width, height);
                    bp.WritePixels(rect, colourData, stride, offset);
                }
            }

            return bp;
        }

        private void UpdateProperties(SimulationStatus status)
        {
            // Occurs when simulation begins
            if (status == SimulationStatus.Starting)
            {
                isSimulationRunning = true;
                SubmitButtonVisibility = Visibility.Hidden;
                StopButtonVisibility = Visibility.Visible;
                BindingMessage = "";
            }

            // Occurs when simulation is stopped by the user
            else if (status == SimulationStatus.Stopped)
            {
                isSimulationRunning = false;
                SubmitButtonVisibility = Visibility.Visible;
                StopButtonVisibility = Visibility.Hidden;
                BindingMessage = "Simulation Stopped";
            }

            // Occurs when simulation is stopped by the user
            else if ((status == SimulationStatus.Finshed) && (isSimulationRunning == true))
            {
                isSimulationRunning = false;
                SubmitButtonVisibility = Visibility.Visible;
                StopButtonVisibility = Visibility.Hidden;
                BindingMessage = "Simulation Over";
            }

        }

        private void Thread_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            var tbp = e.UserState as WriteableBitmap;
            Image = tbp;
        }

        private void Thread_WorkCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            UpdateProperties(SimulationStatus.Finshed);
        }

        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}