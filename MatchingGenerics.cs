public class MatchingGenerics
{
    public bool TwoListsMatch<T>(List<T> source, List<T> destination)
		{
			var listsMatch = true;

			if(source.Count != destination.Count) 
				return false;

			if(source.Count == destination.Count)
			{
				foreach(T item in source)
				{
					listsMatch &= ObjFoundInList(item, destination, out object objFoundInDestination);
					if(objFoundInDestination != null)
					{
						var removed = destination.Remove((T)objFoundInDestination);
					}
        }
			}

			return listsMatch;
		}

		private bool ObjFoundInList<T>(T sourceObj, List<T> destinationList, out object destinationObj)
		{
			var objFound = false;
			destinationObj = null;

            foreach (T item in destinationList)
			{
				if(TwoObjectsMatch(sourceObj, item))
				{
					destinationList.Remove(item);
					destinationObj = item;
					return true;
				}
			}

			return objFound;
		}

		public bool TwoObjectsMatch<T>(T source, T destination)
		{
			var objsMatch = true;

			var objFields = source.GetType().GetFields();

			foreach(var objField in objFields)
			{
				object sourceFieldValue = null;
				object destinationFieldValue = null;

				sourceFieldValue = objField.GetValue(source);
				destinationFieldValue = objField.GetValue(destination);

				var asd = sourceFieldValue.GetType().FullName;
                if (!sourceFieldValue.GetType().FullName.Contains("System."))
				{
					var subObjsMatch = TwoObjectsMatch(sourceFieldValue, destinationFieldValue);
					if(!subObjsMatch)
						return false;
				}
				else
				{
					if (sourceFieldValue.ToString() != destinationFieldValue.ToString())
						return false;
				}
			}

			return objsMatch;
		}
}
