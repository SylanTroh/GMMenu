
using System;
using UdonSharp;
using VRC.SDKBase;

namespace Sylan.GMMenu.Utils
{
    public class ArrayUtils : UdonSharpBehaviour
    {
        public static void Append<T>(ref T[] array,T newElement)
        {
            if (!Utilities.IsValid(array))
            {
                array = new T[] { newElement };
                return;
            }

            var newArray = new T[array.Length + 1];
            array.CopyTo(newArray, 0);
            newArray[newArray.Length - 1] = newElement;

            array = newArray;
        }
        public static T Pop<T>(ref T[] array)
        {
            // Handle an empty array
            if (array.Length == 0)
            {
                return default(T); // Returns default value for T (null for reference types, default for value types)
            }
            
            var returnValue = array[array.Length - 1];
            
            var newArray = new T[array.Length - 1];
            Array.Copy(array, 0, newArray, 0, newArray.Length);

            array = newArray;
            return returnValue;
        }
    }
}
