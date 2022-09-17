
using System.Security;
using System.Xml;
using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;

namespace FinancialBoardsFetch.Modules
{
    internal enum ResourceType
    {
        Commodity,
        Currency,
        Index,
        Share
    }
}

namespace FinancialBoardsFetch.Modules
{
    internal class Resource
    {
        internal static ResourceType MyResourceType;
        internal readonly string Code = "...", Sname = "...", Perc = "...", Trade = "...", Points = "...";
        internal readonly TimeSpan Time = TimeSpan.MinValue;

        public Resource(ResourceType resourceType, string code, string sname, string trade, string time, string perc, string points)
        {
            MyResourceType = resourceType;
            Code = code;
            Sname = sname;
            Trade = trade;
            TimeSpan tsTime = TimeSpan.TryParse(time, out tsTime) ? tsTime : TimeSpan.MinValue;
            Time = tsTime;
            Perc = resourceType == ResourceType.Currency? (-1 * float.Parse(perc, CultureInfo.InvariantCulture.NumberFormat)).ToString() : perc;
            Points = points;
        }

        internal static Resource FromNewFeed(ResourceType resourceType, XmlElement record)
        {
            string time = record["time"].InnerText[..2] == "24" ? "00:00:00" : record["time"].InnerText;
            if (resourceType == ResourceType.Share)
            {
                return new Resource(resourceType, record["code"].InnerText, Refactor(record["code"].InnerText, record["sname"].InnerText), record["trade"].InnerText, time, record["perc"].InnerText, record["movement"].InnerText);
            }

            return new Resource(resourceType, record["code"].InnerText, record["sname"].InnerText, record["trade"].InnerText, time, record["perc"].InnerText, record["movement"].InnerText);
        }

        public static Resource FromOldFeed(ResourceType resourceType, XmlElement quote)
        {
            XmlElement intutl = quote["intutl"];

            string time = intutl["time"].InnerText == "24:00:00" ? "00:00:00" : intutl["time"].InnerText;

            return new Resource(resourceType, intutl["code"].InnerText, intutl["sname"].InnerText, intutl["trade"].InnerText, time, intutl["perc"].InnerText, intutl["points"].InnerText); ;
        }

        private static string Refactor(string code, string deflt)
        {
            string resource_data = Properties.Resources.All_Codes;
            List<string> companies = resource_data.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList();

            //if (companies.All(c => c.Split(',')[0] != code))
            if (companies.Any(c => c.Split(',')[0] == code))
                return companies.Where(c => c.Split(',')[0] == code).First().Split(',')[1];
            else
                return deflt;
        }

        public string ToXml()
        {
            string maestroString = $"<{MyResourceType}>";
            maestroString += $"<Code>{SecurityElement.Escape(Code)}</Code>";
            maestroString += $"<ShortName>{SecurityElement.Escape(Sname)}</ShortName>";
            maestroString += $"<Time>{SecurityElement.Escape(Time.ToString(@"dd\.hh\:mm\:ss"))}</Time>";
            maestroString += $"<Trade>{SecurityElement.Escape(Trade)}</Trade>";
            maestroString += $"<Points>{SecurityElement.Escape(Points)}</Points>";
            maestroString += $"<Perc>{SecurityElement.Escape(Perc)}</Perc>";
            maestroString += $"</{MyResourceType}>";
            return maestroString;
        }
    }
}

