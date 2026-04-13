using System.Data;
using System.Numerics;
using Microsoft.Data.Sqlite;

public class SQLItemInterface // This is extremely similar to patron interface. Maybe want to merge classes.
{
    // Get id of item in database using barcode.
    public static int GetItemId(string barcode)
    {
        try
        {
            using (SqliteConnection connection = new SqliteConnection(SQLInterface.CONNECTION_STRING))
            {
                connection.Open();

                string query = "SELECT * FROM item WHERE barcode = $barcode";
                using (SqliteCommand command = new SqliteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("$barcode", barcode);

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
            Logger<SQLItemInterface>.Log($"Failed to get item id from barcode: {e.Message}", LogLevel.Error);
            return -17;
        }
    }

    public static int InsertItem(string mms_id, string barcode, string title, string? description, string policy)
    {
        if (description == null)
        {
            description = "None";
        }
        if (policy == null)
        {
            description = "NO POLICY FOUND";
        }


        try
        {
            int id = GetItemId(barcode);
            if (id == 0)
            {
                using (SqliteConnection connection = new SqliteConnection(SQLInterface.CONNECTION_STRING))
                {
                    connection.Open();
                    // Write insert logic here based off SQLPatronInterface.

                    string insert = "INSERT INTO item (mms_id, barcode, title, description, policy) VALUES ($mms_id, $barcode, $title, $description, $policy)";
                    using (SqliteCommand insertCommand = new SqliteCommand(insert, connection))
                    {
                        insertCommand.Parameters.AddWithValue("$mms_id", mms_id);
                        insertCommand.Parameters.AddWithValue("$barcode", barcode);
                        insertCommand.Parameters.AddWithValue("$title", title);
                        insertCommand.Parameters.AddWithValue("$description", description);
                        insertCommand.Parameters.AddWithValue("$policy", policy);

                        insertCommand.ExecuteNonQuery();
                    }

                    connection.Close();
                }
            }
            else if (id < 0) // Error -16.
            {
                return id;
            }
        }
        catch (Exception e)
        {
            Logger<SQLItemInterface>.Log($"An error occurred when inserting an item into database: {e.Message}", LogLevel.Error);
            return 16;
        }
        return 0;
    }
}