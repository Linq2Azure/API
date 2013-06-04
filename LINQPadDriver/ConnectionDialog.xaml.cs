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
using LINQPad.Extensibility.DataContext;

namespace Linq2Azure.LINQPadDriver
{
    /// <summary>
    /// Interaction logic for ConnectionDialog.xaml
    /// </summary>
    public partial class ConnectionDialog : Window
    {
        IConnectionInfo _cxInfo;
        Linq2AzureProperties _props;

        public ConnectionDialog(IConnectionInfo cxInfo)
		{
			_cxInfo = cxInfo;
            DataContext = _props = new Linq2AzureProperties(cxInfo);
            InitializeComponent();
        }

        void btnOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        void BrowsePublishSettingsFile(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog()
            {
                Title = "Choose custom assembly",
                DefaultExt = ".publishsettings",
            };

            if (dialog.ShowDialog() == true)
                txtPublishSettingsPath.Text = _props.PublishSettingsPath = dialog.FileName;
        }
    }
}
