using SocialMedia.Core.QueryFilters;
using System;

namespace SocialMedia.Infrastructure.Interfaces
{
    public interface IUriService
    {
        Uri getPostPagintationUri(PostQueryFilter filter, string actionUrl);
    }
}