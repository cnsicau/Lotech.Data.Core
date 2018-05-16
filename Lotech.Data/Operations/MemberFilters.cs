using Lotech.Data.Descriptors;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lotech.Data.Operations
{
    /// <summary>
    /// 
    /// </summary>
    static public class MemberFilters
    {
        /// <summary>
        /// 不特殊过滤
        /// </summary>
        internal static Func<MemberDescriptor, bool> None() { return _ => true; }

        /// <summary>
        /// 排除更新使用
        /// </summary>
        /// <typeparam name="TExclude"></typeparam>
        /// <returns></returns>
        internal static Func<MemberDescriptor, bool> Exclude<TExclude>() where TExclude : class
        {
            var excludeMembers = new HashSet<string>(AttributeDescriptorFactory.Create<TExclude>().Members
                                 .Select(_ => _.Member.Name));

            return _ => !excludeMembers.Contains(_.Member.Name);
        }
        /// <summary>
        /// 包含（用于仅更新）
        /// </summary>
        /// <typeparam name="TInclude"></typeparam>
        /// <returns></returns>
        internal static Func<MemberDescriptor, bool> Include<TInclude>() where TInclude : class
        {
            var excludeMembers = new HashSet<string>(AttributeDescriptorFactory.Create<TInclude>().Members
                                 .Select(_ => _.Member.Name));

            return _ => excludeMembers.Contains(_.Member.Name);
        }
    }
}
