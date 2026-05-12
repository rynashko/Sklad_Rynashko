using System.Collections.Generic;
using System.Linq;

namespace Sklad_Rynashko
{
    public class Warehouse
    {
        // Список всех товаров (База товаров)
        private List<Product> products = new List<Product>();

        // Список всех накладных (История операций)
        private List<Invoice> invoices = new List<Invoice>();

        // Метод для получения списка товаров (Инвентарная ведомость)
        public List<Product> GetInventory() => products;

        // Алгоритм регистрации поступления (Приходная накладная)
        public void RegisterArrival(Product newProduct)
        {
            var existing = products.FirstOrDefault(p => p.Name == newProduct.Name);
            if (existing != null)
            {
                existing.Quantity += newProduct.Quantity;
                existing.LastDelivery = newProduct.LastDelivery;
            }
            else
            {
                products.Add(newProduct);
            }
            invoices.Add(new Invoice("П-" + (invoices.Count + 1), "Приходная", $"{newProduct.Name}: {newProduct.Quantity}"));
        }

        // Алгоритм регистрации отгрузки (Расходная накладная) 
        public bool RegisterShipment(string name, double qty)
        {
            var product = products.FirstOrDefault(p => p.Name == name);
            if (product != null && product.Quantity >= qty)
            {
                product.Quantity -= qty;
                invoices.Add(new Invoice("Р-" + (invoices.Count + 1), "Расходная", $"{name}: {qty}"));
                return true;
            }
            return false; // Ошибка: недостаточно товара
        }
    }
}