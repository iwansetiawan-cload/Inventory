using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_OneWeb.DataAccess.Repository.IRepository
{
    public interface IUnitOfWork : IDisposable
    {
        IArticleRepository Article { get; }
        IApplicationUserRepository ApplicationUser { get; }
        ICategoryRepository Category { get; }
        IBrandRepository Brand { get; }
        IRoomRepository Room { get; }
        IItemsRepository Items { get; }
        ISupplierRepository Supplier { get; }
        IPurchaseOrderHeaderRepository PurchaseOrderHeader { get; }
        IItemTransferRepository ItemTransfer { get; }
        IGenmasterRepository Genmaster { get; }
        ILocationRepository Location { get; }
        IItemServiceRepositoryAsync ItemService { get; }
        void Save();
    }
}
