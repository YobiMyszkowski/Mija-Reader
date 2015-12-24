using System;
using System.Windows;
using System.Windows.Media;

namespace Mija_Reader.AdditionalControls
{
    public enum MessageBoxMessage
    {
        information,
        warning,
        error
    }
    public partial class MetroMessageBox : Window
    {
        
        public MetroMessageBox()
        {
            InitializeComponent();
            DataContext = this;
        }

        public void ShowMessage(Window owner, string message, string title, MessageBoxMessage icon = MessageBoxMessage.information, MessageBoxButton buttons = MessageBoxButton.OK)
        {
            try
            {
                inactivityPanel.Visibility = Visibility.Visible;
                MessageBoxPanel.Visibility = Visibility.Visible;
                MessageBoxTitle.Text = title;
                MessageBoxText.Text = message;

                this.Owner = owner;
                if (Owner != null)
                {
                    if (Owner.WindowState == WindowState.Maximized)
                    {
                        Left = 0;
                        Top = 0;
                        Width = Owner.ActualWidth;
                        Height = Owner.ActualHeight;
                    }
                    else
                    {
                        Left = Owner.Left;
                        Top = Owner.Top;
                        Width = Owner.ActualWidth;
                        Height = Owner.ActualHeight;
                    }
                }

                if (icon == MessageBoxMessage.information)
                {
                    MessageBoxIcon.Data = Geometry.Parse("F1 M 38,19C 48.4934,19 57,27.5066 57,38C 57,48.4934 48.4934,57 38,57C 27.5066,57 19,48.4934 19,38C 19,27.5066 27.5066,19 38,19 Z M 33.25,33.25L 33.25,36.4167L 36.4166,36.4167L 36.4166,47.5L 33.25,47.5L 33.25,50.6667L 44.3333,50.6667L 44.3333,47.5L 41.1666,47.5L 41.1666,36.4167L 41.1666,33.25L 33.25,33.25 Z M 38.7917,25.3333C 37.48,25.3333 36.4167,26.3967 36.4167,27.7083C 36.4167,29.02 37.48,30.0833 38.7917,30.0833C 40.1033,30.0833 41.1667,29.02 41.1667,27.7083C 41.1667,26.3967 40.1033,25.3333 38.7917,25.3333 Z");
                }
                else if (icon == MessageBoxMessage.warning)
                {
                    MessageBoxIcon.Data = Geometry.Parse("F1 M 58.5832,55.4172L 17.4169,55.4171C 15.5619,53.5621 15.5619,50.5546 17.4168,48.6996L 35.201,15.8402C 37.056,13.9852 40.0635,13.9852 41.9185,15.8402L 58.5832,48.6997C 60.4382,50.5546 60.4382,53.5622 58.5832,55.4172 Z M 34.0417,25.7292L 36.0208,41.9584L 39.9791,41.9583L 41.9583,25.7292L 34.0417,25.7292 Z M 38,44.3333C 36.2511,44.3333 34.8333,45.7511 34.8333,47.5C 34.8333,49.2489 36.2511,50.6667 38,50.6667C 39.7489,50.6667 41.1666,49.2489 41.1666,47.5C 41.1666,45.7511 39.7489,44.3333 38,44.3333 Z");
                }
                else
                {
                    MessageBoxIcon.Data = Geometry.Parse("F1 M 38,3.16666C 57.2379,3.16666 72.8333,18.7621 72.8333,38C 72.8333,57.2379 57.2379,72.8333 38,72.8333C 18.7621,72.8333 3.16667,57.2379 3.16667,38C 3.16667,18.7621 18.7621,3.16666 38,3.16666 Z M 52.252,18.9974L 36.4164,18.9974L 23.75,39.5833L 34.8333,39.5833L 25.3316,60.1667L 50.6667,34.8333L 38,34.8333L 52.252,18.9974 Z");
                }

                if(buttons == MessageBoxButton.OK)
                {
                    MessageBoxBtnYes.Visibility = Visibility.Visible;
                    MessageBoxBtnNo.Visibility = Visibility.Collapsed;
                    MessageBoxBtnCancel.Visibility = Visibility.Collapsed;
                }
                else if(buttons == MessageBoxButton.OKCancel)
                {
                    MessageBoxBtnYes.Visibility = Visibility.Visible;
                    MessageBoxBtnNo.Visibility = Visibility.Collapsed;
                    MessageBoxBtnCancel.Visibility = Visibility.Visible;
                }
                else if (buttons == MessageBoxButton.YesNo)
                {
                    MessageBoxBtnYes.Visibility = Visibility.Visible;
                    MessageBoxBtnNo.Visibility = Visibility.Visible;
                    MessageBoxBtnCancel.Visibility = Visibility.Collapsed;
                }
                else if (buttons == MessageBoxButton.YesNoCancel)
                {
                    MessageBoxBtnYes.Visibility = Visibility.Visible;
                    MessageBoxBtnNo.Visibility = Visibility.Visible;
                    MessageBoxBtnCancel.Visibility = Visibility.Visible;
                }
                else
                {
                    MessageBoxBtnYes.Visibility = Visibility.Visible;
                    MessageBoxBtnNo.Visibility = Visibility.Collapsed;
                    MessageBoxBtnCancel.Visibility = Visibility.Collapsed;
                }
                this.ShowDialog();
            }
            finally
            {
            }
        }
    }
}