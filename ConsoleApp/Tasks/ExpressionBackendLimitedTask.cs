using Microsoft.Extensions.Logging;
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

            return records.Select(record => CreateObjectWithProperties(record, targetProps, constructor))
                .AsQueryable<T>();
        }

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
