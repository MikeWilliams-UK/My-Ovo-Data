using System.Collections.Generic;

namespace OvoData.Models.OvoApi.Usage;

public class HalfHourlyUtility
{
    public List<HalfHourlyDataItem>? Data { get; set; }
    public bool Next { get; set; }
    public bool Prev { get; set; }
}