-- Drop V1.04 and earlier surplus table
-- DROP TABLE Information;

CREATE TABLE SupplyPoints (Sprn STRING PRIMARY KEY UNIQUE NOT NULL, FuelType STRING NOT NULL);

CREATE TABLE Meters (SerialNumber STRING PRIMARY KEY UNIQUE NOT NULL, FuelType STRING NOT NULL, MeterType STRING NOT NULL, Status STRING NOT NULL);
CREATE INDEX Idx_Meter ON Meters (SerialNumber ASC);

CREATE TABLE MeterRegisters (StartDate DATE PRIMARY KEY NOT NULL, EndDate DATE, FuelType STRING NOT NULL, Id STRING NOT NULL, TimingCategory STRING NOT NULL, UnitOfMeasurement STRING NOT NULL);
CREATE INDEX Idx_MeterRegister ON MeterRegisters (StartDate ASC);

CREATE TABLE Readings (Date DATE PRIMARY KEY NOT NULL, MeterSerialNumber STRING NOT NULL, FuelType STRING NOT NULL, LifeCycle STRING NOT NULL, RegisterId STRING NOT NULL, Source STRING NOT NULL, TimingCategory STRING NOT NULL, Type STRING NOT NULL, Value DOUBLE NOT NULL);
CREATE INDEX Idx_Reading ON Readings (Date ASC, MeterSerialNumber ASC);
