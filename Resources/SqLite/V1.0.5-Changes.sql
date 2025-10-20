-- Drop V1.04 and earlier surplus table
-- DROP TABLE Information;

CREATE TABLE SupplyPoints (Sprn STRING PRIMARY KEY UNIQUE NOT NULL, FuelType STRING NOT NULL);

CREATE TABLE Meters (SerialNumber STRING PRIMARY KEY UNIQUE NOT NULL, FuelType STRING NOT NULL, MeterType STRING NOT NULL, Status STRING NOT NULL);
CREATE INDEX Idx_Meters ON Meters (SerialNumber ASC);

CREATE TABLE MeterRegisters (StartDate STRING PRIMARY KEY NOT NULL, EndDate STRING, FuelType STRING NOT NULL, Id STRING NOT NULL, TimingCategory STRING NOT NULL, UnitOfMeasurement STRING NOT NULL);
CREATE INDEX Idx_MeterRegisters ON MeterRegisters (StartDate ASC);

CREATE TABLE MeterReadings (Date STRING PRIMARY KEY NOT NULL, MeterSerialNumber STRING NOT NULL, FuelType STRING NOT NULL, LifeCycle STRING NOT NULL, RegisterId STRING NOT NULL, Source STRING NOT NULL, TimingCategory STRING NOT NULL, Type STRING NOT NULL, Value DOUBLE NOT NULL);
CREATE INDEX Idx_Readings ON MeterReadings (Date ASC, MeterSerialNumber ASC);
