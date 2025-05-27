using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;

namespace ClockApp.Scripts.Domain.Common
{
    public class NtpTimeProvider : ITimeProvider
    {
        private readonly string[] _ntpServers = new[]
        {
            "time.google.com",
            "time.windows.com",
            "pool.ntp.org",
            "time.nist.gov"
        };
        
        private TimeSpan _offset = TimeSpan.Zero;
        private DateTime _lastSyncTime = DateTime.MinValue;
        private readonly TimeSpan _syncValidityDuration = TimeSpan.FromHours(1);
        
        public IObservable<DateTime?> GetNetworkTime()
        {
            return Observable.Start(() =>
            {
                var task = GetNetworkTimeInternalAsync();
                task.Wait();
                return task.Result;
            }, Scheduler.ThreadPool)
            .ObserveOnMainThread();
        }
        
        private async Task<DateTime?> GetNetworkTimeInternalAsync()
        {
            DateTime? networkTime = null;
            
            // Try each NTP server until one succeeds
            foreach (var server in _ntpServers)
            {
                try
                {
                    networkTime = await GetNtpTimeAsync(server);
                    if (networkTime.HasValue)
                    {
                        UpdateOffset(networkTime.Value);
                        Debug.Log($"Successfully synced with NTP server: {server}");
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Failed to sync with {server}: {ex.Message}");
                }
            }
            
            return networkTime;
        }
        
        public DateTime GetCurrentTime()
        {
            return GetUtcTime().ToLocalTime();
        }
        
        public DateTime GetUtcTime()
        {
            if (IsSyncValid())
            {
                return DateTime.UtcNow + _offset;
            }
            
            return DateTime.UtcNow;
        }
        
        private bool IsSyncValid()
        {
            return _lastSyncTime != DateTime.MinValue && 
                   DateTime.UtcNow - _lastSyncTime < _syncValidityDuration;
        }
        
        private void UpdateOffset(DateTime networkTime)
        {
            _offset = networkTime - DateTime.UtcNow;
            _lastSyncTime = DateTime.UtcNow;
        }
        
        private async Task<DateTime?> GetNtpTimeAsync(string ntpServer, int timeout = 3000)
        {
            var ntpData = new byte[48];
            ntpData[0] = 0x1B; // LI = 0 (no warning), VN = 3 (IPv4 only), Mode = 3 (client)
            
            try
            {
                using (var socket = new UdpClient())
                {
                    socket.Client.ReceiveTimeout = timeout;
                    socket.Client.SendTimeout = timeout;
                    
                    // Resolve server address
                    var addresses = await Dns.GetHostAddressesAsync(ntpServer);
                    if (addresses.Length == 0)
                    {
                        Debug.LogWarning($"Could not resolve NTP server: {ntpServer}");
                        return null;
                    }
                    
                    var serverEndpoint = new IPEndPoint(addresses[0], 123);
                    
                    // Send request
                    await socket.SendAsync(ntpData, ntpData.Length, serverEndpoint);
                    
                    // Receive response
                    var receiveTask = socket.ReceiveAsync();
                    var timeoutTask = Task.Delay(timeout);
                    
                    var completedTask = await Task.WhenAny(receiveTask, timeoutTask);
                    
                    if (completedTask == timeoutTask)
                    {
                        Debug.LogWarning($"NTP request to {ntpServer} timed out");
                        return null;
                    }
                    
                    var response = await receiveTask;
                    
                    // Validate response
                    if (response.Buffer.Length < 48)
                    {
                        Debug.LogWarning($"Invalid NTP response size from {ntpServer}");
                        return null;
                    }
                    
                    // Extract timestamp from bytes 40-47 (Transmit Timestamp)
                    var intPart = ExtractUInt32(response.Buffer, 40);
                    var fracPart = ExtractUInt32(response.Buffer, 44);
                    
                    // Convert to DateTime
                    var milliseconds = (intPart * 1000L) + ((fracPart * 1000L) / 0x100000000L);
                    var networkDateTime = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                        .AddMilliseconds(milliseconds);
                    
                    // Sanity check - time should be reasonable
                    var yearDiff = Math.Abs((networkDateTime - DateTime.UtcNow).TotalDays);
                    if (yearDiff > 365)
                    {
                        Debug.LogWarning($"NTP time seems incorrect: {networkDateTime}");
                        return null;
                    }
                    
                    return networkDateTime;
                }
            }
            catch (SocketException ex)
            {
                Debug.LogWarning($"Socket error connecting to {ntpServer}: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Unexpected error in NTP request to {ntpServer}: {ex.Message}");
                return null;
            }
        }
        
        private uint ExtractUInt32(byte[] buffer, int offset)
        {
            // Network byte order (big-endian) to host byte order
            return ((uint)buffer[offset] << 24) |
                   ((uint)buffer[offset + 1] << 16) |
                   ((uint)buffer[offset + 2] << 8) |
                   ((uint)buffer[offset + 3]);
        }
    }
    
    /// <summary>
    /// Simple time provider that uses system time (no NTP)
    /// Use this for testing or when network is not available
    /// </summary>
    public class SystemTimeProvider : ITimeProvider
    {
        public IObservable<DateTime?> GetNetworkTime()
        {
            // Просто возвращаем текущее время как "сетевое"
            return Observable.Return<DateTime?>(DateTime.UtcNow);
        }
        
        public DateTime GetCurrentTime()
        {
            return DateTime.Now;
        }
        
        public DateTime GetUtcTime()
        {
            return DateTime.UtcNow;
        }
    }
}