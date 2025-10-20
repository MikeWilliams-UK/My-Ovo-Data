using OvoData.Models.Database;
using System.Data.SQLite;
using System.Text;

namespace OvoData.Helpers;

public partial class SqLiteHelper
{
    public void UpsertSupplyPoint(SupplyPoint supplyPoint)
    {
        using (var connection = GetConnection())
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("INSERT INTO SupplyPoint");
            stringBuilder.AppendLine("VALUES");
            stringBuilder.AppendLine($"('{supplyPoint.Sprn}', '{supplyPoint.Type}')");
            stringBuilder.AppendLine("ON CONFLICT (Sprn)");
            stringBuilder.AppendLine("DO UPDATE SET Sprn = excluded.Sprn, FuelType = excluded.FuelType");

            var command = new SQLiteCommand(stringBuilder.ToString(), connection);
            command.ExecuteNonQuery();
        }
    }
}