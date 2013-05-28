using Linq2Azure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationTests
{
    class TestConstants
    {
        public const string ManagementCertificatePath = @"c:\temp\Linq2Azure Development-5-27-2013-credentials.publishsettings";
        public static readonly Subscription Subscription = Subscription.FromPublisherSettingsPath(TestConstants.ManagementCertificatePath);
    }
}
