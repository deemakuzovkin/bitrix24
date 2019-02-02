using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Bitrix24.Clients
{
    /// <summary>
    /// Клиент для битрикса.
    /// </summary>
    /// <seealso cref="System.Net.WebClient" />
    public class BitrixClient : WebClient
    {
        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="key">Ключ авторизации.</param>
        /// <param name="authUserId">Идентификатор пользователя от которого будет исходить запросы.</param>
        /// <param name="uriBase">Базовый адрес портала.</param>
        public BitrixClient(string key,int authUserId = 1, string uriBase = "des-and-dev.bitrix24.ru")
        {
            UriBase = $@"https://{uriBase}/rest/{authUserId}/{key}/";
        }

        /// <summary>
        /// Базовый адрес портала.
        /// </summary>
        protected string UriBase { get; }

        /// <summary>
        /// Отправить запрос.
        /// </summary>
        /// <param name="postfix">Постфикс запроса.</param>
        /// <returns> Результат. </returns>
        public string GetWebRequest(string postfix)
        {
            var webRequest = GetWebRequest(new Uri($"{UriBase}{postfix}"));
            if (webRequest == null)
            {
                return string.Empty;
            }
            var webResponse = GetWebResponse(webRequest);
            var responseStream = webResponse?.GetResponseStream();
            if (responseStream == null)
            {
                return string.Empty;
            }
            var sr = new StreamReader(responseStream);
            return sr.ReadToEnd();
        }

        /// <summary>
        /// Отправить уведомление.
        /// </summary>
        /// <param name="parameters">Параметры.</param>
        /// <returns>Рузельтат.</returns>
        public string SendNotify(Dictionary<string,object> parameters)
        {
            var join = GetParameters(parameters);
            return GetWebRequest($"im.notify.json?{join}");
        }

        /// <summary>
        /// Создать задачу.
        /// </summary>
        /// <param name="parameters">Параметры.</param>
        /// <returns></returns>
        public string CreateTask(Dictionary<string, object> parameters)
        {
            var join = GetParametersForTasks(parameters);
            return GetWebRequest($"task.item.add.json?&0{join}");
        }

        /// <summary>
        /// Создание задачи вместе с прикрепленными файлами.
        /// </summary>
        /// <param name="parameters">Параметры задачи.</param>
        /// <param name="files">Файлы.</param>
        /// <returns>Результат.</returns>
        public string CreateTaskWithFiles(Dictionary<string, object> parameters, Dictionary<string, string> files)
        {
            try
            {
                var taskParameters = GetParametersForTasks(parameters);
                var resultTask = GetWebRequest($"task.item.add.json?&0{taskParameters}");
                //{"result":68}
                var taskId = int.Parse(Regex.Match(resultTask, @"\d{1,}").Value);
                foreach (var file in files)
                {
                    var fileName = file.Key;
                    var filePath = file.Value;
                    var asBytes = File.ReadAllBytes(filePath);
                    var asBase64String = Convert.ToBase64String(asBytes);
                    var uri = $"{UriBase}task.item.addfile.xml";
                    var fileParameters = new System.Collections.Specialized.NameValueCollection()
                {
                    {"TASK_ID", $"{taskId}"},
                    {"FILE[NAME]", fileName},
                    {"FILE[CONTENT]", asBase64String}
                };
                    using (var client = new WebClient())
                    {
                        client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                        client.QueryString = fileParameters;
                        var responseBytes = client.UploadFile(uri, filePath);
                        var response = Encoding.UTF8.GetString(responseBytes);
                    }
                }
                return "Completed";
            }
            catch (Exception ex)
            {
                return $"Ошибка: {ex}";
            }
        }

        /// <summary>
        /// Returns a <see cref="T:System.Net.WebRequest" /> object for the specified resource.
        /// </summary>
        /// <param name="address">A <see cref="T:System.Uri" /> URL адрес запроса.</param>
        /// <returns>
        /// A new <see cref="T:System.Net.WebRequest" /> object for the specified resource.
        /// </returns>
        protected override WebRequest GetWebRequest(Uri address)
        {
            var webRequest = base.GetWebRequest(address);
            return webRequest;
        }

        /// <summary>
        /// Получить параметры в виде строкового представления.
        /// </summary>
        /// <param name="parameters">Словарь параметров.</param>
        /// <returns>Результат.</returns>
        protected string GetParameters(Dictionary<string, object> parameters)
        {
            var join = string.Join("&", parameters.Select(x => string.Join("", $"{x.Key}={x.Value}")));
            return @join;
        }

        /// <summary>
        /// Получить параметры для задач, в виде строкового представления.
        /// </summary>
        /// <param name="parameters">Словарь параметров.</param>
        /// <returns>Результат.</returns>
        protected string GetParametersForTasks(Dictionary<string, object> parameters)
        {
            var join = string.Join("&0", parameters.Select(x => string.Join("", $"[{x.Key}]={x.Value}")));
            return @join;
        }
    }
}
