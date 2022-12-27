using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using YourFoodBackend.Model;

namespace YourFoodBackend.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PassController : ControllerBase
    {
        public IConfiguration Configuration { get; }
        public PassController(IConfiguration _configuration)
        {
            Configuration = _configuration;
        }
        [HttpGet]
        [Route("pass-info")]
        public PassInfo PassInfo(int passid)
        {
            PassInfo result = new PassInfo();

            result.PassId = passid;
            result.foodInfo = new List<FoodInfo>();

            string query = "SELECT Pass.Price,Pass.PassName, Pass.description, Food.FoodID,Food.Name, Food.Description, Food.image, Pass.image " +
                                "FROM[Pass],[PassFood],[Food]" +
                                     "WHERE Pass.PassID = PassFood.PassID AND Food.FoodID = PassFood.FoodID AND Pass.PassID = @passid";
            string connectionString = Configuration.GetConnectionString("YourFood");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand sqlquery = new SqlCommand(query, connection);

                sqlquery.Parameters.Add("@passid", System.Data.SqlDbType.NChar);
                sqlquery.Parameters["@passid"].Value = passid;

                var table = sqlquery.ExecuteReader();
                table.Read();

                result.Price = Convert.ToInt32(table.GetValue(0).ToString());
                result.passName = table.GetValue(1).ToString();
                result.description = table.GetValue(2).ToString();
                result.image = table.GetValue(7).ToString();
                FoodInfo stat = new FoodInfo();
                stat.foodid = Convert.ToInt32(table.GetValue(3).ToString());
                stat.name = table.GetValue(4).ToString();
                stat.description = table.GetValue(5).ToString();
                stat.image = table.GetValue(6).ToString();

                result.foodInfo.Add(stat);

                while (table.Read())
                {
                    FoodInfo stat1 = new FoodInfo();
                    stat1.foodid = Convert.ToInt32(table.GetValue(3).ToString());
                    stat1.name = table.GetValue(4).ToString();
                    stat1.description = table.GetValue(5).ToString();
                    stat1.image = table.GetValue(6).ToString();

                    result.foodInfo.Add(stat1);
                }

            }

            return result;
        }

        [HttpGet]
        [Route("all-pass")]
        public List<Pass> GetPasses()
        {
            List<Pass> result = new List<Pass>();

            string query = "Select PassID, Price, PassName, image, description from Pass";
            string connectionString = Configuration.GetConnectionString("YourFood");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand sqlquery = new SqlCommand(query, connection);

                var table = sqlquery.ExecuteReader();
                

                while (table.Read())
                {
                    Pass stat1 = new Pass();
                    stat1.passid = Convert.ToInt32(table.GetValue(0).ToString());
                    stat1.price = Convert.ToInt32(table.GetValue(1).ToString());
                    stat1.passname = table.GetValue(2).ToString();
                    stat1.image = table.GetValue(3).ToString();
                    stat1.description = table.GetValue(4).ToString();

                    result.Add(stat1);
                }

            }

            return result;
        }

        [HttpGet]
        [Route("pass-id-by-login")]
        public int GetPassIdByLogin(string login)
        {
            string query = "SELECT PassId From [User] Where Login = @login";
            string connectionString = Configuration.GetConnectionString("YourFood");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand sqlquery = new SqlCommand(query, connection);

                sqlquery.Parameters.Add("@login", System.Data.SqlDbType.NChar);
                sqlquery.Parameters["@login"].Value = login;

                var table = sqlquery.ExecuteReader();
                table.Read();
                if (table.HasRows)
                {
                    try
                    {
                        return Convert.ToInt32(table.GetValue(0).ToString());
                    }
                    catch
                    {
                        return 0;
                    }
                   
                }
                else
                {
                    return 0;
                }

            }
           
        }
        
    }
}
