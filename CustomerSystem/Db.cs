using Microsoft.Data.Sqlite;

namespace BiQ.AlertIntegrationDemo.CustomerSystem
{
    public static class Db
    {
        public static DtoTenant.Customer? GetCustomer(string? customerNumber)
        {
            using var connection = new SqliteConnection(ConfigValues.CustomerSystemConnectionString);
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText =
            @"
                SELECT * FROM Customers
                WHERE CustomerNumber = @customernumber;
            ";
            command.Parameters.AddWithValue("@customernumber", customerNumber);
            using var reader = command.ExecuteReader();
            if (!reader.Read()) return null;

            return new DtoTenant.Customer()
            {
                CustomerCategoryText = ReadNullableString(reader, "CustomerCategoryText"),
                LatestChangeIdFromBiQ = ReadNullableString(reader, "LatestChangeIdFromBiQ"),
                Name = ReadNullableString(reader, "Name"),
                CustomerNumber = customerNumber,
                Attention = ReadNullableString(reader, "Attention"),
                CO = ReadNullableString(reader, "CO"),
                Cpr = ReadNullableString(reader, "Cpr"),
                BirthDay = ReadNullableString(reader, "BirthDay"),
                CompositeAddress = ReadNullableString(reader, "CompositeAddress"),
                Cvr = ReadNullableString(reader, "Cvr"),
                PNumber = ReadNullableString(reader, "PNumber"),
                Phone1 = ReadNullableString(reader, "Phone1"),
                Phone2 = ReadNullableString(reader, "Phone2"),
                Email = ReadNullableString(reader, "Email"),
                Street = ReadNullableString(reader, "Street"),
                StreetCode = ReadNullableString(reader, "StreetCode"),
                HouseNumber = ReadNullableString(reader, "HouseNumber"),
                HouseLetter = ReadNullableString(reader, "HouseLetter"),
                Floor = ReadNullableString(reader, "Floor"),
                Suite = ReadNullableString(reader, "Suite"),
                City = ReadNullableString(reader, "City"),
                Zip = ReadNullableString(reader, "Zip"),
                SubCity = ReadNullableString(reader, "SubCity"),
                AddressDarId = ReadNullableString(reader, "AddressDarId"),
                Country = ReadNullableString(reader, "Country"),
                CountryCode = ReadNullableString(reader, "CountryCode"),
                MunicipalityCode = ReadNullableString(reader, "MunicipalityCode"),
                SecondaryName = ReadNullableString(reader, "SecondaryName"),
                SecondaryCpr = ReadNullableString(reader, "SecondaryCpr"),
                SecondaryPhone = ReadNullableString(reader, "SecondaryPhone"),
                SecondaryPhone2 = ReadNullableString(reader, "SecondaryPhone2"),
                SecondaryEmail = ReadNullableString(reader, "SecondaryEmail"),
                PoBox = ReadNullableString(reader, "PoBox"),
                ChangedBy = ReadNullableString(reader, "ChangedBy"),
                ChangedAt = ReadNullableDateTimeOffset(reader, "ChangedAt")!.Value,
                CreatedAt = ReadNullableDateTimeOffset(reader, "CreatedAt"),
                CreatedBy = ReadNullableString(reader, "CreatedBy"),
                ExtraData1Value = ReadNullableString(reader, "CreatedAs"),
                ExtraData2Value = ReadNullableString(reader, "Role")
            };
        }

