using System.Collections.Generic;

namespace OvoData.Models.OvoApi;

public class DailyUtility
{
    public List<DailyDataItem>? Data { get; set; }
    public bool Next { get; set; }
    public bool Prev { get; set; }
    public bool PrevYear { get; set; }
}