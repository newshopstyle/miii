using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Database;
using LinqToDB;
using NeptuneEvo.Businesses.Factories;
using NeptuneEvo.Core;
using Newtonsoft.Json;
using Redage.SDK;

namespace NeptuneEvo.Database.Models
{
    public class Factory
    {
        private static readonly nLog Log = new nLog("Database.Factory");

        public static void Start()
        {
            var thread = new Thread(Worker);
            thread.IsBackground = true;
            thread.Name = "FactorySave";
            thread.Start();
        }
        private static async void Worker()
        {
            while (true)
            {
                try
                {
                    var factories = BusinessManager.BizList
                        .Where(b => b.Value.Type == 16)
                        .Select(b => b.Value as FactoryBusiness)
                        .Where(f => f != null)
                        .ToList();

                    if (factories.Count > 0)
                    {
                        await using var db = new ServerBD("MainDB");

                        foreach (var factory in factories)
                        {
                            await db.FactoryDatas
                                .Where(f => f.Id == factory.ID)
                                .Set(f => f.Materials, factory.SerializeMaterials())
                                .Set(f => f.Products, factory.SerializeProducts())
                                .Set(f => f.Queue, factory.SerializeQueue())
                                .UpdateAsync();
                        }
                    }
                }
                catch (Exception e)
                {
                    Debugs.Repository.Exception(e);
                }
                Thread.Sleep(1000 * 30);
            }
        }
    }
}
