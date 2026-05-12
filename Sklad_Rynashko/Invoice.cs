using System;

namespace Sklad_Rynashko
{
    // Класс для формирования документов (накладных)
    public class Invoice
    {
        public string Number { get; set; }  // Номер документа
        public DateTime Date { get; set; }  // Дата документа
        public string Type { get; set; }    // Тип: Приходная или Расходная
        public string Content { get; set; } // Описание операции  

        public Invoice(string number, string type, string content)
        {
            Number = number;
            Type = type;
            Content = content;
            Date = DateTime.Now;
        }
    }
}