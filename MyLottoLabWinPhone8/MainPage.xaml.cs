using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using MyLottoLabWinPhone8.Resources;
using System.ComponentModel;
using System.Windows.Shapes;
using ShakeGestures;
using Microsoft.Phone.Tasks;
using System.Windows.Controls.Primitives;
using System.Threading;


namespace MyLottoLabWinPhone8
{
    public class BallDisplayStruct
    {
        public Ellipse BallElipse { get; set; }
        public TextBlock BallText { get; set; }
    }

    public partial class MainPage : PhoneApplicationPage
    {
        ProgressIndicator prog;
        
        private BallDisplayStruct[] _oBallDisplayArray = new BallDisplayStruct[12];

        LotteryGameList LottoGames { get; set; }
        Dictionary<string, LottoRegionMapping> LotteryGameConfigFiles { get; set; }
        InitConfiguration StartupConfiguration { get; set; }

        public LotteryGameRec CurrentLotteryGame { get; set; }

        public BallDisplayStruct[] BallDisplayArray
        {
            get { return _oBallDisplayArray; }
            set { _oBallDisplayArray = value; }
        }

        public bool IsPostInit { get; set; }

        // Constructor
        public MainPage()
        {
            InitializeComponent();


            try
            {
                // register shake event
                ShakeGesturesHelper.Instance.ShakeGesture += new
                    EventHandler<ShakeGestureEventArgs>(Instance_ShakeGesture);

                // optional, set parameters
                ShakeGesturesHelper.Instance.MinimumRequiredMovesForShake = 5;
                ShakeGesturesHelper.Instance.StillCounterThreshold = 3;
                ShakeGesturesHelper.Instance.StillMagnitudeWithoutGravitationThreshold = 8;

                // Add the ball display objects for easy reference
                BallDisplayStruct oBallDisplayStruct = new BallDisplayStruct();
                _oBallDisplayArray[0]
                    = new BallDisplayStruct() { BallElipse = ovalBall1, BallText = txtBall1 };
                _oBallDisplayArray[1]
                    = new BallDisplayStruct() { BallElipse = ovalBall2, BallText = txtBall2 };
                _oBallDisplayArray[2]
                    = new BallDisplayStruct() { BallElipse = ovalBall3, BallText = txtBall3 };
                _oBallDisplayArray[3]
                    = new BallDisplayStruct() { BallElipse = ovalBall4, BallText = txtBall4 };
                _oBallDisplayArray[4]
                    = new BallDisplayStruct() { BallElipse = ovalBall5, BallText = txtBall5 };
                _oBallDisplayArray[5]
                    = new BallDisplayStruct() { BallElipse = ovalBall6, BallText = txtBall6 };
                _oBallDisplayArray[6]
                    = new BallDisplayStruct() { BallElipse = ovalBall7, BallText = txtBall7 };
                _oBallDisplayArray[7]
                    = new BallDisplayStruct() { BallElipse = ovalBall8, BallText = txtBall8 };
                _oBallDisplayArray[8]
                    = new BallDisplayStruct() { BallElipse = ovalBall9, BallText = txtBall9 };
                _oBallDisplayArray[9]
                    = new BallDisplayStruct() { BallElipse = ovalBall10, BallText = txtBall10 };
                _oBallDisplayArray[10]
                    = new BallDisplayStruct() { BallElipse = ovalBall11, BallText = txtBall11 };
                _oBallDisplayArray[11]
                    = new BallDisplayStruct() { BallElipse = ovalBall12, BallText = txtBall12 };

                // Get the Startup configuration
                StartupConfiguration = LottoAlgorithm.GetInitConfiguration();
                
                // Get Lottery Game Config files
                LotteryGameConfigFiles = LottoAlgorithm.GetLotteryGameConfigFiles();

                // Add State names to List Picker
                List<LottoRegionMapping> oRegionsList
                    = LotteryGameConfigFiles.Values.ToList<LottoRegionMapping>();
                this.cboRegion.ItemsSource
                    = oRegionsList;

                // Set List Picker to Selected State
                int iRegionIndex = -1;
                try
                {
                    iRegionIndex
                        = cboRegion.Items.IndexOf(oRegionsList.Where(Reg => Reg.Region == StartupConfiguration.InitRegion).First());
                }
                catch
                {
                    iRegionIndex = -1;
                }

                if (iRegionIndex > -1)
                {
                    cboRegion.SelectedIndex = iRegionIndex;
                }
                else
                {
                    cboRegion.SelectedIndex = 0;
                }

                // Load Lotto Game file for selected State
                if (LotteryGameConfigFiles.ContainsKey(StartupConfiguration.InitRegion))
                {
                    LottoRegionMapping oLottoRegionMappingSI = cboRegion.SelectedItem as LottoRegionMapping;
                    // Load games in to dictionary definitions
                    LottoGames = LottoAlgorithm.GetLotteryGames(oLottoRegionMappingSI.FileName);
                }


                // Add Lotto Game Names to List Picker
                List<LotteryGameRec> oRegionLottoGamesList
                    = LottoGames.RegionLottoGameDict.Values.ToList<LotteryGameRec>();
                this.cboLottoGame.ItemsSource = oRegionLottoGamesList;

                // Set list picker to Initial Lotto Game
                int iGameIndex = -1;
                try
                {
                    iGameIndex
                        = cboLottoGame.Items.IndexOf(oRegionLottoGamesList.Where(RGames => RGames.GameName == StartupConfiguration.InitLottoGame).First());
                }
                catch
                {
                    iGameIndex = -1;
                }

                if (iGameIndex > -1)
                {
                    cboLottoGame.SelectedIndex = iGameIndex;
                }
                else
                {
                    cboLottoGame.SelectedIndex = 0;
                }

                // Populate LottorGame data on screen
                if (cboLottoGame.Items.Count > 0)
                {
                    LotteryGameRec oPickerRec
                        = cboLottoGame.SelectedItem as LotteryGameRec;
                    CurrentLotteryGame = LottoGames.RegionLottoGameDict[oPickerRec.GameName];
                    EnableDisablePopularButtons();
                }

                // Hide/Show Special Ball
                HideOrShowSpecialBall();

                // start shake detection
                ShakeGesturesHelper.Instance.Active = true;
            }
            catch 
            { 
            }
        }

