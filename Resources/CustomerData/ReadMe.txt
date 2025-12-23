Json files from customers go here

Use in C# code like this ...
in HttpHelper.cs
	Function
		public List<Models.MySupplyPoint> ObtainMeterReadings(string accountId)

  Before change line 452
  Add :-
		responseContent =  = ResourceHelper.GetStringResource("CustomerData.Readings.json");