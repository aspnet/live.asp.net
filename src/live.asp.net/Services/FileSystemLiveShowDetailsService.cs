// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileSystemLiveShowDetailsService.cs" company=".NET Foundation">
//   Copyright (c) .NET Foundation. All rights reserved.
//   Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace live.asp.net.Services
{
    using System;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Threading.Tasks;

    using live.asp.net.Models;

    using Microsoft.Framework.Caching.Memory;
    using Microsoft.Framework.Runtime;

    using Newtonsoft.Json;

    /// <summary>
    /// The file system live show details service.
    /// </summary>
    public class FileSystemLiveShowDetailsService : ILiveShowDetailsService
    {
        #region Static Fields

        /// <summary>
        /// The <see cref="cache"/> key.
        /// </summary>
        private static readonly string CacheKey = nameof(FileSystemLiveShowDetailsService);

        /// <summary>
        /// The file name.
        /// </summary>
        private static readonly string FileName = "ShowDetails.json";

        #endregion

        #region Fields

        /// <summary>
        /// The application environment.
        /// </summary>
        private readonly IApplicationEnvironment appEnv;

        /// <summary>
        /// The cache.
        /// </summary>
        private readonly IMemoryCache cache;

        /// <summary>
        /// The file path.
        /// </summary>
        private readonly string filePath;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FileSystemLiveShowDetailsService"/> class.
        /// </summary>
        /// <param name="appEnv">
        /// The application environment.
        /// </param>
        /// <param name="cache">
        /// The cache.
        /// </param>
        public FileSystemLiveShowDetailsService(IApplicationEnvironment appEnv, IMemoryCache cache)
        {
            Contract.Requires(appEnv != null);
            Contract.Requires(cache != null);
            this.appEnv = appEnv;
            this.cache = cache;
            this.filePath = Path.Combine(this.appEnv.ApplicationBasePath, FileName);
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Loads the live show details from <see cref="cache"/> or file.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> of <see cref="LiveShowDetails"/>.
        /// </returns>
        public async Task<LiveShowDetails> LoadAsync()
        {
            var result = this.cache.Get<LiveShowDetails>(CacheKey);

            if (result == null)
            {
                result = await this.LoadFromFile();

                this.cache.Set(CacheKey, result, new MemoryCacheEntryOptions { AbsoluteExpiration = DateTimeOffset.MaxValue });
            }

            return result;
        }

        /// <summary>
        /// Saves the live show details.
        /// </summary>
        /// <param name="liveShowDetails">
        /// The live show details.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/>.
        /// </returns>
        public async Task SaveAsync(LiveShowDetails liveShowDetails)
        {
            if (liveShowDetails == null)
            {
                return;
            }

            var fileContents = JsonConvert.SerializeObject(liveShowDetails);
            using (var fileWriter = new StreamWriter(this.filePath))
            {
                await fileWriter.WriteAsync(fileContents);
            }

            this.cache.Remove(CacheKey);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Loads the live show details from a file.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> of <see cref="LiveShowDetails"/>.
        /// </returns>
        private async Task<LiveShowDetails> LoadFromFile()
        {
            if (!File.Exists(this.filePath))
            {
                return null;
            }

            string fileContents;
            using (var fileReader = new StreamReader(this.filePath))
            {
                fileContents = await fileReader.ReadToEndAsync();
            }

            return JsonConvert.DeserializeObject<LiveShowDetails>(fileContents);
        }

        #endregion
    }
}