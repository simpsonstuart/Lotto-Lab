using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Xml.Linq;
using System.Xml.Schema;
using System.IO;
using System.Xml;
using System.Threading;

namespace MyLottoLabWinPhone8
{
    public partial class Configuration : PhoneApplicationPage
    {
        LotteryGameList LottoGames { get; set; }
        Dictionary<string, LottoRegionMapping> LotteryGameConfigFiles { get; set; }

        public LotteryGameRec CurrentLotteryGame { get; set; }
        public string CurrentLotteryGameFileName { get; set; }
        public bool IsPostInit { get; set; }
        public bool IsPostNavigation { get; set; }

        public string StartLotteryGameName { get; set; }
        public string StartLotteryRegion { get; set; }

        public Configuration()
        {
            InitializeComponent();

            // Get Lottery Game Config files
            LotteryGameConfigFiles = LottoAlgorithm.GetLotteryGameConfigFiles();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            try
            {
                if (!IsPostNavigation)
                {
                    // Get Passed Parameters for Lottery Game name and Region
                    base.OnNavigatedTo(e);
                    string sLotteryGameName = NavigationContext.QueryString["StartLotteryGameName"];
                    string sLotteryRegion = NavigationContext.QueryString["StartLotteryRegion"];
                    StartLotteryGameName = sLotteryGameName;
                    StartLotteryRegion = sLotteryRegion;

                    // Add State names to List Picker
                    List<LottoRegionMapping> oRegionsList
                        = LotteryGameConfigFiles.Values.ToList<LottoRegionMapping>();
                    this.cboRegion.ItemsSource
                        = oRegionsList;

                    // Set List Picker to Selected State
                    // int iRegionIndex = cboRegion.Items.IndexOf(StartLotteryRegion);
                    int iRegionIndex
                            = cboRegion.Items.IndexOf(oRegionsList.Where(Reg => Reg.Region == StartLotteryRegion).First());

                    if (iRegionIndex > -1)
                    {
                        cboRegion.SelectedIndex = iRegionIndex;
                    }
                    else
                    {
                        cboRegion.SelectedIndex = 0;
                    }


                    // Load Lotto Game file for selected State
                    string sLotteryGameFileName = "";
                    if (LotteryGameConfigFiles.ContainsKey(StartLotteryRegion))
                    {
                        LottoRegionMapping oLottoRegionMappingSI = cboRegion.SelectedItem as LottoRegionMapping;
                        sLotteryGameFileName = oLottoRegionMappingSI.FileName;
                        // Load games in to dictionary definitions
                        LottoGames = LottoAlgorithm.GetLotteryGames(sLotteryGameFileName);
                    }
                    CurrentLotteryGameFileName = sLotteryGameFileName;


                    // Add Lotto Game Names to List Picker
                    List<LotteryGameRec> oRegionLottoGamesList
                        = LottoGames.RegionLottoGameDict.Values.ToList<LotteryGameRec>();
                    this.cboLottoGame.ItemsSource = oRegionLottoGamesList;

                    // Set list picker to selected Lotto Game
                    int iGameIndex
                        = cboLottoGame.Items.IndexOf(oRegionLottoGamesList.Where(RGames => RGames.GameName == StartLotteryGameName).First());

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
                        txtRegBallUB.Text = CurrentLotteryGame.LottoBallRegUB.ToString();
                        txtRegBallCount.Text = CurrentLotteryGame.LottoBallRegCount.ToString();
                        txtSpecialBallUB.Text = CurrentLotteryGame.LottoBallSpecialUB.ToString();
                        chkUseSpecialBall.IsChecked = CurrentLotteryGame.UseSpecialBall;
                        txtRegBallOrderH2L.Text = CurrentLotteryGame.LottoBallRegOrderingH2LStr;
                        txtSpecialBallOrderH2L.Text = CurrentLotteryGame.LottoBallSpecialOrderingH2LStr;
                        btnUpdate.IsEnabled = false;
                    }

                    IsPostNavigation = true;
                }
            }
            catch { }
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            btnBack.Focus();
            btnUpdate.IsEnabled = false;

            try
            {
                CurrentLotteryGame.LottoBallRegOrderingH2LStr = txtRegBallOrderH2L.Text;
                CurrentLotteryGame.LottoBallSpecialOrderingH2LStr = txtSpecialBallOrderH2L.Text;
                CurrentLotteryGame.LottoBallRegUB = Convert.ToInt32(txtRegBallUB.Text);
                CurrentLotteryGame.LottoBallSpecialUB = Convert.ToInt32(txtSpecialBallUB.Text);

                LottoAlgorithm.UpdateLottoGames(LottoGames, CurrentLotteryGame);
            }
            catch 
            {
            }
        }

        private void btnAbout_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/About.xaml", UriKind.Relative));
        }

        private void txtRegBallOrderH2L_TextChanged(object sender, TextChangedEventArgs e)
        {
            btnUpdate.IsEnabled = true;
        }

        private void txtSpecialBallOrderH2L_TextChanged(object sender, TextChangedEventArgs e)
        {
            btnUpdate.IsEnabled = true;
        }

        private void txtRegBallUB_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (IsPostInit)
            {
                btnUpdate.IsEnabled = true;
            }
        }

        private void txtSpecialBallUB_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (IsPostInit)
            {
                btnUpdate.IsEnabled = true;
            }
        }

        private void cboLottoGame_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (IsPostInit)
                {
                    if (cboLottoGame.Items.Count > 0)
                    {
                        // Populate LottorGame data on screen
                        ListPicker oLottoGame = sender as ListPicker;
                        LotteryGameRec oPickerRec = oLottoGame.SelectedItem as LotteryGameRec;
                        CurrentLotteryGame = LottoGames.RegionLottoGameDict[oPickerRec.GameName];

                        txtRegBallUB.Text = CurrentLotteryGame.LottoBallRegUB.ToString();
                        txtRegBallCount.Text = CurrentLotteryGame.LottoBallRegCount.ToString();
                        txtSpecialBallUB.Text = CurrentLotteryGame.LottoBallSpecialUB.ToString();
                        chkUseSpecialBall.IsChecked = CurrentLotteryGame.UseSpecialBall;
                        txtRegBallOrderH2L.Text = CurrentLotteryGame.LottoBallRegOrderingH2LStr;
                        txtSpecialBallOrderH2L.Text = CurrentLotteryGame.LottoBallSpecialOrderingH2LStr;
                        btnUpdate.IsEnabled = false;
                    }
                }
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
                    CurrentLotteryGameFileName = sLotteryGameFileName;

                    // Load games in to dictionary definitions
                    LottoGames = LottoAlgorithm.GetLotteryGames(sLotteryGameFileName);

                    // Add Lotto Game Names to List Picker
                    this.cboLottoGame.ItemsSource
                        = LottoGames.RegionLottoGameDict.Values.ToList<LotteryGameRec>();

                    // Set list picker to selected Lotto Game
                    cboLottoGame.SelectedIndex = 0;
                }
            }
            catch { }
        }

        private void pageConfiguaration_Loaded(object sender, RoutedEventArgs e)
        {

            // Set the post initilization flag to true
            IsPostInit = true;
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }
    }
}