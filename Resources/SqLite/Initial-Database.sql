CREATE TABLE Information (AccountId STRING PRIMARY KEY NOT NULL UNIQUE, FirstMonth STRING, LastMonth STRING, FirstDay STRING, LastDay STRING);

CREATE TABLE MonthlyElectric (Month STRING PRIMARY KEY NOT NULL UNIQUE, Mxpn STRING, Consumption DOUBLE, Cost DOUBLE);
CREATE INDEX Idx_MonthlyElectric ON MonthlyElectric (Month ASC);

CREATE TABLE MonthlyGas (Month STRING PRIMARY KEY NOT NULL UNIQUE, Mxpn STRING, Consumption DOUBLE, Cost DOUBLE);
CREATE INDEX Idx_MonthlyGas ON MonthlyGas (Month ASC);

CREATE TABLE DailyElectric (Day STRING PRIMARY KEY NOT NULL UNIQUE, Consumption DOUBLE, Cost DOUBLE, Standing DOUBLE, AnyTime DOUBLE, Peak DOUBLE, OffPeak DOUBLE, HasHhData BOOLEAN);
CREATE INDEX Idx_DailyElectric ON DailyElectric (Day ASC);

CREATE TABLE DailyGas (Day STRING PRIMARY KEY NOT NULL UNIQUE, Consumption DOUBLE, Cost DOUBLE, Standing DOUBLE, AnyTime DOUBLE, Peak DOUBLE, OffPeak DOUBLE, HasHhData BOOLEAN);
CREATE INDEX Idx_DailyGas ON DailyGas (Day ASC);

CREATE TABLE HalfHourlyElectric (StartTime STRING PRIMARY KEY UNIQUE NOT NULL, Consumption DOUBLE);
CREATE INDEX Idx_HalfHourlyElectric ON HalfHourlyElectric (StartTime ASC);

CREATE TABLE HalfHourlyGas (StartTime STRING PRIMARY KEY UNIQUE NOT NULL, Consumption DOUBLE);
CREATE INDEX Idx_HalfHourlyGas ON HalfHourlyGas (StartTime ASC);