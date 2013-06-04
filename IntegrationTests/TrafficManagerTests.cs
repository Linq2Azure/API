using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Linq2Azure;
using Linq2Azure.TrafficManagement;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;

namespace IntegrationTests
{
    [TestClass]
    public class TrafficManagerTests : IDisposable
    {
        public readonly Subscription Subscription = TestConstants.Subscription;
        public readonly CloudServiceTests Parent = new CloudServiceTests();
        public readonly TrafficManagerProfile Profile;

        public TrafficManagerTests()
        {
            Debug.WriteLine("Creating traffic manager profile");
            Profile = new TrafficManagerProfile("Test-" + Guid.NewGuid().ToString("N"), Parent.CloudService.Name + ".trafficmanager.net");
            Profile.CreateAsync(Subscription).Wait();
        }

        async Task<TrafficManagerProfile[]> GetProfilesAsync()
        {
            return (await Subscription.TrafficManagerProfiles.AsTask()).Where(p => p.Name == Profile.Name).ToArray();
        }

        [TestMethod]
        public async Task CanUseTrafficManager()
        {
            Debug.WriteLine("Retrieving traffic manager profile");
            var profiles = await GetProfilesAsync();
            Assert.AreEqual(profiles.Length, 1);
            Assert.AreEqual(profiles[0].Name, Profile.Name);

            var monitor = new TrafficManagerMonitor(80, new TrafficManagerHttpOptions ("/foo"))
            {
                Protocol = MonitorProtocol.HTTPS
            };

            var policy = new TrafficManagerPolicy (new[] { new TrafficManagerEndpoint (Parent.CloudService.Name + ".cloudapp.net") })
            {
                LoadBalancingMethod = LoadBalancingMethod.RoundRobin
            };
            
            var defn = new TrafficManagerDefinition(3600, new[] { monitor }, policy);

            Debug.WriteLine("Creating traffic manager definition");
            await defn.CreateAsync (Profile);

            Debug.WriteLine("Retrieving traffic manager definition");
            var allDefns = await Profile.Definitions.AsTask();
            Assert.AreEqual(1, allDefns.Length);
            
            var defn2 = allDefns[0];
            Assert.AreEqual(defn.DnsTtlInSeconds, defn2.DnsTtlInSeconds);
            
            Assert.AreEqual(1, defn2.Monitors.Count);
            Assert.AreEqual(monitor.IntervalInSeconds, defn2.Monitors[0].IntervalInSeconds);
            Assert.AreEqual(monitor.TimeoutInSeconds, defn2.Monitors[0].TimeoutInSeconds);
            Assert.AreEqual(monitor.Protocol, defn2.Monitors[0].Protocol);

            Assert.AreEqual(policy.LoadBalancingMethod, defn2.Policy.LoadBalancingMethod);
            Assert.AreEqual(1, defn2.Policy.EndPoints.Count);
            Assert.AreEqual(policy.EndPoints[0].DomainName, defn2.Policy.EndPoints[0].DomainName);

            Debug.WriteLine("Enabling profile");
            await Profile.EnableProfileAsync(1);
            Assert.AreEqual(true, (await Subscription.TrafficManagerProfiles.AsTask())[0].Enabled);
        }

        public void Dispose()
        {
            if (Profile != null && Profile.Subscription != null)
            {
                Debug.WriteLine("Deleting traffic manager profile");
                Profile.DeleteAsync().Wait();
                if (GetType() == typeof(TrafficManagerTests)) VerifyDeletion();
            }
        }

        void VerifyDeletion()
        {
            Debug.WriteLine("Verifying traffic manager profile deletion");
            Assert.AreEqual(0, GetProfilesAsync().Result.Length);
        }
    }
}
