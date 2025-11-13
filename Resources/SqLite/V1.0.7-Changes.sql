PRAGMA foreign_keys = 0;

CREATE TABLE sqlitestudio_temp_table AS SELECT * FROM MeterRegisters;
DROP TABLE MeterRegisters;
CREATE TABLE MeterRegisters(StartDate STRING NOT NULL, EndDate STRING, FuelType STRING NOT NULL, Id STRING NOT NULL, TimingCategory STRING, UnitOfMeasurement STRING, MeterSerialNumber STRING NOT NULL, PRIMARY KEY (StartDate ASC, MeterSerialNumber ASC, Id ASC));
INSERT INTO MeterRegisters (StartDate, EndDate, FuelType, Id, TimingCategory, UnitOfMeasurement, MeterSerialNumber) SELECT StartDate, EndDate, FuelType, Id, TimingCategory, UnitOfMeasurement, "?" FROM sqlitestudio_temp_table;
DROP TABLE sqlitestudio_temp_table;
CREATE UNIQUE INDEX Idx_MeterRegisters_2 ON MeterRegisters (StartDate ASC, MeterSerialNumber ASC, Id ASC);

PRAGMA foreign_keys = 1;