        private void Instance_ShakeGesture(object sender, ShakeGestureEventArgs e)
        {
            try
            {
                btnQuickPick.Dispatcher.BeginInvoke(
                    () =>
                    {
                        //Cool Sweep Effect
                        this.BallSpread.Begin();

                        ClearBallDisplay();

                        Dictionary<int, int> oBalls
                            = LottoAlgorithm.QuickPick(CurrentLotteryGame);

                        WriteBallDisplay(oBalls);

                    });
            }
            catch { }
        }


        private void HideOrShowSpecialBall()
        {
            try
            {
                if (!IsPostInit)
                {
                    this.Ballsin.Begin();
                }

                if (CurrentLotteryGame.UseSpecialBall)
                {

                    // Show Regular Balls
                    for (int iCount = 0; iCount < CurrentLotteryGame.LottoBallRegCount; iCount++)
                    {
                        BallDisplayArray[iCount].BallElipse.Visibility
                            = System.Windows.Visibility.Visible;
                        BallDisplayArray[iCount].BallElipse.Fill
                            = DummyWhite.Fill;
                        BallDisplayArray[iCount].BallText.Text = "??";
                        BallDisplayArray[iCount].BallText.Visibility
                            = System.Windows.Visibility.Visible;
                        //Makes text reappear after animation
                        BallDisplayArray[iCount].BallText.Foreground
                             = txtBallDummy.Foreground;
                    }

                    // Show  the Special ball
                    BallDisplayArray[CurrentLotteryGame.LottoBallRegCount].BallElipse.Visibility
                        = System.Windows.Visibility.Visible;
                    BallDisplayArray[CurrentLotteryGame.LottoBallRegCount].BallText.Text = "??";
                    BallDisplayArray[CurrentLotteryGame.LottoBallRegCount].BallText.Visibility
                        = System.Windows.Visibility.Visible;

                    BallDisplayArray[CurrentLotteryGame.LottoBallRegCount].BallElipse.Fill
                        = DummyRed.Fill;
                    //Makes Special ball text reappear after animation
                    BallDisplayArray[CurrentLotteryGame.LottoBallRegCount].BallText.Foreground
                             = txtBallDummy.Foreground;

                    // Hide the rest of the balls
                    for (int iCount = CurrentLotteryGame.LottoBallRegCount + 1; iCount < _oBallDisplayArray.Length; iCount++)
                    {
                        BallDisplayArray[iCount].BallElipse.Visibility
                            = System.Windows.Visibility.Collapsed;
                        BallDisplayArray[iCount].BallText.Visibility
                            = System.Windows.Visibility.Collapsed;
                    }
                }
                else
                {
                    // Show Regular Balls
                    for (int iCount = 0; iCount < CurrentLotteryGame.LottoBallRegCount; iCount++)
                    {
                        BallDisplayArray[iCount].BallElipse.Visibility
                            = System.Windows.Visibility.Visible;
                        BallDisplayArray[iCount].BallElipse.Fill
                            = DummyWhite.Fill;
                        BallDisplayArray[iCount].BallText.Text = "??";
                        BallDisplayArray[iCount].BallText.Visibility
                            = System.Windows.Visibility.Visible;
                        BallDisplayArray[iCount].BallText.Foreground
                             = txtBallDummy.Foreground;
                    }


                    // Hide the rest of the balls
                    for (int iCount = CurrentLotteryGame.LottoBallRegCount; iCount < _oBallDisplayArray.Length; iCount++)
                    {
                        BallDisplayArray[iCount].BallElipse.Visibility
                            = System.Windows.Visibility.Collapsed;
                        BallDisplayArray[iCount].BallText.Visibility
                            = System.Windows.Visibility.Collapsed;
                    }
                }
            }
            catch { }
        }
        private void WriteBallDisplay(Dictionary<int, int> oBalls)
        {
            try
            {
                foreach (UIElement item in gridBallDisplay.Children)
                {
                    if (item.GetType() == typeof(TextBlock))
                    {
                        TextBlock oTextBlock = (TextBlock)item;

                        switch (oTextBlock.Name.ToUpper())
                        {
                            case "TXTBALL1":
                                if (oBalls.Count >= 1)
                                {
                                    oTextBlock.Text = Convert.ToString(oBalls[1]);
                                }
                                break;
                            case "TXTBALL2":
                                if (oBalls.Count >= 2)
                                {
                                    oTextBlock.Text = Convert.ToString(oBalls[2]);
                                }
                                break;
                            case "TXTBALL3":
                                if (oBalls.Count >= 3)
                                {
                                    oTextBlock.Text = Convert.ToString(oBalls[3]);
                                }
                                break;
                            case "TXTBALL4":
                                if (oBalls.Count >= 4)
                                {
                                    oTextBlock.Text = Convert.ToString(oBalls[4]);
                                }
                                break;
                            case "TXTBALL5":
                                if (oBalls.Count >= 5)
                                {
                                    oTextBlock.Text = Convert.ToString(oBalls[5]);
                                }
                                break;
                            case "TXTBALL6":
                                if (oBalls.Count >= 6)
                                {
                                    oTextBlock.Text = Convert.ToString(oBalls[6]);
                                }
                                break;

                            case "TXTBALL7":
                                if (oBalls.Count >= 7)
                                {
                                    oTextBlock.Text = Convert.ToString(oBalls[7]);
                                }
                                break;

                            case "TXTBALL8":
                                if (oBalls.Count >= 8)
                                {
                                    oTextBlock.Text = Convert.ToString(oBalls[8]);
                                }
                                break;
                            case "TXTBALL9":
                                if (oBalls.Count >= 9)
                                {
                                    oTextBlock.Text = Convert.ToString(oBalls[9]);
                                }
                                break;

                            case "TXTBALL10":
                                if (oBalls.Count >= 10)
                                {
                                    oTextBlock.Text = Convert.ToString(oBalls[10]);
                                }
                                break;

                            case "TXTBALL11":
                                if (oBalls.Count >= 11)
                                {
                                    oTextBlock.Text = Convert.ToString(oBalls[11]);
                                }
                                break;

                            case "TXTBALL12":
                                if (oBalls.Count >= 12)
                                {
                                    oTextBlock.Text = Convert.ToString(oBalls[12]);
                                }
                                break;

                            default:
                                break;
                        }
                    }
                }
            }
            catch { }
        }
        private void ClearBallDisplay()
        {
            try
            {
                foreach (UIElement item in gridBallDisplay.Children)
                {
                    if (item.GetType() == typeof(TextBlock))
                    {
                        TextBlock oTextBlock = (TextBlock)item;
                        oTextBlock.Text = "";

                    }
                }
            }
            catch { }
        }

