using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using System.Data.SqlClient;
using YourFoodBackend.Model;
using System.Diagnostics;

namespace YourFoodBackend.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class IoTTemperatureUpdateController
    {
        private IConfiguration Configuration;
        public IoTTemperatureUpdateController(IConfiguration _configuration)
        {
            Configuration = _configuration;
        }
        


        [HttpPatch]
        [Route("run-update-temperature-IoT")]
        public async Task Handle_Received_Application_Message()
        {

            var mqttFactory = new MqttFactory();

            using (var mqttClient = mqttFactory.CreateMqttClient())
            {
                var mqttClientOptions = new MqttClientOptionsBuilder()
                    .WithClientId("Onufriy_web_api")
                    .WithTcpServer("915fca6a684f4c9d82640fe1aed4e208.s2.eu.hivemq.cloud")
                    .WithCredentials("Onufriy", "5432165Wq")
                    .WithTls()
                    .WithCleanSession()
                    .Build();

                
                

                var a = await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

                mqttClient.ApplicationMessageReceivedAsync += e =>
                {
                    
                    var a = System.Text.Encoding.Default.GetString(e.ApplicationMessage.Payload);
                    FridgeController fridge = new FridgeController(Configuration);
                    FridgeTemperature temp = Newtonsoft.Json.JsonConvert.DeserializeObject<FridgeTemperature>(a);
                    try
                    {
                        fridge.FridgeUpdateTemperature(temp);
                    }
                    catch
                    {
                        return Task.CompletedTask;
                    }
                    
                    Console.WriteLine($"Fridge {temp.fridgeId} update temperature to {temp.temperature}");
                    return Task.CompletedTask;
                };
                var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
                    .WithTopicFilter(
                        f =>
                        {
                            f.WithTopic("temperature");
                        })
                    .Build();

                var b = await mqttClient.SubscribeAsync(mqttSubscribeOptions);


                Console.WriteLine("Update fridge temperature start.");

                Console.ReadLine();
                Console.WriteLine("Update fridge temperature stop.");
            }
        }

       

    }
}