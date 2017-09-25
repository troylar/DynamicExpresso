using System;
using System.Linq.Expressions;
using System.Reflection;

namespace DynamicExpresso.Visitors
{
	public class DisableReflectionVisitor : ExpressionVisitor
	{
		protected override Expression VisitMethodCall(MethodCallExpression node)
		{
			if (node.Object != null
#if NET_COREAPP
				&& (typeof(Type).GetTypeInfo().IsAssignableFrom(node.Object.Type)
#else
				&& (typeof(Type).IsAssignableFrom(node.Object.Type)
#endif
#if NET_COREAPP
                || typeof(MemberInfo).GetTypeInfo().IsAssignableFrom(node.Object.Type)))
#else
                || typeof(MemberInfo).IsAssignableFrom(node.Object.Type)))
#endif
            {
				throw new ReflectionNotAllowedException();
			}

			return base.VisitMethodCall(node);
		}

		protected override Expression VisitMember(MemberExpression node)
		{
#if NET_COREAPP
            if ((typeof(Type).GetTypeInfo().IsAssignableFrom(node.Member.DeclaringType)
				|| typeof(MemberInfo).GetTypeInfo().IsAssignableFrom(node.Member.DeclaringType))
#else
            if ((typeof(Type).IsAssignableFrom(node.Member.DeclaringType)
				|| typeof(MemberInfo).IsAssignableFrom(node.Member.DeclaringType))
#endif
                && node.Member.Name != "Name")
			{
				throw new ReflectionNotAllowedException();
			}

			return base.VisitMember(node);
		}
	}
}