        public static void UpdateCustomer(DtoTenant.Customer customer)
        {
            using var connection = new SqliteConnection(ConfigValues.CustomerSystemConnectionString);
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText =
            @"UPDATE Customers
            SET 
                LatestChangeIdFromBiQ = @LatestChangeIdFromBiQ,
                AddressDarId = @AddressDarId,
                BirthDay = @BirthDay,
                ChangedAt = @ChangedAt,
                ChangedBy = @ChangedBy,
                City = @City,
                Attention = @Attention,
                CO = @CO,
                CompositeAddress = @CompositeAddress,
                Country = @Country,
                CountryCode = @CountryCode,  
                Cpr = @Cpr,
                CreatedAs = @CreatedAs,
                CreatedAt = @CreatedAt,
                CreatedBy = @CreatedBy,
                CustomerCategoryText = @CustomerCategoryText,
                Cvr= @Cvr,
                Email = @Email,
                Floor = @Floor,
                HouseLetter = @HouseLetter,
                HouseNumber = @HouseNumber,
                MunicipalityCode = @MunicipalityCode,
                Name = @Name,
                Phone1 = @Phone1,
                Phone2 = @Phone2,
                PNumber = @PNumber,
                PoBox = @PoBox,
                Role = @Role,
                SecondaryCpr = @SecondaryCpr,
                SecondaryEmail = @SecondaryEmail,
                SecondaryName = @SecondaryName,  
                SecondaryPhone = @SecondaryPhone,
                SecondaryPhone2 = @SecondaryPhone2,
                Street = @Street,
                StreetCode = @StreetCode,
                SubCity = @SubCity,
                Suite = @Suite,
                Zip = @Zip
            WHERE CustomerNumber = @customerNumber";
            command.Parameters.AddWithValue("@LatestChangeIdFromBiQ", HandleNull(customer.LatestChangeIdFromBiQ));
            command.Parameters.AddWithValue("@AddressDarId", HandleNull(customer.AddressDarId));
            command.Parameters.AddWithValue("@BirthDay", HandleNull(customer.BirthDay));
            command.Parameters.AddWithValue("@ChangedAt", DateTimeOffset.Now);
            command.Parameters.AddWithValue("@ChangedBy", HandleNull(customer.ChangedBy));
            command.Parameters.AddWithValue("@City", HandleNull(customer.City));
            command.Parameters.AddWithValue("@Attention", HandleNull(customer.Attention));
            command.Parameters.AddWithValue("@CO", HandleNull(customer.CO));
            command.Parameters.AddWithValue("@CompositeAddress", HandleNull(customer.CompositeAddress));
            command.Parameters.AddWithValue("@Country", HandleNull(customer.Country));
            command.Parameters.AddWithValue("@CountryCode", HandleNull(customer.CountryCode));
            command.Parameters.AddWithValue("@Cpr", HandleNull(customer.Cpr));
            command.Parameters.AddWithValue("@CreatedAs", HandleNull(customer.ExtraData1Value));
            command.Parameters.AddWithValue("@CreatedAt", HandleNull(customer.CreatedAt));
            command.Parameters.AddWithValue("@CreatedBy", HandleNull(customer.CreatedBy));
            command.Parameters.AddWithValue("@CustomerCategoryText", HandleNull(customer.CustomerCategoryText));
            command.Parameters.AddWithValue("@Cvr", HandleNull(customer.Cvr));
            command.Parameters.AddWithValue("@Email", HandleNull(customer.Email));
            command.Parameters.AddWithValue("@Floor", HandleNull(customer.Floor));
            command.Parameters.AddWithValue("@HouseLetter", HandleNull(customer.HouseLetter));
            command.Parameters.AddWithValue("@HouseNumber", HandleNull(customer.HouseNumber));
            command.Parameters.AddWithValue("@MunicipalityCode", HandleNull(customer.MunicipalityCode));
            command.Parameters.AddWithValue("@Name", HandleNull(customer.Name));
            command.Parameters.AddWithValue("@Phone1", HandleNull(customer.Phone1));
            command.Parameters.AddWithValue("@Phone2", HandleNull(customer.Phone2));
            command.Parameters.AddWithValue("@PNumber", HandleNull(customer.PNumber));
            command.Parameters.AddWithValue("@PoBox", HandleNull(customer.PoBox));
            command.Parameters.AddWithValue("@Role", HandleNull(customer.ExtraData2Value));
            command.Parameters.AddWithValue("@SecondaryCpr", HandleNull(customer.SecondaryCpr));
            command.Parameters.AddWithValue("@SecondaryEmail", HandleNull(customer.SecondaryEmail));
            command.Parameters.AddWithValue("@SecondaryName", HandleNull(customer.SecondaryName));
            command.Parameters.AddWithValue("@SecondaryPhone", HandleNull(customer.SecondaryPhone));
            command.Parameters.AddWithValue("@SecondaryPhone2", HandleNull(customer.SecondaryPhone2));
            command.Parameters.AddWithValue("@Street", HandleNull(customer.Street));
            command.Parameters.AddWithValue("@StreetCode", HandleNull(customer.StreetCode));
            command.Parameters.AddWithValue("@SubCity", HandleNull(customer.SubCity));
            command.Parameters.AddWithValue("@Suite", HandleNull(customer.Suite));
            command.Parameters.AddWithValue("@Zip", HandleNull(customer.Zip));
            command.Parameters.AddWithValue("@customerNumber", customer.CustomerNumber);
            command.ExecuteNonQuery();
        }

