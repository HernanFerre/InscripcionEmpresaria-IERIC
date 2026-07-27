using System.Linq.Expressions;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Reflection;

public class LambdaExpresions
{
    private object ConvertValue(object value, Type targetType)
    {
        if (value == null) return null;

        if (targetType.IsEnum)
            return Enum.Parse(targetType, value.ToString());

        return Convert.ChangeType(value, targetType);
    }
    private Expression CreateAndEqualConditions(ParameterExpression entity, Dictionary<string, object> equalFilters)
    {
        var equalityClauses = new List<Expression>();

        foreach (var filter in equalFilters)
        {
            var propertyAccess = Expression.PropertyOrField(entity, filter.Key);
            var targetValue = ConvertValue(filter.Value, propertyAccess.Type);
            var equalsClause = Expression.Equal(propertyAccess, Expression.Constant(targetValue, propertyAccess.Type));
            equalityClauses.Add(equalsClause);
        }

        return equalityClauses.Aggregate(Expression.AndAlso);
    }

    private Expression CreateOrEqualConditions(ParameterExpression entity, Dictionary<string, object> equalFilters)
    {
        var equalityClauses = new List<Expression>();

        foreach (var filter in equalFilters)
        {
            var propertyAccess = Expression.PropertyOrField(entity, filter.Key);
            var targetValue = ConvertValue(filter.Value, propertyAccess.Type);
            equalityClauses.Add(Expression.Equal(propertyAccess, Expression.Constant(targetValue, propertyAccess.Type)));

        }

        return equalityClauses.Aggregate(Expression.OrElse);
    }

    private Expression CreateOrIdEqualConditions(ParameterExpression entity, Dictionary<string, object> equalFilters)
    {
        var equalityClauses = new List<Expression>();

        foreach (var filter in equalFilters)
        {
            var propertyAccess = Expression.PropertyOrField(entity, filter.Key);
            var id = Expression.PropertyOrField(propertyAccess, "Id");
            var targetValue = ConvertValue(filter.Value, id.Type);
            var andExp = Expression.AndAlso(Expression.NotEqual(propertyAccess, Expression.Constant(null)), Expression.Equal(id, Expression.Constant(targetValue, id.Type)));
            equalityClauses.Add(andExp);
        }

        return equalityClauses.Aggregate(Expression.OrElse);
    }

    private Expression CreateAndIdEqualConditions(ParameterExpression entity, Dictionary<string, object> equalFilters)
    {
        var equalityClauses = new List<Expression>();

        foreach (var filter in equalFilters)
        {
            var propertyAccess = Expression.PropertyOrField(entity, filter.Key);
            var id = Expression.PropertyOrField(propertyAccess, "Id");
            var targetValue = ConvertValue(filter.Value, id.Type);
            equalityClauses.Add(Expression.Equal(id, Expression.Constant(targetValue, id.Type)));
        }

        return equalityClauses.Aggregate(Expression.AndAlso);
    }

    private Expression BuildContainsClause(MemberExpression propertyAcces, object filterValue, Type type)
    {
        var constant = Expression.Constant(filterValue);
        var body = Expression.Call(typeof(Enumerable), "Contains", new Type[] { type }, constant, propertyAcces);
        return body;
    }
    private Expression CreateAndContainsConditions(ParameterExpression entity, Dictionary<string, object> containsFilters)
    {
        var containsClauses = new List<Expression>();

        foreach (var filter in containsFilters)
        {
            var propertyAccess = Expression.PropertyOrField(entity, filter.Key);
            var containsClause = BuildContainsClause(propertyAccess, filter.Value, propertyAccess.Type);
            containsClauses.Add(containsClause);
        }

        return containsClauses.Aggregate(Expression.AndAlso);
    }
    private Expression CreateOrContainsConditions(ParameterExpression entity, Dictionary<string, object> containsFilters)
    {
        var containsClauses = new List<Expression>();

        foreach (var filter in containsFilters)
        {
            var propertyAccess = Expression.PropertyOrField(entity, filter.Key);
            var containsClause = BuildContainsClause(propertyAccess, filter.Value, propertyAccess.Type);
            containsClauses.Add(containsClause);
        }

        return containsClauses.Aggregate(Expression.OrElse);
    }
    private Expression CreateAndRangeConditions(ParameterExpression entity, Dictionary<string, List<object>> rangeFilters)
    {
        var rangeClauses = new List<Expression>();

        foreach (var filter in rangeFilters)
        {
            var propertyAccess = Expression.PropertyOrField(entity, filter.Key);
            var lowerBound = ConvertValue(filter.Value[0], propertyAccess.Type);
            var upperBound = ConvertValue(filter.Value[1], propertyAccess.Type);

            var lowerBoundCheck = Expression.GreaterThanOrEqual(propertyAccess, Expression.Constant(lowerBound, propertyAccess.Type));
            var upperBoundCheck = Expression.LessThanOrEqual(propertyAccess, Expression.Constant(upperBound, propertyAccess.Type));

            rangeClauses.Add(Expression.AndAlso(lowerBoundCheck, upperBoundCheck));
        }

        return rangeClauses.Aggregate(Expression.AndAlso);
    }

