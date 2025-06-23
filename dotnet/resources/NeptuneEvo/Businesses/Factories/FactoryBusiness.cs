using NeptuneEvo.Core;
using Newtonsoft.Json;
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

    public class FactoryBusiness : Business
    {
        // материалы, готовая продукция и очередь производства
        public Dictionary<string, int> MaterialsStorage { get; set; } = new();
        public Dictionary<string, int> ProductsStorage { get; set; } = new();
        public List<ProductionJob> ProductionQueue { get; set; } = new();

        public FactoryBusiness(int id, string owner, int sellPrice, int type, List<Product> products,
            Vector3 enterPoint, Vector3 unloadPoint, int bankID, int mafia, List<Order> orders, double tax = 0.026)
            : base(id, owner, sellPrice, type, products, enterPoint, unloadPoint, bankID, mafia, orders, tax)
        {
        }

        // запуск производства если есть материалы
        public bool StartProduction(string product, int quantity, Dictionary<string, int> needMaterials, TimeSpan duration)
        {
            foreach (var mat in needMaterials)
            {
                if (!MaterialsStorage.ContainsKey(mat.Key) || MaterialsStorage[mat.Key] < mat.Value * quantity)
                    return false;
            }

            foreach (var mat in needMaterials)
                MaterialsStorage[mat.Key] -= mat.Value * quantity;

            ProductionQueue.Add(new ProductionJob
            {
                ProductType = product,
                Quantity = quantity,
                EndTime = DateTime.Now.Add(duration)
            });

            return true;
        }

        // проверка очереди и добавление готовых изделий на склад
        public void ProcessQueue()
        {
            var finished = new List<ProductionJob>();
            foreach (var job in ProductionQueue)
            {
                if (DateTime.Now >= job.EndTime)
                {
                    if (!ProductsStorage.ContainsKey(job.ProductType))
                        ProductsStorage[job.ProductType] = 0;

                    ProductsStorage[job.ProductType] += job.Quantity;
                    finished.Add(job);
                }
            }

            foreach (var job in finished)
                ProductionQueue.Remove(job);
        }

        // сериализация состояния для базы данных
        public string SerializeMaterials() => JsonConvert.SerializeObject(MaterialsStorage);
        public string SerializeProducts() => JsonConvert.SerializeObject(ProductsStorage);
        public string SerializeQueue() => JsonConvert.SerializeObject(ProductionQueue);

        public void LoadState(string materialsJson, string productsJson, string queueJson)
        {
            if (!string.IsNullOrEmpty(materialsJson))
                MaterialsStorage = JsonConvert.DeserializeObject<Dictionary<string, int>>(materialsJson);

            if (!string.IsNullOrEmpty(productsJson))
                ProductsStorage = JsonConvert.DeserializeObject<Dictionary<string, int>>(productsJson);

            if (!string.IsNullOrEmpty(queueJson))
                ProductionQueue = JsonConvert.DeserializeObject<List<ProductionJob>>(queueJson);
        }

        // вспомогательный метод, чтобы по ID получить фабрику
        public static FactoryBusiness GetById(int id)
        {
            return BusinessManager.Get(id) as FactoryBusiness;
        }
    }
}
