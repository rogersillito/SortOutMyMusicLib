using System;
using System.Linq.Expressions;
using System.Reflection;

namespace SortOutMyMusicLib.Lib
{
    public static class PropertyExtensions
    {
        public static void SetPropertyValue<T>(this T target, Expression<Func<T, object>> memberLamda, object value)
        {
            var memberSelectorExpression = memberLamda.Body as MemberExpression;
            if (memberSelectorExpression == null) return;
            var property = memberSelectorExpression.Member as PropertyInfo;
            if (property != null)
                property.SetValue(target, value, null);
        }

        public static object GetPropertyValue<T, TResult>(this T target, Expression<Func<T, TResult>> memberLamda)
        {
            var memberSelectorExpression = memberLamda.Body as MemberExpression;
            if (memberSelectorExpression == null) return null;
            var property = memberSelectorExpression.Member as PropertyInfo;
            return property != null ? property.GetValue(target, null) : null;
        }

        public static string GetPropertyName<T, TResult>(this Expression<Func<T, TResult>> memberLamda)
        {
            var memberSelectorExpression = memberLamda.Body as MemberExpression;
            if (memberSelectorExpression == null) return null;
            var property = memberSelectorExpression.Member as PropertyInfo;
            return property != null ? property.Name : null;
        }
    }
}
