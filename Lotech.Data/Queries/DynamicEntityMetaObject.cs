using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;

namespace Lotech.Data.Queries
{
    class DynamicEntityMetaObject : DynamicMetaObject
    {
        public DynamicEntityMetaObject(Expression expression, DynamicEntity value)
            : base(expression, BindingRestrictions.Empty, value) { }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return ((DynamicEntity)Value).Fields;
        }

        public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
        {
            var index = ((DynamicEntity)Value).GetOrdinal(binder.Name);
            Expression value;
            if (index == -1)
            {
                value = Expression.Constant(null, binder.ReturnType);
            }
            else
            {
                value = Expression.Call(
                                Expression.Convert(Expression, LimitType),
                                LimitType.GetMethod(nameof(DynamicEntity.GetValue), new Type[] { typeof(int) }),
                                Expression.Constant(index)
                        );
            }
            return new DynamicMetaObject(value, BindingRestrictions.GetTypeRestriction(Expression, LimitType));
        }

        public override DynamicMetaObject BindConvert(ConvertBinder binder)
        {
            if (binder.ReturnType == typeof(IDictionary)
                || typeof(IEnumerable<KeyValuePair<string, object>>).IsAssignableFrom(binder.ReturnType))
            {
                var pairCtor = typeof(KeyValuePair<string, object>).GetConstructor(new[] { typeof(string), typeof(object) });
                var pairs = new List<Expression>();
                var fields = ((DynamicEntity)Value).Fields;

                for (int i = 0; i < fields.Length; i++)
                {
                    var name = fields[i];
                    var index = ((DynamicEntity)Value).GetOrdinal(fields[i]);
                    var value = Expression.Call(
                            Expression.Convert(Expression, LimitType),
                            LimitType.GetMethod(nameof(DynamicEntity.GetValue), new Type[] { typeof(int) }),
                            Expression.Constant(index)
                    );
                    pairs.Add(Expression.New(pairCtor, Expression.Constant(name), value));
                }

                var ctor = typeof(Dictionary<string, object>).GetConstructor(new[] { typeof(IEnumerable<KeyValuePair<string, object>>) });

                var expression = Expression.New(ctor, Expression.NewArrayInit(
                        typeof(KeyValuePair<string, object>),
                        pairs.ToArray()));

                return new DynamicMetaObject(expression, BindingRestrictions.GetTypeRestriction(Expression, LimitType));
            }
            else if (binder.ReturnType.IsClass && !binder.ReturnType.IsAbstract && !binder.ReturnType.IsArray && !binder.ReturnType.IsByRef)
            {
                var fields = ((DynamicEntity)Value).Fields;
                var model = Expression.Variable(binder.ReturnType);
                var fieldValue = Expression.Variable(typeof(object));

                var expressions = new List<Expression>();
                expressions.Add(Expression.Assign(model, Expression.New(binder.ReturnType)));

                var members = binder.ReturnType.GetMembers();

                for (int i = 0; i < fields.Length; i++)
                {
                    MemberInfo fieldMember = null;
                    foreach (var member in members)
                    {
                        if (!string.Equals(member.Name, fields[i], StringComparison.InvariantCultureIgnoreCase)) continue;
                        if (member.MemberType == MemberTypes.Property && ((PropertyInfo)member).CanWrite
                            || member.MemberType == MemberTypes.Field)
                        {
                            fieldMember = member;
                            break;
                        }
                    }
                    if (fieldMember == null) continue;

                    expressions.Add(Expression.Assign(fieldValue,
                        Expression.Call(
                                Expression.Convert(Expression, LimitType),
                                LimitType.GetMethod(nameof(DynamicEntity.GetValue), new Type[] { typeof(int) }),
                                Expression.Constant(i)
                        )));

                    var memberValueType = fieldMember.MemberType == MemberTypes.Field
                                ? ((FieldInfo)fieldMember).FieldType
                                : ((PropertyInfo)fieldMember).PropertyType;

                    expressions.Add(Expression.Assign(
                            Expression.MakeMemberAccess(model, fieldMember),
                                Expression.Convert(
                                        Expression.Call(
                                            typeof(Convert).GetMethod(nameof(Convert.ChangeType), new Type[] { typeof(object), typeof(Type) })
                                            , new Expression[] { fieldValue, Expression.Constant(memberValueType) })
                                    , memberValueType)
                            ));
                }

                expressions.Add(model);
                return new DynamicMetaObject(Expression.Block(binder.ReturnType,
                        new ParameterExpression[] { model, fieldValue },
                        expressions
                    ), BindingRestrictions.GetTypeRestriction(Expression, LimitType));
            }
            return base.BindConvert(binder);
        }
    }
}
