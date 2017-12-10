using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Web.Services.Description;
using System.Linq;
using System;
using System.Collections.Generic;
using BotCampDemo.Model;
using Microsoft.ProjectOxford.Vision;
using Microsoft.Cognitive.LUIS;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Vision.Contract;
using Newtonsoft.Json.Linq;
using Microsoft.ProjectOxford.Face.Contract;
using System.Threading;
using System.Data.SqlClient;
using System.Text;

namespace Bot_Application1
{
    public class Global

    {
        public static string userid;

    }
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>

        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {

                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                Activity reply = activity.CreateReply();
                //Trace.TraceInformation(JsonConvert.SerializeObject(reply, Formatting.Indented));

                if (activity.Attachments?.Count > 0 && activity.Attachments.First().ContentType.StartsWith("image"))
                {
                    //user傳送一張照片
                    ImageTemplate(reply, activity.Attachments.First().ContentUrl);

                }
                else if (activity.Text == "help")
                {
                    reply.Text = "歡迎使用管理聊天機器人\n\n" +
                        "如需新增使用者，請輸入\n\n「名稱+『欲使用名稱』」\n\n" +
                        "例如:名稱王大明\n\n" +
                        "\n\n如需查詢使用情況，請輸入\n\n" +
                        "「查詢+本日/本周/本月+指定使用者+『欲查詢名稱』」\n\n" +
                        "例如:查詢本日指定使用者王大明\n\n" +
                        "\n\n如需查詢指定小時前所有使用者，請輸入\n\n" +
                        "「查詢幾小時前所有使用者+『欲查詢小時』\n\n" +
                        "例如:查詢幾小時前使用者8\n\n" +
                        "\n\n本聊天機器人由Godseye團隊製作。";
                }
                else if (activity.Text == "subscription")
                {
                    reply.Text = "this is subscription";
                }
                else if (activity.Text == "last")
                {
                    SQLCollectTimeOne(reply);
                }
                else if (activity.Text == "RECENT_TODAY_PAYLOAD")
                {
                    string timefinish = DateTime.UtcNow.AddHours(32).ToShortDateString();
                    string timestart = DateTime.UtcNow.AddHours(8).ToShortDateString();
                    SQLCollectTime(timestart, timefinish, reply);
                    
                }
                else if (activity.Text == "RECENT_WEEK_PAYLOAD")
                {
                    string timefinish = DateTime.UtcNow.AddHours(32).ToShortDateString();
                    string timestart = DateTime.UtcNow.AddDays(-7).ToShortDateString();
                    SQLCollectTime(timestart, timefinish, reply);
                }
                else
                {
                    //if(activity.ChannelId == "emulator")
                    if (activity.ChannelId == "facebook")
                    {
                        string nametest = activity.Text;
                        bool keyin = nametest.StartsWith("名稱");
                        bool reqTime = nametest.StartsWith("時間");
                        bool Test = nametest.StartsWith("測試");
                        bool recent = nametest.StartsWith("查詢幾小時前所有使用者");
                        bool recent_day = nametest.StartsWith("查詢本日指定使用者");
                        bool recent_week = nametest.StartsWith("查詢本周指定使用者");
                        bool recent_month = nametest.StartsWith("查詢本月指定使用者");
                        StateClient stateClient = activity.GetStateClient();
                        var fbData = JsonConvert.DeserializeObject<FBChannelModel>(activity.ChannelData.ToString());
                        if (fbData.postback != null)
                        {

                            var url = fbData.postback.payload.Split('>')[1];

                            if (fbData.postback.payload.StartsWith("Face>"))
                            {
                                //try
                                //{
                                FaceServiceClient client = new FaceServiceClient("6ef41877566d45d68b93b527f187fbfa", "https://westcentralus.api.cognitive.microsoft.com/face/v1.0");
                                CreatePersonResult result_Person = await client.CreatePersonAsync("security", Global.userid);
                                await client.AddPersonFaceAsync("security", result_Person.PersonId, url);

                                await client.TrainPersonGroupAsync("security");
                                var result = client.GetPersonGroupTrainingStatusAsync("security");
                                reply.Text = $"使用者已創立,person_id為:{result_Person.PersonId}";
                                //}
                                //catch (FaceAPIException f)
                                //{
                                //    reply.Text=""f.ErrorMessage.ToString()"+"\n\n"+"f.ErrorCode.ToString()"";
                                //    return new Face[0];
                                //}
                                //faceAPI
                            }
                            /*else if (fbData.postback.payload.StartsWith("Recent")||recent)
                            {
                                string timefinish = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                               

                                string timestart = DateTime.Now.AddHours(-6).ToString("yyyy-MM-dd HH:mm:ss");

                                SQLCollectTime(timestart, timefinish, reply);
                                //得到最近的時間
                            }*/

                            else if (fbData.postback.payload.StartsWith("TypeIn"))
                            {


                            }
                            //if (fbData.postback.payload.StartsWith("Analyze>"))
                            //{
                            //    //辨識圖片
                            //    VisionServiceClient client = new VisionServiceClient("88b8704fe3bd4483ac755befdc8624db", "https://westcentralus.api.cognitive.microsoft.com/vision/v1.0");
                            //    var result = await client.AnalyzeImageAsync(url, new VisualFeature[] { VisualFeature.Description });
                            //    reply.Text = result.Description.Captions.First().Text;
                            //}
                            else
                                reply.Text = $"nope";
                        }
                        else if (keyin)
                        {
                            Global.userid = activity.Text.Trim("名稱".ToCharArray()); //移除"名稱"
                            reply.Text = $"name set as:{Global.userid}";
                        }
                        else if (reqTime)
                        {
                            TimeTemplate(reply);
                        }
                        else if (Test)
                        {

                            SQLCollectTime("2017-09-03 12:10", "2017-09-03 12:30", reply);
                        }
                        else if (recent)
                        {
                            int before = int.Parse(activity.Text.Trim("查詢幾小時前所有使用者".ToCharArray()));
                            string timefinish = DateTime.UtcNow.AddHours(8).ToString("yyyy-MM-dd HH:mm");
                            string timestart = DateTime.UtcNow.AddHours(8 - before).ToString("yyyy-MM-dd HH:mm");
                            SQLCollectTime(timestart, timefinish, reply);
                            //得到最近的時間
                        }
                        else if (recent_day)
                        {
                            string searchid = activity.Text.Trim("查詢本日指定使用者".ToCharArray());
                            string timefinish = DateTime.UtcNow.AddHours(32).ToShortDateString();
                            string timestart = DateTime.UtcNow.AddHours(8).ToShortDateString();
                            SQLCollectTimeName(timestart, timefinish, searchid, reply);
                            //得到最近的時間
                        }
                        else if (recent_week)
                        {
                            string searchid = activity.Text.Trim("查詢本周指定使用者".ToCharArray());
                            string timefinish = DateTime.UtcNow.AddHours(32).ToShortDateString();
                            string timestart = DateTime.UtcNow.AddDays(-7).ToShortDateString();
                            SQLCollectTimeName(timestart, timefinish, searchid, reply);
                            //得到最近的時間
                        }
                        else if (recent_month)
                        {
                            string searchid = activity.Text.Trim("查詢本月指定使用者".ToCharArray());
                            string timefinish = DateTime.UtcNow.AddHours(32).ToShortDateString();
                            string timestart = DateTime.UtcNow.AddDays(-30).ToShortDateString();
                            SQLCollectTimeName(timestart, timefinish, searchid,reply);
                            //得到最近的時間
                        }
                        else
                        {
                            reply.Text = $"nope";
                        }

                    }
                }
                await connector.Conversations.ReplyToActivityAsync(reply);
            }

            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        //private async Task <string> ProcessLUIS(string text)
        //{
        //    using (LuisClient client = new LuisClient("48d2dd1c-c0e4-418b-abb9-fab10b31e5ba", "7b780ccf7f9044a2a0cfd26affd6f13b"))
        //    {
        //        var result = await client.Predict(text);
        //        if(result.Intents.Count() <= 0 || result.TopScoringIntent.Name != "查匯率")
        //        {
        //            return "看不懂";
        //        }

