using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Products;

namespace clothingproduct
{
    interface IClothingRepository {
        Task<IEnumerable<ClothProduct>> Getallproducts();
        Task AddProduct(ClothProduct cp);
        Task DeleteProduct(int productid);

        Task<ClothProduct> GetProduct(int productid);

    }

    class ClothingRepository : IClothingRepository
    {
        private IReliableStateManager _manager;
        public ClothingRepository(IReliableStateManager manager)
        {
            _manager = manager;
        }
        public async Task AddProduct(ClothProduct cp)
        {
           var dictionary = await _manager.GetOrAddAsync<IReliableDictionary<int, ClothProduct>>("clothes");
            using (var tx = _manager.CreateTransaction()) { 
                await dictionary.AddOrUpdateAsync(tx, cp.productid, cp, (id, _cp) =>  _cp);
                await tx.CommitAsync();
            }
        }

        public async Task DeleteProduct(int productid)
        {
            var dictionary = await _manager.GetOrAddAsync<IReliableDictionary<int, ClothProduct>>("clothes");
            using (var tx = _manager.CreateTransaction())
            {
                bool exists = await dictionary.ContainsKeyAsync(tx, productid);
                if (exists) {
                    await dictionary.TryRemoveAsync(tx, productid);
                    await tx.CommitAsync();
                }
                
            }
        }

        public async Task<IEnumerable<ClothProduct>> Getallproducts()
        {
            List<ClothProduct> allclothes = new List<ClothProduct>();
            var dictionary = await _manager.GetOrAddAsync<IReliableDictionary<int, ClothProduct>>("clothes");
            using (var tx = _manager.CreateTransaction())
            {
                var enumerable = await dictionary.CreateEnumerableAsync(tx,EnumerationMode.Ordered);
                using (var enumerator = enumerable.GetAsyncEnumerator()) {
                    while (await enumerator.MoveNextAsync(CancellationToken.None)) {
                        KeyValuePair<int, ClothProduct> instance = enumerator.Current;
                        allclothes.Add(instance.Value);
                    }
                }
            }
            return allclothes;
        }

        public async Task<ClothProduct> GetProduct(int productid)
        {
            var dictionary = await _manager.GetOrAddAsync<IReliableDictionary<int, ClothProduct>>("clothes");
            using (var tx = _manager.CreateTransaction())
            {
                ConditionalValue<ClothProduct> cp =  await dictionary.TryGetValueAsync(tx, productid);
                return cp.Value;
            }
        }
    }
}
