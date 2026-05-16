using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sklad_Rynashko
{
    public class Warehouse
    {
        private List<Product> products = new List<Product>();
        private List<Invoice> invoices = new List<Invoice>();

        // Регистрация прихода (Формирование приходной накладной)
        public void RegisterArrival(Product newProduct)
        {
            var existing = products.FirstOrDefault(p => p.Name.Equals(newProduct.Name, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                existing.Quantity += newProduct.Quantity;
                existing.LastDelivery = newProduct.LastDelivery;
            }
            else
            {
                products.Add(newProduct);
            }

            string invNumber = $"ПР-{DateTime.Now:yyyyMMdd}-{invoices.Count(i => i.Type == "Приходная") + 1}";
            invoices.Add(new Invoice(invNumber, "Приходная", $"{newProduct.Name}: {newProduct.Quantity} {newProduct.Unit}"));
        }

        // Регистрация отгрузки (Формирование расходной накладной)
        public bool RegisterShipment(string name, double qty)
        {
            var product = products.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (product != null && product.Quantity >= qty)
            {
                product.Quantity -= qty;

                string invNumber = $"РМ-{DateTime.Now:yyyyMMdd}-{invoices.Count(i => i.Type == "Расходная") + 1}";
                invoices.Add(new Invoice(invNumber, "Расходная", $"{name}: {qty} {product.Unit}"));
                return true;
            }
            return false;
        }

        // Полное списание / Удаление позиции
        public bool DeleteProduct(string name)
        {
            var product = products.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (product != null)
            {
                string invNumber = $"СП-{DateTime.Now:yyyyMMdd}-{invoices.Count(i => i.Type == "Списание") + 1}";
                invoices.Add(new Invoice(invNumber, "Списание", $"{product.Name} полностью убран со склада"));
                products.Remove(product);
                return true;
            }
            return false;
        }

        // Вывод инвентарной ведомости в файл экспорта
        public string ExportInventoryToTXT()
        {
            string folderPath = AppDomain.CurrentDomain.BaseDirectory;
            string filePath = Path.Combine(folderPath, "Инвентарная_ведомость.txt");

            using (StreamWriter writer = new StreamWriter(filePath, false, System.Text.Encoding.UTF8))
            {
                writer.WriteLine("=========================================================");
                writer.WriteLine($"        ИНВЕНТАРНАЯ ВЕДОМОСТЬ СКЛАДА ОТ {DateTime.Now:dd.MM.yyyy}");
                writer.WriteLine("=========================================================");
                writer.WriteLine($"{"Наименование",-20} | {"Ед. изм.",-10} | {"Остаток",-10} | {"Дата обновления",-15}");
                writer.WriteLine("---------------------------------------------------------");

                foreach (var p in products)
                {
                    writer.WriteLine($"{p.Name,-20} | {p.Unit,-10} | {p.Quantity,-10} | {p.LastDelivery:dd.MM.yyyy HH:mm}");
                }
                writer.WriteLine("=========================================================");
                writer.WriteLine($"Всего наименований товаров: {products.Count}");
            }
            return filePath;
        }

        // Методы доступа к спискам данных
        public List<Invoice> GetInvoices()
        {
            return invoices;
        }

        public List<Product> GetInventory()
        {
            return products;
        }
    }
}