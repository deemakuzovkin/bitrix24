using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Bitrix24.Clients;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bitrix24Tests.Api
{
    /// <summary>
    /// Тестирование уведомлений.
    /// </summary>
    [TestClass]
    public class NotifycationUnitTest
    {
        /// <summary>
        /// Конструктор.
        /// </summary>
        public NotifycationUnitTest()
        {
            AuthKey = "{authKey}";
            Client = new BitrixClient(AuthKey);
        }

        /// <summary>
        /// Ключ аутентификации в приложениее.
        /// Веб хук.
        /// </summary>
        protected string AuthKey { get; }

        /// <summary>
        /// Клиет.
        /// </summary>
        protected BitrixClient Client { get; }

        /// <summary>
        /// Тестовая директория с файлами.
        /// </summary>
        protected DirectoryInfo TestFilesDirectory => Directory.CreateDirectory("TestFiles");

        /// <summary>
        /// Отправить уведомление.
        /// </summary>
        [TestMethod]
        public void SendNotify()
        {
            var dictionary = new Dictionary<string, object>();
            dictionary["message"] = "Тестовое сообщение для пользователя";
            dictionary["to"] = 1;
            var sendNotify = Client.SendNotify(dictionary);
            //var webRequest = Client.GetWebRequest("im.notify.json?message=%D0%9F%D1%80%D0%B8%D0%B2%D0%B5%D1%82%2C%20%D0%9E%D0%B4%D0%BC%D0%B8%D0%BD!&to=1");
        }

        /// <summary>
        /// Создать задачу.
        /// </summary>
        [TestMethod]
        public void AddNewTask()
        {
            var dictionary = new Dictionary<string, object>();
            dictionary["TITLE"] = "Создаине задачи из БАГ трекера";
            dictionary["DESCRIPTION"] = "Описание задачи";
            dictionary["GROUP_ID"] = 8; //Группа BugTracker
            dictionary["RESPONSIBLE_ID"] = 1; //Кузовкин Дмитрий
            dictionary["CREATED_BY"] = 6 ; //Михаил Соколов            
            var task = Client.CreateTask(dictionary);
        }

        /// <summary>
        /// Добавление задачи вместе с прикрепленными файлами.
        /// </summary>
        [TestMethod]
        public void AddTaskWithFiles()
        {
            var dictionaryFiles = new Dictionary<string, string>();
            dictionaryFiles["App 2017-05-16.log"] = $@"{TestFilesDirectory.FullName}\App 2017-05-16.log";
            dictionaryFiles["App1 2017-05-16.log"] = $@"{TestFilesDirectory.FullName}\App 2017-05-16.log";
            dictionaryFiles["App2 2017-05-16.log"] = $@"{TestFilesDirectory.FullName}\App 2017-05-16.log";

            var dictionary = new Dictionary<string, object>();
            dictionary["TITLE"] = "Создаине задачи из БАГ трекера";
            dictionary["DESCRIPTION"] = "Описание задачи";
            dictionary["GROUP_ID"] = 8; //Группа BugTracker
            dictionary["RESPONSIBLE_ID"] = 1; //Кузовкин Дмитрий
            dictionary["CREATED_BY"] = 6; //Михаил Соколов            
            var task = Client.CreateTaskWithFiles(dictionary, dictionaryFiles);
        }

        /// <summary>
        /// Добавление файла к задаче.
        /// </summary>
        [TestMethod]
        public void AddFileToTask()
        {
            var taskId = 66;
            var filePath = $@"{TestFilesDirectory.FullName}\App 2017-05-16.log";
            var asBytes = File.ReadAllBytes(filePath);
            var asBase64String = Convert.ToBase64String(asBytes);
            var uri = "https://des-and-dev.bitrix24.ru/rest/1/{authKey}/task.item.addfile.xml";
            var parameters = new System.Collections.Specialized.NameValueCollection()
            {
                {"TASK_ID", $"{taskId}"},
                {"FILE[NAME]", $"App 2017-05-16.log"},
                {"FILE[CONTENT]", asBase64String}
            };
            using (var client = new WebClient())
            {
                client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                client.QueryString = parameters;
                var responseBytes = client.UploadFile(uri, filePath);
                var response = Encoding.UTF8.GetString(responseBytes);
            }
        }
    }
}