    private Expression CreateAndAnyCondition(ParameterExpression entity, Dictionary<string[], object> containsFilters)
    {
        var containsClauses = new List<Expression>();

        foreach (var filter in containsFilters)
        {

            var lista = Expression.Property(entity, filter.Key[0]);
            var listaTipo = lista.Type.GenericTypeArguments.First();
            MethodInfo metodoAny = typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public).First(m => m.Name == "Any" && m.GetParameters().Count() == 2);
            var specificMethod = metodoAny.MakeGenericMethod(listaTipo);

            ParameterExpression parametro = Expression.Parameter(listaTipo, "e");

            Expression propiedad = Expression.Property(parametro, filter.Key[1]);
            Expression valor = Expression.Constant(ConvertValue(filter.Value, propiedad.Type));
            //Expression igualdad = Expression.Equal(propiedad, valor);
            Expression condicion = Expression.Call(propiedad, "Equals", null, valor);

            var delegateType = Expression.GetFuncType(listaTipo, typeof(bool));

            var expr = Expression.Lambda(delegateType, condicion, parametro);

            var any = Expression.Call(specificMethod, lista, expr);

            containsClauses.Add(any);
        }


        return containsClauses.Aggregate(Expression.AndAlso);
    }

    public Expression CreateAndParentesis<T>(ParameterExpression entity, List<FilterCriteria> expresiones)
    {
        var containsClauses = new List<Expression>();

        foreach (var expre in expresiones)
        {
            containsClauses.Add(BuildFilterExpression<T>(expre, entity));
        }

        return containsClauses.Aggregate(Expression.AndAssign);
    }



    private List<Expression> GetFilterClauses<T>(FilterCriteria criteria, ParameterExpression parameter)
    {

        var filterClauses = new List<Expression>();

        if (criteria.EqualFiltersAnd.Any())
        {
            filterClauses.Add(CreateAndEqualConditions(parameter, criteria.EqualFiltersAnd));
        }

        if (criteria.EqualFiltersOr.Any())
        {
            filterClauses.Add(CreateOrEqualConditions(parameter, criteria.EqualFiltersOr));
        }

        if (criteria.ContainsFiltersAnd.Any())
        {
            filterClauses.Add(CreateAndContainsConditions(parameter, criteria.ContainsFiltersAnd));
        }

        if (criteria.ContainsFiltersOr.Any())
        {
            filterClauses.Add(CreateOrContainsConditions(parameter, criteria.ContainsFiltersOr));
        }

        if (criteria.RangeFiltersAnd.Any())
        {
            filterClauses.Add(CreateAndRangeConditions(parameter, criteria.RangeFiltersAnd));
        }

        if (criteria.CreateAndAnyCondition.Any())
        {
            filterClauses.Add(CreateAndAnyCondition(parameter, criteria.CreateAndAnyCondition));
        }

        if (criteria.CreateAndParentesis.Any())
        {
            filterClauses.Add(CreateAndParentesis<T>(parameter, criteria.CreateAndParentesis));
        }
        if (criteria.EqualIdFiltersOr.Any())
        {
            filterClauses.Add(CreateOrIdEqualConditions(parameter, criteria.EqualIdFiltersOr));
        }

        if (criteria.EqualIdFiltersAnd.Any())
        {
            filterClauses.Add(CreateAndIdEqualConditions(parameter, criteria.EqualIdFiltersAnd));
        }

        return filterClauses;

    }
    public Expression<Func<T, bool>> BuildFilterExpression<T>(FilterCriteria criteria)
    {
        var parameter = Expression.Parameter(typeof(T), "entity");
        var filterClauses = GetFilterClauses<T>(criteria, parameter);
        var finalClause = filterClauses.Aggregate(Expression.AndAlso);
        return Expression.Lambda<Func<T, bool>>(finalClause, parameter);
    }

    public Expression BuildFilterExpression<T>(FilterCriteria criteria, ParameterExpression parameter)
    {
        var filterClauses = GetFilterClauses<T>(criteria, parameter);
        var finalClause = filterClauses.Aggregate(Expression.AndAlso);
        return finalClause;
    }

    public class FilterCriteria
    {
        public Dictionary<string, object> EqualFiltersAnd = new Dictionary<string, object>();
        public Dictionary<string, object> EqualFiltersOr = new Dictionary<string, object>();
        public Dictionary<string, object> EqualIdFiltersOr = new Dictionary<string, object>();
        public Dictionary<string, object> EqualIdFiltersAnd = new Dictionary<string, object>();
        public Dictionary<string, object> ContainsFiltersAnd = new Dictionary<string, object>();
        public Dictionary<string, object> ContainsFiltersOr = new Dictionary<string, object>();
        public Dictionary<string, List<object>> RangeFiltersAnd = new Dictionary<string, List<object>>();
        public Dictionary<string[], object> CreateAndAnyCondition = new Dictionary<string[], object>();
        public List<FilterCriteria> CreateAndParentesis = new List<FilterCriteria>();



    }

}

