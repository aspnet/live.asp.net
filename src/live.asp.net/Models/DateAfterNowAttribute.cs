using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace live.asp.net.Models
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class DateAfterNowAttribute : ValidationAttribute
    {
        public DateAfterNowAttribute()
            : base("The supplied date must be in the future.")
        {

        }

        public string TimeZoneId { get; set; }

        public override bool IsValid(object value)
        {
            if (value == null)
            {
                return true;
            }

            if (!(value is DateTime))
            {
                return false;
            }

            var now = DateTime.UtcNow;
            if (!string.IsNullOrEmpty(TimeZoneId))
            {
                var timeZone = TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId);
                now = TimeZoneInfo.ConvertTimeFromUtc(now, timeZone);
            }
            
            return (DateTime)value > now;
        }
    }
}
