using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;

namespace InfoV.Fund
{
  public enum DateRange
  {
    [Description("今年")]
    ThisYear,
    [Description("近3月")]
    Near3Month,
    [Description("近6月")]
    Near6Month,
    [Description("近1年")]
    Near1Year,
    [Description("近3年")]
    Near3Year
  }
  public class Fund
  {
    public string Code { get; set; }
    
    public string Name { get; set; }
    
    public string Type { get; set; }

    public string StartRecordTimeSpan { get; set; }
    public string LastRecordTimeSpan { get; set; }
    public DateRange DateRange { get; set; }

    public IEnumerable<Point> Detail { get; set; }

    public override string ToString()
    {
      return $"{Code}   {Name}";
    }
  }

  public class DataServices
  {
    public static IEnumerable<Fund> GetFund(string findstring = null)
    {
      string resultstr = Util.HttpCommon.HttpGet(@"http://fund.eastmoney.com/js/fundcode_search.js?v=" + DateTime.Now.ToString("yyyyMMddHHmmss"));
      //Regex regex = new Regex("var r = (?<Data>\\[\\[\"(?<Code>[0-9]{6})\",\".*?\",\"(?<Name>.*?)\",\"(?<Type>.*?)\".*?\\],{0,1})*\\]");
      Regex regex = new Regex("var r = (?<Data>\\[(\\[\"(?<Code>[0-9]{6})\"\\,\".*?\"\\,\"(?<Name>.*?)\"\\,\"(?<Type>.*?)\".*?\\][\\,]{0,1})+\\])");
      Match match = regex.Match(resultstr);
      //int matchSize = match.Groups["Code"].Captures.Count;
      
      IEnumerable<Fund> s = match.Groups["Code"].Captures.Cast<Capture>().Select((v, i) => new { value = v.Value, index = i }).Join(match.Groups["Name"].Captures.Cast<Capture>().Select((v, i) => new { value = v.Value, index = i }), a => a.index, b => b.index, (a, b) => new { Code = a.value, Name = b.value, a.index }).Join(match.Groups["Type"].Captures.Cast<Capture>().Select((v, i) => new { value = v.Value, index = i }), a => a.index, b => b.index, (a, b) => new Fund { Code = a.Code, Name = a.Name, Type = b.value }).Where(w=> (findstring is null || (w.Code.Contains(findstring) || w.Name.Contains(findstring) || w.Type.Contains(findstring)))&&!w.Name.EndsWith("(后端)"));
      return s;
    }

    public static void GetFundDetail(Fund fundCode)
    {
      if (fundCode.Detail == null)
      {
        if (fundCode.Type=="货币型"|| fundCode.Type == "QDII")
        {
          return;
        }
        string resultstr = Util.HttpCommon.HttpGet($"https://fund.eastmoney.com/pingzhongdata/{fundCode.Code}.js?v={DateTime.Now.ToString("yyyyMMddHHmmss")}");

        if (resultstr.StartsWith("远程服务器返回错误"))
        {
          return;
        }

        Regex regex = new Regex("var Data_netWorthTrend = (?<Data>\\[(\\{\"x\"\\:(?<Date>[0-9]{13})\\,\"y\"\\:(?<Value>.*?)\\,.*?\\}[\\,]{0,1})+\\])");
        Match match = regex.Match(resultstr);
        fundCode.StartRecordTimeSpan = match.Groups["Date"].Captures.Cast<Capture>().First().Value;
        fundCode.LastRecordTimeSpan = match.Groups["Date"].Captures.Cast<Capture>().Last().Value;
        //fundCode.Detail = 
        fundCode.Detail = match.Groups["Date"].Captures.Cast<Capture>().Select((v,i) =>new { index =i,value = Convert.ToSingle(v.Value) / 100000 } ).Join(match.Groups["Value"].Captures.Cast<Capture>().Select((v, i) => new { index = i, value = Convert.ToDouble(v.Value) }),a=>a.index,b=>b.index,(a,b)=>new Point{ X=a.value ,Y = b.value});
      }
    }
  }
}
