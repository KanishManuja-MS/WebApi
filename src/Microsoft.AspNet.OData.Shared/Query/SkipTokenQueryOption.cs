﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Query.Expressions;
using Microsoft.AspNet.OData.Query.Validators;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNet.OData.Query
{
    /// <summary>
    /// This defines a $skiptoken OData query option for querying.
    /// </summary>
    public class SkipTokenQueryOption
    {
        private string _value;
        private ODataQueryOptionParser _queryOptionParser;
        private IDictionary<string, object> _propertyValuePairs;
        private static readonly MethodInfo _fetchLastItemInResults = typeof(SkipTokenQueryOption).GetMethod("FetchLastItemInResults");

        /// <summary>
        /// Initialize a new instance of <see cref="SkipQueryOption"/> based on the raw $skip value and
        /// an EdmModel from <see cref="ODataQueryContext"/>.
        /// </summary>
        /// <param name="rawValue">The raw value for $skiptoken query. It can be null or empty.</param>
        /// <param name="context">The <see cref="ODataQueryContext"/> which contains the <see cref="IEdmModel"/> and some type information</param>
        /// <param name="queryOptionParser">The <see cref="ODataQueryOptionParser"/> which is used to parse the query option.</param>
        public SkipTokenQueryOption(string rawValue, ODataQueryContext context, ODataQueryOptionParser queryOptionParser)
        {
            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }

            if (String.IsNullOrEmpty(rawValue))
            {
                throw Error.ArgumentNullOrEmpty("rawValue");
            }

            if (queryOptionParser == null)
            {
                throw Error.ArgumentNull("queryOptionParser");
            }

            Context = context;
            RawValue = rawValue;
            ParseKeyValuePairs(rawValue);
            Validator = SkipTokenQueryValidator.GetSkipTokenQueryValidator(context);
            _queryOptionParser = queryOptionParser;
        }

        // This constructor is intended for unit testing only.
        internal SkipTokenQueryOption(string rawValue, ODataQueryContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }

            if (String.IsNullOrEmpty(rawValue))
            {
                throw Error.ArgumentNullOrEmpty("rawValue");
            }

            Context = context;
            RawValue = rawValue;
            Validator = SkipTokenQueryValidator.GetSkipTokenQueryValidator(context);
            _queryOptionParser = new ODataQueryOptionParser(
                context.Model,
                context.ElementType,
                context.NavigationSource,
                new Dictionary<string, string> { { "$skiptoken", rawValue } },
                context.RequestContainer);
        }

        private void ParseKeyValuePairs(string rawValue)
        {
            _propertyValuePairs = new Dictionary<string, object>();
            string[] keyValues = rawValue.Split(',');
            foreach (string keyAndValue in keyValues)
            {
                string[] pieces = keyAndValue.Split(new char[] { ':' }, 2);
                if (pieces.Length > 1 && !String.IsNullOrWhiteSpace(pieces[1]))
                {
                    IEdmTypeReference type = EdmLibHelpers.GetTypeReferenceOfProperty(Context.Model, Context.ElementClrType, pieces[0]);
                    object value = ODataUriUtils.ConvertFromUriLiteral(pieces[1], ODataVersion.V401, Context.Model, type);
                    if (!String.IsNullOrWhiteSpace(pieces[0]))
                    {
                        _propertyValuePairs.Add(pieces[0], value);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the given <see cref="ODataQueryContext"/>.
        /// </summary>
        public ODataQueryContext Context { get; private set; }

        /// <summary>
        /// Gets the raw $skiptoken value.
        /// </summary>
        public string RawValue { get; private set; }

        /// <summary>
        /// Gets the value of the $skiptoken as a parsed integer.
        /// </summary>
        public string Value
        {
            get
            {
                if (_value == null)
                {
                    string skipValue = _queryOptionParser.ParseSkipToken();
                    _value = skipValue;
                }
                return _value;
            }
        }

        /// <summary>
        /// Gets or sets the SkipToken Query Validator.
        /// </summary>
        public SkipTokenQueryValidator Validator { get; set; }

        /// <summary>
        /// Apply the $skiptoken query to the given IQueryable.
        /// </summary>
        /// <param name="query">The original <see cref="IQueryable"/>.</param>
        /// <param name="querySettings">The query settings to use while applying this query option.</param>
        /// <param name="orderBy">Information about the orderby query option.</param>
        /// <returns>The new <see cref="IQueryable"/> after the skip query has been applied to.</returns>
        public IQueryable<T> ApplyTo<T>(IQueryable<T> query, ODataQuerySettings querySettings, OrderByQueryOption orderBy)
        {
            return ApplyToCore(query, querySettings, orderBy) as IOrderedQueryable<T>;
        }

        /// <summary>
        /// Apply the $skiptoken query to the given IQueryable.
        /// </summary>
        /// <param name="query">The original <see cref="IQueryable"/>.</param>
        /// <param name="querySettings">The query settings to use while applying this query option.</param>
        /// <param name="orderBy">Information about the orderby query option.</param>
        /// <returns>The new <see cref="IQueryable"/> after the skip query has been applied to.</returns>
        public IQueryable ApplyTo(IQueryable query, ODataQuerySettings querySettings, OrderByQueryOption orderBy)
        {
            return ApplyToCore(query, querySettings, orderBy);
        }

        /// <summary>
        /// Validate the skip query based on the given <paramref name="validationSettings"/>. It throws an ODataException if validation failed.
        /// </summary>
        /// <param name="validationSettings">The <see cref="ODataValidationSettings"/> instance which contains all the validation settings.</param>
        public void Validate(ODataValidationSettings validationSettings)
        {
            if (validationSettings == null)
            {
                throw Error.ArgumentNull("validationSettings");
            }

            if (Validator != null)
            {
                Validator.Validate(this, validationSettings);
            }
        }

        private IQueryable ApplyToCore(IQueryable query, ODataQuerySettings querySettings, OrderByQueryOption orderBy)
        {
            if (Context.ElementClrType == null)
            {
                throw Error.NotSupported(SRResources.ApplyToOnUntypedQueryOption, "ApplyTo");
            }
            ExpressionBinderBase binder = new FilterBinder(Context.RequestContainer);
            IDictionary<string, OrderByDirection> directionMap = PopulateDirections(orderBy);
            bool parameterizeConstant = querySettings.EnableConstantParameterization;
            ParameterExpression param = Expression.Parameter(Context.ElementClrType);
            Expression where = null;
            int count = 0;
            /* We will create a where lambda of the following form -
             * Where (true AND Prop1>Value1)
             * OR (true AND Prop1=Value1 AND Prop2>Value2)
             * OR (true AND Prop1=Value1 AND Prop2=Value2 AND Prop3>Value3)
             * and so on...
             * Adding the first true to simplify implementation.
             */
            Expression lastEquality = Expression.Constant(true);
            foreach (KeyValuePair<string, object> item in _propertyValuePairs)
            {
                string key = item.Key;
                MemberExpression property = Expression.Property(param, key);
                object value = item.Value;

                Expression compare = null;
                ODataEnumValue enumValue = value as ODataEnumValue;
                if (enumValue != null)
                {
                    value = enumValue.Value;
                }
                Expression constant = parameterizeConstant ? LinqParameterContainer.Parameterize(value.GetType(), value) : Expression.Constant(value);
                if (directionMap.ContainsKey(key))
                {
                    compare = directionMap[key] == OrderByDirection.Descending ? binder.CreateBinaryExpression(BinaryOperatorKind.LessThan, property, constant, true) : binder.CreateBinaryExpression(BinaryOperatorKind.GreaterThan, property, constant, true);
                }
                else
                {
                    compare = binder.CreateBinaryExpression(BinaryOperatorKind.GreaterThan, property, constant, true);
                }

                Expression condition = Expression.AndAlso(lastEquality, compare);
                where = where == null ? condition : Expression.OrElse(where, condition);

                lastEquality = Expression.AndAlso(lastEquality, binder.CreateBinaryExpression(BinaryOperatorKind.Equal, property, constant, true));
                count++;
            }

            Expression whereLambda = Expression.Lambda(where, param);
            return ExpressionHelpers.Where(query, whereLambda, query.ElementType);
        }

        private static IDictionary<string, OrderByDirection> PopulateDirections(OrderByQueryOption orderBy)
        {
            IDictionary<string, OrderByDirection> directions = new Dictionary<string, OrderByDirection>();
            if (orderBy == null)
            {
                return directions;
            }

            foreach (OrderByPropertyNode node in orderBy.OrderByNodes)
            {
                if (node != null)
                {
                    directions[node.Property.Name] = node.Direction;
                }
            }
            return directions;
        }

        /// <summary>
        /// Bad Documentation
        /// </summary>
        /// <param name="result"></param>
        /// <param name="model"></param>
        /// <param name="orderByQueryOption"></param>
        /// <returns></returns>
        public static string GetSkipTokenValue(IQueryable result, IEdmModel model, OrderByQueryOption orderByQueryOption)
        {
            object lastMember = FetchLast(result);
            object value;
            IEnumerable<IEdmProperty> propertiesForSkipToken = GetPropertiesForSkipToken(lastMember, model, orderByQueryOption);

            String skipTokenvalue = String.Empty;
            if (propertiesForSkipToken == null)
            {
                return skipTokenvalue;
            }

            int count = 0;
            int lastIndex = propertiesForSkipToken.Count() - 1;
            foreach (IEdmProperty property in propertiesForSkipToken)
            {
                bool islast = count == lastIndex;
                IEdmStructuredObject obj = lastMember as IEdmStructuredObject;
                if (obj != null)
                {
                    obj.TryGetPropertyValue(property.Name, out value);
                }
                else
                {
                    value = lastMember.GetType().GetProperty(property.Name).GetValue(lastMember);
                }

                String uriLiteral = String.Empty;
                if (TypeHelper.IsEnum(value.GetType()))
                {
                    ODataEnumValue enumValue = new ODataEnumValue(value.ToString(), value.GetType().FullName);
                    uriLiteral = ODataUriUtils.ConvertToUriLiteral(enumValue, ODataVersion.V401, model);
                }
                else
                {
                    uriLiteral = ODataUriUtils.ConvertToUriLiteral(value, ODataVersion.V401, model);
                }
                skipTokenvalue += property.Name + ":" + uriLiteral + (islast ? String.Empty : ",");
                count++;
            }

            return skipTokenvalue;
        }

        private static IEnumerable<IEdmProperty> GetPropertiesForSkipToken(object lastMember, IEdmModel model, OrderByQueryOption orderByQueryOption)
        {
            IEdmType edmType = GetTypeFromObject(lastMember, model);
            IEdmEntityType entity = edmType as IEdmEntityType;
            if (entity == null)
            {
                return null;
            }

            IList<IEdmProperty> key = entity.Key().AsIList<IEdmProperty>();
            if (orderByQueryOption != null)
            {
                IList<IEdmProperty> orderByProps = orderByQueryOption.OrderByNodes.OfType<OrderByPropertyNode>().Select(p => p.Property).AsIList();
                foreach (IEdmProperty subKey in key)
                {
                    orderByProps.Add(subKey);
                }

                return orderByProps.AsEnumerable();
            }
            return key.AsEnumerable();
        }

        private static IEdmType GetTypeFromObject(object obj, IEdmModel model)
        {
            SelectExpandWrapper selectExpand = obj as SelectExpandWrapper;
            if (selectExpand != null)
            {
                return selectExpand.GetEdmType(model);
            }

            Type clrType = obj.GetType();
            return EdmLibHelpers.GetEdmType(model, clrType);
        }

        private static object FetchLast(IQueryable queryable)
        {
            MethodInfo genericMethod = _fetchLastItemInResults.MakeGenericMethod(queryable.ElementType);
            object[] args = new object[] { queryable };
            object results = genericMethod.Invoke(null, args);
            return results;
        }

        /// <summary>
        /// XML doc, make it public to avoid security issues
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="records"></param>
        /// <returns></returns>
        public static T FetchLastItemInResults<T>(IQueryable<T> records)
        {
            T[] arr = records.AsArray();
            return arr[arr.Length - 1];
        }
    }
}