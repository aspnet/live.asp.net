// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DateAfterNowAttribute.cs" company=".NET Foundation">
//   Copyright (c) .NET Foundation. All rights reserved.
//   Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace live.asp.net.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// The date after now attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class DateAfterNowAttribute : ValidationAttribute
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DateAfterNowAttribute"/> class.
        /// </summary>
        public DateAfterNowAttribute()
            : base("The supplied date must be in the future.")
        {
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the time zone identifier.
        /// </summary>
        /// <value>The time zone identifier.</value>
        public string TimeZoneId { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Whether or not the <paramref name="value"/> is valid.
        /// </summary>
        /// <param name="value">
        /// The <paramref name="value"/>.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public override bool IsValid(object value)
        {
            if (value == null)
            {
                return true;
            }

            if (value is DateTime)
            {
                var now = DateTime.UtcNow;
                if (!string.IsNullOrEmpty(this.TimeZoneId))
                {
                    var timeZone = TimeZoneInfo.FindSystemTimeZoneById(this.TimeZoneId);
                    now = TimeZoneInfo.ConvertTimeFromUtc(now, timeZone);
                }

                return (DateTime)value > now;
            }

            return false;
        }

        #endregion
    }
}
