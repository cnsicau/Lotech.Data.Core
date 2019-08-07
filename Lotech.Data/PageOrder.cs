using System;

namespace Lotech.Data
{
    /// <summary>
    ///  排序
    /// </summary>
    public class PageOrder
    {
        /// <summary>
        /// 排序列名
        /// </summary>
        public string Column { get; set; }

        /// <summary>
        /// 排序方向
        /// </summary>
        public PageOrderDirection Direction { get; set; }

        /// <summary>
        /// 从 OrderBy 解析构建，
        ///     如:  Field1 DESC, Field2
        /// </summary>
        /// <param name="orderBy"> 如:  Field1 DESC, Field2</param>
        /// <returns></returns>
        public static PageOrder[] Parse(string orderBy)
        {
            if (orderBy == null) throw new ArgumentNullException(nameof(orderBy));

            var orders = orderBy.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var pageOrders = new PageOrder[orders.Length];
            for (int i = 0; i < orders.Length; i++)
            {
                pageOrders[i] = CreateOrder(orders[i].TrimEnd());
            }
            return pageOrders;
        }

        static PageOrder CreateOrder(string order)
        {
            var index = order.LastIndexOf(' ');
            if (index == -1) return new PageOrder { Column = order.Trim() };
            return new PageOrder
            {
                Column = order.Substring(0, index).Trim(),
                Direction = order[index + 1] == 'D' || order[index + 1] == 'd' ? PageOrderDirection.DESC : PageOrderDirection.ASC
            };
        }
    }
}
