using NeptuneEvo.Businesses;      // базовый класс
using NeptuneEvo.Businesses.Models; // если есть
using NeptuneEvo.Core;
using NeptuneEvo.Jobs;
using System;
using System.Collections.Generic;

namespace NeptuneEvo.Businesses.Factories
{
    public class ProductionJob
    {
        public string ProductType { get; set; }
        public int Quantity { get; set; }
        public DateTime EndTime { get; set; }
    }

    public class FactoryBusiness : BaseBusiness  // или нужный вам родитель
    {
        // здесь храним материалы, продукцию и очередь
        public Dictionary<string, int> MaterialsStorage { get; set; } = new();
        public Dictionary<string, int> ProductsStorage { get; set; } = new();
        public List<ProductionJob> ProductionQueue { get; set; } = new();
        public List<Order> Orders { get; set; } = new();

        // вспомогательный метод, чтобы по ID получить фабрику
        public static FactoryBusiness GetById(int id)
        {
            return BusinessManager.Get(id) as FactoryBusiness;
        }

        // Заказать сырье с шахты. Возвращает true при успешном списании ресурсов
        public bool PlaceMaterialOrder(string materialKey, int amount)
        {
            var oreIndex = Miner.GetOreIndex(materialKey);
            if (oreIndex == -1)
                return false;

            if (!Miner.TryTakePlantStock(oreIndex, amount))
                return false;

            var order = new Order(materialKey, amount);
            var random = new Random();
            do
            {
                order.UID = random.Next(000000, 999999);
            } while (BusinessManager.Orders.ContainsKey(order.UID));

            Orders.Add(order);
            BusinessManager.Orders.TryAdd(order.UID, ID);
            return true;
        }
    }
}
