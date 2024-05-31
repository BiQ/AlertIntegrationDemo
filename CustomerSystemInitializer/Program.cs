using Microsoft.Data.Sqlite;

namespace BiQ.AlertIntegrationDemo.CustomerSystemInitializer
{
    internal class Program
    {
        static void Main()
        {
            Console.WriteLine("Initializing...");
            Console.WriteLine("Db file at:"+ ConfigValues.CustomerSystemConnectionString);
            using (var connection = new SqliteConnection(ConfigValues.CustomerSystemConnectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText =
                @"
CREATE TABLE Customers (
CustomerNumber TEXT NOT NULL PRIMARY KEY,
ChangedBy TEXT NULL,
ChangedAt DateTimeOffset NULL,
CreatedBy TEXT NULL,
CreatedAt DateTimeOffset NULL,
CustomerCategoryText TEXT NULL,
LatestChangeIdFromBiQ TEXT NULL,
Name TEXT NULL,
Active INTEGER NULL,
Cpr TEXT NULL,
Cvr TEXT NULL,
PNumber TEXT NULL,
Phone1 TEXT NULL,
Phone2 TEXT NULL,
Email TEXT NULL,
BirthDay TEXT NULL,
CO TEXT NULL,
Attention TEXT NULL,
PoBox TEXT NULL,
CompositeAddress TEXT NULL,
Street TEXT NULL,
StreetCode TEXT NULL,
HouseNumber TEXT NULL,
HouseLetter TEXT NULL,
Floor TEXT NULL,
Suite TEXT NULL,
City TEXT NULL,
Zip TEXT NULL,
SubCity TEXT NULL,
AddressDarId TEXT NULL,
Country TEXT NULL,
CountryCode TEXT NULL,
MunicipalityCode TEXT NULL,
SecondaryName TEXT NULL,
SecondaryCpr TEXT NULL,
SecondaryPhone TEXT NULL,
SecondaryPhone2 TEXT NULL,
SecondaryEmail TEXT NULL,
CreatedAs TEXT NULL,
Role TEXT NULL
);

INSERT INTO Customers 
(CustomerNumber, Name,            Cvr,       Street, HouseNumber,    Zip,  City,    Active, CustomerCategoryText, Cpr, ChangedAt)
VALUES 
('#001', 'The Pizza Company ApS',  '40563350','Sværtegade','11',     '1118','København K',1,'erhverv', null,        '2024-01-02'),
('2',    'Pico Pizza Nørrebro ApS','39972190','Skyttegade','3',      '2200','København N',1,'erhverv', null,        '2024-01-03'),
('00003','Hejls Pizza',            '40639144','Vargårdevej','1',     '6094','Hejls',      1,'erhverv', null,        '2024-01-04'),
('00004','Anders A.',              null,      'Paradisæblevej','13', null,  'Andeby',     0,'privat',  null,        '2024-01-05'),
('00005','Kim Larsen',             null,      'Claus Bergs Gade','5','5000','Odense C',   0,'privat', '231045-0637','2024-01-06');
";
                command.ExecuteNonQuery();
            }
            Console.WriteLine("Done! No need for running this again.");
        }
    }
}
