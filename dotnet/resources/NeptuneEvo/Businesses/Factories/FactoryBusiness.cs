using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace NeptuneEvo.Businesses.Factories
{
    // информация о задании на производство
    public class ProductionJob
    {
        public string ProductType { get; set; }
        public int Quantity { get; set; }
        public DateTime EndTime { get; set; }
    }

    // бизнес типа "Фабрика"
    public class FactoryBusiness : Core.Business
    {
        // склады материалов и готовой продукции
        public Dictionary<string, int> MaterialsStorage { get; set; } = new();
        public Dictionary<string, int> ProductsStorage { get; set; } = new();
        public List<ProductionJob> ProductionQueue { get; set; } = new();

        public FactoryBusiness(int id, string owner, int sellPrice, int type, System.Collections.Generic.List<Core.Product> products, GTANetworkAPI.Vector3 enterPoint, GTANetworkAPI.Vector3 unloadPoint, int bankID, int mafia, System.Collections.Generic.List<Core.Order> orders, double tax = 0.026)
            : base(id, owner, sellPrice, type, products, enterPoint, unloadPoint, bankID, mafia, orders, tax)
        {
        }

        public static FactoryBusiness GetById(int id)
        {
            return Core.BusinessManager.BizList.ContainsKey(id) ? Core.BusinessManager.BizList[id] as FactoryBusiness : null;
        }

        // запускаем производство, если хватает материалов
        public bool TryStartProduction(string productType, int quantity, Dictionary<string, int> requiredMaterials, TimeSpan craftTime)
        {
            foreach (var req in requiredMaterials)
            {
                if (!MaterialsStorage.TryGetValue(req.Key, out var have) || have < req.Value * quantity)
                    return false;
            }

            foreach (var req in requiredMaterials)
                MaterialsStorage[req.Key] -= req.Value * quantity;

            ProductionQueue.Add(new ProductionJob
            {
                ProductType = productType,
                Quantity = quantity,
                EndTime = DateTime.Now.Add(craftTime)
            });

            return true;
        }

        // переносим готовые продукты на склад
        public void ProcessQueue()
        {
            var finished = ProductionQueue.FindAll(j => j.EndTime <= DateTime.Now);
            foreach (var job in finished)
            {
                if (ProductsStorage.ContainsKey(job.ProductType))
                    ProductsStorage[job.ProductType] += job.Quantity;
                else
                    ProductsStorage[job.ProductType] = job.Quantity;

                ProductionQueue.Remove(job);
            }
        }

        // сериализация для сохранения в БД
        public string SerializeMaterials() => JsonConvert.SerializeObject(MaterialsStorage);
        public string SerializeProducts() => JsonConvert.SerializeObject(ProductsStorage);
        public string SerializeQueue() => JsonConvert.SerializeObject(ProductionQueue);

        public void LoadData(string materials, string products, string queue)
        {
            if (!string.IsNullOrEmpty(materials))
                MaterialsStorage = JsonConvert.DeserializeObject<Dictionary<string, int>>(materials);
            if (!string.IsNullOrEmpty(products))
                ProductsStorage = JsonConvert.DeserializeObject<Dictionary<string, int>>(products);
            if (!string.IsNullOrEmpty(queue))
                ProductionQueue = JsonConvert.DeserializeObject<List<ProductionJob>>(queue);
        }
    }
}

