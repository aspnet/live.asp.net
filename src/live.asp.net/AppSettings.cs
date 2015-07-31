using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace live.asp.net
{
    public class AppSettings
    {
        public string YouTubeApplicationName { get; set; }

        public string YouTubeApiKey { get; set; }

        public string YouTubePlaylistId { get; set; }

        public string AzureStorageConnectionString { get; set; }

        public string AzureStorageBlobName { get; set; }

        public string AzureStorageContainerName { get; set; }
    }
}
