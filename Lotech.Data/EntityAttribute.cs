using System;

namespace Lotech.Data
{
    /// <summary>
    /// 实体
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class EntityAttribute : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        EntityAttribute() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        public EntityAttribute(string name)
        {
            if(name  == null) throw new ArgumentNullException(nameof(name));
            Name  = name ;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="name"></param>
        public EntityAttribute(string schema, string name)
        {
            if(schema  == null) throw new ArgumentNullException(nameof(schema));
            Schema  = schema ;

            if(name  == null) throw new ArgumentNullException(nameof(name));
            Name  = name ;

        }

        /// <summary>
        /// 架构
        /// </summary>
        public string Schema { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
    }
}
