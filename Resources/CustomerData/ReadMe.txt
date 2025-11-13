Json files from customers go here

Use in C# code like this ...
var customerReadings = ResourceHelper.GetStringResource("CustomerData.Readings.json");

Then
  change
var readingsResponse = JsonSerializer.Deserialize<ReadingsResponse>(responseContent, JsonSerializerOptions);
  to
var readingsResponse = JsonSerializer.Deserialize<ReadingsResponse>(customerReadings, JsonSerializerOptions);
