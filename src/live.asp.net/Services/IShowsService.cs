// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IShowsService.cs" company=".NET Foundation">
//   Copyright (c) .NET Foundation. All rights reserved.
//   Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace live.asp.net.Services
{
    using System.Security.Claims;
    using System.Threading.Tasks;

    /// <summary>
    ///     The show service <see langword="interface" /> .
    /// </summary>
    public interface IShowsService
    {
        #region Public Methods and Operators

        /// <summary>
        /// Gets the recorded shows asynchronously.
        /// </summary>
        /// <param name="user">
        /// The <paramref name="user"/> .
        /// </param>
        /// <param name="disableCache">
        /// A value indicating whether cache should be disabled.
        /// </param>
        /// <returns>
        /// A Task of <see cref="ShowList"/> .
        /// </returns>
        Task<ShowList> GetRecordedShowsAsync(ClaimsPrincipal user, bool disableCache);

        #endregion
    }
}