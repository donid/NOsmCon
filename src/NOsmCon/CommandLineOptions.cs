using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommandLine;
using CommandLine.Text;

namespace NOsmCon;

public enum OsmFileFormat
{
	pbf,
	xml
}

[Verb("extract", HelpText = "extract data from a file")]
public class ExtractCommandLineOptions
{
	[Option(shortName: 'c', longName: "CompleteWays")]
	public bool CompleteWays { get; }

	[Option(shortName: 'f', longName: "OutputFormat")]
	public OsmFileFormat OutputFormat { get; }


	[Value(0, MetaName = "SourceFile")]
	public string SourceFile { get; }

	[Value(1, MetaName = "OutputFile")]
	public string OutputFile { get; }

	[Value(2, MetaName = "BoundingBox", HelpText = "left,right,bottom,top")]
	public string BoundingBox { get; }

	// param-order has to match property-order: https://github.com/commandlineparser/commandline/wiki/Immutable-Options-Type
	public ExtractCommandLineOptions(bool completeWays, OsmFileFormat outputFormat, string sourceFile, string outputFile, string boundingBox)
	{
		CompleteWays = completeWays;
		SourceFile = sourceFile;
		OutputFile = outputFile;
		OutputFormat = outputFormat;
		BoundingBox = boundingBox;
	}

	[Usage(ApplicationAlias = CommandLineOptions.cAppName)]
	public static IEnumerable<Example> Examples
	{
		get
		{
			return new List<Example>()
				{
					new Example(
						"Extract data from a file that falls into a very small area",
						new ExtractCommandLineOptions(true, OsmFileFormat.xml,@"c:\source.osm.pbf", @"c:\result.xml","8.9950,52.2115,8.9963,52.2106")
						)
				};
		}
	}

}

[Verb("merge", HelpText = "merge data from files into one")]
public class MergeCommandLineOptions
{

}

public class CommandLineOptions
{
	public const string cAppName = "NOsmCon";

	//[Option('d', "dictionaryfile", Required = false, HelpText = "sometext")]
	//public string DictionaryFilePath { get; }

	//public CommandLineOptions( string dictionaryFilePath)
	//{
	//	DictionaryFilePath = dictionaryFilePath;
	//}

}
