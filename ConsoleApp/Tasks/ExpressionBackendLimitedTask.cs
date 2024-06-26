using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using System.Reflection;

namespace ConsoleApp.Tasks
{
    public class ExpressionBackendLimitedTask : ExpressionBackendTask
    {
        public ExpressionBackendLimitedTask(ILogger logger) : base(logger)
        {

        }

        protected override async Task<IQueryable<T>> LoadRecords<T>(params string[] fields)
        {
            var records = await base.LoadRecords<T>(fields);

            var targetProps = typeof(T).GetProperties()
                .Where(prop => fields.Contains(prop.Name));
            var constructor = typeof(T).GetConstructors()
                ?.First();

            //return records.Select(record => CreateObjectWithProperties(record, targetProps, constructor))
            //    .AsQueryable<T>();
            return records.Select(record => CreateObjectWithPropertiesExpression<T>(targetProps, constructor).Compile()(record))
                .AsQueryable<T>();
        }

        // Reflection with Expressions
        private Expression<Func<T, T>> CreateObjectWithPropertiesExpression<T>(IEnumerable<PropertyInfo> propsInfo, ConstructorInfo constructorInfo)
        {
            Type recordType = typeof(T);

            List<ParameterExpression> blockVariables = new List<ParameterExpression>();

            var ctorArgs = constructorInfo.GetParameters()
                          .Select(ctorParam =>
                          {
                              var ctorParamType = ctorParam.ParameterType;

                              var paramExp = Expression.Parameter(ctorParam.ParameterType, ctorParam.Name);
                              var defaultArgValueExp = ctorParamType.IsValueType ?
                                Expression.New(ctorParamType) :
                                Expression.Convert(Expression.Constant(null), ctorParamType) as Expression;

                              var res = Expression.Assign(
                                  paramExp,
                                  defaultArgValueExp);

                              blockVariables.Add(paramExp);

                              return res;
                          });
            var newRecord = Expression.New(constructorInfo, ctorArgs);

            var recordParamExp = Expression.Parameter(recordType, "record");
            var resultRecoredParamExp = Expression.Variable(recordType, "resRecord");

            blockVariables.Add(resultRecoredParamExp);

            var assignResultParamExp = Expression.Assign(resultRecoredParamExp, newRecord);
            var assignTargetPropsExpressions = propsInfo.Select(propInfo =>
            {
                var propAccessor = Expression.Property(resultRecoredParamExp, propInfo);
                var propValue = Expression.Property(recordParamExp, propInfo);
                
                return Expression.Assign(propAccessor, propValue);
            });

            var blockExpressions = new List<Expression>() { assignResultParamExp };
            blockExpressions.AddRange(assignTargetPropsExpressions);
            blockExpressions.Add(resultRecoredParamExp);

            var blockExp = Expression.Block(
                blockVariables,
                blockExpressions
                );

            return Expression.Lambda<Func<T, T>>(blockExp, recordParamExp);
        }

        // Reflection only approach
        private T CreateObjectWithProperties<T>(T record, IEnumerable<PropertyInfo> propsInfo, ConstructorInfo constructorInfo)
        {
            T obj = constructorInfo != null ?
                   (T)constructorInfo.Invoke(
                       constructorInfo.GetParameters()
                           .Select(ctorParam =>
                           {
                               var ctorParamType = ctorParam.GetType();
                               return ctorParamType.IsValueType ?
                                   Activator.CreateInstance(ctorParamType) :
                                   null;
                           }).ToArray()) :
                   (T)Activator.CreateInstance(typeof(T));

            foreach (var propInfo in propsInfo)
            {
                propInfo.SetValue(obj, propInfo.GetValue(record));
            }

            return obj;
        }
    }
}
