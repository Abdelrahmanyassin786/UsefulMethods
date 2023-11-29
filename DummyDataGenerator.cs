public static class DummyDataGenerator
{
    private const string LOREM_IPSUM = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua";

    /// <inheritdoc cref="PopulateWithDummyData{T}(T, string[])"/>
    public static T PopulateWithDummyData<T>() where T : class, new()
    {
        T result = new T();
        return PopulateWithDummyData<T>(result);
    }

    /// <inheritdoc cref="PopulateWithDummyData{T}(T, string[])"/>
    public static T PopulateWithDummyData<T>(params string[] fieldsToExclude) where T : class, new()
    {
        T result = new T();
        return PopulateWithDummyData<T>(result, fieldsToExclude);
    }

    /// <summary>
    /// Returns an instance<![CDATA[<]]><typeparamref name="T"/>> populated with dummy data.
    /// </summary>
    /// <typeparam name="T">The type to populate with dummy data</typeparam>
    /// <param name="obj">The object to populate</param>
    /// <param name="fieldsToExclude">Fields in <paramref name="obj"/> to exclude from populating with dummy data.</param>
    /// <returns></returns>
    public static T PopulateWithDummyData<T>(T obj, params string[] fieldsToExclude) where T : class
    {
        // Get fields of obj
        var objFields = obj.GetType().GetFields();
        if(fieldsToExclude.Length > 0)
        {
            var filteredObjFields = objFields.ToList();
            foreach( var fieldToExclude in fieldsToExclude)
            {
                var fieldExist = objFields.ToList().Where(w => w.Name == fieldToExclude).FirstOrDefault();
                if (fieldExist != null)
                {
                    var success = filteredObjFields.Remove(fieldExist);
                }
            }
            objFields = filteredObjFields.ToArray();
        }
        
        foreach (var fieldInfo in objFields)
        {
            var fieldValue = fieldInfo.GetValue(obj);

            // Check if this is a list/array
            if (fieldInfo.FieldType.Namespace.Equals("System.Collections.Generic"))
            //if (typeof(IList).IsAssignableFrom(fieldValue.GetType()))
            {
                // By now, we know that this is assignable from IList.
                var listUnderlyingTypeFields = fieldValue.GetType().GetTypeInfo().DeclaredFields.First().ReflectedType.GenericTypeArguments.First().GetTypeInfo().DeclaredFields.ToArray();
                var listUnderlyingType = fieldValue.GetType().GetTypeInfo().DeclaredFields.First().ReflectedType.GenericTypeArguments.First();

                var rnd = new Random((int)DateTime.Now.Ticks).Next(1, 10);
                for (int i = 0; i < rnd; i++)
                {
                    object listObj = Activator.CreateInstance(listUnderlyingType);
                    ((IList)fieldValue).Add(PopulateWithDummyData<object>(listObj));
                }
            }
            // Check if the field is a system type, i.e: string, int, bool, ...
            else if (fieldValue.GetType().FullName.StartsWith("System."))
            {
                dynamic _fieldValue = CreateRandomValueBasedOnType(fieldInfo.FieldType.FullName);
                fieldInfo.SetValue(obj, _fieldValue);
            }
            // If reaches here then field is another class
            else
            {
                PopulateWithDummyData<object>(fieldValue);
            }
        }

        return obj;
    }

    /// <summary>
    /// returns a random value for <paramref name="value"/> based on the type passed in <paramref name="fieldType"/>
    /// </summary>
    private static dynamic CreateRandomValueBasedOnType(string fieldType)
    {
        var rnd = new Random((int)DateTime.Now.Ticks).Next();
        dynamic result = null;

        switch (fieldType)
        {
            case "System.String":
            case "System.string":
                {
                    result = LoremIpsumGenerator();
                    break;
                }
            case "System.Boolean":
            case "System.bool":
                {
                    result = rnd % 2 == 0;
                    break;
                }
            case "System.Byte":
                {
                    result = new byte();
                    break;
                }
            case "System.Char":
                {
                    result = string.Empty[0];
                    break;
                }
            case "System.Int16":
            case "System.Int32":
            case "System.Int64":
                {
                    result = rnd;
                    break;
                }
            case "System.Double":
                {
                    result = (double)rnd;
                    break;
                }
            case "System.Decimal":
                {
                    result = (decimal)rnd;
                    break;
                }
            case "System.Single":
            case "System.float":
                {
                    result = (float)rnd;
                    break;
                }
            case "System.DateTime":
                {
                    result = new DateTime();
                    break;
                }
            default:
                {
                    return null;
                }
        }
        return result;
    }

    /// <summary>
    /// Returns a random number of random words. (Lorem Ipsum)
    /// </summary>
    /// <returns></returns>
    private static string LoremIpsumGenerator()
    {
        var result = new StringBuilder();
        var loremIpsums = LOREM_IPSUM.Split(' ');
        var rng = new Random((int)DateTime.Now.Ticks);
        var numOfWords = rng.Next(0, loremIpsums.Length);
        for (int i = 0; i < numOfWords; i++)
        {
            var wordIndex = rng.Next(0, loremIpsums.Length);
            var wordToAdd = loremIpsums[wordIndex];
            result.Append(wordToAdd).Append(' ');
        }
        return result.ToString();
    }
}
