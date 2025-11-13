PRAGMA foreign_keys = 0;

-- -- MeterRegisters --

CREATE TABLE temp_table_1 AS SELECT * FROM MeterRegisters;
DROP TABLE MeterRegisters;
CREATE TABLE MeterRegisters(StartDate STRING NOT NULL, EndDate STRING, FuelType STRING NOT NULL, MeterSerialNumber STRING NOT NULL, Id STRING NOT NULL, TimingCategory STRING, UnitOfMeasurement STRING, PRIMARY KEY (StartDate ASC, MeterSerialNumber ASC, Id ASC));
INSERT INTO MeterRegisters (StartDate, EndDate, FuelType, Id, TimingCategory, UnitOfMeasurement, MeterSerialNumber) SELECT StartDate, EndDate, FuelType, Id, TimingCategory, UnitOfMeasurement, "?" FROM temp_table_1;
DROP TABLE temp_table_1;

CREATE UNIQUE INDEX Idx_MeterRegisters_2 ON MeterRegisters (StartDate ASC, MeterSerialNumber ASC, Id ASC);

-- -- MeterReadings --

CREATE TABLE temp_table_2 AS SELECT * FROM MeterReadings;
DROP TABLE MeterReadings;
CREATE TABLE MeterReadings(Date STRING NOT NULL, MeterSerialNumber STRING, FuelType STRING NOT NULL, LifeCycle STRING, RegisterId STRING, Source STRING, TimingCategory STRING, Type STRING, Value DOUBLE NOT NULL, PRIMARY KEY (Date ASC, FuelType ASC, RegisterId ASC) );
INSERT INTO MeterReadings (Date, MeterSerialNumber, FuelType, LifeCycle, RegisterId, Source, TimingCategory, Type, Value ) SELECT Date, MeterSerialNumber, FuelType, LifeCycle, RegisterId, Source, TimingCategory, Type, Value FROM temp_table_2;
DROP TABLE temp_table_2;

CREATE UNIQUE INDEX Idx_Readings_2 ON MeterReadings (Date ASC, FuelType ASC, RegisterId ASC);

-- ----

PRAGMA foreign_keys = 1; 