        private void EnableDisablePopularButtons()
        {
            try
            {
                // Enable/Disable Least popular buttons
                if (
                    (CurrentLotteryGame.LottoBallRegOrderingH2LCount()
                    >= ((Convert.ToSingle(txtMostPopularPercent.Text) / 100) * CurrentLotteryGame.LottoBallRegUB))
                    &&
                    (CurrentLotteryGame.LottoBallSpecialOrderingH2LCount()
                    >= ((Convert.ToSingle(txtMostPopularPercent.Text) / 100) * CurrentLotteryGame.LottoBallSpecialUB))
                    )
                {
                    btnPickMostPopular.IsEnabled = true;
                }
                else
                {
                    btnPickMostPopular.IsEnabled = false;
                }

                // Enable/Disable Least popular buttons
                if
                    (
                    (CurrentLotteryGame.LottoBallRegOrderingH2LCount()
                    >= ((Convert.ToSingle(txtLeastPopularPercent.Text) / 100) * CurrentLotteryGame.LottoBallRegUB))
                    &&
                    (CurrentLotteryGame.LottoBallSpecialOrderingH2LCount()
                    >= ((Convert.ToSingle(txtLeastPopularPercent.Text) / 100) * CurrentLotteryGame.LottoBallSpecialUB))
                    )
                {
                    btnPickLeastPopular.IsEnabled = true;
                }
                else
                {
                    btnPickLeastPopular.IsEnabled = false;
                }
            }
            catch { }
        }

