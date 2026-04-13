using System.Data;
using Microsoft.Data.Sqlite;

public class SQLLoanInterface
{
    public static int GetSQLLoanId(string almaLoanId)
    {
        try
        {
            using (SqliteConnection connection = new SqliteConnection(SQLInterface.CONNECTION_STRING))
            {
                connection.Open();

                string query = "SELECT * FROM loan WHERE alma_id = $alma_id";
                using (SqliteCommand command = new SqliteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("$alma_id", almaLoanId);

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
            return -19;
        }
    }

    // Inserts a loan into the database.
    public static int InsertLoan(string loanId, string outCircDesk, int patronId, int itemId, string loan_date, string due_date)
    {
        if (outCircDesk == null)
        {
            outCircDesk = "None";
        }

        try
        {
            int id = GetSQLLoanId(loanId);
            if (id == 0)
            {
                using (SqliteConnection connection = new SqliteConnection(SQLInterface.CONNECTION_STRING))
                {
                    connection.Open();

                    string insert = "INSERT INTO loan (alma_id, out_circ_desk, patron_id, item_id, loan_date, due_date) VALUES ($almaId, $outCircDesk, $patronId, $itemId, $loanDate, $dueDate)";
                    using (SqliteCommand insertCommand = new SqliteCommand(insert, connection))
                    {
                        insertCommand.Parameters.AddWithValue("$almaId", loanId);
                        insertCommand.Parameters.AddWithValue("$outCircDesk", outCircDesk);
                        insertCommand.Parameters.AddWithValue("$patronId", patronId);
                        insertCommand.Parameters.AddWithValue("$itemId", itemId);
                        insertCommand.Parameters.AddWithValue("$loanDate", loan_date);
                        insertCommand.Parameters.AddWithValue("$dueDate", due_date);

                        insertCommand.ExecuteNonQuery();
                    }

                    connection.Close();
                }
            }
            else if (id < 0) // Error -19
            {
                return id;
            }
        }
        catch (Exception e)
        {
            Logger<SQLLoanInterface>.Log($"An error occurred when inserting a loan into database: {e.Message}", LogLevel.Error);
            return 20;
        }

        return 0;
    }
}