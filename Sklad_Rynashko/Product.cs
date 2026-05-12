using System;

namespace Sklad_Rynashko
{
    // Класс, описывающий товар на складе 
    public class Product
    {
        public string Name { get; set; }           // Наименование
        public string Unit { get; set; }           // Единица измерения
        public double Quantity { get; set; }       // Количество
        public DateTime LastDelivery { get; set; } // Дата последнего завоза

        public Product(string name, string unit, double quantity, DateTime lastDelivery)
        {
            Name = name;
            Unit = unit;
            Quantity = quantity;
            LastDelivery = lastDelivery;
        }
    }
}