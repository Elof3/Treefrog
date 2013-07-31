﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace Treefrog.Utility
{
    public static class NotificationExtensions
    {
        #region Delegates

        /// <summary>
        /// A property changed handler without the property name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sender">The object that raised the event.</param>
        public delegate void PropertyChangedHandler<TSender> (TSender sender);

        #endregion

        /// <summary>
        /// Notifies listeners about a change.
        /// </summary>
        /// <param name="EventHandler">The event to raise.</param>
        /// <param name="Property">The property that changed.</param>
        public static void Notify (this PropertyChangedEventHandler EventHandler, Expression<Func<object>> Property)
        {
            // Check for null
            if (EventHandler == null)
                return;

            // Get property name
            var lambda = Property as LambdaExpression;
            MemberExpression memberExpression;
            if (lambda.Body is UnaryExpression) {
                var unaryExpression = lambda.Body as UnaryExpression;
                memberExpression = unaryExpression.Operand as MemberExpression;
            }
            else {
                memberExpression = lambda.Body as MemberExpression;
            }

            ConstantExpression constantExpression;
            if (memberExpression.Expression is UnaryExpression) {
                var unaryExpression = memberExpression.Expression as UnaryExpression;
                constantExpression = unaryExpression.Operand as ConstantExpression;
            }
            else {
                constantExpression = memberExpression.Expression as ConstantExpression;
            }

            var propertyInfo = memberExpression.Member as PropertyInfo;

            // Invoke event
            foreach (Delegate del in EventHandler.GetInvocationList()) {
                del.DynamicInvoke(new[]
                {
                    constantExpression.Value, new PropertyChangedEventArgs(propertyInfo.Name)
                });
            }
        }


        /// <summary>
        /// Subscribe to changes in an object implementing INotifiyPropertyChanged.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ObjectThatNotifies">The object you are interested in.</param>
        /// <param name="Property">The property you are interested in.</param>
        /// <param name="Handler">The delegate that will handle the event.</param>
        public static PropertyChangedEventHandler SubscribeToChange<T> (this T ObjectThatNotifies, Expression<Func<object>> Property, PropertyChangedHandler<T> Handler) 
            where T : INotifyPropertyChanged
        {
            string propertyName = GetPropertyName(Property);
            PropertyChangedEventHandler evh = (s, e) => {
                if (e.PropertyName.Equals(propertyName)) {
                    Handler(ObjectThatNotifies);
                }
            };

            ObjectThatNotifies.PropertyChanged += evh;
            return evh;
        }

        public static void UnsubscribeFromChange<T> (this T ObjectThatNotifies, PropertyChangedEventHandler handler)
            where T : INotifyPropertyChanged
        {
            ObjectThatNotifies.PropertyChanged -= handler;
        }

        public static void UnsubscribeFromChange<T> (this T ObjectThatNotifies, IEnumerable<PropertyChangedEventHandler> handlers)
            where T : INotifyPropertyChanged
        {
            foreach (var item in handlers)
                ObjectThatNotifies.PropertyChanged -= item;
        }

        private static MemberExpression GetMemberExpression (Expression<Func<object>> property)
        {
            LambdaExpression lambda = property as LambdaExpression;
            MemberExpression memberExpression;
            if (lambda.Body is UnaryExpression) {
                UnaryExpression unaryExpression = lambda.Body as UnaryExpression;
                memberExpression = unaryExpression.Operand as MemberExpression;
            }
            else {
                memberExpression = lambda.Body as MemberExpression;
            }

            return memberExpression;
        }

        private static string GetPropertyName (Expression<Func<object>> property)
        {
            PropertyInfo propertyInfo = GetMemberExpression(property).Member as PropertyInfo;
            return propertyInfo.Name;
        }
    }
}
