using Shared.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;

namespace Shared.Common.Interfaces
{
    public interface ISharingService
    {
        Task<QuerySet<ShareRequest>> QueryAsync(int skip, int take, string? orderBy, bool descending, CancellationToken cancellationToken);
    }
}
