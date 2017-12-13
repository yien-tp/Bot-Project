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
using System.Data;

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
                        bool delete_user = nametest.StartsWith("刪除使用者");
                        bool keyin = nametest.StartsWith("名稱");
                        bool Test = nametest.StartsWith("測試");
                        bool res_time = nametest.StartsWith("預約");
                        bool res_delete = nametest.StartsWith("刪除預約");
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
                                AddPersistedFaceResult result_add = await client.AddPersonFaceAsync("security", result_Person.PersonId, url);                           
                                await client.TrainPersonGroupAsync("security");
                                TrainingStatus result =await client.GetPersonGroupTrainingStatusAsync("security");
                                reply.Text = $"使用者已創立,person_id為:{result_Person.PersonId}";
                                SQLNameRegister(Global.userid, reply);
                                //}
                                //catch (FaceAPIException f)
                                //{
                                //    reply.Text=""f.ErrorMessage.ToString()"+"\n\n"+"f.ErrorCode.ToString()"";
                                //    return new Face[0];
                                //}
                                //faceAPI
                            }
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
                        else if (delete_user)
                        {
                            string username = activity.Text.Trim("刪除使用者".ToCharArray());
                            //TODO
                            //SQLDELETENAME select  personid where username == username
                            //delete api
                        }
                        else if (Test)
                        {
                            string ChanData = activity.ChannelData.ToString();
                            reply.Text = ChanData;
                        }
                        else if (recent)
                        {
                            int before = int.Parse(activity.Text.Trim("查詢幾小時前所有使用者".ToCharArray()));
                            string timefinish = DateTime.UtcNow.AddHours(8).ToString("yyyy-MM-dd HH:mm");
                            string timestart = DateTime.UtcNow.AddHours(8 - before).ToString("yyyy-MM-dd HH:mm");
                            SQLCollectTime(timestart, timefinish, reply);
                            //得到最近的時間
                        }
                        else if (res_time)
                        {
                            string searchid = activity.Text.Trim("預約".ToCharArray());
                            string[] strs = {};
                            DateTime dateValue_start;
                            DateTime dateValue_finish;
                            strs = searchid.Split(new string[] { "@" }, StringSplitOptions.None);
                            System.Text.StringBuilder sb = new System.Text.StringBuilder();
                            if (strs.Length == 3)
                            {
                                if (DateTime.TryParse(strs[1], out dateValue_start))
                                {
                                    if (DateTime.TryParse(strs[2], out dateValue_finish))
                                    {
                                       double s = new TimeSpan(dateValue_finish.Ticks - dateValue_start.Ticks).TotalMinutes;
                                        if ((dateValue_finish > dateValue_start) && (s <= 300))
                                        {
                                            if (SQLReserveTimeNameCheck(strs[0]))
                                            {
                                                if(!SQLReserveTimeIsConflict(dateValue_start, dateValue_finish, strs[0]))
                                                {
                                                    SQLReserveTimeInsert(dateValue_start, dateValue_finish, strs[0],reply);
                                                    
                                                }
                                                else
                                                {
                                                    reply.Text = "指定時間已被預約，請重新選擇。";
                                                }
                                            }
                                            else
                                            {
                                                reply.Text = "使用者不在資料庫中，請先註冊再做預約。";
                                            }
                                        }
                                        else
                                        {
                                            reply.Text = "開始時間晚於結束時間或輸入時間大於5小時，請重新輸入。";
                                        }
                                        
                                    }
                                    else
                                    {
                                        reply.Text = ("日期格式錯誤，請參照範例格式。");
                                    }
                                }
                                else
                                {
                                    reply.Text = ("日期格式錯誤，請參照範例格式。");
                                }
                            }
                            else
                            {
                                reply.Text = "請輸入格式:名稱@起始時間@結束時間";
                            }
                        }
                        else if (res_delete)
                        {
                            string searchid = activity.Text.Trim("刪除預約".ToCharArray());
                            //reply.Text = SQLReserveTimeDelete(searchid);
                            CreateButtonOne(reply,searchid);
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
        private void CreateButtonOne(Activity reply,string name)
        {
            string del_time = SQLReserveTimeDelete(name);
            string[] strs = del_time.Split(new string[] { "@@" }, StringSplitOptions.None);
            //reply.Text = (strs[0]+strs[1]+strs[2]);
            List<Attachment> att = new List<Attachment>();
            for (int i = 0; i < 3; i++)
            {
                att.Add(new HeroCard()
                {
                    Title = "請問要刪除\n\n" + strs[i] + "",
                    Buttons = new List<CardAction>()
                    {
                        new CardAction(ActionTypes.PostBack,"是", value: "s"),
                    }

                }.ToAttachment());
            }
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            reply.Attachments = att;
            if (strs.Length > 1)
            {

            }

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

                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    connection.Open();
                    StringBuilder sb = new StringBuilder();
                    sb.Append("SELECT * FROM [dbo].[detect] WHERE detecttime >= CONVERT(datetime,'");
                    sb.Append(timestart);
                    sb.Append("', 110) and detecttime <= CONVERT(datetime,'");
                    sb.Append(timefinish);
                    sb.Append("', 110) order by detecttime; ");
                    String sql = sb.ToString();

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
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
        private bool SQLReserveTimeNameCheck(string name)
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
                    sb.Append("IF EXISTS(SELECT 1 FROM [dbo].[users] WHERE Name = '"+name+"') BEGIN " +
                        "SELECT 'True' END ELSE BEGIN SELECT 'False' END");

                    String sql = sb.ToString();
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                sqlresult.Append(reader.GetString(0));
                            }
                            if (sqlresult.ToString() == "True")
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                            return false;
                        }
                    }
                    connection.Close();
                }
            }
            catch (SqlException e)
            {
                return false;
            }
        }
        private bool SQLReserveTimeIsConflict(DateTime timestart, DateTime timefinish, string name)
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
                    sb.Append("IF (SELECT COUNT(Person) FROM dbo.reservation WHERE StartTime <= CONVERT(datetime,'"+timefinish+"',110) and EndTime >= CONVERT(datetime,'"+timefinish+"',110)" +
                        " or StartTime <= CONVERT(datetime,'"+timestart+"',110) and EndTime >= CONVERT(datetime,'"+timestart+"',110)" +
                        " or StartTime >= CONVERT(datetime,'"+timestart+"',110) and EndTime <= CONVERT(datetime,'"+timefinish+"',110)) > 0 "+
                        " BEGIN SELECT 'True' END "+  
                        " ELSE BEGIN SELECT 'False' END");
                    String sql = sb.ToString();
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {                      
                                sqlresult.Append(reader.GetString(0));
                            }
                            if (sqlresult.ToString() == "True")
                            {
                                return true;
                                
                            }
                            else
                            {
                                return false;
                                
                            }
                            
                        }
                    }
                    connection.Close();
                    
                }
            }
            catch (SqlException e)
            {
                return false;
                //reply.Text = $"{ e.ToString()}";
            }



        }
        private void SQLReserveTimeInsert(DateTime timestart, DateTime timefinish, string name, Activity reply)
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
                    sb.Append("INSERT INTO [dbo].[reservation]([Person],[StartTime],[EndTime]) VALUES('" + name + "', CONVERT(smalldatetime,'"+timestart+ "',110), CONVERT(smalldatetime,'"+timefinish+"',110))");
                    String sql = sb.ToString();
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                sqlresult.Append(reader.GetString(0));
                            }
                            reply.Text = "預約成功!請記得於預約時間使用!\n\n若無需使用請取消預約!";
                        }
                    }
                    connection.Close();
                }
            }
            catch (SqlException e)
            {
                reply.Text = "資料庫寫入失敗，請確認輸入格式正確後，稍後再試。";
                //reply.Text = $"{ e.ToString()}";
            }



        }
        private string SQLReserveTimeDelete(string name)
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
                    sb.Append("SELECT * FROM [dbo].[reservation] WHERE Person = '"+name+"' AND StartTime >= DateAdd(HH, 8, Getdate())");
                    String sql = sb.ToString();
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                sqlresult.Append(reader.GetDateTime(1).ToString("yyyy-MM-dd HH:mm"));
                                sqlresult.Append("~");
                                sqlresult.Append(reader.GetDateTime(2).ToString("yyyy-MM-dd HH:mm"));
                                sqlresult.Append("@@");
                                //return sqlresult.ToString();                                
                            }
                            return sqlresult.ToString();

                        }
                    }
                    connection.Close();
                }
            }
            catch (SqlException e)
            {
                return "error";
                //reply.Text = "資料庫無預約資料。";
                //reply.Text = $"{ e.ToString()}";
            }



        }
        private void SQLNameRegister(string name, Activity reply)
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
                    sb.Append("INSERT INTO [dbo].[users]([NAME]) VALUES('"+name+"')");
                    String sql = sb.ToString();
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                sqlresult.Append(reader.GetString(0));
                            }
                            reply.Text = sqlresult.ToString();
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
        private void SQLNameDelete(string name, Activity reply)
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
                    sb.Append("IF EXISTS(SELECT 1 FROM [dbo].[users] WHERE Name = 'Ian') BEGIN SELECT 'True' END ELSE BEGIN SELECT 'False' END");
                    String sql = sb.ToString();
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                sqlresult.Append(reader.GetString(0));
                            }
                            reply.Text = sqlresult.ToString();
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