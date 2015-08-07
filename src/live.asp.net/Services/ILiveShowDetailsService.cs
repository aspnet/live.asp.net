// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ILiveShowDetailsService.cs" company=".NET Foundation">
//   Copyright (c) .NET Foundation. All rights reserved.
//   Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace live.asp.net.Services
{
    using System.Threading.Tasks;

    using live.asp.net.Models;

    /// <summary>
    /// The live show details service <see langword="interface"/>.
    /// </summary>
    public interface ILiveShowDetailsService
    {
        #region Public Methods and Operators

        /// <summary>
        /// Loads the live show details.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/> of <see cref="LiveShowDetails"/>.
        /// </returns>
        Task<LiveShowDetails> LoadAsync();

        /// <summary>
        /// Saves the live show details.
        /// </summary>
        /// <param name="liveShowDetails">
        /// The live show details.
        /// </param>
        /// <returns>
        /// A Task.
        /// </returns>
        Task SaveAsync(LiveShowDetails liveShowDetails);

        #endregion
    }
}
