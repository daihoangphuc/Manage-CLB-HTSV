using PagedList;

namespace website_CLB_HTSV.Extensions
{
    public static class PagedListExtensions
    {
        public static IPagedList<T> ToCustomPagedList<T>(this IEnumerable<T> source, int pageNumber, int pageSize)
        {
            var totalCount = source.Count();
            var items = source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            return new StaticPagedList<T>(items, pageNumber, pageSize, totalCount);
        }
    }
}
