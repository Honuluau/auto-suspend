using System.Data;
using System.Runtime.CompilerServices;
using Microsoft.Data.Sqlite;

public class SQLPatronInterface
{
    // Get id of patron through eagle id
    public static int GetPatronId(string eagleId)
    {
        try
        {
            using (SqliteConnection connection = new SqliteConnection(SQLInterface.CONNECTION_STRING))
            {
                connection.Open();

                string query = "SELECT * FROM patron WHERE eagle_id = $eagle_id";
                using (SqliteCommand command = new SqliteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("$eagle_id", eagleId);

                    SqliteDataReader reader = command.ExecuteReader();
                    DataTable table = new DataTable();
                    table.Load(reader);

                    connection.Close();

                    if (table.Rows.Count > 0)
                    {
                        return Convert.ToInt32(table.Rows[0][0]);
                    }
                    else
                    {
                        return 0;
                    }
                }

            }
        }
        catch (Exception e)
        {
            Logger<SQLInterface>.Log($"Failed to get patron id from eagle id: {e.Message}", LogLevel.Error);
            return -15; // Negative error code because the id cannot be negative.
        }
    }

    // Insert patron information into database (does not insert duplicate information).
    public static int InsertPatron(string eagleId, string firstName, string lastName, string userGroup)
    {
        try
        {
            // Check if patron already exists in database.
            int id = GetPatronId(eagleId);
            if (id == 0)
            {
                // Patron does not exist therefore we can insert.
                using (SqliteConnection connection = new SqliteConnection(SQLInterface.CONNECTION_STRING))
                {
                    connection.Open();

                    string insert = "INSERT INTO patron (eagle_id, first_name, last_name, user_group) VALUES ($eagleId, $firstName, $lastName, $userGroup)";
                    using (SqliteCommand insertCommand = new SqliteCommand(insert, connection))
                    {
                        insertCommand.Parameters.AddWithValue("$eagleId", eagleId);
                        insertCommand.Parameters.AddWithValue("$firstName", firstName);
                        insertCommand.Parameters.AddWithValue("$lastName", lastName);
                        insertCommand.Parameters.AddWithValue("$userGroup", userGroup);

                        insertCommand.ExecuteNonQuery();
                    }

                    connection.Close();
                }
            }
            else if (id < 0) // Error -15.
            {
                return id;
            }
        }
        catch (Exception e)
        {
            Logger<SQLInterface>.Log($"Failed to write patron information to database: {e.Message}", LogLevel.Error);
            return 14;
        }

        return 0;
    }
}