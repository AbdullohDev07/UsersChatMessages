using Npgsql;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
class Program
{
    static string connectionString = "Host=localhost;Port=16172;Database=console_task;User Id=postgres;Password=axihub;";
    static void Main()
    {
        int num;
        string username, password, b_user;
        bool check;
        while (true)
        {
            Console.Write("\n1. Kirish\r\n2. Chiqish\r\nEnter number: ");
            num = int.Parse(Console.ReadLine());
            Console.Clear();
            if (num == 1)
            {
                Console.Write("\nUsername: ");
                username = Console.ReadLine();
                Console.Write("Password: ");
                password = Console.ReadLine();
                if (true == inUser(username, password))
                {
                    if (true == checkUsers(username))
                    {
                        check = true;
                        while (true)
                        {
                            if (check == true)
                            {
                                Console.Write("0. Chiqish\nEnter number: ");
                                check = false;
                            }
                            else
                            {
                                checkUsers(username);
                                Console.Write("0. Chiqish\nEnter number: ");
                            }
                            int n = int.Parse(Console.ReadLine());
                            Console.Clear();
                            if (n != 0)
                            {
                                b_user = choseUser(username, n);
                                if (b_user != null)
                                {
                                    while (true)
                                    {
                                        if (false == chats(username, b_user))
                                        {
                                            Console.WriteLine("Xabarlar mavjud emas !!!");
                                        }
                                        Console.Write("\n1. Xabar yuborish\n2. Chiqish\nEnter number: ");
                                        if (1 == int.Parse(Console.ReadLine()))
                                        {
                                            Console.Clear();
                                            Console.Write("\nText: "); 
                                            insertInMessaeges(username, b_user, Console.ReadLine());
                                            Console.Clear();
                                        }
                                        else
                                        {
                                            Console.Clear();
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Bunday son ostidagi user yo'q");
                                }
                            }
                            else
                            {
                                break;
                            }

                        }
                    }
                    else
                    {
                        Console.WriteLine("\nSizdan boshqa User lar mavjud emas");
                    }
                }
            }
            else if (num == 2)
            {
                break;
            }
        }
    }

    

    public static bool inUser(string username, string password)
    {
        const int keySize = 64;
        const int iterations = 350000;
        HashAlgorithmName hashAlgorithm = HashAlgorithmName.SHA512;
        using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
        {
            connection.Open();

            string query = $"select username, password,salt from users;";
            using NpgsqlCommand cmd = new NpgsqlCommand(query, connection);

            var result = cmd.ExecuteReader();
            while (result.Read())
            {
                if (result[0].ToString() == username)
                {
                    if (DeHashPassword(password, result[1].ToString(), result[2].ToString(),keySize,iterations,hashAlgorithm))
                    {
                        Console.WriteLine("\nIn to the account");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("\nPassword xato !"); 
                        return false;
                    }
                }
            }

            insertInUsersTable(username, password);
            return true;
        }
    }
    public static void insertInUsersTable(string username, string password)
    {
        using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
        {
            connection.Open();
            password = HashPasword(password, out byte[]? solt);
            string query = $"insert into users(username, password, salt) values ('{username}', '{password}', '{Convert.ToHexString(solt)}');";
            using NpgsqlCommand cmd = new NpgsqlCommand(query, connection);

            cmd.ExecuteNonQuery();
            Console.WriteLine("\nAdd New User");
        }
    }


    public static string HashPasword(string password, out byte[] salt)
    {
        const int keySize = 64;
        const int iterations = 350000;
        HashAlgorithmName hashAlgorithm = HashAlgorithmName.SHA512;

        salt = RandomNumberGenerator.GetBytes(keySize);

        var hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            iterations,
            hashAlgorithm,
            keySize);

        return Convert.ToHexString(hash);
    }
    private static bool DeHashPassword(
    string passwordFromUser,
    string hashFromPg,
    string saltAsStringFromPg,
    int keySizeFromProgram,
    int iterationsFromProgram,
    HashAlgorithmName hashAlgorithmFromProgram)
    {
        byte[] salt = Convert.FromHexString(saltAsStringFromPg);

        var hashToCompare = Rfc2898DeriveBytes.Pbkdf2(
            password: passwordFromUser,
            salt,
            iterations: iterationsFromProgram,
            hashAlgorithm: hashAlgorithmFromProgram,
            outputLength: keySizeFromProgram);

        return CryptographicOperations.FixedTimeEquals(hashToCompare, Convert.FromHexString(hashFromPg));
    }



    public static bool checkUsers(string username)
    {
        using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
        {
            connection.Open();

            string query = $"select username from users where username != '{username}';";
            using NpgsqlCommand cmd = new NpgsqlCommand(query, connection);

            var result = cmd.ExecuteReader();
            int num = 1, x = 0;
            Console.WriteLine();
            while (result.Read())
            {
                Console.WriteLine($"{num}. {result[0]}");
                num++;
                x++;
            }
            if (x > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public static string choseUser(string username, int n)
    {
        using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
        {
            connection.Open();

            string query = $"select username from users where username != '{username}';";
            using NpgsqlCommand cmd = new NpgsqlCommand(query, connection);

            var result = cmd.ExecuteReader();
            int num = 1, x = 0;
            while (result.Read())
            {
                if (num == n)
                {
                    return result[0].ToString();
                }
                num++;
            }
            return null;

        }
    }

    public static bool chats(string username, string b_user)
    {
        using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
        {
            connection.Open();

            string query = $"select * from messaeges where (a_user = '{username}' and b_user = '{b_user}') or (a_user = '{b_user}' and b_user = '{username}');";
            using NpgsqlCommand cmd = new NpgsqlCommand(query, connection);
            Console.WriteLine();
            var result = cmd.ExecuteReader();
            int x = 0;
            while (result.Read())
            {
                Console.WriteLine($"{result[1]} -> {result[2]}");
                x++;
            }
            if (x > 0)
            {
                return true;
            }
            return false;
        }
    }

    public static void insertInMessaeges(string username, string b_user, string message)
    {
        using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
        {
            connection.Open();

            string query = $"insert into messaeges(a_user, text_message, b_user) values ('{username}', '{message}', '{b_user}');";
            using NpgsqlCommand cmd = new NpgsqlCommand(query, connection);

            cmd.ExecuteNonQuery();
            Console.WriteLine("Send message");
        }
    }
}