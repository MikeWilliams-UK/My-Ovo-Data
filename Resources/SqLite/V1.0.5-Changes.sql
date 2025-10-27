-- Drop V1.04 and earlier surplus table
DROP TABLE Information;

-- Create new tables for V1.0.5
CREATE TABLE SupplyPoints (Sprn STRING PRIMARY KEY UNIQUE NOT NULL, FuelType STRING NOT NULL);

CREATE TABLE Meters (SerialNumber STRING PRIMARY KEY UNIQUE NOT NULL, FuelType STRING NOT NULL, MeterType STRING, Status STRING);
CREATE UNIQUE INDEX Idx_Meters ON Meters (SerialNumber ASC);

CREATE TABLE MeterRegisters (StartDate STRING PRIMARY KEY NOT NULL, EndDate STRING, FuelType STRING NOT NULL, Id STRING, TimingCategory STRING, UnitOfMeasurement STRING);
CREATE UNIQUE INDEX Idx_MeterRegisters ON MeterRegisters (StartDate ASC);

CREATE TABLE MeterReadings (Date STRING NOT NULL, MeterSerialNumber STRING, FuelType STRING NOT NULL, LifeCycle STRING, RegisterId STRING, Source STRING, TimingCategory STRING, Type STRING, Value DOUBLE NOT NULL, PRIMARY KEY (Date ASC, FuelType ASC));
CREATE UNIQUE INDEX Idx_Readings ON MeterReadings (Date ASC, FuelType ASC);