using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using EsportsCoverage;
using System.IO;
using System.Configuration;

namespace SC2StreamHelper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        class AppSettings
        {
            public string EsportsCoverageStreamKey = "";
            public string SaveFilePath = @".\currentmatch.txt";
            public string SaveFileFormat = "@bo: @player1 @score1 - @score2 @player2";
            public bool WCSGameHeartMode = false;

            public AppSettings()
            {
                this.EsportsCoverageStreamKey = ReadSetting("EsportsCoverageStreamKey", EsportsCoverageStreamKey);
                this.SaveFilePath = ReadSetting("SaveFilePath", SaveFilePath);
                this.SaveFileFormat = ReadSetting("SaveFileFormat", SaveFileFormat);
                this.WCSGameHeartMode = (ReadSetting("WCSGameHeartMode", WCSGameHeartMode.ToString()).Equals("True", StringComparison.InvariantCultureIgnoreCase));
            }

            static string ReadSetting(string key, string defaultvalue)
            {
                var appSettings = ConfigurationManager.AppSettings;
                if (appSettings[key] != null)
                {
                    return appSettings[key];
                }

                // automatically creates settings that are missing with the defaultvalues
                WriteSetting(key, defaultvalue);

                return defaultvalue;
            }

            static void WriteSetting(string key, string value)
            {
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = configFile.AppSettings.Settings;
                if (settings[key] == null)
                {
                    settings.Add(key, value);
                }
                else
                {
                    settings[key].Value = value;
                }
                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
            }
        }

        private AppSettings MyAppSettings = null;
        private EsportsCoverageAPI api = null;

        private Player LastPlayer1 = null;
        private Player LastPlayer2 = null;

        public MainWindow()
        {
            InitializeComponent();

            MyAppSettings = new AppSettings();

            InitBestOfPossibilities();
            InitScorePossibilities();

            #if DEBUG
            barStatus.Visibility = System.Windows.Visibility.Visible;
            #endif

            if (MyAppSettings.EsportsCoverageStreamKey != "")
            {
                api = new EsportsCoverageAPI(MyAppSettings.EsportsCoverageStreamKey);

                mnMain.Visibility = System.Windows.Visibility.Visible;
                this.Height = 295;
                grpStream.Visibility = System.Windows.Visibility.Visible;

                InitEvents();

                FetchStreamDetails();
            }
        }

        private void FetchStreamDetails()
        {
            var details = api.GetStreamDetails();

            LastPlayer1 = details.player1;
            LastPlayer2 = details.player2;

            edPlayer1.Text = details.player1.actualname;
            edPlayer2.Text = details.player2.actualname;

            edPlayer1.ToolTip = GetPlayerDetailTooltipText(details.player1);
            edPlayer2.ToolTip = GetPlayerDetailTooltipText(details.player2);

            cbPlayer1Score.SelectedIndex = details.score1;
            cbPlayer2Score.SelectedIndex = details.score2;

            lblStreamname.Content = details.streamname;

            cbEvent.SelectedValue = details.event_id;
        }

        private object GetPlayerDetailTooltipText(Player player)
        {
            // todo: figure out a prettier way to display player details
            return
                (player.fullname + "\r\n" +
                player.teamname + "\r\n" +
                player.race_text + "\r\n" +
                player.country_long + "\r\n" +
                player.twitterlink + "\r\n" +
                player.twitchlink).Trim();
        }

        private void InitEvents()
        {
            cbEvent.ItemsSource = api.ListEventNames();
            cbEvent.DisplayMemberPath = "Value";
            cbEvent.SelectedValuePath = "Key";
        }

        private void InitBestOfPossibilities()
        {
            string[] arrPossibilities = new string[] {
                "Bo1",
                "Bo3",
                "Bo5",
                "Bo7",
                "Bo9"
            };

            cbBestOf.ItemsSource = arrPossibilities;
            cbBestOf.SelectedIndex = 1; // bo3 most common
        }

        private void InitScorePossibilities()
        {
            int[] arrPossibilities = new int[] {
                0,
                1,
                2,
                3,
                4,
                5,
                6,
                7,
                8,
                9
            };

            cbPlayer1Score.ItemsSource = arrPossibilities;
            cbPlayer2Score.ItemsSource = arrPossibilities;

            cbPlayer1Score.SelectedIndex = 0;
            cbPlayer2Score.SelectedIndex = 0;
        }

        private void btnSendToGameHeart_Click(object sender, RoutedEventArgs e)
        {
            // swap to starcraft, 
            IntPtr hWnd = User32.FindWindow(null, "StarCraft II");
            if (hWnd != null)
            {
                User32.SetForegroundWindow(hWnd);
            }
            else
            {
                MessageBox.Show("StarCraft II Not Found!");
                return;
            }

            RemoteKeyboardInput input = new RemoteKeyboardInput();

            // enable score if more than Bo1
            if (cbBestOf.SelectedIndex > 0)
            {
                if (!MyAppSettings.WCSGameHeartMode)
                {
                    // wcs gameheart doesn't need this, just straight to bestof
                    input.Send_CtrlShiftKeyPress((byte)'S');
                }

                // set best of...
                if (MyAppSettings.WCSGameHeartMode)
                {
                    // setting bo3 not needed in normal GH ui, is needed in WCS GH
                    if (cbBestOf.SelectedIndex == 1) input.Send_CtrlShiftKeyPress((short)(0x30 + 3));
                }
                
                if (cbBestOf.SelectedIndex == 2) input.Send_CtrlShiftKeyPress((short)(0x30 + 5));
                if (cbBestOf.SelectedIndex == 3) input.Send_CtrlShiftKeyPress((short)(0x30 + 7));
                if (cbBestOf.SelectedIndex == 4) input.Send_CtrlShiftKeyPress((short)(0x30 + 9));
            }

            // swap player positions
            if (chkDifferentSpawns.IsChecked.GetValueOrDefault(false))
            {
                input.Send_CtrlKeyPress((byte)'X');

                if (!chkDifferentInitialOrder.IsChecked.GetValueOrDefault(false))
                {
                    // players are swapped, add scores reversed

                    // 1st player
                    if (cbPlayer2Score.SelectedIndex > 0)
                    {
                        input.Send_ShiftKeyPress((short)(0x30 + cbPlayer2Score.SelectedIndex));
                    }

                    // 2nd player
                    if (cbPlayer1Score.SelectedIndex > 0)
                    {
                        input.Send_CtrlKeyPress((short)(0x30 + cbPlayer1Score.SelectedIndex));
                    }
                }
                else
                {
                    // but if the initial order was already reversed what was entered into this application, we do it the other way around

                    // 1st player
                    if (cbPlayer1Score.SelectedIndex > 0)
                    {
                        input.Send_ShiftKeyPress((short)(0x30 + cbPlayer1Score.SelectedIndex));
                    }

                    // 2nd player
                    if (cbPlayer2Score.SelectedIndex > 0)
                    {
                        input.Send_CtrlKeyPress((short)(0x30 + cbPlayer2Score.SelectedIndex));
                    }
                }
            }
            else
            {
                if (!chkDifferentInitialOrder.IsChecked.GetValueOrDefault(false))
                {
                    // 1st player
                    if (cbPlayer1Score.SelectedIndex > 0)
                    {
                        input.Send_ShiftKeyPress((short)(0x30 + cbPlayer1Score.SelectedIndex));
                    }

                    // 2nd player
                    if (cbPlayer2Score.SelectedIndex > 0)
                    {
                        input.Send_CtrlKeyPress((short)(0x30 + cbPlayer2Score.SelectedIndex));
                    }
                }
                else
                {
                    // 1st player
                    if (cbPlayer2Score.SelectedIndex > 0)
                    {
                        input.Send_ShiftKeyPress((short)(0x30 + cbPlayer2Score.SelectedIndex));
                    }

                    // 2nd player
                    if (cbPlayer1Score.SelectedIndex > 0)
                    {
                        input.Send_CtrlKeyPress((short)(0x30 + cbPlayer1Score.SelectedIndex));
                    }
                }
            }

            if (!MyAppSettings.WCSGameHeartMode)
            {
                // todo: how does this work in WCS Gameheart?

                // open production tab
                input.Send_NormalKeyPress((byte)'D');
            }
        }
        
        private void SaveToFile()
        {
            string format = MyAppSettings.SaveFileFormat;

            format = format.Replace("@player1", edPlayer1.Text);
            format = format.Replace("@player2", edPlayer2.Text);

            format = format.Replace("@score1", cbPlayer1Score.Text);
            format = format.Replace("@score2", cbPlayer2Score.Text);

            format = format.Replace("@bo", cbBestOf.Text);

            format = format.Replace("\\r\\n", "\r\n");

            var writer = new StreamWriter(MyAppSettings.SaveFilePath);
            try
            {
                writer.Write(format);
            }
            finally
            {
                writer.Close();
            }
        }
        
        private void btnSaveToFile_Click(object sender, RoutedEventArgs e)
        {
            SaveToFile();
        }

        private void miFetchPlayerInfo_Click(object sender, RoutedEventArgs e)
        {
            FetchStreamDetails();
        }

        private void miSendScore_Click(object sender, RoutedEventArgs e)
        {
            api.SetCurrentScore(cbPlayer1Score.SelectedIndex, cbPlayer2Score.SelectedIndex);
        }

        private void miSendPlayers_Click(object sender, RoutedEventArgs e)
        {
            if (api.SetCurrentPlayers(edPlayer1.Text, edPlayer2.Text, cbPlayer1Score.SelectedIndex, cbPlayer2Score.SelectedIndex))
            {
                FetchStreamDetails();
            }
            else
            {
                // api.LastError
                barStatus.Items.Add(api.LastError);
            }
        }

        private void cbEvent_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbEvent.SelectedValue != null) {
                string s = cbEvent.SelectedValue.ToString();
                int iEventId = int.Parse(s);

                if (api != null)
                {
                    // directly set event through the api when you change it, separate button/menu would be overkill in this particular case
                    api.SetEvent(iEventId);
                }
            }
        }

        private void edPlayer1_TextChanged(object sender, TextChangedEventArgs e)
        {
            // reset tooltip if you typed in a different player name

            edPlayer1.ToolTip = "";
            if (LastPlayer1 != null)
            {
                if (LastPlayer1.playername == edPlayer1.Text)
                {
                    edPlayer1.ToolTip = GetPlayerDetailTooltipText(LastPlayer1);
                }
            }
        }

        private void edPlayer2_TextChanged(object sender, TextChangedEventArgs e)
        {
            edPlayer2.ToolTip = "";
            if (LastPlayer2 != null)
            {
                if (LastPlayer2.playername == edPlayer2.Text)
                {
                    edPlayer2.ToolTip = GetPlayerDetailTooltipText(LastPlayer2);
                }
            }
        }
    }
}
