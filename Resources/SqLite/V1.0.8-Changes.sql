-- Drop V1.04 and earlier surplus table
DROP TABLE IF EXISTS Information;

PRAGMA foreign_keys = 0;

CREATE TABLE temp_table_3 AS SELECT * FROM SupplyPoints;

DROP TABLE SupplyPoints;

CREATE TABLE SupplyPoints (Sprn STRING PRIMARY KEY UNIQUE NOT NULL, FuelType STRING NOT NULL, StartDate STRING NOT NULL, EndDate STRING );
INSERT INTO SupplyPoints (Sprn, FuelType) SELECT Sprn, FuelType, "?" FROM temp_table_3;
DROP TABLE temp_table_3;

PRAGMA foreign_keys = 1;
