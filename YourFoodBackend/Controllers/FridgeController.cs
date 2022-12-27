using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using YourFoodBackend.Model;

namespace YourFoodBackend.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class FridgeController : ControllerBase
    {

        private IConfiguration Configuration;
        public FridgeController(IConfiguration _configuration)
        {
            Configuration = _configuration;
        }

        [HttpGet]
        [Route("fridge-stat-by-adress")]
        public List<FridgeStat> FridgeStatByAdress(string adress)
        {
            List<FridgeStat> result = new List<FridgeStat>();
            string query = "SELECT Fridge.[FridgeID],[CellCount],[Adress], COUNT([User].login) FROM [Fridge] LEFT JOIN [User] ON [User].FridgeId = Fridge.FridgeID WHERE [Adress] LIKE '%" + adress.ToString() + "%' group by Fridge.[FridgeID],[CellCount],[Adress];";
            string connectionString = Configuration.GetConnectionString("YourFood");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand sqlquery = new SqlCommand(query, connection);


                var table = sqlquery.ExecuteReader();

                while (table.Read())
                {
                    FridgeStat stat = new FridgeStat();

                    stat.fridgeId = Convert.ToInt32(table.GetValue(0).ToString());
                    stat.cellCount = Convert.ToInt32(table.GetValue(1).ToString());
                    stat.adress = table.GetValue(2).ToString();
                    stat.userCount = Convert.ToInt32(table.GetValue(3).ToString());

                    result.Add(stat);
                }

            }
            return result;
        }

        [HttpGet]
        [Route("fridge-stat")]
        public List<FridgeStat> FridgeStat()
        {
            List<FridgeStat> result = new List<FridgeStat>();
            string query = "SELECT Fridge.[FridgeID],[CellCount],[Adress], COUNT([User].login) FROM [Fridge] LEFT JOIN [User] ON [User].FridgeId = Fridge.FridgeID group by Fridge.[FridgeID],[CellCount],[Adress];";
            string connectionString = Configuration.GetConnectionString("YourFood");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand sqlquery = new SqlCommand(query, connection);


                var table = sqlquery.ExecuteReader();

                while (table.Read())
                {
                    FridgeStat stat = new FridgeStat();

                    stat.fridgeId = Convert.ToInt32(table.GetValue(0).ToString());
                    stat.cellCount = Convert.ToInt32(table.GetValue(1).ToString());
                    stat.adress = table.GetValue(2).ToString();
                    stat.userCount = Convert.ToInt32(table.GetValue(3).ToString());

                    result.Add(stat);
                }

            }
            return result;
        }
        [HttpPost]
        [Route("add-fridge")]
        public ActionResult FridgeNew(FridgeNew p)
        {
            string query = "INSERT INTO Fridge([CellCount],[Adress]) VALUES(@cellCount, @adress)";
            string connectionString = Configuration.GetConnectionString("YourFood");
            string querygetid = "SELECT [FridgeID] FROM [YourFood].[dbo].[Fridge] where [Adress] = @adress";
            string queryCell = "INSERT INTO Cells([FridgeID]) VALUES(@FridgeID)";


            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlCommand sqlquery0 = new SqlCommand(querygetid, connection);
                sqlquery0.Parameters.Add("@adress", System.Data.SqlDbType.NChar);
                sqlquery0.Parameters["@adress"].Value = p.adress;
                bool NewFridge = true;
                var adress = sqlquery0.ExecuteReader();
                adress.Read();

                int id = 0;
                if (adress.HasRows)
                {
                    NewFridge = false;
                    id = Convert.ToInt32(adress.GetValue(0).ToString());
                }
                adress.Close();



                if (NewFridge)
                {
                    SqlCommand sqlquery = new SqlCommand(query, connection);

                    sqlquery.Parameters.Add("@cellCount", System.Data.SqlDbType.Int);
                    sqlquery.Parameters["@cellCount"].Value = p.cellCount;

                    sqlquery.Parameters.Add("@adress", System.Data.SqlDbType.NChar);
                    sqlquery.Parameters["@adress"].Value = p.adress;


                    try
                    {
                        sqlquery.ExecuteNonQuery();
                    }

                    catch
                    {
                        return new StatusCodeResult(204);
                    }

                    SqlCommand sqlquery2 = new SqlCommand(querygetid, connection);
                    sqlquery2.Parameters.Add("@adress", System.Data.SqlDbType.NChar);
                    sqlquery2.Parameters["@adress"].Value = p.adress;
                    var r = sqlquery2.ExecuteReader();
                    r.Read();

                    id = Convert.ToInt32(r.GetValue(0).ToString());

                    r.Close();
                }
                else
                {
                    string updateCell = "UPDATE Fridge SET CellCount = CellCount + @cell WHERE FridgeID = @id;";
                    SqlCommand update = new SqlCommand(updateCell, connection);

                    update.Parameters.Add("@id", System.Data.SqlDbType.Int);
                    update.Parameters["@id"].Value = id;

                    update.Parameters.Add("@cell", System.Data.SqlDbType.Int);
                    update.Parameters["@cell"].Value = p.cellCount;

                    update.ExecuteNonQuery();
                }


                for (int i = 0; i < p.cellCount; i++)
                {
                    SqlCommand sqlquerycell = new SqlCommand(queryCell, connection);

                    sqlquerycell.Parameters.Add("@FridgeID", System.Data.SqlDbType.Int);
                    sqlquerycell.Parameters["@FridgeID"].Value = id;

                    sqlquerycell.ExecuteNonQuery();

                }


            }
            return new StatusCodeResult(200);
        }

        [HttpGet]
        [Route("fridge")]
        public Fridge Fridgebylogin(string login)
        {
            Fridge result = new Fridge();

            string query = "Select Fridge.FridgeId,CellCount, Adress, Temperature from Fridge , [User] WHERE [User].FridgeID = Fridge.FridgeID AND Login = @login;";
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
                    result.fridgeId = Convert.ToInt32(table.GetValue(0).ToString());
                    result.cellCount = Convert.ToInt32(table.GetValue(1).ToString());
                    result.adress = table.GetValue(2).ToString();
                    result.temperature = Convert.ToDouble(table.GetValue(3).ToString());
                }

            }

            return result;
        }



        [HttpPatch]
        [Route("update-temperature")]
        public ActionResult FridgeUpdateTemperature(FridgeTemperature p)
        {
            string query = "UPDATE Fridge SET Temperature = @temperature WHERE FridgeID = @FridgeID";
            string connectionString = Configuration.GetConnectionString("YourFood");


            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand sqlquery = new SqlCommand(query, connection);

                sqlquery.Parameters.Add("@temperature", System.Data.SqlDbType.Float);
                sqlquery.Parameters["@temperature"].Value = p.temperature;

                sqlquery.Parameters.Add("@FridgeID", System.Data.SqlDbType.Int);
                sqlquery.Parameters["@FridgeID"].Value = p.fridgeId;

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
        [Route("temperature")]
        public double? FridgeGetTemperature(int id)
        {
            List<FridgeStat> result = new List<FridgeStat>();
            string query = "SELECT [Temperature] FROM [YourFood].[dbo].[Fridge] WHERE FridgeID = @fridgeid;";
            string connectionString = Configuration.GetConnectionString("YourFood");



            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand sqlquery = new SqlCommand(query, connection);
                sqlquery.Parameters.Add("@fridgeid", System.Data.SqlDbType.Int);
                sqlquery.Parameters["@fridgeid"].Value = id;

                var table = sqlquery.ExecuteReader();

                table.Read();
                try
                {
                    return Convert.ToDouble(table.GetValue(0).ToString());
                }
                catch
                {
                    return null;
                }

            }
        }



        [HttpGet]
        [Route("cell-by-user-order")]
        public int? GetCell(string login)
        {
            string query = "SELECT Cells.CellId FROM Cells, [Order] WHERE [Order].OrderID = Cells.OrderId AND Login = @login;";
            string connectionString = Configuration.GetConnectionString("YourFood");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand sqlquery = new SqlCommand(query, connection);
                sqlquery.Parameters.Add("@login", System.Data.SqlDbType.NChar);
                sqlquery.Parameters["@login"].Value = login;

                var table = sqlquery.ExecuteReader();

                table.Read();
                try
                {
                    return Convert.ToInt32(table.GetValue(0).ToString());
                }
                catch
                {
                    return null;
                }

            }
        }

    }
}
