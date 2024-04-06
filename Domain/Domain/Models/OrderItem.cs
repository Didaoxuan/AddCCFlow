using Domain.Core.Models;

namespace Domain.Models
{
    public class OrderItem : Entity
    {

        /// <summary>
        /// 详情名
        /// </summary>
        public string Name { get; private set; }

        public virtual Order Order { get; set; }
    }
}
