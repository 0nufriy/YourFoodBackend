using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using YourFoodBackend.Model;

namespace YourFoodBackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private IConfiguration Configuration;
        public UserController(IConfiguration _configuration)
        {
            Configuration = _configuration;
        }
        [HttpPost]
        [Route("register")]
        public ActionResult RegisterUser(UserRegister p)
        {
            string query = "INSERT INTO [User] ([Login],[Password],[Email],[Phone],[Role], [Name])VALUES (@login, @password,@email,@phone,@role,@name);";
            string connectionString = Configuration.GetConnectionString("YourFood");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand sqlquery = new SqlCommand(query, connection);
                

                sqlquery.Parameters.Add("@login", System.Data.SqlDbType.NChar);
                sqlquery.Parameters["@login"].Value = p.login;

                sqlquery.Parameters.Add("@password", System.Data.SqlDbType.NChar);
                sqlquery.Parameters["@password"].Value = p.password;

                sqlquery.Parameters.Add("@email", System.Data.SqlDbType.NChar);
                sqlquery.Parameters["@email"].Value = p.email;

                sqlquery.Parameters.Add("@phone", System.Data.SqlDbType.NChar);
                sqlquery.Parameters["@phone"].Value = p.phone;

                sqlquery.Parameters.Add("@role", System.Data.SqlDbType.NChar);
                sqlquery.Parameters["@role"].Value = p.role;

                sqlquery.Parameters.Add("@name", System.Data.SqlDbType.NChar);
                sqlquery.Parameters["@name"].Value = p.name;

                try
                {
                    sqlquery.ExecuteNonQuery();
                }
                catch
                {
                    return new StatusCodeResult(204);
                }
            }

            return new StatusCodeResult(200);
        }

        [HttpPost]
        [Route("auth")]
        public ActionResult UserAuth([FromBody] UserAuth p)
        {
            string query = "";
            if (p.admin)
            {
                query = "SELECT Login from [User] where Login = @login AND Password = @password AND Role != 'User'";
            }
            else
            {
                query = "SELECT Login from [User] where Login = @login AND Password = @password AND Role = 'User';";
            }
            string connectionString = Configuration.GetConnectionString("YourFood");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand sqlquery = new SqlCommand(query, connection);


                sqlquery.Parameters.Add("@login", System.Data.SqlDbType.NChar);
                sqlquery.Parameters["@login"].Value = p.login;

                sqlquery.Parameters.Add("@password", System.Data.SqlDbType.NChar);
                sqlquery.Parameters["@password"].Value = p.password;

                var result = sqlquery.ExecuteReader();
                result.Read();

                if (result.HasRows)
                {
                    return new StatusCodeResult(200);
                }
                else
                {
                    return new StatusCodeResult(204);
                }


            }
        }

        

        [HttpPatch]
        [Route("update-fridge")]
        public ActionResult UserFridgeUpdate(UserFridgeUpdate p)
        {
            string query1 = "Update [User] Set FridgeID = @fridgeID Where Login = @login;";
            string query2 = "SELECT [CellCount], COUNT(login) FROM [Fridge] LEFT JOIN [User] ON [User].FridgeId = Fridge.FridgeID Where [User].FridgeID = @fridgeID group by Fridge.FridgeID, CellCount";
            string connectionString = Configuration.GetConnectionString("YourFood");

            using(SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlCommand sqlquery2 = new SqlCommand(query2, connection);

                sqlquery2.Parameters.Add("@fridgeID", System.Data.SqlDbType.NChar);
                sqlquery2.Parameters["@fridgeID"].Value = p.fridgeID;

                var result = sqlquery2.ExecuteReader();

                if (result.Read())
                {
                    if (Convert.ToInt32(result.GetValue(0)) <= Convert.ToInt32(result.GetValue(1)))
                    {
                        return new StatusCodeResult(210);
                    }
                }
               
                result.Close();
                SqlCommand sqlquery1 = new SqlCommand(query1, connection);


                sqlquery1.Parameters.Add("@login", System.Data.SqlDbType.NChar);
                sqlquery1.Parameters["@login"].Value = p.login;

                sqlquery1.Parameters.Add("@fridgeID", System.Data.SqlDbType.Int);
                sqlquery1.Parameters["@fridgeID"].Value = p.fridgeID;


                try
                {
                    sqlquery1.ExecuteNonQuery();
                }
                catch
                {
                    return new StatusCodeResult(204);
                }
            }

            return new StatusCodeResult(200);
        }

        [HttpPatch]
        [Route("update-pass")]
        public ActionResult UserPassUpdate(UserPassUpdate p)
        {
            string query = "Update [User] Set PassID = @passId Where Login = @login;";
            string connectionString = Configuration.GetConnectionString("YourFood");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand sqlquery = new SqlCommand(query, connection);


                sqlquery.Parameters.Add("@login", System.Data.SqlDbType.NChar);
                sqlquery.Parameters["@login"].Value = p.login;

                sqlquery.Parameters.Add("@passId", System.Data.SqlDbType.Int);
                sqlquery.Parameters["@passId"].Value = p.passId;


                try
                {
                    sqlquery.ExecuteNonQuery();
                }
                catch
                {
                    return new StatusCodeResult(204);
                }
            }

            return new StatusCodeResult(200);
        }
       
        [HttpGet]
        [Route("user-stat")]
        public List<UserStat> UserStat()
        {
            List<UserStat> result = new List<UserStat>();

            string query = "Select [Login],[Name],Fridge.[Adress] ,[PassID],[Email],[Phone],Fridge.[FridgeID], Role FROM [User] LEFT JOIN Fridge ON Fridge.FridgeID=[User].FridgeID";
            string connectionString = Configuration.GetConnectionString("YourFood");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand sqlquery = new SqlCommand(query, connection);


                var table = sqlquery.ExecuteReader();

                while (table.Read())
                {
                    UserStat stat = new UserStat();
                    stat.login = table.GetValue(0).ToString();
                    stat.name = table.GetValue(1).ToString();
                    stat.adress = table.GetValue(2).ToString();
                    try
                    {
                        stat.passId = Convert.ToInt32(table.GetValue(3).ToString());
                    }
                    catch
                    {
                        stat.passId = 0;
                    }
                    
                   
                    stat.email = table.GetValue(4).ToString();
                    stat.phone = table.GetValue(5).ToString();
                    try
                    {
                        stat.fridgeId = Convert.ToInt32(table.GetValue(6).ToString());
                    }
                    catch
                    {
                        stat.fridgeId = 0;
                    }
                    
                    stat.role = table.GetValue(7).ToString();
                    result.Add(stat);
                }

            }

            return result;
        }
        [HttpGet]
        [Route("user-by-login")]
        public UserStatLogin UsernyLogin(string login )
        {
            UserStatLogin result = new UserStatLogin();

            string query = "Select [Login],[Name],Fridge.[Adress] ,[PassName],[Email],[Phone],Fridge.[FridgeID], Role, [Temperature] FROM [User] LEFT JOIN Fridge ON Fridge.FridgeID=[User].FridgeID LEFT JOIN [Pass] ON [Pass].PassID =[User].PassID WHERE [Login] = @login";
            string connectionString = Configuration.GetConnectionString("YourFood");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand sqlquery = new SqlCommand(query, connection);
                sqlquery.Parameters.Add("@login", System.Data.SqlDbType.NChar);
                sqlquery.Parameters["@login"].Value = login;

                var table = sqlquery.ExecuteReader();

                table.Read();
                result.login = table.GetValue(0).ToString();
                result.name = table.GetValue(1).ToString();
                result.adress = table.GetValue(2).ToString();
                result.passName = table.GetValue(3).ToString();


                result.email = table.GetValue(4).ToString();
                result.phone = table.GetValue(5).ToString();
                try
                {
                    result.fridgeId = Convert.ToInt32(table.GetValue(6).ToString());
                }
                catch
                {
                    result.fridgeId = 0;
                }

                

                result.role = table.GetValue(7).ToString();
                try
                {
                    result.temperature = Convert.ToDouble(table.GetValue(8).ToString());
                }
                catch
                {
                    result.temperature = -300;
                }
               
            }

            return result;
        }

    }
}
