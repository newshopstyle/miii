using NeptuneEvo.Businesses;      // базовый класс
using NeptuneEvo.Businesses.Models; // если есть
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

        // вспомогательный метод, чтобы по ID получить фабрику
        public static FactoryBusiness GetById(int id)
        {
            return BusinessManager.Get(id) as FactoryBusiness;
        }
    }
}
