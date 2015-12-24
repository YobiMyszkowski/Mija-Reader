using System.Windows.Controls;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Mija_Reader.AdditionalControls
{
    public partial class SearchTextBox : UserControl
    {
        public SearchTextBox()
        {
            InitializeComponent();
        }
        public event TextChangedEventHandler TextChanged;

        public static readonly DependencyProperty WatermarkProperty =
            DependencyProperty.Register("Watermark",
                                        typeof(string),
                                        typeof(SearchTextBox),
                                        new UIPropertyMetadata(string.Empty));

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text",
                                        typeof(string),
                                        typeof(SearchTextBox),
                                        new UIPropertyMetadata(string.Empty));

        public static readonly DependencyProperty TextChangedProperty =
            DependencyProperty.Register("TextChanged",
                                        typeof(TextChangedEventHandler),
                                        typeof(SearchTextBox),
                                        new UIPropertyMetadata(null));

        public string Watermark
        {
            get { return (string)GetValue(WatermarkProperty); }
            set
            {
                SetValue(WatermarkProperty, value);
            }
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TextChanged != null) TextChanged(this, e);
        }

        public void Clear()
        {
            textBox.Clear();
        }
    }
    class ShadowConverter : IMultiValueConverter
    {
        #region Implementation of IMultiValueConverter

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var text = (string)values[0];
            return text == string.Empty
                       ? Visibility.Visible
                       : Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return new object[0];
        }

        #endregion
    }
}
