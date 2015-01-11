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

namespace SC2StreamHelper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr GetWindow(IntPtr hWnd, GetWindow_Cmd uCmd);

        enum GetWindow_Cmd : uint
        {
            GW_HWNDFIRST = 0,
            GW_HWNDLAST = 1,
            GW_HWNDNEXT = 2,
            GW_HWNDPREV = 3,
            GW_OWNER = 4,
            GW_CHILD = 5,
            GW_ENABLEDPOPUP = 6
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImportAttribute("User32.dll")]
        private static extern IntPtr FindWindow(String ClassName, String WindowName);

        private RemoteKeyboardInput input;

        public MainWindow()
        {
            InitializeComponent();

            InitBestOfPossibilities();
            InitScorePossibilities();

            input = new RemoteKeyboardInput();
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
            IntPtr hWnd = FindWindow(null, "StarCraft II");
            if (hWnd != null)
            {
                SetForegroundWindow(hWnd); //Activate it
            }
            else
            {
                MessageBox.Show("StarCraft II Not Found!");
                return;
            }

            // enable score if more than Bo1
            if (cbBestOf.SelectedIndex > 0)
            {
                input.Send_CtrlShiftKeyPress((byte)'S');

                // set best of...
                //if (cbBestOf.SelectedIndex == 1) input.Send_CtrlShiftKeyPress((short)(0x30 + 3)); // setting bo3 not needed
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

            // open production tab
            input.Send_NormalKeyPress((byte)'D');
        }
    }
}
