using System.Collections.Generic;

namespace OvoData.Models.OvoApi;

public class MonthlyUtility
{
    public bool Prev { get; set; }
    public bool Next { get; set; }
    public List<MonthlyDataItem> Data { get; set; } = new List<MonthlyDataItem>();
}