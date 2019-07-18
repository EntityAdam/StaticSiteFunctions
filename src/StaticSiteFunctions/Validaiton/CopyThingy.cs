using Microsoft.AspNetCore.Http;

namespace StaticSiteFunctions
{
    public static class CopyThingy
    {
        public static T Populate<T>(IFormCollection formCollection)
        {
            var x = System.Activator.CreateInstance<T>();
            var props = x.GetType().GetProperties();
            foreach (var prop in props)
            {
                var test = formCollection[prop.Name];
                try
                {
                    if (prop.PropertyType == typeof(string))
                    {
                        prop.SetValue(x, formCollection[prop.Name].ToString());
                    }
                    else
                    {
                        continue;
                        //throw new ArgumentException(nameof(CopyThingy), "CopyThingy can only use POCOs with public strings");
                    }
                }
                catch (System.Exception)
                {
                    throw;
                }
            }
            return x;
        }
    }
}