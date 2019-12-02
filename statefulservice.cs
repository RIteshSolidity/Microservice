using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Products;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;

namespace clothingproduct
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class clothingproduct : StatefulService, IClothService
    {
        private IClothingRepository _repo;
        public clothingproduct(StatefulServiceContext context)
            : base(context)
        { }

        public async Task AddClothes(ClothProduct product)
        {
            await _repo.AddProduct(product);
        }

        public async Task<IEnumerable<ClothProduct>> allClothesGet()
        {
            var item = await _repo.Getallproducts();
            return item;
        }

        public async Task DeleteCloth(int clothid)
        {
            await _repo.DeleteProduct(clothid);
        }

        public async Task<ClothProduct> GetSingleCloth(int clothid)
        {
            var item = await _repo.GetProduct(clothid);
            return item;
        }

        /// <summary>
        /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
        /// </summary>
        /// <remarks>
        /// For more information on service communication, see https://aka.ms/servicefabricservicecommunication
        /// </remarks>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return this.CreateServiceRemotingReplicaListeners();
        }


        protected override async Task RunAsync(CancellationToken cancellationToken)
        {

            _repo = new ClothingRepository(this.StateManager);
        }
    }
}
