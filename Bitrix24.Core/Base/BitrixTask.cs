using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitrix24.Core.Base
{
    /// <summary>
    /// Задача в битриксе
    /// </summary>
    public class BitrixTask
    {
        /// <summary>
        /// Конструктор.
        /// </summary>
        public BitrixTask()
        {
        }

        /// <summary>
        /// Заголовок.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Тело сообщения.
        /// </summary>
        public string Body { get; set; }


    }
}
