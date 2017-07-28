namespace SmartAudio
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Windows.Markup;

    public static class BamlHelper
    {
        private static readonly MethodInfo LoadBamlMethod = typeof(XamlReader).GetMethod("LoadBaml", BindingFlags.NonPublic | BindingFlags.Static);

        public static TRoot LoadBaml<TRoot>(Stream stream)
        {
            ParserContext context = new ParserContext();
            object[] objArray2 = new object[4];
            objArray2[0] = stream;
            objArray2[1] = context;
            objArray2[3] = false;
            object[] parameters = objArray2;
            return (TRoot) LoadBamlMethod.Invoke(null, parameters);
        }
    }
}

