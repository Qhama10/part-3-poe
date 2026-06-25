using MySql.Data.MySqlClient;

public class DatabaseHelper
{
    // Keep connection string readonly so it can't be changed accidentally
    private readonly string connectionString =
        "server=localhost;port=3306;user=root;password=20040110Zh;database=cybersecurity_bot;";

    // Returns a NEW connection each time (correct practice)
    public MySqlConnection GetConnection()
    {
        return new MySqlConnection(connectionString);
    }

    // Optional helper: test connection quickly
    public bool TestConnection()
    {
        try
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                return true;
            }
        }
        catch
        {
            return false;
        }
    }
}