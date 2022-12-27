using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using YourFoodBackend.Model;

namespace YourFoodBackend.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private IConfiguration Configuration;
        public OrderController(IConfiguration _configuration)
        {
            Configuration = _configuration;
        }

        [HttpPatch]
        [Route("update-status")]
        public ActionResult OrderStatus(OrderStatus p)
        {
            string query = "Update [Order] Set Status = @status Where OrderID = @orderid;";
            string connectionString = Configuration.GetConnectionString("YourFood");
            int cellId = 0;
            string queryForCellId = "SELECT [CellId] FROM [YourFood].[dbo].[Order] WHERE OrderID = @orderid";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand sqlquery = new SqlCommand(queryForCellId, connection);

                sqlquery.Parameters.Add("@orderId", System.Data.SqlDbType.Int);
                sqlquery.Parameters["@orderId"].Value = p.orderid;

                var res = sqlquery.ExecuteReader();
                res.Read();
                try
                {
                    cellId = Convert.ToInt32(res.GetValue(0).ToString());
                }
                catch
                {
                    return new StatusCodeResult(204);
                }

                res.Close();
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand sqlquery = new SqlCommand(query, connection);


                sqlquery.Parameters.Add("@status", System.Data.SqlDbType.NChar);
                sqlquery.Parameters["@status"].Value = p.status;

                sqlquery.Parameters.Add("@orderid", System.Data.SqlDbType.Int);
                sqlquery.Parameters["@orderid"].Value = p.orderid;


                try
                {
                    sqlquery.ExecuteNonQuery();
                }
                catch
                {
                    return new StatusCodeResult(204);
                }
            }

            if(p.status == "In cell")
            {
                string query1 = "UPDATE Cells SET OrderId = @orderId WHERE CellId = @cellId";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand sqlquery = new SqlCommand(query1, connection);


                    sqlquery.Parameters.Add("@cellId", System.Data.SqlDbType.Int);
                    sqlquery.Parameters["@cellId"].Value = cellId;

                    sqlquery.Parameters.Add("@orderid", System.Data.SqlDbType.Int);
                    sqlquery.Parameters["@orderid"].Value = p.orderid;


                    try
                    {
                        sqlquery.ExecuteNonQuery();
                    }
                    catch
                    {
                        return new StatusCodeResult(204);
                    }
                }
            }
            else
            {
                string query1 = "UPDATE Cells SET OrderId = NULL WHERE CellId = @cellId";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlCommand sqlquery = new SqlCommand(query1, connection);


                    sqlquery.Parameters.Add("@cellId", System.Data.SqlDbType.Int);
                    sqlquery.Parameters["@cellId"].Value = cellId;

                    sqlquery.Parameters.Add("@orderid", System.Data.SqlDbType.Int);
                    sqlquery.Parameters["@orderid"].Value = p.orderid;


                    try
                    {
                        sqlquery.ExecuteNonQuery();
                    }
                    catch
                    {
                        return new StatusCodeResult(204);
                    }
                }
            }

            return new StatusCodeResult(200);
        }

        [HttpPost]
        [Route("create")]
        public ActionResult OrderNew(string login)
        {
            int cellId;
            string queryforcellid = "Select CellId from Cells WHERE FridgeId = " +
                "(SELECT FridgeID FROM [User] WHERE [User].Login = @login) " +
                "AND OrderId IS NULL AND NOT CellID = " +
                "ANY (SELECT [CellId] FROM [YourFood].[dbo].[Order] WHERE Status != 'Done' AND Status != 'Declined')";
            string connectionString = Configuration.GetConnectionString("YourFood");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand sqlquery = new SqlCommand(queryforcellid, connection);


                sqlquery.Parameters.Add("@login", System.Data.SqlDbType.NChar);
                sqlquery.Parameters["@login"].Value = login;

                sqlquery.Parameters.Add("@status", System.Data.SqlDbType.NChar);
                sqlquery.Parameters["@status"].Value = "In road";


                
                 var res = sqlquery.ExecuteReader();
                res.Read();
                try
                {
                    cellId = Convert.ToInt32(res.GetValue(0).ToString());
                }
                catch
                {
                    return new StatusCodeResult(204);
                }
                res.Close();
            }

            string query = "Insert into [Order] ([Login],[Date],[Status], [PassID], CellId, [Adress]) Values(@login, GETDATE(), @status, (Select PassId From [User] Where Login = @login),@cellid, (SELECT Adress FROM [User], [Fridge] Where [User].FridgeID = [Fridge].FridgeID AND [Login] = @login)); ";
            

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand sqlquery = new SqlCommand(query, connection);


                sqlquery.Parameters.Add("@login", System.Data.SqlDbType.NChar);
                sqlquery.Parameters["@login"].Value = login;

                sqlquery.Parameters.Add("@status", System.Data.SqlDbType.NChar);
                sqlquery.Parameters["@status"].Value = "In road";

                sqlquery.Parameters.Add("@cellid", System.Data.SqlDbType.Int);
                sqlquery.Parameters["@cellid"].Value = cellId;


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
        [Route("list-for-date")]
        public List<OrderStat> OrderStat(string From, string To )
        {
            List <OrderStat> result = new List<OrderStat>();

            string query = "Select OrderID, [User].Login, [User].PassID, PassName, [Order].Adress, [Order].Status, [Order].Date, [Order].[CellId] from [Order], [User], [Fridge], [Pass] " +
                                "WHERE[User].FridgeID = Fridge.FridgeID AND[Order].PassID = Pass.PassID AND[User].Login = [Order].Login AND " +
                                    "DATE Between  \'" + From + "\'  AND \'" + To+ "\' ORDER BY [Order].Date DESC;";
            string connectionString = Configuration.GetConnectionString("YourFood");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand sqlquery = new SqlCommand(query, connection);
                var table = sqlquery.ExecuteReader();

                while (table.Read())
                {
                    OrderStat stat = new OrderStat();
                    stat.orderID = Convert.ToInt32(table.GetValue(0).ToString());
                    stat.login = table.GetValue(1).ToString();
                    stat.passID = Convert.ToInt32(table.GetValue(2).ToString());
                    stat.passName = table.GetValue(3).ToString();
                    stat.adress = table.GetValue(4).ToString();
                    stat.status = table.GetValue(5).ToString();
                    stat.date = table.GetValue(6).ToString();
                    stat.cellId = Convert.ToInt32(table.GetValue(7).ToString());
                    result.Add(stat);
                }
               
            }

            return result;
        }

        [HttpGet]
        [Route("user-order")]
        public List<OrderStat> OrderUser(string login)
        {
            List<OrderStat> result = new List<OrderStat>();

            string query = "Select OrderID, [User].Login, [User].PassID, PassName, [Order].Adress, [Order].Status, [Order].Date, [Order].[CellId] from [Order], [User], [Fridge], [Pass] " +
                                "WHERE[User].FridgeID = Fridge.FridgeID AND [Order].PassID = Pass.PassID AND[User].Login = [Order].Login AND " +
                                    "[User].login = \'" +login + "\' ORDER BY [Order].Date DESC;";
            string connectionString = Configuration.GetConnectionString("YourFood");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand sqlquery = new SqlCommand(query, connection);
                var table = sqlquery.ExecuteReader();

                while (table.Read())
                {
                    OrderStat stat = new OrderStat();
                    stat.orderID = Convert.ToInt32(table.GetValue(0).ToString());
                    stat.login = table.GetValue(1).ToString();
                    stat.passID = Convert.ToInt32(table.GetValue(2).ToString());
                    stat.passName = table.GetValue(3).ToString();
                    stat.adress = table.GetValue(4).ToString();
                    stat.status = table.GetValue(5).ToString();
                    stat.date = table.GetValue(6).ToString();
                    stat.cellId = Convert.ToInt32(table.GetValue(7).ToString());
                    result.Add(stat);
                }

            }

            return result;
        }

        [HttpPost]
        [Route("create-order-for-today")]
        public ActionResult OrdersForToday()
        {
            string query = "SELECT [Login] FROM [YourFood].[dbo].[User] WHERE PassID IS NOT NULL AND FridgeID IS NOT NULL";
            string queryForUnDublicate = "SELECT [OrderID] FROM [YourFood].[dbo].[Order] WHERE [Date] = \'" + DateTime.Today.ToString() + " \' AND Login = @login;";
            
            string connectionString = Configuration.GetConnectionString("YourFood");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                SqlCommand sqlquery = new SqlCommand(query, connection);
                var table = sqlquery.ExecuteReader();
                List<string> result = new List<string>();
                while (table.Read())
                {
                    result.Add(table.GetValue(0).ToString());   
                }
                table.Close();

                for (int i = 0; i < result.Count; i++)
                {
                    SqlCommand sqlquery1 = new SqlCommand(queryForUnDublicate, connection);

                    sqlquery1.Parameters.Add("@login", System.Data.SqlDbType.NChar);
                    sqlquery1.Parameters["@login"].Value = result[i];

                    var table1 = sqlquery1.ExecuteReader();

                    table1.Read();

                    if (!table1.HasRows)
                    {
                        OrderNew(result[i]);
                    }

                    table1.Close();
                }

            }

            return new StatusCodeResult(200);
        }

    }
}