        public static void InsertCustomer(DtoTenant.Customer customer)
        {
            using var connection = new SqliteConnection(ConfigValues.CustomerSystemConnectionString);
            connection.Open();
            var command = connection.CreateCommand();
            var now = DateTimeOffset.Now;
            command.CommandText =
            @"INSERT INTO Customers
            (
                CustomerNumber,
                CustomerCategoryText,
                LatestChangeIdFromBiQ,
                Name,
                Attention,
                CO,
                Cpr,
                BirthDay,
                CompositeAddress ,
                Cvr,

                PNumber,
                Phone1,
                Phone2,
                Email,
                Street,
                StreetCode,
                HouseNumber,
                HouseLetter,
                Floor,
                Suite,

                City,
                Zip,
                SubCity,
                AddressDarId,
                Country,
                CountryCode,  
                MunicipalityCode,
                SecondaryName,  
                SecondaryCpr,
                SecondaryPhone,

                SecondaryPhone2,
                SecondaryEmail,
                PoBox,
                ChangedBy,
                ChangedAt,
                CreatedAt,
                CreatedBy,
                CreatedAs,
                Role
            )
            VALUES
            (
                @customerNumber,
                @CustomerCategoryText,
                @LatestChangeIdFromBiQ,
                @Name,
                @Attention,
                @CO,
                @Cpr,
                @BirthDay,
                @CompositeAddress,
                @Cvr,

                @PNumber,
                @Phone1,
                @Phone2,
                @Email,
                @Street,
                @StreetCode,
                @HouseNumber,
                @HouseLetter,
                @Floor,
                @Suite,

                @City,
                @Zip,
                @SubCity,
                @AddressDarId,
                @Country,
                @CountryCode,  
                @MunicipalityCode,
                @SecondaryName,  
                @SecondaryCpr,
                @SecondaryPhone,

                @SecondaryPhone2,
                @SecondaryEmail,
                @PoBox,
                @ChangedBy,
                @ChangedAt,
                @CreatedAt,
                @CreatedBy,
                @CreatedAs,
                @Role                
            )";
            command.Parameters.AddWithValue("@customerNumber", customer.CustomerNumber);
            command.Parameters.AddWithValue("@CustomerCategoryText", HandleNull(customer.CustomerCategoryText));
            command.Parameters.AddWithValue("@LatestChangeIdFromBiQ", HandleNull(customer.LatestChangeIdFromBiQ));            
            command.Parameters.AddWithValue("@Name", HandleNull(customer.Name));
            command.Parameters.AddWithValue("@Attention", HandleNull(customer.Attention));
            command.Parameters.AddWithValue("@CO", HandleNull(customer.CO));
            command.Parameters.AddWithValue("@Cpr", HandleNull(customer.Cpr));
            command.Parameters.AddWithValue("@BirthDay", HandleNull(customer.BirthDay));
            command.Parameters.AddWithValue("@CompositeAddress", HandleNull(customer.CompositeAddress));
            command.Parameters.AddWithValue("@Cvr", HandleNull(customer.Cvr));
            command.Parameters.AddWithValue("@PNumber", HandleNull(customer.PNumber));

            command.Parameters.AddWithValue("@Phone1", HandleNull(customer.Phone1));
            command.Parameters.AddWithValue("@Phone2", HandleNull(customer.Phone2));
            command.Parameters.AddWithValue("@Email", HandleNull(customer.Email));
            command.Parameters.AddWithValue("@Street", HandleNull(customer.Street));
            command.Parameters.AddWithValue("@StreetCode", HandleNull(customer.StreetCode));
            command.Parameters.AddWithValue("@HouseNumber", HandleNull(customer.HouseNumber));
            command.Parameters.AddWithValue("@HouseLetter", HandleNull(customer.HouseLetter));
            command.Parameters.AddWithValue("@Floor", HandleNull(customer.Floor));
            command.Parameters.AddWithValue("@Suite", HandleNull(customer.Suite));

            command.Parameters.AddWithValue("@City", HandleNull(customer.City));
            command.Parameters.AddWithValue("@Zip", HandleNull(customer.Zip));
            command.Parameters.AddWithValue("@SubCity", HandleNull(customer.SubCity));
            command.Parameters.AddWithValue("@AddressDarId", HandleNull(customer.AddressDarId));
            command.Parameters.AddWithValue("@Country", HandleNull(customer.Country));
            command.Parameters.AddWithValue("@CountryCode", HandleNull(customer.CountryCode));
            command.Parameters.AddWithValue("@MunicipalityCode", HandleNull(customer.MunicipalityCode));
            command.Parameters.AddWithValue("@SecondaryName", HandleNull(customer.SecondaryName));
            command.Parameters.AddWithValue("@SecondaryCpr", HandleNull(customer.SecondaryCpr));
            command.Parameters.AddWithValue("@SecondaryPhone", HandleNull(customer.SecondaryPhone));

            command.Parameters.AddWithValue("@SecondaryPhone2", HandleNull(customer.SecondaryPhone2));
            command.Parameters.AddWithValue("@SecondaryEmail", HandleNull(customer.SecondaryEmail));
            command.Parameters.AddWithValue("@PoBox", HandleNull(customer.PoBox));
            command.Parameters.AddWithValue("@ChangedBy", HandleNull(customer.ChangedBy));
            command.Parameters.AddWithValue("@ChangedAt", now);
            command.Parameters.AddWithValue("@CreatedAt", now);
            command.Parameters.AddWithValue("@CreatedBy", HandleNull(customer.CreatedBy));
            command.Parameters.AddWithValue("@CreatedAs", HandleNull(customer.ExtraData1Value));
            command.Parameters.AddWithValue("@Role", HandleNull(customer.ExtraData2Value));
            command.ExecuteNonQuery();
        }

