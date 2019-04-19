using Microsoft.DataPrep;
using Microsoft.DataPrep.Common;
using Microsoft.Data.DataView;
using System;
using System.Collections.Generic;
using System.IO;

namespace DataPrepSample
{
    class Program
    {
        static void Main(string[] args)
        {
            //Read in csv file(s) or a .dprep package.
            string FMInfoPath = Path.Combine(Directory.GetCurrentDirectory(), "farmers-markets-info.csv");
            string FMProductsPath = Path.Combine(Directory.GetCurrentDirectory(), "farmers-markets-products.csv");
            string resultPath = Path.Combine(Directory.GetCurrentDirectory(), "result.csv");

            DataFlow dataFlowMarketInfo = Reader.AutoReadFile(FMInfoPath);
            DataFlow dataFlowProducts = Reader.AutoReadFile(FMProductsPath);
                  
            //Select useful columns.
            dataFlowMarketInfo = dataFlowMarketInfo.KeepColumns(
                columnNames: new string[] {"FMID", "MarketName", "State", "zip", "Location"});
            dataFlowProducts = dataFlowProducts.KeepColumns(
                columnNames: new string[] {"Credit", "Organic", "Flowers"});

            //Column type conversions: ToBool, ToLong, ToNumber, ToString, ToDateTime.
            dataFlowMarketInfo = dataFlowMarketInfo.ToString(columnName: "FMID");
            dataFlowProducts = dataFlowProducts.ToBool(
                columnName: "Organic", 
                trueValues: new List<string> {"Y"}, 
                falseValues: new List<string> {"N"}, 
                mismatchAs: MismatchAsOption.AsFalse);

            //Replace values to null values.
            dataFlowMarketInfo = dataFlowMarketInfo.ReplaceNa(
                columnName: "Location", 
                customNaList: "Other");
            dataFlowMarketInfo = dataFlowMarketInfo.ReplaceNa(columnName: "zip");

            //Append columns from one dataflow to the other. 
            DataFlow dataFlowCombined = dataFlowMarketInfo.AppendColumns(
                dataflows: new List<DataFlow> { dataFlowProducts });

            //Sort values in selected column.
            List<Tuple<string, bool>> sortOrder = new List<Tuple<string, bool>>
            {
                new Tuple<string, bool>("State", false)
            };
            dataFlowCombined = dataFlowCombined.Sort(sortOrder);

            //Write output to a csv file or save to .dprep package.
            DataFlow result = dataFlowCombined.WriteDelimitedFile(resultPath);

            // Get a IDataView object for further model training, such as ML.Net
            IDataView dataView = result.ToDataView();
        }
    }
}