        private void btnQuickPick_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Cool Sweep Effect
                this.BallSpread.Begin();

                this.quickflare.Begin();

                ClearBallDisplay();

                Dictionary<int, int> oBalls
                    = LottoAlgorithm.QuickPick(CurrentLotteryGame);

                WriteBallDisplay(oBalls);
            }
            catch { }
        }

        private void btnThrowoutByPick_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ClearBallDisplay();

                    //Load the waiting animation for balls
                    this.WaitingProgress.Begin();

                    SystemTray.SetIsVisible(this, true);
                    SystemTray.SetOpacity(this, 0);

                    prog = new ProgressIndicator();
                    prog.IsVisible = true;
                    prog.IsIndeterminate = true;

                    SystemTray.SetProgressIndicator(this, prog);

                double dbThrowoutByPickCount
                    = Convert.ToDouble(txtThrowoutByPickCount.Text);

                Dictionary<int, int> oBalls = new Dictionary<int, int>();

                BackgroundWorker oBackgroundWoker = new BackgroundWorker();

                // what to do in the background thread
                oBackgroundWoker.DoWork += new DoWorkEventHandler(
                delegate(object oDelagateObject, DoWorkEventArgs args)
                {
                    BackgroundWorker oBW = oDelagateObject as BackgroundWorker;

                    oBalls = LottoAlgorithm.ThrowoutByPick(CurrentLotteryGame, dbThrowoutByPickCount);
                });

                // what to do when worker completes its task (notify the user)
                oBackgroundWoker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(
                delegate(object o, RunWorkerCompletedEventArgs args)
                {
                    WriteBallDisplay(oBalls);

                    //Stop the waiting animation for balls
                    this.WaitingProgress.Stop();
                    SystemTray.SetIsVisible(this, false);

                    prog = new ProgressIndicator();
                    prog.IsVisible = false;
                    prog.IsIndeterminate = false;

                    SystemTray.SetProgressIndicator(this, prog);


                });

                oBackgroundWoker.RunWorkerAsync();
                
            }
            catch
            {
            }
        }
        private void btnThrowoutByBall_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ClearBallDisplay();

                int iThrowoutByBallPercent
                    = Convert.ToInt32(txtThrowoutByBallPercent.Text);

                Dictionary<int, int> oBalls
                    = LottoAlgorithm.ThrowoutByBall(CurrentLotteryGame
                      , iThrowoutByBallPercent);

                WriteBallDisplay(oBalls);

                this.BallSpread.Begin();
            }
            catch
            {
            }

        }

        private void btnPickByMostPopular_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ClearBallDisplay();

                int iMostPopularPercent
                    = Convert.ToInt32(txtMostPopularPercent.Text);

                Dictionary<int, int> oBalls
                    = LottoAlgorithm.Popular(CurrentLotteryGame, true, iMostPopularPercent);

                WriteBallDisplay(oBalls);
                //Cool Sweep Effect
                this.BallSpread.Begin();
            }
            catch { }
        }
        private void btnPickBayLeastPopular_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ClearBallDisplay();

                int iLeastPopularPercent
                    = Convert.ToInt32(txtMostPopularPercent.Text);

                Dictionary<int, int> oBalls
                    = LottoAlgorithm.Popular(CurrentLotteryGame, false, iLeastPopularPercent);

                WriteBallDisplay(oBalls);
                //Cool Sweep Effect
                this.BallSpread.Begin();
            }
            catch { }
        }

        private void btnConfiguration_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LottoRegionMapping oLottoRegionMapping
                    = cboRegion.SelectedItem as LottoRegionMapping;

                LotteryGameRec oLotteryGameRec = cboLottoGame.SelectedItem as LotteryGameRec;

                // Save Init config
                LottoAlgorithm.UpdateInitConfig(StartupConfiguration);

                //Setings Transition Begin
                this.SGwow.Begin();

                NavigationService.Navigate(new Uri("/Configuration.xaml?StartLotteryRegion="
                    + oLottoRegionMapping.Region
                    + "&StartLotteryGameName="
                    + oLotteryGameRec.GameName
                    , UriKind.Relative));
            }
            catch { }
        }

        private void cboRegion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (IsPostInit)
                {
                    ListPicker oPicker = sender as ListPicker;
                    LottoRegionMapping oLottoRegionMapping = oPicker.SelectedItem as LottoRegionMapping;
                    // Load Lotto Game file for Selected State
                    string sLotteryGameFileName = "";
                    if (LotteryGameConfigFiles.ContainsKey(oLottoRegionMapping.Region))
                    {
                        sLotteryGameFileName = oLottoRegionMapping.FileName;
                    }

                    // Load games in to dictionary definitions
                    LottoGames = LottoAlgorithm.GetLotteryGames(sLotteryGameFileName);

                    // Add Lotto Game Names to List Picker
                    this.cboLottoGame.ItemsSource
                        = LottoGames.RegionLottoGameDict.Values.ToList<LotteryGameRec>();

                    // Set list picker to select First Game in list
                    cboLottoGame.SelectedIndex = 0;

                    // Update Selected Game at index 0
                    LotteryGameRec oLottoGameRec = cboLottoGame.SelectedItem as LotteryGameRec;
                    StartupConfiguration.InitLottoGame = oLottoGameRec.GameName;


                    // Update Selected Region
                    StartupConfiguration.InitRegion = oLottoRegionMapping.Region;
                }
            }
            catch { }
        }

        private void cboLottoGame_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (IsPostInit)
                {
                    // Get current lottery game data
                    if (cboLottoGame.Items.Count > 0)
                    {
                        ListPicker oLottoGame = sender as ListPicker;
                        LotteryGameRec oPickerRec = oLottoGame.SelectedItem as LotteryGameRec;
                        CurrentLotteryGame = LottoGames.RegionLottoGameDict[oPickerRec.GameName];
                        EnableDisablePopularButtons();

                        // Hide/Show Special Ball
                        HideOrShowSpecialBall();

                        // Update Selected Game
                        StartupConfiguration.InitLottoGame = oPickerRec.GameName;
                    }
                }
            }
            catch { }
        }

        private void pageMainPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Set the post initilization flag to true after trial nag screen
            IsPostInit = true;
            
        }

        private void pageMainPage_BackKeyPress(object sender, CancelEventArgs e)
        {
            LottoAlgorithm.UpdateInitConfig(StartupConfiguration);
        }

        private void ToggleSwitch_Checked(object sender, RoutedEventArgs e)
        {
            // start shake detection
            ShakeGesturesHelper.Instance.Active = true;
            toggleShake.Content = "On";
        }

        private void toggleShake_Unchecked(object sender, RoutedEventArgs e)
        {
            // stop shake detection
            ShakeGesturesHelper.Instance.Active = false;
            toggleShake.Content = "Off";
        }

        private void sliderThrowoutByBallPercent_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                if (IsPostInit)
                {
                    txtThrowoutByBallPercent.Text = Convert.ToInt32(e.NewValue).ToString();
                }
            }
            catch { }
        }

        private void sliderPickByMostPopularPercent_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                if (IsPostInit)
                {
                    txtMostPopularPercent.Text = Convert.ToInt32(e.NewValue).ToString();
                }
            }
            catch { }
        }

        private void sliderPickByLeastPopularPercent_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                if (IsPostInit)
                {
                    txtLeastPopularPercent.Text = Convert.ToInt32(e.NewValue).ToString();
                }
            }
            catch { }
        }
    }
}