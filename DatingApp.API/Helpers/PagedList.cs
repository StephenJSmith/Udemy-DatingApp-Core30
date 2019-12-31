using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Helpers
{
  public class PagedList<T> : List<T>
  {
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }

    public PagedList(List<T> items, int count, int pageNumber, int pageSize)
    {
      TotalCount = count;
      PageSize = pageSize;
      CurrentPage = pageNumber;
      TotalPages = (int)Math.Ceiling(count / (double)pageSize);
      AddRange(items);
    }

    public static async Task<PagedList<T>> CreateAsync(
        IQueryable<T> source,
        int pageNumber,
        int pageSize)
    {
      var count = await source.CountAsync();
      var skipCount = (pageNumber - 1) * pageSize;
      var items = await source
          .Skip(skipCount)
          .Take(pageSize)
          .ToListAsync();

      return new PagedList<T>(items, count, pageNumber, pageSize);
    }
  }
}