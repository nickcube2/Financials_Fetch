using System;
using System.Xml;

namespace FinancialBoardsFetch.Modules
{
    class Fetch
    {
        public static string Update(string requestString)
        {
            string output;
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(requestString);
                output = doc.OuterXml;
            }
            catch (Exception ex)
            {
                output = $"<Error><Message>{ex.Message}</Message></Error>";
            }

            return output;
        }
    }
}
