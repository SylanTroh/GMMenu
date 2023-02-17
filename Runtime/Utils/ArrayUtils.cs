
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

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
    }
}
