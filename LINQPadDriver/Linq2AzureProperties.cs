using LINQPad.Extensibility.DataContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Linq2Azure.LINQPadDriver
{
    class Linq2AzureProperties
    {
        readonly IConnectionInfo _cxInfo;
		readonly XElement _driverData;

        public Linq2AzureProperties(IConnectionInfo cxInfo)
		{
			_cxInfo = cxInfo;
			_driverData = cxInfo.DriverData;
		}

        public string DisplayName
        {
            get { return _cxInfo.DisplayName; }
            set { _cxInfo.DisplayName = value; }
        }

		public bool Persist
		{
			get { return _cxInfo.Persist; }
			set { _cxInfo.Persist = value; }
		}

		public string PublishSettingsPath
		{
            get { return (string)_driverData.Element("PublishSettingsPath") ?? ""; }
            set { _driverData.SetElementValue("PublishSettingsPath", value); }
		}
    }
}