        public static List<string> GetCustomerNumbersFrom(DateTimeOffset lastTime)
        {
            using var connection = new SqliteConnection(ConfigValues.CustomerSystemConnectionString);
            var result = new List<string>();
            int count = 0;
            int maxCount = 10;
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText =
            @"SELECT CustomerNumber FROM Customers
            WHERE ChangedAt > @lasttime
            ORDER BY ChangedAt";
            command.Parameters.AddWithValue("@lasttime", lastTime);
            using var reader = command.ExecuteReader();
            while (true)
            {
                count++;
                if (!reader.Read())
                    return result;
                if (maxCount < count)
                    return result;

                var CustomerNumber = reader.GetString(reader.GetOrdinal("CustomerNumber"));
                result.Add(CustomerNumber);
            }
        }

        private static object HandleNull(object? value)
        {
            if (value is null)
                return DBNull.Value;
            else
                return value;
        }

        private static string? ReadNullableString(SqliteDataReader reader, string column)
        {
            int ordinal = reader.GetOrdinal(column);
            if (reader.IsDBNull(ordinal))
                return null;
            else
                return reader.GetString(reader.GetOrdinal(column));
        }

        private static DateTimeOffset? ReadNullableDateTimeOffset(SqliteDataReader reader, string column)
        {
            int ordinal = reader.GetOrdinal(column);
            if (reader.IsDBNull(ordinal))
                return null;
            else
                return reader.GetDateTimeOffset(reader.GetOrdinal(column));
        }
    }
}
