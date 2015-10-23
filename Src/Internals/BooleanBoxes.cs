// Copyright (c) 2015 'Abdelkarim Sellamna' (abdelkarim.se [at] gmail [dot] com)
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace WpfGhost.Internals
{
    /// <summary>
    /// A Helper class that provides pre-boxed boolean values.
    /// </summary>
    internal class BooleanBoxes
    {
        public static object True = true;
        public static object False = false;

        public static object Box(bool value)
        {
            return value ? True : False;
        }
    }
}