        //        if(result.Entities == null || !result.Entities.Any(x=>x.Key.StartsWith("幣別")))
        //        {
        //            return "目前只支援日幣與美金QQ";
        //        }
        //        var currency = result.Entities?.Where(x => x.Key.StartsWith("幣別"))?.First().Value[0].Value;
        //        return $"查詢的外幣是{currency},價格是xxx";
        //    }

        //}

        private void ImageTemplate(Activity reply, string url)
        {
            List<Attachment> att = new List<Attachment>();
            att.Add(new HeroCard()
            {
                Title = "Cognitive services",
                Subtitle = "Select from below",
                Images = new List<CardImage>() { new CardImage(url) },
                Buttons = new List<CardAction>()
                    {
                        new CardAction(ActionTypes.PostBack, "上傳使用者圖片", value: $"Face>{url}"),
                        //new CardAction(ActionTypes.PostBack, "辨識圖片", value: $"Analyze>{url}")
                    }
            }.ToAttachment());

            reply.Attachments = att;
        }
        //check
        private void TimeTemplate(Activity reply)
        {
            List<Attachment> attr = new List<Attachment>();
            attr.Add(new HeroCard()
            {
                Title = "Get time record",
                Subtitle = "Select from below",
                Buttons = new List<CardAction>()
                    {
                        new CardAction(ActionTypes.PostBack, "得到最近時間", value: "Recent time"),
                        new CardAction(ActionTypes.PostBack, "輸入時段", value: $"TypeIn")
                    }
            }.ToAttachment());

            reply.Attachments = attr;
        }
        private void SQLCollectTime(string timestart, string timefinish, Activity reply)
        {
            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                builder.DataSource = "mrlsql.database.windows.net";
                builder.UserID = "mrlsql";
                builder.Password = "MRL666@mrl";
                builder.InitialCatalog = "mrlsql";
                StringBuilder sqlresult = new StringBuilder();
                //string time = activity.Text.Trim("測試".ToCharArray());

                //string timestart = "2017-09-03 12:10", timefinish = "2017-09-03 12:30"; //yyyy-mm-dd h-m-s


                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    connection.Open();
                    StringBuilder sb = new StringBuilder();
                    sb.Append("SELECT * FROM [dbo].[detect] WHERE detecttime >= CONVERT(datetime,'");
                    sb.Append(timestart);
                    sb.Append("', 110) and detecttime <= CONVERT(datetime,'");
                    sb.Append(timefinish);
                    sb.Append("', 110) order by detecttime; ");
                    /*sb.Append("FROM [SalesLT].[ProductCategory] pc ");
                    sb.Append("JOIN [SalesLT].[Product] p ");
                    sb.Append("ON pc.productcategoryid = p.productcategoryid;");*/
                    String sql = sb.ToString();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            //sqlresult.Append("出現時間: \n\n");
                            //sqlresult.Append(timestart);
                            //sqlresult.Append(" till ");
                            //sqlresult.Append(timefinish);
                            //sqlresult.Append("\n\n");

                            while (reader.Read())
                            {
                                //Console.WriteLine("{0} {1}", reader.GetString(0), reader.GetString(1));
                                sqlresult.Append(reader.GetString(1));
                                sqlresult.Append(" ");
                                sqlresult.Append(reader.GetDateTime(2).ToString("yyyy-MM-dd HH:mm:ss"));
                                sqlresult.Append("\n\n");

                                reply.Text = sqlresult.ToString();
                            }
                        }
                    }
                    connection.Close();
                }
            }
            catch (SqlException e)
            {
                reply.Text = $"{ e.ToString()}";
            }

        }
        private void SQLCollectTimeOne(Activity reply)
        {
            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                builder.DataSource = "mrlsql.database.windows.net";
                builder.UserID = "mrlsql";
                builder.Password = "MRL666@mrl";
                builder.InitialCatalog = "mrlsql";
                StringBuilder sqlresult = new StringBuilder();
                //string time = activity.Text.Trim("測試".ToCharArray());

                //string timestart = "2017-09-03 12:10", timefinish = "2017-09-03 12:30"; //yyyy-mm-dd h-m-s


                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    connection.Open();
                    StringBuilder sb = new StringBuilder();
                    sb.Append("SELECT TOP 1 [PersonID],[Person],[detecttime] FROM [dbo].[detect] ORDER BY detecttime DESC");
                    /*sb.Append("FROM [SalesLT].[ProductCategory] pc ");
                    sb.Append("JOIN [SalesLT].[Product] p ");
                    sb.Append("ON pc.productcategoryid = p.productcategoryid;");*/
                    String sql = sb.ToString();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            //sqlresult.Append("出現時間: \n\n");
                            //sqlresult.Append(timestart);
                            //sqlresult.Append(" till ");
                            //sqlresult.Append(timefinish);
                            //sqlresult.Append("\n\n");

                            while (reader.Read())
                            {
                                //Console.WriteLine("{0} {1}", reader.GetString(0), reader.GetString(1));
                                sqlresult.Append(reader.GetString(1));
                                sqlresult.Append(" ");
                                sqlresult.Append(reader.GetDateTime(2).ToString("yyyy-MM-dd HH:mm:ss"));
                                sqlresult.Append("\n\n");

                                reply.Text = sqlresult.ToString();
                            }
                        }
                    }
                    connection.Close();
                }
            }
            catch (SqlException e)
            {
                reply.Text = $"{ e.ToString()}";
            }



        }
        private void SQLCollectTimeName(string timestart, string timefinish,string name, Activity reply)
        {
            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                builder.DataSource = "mrlsql.database.windows.net";
                builder.UserID = "mrlsql";
                builder.Password = "MRL666@mrl";
                builder.InitialCatalog = "mrlsql";
                StringBuilder sqlresult = new StringBuilder();                
                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    connection.Open();
                    StringBuilder sb = new StringBuilder();
                    sb.Append("SELECT * FROM [dbo].[detect] WHERE detecttime >= CONVERT(datetime,'");
                    sb.Append(timestart);
                    sb.Append("', 110) and detecttime <= CONVERT(datetime,'");
                    sb.Append(timefinish);
                    sb.Append("', 110) and Person = '"+name+"' ORDER BY detecttime DESC ");
                    //sb.Append("SELECT * FROM [dbo].[detect] WHERE Person = '"+name+"' ORDER BY detecttime DESC");
                    /*sb.Append("FROM [SalesLT].[ProductCategory] pc ");
                    sb.Append("JOIN [SalesLT].[Product] p ");
                    sb.Append("ON pc.productcategoryid = p.productcategoryid;");*/
                    String sql = sb.ToString();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            //sqlresult.Append("出現時間: \n\n");
                            //sqlresult.Append(timestart);
                            //sqlresult.Append(" till ");
                            //sqlresult.Append(timefinish);
                            //sqlresult.Append("\n\n");

                            while (reader.Read())
                            {
                                //Console.WriteLine("{0} {1}", reader.GetString(0), reader.GetString(1));
                                sqlresult.Append(reader.GetString(1));
                                sqlresult.Append(" ");
                                sqlresult.Append(reader.GetDateTime(2).ToString("yyyy-MM-dd HH:mm:ss"));
                                sqlresult.Append("\n\n");

                                reply.Text = sqlresult.ToString();
                            }
                        }
                    }
                    connection.Close();
                }
            }
            catch (SqlException e)
            {
                reply.Text = $"{ e.ToString()}";
            }



        }
        private void SQLReserveTimeName(string timestart, string timefinish, string name, Activity reply)
        {
            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                builder.DataSource = "mrlsql.database.windows.net";
                builder.UserID = "mrlsql";
                builder.Password = "MRL666@mrl";
                builder.InitialCatalog = "mrlsql";
                StringBuilder sqlresult = new StringBuilder();
                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    connection.Open();
                    StringBuilder sb = new StringBuilder();
                    sb.Append("SELECT * FROM [dbo].[reservation] WHERE detecttime >= CONVERT(datetime,'");
                    sb.Append(timestart);
                    sb.Append("', 110) and detecttime <= CONVERT(datetime,'");
                    sb.Append(timefinish);
                    sb.Append("', 110) and Person = '" + name + "' ORDER BY detecttime DESC ");
                    //sb.Append("SELECT * FROM [dbo].[detect] WHERE Person = '"+name+"' ORDER BY detecttime DESC");
                    /*sb.Append("FROM [SalesLT].[ProductCategory] pc ");
                    sb.Append("JOIN [SalesLT].[Product] p ");
                    sb.Append("ON pc.productcategoryid = p.productcategoryid;");*/
                    String sql = sb.ToString();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            //sqlresult.Append("出現時間: \n\n");
                            //sqlresult.Append(timestart);
                            //sqlresult.Append(" till ");
                            //sqlresult.Append(timefinish);
                            //sqlresult.Append("\n\n");

                            while (reader.Read())
                            {
                                //Console.WriteLine("{0} {1}", reader.GetString(0), reader.GetString(1));
                                sqlresult.Append(reader.GetString(1));
                                sqlresult.Append(" ");
                                sqlresult.Append(reader.GetDateTime(2).ToString("yyyy-MM-dd HH:mm:ss"));
                                sqlresult.Append("\n\n");

                                reply.Text = sqlresult.ToString();
                            }
                        }
                    }
                    connection.Close();
                }
            }
            catch (SqlException e)
            {
                reply.Text = $"{ e.ToString()}";
            }



        }
        private void SQLReserveTimeSearch(string timestart, string timefinish, string name, Activity reply)
        {
            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                builder.DataSource = "mrlsql.database.windows.net";
                builder.UserID = "mrlsql";
                builder.Password = "MRL666@mrl";
                builder.InitialCatalog = "mrlsql";
                StringBuilder sqlresult = new StringBuilder();
                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    connection.Open();
                    StringBuilder sb = new StringBuilder();
                    sb.Append("SELECT * FROM [dbo].[reservation] WHERE detecttime >= CONVERT(datetime,'");
                    sb.Append(timestart);
                    sb.Append("', 110) and detecttime <= CONVERT(datetime,'");
                    sb.Append(timefinish);
                    sb.Append("', 110) and Person = '" + name + "' ORDER BY detecttime DESC ");
                    //sb.Append("SELECT * FROM [dbo].[detect] WHERE Person = '"+name+"' ORDER BY detecttime DESC");
                    /*sb.Append("FROM [SalesLT].[ProductCategory] pc ");
                    sb.Append("JOIN [SalesLT].[Product] p ");
                    sb.Append("ON pc.productcategoryid = p.productcategoryid;");*/
                    String sql = sb.ToString();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            //sqlresult.Append("出現時間: \n\n");
                            //sqlresult.Append(timestart);
                            //sqlresult.Append(" till ");
                            //sqlresult.Append(timefinish);
                            //sqlresult.Append("\n\n");

                            while (reader.Read())
                            {
                                //Console.WriteLine("{0} {1}", reader.GetString(0), reader.GetString(1));
                                sqlresult.Append(reader.GetString(1));
                                sqlresult.Append(" ");
                                sqlresult.Append(reader.GetDateTime(2).ToString("yyyy-MM-dd HH:mm:ss"));
                                sqlresult.Append("\n\n");

                                reply.Text = sqlresult.ToString();
                            }
                        }
                    }
                    connection.Close();
                }
            }
            catch (SqlException e)
            {
                reply.Text = $"{ e.ToString()}";
            }



        }

    }
}