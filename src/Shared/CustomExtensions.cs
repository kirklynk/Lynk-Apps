using System;
using System.Collections.Generic;
using System.Text;

namespace Shared
{
    public static class CustomExtensions
    {
        public static string FormatFileSize(this long bytes)
        {
            const double kiloByte = 1024d;
            const double megaByte = kiloByte * 1024d;
            const double gigaByte = megaByte * 1024d;

            var absoluteBytes = Math.Abs((double)bytes);

            if (absoluteBytes >= gigaByte)
            {
                return $"{bytes / gigaByte:0.##} GB";
            }

            if (absoluteBytes >= megaByte)
            {
                return $"{bytes / megaByte:0.##} MB";
            }

            if (absoluteBytes >= kiloByte)
            {
                return $"{bytes / kiloByte:0.##} KB";
            }

            return $"{bytes} B";
        }
    }
}
