using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BilgeShop.Data.Entities
{
    public class BaseEntity
    {
        public BaseEntity()
        {
            CreatedDate = DateTime.Now;
        }
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; } // ? -> nullable
        public bool IsDeleted { get; set; }
    }

    public abstract class BaseConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
        where TEntity : BaseEntity
    {
        // virtual tanımlıyorum ki derived class'lar ezebilsin.
        public virtual void Configure(EntityTypeBuilder<TEntity> builder)
        {
            builder.HasQueryFilter(x => x.IsDeleted == false);
            // Bu veritabanı üerinde yapılacak bütün sorgulamalarda yukarıdaki linq geçerli olacak. Böylelikle benim silinmişleri getir şeklinde bir Where kodlaması yapmama gerek kalmayacak.

            builder.Property(x => x.ModifiedDate).IsRequired(false);
            // ModifiedDate kolonuna null değer atanabilir.

        }
    }
}
